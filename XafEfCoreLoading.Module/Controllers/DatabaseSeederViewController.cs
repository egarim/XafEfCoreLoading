using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.SystemModule;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using XafEfCoreLoading.Module.BusinessObjects;

namespace XafEfCoreLoading.Module.Controllers
{
    /// <summary>
    /// View controller that provides actions to demonstrate EF Core loading patterns
    /// </summary>
    public class DatabaseSeederViewController : ViewController
    {
        private readonly StringBuilder _logOutput = new StringBuilder();

        // Define all actions
        public SimpleAction SeedDatabaseAction { get; private set; }
        public SimpleAction NPlusOneProblemAction { get; private set; }
        public SimpleAction NPlusOneProblemAlternativeAction { get; private set; }
        public SimpleAction NPlusOneVsIncludeAction { get; private set; }
        public SimpleAction GuaranteedNPlusOneAction { get; private set; }
        public SimpleAction EagerLoadingAction { get; private set; }
        public SimpleAction MultipleIncludesAction { get; private set; }
        public SimpleAction ProjectionAction { get; private set; }
        public SimpleAction SplitQueryAction { get; private set; }
        public SimpleAction FilteredIncludeAction { get; private set; }
        public SimpleAction ExplicitLoadingAction { get; private set; }
        public SimpleAction BatchLoadingAction { get; private set; }
        public SimpleAction PerformanceComparisonAction { get; private set; }
        
        public DatabaseSeederViewController()
        {
            TargetViewType = ViewType.Any;

            // Initialize all actions
            InitializeActions();
        }

        private void InitializeActions()
        {
            // Seed Database Action
            SeedDatabaseAction = new SimpleAction(this, "SeedDatabase", "View")
            {
                Caption = "Seed Initial Data",
                ConfirmationMessage = "This will create initial blog data. Are you sure?",
               
            };
            SeedDatabaseAction.Execute += SeedDatabaseAction_Execute;

            // N+1 Problem with Lazy Loading
            NPlusOneProblemAction = new SimpleAction(this, "NPlusOneProblem", "View")
            {
                Caption = "N+1 Problem (Lazy Loading)",
                ToolTip = "Demonstrates the N+1 query problem using lazy loading",
              
            };
            NPlusOneProblemAction.Execute += NPlusOneProblemAction_Execute;

            // N+1 Problem Alternative Approach
            NPlusOneProblemAlternativeAction = new SimpleAction(this, "NPlusOneProblemAlternative", "View")
            {
                Caption = "N+1 Problem (Alternative)",
                ToolTip = "Demonstrates the N+1 problem using explicit loading",
               
            };
            NPlusOneProblemAlternativeAction.Execute += NPlusOneProblemAlternativeAction_Execute;

            // N+1 vs Include Comparison
            NPlusOneVsIncludeAction = new SimpleAction(this, "NPlusOneVsInclude", "View")
            {
                Caption = "N+1 vs Include Comparison",
                ToolTip = "Compares N+1 queries with Include solution",
              
            };
            NPlusOneVsIncludeAction.Execute += NPlusOneVsIncludeAction_Execute;

            // Guaranteed N+1 Problem
            GuaranteedNPlusOneAction = new SimpleAction(this, "GuaranteedNPlusOne", "View")
            {
                Caption = "Guaranteed N+1 Problem",
                ToolTip = "Demonstrates a guaranteed N+1 query problem",
                
            };
            GuaranteedNPlusOneAction.Execute += GuaranteedNPlusOneAction_Execute;

            // Eager Loading with Include
            EagerLoadingAction = new SimpleAction(this, "EagerLoading", "View")
            {
                Caption = "Eager Loading (Include)",
                ToolTip = "Demonstrates eager loading with Include",
                
            };
            EagerLoadingAction.Execute += EagerLoadingAction_Execute;

            // Multiple Includes with ThenInclude
            MultipleIncludesAction = new SimpleAction(this, "MultipleIncludes", "View")
            {
                Caption = "Multiple Includes & ThenInclude",
                ToolTip = "Demonstrates multiple includes with ThenInclude",
               
            };
            MultipleIncludesAction.Execute += MultipleIncludesAction_Execute;

            // Projection with Select
            ProjectionAction = new SimpleAction(this, "Projection", "View")
            {
                Caption = "Projection (Select)",
                ToolTip = "Demonstrates projection with Select",
               
            };
            ProjectionAction.Execute += ProjectionAction_Execute;

            // Split Query
            SplitQueryAction = new SimpleAction(this, "SplitQuery", "View")
            {
                Caption = "Split Query",
                ToolTip = "Demonstrates split query loading",
                
            };
            SplitQueryAction.Execute += SplitQueryAction_Execute;

            // Filtered Include
            FilteredIncludeAction = new SimpleAction(this, "FilteredInclude", "View")
            {
                Caption = "Filtered Include",
                ToolTip = "Demonstrates filtered include loading",
              
            };
            FilteredIncludeAction.Execute += FilteredIncludeAction_Execute;

            // Explicit Loading
            ExplicitLoadingAction = new SimpleAction(this, "ExplicitLoading", "View")
            {
                Caption = "Explicit Loading",
                ToolTip = "Demonstrates explicit loading of related entities",
                
            };
            ExplicitLoadingAction.Execute += ExplicitLoadingAction_Execute;

            // Batch Loading Pattern
            BatchLoadingAction = new SimpleAction(this, "BatchLoading", "View")
            {
                Caption = "Batch Loading Pattern",
                ToolTip = "Demonstrates batch loading pattern",
                
            };
            BatchLoadingAction.Execute += BatchLoadingAction_Execute;

            // Performance Comparison
            PerformanceComparisonAction = new SimpleAction(this, "PerformanceComparison", "View")
            {
                Caption = "Performance Comparison",
                ToolTip = "Compares performance of different loading strategies",
               
            };
            PerformanceComparisonAction.Execute += PerformanceComparisonAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
           
        }

