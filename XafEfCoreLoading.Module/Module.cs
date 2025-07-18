using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using XafEfCoreLoading.Module.BusinessObjects;
using XafEfCoreLoading.Module.Controllers;
using System.Diagnostics;

namespace XafEfCoreLoading.Module;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
public sealed class XafEfCoreLoadingModule : ModuleBase {
    public XafEfCoreLoadingModule() {
        //
        // XafEfCoreLoadingModule
        //
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.CloneObject.CloneObjectModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
        
        // Register controllers (required for XAF controllers)
        this.AdditionalExportedTypes.Add(typeof(DatabaseSeederViewController));
    }
    
    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
        ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
        return new ModuleUpdater[] { updater };
    }
    
    public override void Setup(XafApplication application) {
        base.Setup(application);
        // Manage various aspects of the application UI and behavior at the module level.
        
        // Register for application events to ensure database seeding happens at the right time
        //application.SetupComplete += Application_SetupComplete;
        this.Application.CreateCustomCollectionSource += Application_CreateCustomCollectionSource;
    }
    private void Application_CreateCustomCollectionSource(object sender, CreateCustomCollectionSourceEventArgs e)
    {
        Console.Clear();
        // Create a custom collection source specifically for the Blog type
        if (e.ObjectType == typeof(Blog))
        {
            var _context = (e.ObjectSpace as EFCoreObjectSpace).DbContext as BlogContext;

            // Use the QueryDebugHelper for enhanced logging
            var blogsWithPostsAndComments = QueryDebugHelper.ExecuteWithDebugLogging(
                "Eager Loading Blogs with Posts and Comments", 
                () => {
                    var query = _context.Blogs
                                    .Include(b => b.Posts)
                                        .ThenInclude(p => p.Comments)
                                    .Include(b => b.Tags);

                    // Log the SQL that will be generated
                    QueryDebugHelper.LogQuerySql(query, "Blogs Eager Loading Query");

                    // Execute the query
                    return query.ToList();
                });

            Debug.WriteLine($"📊 Loaded {blogsWithPostsAndComments.Count} blogs with related data");
            Console.WriteLine($"📊 Loaded {blogsWithPostsAndComments.Count} blogs with related data");
            TestLogger.WriteLine($"📊 Loaded {blogsWithPostsAndComments.Count} blogs with related data");

            // Option 1: Use the pre-loaded data directly with a custom collection source
            e.CollectionSource = new EagerLoadedCollectionSource(e.ObjectSpace, e.ObjectType, blogsWithPostsAndComments);

            // Option 2: Alternatively, use standard CollectionSource with pre-loaded context
            // AttachPreLoadedDataToObjectSpace(e.ObjectSpace, blogsWithPostsAndComments);
            // e.CollectionSource = new CollectionSource(e.ObjectSpace, e.ObjectType);
        }
    }

    private void AttachPreLoadedDataToObjectSpace(IObjectSpace objectSpace, List<Blog> preLoadedBlogs)
    {
        // For EF Core ObjectSpace, we need to ensure the entities are tracked
        if (objectSpace is EFCoreObjectSpace efObjectSpace)
        {
            var context = efObjectSpace.DbContext;

            // Attach all the pre-loaded entities to the current context if they're not already tracked
            foreach (var blog in preLoadedBlogs)
            {
                var trackedBlog = context.Entry(blog);
                if (trackedBlog.State == EntityState.Detached)
                {
                    context.Attach(blog);

                    // Also attach related entities
                    foreach (var post in blog.Posts)
                    {
                        var trackedPost = context.Entry(post);
                        if (trackedPost.State == EntityState.Detached)
                        {
                            context.Attach(post);

                            // Attach comments
                            foreach (var comment in post.Comments)
                            {
                                var trackedComment = context.Entry(comment);
                                if (trackedComment.State == EntityState.Detached)
                                {
                                    context.Attach(comment);
                                }
                            }
                        }
                    }

                    // Attach tags
                    foreach (var tag in blog.Tags)
                    {
                        var trackedTag = context.Entry(tag);
                        if (trackedTag.State == EntityState.Detached)
                        {
                            context.Attach(tag);
                        }
                    }
                }
            }
        }
    }
    private void Application_SetupComplete(object sender, EventArgs e)
    {
        // This ensures our seeder runs after application setup is complete
        // Useful for making sure database connections are ready
        Application.ObjectSpaceCreated += Application_ObjectSpaceCreated;
    }

    private void Application_ObjectSpaceCreated(object sender, ObjectSpaceCreatedEventArgs e)
    {
        // Only need to do this once
        Application.ObjectSpaceCreated -= Application_ObjectSpaceCreated;
        
        // Will ensure the DatabaseSeederViewController gets activated
        var controller = Application.CreateController<DatabaseSeederViewController>();
        controller.Active["AutoInitialize"] = true;
    }
}
