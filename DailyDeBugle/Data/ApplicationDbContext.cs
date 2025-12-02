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
        public DbSet<GlobalTextStyle> GlobalTextStyles { get; set; }
        
        // ДОБАВЛЯЕМ ЭТУ СТРОЧКУ
        public DbSet<ArticlePart> ArticleParts { get; set; }

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
            
            // ДОБАВЛЯЕМ КОНФИГУРАЦИЮ ДЛЯ ARTICLE С ПРОДОЛЖЕНИЯМИ
            modelBuilder.Entity<Article>(entity =>
            {
                // Связь для продолжений (самосвязь)
                entity.HasOne(a => a.ContinuedFromArticle)
                    .WithMany()
                    .HasForeignKey(a => a.ContinuedFromArticleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
        
                entity.HasOne(a => a.ContinuedOnArticle)
                    .WithMany()
                    .HasForeignKey(a => a.ContinuedOnArticleId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
    
                // Связь с частями статьи
                entity.HasMany(a => a.ArticleParts)
                    .WithOne(ap => ap.Article)
                    .HasForeignKey(ap => ap.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);
    
                // Связь с автором
                entity.HasOne(a => a.Author)
                    .WithMany(u => u.Articles)
                    .HasForeignKey(a => a.AuthorId);
    
                // Связь с выпуском
                entity.HasOne(a => a.Issue)
                    .WithMany(i => i.Articles)
                    .HasForeignKey(a => a.IssueId);
    
                // Ограничения для новых полей
                entity.Property(a => a.EstimatedHeightCm)
                    .HasDefaultValue(0);
        
                entity.Property(a => a.WordCount)
                    .HasDefaultValue(0);
        
                entity.Property(a => a.CharacterCount)
                    .HasDefaultValue(0);
        
                entity.Property(a => a.ParagraphCount)
                    .HasDefaultValue(0);
        
                entity.Property(a => a.HasContinuation)
                    .HasDefaultValue(false);
        
                entity.Property(a => a.FontFamily)
                    .HasMaxLength(50);
            });
            
            // ДОБАВЛЯЕМ КОНФИГУРАЦИЮ ДЛЯ ARTICLEPART
            modelBuilder.Entity<ArticlePart>(entity =>
            {
                entity.HasKey(ap => ap.ArticlePartId);
                
                entity.Property(ap => ap.ContentPart)
                    .IsRequired()
                    .HasMaxLength(4000);
                    
                entity.Property(ap => ap.PartNumber)
                    .IsRequired();
                    
                entity.Property(ap => ap.TotalParts)
                    .IsRequired();
                    
                entity.Property(ap => ap.IsBeginning)
                    .HasDefaultValue(false);
                    
                entity.Property(ap => ap.IsEnding)
                    .HasDefaultValue(false);
                    
                entity.Property(ap => ap.CreatedDate)
                    .HasDefaultValueSql("NOW()");
                    
                // Индекс для быстрого поиска частей по статье
                entity.HasIndex(ap => ap.ArticleId);
                
                // Индекс для поиска по номеру страницыи
                entity.HasIndex(ap => ap.PageNumber);
            });

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
            
            modelBuilder.Entity<LayoutElement>(entity =>
            {
                entity.HasKey(e => e.LayoutElementId);
        
                // Настройка связей
                entity.HasOne(e => e.PageLayout)
                    .WithMany(p => p.LayoutElements)
                    .HasForeignKey(e => e.PageLayoutId)
                    .OnDelete(DeleteBehavior.Cascade);
        
                entity.HasOne(e => e.Article)
                    .WithMany()
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);
        
                entity.HasOne(e => e.AdvertisementBlock)
                    .WithMany()
                    .HasForeignKey(e => e.AdvertisementBlockId)
                    .OnDelete(DeleteBehavior.Restrict);
              
                // Или если в классе свойство называется CreatedDate, то просто:
                entity.Property(e => e.CreatedDate);
        
                // Настройка других свойств при необходимости
                entity.Property(e => e.Type)
                    .HasConversion<int>(); // если это enum
        
                entity.Property(e => e.Position)
                    .IsRequired();
              
                entity.Property(e => e.Size)
                    .IsRequired();
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
            
            // Конфигурация для GlobalTextStyle
            modelBuilder.Entity<GlobalTextStyle>(entity =>
            {
                entity.HasKey(g => g.Id);
            
                entity.HasOne(g => g.Issue)
                    .WithOne()
                    .HasForeignKey<GlobalTextStyle>(g => g.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);
            
                entity.HasIndex(g => g.IssueId)
                    .IsUnique();
            
                // Значения по умолчанию
                entity.Property(g => g.PrimaryFont).HasDefaultValue("Times New Roman");
                entity.Property(g => g.HeadingFont).HasDefaultValue("Times New Roman");
                entity.Property(g => g.H1Size).HasDefaultValue(24);
                entity.Property(g => g.H2Size).HasDefaultValue(20);
                entity.Property(g => g.BodySize).HasDefaultValue(14);
                entity.Property(g => g.BodyLineSpacing).HasDefaultValue(1.4);
                entity.Property(g => g.HeadingLineSpacing).HasDefaultValue(1.2);
                entity.Property(g => g.ColumnCount).HasDefaultValue(2);
                entity.Property(g => g.ColumnGap).HasDefaultValue(1.0);
            });
            
            // ДОБАВЛЯЕМ КОНФИГУРАЦИЮ ДЛЯ ADVERTISEMENTBLOCK (если нужно)
            modelBuilder.Entity<AdvertisementBlock>(entity =>
            {
                entity.HasKey(ab => ab.AdvertisementBlockId);
                
                entity.Property(ab => ab.Advertiser)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(ab => ab.Content)
                    .IsRequired()
                    .HasMaxLength(1000);
                    
                entity.HasMany(ab => ab.LayoutElements)
                    .WithOne(le => le.AdvertisementBlock)
                    .HasForeignKey(le => le.AdvertisementBlockId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}