        #region Action Event Handlers

        private void SeedDatabaseAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            SeedInitialData();
            ShowMessage("Database seeded successfully", InformationType.Success);
        }

        private void NPlusOneProblemAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
               
                LogLine("Demonstrating N+1 Problem with Lazy Loading");
                LogLine("Each access to Posts collection will trigger a separate SQL query!");

                // Clear change tracker to ensure fresh entities
                context.ChangeTracker.Clear();

                // This will cause N+1 queries due to lazy loading
                var blogs = context.Blogs.ToList(); // Query 1: Load blogs
                LogLine($"Loaded {blogs.Count} blogs with initial query");

                // Check if entities are proxies (they should be for lazy loading to work)
                foreach (var blog in blogs)
                {
                    var entityType = blog.GetType();
                    LogLine($"Blog entity type: {entityType.Name} (IsProxy: {entityType.BaseType != null})");
                }

                LogLine("\nNow accessing Posts for each blog (this triggers lazy loading):");

                foreach (var blog in blogs)
                {
                    LogLine($"\nBlog: {blog.Title}");
                    // Each access to Posts triggers a separate query due to lazy loading!
                    var postCount = blog.Posts.Count; // This should trigger a query
                    LogLine($"  Post count: {postCount}");

                    // If lazy loading didn't work, manually load to show the difference
                    if (postCount == 0)
                    {
                        LogLine("  Lazy loading didn't trigger - manually loading posts...");
                        context.Entry(blog).Collection(b => b.Posts).Load();
                        LogLine($"  Post count after manual load: {blog.Posts.Count}");
                    }
                }

