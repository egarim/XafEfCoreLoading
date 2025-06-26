using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using Microsoft.Extensions.Logging;

namespace XafEfCoreLoading.Module.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891/core-prerequisites-for-design-time-model-editor-with-entity-framework-core-data-model.
public class XafEfCoreLoadingContextInitializer : DbContextTypesInfoInitializerBase {
    protected override DbContext CreateDbContext() {
        var optionsBuilder = new DbContextOptionsBuilder<XafEfCoreLoadingEFCoreDbContext>()
            .UseSqlServer(";")//.UseSqlite(";") wrong for a solution with SqLite, see https://isc.devexpress.com/internal/ticket/details/t1240173
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new XafEfCoreLoadingEFCoreDbContext(optionsBuilder.Options);
    }
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class XafEfCoreLoadingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<XafEfCoreLoadingEFCoreDbContext> {
    public XafEfCoreLoadingEFCoreDbContext CreateDbContext(string[] args) {
        throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
        //var optionsBuilder = new DbContextOptionsBuilder<XafEfCoreLoadingEFCoreDbContext>();
        //optionsBuilder.UseSqlServer("Integrated Security=SSPI;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=XafEfCoreLoading");
        //optionsBuilder.UseChangeTrackingProxies();
        //optionsBuilder.UseObjectSpaceLinkProxies();
        //return new XafEfCoreLoadingEFCoreDbContext(optionsBuilder.Options);
    }
}
[TypesInfoInitializer(typeof(XafEfCoreLoadingContextInitializer))]
public class XafEfCoreLoadingEFCoreDbContext : BlogContext {
    public XafEfCoreLoadingEFCoreDbContext(DbContextOptions<XafEfCoreLoadingEFCoreDbContext> options) : base(options) 
    {
    }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure logging to show SQL queries
        optionsBuilder
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

 

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseDeferredDeletion(this);
        modelBuilder.UseOptimisticLock();
        modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

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
        //SeedData(modelBuilder);
    }
}
