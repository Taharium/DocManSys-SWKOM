using DocManSys_DAL.Data;
using DocManSys_DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocManSys_DAL.Repositories {
    public class DocumentRepository(DocumentContext context) : IDocumentRepository {
        public async Task<IEnumerable<Document>> GetAllDocumentsAsync() {
            return await context.Documents.ToListAsync();
        }
        public async Task<Document?> GetDocumentByIdAsync(int id) {
            return await context.Documents.FindAsync(id);
        }

        public async Task AddDocumentAsync(Document document) {
            await context.Documents.AddAsync(document);
            await context.SaveChangesAsync();
        }
        public async Task DeleteDocumentAsync(int id) {
            var item = await context.Documents.FindAsync(id);
            if(item != null) {
                context.Documents.Remove(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateDocumentAsync(Document document) {
            context.Documents.Update(document);
            await context.SaveChangesAsync();
        }
    }
}
