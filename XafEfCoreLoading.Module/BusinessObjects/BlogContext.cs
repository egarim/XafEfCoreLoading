using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogContext" /> class. The
        /// <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" />
        /// method will be called to configure the database (and other options) to be used for this context.
        /// </summary>
        /// <remarks>
        /// See <see href="https://aka.ms/efcore-docs-dbcontext">BlogContext lifetime, configuration, and initialization</see>
        /// for more information and examples.
        /// </remarks>
        [RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
        [RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT, and running the application may generate unexpected runtime failures.")]
        protected BlogContext()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogContext" /> class using the specified options.
        /// The <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" /> method will still be called to allow further
        /// configuration of the options.
        /// </summary>
        /// <remarks>
        /// See <see href="https://aka.ms/efcore-docs-dbcontext">BlogContext lifetime, configuration, and initialization</see> and
        /// <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
        /// </remarks>
        /// <param name="options">The options for this context.</param>
        [RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
        [RequiresDynamicCode("EF Core isn't fully compatible with NativeAOT, and running the application may generate unexpected runtime failures.")]
        public BlogContext(DbContextOptions options) : base(options)
        {

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

            // Only seed data if it hasn't already been seeded
            SeedDataHelper.SeedData(modelBuilder);
        }
    }

    // Helper class for seed data to eliminate duplication
    public static class SeedDataHelper
    {
        // Track if the seeding has been done already
        private static bool _isSeedDataApplied = false;

        public static void SeedData(ModelBuilder modelBuilder)
        {
            // Skip seeding if data has already been added to this ModelBuilder
            if (HasSeedDataBeenApplied(modelBuilder))
            {
                return;
            }

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

            // Mark that seed data has been applied
            _isSeedDataApplied = true;
        }

        private static bool HasSeedDataBeenApplied(ModelBuilder modelBuilder)
        {
            // Check if any entities in the modelBuilder already have seed data
            var entityType = modelBuilder.Model.FindEntityType(typeof(Blog));
            if (entityType == null) return false;
            
            // Check the internal _data collection to see if seed data has been defined
            var seedDataProperty = typeof(Microsoft.EntityFrameworkCore.Metadata.Internal.EntityType)
                .GetProperty("SeedData", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (seedDataProperty == null) return _isSeedDataApplied;
            
            var seedData = seedDataProperty.GetValue(entityType);
            return seedData != null || _isSeedDataApplied;
        }
    }
}
