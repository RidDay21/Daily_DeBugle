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
        public DbSet<ArticlePart> ArticleParts { get; set; }
        public DbSet<ArticleImage> ArticleImages { get; set; }
        public DbSet<IssueComment> IssueComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация связей и ограничений
            modelBuilder.Entity<Publication>()
                .HasMany(p => p.Editors)
                .WithMany(u => u.Publications);

            // КОНФИГУРАЦИЯ ДЛЯ TEMPLATE
            modelBuilder.Entity<Template>(entity =>
            {
                entity.HasKey(t => t.TemplateId);
                
                entity.Property(t => t.DefaultColumnCount)
                    .HasDefaultValue(2);
                
                entity.Property(t => t.DefaultMarginTop)
                    .HasDefaultValue(2.0);
                entity.Property(t => t.DefaultMarginBottom)
                    .HasDefaultValue(2.0);
                entity.Property(t => t.DefaultMarginLeft)
                    .HasDefaultValue(2.0);
                entity.Property(t => t.DefaultMarginRight)
                    .HasDefaultValue(2.0);
                entity.Property(t => t.DefaultColumnGap)
                    .HasDefaultValue(1.0);
                
                entity.Property(t => t.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(t => t.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
            });

            // КОНФИГУРАЦИЯ ДЛЯ PAGE LAYOUT
            modelBuilder.Entity<PageLayout>(entity =>
            {
                entity.HasKey(p => p.PageLayoutId);
                
                entity.HasOne(p => p.Issue)
                    .WithMany(i => i.PageLayouts)
                    .HasForeignKey(p => p.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(p => p.Template)
                    .WithMany(t => t.PageLayouts)
                    .HasForeignKey(p => p.TemplateId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
                
                entity.Property(p => p.TemplateId)
                    .HasDefaultValue(1);
                
                entity.Property(p => p.ColumnCount)
                    .HasDefaultValue(2);
                
                entity.Property(p => p.MarginTop)
                    .HasDefaultValue(2.0);
                entity.Property(p => p.MarginBottom)
                    .HasDefaultValue(2.0);
                entity.Property(p => p.MarginLeft)
                    .HasDefaultValue(2.0);
                entity.Property(p => p.MarginRight)
                    .HasDefaultValue(2.0);
                entity.Property(p => p.ColumnGap)
                    .HasDefaultValue(1.0);
                
                entity.Property(p => p.TextAreaWidth)
                    .HasDefaultValue(8.0);
                entity.Property(p => p.TextAreaHeight)
                    .HasDefaultValue(10.0);
                entity.Property(p => p.ImageAreaWidth)
                    .HasDefaultValue(8.0);
                entity.Property(p => p.ImageAreaHeight)
                    .HasDefaultValue(6.0);
                
                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(p => p.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
            });

            // ИСПРАВЛЕННАЯ КОНФИГУРАЦИЯ ДЛЯ LAYOUT ELEMENT
            modelBuilder.Entity<LayoutElement>(entity =>
            {
                entity.HasKey(e => e.LayoutElementId);

                // Связь с PageLayout
                entity.HasOne(e => e.PageLayout)
                    .WithMany(p => p.LayoutElements)
                    .HasForeignKey(e => e.PageLayoutId)
                    .OnDelete(DeleteBehavior.Cascade);

                // ИСПРАВЛЕНО: Связь с Article - каскадное удаление
                entity.HasOne(e => e.Article)
                    .WithMany()
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade)  // Изменено с Restrict на Cascade
                    .IsRequired(false);

                // Связь с AdvertisementBlock - только ОДНА
                entity.HasOne(e => e.AdvertisementBlock)
                    .WithMany()
                    .HasForeignKey(e => e.AdvertisementBlockId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.Type)
                    .HasConversion<int>();

                entity.Property(e => e.Position)
                    .IsRequired();

                entity.Property(e => e.Size)
                    .IsRequired();
            });

            modelBuilder.Entity<GlobalTextStyle>()
                .HasOne(g => g.Issue)
                .WithMany()
                .HasForeignKey(g => g.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            // КОНФИГУРАЦИЯ ДЛЯ ARTICLE С ПРОДОЛЖЕНИЯМИ
            modelBuilder.Entity<Article>(entity =>
            {
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

                entity.HasMany(a => a.ArticleParts)
                    .WithOne(ap => ap.Article)
                    .HasForeignKey(ap => ap.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Author)
                    .WithMany(u => u.Articles)
                    .HasForeignKey(a => a.AuthorId);

                entity.HasOne(a => a.Issue)
                    .WithMany(i => i.Articles)
                    .HasForeignKey(a => a.IssueId);

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

            modelBuilder.Entity<ArticleImage>(entity =>
            {
                entity.HasKey(ai => ai.ArticleImageId);

                entity.HasOne(ai => ai.Article)
                    .WithMany(a => a.Images)
                    .HasForeignKey(ai => ai.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(ai => ai.ImagePath).IsRequired().HasMaxLength(500);
                entity.Property(ai => ai.Caption).HasMaxLength(200);
            });

            modelBuilder.Entity<IssueComment>(entity =>
            {
                entity.HasKey(c => c.IssueCommentId);

                entity.HasOne(c => c.Issue)
                    .WithMany(i => i.ReaderComments)
                    .HasForeignKey(c => c.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.Content).IsRequired().HasMaxLength(2000);
            });

            // КОНФИГУРАЦИЯ ДЛЯ ARTICLEPART
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

                entity.HasIndex(ap => ap.ArticleId);
                entity.HasIndex(ap => ap.PageNumber);
            });

            // Конфигурация для HeaderFooterSettings
            modelBuilder.Entity<HeaderFooterSettings>(entity =>
            {
                entity.HasKey(hf => hf.Id);

                entity.HasOne(hf => hf.Issue)
                    .WithOne()
                    .HasForeignKey<HeaderFooterSettings>(hf => hf.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(hf => hf.IssueId)
                    .IsUnique();

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

            // Конфигурация для рекламных блоков
            modelBuilder.Entity<AdvertisementBlock>(entity =>
            {
                entity.HasKey(e => e.AdvertisementBlockId);
                entity.Property(e => e.Advertiser).IsRequired().HasMaxLength(200);
            });

            // Конфигурация для размещения рекламы
            modelBuilder.Entity<AdvertisementPlacement>(entity =>
            {
                entity.HasKey(e => e.AdvertisementPlacementId);

                entity.HasOne(e => e.Issue)
                    .WithMany()
                    .HasForeignKey(e => e.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AdvertisementBlock)
                    .WithMany()
                    .HasForeignKey(e => e.AdvertisementBlockId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}