using DocManSys_DAL.Entities;

namespace DocManSys_DAL.Repositories {
    public interface IDocumentRepository {
        //Async methods
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<Document?> GetDocumentByIdAsync(int id);
        Task AddDocumentAsync(Document document);
        Task UpdateDocumentAsync(Document document);
        Task DeleteDocumentAsync(int id);
    }
}
