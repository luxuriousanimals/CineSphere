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
using Windows.UI.Xaml.Data;
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
                obj.Size = 140;
                gi.Style = Application.Current.Resources["CineSphereGridViewItemStyle1"] as Style;
                //gi.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                //gi.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
               // gi.Template = (DataTemplate)Application.Current.Resources["IconGridDataTemplate"] as DataTemplate;
            }
            else {

                obj.Size = 140;
                gi.Style = Application.Current.Resources["CineSphereGridViewItemStyle1"] as Style;
                //gi.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                //gi.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
            }

            base.PrepareContainerForItemOverride(gi, item);
        }


    }

    public class AddSizeToHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string s)
        {
            Debug.WriteLine(value);
            if ((int)value == 420) {
                value = 500;
            } else {
                value = 200;
            }


            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string s)
        {
            throw new NotImplementedException();
        }
    }

    public class ReturnFontAdjusted : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string s)
        {
            Debug.WriteLine(value);
            if ((int)value == 420)
            {
                value = 28;
            }
            else
            {
                value = 18;
            }


            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string s)
        {
            throw new NotImplementedException();
        }
    }

    public class ReturnHeightAdjusted : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string s)
        {
            Debug.WriteLine(value);
            if ((int)value == 420)
            {
                value = 28*1.8;
            }
            else
            {
                value = 18*1.8;
            }


            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string s)
        {
            throw new NotImplementedException();
        }
    }
}
