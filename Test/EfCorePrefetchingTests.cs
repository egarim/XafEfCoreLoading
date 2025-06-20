// Install these NuGet packages:
// Microsoft.EntityFrameworkCore.Sqlite
// Microsoft.EntityFrameworkCore.Proxies  (REQUIRED for lazy loading)
// NUnit
// Microsoft.NET.Test.Sdk
// NUnit3TestAdapter

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

// Test Class
[TestFixture]
public class EfCorePrefetchingTests
{
    private string _databasePath = string.Empty;
    private BlogContext _context = null!;

    [SetUp]
    public void Setup()
    {
        // Use a file-based SQLite database to ensure we can see the queries
        _databasePath = Path.Combine(Path.GetTempPath(), $"test_blog_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={_databasePath}";

        _context = new BlogContext(connectionString, useFileDatabase: true);
        _context.Database.EnsureCreated();

        TestLogger.WriteLine($"=== Database created at: {_databasePath} ===");
        TestLogger.WriteLine("=== Starting test with fresh data ===\n");
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();

        if (File.Exists(_databasePath))
        {
            //File.Delete(_databasePath);
            TestLogger.WriteLine($"\n=== Database deleted: {_databasePath} ===");
        }
    }

    [Test]
    public void Test_N_Plus_One_Problem_With_Lazy_Loading()
    {
        TestLogger.WriteHeader("Demonstrating N+1 Problem with Lazy Loading");
        TestLogger.WriteLine("Each access to Posts collection will trigger a separate SQL query!");

        // Clear change tracker to ensure fresh entities
        _context.ChangeTracker.Clear();

        // This will cause N+1 queries due to lazy loading
        var blogs = _context.Blogs.ToList(); // Query 1: Load blogs
        TestLogger.WriteLine($"Loaded {blogs.Count} blogs with initial query");

        // Check if entities are proxies (they should be for lazy loading to work)
        foreach (var blog in blogs)
        {
            var entityType = blog.GetType();
            TestLogger.WriteLine($"Blog entity type: {entityType.Name} (IsProxy: {entityType.BaseType != null})");
        }

        TestLogger.WriteLine("\nNow accessing Posts for each blog (this triggers lazy loading):");

        foreach (var blog in blogs)
        {
            TestLogger.WriteLine($"\nBlog: {blog.Title}");
            // Each access to Posts triggers a separate query due to lazy loading!
            var postCount = blog.Posts.Count; // This should trigger a query
            TestLogger.WriteLine($"  Post count: {postCount}");

            // If lazy loading didn't work, manually load to show the difference
            if (postCount == 0)
            {
                TestLogger.WriteLine("  Lazy loading didn't trigger - manually loading posts...");
                _context.Entry(blog).Collection(b => b.Posts).Load();
                TestLogger.WriteLine($"  Post count after manual load: {blog.Posts.Count}");
            }
        }

        TestLogger.WriteLine("\n>>> This should result in 1 + N queries (N+1 problem)!");
        TestLogger.WriteLine(">>> 1 query for blogs + 1 query per blog for posts = 4 total queries");
    }

    [Test]
    public void Test_N_Plus_One_Problem_Alternative_Approach()
    {
        TestLogger.WriteLine("=== Alternative N+1 Demonstration (Explicit Loading) ===");
        TestLogger.WriteLine("This manually demonstrates the N+1 pattern even if lazy loading isn't working");

        _context.ChangeTracker.Clear();

        // Query 1: Load blogs only
        var blogs = _context.Blogs.ToList();
        TestLogger.WriteLine($"Query 1: Loaded {blogs.Count} blogs");

        // Now manually trigger one query per blog (simulating lazy loading)
        foreach (var blog in blogs)
        {
            TestLogger.WriteLine($"\nBlog: {blog.Title}");

            // This explicitly loads posts for THIS blog only (simulates lazy loading)
            var posts = _context.Posts.Where(p => p.BlogId == blog.Id).ToList();
            TestLogger.WriteLine($"Query {blog.Id + 1}: Loaded {posts.Count} posts for blog {blog.Id}");

            foreach (var post in posts.Take(1))
            {
                TestLogger.WriteLine($"  Sample post: {post.Title}");
            }
        }

        TestLogger.WriteLine($"\n>>> Total queries executed: {blogs.Count + 1} (1 for blogs + {blogs.Count} for posts)");
        TestLogger.WriteLine(">>> This is the N+1 problem!");
    }

    [Test]
    public void Test_Verify_Proxy_Creation()
    {
        TestLogger.WriteLine("=== Verifying Proxy Creation for Lazy Loading ===");

        _context.ChangeTracker.Clear();

        var blog = _context.Blogs.First();

        TestLogger.WriteLine($"Blog type: {blog.GetType().FullName}");
        TestLogger.WriteLine($"Is proxy: {blog.GetType().BaseType != typeof(object)}");
        TestLogger.WriteLine($"Base type: {blog.GetType().BaseType?.Name}");

        // Try to access Posts and see what happens
        TestLogger.WriteLine("\nAccessing Posts property...");
        var postCount = blog.Posts.Count;
        TestLogger.WriteLine($"Posts count: {postCount}");

        // Check if the Posts collection was actually loaded
        var entry = _context.Entry(blog);
        var postsEntry = entry.Collection(b => b.Posts);
        TestLogger.WriteLine($"Posts collection is loaded: {postsEntry.IsLoaded}");
    }
        [Test]
        public void Test_N_Plus_One_vs_Include_Comparison()
        {
            TestLogger.WriteLine("=== SIDE-BY-SIDE: N+1 Problem vs Include() Solution ===\n");

            // === PART 1: Demonstrate N+1 Problem ===
            TestLogger.WriteLine("🔴 BAD APPROACH: N+1 Problem");
            TestLogger.WriteLine("════════════════════════════════════════");
            _context.ChangeTracker.Clear();

            var blogsN1 = _context.Blogs.ToList(); // Query 1
            TestLogger.WriteLine($"Step 1: Loaded {blogsN1.Count} blogs");

            foreach (var blog in blogsN1)
            {
                // Each of these causes a separate query - the N+1 problem!
                var posts = _context.Posts.Where(p => p.BlogId == blog.Id).ToList();
                TestLogger.WriteLine($"Step {blog.Id + 1}: Blog '{blog.Title}' has {posts.Count} posts");
            }

            TestLogger.WriteLine("🔴 Result: 4 total queries (1 + 3) - INEFFICIENT!\n");

            // === PART 2: Show Include() Solution ===
            TestLogger.WriteLine("✅ GOOD APPROACH: Include() Solution");
            TestLogger.WriteLine("════════════════════════════════════════");
            _context.ChangeTracker.Clear();

            var blogsInclude = _context.Blogs
                .Include(b => b.Posts)
                .ToList(); // Single query with JOIN

            TestLogger.WriteLine($"Step 1: Loaded {blogsInclude.Count} blogs WITH their posts in a single query");

            foreach (var blog in blogsInclude)
            {
                // No additional queries needed - data is already loaded!
                TestLogger.WriteLine($"Blog '{blog.Title}' has {blog.Posts.Count} posts (no additional query)");
            }

            TestLogger.WriteLine("✅ Result: 1 total query - EFFICIENT!\n");

            TestLogger.WriteLine(">>> CONCLUSION:");
            TestLogger.WriteLine(">>> N+1 Problem: 4 queries (1 for blogs + 3 for posts)");
            TestLogger.WriteLine(">>> Include() Solution: 1 query (blogs JOIN posts)");
            TestLogger.WriteLine(">>> Performance improvement: 75% fewer database round trips!");
        }

        [Test]
        public void Test_Guaranteed_N_Plus_One_Problem()
        {
            TestLogger.WriteLine("=== GUARANTEED N+1 Problem Demonstration ===");
            TestLogger.WriteLine("This will definitely show the N+1 pattern with explicit queries\n");

            _context.ChangeTracker.Clear();

            TestLogger.WriteLine(">>> STEP 1: Loading all blogs (Query #1)");
            var blogs = _context.Blogs.ToList();
            TestLogger.WriteLine($"Loaded {blogs.Count} blogs\n");

            TestLogger.WriteLine(">>> STEP 2: Loading posts for each blog separately (Queries #2, #3, #4...)");
            int queryCount = 1; // Already executed 1 query for blogs

            foreach (var blog in blogs)
            {
                queryCount++;
                TestLogger.WriteLine($"Loading posts for blog '{blog.Title}' (Query #{queryCount}):");

                // This explicitly demonstrates the N+1 pattern - one query per blog
                var posts = _context.Posts.Where(p => p.BlogId == blog.Id).ToList();

                TestLogger.WriteLine($"  Found {posts.Count} posts");
                foreach (var post in posts)
                {
                    TestLogger.WriteLine($"    - {post.Title}");
                }
                //TestLogger.WriteLine();
            }

            TestLogger.WriteLine($">>> RESULT: Executed {queryCount} total queries");
            TestLogger.WriteLine($">>> PATTERN: 1 query for blogs + {blogs.Count} queries for posts = N+1 Problem!");
            TestLogger.WriteLine(">>> This is what happens with lazy loading when you access navigation properties\n");
        }

        [Test]
        public void Test_Eager_Loading_With_Include()
        {
            TestLogger.WriteLine("=== Eager Loading with Include ===");

            var blogsWithPosts = _context.Blogs
                .Include(b => b.Posts)
                .ToList();

            TestLogger.WriteLine($"Loaded {blogsWithPosts.Count} blogs with posts");

            foreach (var blog in blogsWithPosts)
            {
                TestLogger.WriteLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}");
            }
        }

