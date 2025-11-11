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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация связей и ограничений
            modelBuilder.Entity<Publication>()
                .HasMany(p => p.Editors)
                .WithMany(u => u.Publications);
                
            modelBuilder.Entity<Article>()
                .HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.AuthorId);
                
            // Добавь остальные конфигурации по аналогии
        }
    }
}