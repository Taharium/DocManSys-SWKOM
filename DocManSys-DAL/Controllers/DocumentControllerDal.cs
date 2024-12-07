using DocManSys_DAL.Entities;
using DocManSys_DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DocManSys_DAL.Controllers {
    [Area("DAL")]
    [Route("api/[area]/document")]
    [ApiController]
    public class DocumentControllerDal(IDocumentRepository documentRepository, ILogger<DocumentControllerDal> logger) : ControllerBase {
        [HttpGet]
        public async Task<IEnumerable<DocumentEntity>> GetAllDocuments([FromQuery] string searchTerm = "") {
            var documents = await documentRepository.GetAllDocumentsAsync();
            if (!string.IsNullOrEmpty(searchTerm)) {
                documents = documents.Where(doc => 
                    doc.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    doc.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            logger.LogInformation("DAL: Retrieving Documents from Database!");
            return documents.Reverse();
        }

        [HttpGet("{id}")]
        public async Task<DocumentEntity?> GetDocumentById(int id) {
            logger.LogInformation($"DAL: Retrieving Document with ID: {id}");
            return await documentRepository.GetDocumentByIdAsync(id);
        }

        [HttpPost]
        public async Task<IActionResult> AddDocument(DocumentEntity documentEntity) {
            if(string.IsNullOrEmpty(documentEntity.Title)) {
                logger.LogWarning("DAL: Error while adding Document: Title is empty");
                return BadRequest(new {message = "Title is required"});
            }

            logger.LogInformation($"DAL: Adding Document with ID: {documentEntity.Id}");
            await documentRepository.AddDocumentAsync(documentEntity);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDocument(DocumentEntity documentEntity) {
            var item = await documentRepository.GetDocumentByIdAsync(documentEntity.Id);
            if (item == null) {
                logger.LogWarning($"DAL: Error while updating Document: Document with ID: {documentEntity.Id} not found");
                return NotFound();
            }
            
            item.Title = documentEntity.Title;
            logger.LogInformation($"DAL: Updating Document with ID: {documentEntity.Id}");
            await documentRepository.UpdateDocumentAsync(item);
            return Ok();
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, DocumentEntity item) {
            var documentEntity = await documentRepository.GetDocumentByIdAsync(id);
            if (documentEntity == null) {
                logger.LogWarning($"DAL: Error while updating Document: Document with ID: {item.Id} not found");

                return NotFound();
            }

            documentEntity.Author = item.Author;
            documentEntity.Title = item.Title;
            documentEntity.OcrText = item.OcrText;
            logger.LogInformation($"DAL: Updating Document with ID: {documentEntity.Id}");
            await documentRepository.UpdateDocumentAsync(documentEntity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id) {
            var item = await documentRepository.GetDocumentByIdAsync(id);
            if (item == null) {
                logger.LogWarning($"DAL: Error while deleting Document: Document with ID: {id} not found");
                return NotFound();
            }
            
            logger.LogInformation($"DAL: Deleting Document with ID: {id}");
            await documentRepository.DeleteDocumentAsync(id);
            return NoContent();
        }

    }
}
