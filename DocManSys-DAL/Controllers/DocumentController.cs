using DocManSys_DAL.Entities;
using DocManSys_DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocManSys_DAL.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController(IDocumentRepository documentRepository) : ControllerBase {
        [HttpGet]
        public async Task<IEnumerable<Document>> GetAllDocuments() {
            return await documentRepository.GetAllDocumentsAsync();
        }

        [HttpGet("{id}")]
        public async Task<Document?> GetDocumentById(int id) {
            return await documentRepository.GetDocumentByIdAsync(id);
        }

        [HttpPost]
        public async Task<IActionResult> AddDocument(Document document) {
            if(string.IsNullOrEmpty(document.Title)) {
                return BadRequest(new {message = "Title is required"});
            }

            await documentRepository.AddDocumentAsync(document);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDocument(Document document) {
            var item = await documentRepository.GetDocumentByIdAsync(document.Id);
            if (item == null) {
                return NotFound();
            }

            item.Title = document.Title;
            await documentRepository.UpdateDocumentAsync(item);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id) {
            var item = await documentRepository.GetDocumentByIdAsync(id);
            if (item == null) {
                return NotFound();
            }

            await documentRepository.DeleteDocumentAsync(id);
            return NoContent();
        }

    }
}
