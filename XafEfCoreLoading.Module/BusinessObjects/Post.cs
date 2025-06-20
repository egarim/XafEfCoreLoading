using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace XafEfCoreLoading.Module.BusinessObjects
{
    [DefaultClassOptions]
    // Entity Classes
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int BlogId { get; set; }

        public Blog Blog { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new();
    }
}