        [Test]
        public void Test_Multiple_Includes_With_ThenInclude()
        {
            TestLogger.WriteLine("=== Multiple Includes with ThenInclude ===");

            var blogsWithPostsAndComments = _context.Blogs
                .Include(b => b.Posts)
                    .ThenInclude(p => p.Comments)
                .ToList();

            foreach (var blog in blogsWithPostsAndComments)
            {
                TestLogger.WriteLine($"Blog: {blog.Title}");
                foreach (var post in blog.Posts)
                {
                    TestLogger.WriteLine($"  Post: {post.Title} - Comments: {post.Comments.Count}");
                }
            }
        }

        [Test]
        public void Test_Projection_With_Select()
        {
            TestLogger.WriteLine("=== Projection with Select ===");

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
                TestLogger.WriteLine($"Blog: {blog.BlogTitle} - Total Posts: {blog.PostCount}");
                foreach (var post in blog.RecentPosts)
                {
                    TestLogger.WriteLine($"  Recent Post: {post.Title} ({post.PublishedDate:yyyy-MM-dd})");
                }
            }
        }

        [Test]
        public void Test_Split_Query()
        {
            TestLogger.WriteHeader("Split Query");

            var blogs = _context.Blogs
                .AsSplitQuery()
                .Include(b => b.Posts)
                .Include(b => b.Tags)
                .ToList();

            foreach (var blog in blogs)
            {
                TestLogger.WriteLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}, Tags: {blog.Tags.Count}");
            }
        }

        [Test]
        public void Test_Filtered_Include()
        {
            TestLogger.WriteHeader("Filtered Include (Recent Posts Only)");

            var cutoffDate = DateTime.Now.AddDays(-15);
            var blogsWithRecentPosts = _context.Blogs
                .Include(b => b.Posts.Where(p => p.PublishedDate > cutoffDate))
                .ToList();

            foreach (var blog in blogsWithRecentPosts)
            {
                TestLogger.WriteLine($"Blog: {blog.Title} - Recent Posts: {blog.Posts.Count}");
                foreach (var post in blog.Posts)
                {
                    TestLogger.WriteLine($"  Recent Post: {post.Title} ({post.PublishedDate:yyyy-MM-dd})");
                }
            }
        }

        [Test]
        public void Test_Explicit_Loading()
        {
            TestLogger.WriteHeader("Explicit Loading");

            var blogs = _context.Blogs.ToList();
            TestLogger.WriteLine($"Loaded {blogs.Count} blogs (without posts)");

            // Now explicitly load posts for all blogs in one query
            foreach (var blog in blogs)
            {
                _context.Entry(blog)
                    .Collection(b => b.Posts)
                    .Load();
            }

            foreach (var blog in blogs)
            {
                TestLogger.WriteLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}");
            }
        }

        [Test]
        public void Test_Batch_Loading_Pattern()
        {
            TestLogger.WriteHeader("Batch Loading Pattern");

            var blogs = _context.Blogs.ToList();
            var blogIds = blogs.Select(b => b.Id).ToList();

            TestLogger.WriteLine($"Loaded {blogs.Count} blogs");

            // Single query to get all posts for all blogs
            var posts = _context.Posts
                .Where(p => blogIds.Contains(p.BlogId))
                .ToList();

            TestLogger.WriteLine($"Loaded {posts.Count} posts in batch");

            // Group posts by blog in memory
            var postsByBlog = posts.GroupBy(p => p.BlogId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var blog in blogs)
            {
                var blogPosts = postsByBlog.GetValueOrDefault(blog.Id, new List<Post>());
                TestLogger.WriteLine($"Blog: {blog.Title} - Posts: {blogPosts.Count}");
            }
        }

        [Test]
        public void Test_Performance_Comparison()
        {
            TestLogger.WriteHeader("Performance Comparison");

            // Reset context to clear any cached data
            _context.ChangeTracker.Clear();

            TestLogger.WriteSubHeader("N+1 Problem (Multiple Queries)");
            var blogs1 = _context.Blogs.ToList();
            foreach (var blog in blogs1)
            {
                var count = blog.Posts.Count(); // Triggers separate query
            }

            _context.ChangeTracker.Clear();

            TestLogger.WriteSubHeader("Eager Loading (Single Query)");
            var blogs2 = _context.Blogs
                .Include(b => b.Posts)
                .ToList();
            foreach (var blog in blogs2)
            {
                var count = blog.Posts.Count; // No additional query
            }

            _context.ChangeTracker.Clear();

            TestLogger.WriteSubHeader("Projection (Minimal Data)");
            var blogSummaries = _context.Blogs
                .Select(b => new { b.Title, PostCount = b.Posts.Count() })
                .ToList();
            foreach (var summary in blogSummaries)
            {
                // All data already loaded
            }
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