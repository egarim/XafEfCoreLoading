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
        public virtual int Id { get; set; }
        public virtual string Author { get; set; } = string.Empty;
        public virtual string Content { get; set; } = string.Empty;
        public virtual DateTime CreatedDate { get; set; }
        public virtual int PostId { get; set; }

        // VIRTUAL is required for lazy loading proxies
        public virtual Post Post { get; set; } = null!;
    }
}