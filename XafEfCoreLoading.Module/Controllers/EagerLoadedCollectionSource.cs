using DevExpress.ExpressApp;
using System;
using System.Linq;
using XafEfCoreLoading.Module.BusinessObjects;

namespace XafEfCoreLoading.Module.Controllers
{
    /// <summary>
    /// Custom collection source that uses pre-loaded data with eager loading to prevent N+1 queries
    /// </summary>
    public class EagerLoadedCollectionSource : CollectionSource
    {
        private readonly List<Blog> _preLoadedData;

        public EagerLoadedCollectionSource(IObjectSpace objectSpace, Type objectType, List<Blog> preLoadedData)
            : base(objectSpace, objectType)
        {
            _preLoadedData = preLoadedData;
        }

        /// <summary>
        /// Override to provide custom data access that uses our pre-loaded entities
        /// </summary>
        protected override object CreateCollection()
        {
            // Return our pre-loaded data as the collection
            // XAF will work with this data directly, avoiding database queries
            return _preLoadedData.AsQueryable();
        }
    }
}
