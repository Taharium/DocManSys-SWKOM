using DocManSys_DAL.Entities;

namespace DocManSys_DAL.Repositories {
    public interface IDocumentRepository {
        //Async methods
        Task<IEnumerable<DocumentEntity>> GetAllDocumentsAsync();
        Task<DocumentEntity?> GetDocumentByIdAsync(int id);
        Task AddDocumentAsync(DocumentEntity documentEntity);
        Task UpdateDocumentAsync(DocumentEntity documentEntity);
        Task DeleteDocumentAsync(int id);
    }
}
