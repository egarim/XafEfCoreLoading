using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System;
using System.Linq;

namespace XafEfCoreLoading.Module.BusinessObjects
{
    [DefaultClassOptions]
    // Entity Classes
    public class Comment
    {
        public int Id { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int PostId { get; set; }

        // VIRTUAL is required for lazy loading proxies
        public virtual Post Post { get; set; } = null!;
    }
}