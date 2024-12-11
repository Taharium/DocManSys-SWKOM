using Microsoft.AspNetCore.Mvc;
using DocManSys_RestAPI.Models;
using AutoMapper;
using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Services;
using RabbitMQ.Client.Events;
using System.Text;
using Elastic.Clients.Elasticsearch;
using Minio;

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
        private readonly IMinioClientService _minioclientservice;
        private readonly ElasticsearchService _elasticsearchService;

        public DocumentController(IHttpClientFactory clientFactory, ILogger<DocumentController> logger, IMapper mapper,
            IMessageQueueService messageQueueService, IMinioClientService minioClientService,
            ElasticsearchService elasticsearchService) {
            _logger = logger;
            _clientFactory = clientFactory;
            _mapper = mapper;
            _messageQueueService = messageQueueService;
            _minioclientservice = minioClientService;
            _elasticsearchService = elasticsearchService;
        }

        /// <summary>
        /// Retrieves all documents.
        /// </summary>
        /// <remarks>
        /// This endpoint fetches all documents from the underlying data source.
        /// </remarks>
        /// <returns>A list of all documents.</returns>
        /// <response code="200">Returns the list of documents.</response>
        /// <response code="500">If there is an error fetching documents from the DAL.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Document>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDocuments() {
            _logger.LogInformation("TEST");

            var client = _clientFactory.CreateClient("DocManSys-DAL");

            var response = await client.GetAsync("api/DAL/document");
            if (response.IsSuccessStatusCode) {
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<DocumentEntity>>();
                var documents = _mapper.Map<IEnumerable<Document>>(items);
                _logger.LogInformation($"All Documents fetched!");
                return Ok(documents);
            }

            _logger.LogError("Failed to retrieve Documents from Database!");
            return StatusCode((int)response.StatusCode, "Error retrieving Documents from DAL");
        }


        /// <summary>
        /// Retrieves a specific document by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the document.</param>
        /// <returns>The document corresponding to the specified ID.</returns>
        /// <response code="200">Returns the requested document.</response>
        /// <response code="404">If the document is not found.</response>
        /// <response code="500">If there's an error retrieving the document from the data source.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Document), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
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

        /// <summary>
        /// Updates an existing document by ID.
        /// </summary>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="document">The updated document object.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">If the document was successfully updated.</response>
        /// <response code="400">If the provided ID does not match the document ID.</response>
        /// <response code="500">If there's an error updating the document in the data source.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PutDocument(int id, Document document) {
            if (id != document.Id) return BadRequest();
            _logger.LogInformation($@"[PUT] Incomming OcrText: {document.OcrText}");
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var item = _mapper.Map<DocumentEntity>(document);
            _logger.LogInformation($@"[PUT] mapped OcrText: {item.OcrText}");
            var response = await client.PutAsJsonAsync($"api/DAL/document/{id}", item);
            if (response.IsSuccessStatusCode) {
                _logger.LogInformation($"Updated Document with ID: {id}");
                return NoContent();
            }

            _logger.LogError($"Failed to update Document from Database with the ID {id}");
            return StatusCode((int)response.StatusCode, "Error updating Document in DAL");
        }


        /// <summary>
        /// Uploads a file for the specified document and updates the document metadata.
        /// </summary>
        /// <param name="id">The ID of the document to which the file belongs.</param>
        /// <param name="documentFile">The file to be uploaded (must be a PDF).</param>
        /// <returns>A message indicating the result of the operation.</returns>
        /// <response code="200">If the file was successfully uploaded and metadata updated.</response>
        /// <response code="400">If the input is invalid (e.g., missing or invalid file type).</response>
        /// <response code="404">If the document with the specified ID is not found.</response>
        /// <response code="500">If an internal server error occurs during the operation.</response>
        [HttpPut("{id}/upload")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UploadFile(int id, IFormFile? documentFile) {
            if (documentFile == null || documentFile.Length == 0) {
                ModelState.AddModelError("documentFile", "No File Uploaded.");
                return BadRequest(ModelState);
            }

            if (!documentFile.FileName.EndsWith(".pdf")) {
                ModelState.AddModelError("documentFile", "Only PDF-Files are allowed.");
                return BadRequest(ModelState);
            }

            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var response = await client.GetAsync($"api/DAL/document/{id}");
            if (!response.IsSuccessStatusCode) {
                _logger.LogError($"Error at retrieving document with ID: {id}");
                return NotFound(new { message = $"Error at retrieving document with ID: {id}"} );
            }

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

            var updatedDocument = _mapper.Map<DocumentEntity>(document);

            var updateResponse = await client.PutAsJsonAsync($"api/DAL/document/{id}", updatedDocument);

            if (!updateResponse.IsSuccessStatusCode) {
                _logger.LogError($"Error at saving the filename for document with ID {id}");
                return StatusCode((int)updateResponse.StatusCode,
                    $"Error at saving the filename for document with ID {id}");
            }

            var filePath = Path.Combine("/app/uploads", documentFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await using (var stream = new FileStream(filePath, FileMode.Create)) {
                await documentFile.CopyToAsync(stream);
            }

            try {
                _messageQueueService.SendToQueue($"{id}|{filePath}");
                _logger.LogInformation("Successfully send message to RabbitMQ");
            }
            catch (Exception ex) {
                _logger.LogError($"Error at sending the message to RabbitMQ: {ex.Message}");
                return StatusCode(500, $"Error at sending the message to RabbitMQ: {ex.Message}");
            }

            try {
                await _minioclientservice.UploadFile(documentFile);
                _logger.LogInformation("Successfully uploaded the file to Minio");
            }
            catch (Exception ex) {
                _logger.LogError($"Error at uploading the file to Minio: {ex.Message}");
                return StatusCode(500, $"Error at uploading the file to Minio: {ex.Message}");
            }

            return Ok(new { message = $"Filename {documentFile.FileName} for document {id} successfully saved." });
        }

        /// <summary>
        /// Creates a new document and indexes it in Elasticsearch.
        /// </summary>
        /// <param name="document">The document to be created.</param>
        /// <returns>Returns the created document with its generated ID.</returns>
        /// <response code="201">Document successfully created and indexed.</response>
        /// <response code="400">Invalid document data.</response>
        /// <response code="500">Internal server error while saving or indexing the document.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Document), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost]
        public async Task<IActionResult> PostDocument(Document document) {
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var item = _mapper.Map<DocumentEntity>(document);

            var response = await client.PostAsJsonAsync("api/DAL/document", item);
            if (!response.IsSuccessStatusCode) {
                return StatusCode((int)response.StatusCode,
                    new { message = "Failed to save document to the database" });
            }

            // Get the updated item (with its database ID populated)
            var savedItem = await response.Content.ReadFromJsonAsync<DocumentEntity>();
            if (savedItem == null || savedItem.Id == 0) {
                return StatusCode(500, new { message = "Failed to retrieve saved document with generated ID" });
            }

            try {
                _logger.LogInformation($"Added Document with ID: {savedItem.Id}");

                var indexResponse = await _elasticsearchService.IndexDocumentAsync(savedItem);
                if (!indexResponse.IsValidResponse) {
                    return StatusCode(500,
                        new { message = "Failed to index document", details = indexResponse.DebugInformation });
                }
            }
            catch (Exception e) {
                _logger.LogError($"Error while sending message to RabbitMQ: {e.Message}");
                return StatusCode(500, $"Error while sending message to RabbitMQ: {e.Message}");
            }

            return CreatedAtAction(nameof(GetDocument), new { id = savedItem.Id }, savedItem);
        }


        /// <summary>
        /// Downloads a file from the server by its name.
        /// </summary>
        /// <param name="fileName">The name of the file to be downloaded.</param>
        /// <returns>Returns the file for download or a 404 if the file is not found.</returns>
        /// <response code="200">File successfully retrieved for download.</response>
        /// <response code="404">The requested file was not found on the server.</response>
        /// <response code="500">Internal server error while retrieving the file.</response>
        [HttpGet("download/{fileName}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(Object), 404)]
        [ProducesResponseType(typeof(Object), 500)] 
        public IActionResult DownloadFile(string fileName) {
            // Combine the requested file with the uploads path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", fileName);
            Console.WriteLine(filePath);

            if (!System.IO.File.Exists(filePath)) {
                return NotFound(new { message = $"File '{filePath}' not found." });
            }

            // Determine the content type (optional, defaults to binary)
            var contentType = "application/octet-stream";

            // Return the file as a download
            return PhysicalFile(filePath, contentType, fileName);
        }

        /// <summary>
        /// Deletes a document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to be deleted.</param>
        /// <returns>Returns a 204 No Content if the document is successfully deleted, or a 404 if the document does not exist.</returns>
        /// <response code="204">Document successfully deleted.</response>
        /// <response code="400">Invalid ID supplied.</response>
        /// <response code="404">Document not found.</response>
        /// <response code="500">Internal server error while deleting the document.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteDocument(int id) {
            var documentresult = await GetDocument(id);
            if (documentresult is NotFoundResult) {
                return NotFound();
            }

            var document = (documentresult as OkObjectResult)?.Value as Document;
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var response = await client.DeleteAsync($"api/DAL/document/{id}");
            if (response.IsSuccessStatusCode) {
                var result = await _minioclientservice.DeleteFile(document.Title);
                var deleteResponse = await _elasticsearchService.DeleteDocumentAsync(id);

                _logger.LogInformation(result.ToString());
                _logger.LogInformation($"Deleted Document with ID: {id}");
                return NoContent();
            }

            _logger.LogError($"Failed to delete Document from Database with the ID {id}");
            return StatusCode((int)response.StatusCode, "Error deleting Document in DAL");
        }

        /// <summary>
        /// Searches for documents using a fuzzy search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for in the documents.</param>
        /// <returns>Returns a list of documents that match the fuzzy search term.</returns>
        /// <response code="200">Search results successfully returned (list of Documents).</response>
        /// <response code="204">No matching document</response>
        /// <response code="500">Internal server error while performing the search.</response>
        [HttpPost("search/fuzzy")]
        [ProducesResponseType(typeof(IEnumerable<Document>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByFuzzy([FromBody] string searchTerm) {
            if (string.IsNullOrWhiteSpace(searchTerm)) {
                return BadRequest(new { message = "Search term cannot be empty" });
            }

            var response = await _elasticsearchService.SearchDocumentsFuzzyAsync(searchTerm);

            return HandleSearchResponse(response);
        }

        /// <summary>
        /// Searches for documents using a search term passed as a query string.
        /// </summary>
        /// <param name="searchTerm">The term to search for in the documents.</param>
        /// <returns>Returns a list of documents that match the search term.</returns>
        /// <response code="200">Search results successfully returned (list of Documents).</response>
        /// <response code="204">No matching document</response>
        /// <response code="500">Internal server error while performing the search.</response>
        [HttpPost("search/querystring")]
        [ProducesResponseType(typeof(IEnumerable<Document>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchByQueryString([FromBody] string searchTerm) {
            if (string.IsNullOrWhiteSpace(searchTerm)) {
                return BadRequest(new { message = "Search term cannot be empty" });
            }

            var response = await _elasticsearchService.SearchDocumentsQueryAsync(searchTerm);

            return HandleSearchResponse(response);
        }
        
        private IActionResult HandleSearchResponse(SearchResponse<Document> response) {
            if (response.IsValidResponse) {
                if (response.Documents.Any()) {
                    return Ok(response.Documents);
                }

                return NoContent();
            }

            return StatusCode(500, new { message = "Failed to search documents", details = response.DebugInformation });
        }
    }
}