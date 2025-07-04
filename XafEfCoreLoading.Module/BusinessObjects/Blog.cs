using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace XafEfCoreLoading.Module.BusinessObjects
{
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<EntityObject1> EntityObject1s { get; set; }" syntax.
    [DefaultClassOptions]
    // Entity Classes
    public class Blog
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; } = string.Empty;
        public virtual  string Description { get; set; } = string.Empty;
        public virtual DateTime CreatedDate { get; set; }

        // VIRTUAL is required for lazy loading proxies
        public virtual ObservableCollection<Post> Posts { get; set; } = new ObservableCollection<Post>();
        public virtual ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
    }
}