using CineSphere.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CineSphere.Common
{

    class GridViewOverride : GridView
    {
     
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var obj = item as VideoViewModel;
            var gi = element as GridViewItem;

            
            if (obj.isMRU)
            {
                obj.Size = 380;
                //gi.Style = this.Resources["CineSphereGridViewItemStyle1"] as Style;
            }
            else {

                obj.Size = 140;
            }

            base.PrepareContainerForItemOverride(gi, item);
        }


    }
}
