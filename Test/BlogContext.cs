// Install these NuGet packages:
// Microsoft.EntityFrameworkCore.Sqlite
// Microsoft.EntityFrameworkCore.Proxies  (REQUIRED for lazy loading)
// NUnit
// Microsoft.NET.Test.Sdk
// NUnit3TestAdapter

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using XafEfCoreLoading.Module.BusinessObjects;

// DbContext for testing with lazy loading
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
            optionsBuilder.UseInMemoryDatabase(_connectionString);
        }

        // Enable lazy loading
        optionsBuilder.UseLazyLoadingProxies();

        // Configure logging to show SQL queries
        optionsBuilder
            .LogTo(TestLogger.WriteSqlQuery, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // Seed some data using the shared helper
        SeedDataHelper.SeedData(modelBuilder);
    }
}

/*
=== WHY LAZY LOADING IS ESSENTIAL FOR DEMONSTRATING N+1 ===

Without lazy loading:
- blog.Posts.Count returns 0 (collection is not loaded)
- No additional queries are triggered
- N+1 problem is NOT demonstrated

With lazy loading enabled:
- blog.Posts.Count triggers automatic loading of posts
- Each blog access creates a separate SQL query
- N+1 problem is clearly visible in SQL logs

Required Setup for Lazy Loading:
1. Install Microsoft.EntityFrameworkCore.Proxies NuGet package
2. Call UseLazyLoadingProxies() in OnConfiguring
3. Make all navigation properties VIRTUAL
4. Use ICollection<T> instead of List<T> for collections

Alternative Ways to Enable Lazy Loading:
1. Proxies (what we use): Automatic, requires virtual properties
2. Explicit lazy loading: Manual control using context.Entry().Reference/Collection
3. ILazyLoader injection: More advanced, cleaner entities

The lazy loading proxies create dynamic proxy classes at runtime that inherit 
from your entity classes and override the virtual navigation properties to 
trigger loading when accessed.
*/