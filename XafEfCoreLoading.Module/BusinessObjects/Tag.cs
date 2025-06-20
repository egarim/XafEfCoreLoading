using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System;
using System.Linq;

namespace XafEfCoreLoading.Module.BusinessObjects
{
    [DefaultClassOptions]
    // Entity Classes
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // VIRTUAL is required for lazy loading proxies
        public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}