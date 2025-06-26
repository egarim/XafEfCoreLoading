using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace XafEfCoreLoading.Module.BusinessObjects
{
    // DbContext with Logging
    public class BlogContext : DbContext
    {
        private readonly string _connectionString;
        private readonly bool _useFileDatabase;

        public BlogContext(string connectionString, bool useFileDatabase = false)
        {
            _connectionString = connectionString;
            _useFileDatabase = useFileDatabase;
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_useFileDatabase)
            {
                optionsBuilder.UseSqlite(_connectionString);
            }
            else
            {
                optionsBuilder.UseSqlite(_connectionString);
            }

            // ENABLE LAZY LOADING - This is crucial for N+1 problem demonstration
            optionsBuilder.UseLazyLoadingProxies();

            // Configure logging to show SQL queries using our TestLogger
            optionsBuilder
                .LogTo(TestLogger.WriteSqlQuery, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Blog)
                .WithMany(b => b.Posts)
                .HasForeignKey(p => p.BlogId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId);

            // Many-to-many relationship between Blog and Tag
            modelBuilder.Entity<Blog>()
                .HasMany(b => b.Tags)
                .WithMany(t => t.Blogs);

            // Seed some data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var blogs = new[]
            {
            new Blog { Id = 1, Title = "Tech Blog", Description = "Technology articles", CreatedDate = DateTime.Now.AddDays(-100) },
            new Blog { Id = 2, Title = "Cooking Blog", Description = "Delicious recipes", CreatedDate = DateTime.Now.AddDays(-80) },
            new Blog { Id = 3, Title = "Travel Blog", Description = "Adventure stories", CreatedDate = DateTime.Now.AddDays(-60) }
        };

            var tags = new[]
            {
            new Tag { Id = 1, Name = "Programming" },
            new Tag { Id = 2, Name = "C#" },
            new Tag { Id = 3, Name = "Food" },
            new Tag { Id = 4, Name = "Travel" }
        };

            var posts = new[]
            {
            new Post { Id = 1, Title = "EF Core Basics", Content = "Introduction to EF Core", PublishedDate = DateTime.Now.AddDays(-10), BlogId = 1 },
            new Post { Id = 2, Title = "Advanced EF Core", Content = "Advanced EF Core techniques", PublishedDate = DateTime.Now.AddDays(-5), BlogId = 1 },
            new Post { Id = 3, Title = "Chocolate Cake Recipe", Content = "How to make chocolate cake", PublishedDate = DateTime.Now.AddDays(-15), BlogId = 2 },
            new Post { Id = 4, Title = "Pasta Recipe", Content = "Italian pasta recipe", PublishedDate = DateTime.Now.AddDays(-8), BlogId = 2 },
            new Post { Id = 5, Title = "Paris Trip", Content = "My trip to Paris", PublishedDate = DateTime.Now.AddDays(-20), BlogId = 3 },
            new Post { Id = 6, Title = "Tokyo Adventure", Content = "Adventures in Tokyo", PublishedDate = DateTime.Now.AddDays(-12), BlogId = 3 }
        };

            var comments = new[]
            {
            new Comment { Id = 1, Author = "John", Content = "Great article!", CreatedDate = DateTime.Now.AddDays(-9), PostId = 1 },
            new Comment { Id = 2, Author = "Jane", Content = "Very helpful", CreatedDate = DateTime.Now.AddDays(-8), PostId = 1 },
            new Comment { Id = 3, Author = "Bob", Content = "Thanks for sharing", CreatedDate = DateTime.Now.AddDays(-4), PostId = 2 },
            new Comment { Id = 4, Author = "Alice", Content = "Delicious!", CreatedDate = DateTime.Now.AddDays(-14), PostId = 3 },
            new Comment { Id = 5, Author = "Charlie", Content = "I tried this recipe", CreatedDate = DateTime.Now.AddDays(-7), PostId = 4 },
            new Comment { Id = 6, Author = "Diana", Content = "Amazing photos!", CreatedDate = DateTime.Now.AddDays(-19), PostId = 5 }
        };

            modelBuilder.Entity<Blog>().HasData(blogs);
            modelBuilder.Entity<Tag>().HasData(tags);
            modelBuilder.Entity<Post>().HasData(posts);
            modelBuilder.Entity<Comment>().HasData(comments);
        }
    }
}
