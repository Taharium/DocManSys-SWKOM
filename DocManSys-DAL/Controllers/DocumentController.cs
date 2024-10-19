using DocManSys_DAL.Entities;
using DocManSys_DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DocManSys_DAL.Controllers {
    [Area("DAL")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class DocumentController(IDocumentRepository documentRepository) : ControllerBase {
        [HttpGet]
        public async Task<IEnumerable<DocumentEntity>> GetAllDocuments([FromQuery] string searchTerm = "") {
            var documents = await documentRepository.GetAllDocumentsAsync();
            if (!string.IsNullOrEmpty(searchTerm)) {
                documents = documents.Where(doc => 
                    doc.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    doc.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            return documents.Reverse();
        }

        [HttpGet("{id}")]
        public async Task<DocumentEntity?> GetDocumentById(int id) {
            return await documentRepository.GetDocumentByIdAsync(id);
        }

        [HttpPost]
        public async Task<IActionResult> AddDocument(DocumentEntity documentEntity) {
            if(string.IsNullOrEmpty(documentEntity.Title)) {
                return BadRequest(new {message = "Title is required"});
            }

            await documentRepository.AddDocumentAsync(documentEntity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDocument(DocumentEntity documentEntity) {
            var item = await documentRepository.GetDocumentByIdAsync(documentEntity.Id);
            if (item == null) {
                return NotFound();
            }

            item.Title = documentEntity.Title;
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
