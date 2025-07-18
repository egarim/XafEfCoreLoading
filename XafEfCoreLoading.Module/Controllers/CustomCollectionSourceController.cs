using DevExpress.ExpressApp;
using XafEfCoreLoading.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using DevExpress.Data.Filtering;

namespace XafEfCoreLoading.Module.Controllers
{
    public class CustomCollectionSourceController : ViewController
    {
        private static List<Blog> _cachedBlogsWithRelatedData;

        public CustomCollectionSourceController() : base()
        {
            // Target required Views (use the TargetXXX properties) and create their Actions.
            this.TargetObjectType = typeof(Blog);
            this.TargetViewType = ViewType.ListView; 
        }
        protected override void OnActivated()
        {
            base.OnActivated();
           
            // Perform various tasks depending on the target View.
        }



        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
    }
}
