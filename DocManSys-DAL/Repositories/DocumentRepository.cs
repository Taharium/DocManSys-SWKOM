using DocManSys_DAL.Data;
using DocManSys_DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocManSys_DAL.Repositories {
    public class DocumentRepository(DocumentContext context) : IDocumentRepository {
        public async Task<IEnumerable<DocumentEntity>> GetAllDocumentsAsync() {
            return await context.Documents.ToListAsync();
        }
        public async Task<DocumentEntity?> GetDocumentByIdAsync(int id) {
            return await context.Documents.FindAsync(id);
        }

        public async Task AddDocumentAsync(DocumentEntity documentEntity) {
            await context.Documents.AddAsync(documentEntity);
            await context.SaveChangesAsync();
        }
        public async Task DeleteDocumentAsync(int id) {
            var item = await context.Documents.FindAsync(id);
            if(item != null) {
                context.Documents.Remove(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateDocumentAsync(DocumentEntity documentEntity) {
            context.Documents.Update(documentEntity);
            await context.SaveChangesAsync();
        }
    }
}
