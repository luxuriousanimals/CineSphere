using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CineSphere.Common
{
    public class MineColorHelper : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private SolidColorBrush _strokeColorA;
        public SolidColorBrush StrokeColorA
        {
            get { return _strokeColorA; }
            set
            {
                _strokeColorA = value;
                NotifyPropertyChanged("StrokeColorA");

            }
        }

        private SolidColorBrush _strokeColorB;
        public SolidColorBrush StrokeColorB
        {
            get { return _strokeColorB; }
            set
            {
                _strokeColorB = value;
                NotifyPropertyChanged("StrokeColorB");

            }
        }


        private SolidColorBrush _strokeColorC;
        public SolidColorBrush StrokeColorC
        {
            get { return _strokeColorC; }
            set
            {
                _strokeColorC = value;
                NotifyPropertyChanged("StrokeColorC");

            }
        }

        //private SolidColorBrush _strokeColorD;
        //public SolidColorBrush StrokeColorD
        //{
        //    get { return _strokeColorD; }
        //    set
        //    {
        //        _strokeColorD = value;
        //        NotifyPropertyChanged("StrokeColorD");

        //    }
        //}

        private SolidColorBrush _ButtonBackGroundFillColor;
        public SolidColorBrush ButtonBackGroundFillColor
        {
            get { return _ButtonBackGroundFillColor; }
            set
            {
                _ButtonBackGroundFillColor = value;
                NotifyPropertyChanged("ButtonBackGroundFillColor");

            }
        }

        private SolidColorBrush _ButtonFillColor;
        public SolidColorBrush ButtonFillColor
        {
            get { return _ButtonFillColor; }
            set
            {
                _ButtonFillColor = value;
                NotifyPropertyChanged("ButtonFillColor");

            }
        }


        private SolidColorBrush _HolderFillColor;
        public SolidColorBrush HolderFillColor
        {
            get { return _HolderFillColor; }
            set
            {
                _HolderFillColor = value;
                NotifyPropertyChanged("HolderFillColor");

            }
        }

        private SolidColorBrush _ProgressFrameFillColor;
        public SolidColorBrush ProgressFrameFillColor
        {
            get { return _ProgressFrameFillColor; }
            set
            {
                _ProgressFrameFillColor = value;
                NotifyPropertyChanged("ProgressFrameFillColor");

            }
        }

        private SolidColorBrush _VolumeSliderFillColor;
        public SolidColorBrush VolumeSliderFillColor
        {
            get { return _VolumeSliderFillColor; }
            set
            {
                _VolumeSliderFillColor = value;
                NotifyPropertyChanged("VolumeSliderFillColor");

            }
        }



        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}


  sealed class Win32
  {
      [DllImport("user32.dll")]
      static extern IntPtr GetDC(IntPtr hwnd);

      [DllImport("user32.dll")]
      static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

      [DllImport("gdi32.dll")]
      static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

      static public Windows.UI.Color GetPixelColor(int x, int y)
      {
       IntPtr hdc = GetDC(IntPtr.Zero);
       uint pixel = GetPixel(hdc, x, y);
       ReleaseDC(IntPtr.Zero, hdc);
       
       Color color = Color.FromArgb(255,(byte)(pixel & 0x000000FF),
                    (byte)((pixel & 0x0000FF00) >> 8),
                    (byte)((pixel & 0x00FF0000) >> 16));
       return color;
      }
   }

 public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string s)
        {
            SolidColorBrush b = value as SolidColorBrush;
            if (b != null)
            {
                return b.Color;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string s)
        {
            throw new NotImplementedException();
        }
    }


   
      //public string HSL2RGB(double h, double sl, double l)
      //{
      //      double v;
      //      double r,g,b;
 
      //      r = l;   // default to gray
      //      g = l;
      //      b = l;
      //      v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
      //      if (v > 0)
      //      {
      //            double m;
      //            double sv;
      //            int sextant;
      //            double fract, vsf, mid1, mid2;
 
      //            m = l + l - v;
      //            sv = (v - m ) / v;
      //            h *= 6.0;
      //            sextant = (int)h;
      //            fract = h - sextant;
      //            vsf = v * sv * fract;
      //            mid1 = m + vsf;
      //            mid2 = v - vsf;
      //            switch (sextant)
      //            {
      //                  case 0:
      //                        r = v;
      //                        g = mid1;
      //                        b = m;
      //                        break;
      //                  case 1:
      //                        r = mid2;
      //                        g = v;
      //                        b = m;
      //                        break;
      //                  case 2:
      //                        r = m;
      //                        g = v;
      //                        b = mid1;
      //                        break;
      //                  case 3:
      //                        r = m;
      //                        g = mid2;
      //                        b = v;
      //                        break;
      //                  case 4:
      //                        r = mid1;
      //                        g = m;
      //                        b = v;
      //                        break;
      //                  case 5:
      //                        r = v;
      //                        g = m;
      //                        b = mid2;
      //                        break;
      //            }
      //      }
      //      ColorRGB rgb;
      //      rgb.R = Convert.ToByte(r * 255.0f);
      //      rgb.G = Convert.ToByte(g * 255.0f);
      //      rgb.B = Convert.ToByte(b * 255.0f);
      //      return rgb;
      //}

