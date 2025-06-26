using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Linq;
using XafEfCoreLoading.Module.BusinessObjects;

namespace XafEfCoreLoading.Module.Controllers
{
    /// <summary>
    /// View controller that checks for existing blogs and creates initial data if none exist
    /// </summary>
    public class DatabaseSeederViewController : ViewController
    {
        private SimpleAction seedDatabaseAction;
        
        public DatabaseSeederViewController()
        {
            // Initialize and configure the action
            seedDatabaseAction = new SimpleAction(
                this, 
                "SeedDatabase",
                "Tools")
            {
                Caption = "Seed Initial Data",
                ConfirmationMessage = "This will create initial blog data. Are you sure?",
                ImageName = "Action_Database"
            };
            
            // Set the action's execute event handler
            seedDatabaseAction.Execute += SeedDatabaseAction_Execute;
            
            // Set TargetViewType to a type of view you will use with this controller
            // Any means it will be active on any view
            TargetViewType = ViewType.Any;
        }
        
        protected override void OnActivated()
        {
            base.OnActivated();
           
        }
        
        private void SeedDatabaseAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // Run the seeding process manually when action is executed
            SeedInitialData();
        }
        
   
        
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
                // Show error message if seeding fails
               
            }
        }
    }
    
   
}