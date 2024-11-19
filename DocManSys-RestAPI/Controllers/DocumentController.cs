using Microsoft.AspNetCore.Mvc;
using DocManSys_RestAPI.Models;
using AutoMapper;
using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Services;

namespace DocManSys_RestAPI.Controllers {
    /// <summary>
    /// Document Controller with GET, PUT, POST, DELETE methods
    /// </summary>
    [Area("RestAPI")]
    [ApiController]
    [Route("api/[area]/[controller]")]
    public class DocumentController : ControllerBase {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<DocumentController> _logger;
        private readonly IMapper _mapper;
        private readonly IMessageQueueService _messageQueueService;

        public DocumentController(IHttpClientFactory clientFactory, ILogger<DocumentController> logger, IMapper mapper,
            IMessageQueueService messageQueueService) {
            _logger = logger;
            _clientFactory = clientFactory;
            _mapper = mapper;
            _messageQueueService = messageQueueService;
        }

        // GET: api/Document
        /// <summary>
        /// Get all documents
        /// </summary>
        /// <returns>IEnumerable<Document></Document>></returns>
        [HttpGet]
        public async Task<IActionResult> GetDocuments([FromQuery] string searchTerm = "") {
            _logger.LogInformation("TEST");

            var client = _clientFactory.CreateClient("DocManSys-DAL");
            string requestUri = "api/DAL/document";
            if (!string.IsNullOrEmpty(searchTerm)) {
                requestUri += $"?searchTerm={Uri.EscapeDataString(searchTerm)}"; // URL encode the search term
            }

            var response = await client.GetAsync(requestUri);
            if (response.IsSuccessStatusCode) {
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<DocumentEntity>>();
                var documents = _mapper.Map<IEnumerable<Document>>(items);
                return Ok(documents);
            }

            _logger.LogError("Failed to retrieve Documents from Database!");
            return StatusCode((int)response.StatusCode, "Error retrieving Documents from DAL");
        }

        // GET: api/Document/5
        /// <summary>
        /// Get a document by id
        /// </summary>
        /// <param name="id">Id of the document that should be searched for</param>
        /// <returns><Document></Document>></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id) {
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var response = await client.GetAsync($"api/DAL/document/{id}");
            if (response.IsSuccessStatusCode) {
                var item = await response.Content.ReadFromJsonAsync<DocumentEntity>();
                var document = _mapper.Map<Document>(item);
                if (item != null) {
                    _logger.LogInformation($"Retrieved Document with ID: {id}!");
                    return Ok(document);
                }

                _logger.LogInformation($"Document with ID: {id} not found");
                return NotFound();
            }

            _logger.LogError($"Failed to retrieve Document from Database with the ID {id}");
            return StatusCode((int)response.StatusCode, "Error retrieving Document from DAL");
        }

        // PUT: api/Document/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Update Document entry
        /// </summary>
        /// <param name="id">Id of the document that should be updated</param>
        /// <param name="document">the data that should be in the updated document</param>
        /// <returns>Returns bad Status Codes if something went wrong</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, Document document) {
            if (id != document.Id) return BadRequest();
            Console.WriteLine($@"[PUT] Incomming OcrText: {document.OcrText}");
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var item = _mapper.Map<DocumentEntity>(document);
            Console.WriteLine($@"[PUT] mapped OcrText: {item.OcrText}");
            var response = await client.PutAsJsonAsync($"api/DAL/document/{id}", item);
            if (response.IsSuccessStatusCode) {
                return NoContent();
            }

