using Microsoft.EntityFrameworkCore;
using DailyDeBugle.Models;

namespace DailyDeBugle.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<AdvertisementBlock> AdvertisementBlocks { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<PageLayout> PageLayouts { get; set; }
        public DbSet<LayoutElement> LayoutElements { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<HeaderFooterSettings> HeaderFooterSettings { get; set; }
        public DbSet<HeaderFooterTemplate> HeaderFooterTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Конфигурация связей и ограничений
            modelBuilder.Entity<Publication>()
                .HasMany(p => p.Editors)
                .WithMany(u => u.Publications);
                
            modelBuilder.Entity<Article>()
                .HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.AuthorId);

            // Конфигурация для HeaderFooterSettings
            modelBuilder.Entity<HeaderFooterSettings>(entity =>
            {
                entity.HasKey(hf => hf.Id);
                
                // Один-к-одному отношение с Issue
                entity.HasOne(hf => hf.Issue)
                      .WithOne()
                      .HasForeignKey<HeaderFooterSettings>(hf => hf.IssueId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Уникальный индекс для IssueId
                entity.HasIndex(hf => hf.IssueId)
                      .IsUnique();
                
                // Значения по умолчанию
                entity.Property(hf => hf.FontFamily)
                      .HasDefaultValue("Times New Roman");
                entity.Property(hf => hf.FontSize)
                      .HasDefaultValue(10);
                entity.Property(hf => hf.Alignment)
                      .HasDefaultValue("center");
                entity.Property(hf => hf.HeaderEnabled)
                      .HasDefaultValue(true);
                entity.Property(hf => hf.FooterEnabled)
                      .HasDefaultValue(true);
            });

            // Конфигурация для HeaderFooterTemplate
            modelBuilder.Entity<HeaderFooterTemplate>(entity =>
            {
                entity.HasKey(hft => hft.Id);
                
                entity.Property(hft => hft.IsSystemTemplate)
                      .HasDefaultValue(false);
                
                entity.Property(hft => hft.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(hft => hft.TemplateType)
                      .IsRequired()
                      .HasMaxLength(20);
            });

            // Добавляем начальные данные для шаблонов
            modelBuilder.Entity<HeaderFooterTemplate>().HasData(
                new HeaderFooterTemplate
                {
                    Id = 1,
                    Name = "Стандартный верхний",
                    TemplateType = "header",
                    ContentTemplate = "{PublicationName} • Выпуск №{IssueNumber} • {IssueDate}",
                    IsSystemTemplate = true
                },
                new HeaderFooterTemplate
                {
                    Id = 2,
                    Name = "Стандартный нижний", 
                    TemplateType = "footer",
                    ContentTemplate = "Контакт: {ContactEmail} • Страница {PageNumber} • {CurrentDate}",
                    IsSystemTemplate = true
                },
                new HeaderFooterTemplate
                {
                    Id = 3,
                    Name = "Минималистичный верхний",
                    TemplateType = "header", 
                    ContentTemplate = "{PublicationName} | {IssueDate}",
                    IsSystemTemplate = true
                },
                new HeaderFooterTemplate
                {
                    Id = 4,
                    Name = "Минималистичный нижний",
                    TemplateType = "footer",
                    ContentTemplate = "Стр. {PageNumber}",
                    IsSystemTemplate = true
                }
            );

            // Добавь остальные конфигурации по аналогии
        }
    }
}