using Microsoft.EntityFrameworkCore;

namespace DocManSys_RestAPI.Models {
    public class DocumentContext : DbContext {
        public DocumentContext(DbContextOptions<DocumentContext> options) : base(options) {
        }

        public DbSet<Document> Documents { get; set; } = null!;
    }
}