            _logger.LogError($"Failed to update Document from Database with the ID {id}");
            return StatusCode((int)response.StatusCode, "Error updating Document in DAL");
        }

        // PUT: api/Document/5/upload
        /// <summary>
        /// Upload a Document File
        /// </summary>
        /// <param name="id">id of the document that should be updated</param>
        /// <param name="documentFile">File of the document that should be added</param>
        /// <returns>Returns bad Status Codes if something went wrong</returns>
        [HttpPut("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile? documentFile) {
            if (documentFile == null || documentFile.Length == 0) {
                ModelState.AddModelError("documentFile", "No File Uploaded.");
                return BadRequest(ModelState);
            }

            // Hole den Task vom DAL
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var response = await client.GetAsync($"api/DAL/document/{id}");
            if (!response.IsSuccessStatusCode) {
                _logger.LogError($"Error at retrieving document with ID: {id}");
                return NotFound($"Error at retrieving document with ID: {id}");
            }

            // Mappe das empfangene TodoItem auf ein TodoItemDto
            var documentItem = await response.Content.ReadFromJsonAsync<DocumentEntity>();
            if (documentItem == null) {
                _logger.LogError($"Document with ID {id} not found.");
                return NotFound($"Document with ID {id} not found.");
            }

            var document = _mapper.Map<Document>(documentItem);
            
            _logger.LogInformation($"Title changed to {documentFile.FileName}");
            document.Title = documentFile.FileName;

            var validator = new DocumentValidator();
            var validationResult = await validator.ValidateAsync(document); // Validiere das DTO
            if (!validationResult.IsValid) {
                return BadRequest(validationResult.Errors);
            }

            // Mappe wieder zurück zu TodoItem, um es im DAL zu aktualisieren
            var updatedDocument = _mapper.Map<DocumentEntity>(document);

            var updateResponse = await client.PutAsJsonAsync($"api/DAL/document/{id}", updatedDocument);
            if (!updateResponse.IsSuccessStatusCode) {
                _logger.LogError($"Error at saving the filename for document with ID {id}");
                return StatusCode((int)updateResponse.StatusCode,
                    $"Error at saving the filename for document with ID {id}");
            }

            var filePath = Path.Combine("/app/uploads", documentFile.FileName);
            Directory.CreateDirectory(
                Path.GetDirectoryName(filePath)!); // Erstelle das Verzeichnis, falls es nicht existiert
            await using (var stream = new FileStream(filePath, FileMode.Create)) {
                await documentFile.CopyToAsync(stream);
            }

            // Nachricht an RabbitMQ
            try {
                _messageQueueService.SendToQueue($"{id}|{filePath}");
                _logger.LogInformation("Successfully send message to RabbitMQ");
            }
            catch (Exception ex) {
                _logger.LogError($"Error at sending the message to RabbitMQ: {ex.Message}");
                return StatusCode(500, $"Error at sending the message to RabbitMQ: {ex.Message}");
            }

            return Ok(new { message = $"Filename {documentFile.FileName} for document {id} successfully saved." });
        }

        // POST: api/Document
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Create a new Document entry
        /// </summary>
        /// <param name="document">new document that should be added in the database</param>
        /// <returns><CreatedAtAction></CreatedAtAction>></returns>
        [HttpPost]
        public async Task<IActionResult> PostDocument(Document document) {
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var item = _mapper.Map<DocumentEntity>(document);
            var response = await client.PostAsJsonAsync("api/DAL/document", item);
            if (response.IsSuccessStatusCode) {
                //var item = await response.Content.ReadFromJsonAsync<Document>();
                //if (item != null)
                try {
                    _logger.LogInformation($"Added Document with ID: {document.Id}");
                    _messageQueueService.SendToQueue($"{document.Id}|{document.Title}");
                }
                catch (Exception e) {
                    _logger.LogError($"Error while sending message to RabbitMQ: {e.Message}");
                    return StatusCode(500, $"Error while sending message to RabbitMQ: {e.Message}");
                }

                return CreatedAtAction(nameof(GetDocument), new { id = item.Id }, item);
            }

            _logger.LogError(
                $"Failed to write new Document into Database with the Document Title: {document.Title} and Author: {document.Author} ");
            return StatusCode((int)response.StatusCode, "Error creating Document in DAL");
        }

        // DELETE: api/Document/5
        /// <summary>
        /// Delete a Document entry
        /// </summary>
        /// <param name="id">id of the document that should be deleted</param>
        /// <returns>Returns bad Status Codes if something went wrong</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id) {
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var response = await client.DeleteAsync($"api/DAL/document/{id}");
            if (response.IsSuccessStatusCode) {
                return NoContent();
            }

            _logger.LogError($"Failed to delete Document from Database with the ID {id}");
            return StatusCode((int)response.StatusCode, "Error deleting Document in DAL");
        }
    }
}