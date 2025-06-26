using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XafEfCoreLoading.Module.BusinessObjects
{
    [DefaultClassOptions]
    // Entity Classes
    public class Post
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; } = string.Empty;
        public virtual string Content { get; set; } = string.Empty;
        public virtual DateTime PublishedDate { get; set; }
        public virtual int BlogId { get; set; }

        // VIRTUAL is required for lazy loading proxies
        public virtual Blog Blog { get; set; } = null!;
        // VIRTUAL is required for lazy loading proxies and ICollection instead of List
        public virtual ICollection<Comment> Comments { get; set; } = new ObservableCollection<Comment>();
    }
}