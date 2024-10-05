using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocManSys_RestAPI.Models;

namespace DocManSys_RestAPI.Controllers
{
    /// <summary>
    /// Document Controller with GET, PUT, POST, DELETE methods
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase {
        private readonly IHttpClientFactory _clientFactory;

        public DocumentController(IHttpClientFactory clientFactory) {
            _clientFactory = clientFactory;
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
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<Document>>();
                return Ok(items);
            }

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
                var item = await response.Content.ReadFromJsonAsync<Document>();
                if (item != null) return Ok(item);
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
            var response = await client.PutAsJsonAsync($"/api/document/{id}", document);
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
            var response = await client.PostAsJsonAsync("/api/document", document);
            if (response.IsSuccessStatusCode) {
                //var item = await response.Content.ReadFromJsonAsync<Document>();
                //if (item != null)
                    return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
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
