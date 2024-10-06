using System.Collections.Generic;
using DocManSys_DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocManSys_DAL.Data {
    public sealed class DocumentContext(DbContextOptions<DocumentContext> options) : DbContext(options) {
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Manuelle Konfiguration der Tabelle
            modelBuilder.Entity<Document>(entity => {
                entity.ToTable("Documents");  // Setzt den Tabellennamen

                entity.HasKey(e => e.Id);  // Setzt den Primärschlüssel

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);  // Konfiguriert den "Name"-Spalten
                
                entity.Property(e => e.Author)
                    .HasMaxLength(50);

                entity.Property(e => e.Image);
            });

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>().HasData(
                new Document {
                    Id = 1,
                    Title = "Sample Document 1",
                    Author = "Author A",
                    Image = "Images/default_pdf.png"
                },
                new Document {
                    Id = 2,
                    Title = "Sample Document 2",
                    Author = "Author B",
                    Image = "Images/default_pdf.png"
                }
            );
        }
    }
}
