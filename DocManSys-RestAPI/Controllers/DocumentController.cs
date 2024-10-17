using Microsoft.AspNetCore.Mvc;
using DocManSys_RestAPI.Models;
using AutoMapper;

namespace DocManSys_RestAPI.Controllers
{
    /// <summary>
    /// Document Controller with GET, PUT, POST, DELETE methods
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<DocumentController> _logger;
        private readonly IMapper _mapper;

        public DocumentController(IHttpClientFactory clientFactory, ILogger<DocumentController> logger, IMapper mapper) {
            _logger = logger;
            _clientFactory = clientFactory;
            _mapper = mapper;
        }

        // GET: api/Document
        /// <summary>
        /// Get all documents
        /// </summary>
        /// <returns>IEnumerable<Document></Document>></returns>
        [HttpGet]
        public async Task<IActionResult> GetDocuments([FromQuery] string searchTerm = "") {
            var client = _clientFactory.CreateClient("DocManSys-DAL");
            
            string requestUri = "/api/document";
            if (!string.IsNullOrEmpty(searchTerm)) {
                requestUri += $"?searchTerm={Uri.EscapeDataString(searchTerm)}"; // URL encode the search term
            }
            
            var response = await client.GetAsync(requestUri);
            if (response.IsSuccessStatusCode) {
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<DocManSys_DAL.Entities.Document>>();
                var documents = _mapper.Map<IEnumerable<Document>>(items);
                return Ok(documents);
            }
            
            //_logger.LogError();
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
            var response = await client.GetAsync($"/api/document/{id}");
            if (response.IsSuccessStatusCode) {
                var item = await response.Content.ReadFromJsonAsync<DocManSys_DAL.Entities.Document>();
                var document = _mapper.Map<Document>(item);
                if (item != null) return Ok(document);
                return NotFound();
            }

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

            var client = _clientFactory.CreateClient("DocManSys-DAL");
            var item = _mapper.Map<DocManSys_DAL.Entities.Document>(document);
            var response = await client.PutAsJsonAsync($"/api/document/{id}", item);
            if (response.IsSuccessStatusCode) {
                return NoContent();
            }

            return StatusCode((int)response.StatusCode, "Error updating Document in DAL");
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
            var item = _mapper.Map<DocManSys_DAL.Entities.Document>(document);
            var response = await client.PostAsJsonAsync("/api/document", item);
            if (response.IsSuccessStatusCode) {
                //var item = await response.Content.ReadFromJsonAsync<Document>();
                //if (item != null)
                    return CreatedAtAction(nameof(GetDocument), new { id = item.Id }, item);
            }

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
            var response = await client.DeleteAsync($"/api/document/{id}");
            if (response.IsSuccessStatusCode) {
                return NoContent();
            }
            return StatusCode((int)response.StatusCode, "Error deleting Document in DAL");
        }
        
        
    }
}
