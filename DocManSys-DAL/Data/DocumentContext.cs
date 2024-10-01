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
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
