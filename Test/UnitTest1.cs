// Test Class
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Test;
using XafEfCoreLoading.Module.BusinessObjects;

[TestFixture]
public class EfCorePrefetchingTests
{
    private string _databasePath = string.Empty;
    private BlogContext _context = null!;

    public static void WriteLine(string value)
    {
        Debug.WriteLine(value);
        Console.WriteLine(value);   
    }
    [SetUp]
    public void Setup()
    {
        // Use a file-based SQLite database to ensure we can see the queries
        _databasePath = Path.Combine(Path.GetTempPath(), $"test_blog_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={_databasePath}";

        _context = new BlogContext(connectionString, useFileDatabase: true);
        _context.Database.EnsureCreated();

        WriteLine($"=== Database created at: {_databasePath} ===");
        WriteLine("=== Starting test with fresh data ===\n");
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
            WriteLine($"\n=== Database deleted: {_databasePath} ===");
        }
    }

    [Test]
    public void Test_N_Plus_One_Problem()
    {
        WriteLine("=== Demonstrating N+1 Problem ===");

        // This will cause N+1 queries
        var blogs = _context.Blogs.ToList();
        WriteLine($"Loaded {blogs.Count} blogs");

        foreach (var blog in blogs)
        {
            WriteLine($"Blog: {blog.Title}");
            // Each access to Posts triggers a separate query
            WriteLine($"  Post count: {blog.Posts.Count}");
        }
    }

    [Test]
    public void Test_Eager_Loading_With_Include()
    {
        WriteLine("=== Eager Loading with Include ===");

        var blogsWithPosts = _context.Blogs
            .Include(b => b.Posts)
            .ToList();

        WriteLine($"Loaded {blogsWithPosts.Count} blogs with posts");

        foreach (var blog in blogsWithPosts)
        {
            WriteLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}");
        }
    }

    [Test]
    public void Test_Multiple_Includes_With_ThenInclude()
    {
        WriteLine("=== Multiple Includes with ThenInclude ===");

        var blogsWithPostsAndComments = _context.Blogs
            .Include(b => b.Posts)
                .ThenInclude(p => p.Comments)
            .ToList();

        foreach (var blog in blogsWithPostsAndComments)
        {
            WriteLine($"Blog: {blog.Title}");
            foreach (var post in blog.Posts)
            {
                WriteLine($"  Post: {post.Title} - Comments: {post.Comments.Count}");
            }
        }
    }

    [Test]
    public void Test_Projection_With_Select()
    {
        WriteLine("=== Projection with Select ===");

        var blogData = _context.Blogs
            .Select(b => new
            {
                BlogTitle = b.Title,
                PostCount = b.Posts.Count(),
                RecentPosts = b.Posts
                    .OrderByDescending(p => p.PublishedDate)
                    .Take(2)
                    .Select(p => new { p.Title, p.PublishedDate })
            })
            .ToList();

        foreach (var blog in blogData)
        {
            WriteLine($"Blog: {blog.BlogTitle} - Total Posts: {blog.PostCount}");
            foreach (var post in blog.RecentPosts)
            {
                WriteLine($"  Recent Post: {post.Title} ({post.PublishedDate:yyyy-MM-dd})");
            }
        }
    }

    [Test]
    public void Test_Split_Query()
    {
        WriteLine("=== Split Query ===");

        var blogs = _context.Blogs
            .AsSplitQuery()
            .Include(b => b.Posts)
            .Include(b => b.Tags)
            .ToList();

        foreach (var blog in blogs)
        {
            Console.WriteLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}, Tags: {blog.Tags.Count}");
        }
    }

    [Test]
    public void Test_Filtered_Include()
    {
        WriteLine("=== Filtered Include (Recent Posts Only) ===");

        var cutoffDate = DateTime.Now.AddDays(-15);
        var blogsWithRecentPosts = _context.Blogs
            .Include(b => b.Posts.Where(p => p.PublishedDate > cutoffDate))
            .ToList();

        foreach (var blog in blogsWithRecentPosts)
        {
            WriteLine($"Blog: {blog.Title} - Recent Posts: {blog.Posts.Count}");
            foreach (var post in blog.Posts)
            {
                WriteLine($"  Recent Post: {post.Title} ({post.PublishedDate:yyyy-MM-dd})");
            }
        }
    }

    [Test]
    public void Test_Explicit_Loading()
    {
        WriteLine("=== Explicit Loading ===");

        var blogs = _context.Blogs.ToList();
        WriteLine($"Loaded {blogs.Count} blogs (without posts)");

        // Now explicitly load posts for all blogs in one query
        foreach (var blog in blogs)
        {
            _context.Entry(blog)
                .Collection(b => b.Posts)
                .Load();
        }

        foreach (var blog in blogs)
        {
           WriteLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}");
        }
    }

    [Test]
    public void Test_Batch_Loading_Pattern()
    {
        WriteLine("=== Batch Loading Pattern ===");

        var blogs = _context.Blogs.ToList();
        var blogIds = blogs.Select(b => b.Id).ToList();

        WriteLine($"Loaded {blogs.Count} blogs");

        // Single query to get all posts for all blogs
        var posts = _context.Posts
            .Where(p => blogIds.Contains(p.BlogId))
            .ToList();

        WriteLine($"Loaded {posts.Count} posts in batch");

        // Group posts by blog in memory
        var postsByBlog = posts.GroupBy(p => p.BlogId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var blog in blogs)
        {
            var blogPosts = postsByBlog.GetValueOrDefault(blog.Id, new List<Post>());
            WriteLine($"Blog: {blog.Title} - Posts: {blogPosts.Count}");
        }
    }

    [Test]
    public void Test_Performance_Comparison()
    {
        WriteLine("=== Performance Comparison ===");

        // Reset context to clear any cached data
        _context.ChangeTracker.Clear();

        WriteLine("\n--- N+1 Problem (Multiple Queries) ---");
        var blogs1 = _context.Blogs.ToList();
        foreach (var blog in blogs1)
        {
            var count = blog.Posts.Count(); // Triggers separate query
        }

        _context.ChangeTracker.Clear();

        WriteLine("\n--- Eager Loading (Single Query) ---");
        var blogs2 = _context.Blogs
            .Include(b => b.Posts)
            .ToList();
        foreach (var blog in blogs2)
        {
            var count = blog.Posts.Count; // No additional query
        }

        _context.ChangeTracker.Clear();

        WriteLine("\n--- Projection (Minimal Data) ---");
        var blogSummaries = _context.Blogs
            .Select(b => new { b.Title, PostCount = b.Posts.Count() })
            .ToList();
        foreach (var summary in blogSummaries)
        {
            // All data already loaded
        }
    }
}