                LogLine("\n>>> This should result in 1 + N queries (N+1 problem)!");
                LogLine(">>> 1 query for blogs + 1 query per blog for posts = 4 total queries");
            });
        }

        private void NPlusOneProblemAlternativeAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Alternative N+1 Demonstration (Explicit Loading) ===");
                LogLine("This manually demonstrates the N+1 pattern even if lazy loading isn't working");

                context.ChangeTracker.Clear();

                // Query 1: Load blogs only
                var blogs = context.Blogs.ToList();
                LogLine($"Query 1: Loaded {blogs.Count} blogs");

                // Now manually trigger one query per blog (simulating lazy loading)
                foreach (var blog in blogs)
                {
                    LogLine($"\nBlog: {blog.Title}");

                    // This explicitly loads posts for THIS blog only (simulates lazy loading)
                    var posts = context.Posts.Where(p => p.BlogId == blog.Id).ToList();
                    LogLine($"Query {blog.Id + 1}: Loaded {posts.Count} posts for blog {blog.Id}");

                    foreach (var post in posts.Take(1))
                    {
                        LogLine($"  Sample post: {post.Title}");
                    }
                }

                LogLine($"\n>>> Total queries executed: {blogs.Count + 1} (1 for blogs + {blogs.Count} for posts)");
                LogLine(">>> This is the N+1 problem!");
            });
        }

        private void NPlusOneVsIncludeAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== SIDE-BY-SIDE: N+1 Problem vs Include() Solution ===\n");

                // === PART 1: Demonstrate N+1 Problem ===
                LogLine("🔴 BAD APPROACH: N+1 Problem");
                LogLine("════════════════════════════════════════");
                context.ChangeTracker.Clear();

                var blogsN1 = context.Blogs.ToList(); // Query 1
                LogLine($"Step 1: Loaded {blogsN1.Count} blogs");

                foreach (var blog in blogsN1)
                {
                    // Each of these causes a separate query - the N+1 problem!
                    var posts = context.Posts.Where(p => p.BlogId == blog.Id).ToList();
                    LogLine($"Step {blog.Id + 1}: Blog '{blog.Title}' has {posts.Count} posts");
                }

                LogLine("🔴 Result: 4 total queries (1 + 3) - INEFFICIENT!\n");

                // === PART 2: Show Include() Solution ===
                LogLine("✅ GOOD APPROACH: Include() Solution");
                LogLine("════════════════════════════════════════");
                context.ChangeTracker.Clear();

                var blogsInclude = context.Blogs
                    .Include(b => b.Posts)
                    .ToList(); // Single query with JOIN

                LogLine($"Step 1: Loaded {blogsInclude.Count} blogs WITH their posts in a single query");

                foreach (var blog in blogsInclude)
                {
                    // No additional queries needed - data is already loaded!
                    LogLine($"Blog '{blog.Title}' has {blog.Posts.Count} posts (no additional query)");
                }

                LogLine("✅ Result: 1 total query - EFFICIENT!\n");

                LogLine(">>> CONCLUSION:");
                LogLine(">>> N+1 Problem: 4 queries (1 for blogs + 3 for posts)");
                LogLine(">>> Include() Solution: 1 query (blogs JOIN posts)");
                LogLine(">>> Performance improvement: 75% fewer database round trips!");
            });
        }

        private void GuaranteedNPlusOneAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== GUARANTEED N+1 Problem Demonstration ===");
                LogLine("This will definitely show the N+1 pattern with explicit queries\n");

                context.ChangeTracker.Clear();

                LogLine(">>> STEP 1: Loading all blogs (Query #1)");
                var blogs = context.Blogs.ToList();
                LogLine($"Loaded {blogs.Count} blogs\n");

                LogLine(">>> STEP 2: Loading posts for each blog separately (Queries #2, #3, #4...)");
                int queryCount = 1; // Already executed 1 query for blogs

                foreach (var blog in blogs)
                {
                    queryCount++;
                    LogLine($"Loading posts for blog '{blog.Title}' (Query #{queryCount}):");

                    // This explicitly demonstrates the N+1 pattern - one query per blog
                    var posts = context.Posts.Where(p => p.BlogId == blog.Id).ToList();

                    LogLine($"  Found {posts.Count} posts");
                    foreach (var post in posts)
                    {
                        LogLine($"    - {post.Title}");
                    }
                }

                LogLine($">>> RESULT: Executed {queryCount} total queries");
                LogLine($">>> PATTERN: 1 query for blogs + {blogs.Count} queries for posts = N+1 Problem!");
                LogLine(">>> This is what happens with lazy loading when you access navigation properties\n");
            });
        }

        private void EagerLoadingAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Eager Loading with Include ===");

                var blogsWithPosts = context.Blogs
                    .Include(b => b.Posts)
                    .ToList();

                LogLine($"Loaded {blogsWithPosts.Count} blogs with posts");

                foreach (var blog in blogsWithPosts)
                {
                    LogLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}");
                }
            });
        }

        private void MultipleIncludesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Multiple Includes with ThenInclude ===");

                var blogsWithPostsAndComments = context.Blogs
                    .Include(b => b.Posts)
                        .ThenInclude(p => p.Comments)
                    .ToList();

                foreach (var blog in blogsWithPostsAndComments)
                {
                    LogLine($"Blog: {blog.Title}");
                    foreach (var post in blog.Posts)
                    {
                        LogLine($"  Post: {post.Title} - Comments: {post.Comments.Count}");
                    }
                }
            });
        }

        private void ProjectionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Projection with Select ===");

                var blogData = context.Blogs
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
                    LogLine($"Blog: {blog.BlogTitle} - Total Posts: {blog.PostCount}");
                    foreach (var post in blog.RecentPosts)
                    {
                        LogLine($"  Recent Post: {post.Title} ({post.PublishedDate:yyyy-MM-dd})");
                    }
                }
            });
        }

        private void SplitQueryAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Split Query ===");
                LogLine("=========================================");

                var blogs = context.Blogs
                    .AsSplitQuery()
                    .Include(b => b.Posts)
                    .Include(b => b.Tags)
                    .ToList();

                foreach (var blog in blogs)
                {
                    LogLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}, Tags: {blog.Tags.Count}");
                }
            });
        }

        private void FilteredIncludeAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Filtered Include (Recent Posts Only) ===");
                LogLine("============================================");

                var cutoffDate = DateTime.Now.AddDays(-15);
                var blogsWithRecentPosts = context.Blogs
                    .Include(b => b.Posts.Where(p => p.PublishedDate > cutoffDate))
                    .ToList();

                foreach (var blog in blogsWithRecentPosts)
                {
                    LogLine($"Blog: {blog.Title} - Recent Posts: {blog.Posts.Count}");
                    foreach (var post in blog.Posts)
                    {
                        LogLine($"  Recent Post: {post.Title} ({post.PublishedDate:yyyy-MM-dd})");
                    }
                }
            });
        }

        private void ExplicitLoadingAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Explicit Loading ===");
                LogLine("========================");

                var blogs = context.Blogs.ToList();
                LogLine($"Loaded {blogs.Count} blogs (without posts)");

                // Now explicitly load posts for all blogs in one query
                foreach (var blog in blogs)
                {
                    context.Entry(blog)
                        .Collection(b => b.Posts)
                        .Load();
                }

                foreach (var blog in blogs)
                {
                    LogLine($"Blog: {blog.Title} - Posts: {blog.Posts.Count}");
                }
            });
        }

        private void BatchLoadingAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Batch Loading Pattern ===");
                LogLine("============================");

                var blogs = context.Blogs.ToList();
                var blogIds = blogs.Select(b => b.Id).ToList();

                LogLine($"Loaded {blogs.Count} blogs");

                // Single query to get all posts for all blogs
                var posts = context.Posts
                    .Where(p => blogIds.Contains(p.BlogId))
                    .ToList();

                LogLine($"Loaded {posts.Count} posts in batch");

                // Group posts by blog in memory
                var postsByBlog = posts.GroupBy(p => p.BlogId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var blog in blogs)
                {
                    var blogPosts = postsByBlog.GetValueOrDefault(blog.Id, new List<Post>());
                    LogLine($"Blog: {blog.Title} - Posts: {blogPosts.Count}");
                }
            });
        }

        private void PerformanceComparisonAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            Console.Clear();
            ExecuteWithLogging(() => {
                var context = GetDbContext();
                
                LogLine("=== Performance Comparison ===");
                LogLine("=============================");

                // Reset context to clear any cached data
                context.ChangeTracker.Clear();

                LogLine("\n--- N+1 Problem (Multiple Queries) ---");
                var blogs1 = context.Blogs.ToList();
                foreach (var blog in blogs1)
                {
                    var count = blog.Posts.Count(); // Triggers separate query
                }

                context.ChangeTracker.Clear();

                LogLine("\n--- Eager Loading (Single Query) ---");
                var blogs2 = context.Blogs
                    .Include(b => b.Posts)
                    .ToList();
                foreach (var blog in blogs2)
                {
                    var count = blog.Posts.Count; // No additional query
                }

                context.ChangeTracker.Clear();

                LogLine("\n--- Projection (Minimal Data) ---");
                var blogSummaries = context.Blogs
                    .Select(b => new { b.Title, PostCount = b.Posts.Count() })
                    .ToList();
                foreach (var summary in blogSummaries)
                {
                    // All data already loaded
                }
            });
        }

        #endregion

        #region Helper Methods

        private void SeedInitialData()
        {
            try
            {
                // Create blogs
                var techBlog = ObjectSpace.CreateObject<Blog>();
                techBlog.Title = "Tech Blog";
                techBlog.Description = "Technology articles";
                techBlog.CreatedDate = DateTime.Now.AddDays(-100);
                
                var cookingBlog = ObjectSpace.CreateObject<Blog>();
                cookingBlog.Title = "Cooking Blog";
                cookingBlog.Description = "Delicious recipes";
                cookingBlog.CreatedDate = DateTime.Now.AddDays(-80);
                
                var travelBlog = ObjectSpace.CreateObject<Blog>();
                travelBlog.Title = "Travel Blog";
                travelBlog.Description = "Adventure stories";
                travelBlog.CreatedDate = DateTime.Now.AddDays(-60);
                
                // Create tags
                var programmingTag = ObjectSpace.CreateObject<Tag>();
                programmingTag.Name = "Programming";
                
                var csharpTag = ObjectSpace.CreateObject<Tag>();
                csharpTag.Name = "C#";
                
                var foodTag = ObjectSpace.CreateObject<Tag>();
                foodTag.Name = "Food";
                
                var travelTag = ObjectSpace.CreateObject<Tag>();
                travelTag.Name = "Travel";
                
                // Create posts for Tech Blog
                var efCoreBasicsPost = ObjectSpace.CreateObject<Post>();
                efCoreBasicsPost.Title = "EF Core Basics";
                efCoreBasicsPost.Content = "Introduction to EF Core";
                efCoreBasicsPost.PublishedDate = DateTime.Now.AddDays(-10);
                efCoreBasicsPost.Blog = techBlog;
                
                var advancedEfPost = ObjectSpace.CreateObject<Post>();
                advancedEfPost.Title = "Advanced EF Core";
                advancedEfPost.Content = "Advanced EF Core techniques";
                advancedEfPost.PublishedDate = DateTime.Now.AddDays(-5);
                advancedEfPost.Blog = techBlog;
                
                // Create posts for Cooking Blog
                var chocolateCakePost = ObjectSpace.CreateObject<Post>();
                chocolateCakePost.Title = "Chocolate Cake Recipe";
                chocolateCakePost.Content = "How to make chocolate cake";
                chocolateCakePost.PublishedDate = DateTime.Now.AddDays(-15);
                chocolateCakePost.Blog = cookingBlog;
                
                var pastaPost = ObjectSpace.CreateObject<Post>();
                pastaPost.Title = "Pasta Recipe";
                pastaPost.Content = "Italian pasta recipe";
                pastaPost.PublishedDate = DateTime.Now.AddDays(-8);
                pastaPost.Blog = cookingBlog;
                
                // Create posts for Travel Blog
                var parisPost = ObjectSpace.CreateObject<Post>();
                parisPost.Title = "Paris Trip";
                parisPost.Content = "My trip to Paris";
                parisPost.PublishedDate = DateTime.Now.AddDays(-20);
                parisPost.Blog = travelBlog;
                
                var tokyoPost = ObjectSpace.CreateObject<Post>();
                tokyoPost.Title = "Tokyo Adventure";
                tokyoPost.Content = "Adventures in Tokyo";
                tokyoPost.PublishedDate = DateTime.Now.AddDays(-12);
                tokyoPost.Blog = travelBlog;
                
                // Create comments for posts
                var comment1 = ObjectSpace.CreateObject<Comment>();
                comment1.Author = "John";
                comment1.Content = "Great article!";
                comment1.CreatedDate = DateTime.Now.AddDays(-9);
                comment1.Post = efCoreBasicsPost;
                
                var comment2 = ObjectSpace.CreateObject<Comment>();
                comment2.Author = "Jane";
                comment2.Content = "Very helpful";
                comment2.CreatedDate = DateTime.Now.AddDays(-8);
                comment2.Post = efCoreBasicsPost;
                
                var comment3 = ObjectSpace.CreateObject<Comment>();
                comment3.Author = "Bob";
                comment3.Content = "Thanks for sharing";
                comment3.CreatedDate = DateTime.Now.AddDays(-4);
                comment3.Post = advancedEfPost;
                
                var comment4 = ObjectSpace.CreateObject<Comment>();
                comment4.Author = "Alice";
                comment4.Content = "Delicious!";
                comment4.CreatedDate = DateTime.Now.AddDays(-14);
                comment4.Post = chocolateCakePost;
                
                var comment5 = ObjectSpace.CreateObject<Comment>();
                comment5.Author = "Charlie";
                comment5.Content = "I tried this recipe";
                comment5.CreatedDate = DateTime.Now.AddDays(-7);
                comment5.Post = pastaPost;
                
                var comment6 = ObjectSpace.CreateObject<Comment>();
                comment6.Author = "Diana";
                comment6.Content = "Amazing photos!";
                comment6.CreatedDate = DateTime.Now.AddDays(-19);
                comment6.Post = parisPost;
                
                // Associate blogs with tags
                techBlog.Tags.Add(programmingTag);
                techBlog.Tags.Add(csharpTag);
                cookingBlog.Tags.Add(foodTag);
                travelBlog.Tags.Add(travelTag);
                
                // Save all changes
                ObjectSpace.CommitChanges();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to seed database: {ex.Message}", InformationType.Error);
            }
        }

        private BlogContext GetDbContext()
        {
            // Cast the ObjectSpace to EFCoreObjectSpace to get the DbContext
            if (ObjectSpace is EFCoreObjectSpace efObjectSpace)
            {
                return efObjectSpace.DbContext as BlogContext;
            }
            
            throw new InvalidOperationException("Could not get EF Core DbContext from ObjectSpace");
        }

        private void ExecuteWithLogging(Action action)
        {
            try
            {
                _logOutput.Clear();
                
                // Execute the action
                action();
                
                // Show the log output in a popup
                ShowMessage(_logOutput.ToString(), InformationType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"An error occurred: {ex.Message}", InformationType.Error);
            }
        }

        private void LogLine(string value)
        {
            _logOutput.AppendLine(value);
            Console.WriteLine(value);

            TestLogger.WriteLine(value);
        }

        private void ShowMessage(string message, InformationType informationType)
        {
            // Use the Application.ShowViewStrategy to display a message
            Application.ShowViewStrategy.ShowMessage(message, informationType, 10000, InformationPosition.Bottom);
        }

        #endregion
    }
}