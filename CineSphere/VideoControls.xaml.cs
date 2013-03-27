using CineSphere.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace CineSphere
{

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class VideoControls : CineSphere.Common.LayoutAwarePage
    {

        public Size PreviousMediaSize
        {
            get { return _previousmediasize; }
            set { _previousmediasize = value; }
        }
        public bool IsFullscreen;
        public PointerEventHandler pointerpressedstage;
        public string CurrentColor = "null";

        private Thickness _previousmediaelementmargin;
        private Brush _previousBGColor;
        private Size _previousmediasize;
        private static VideoControls Current;
        private Canvas VolumeControlHolder;
        private DispatcherTimer _timer;
        private Ellipse ControlBackground;
        private Ellipse ProgressDot;
        private double _CenterX;
        private double _CenterY;
        private TranslateTransform ProgressDotTrans;
        private Windows.UI.Xaml.Shapes.Path ProgressSliderFrame;
        private Windows.UI.Xaml.Shapes.Path ProgressSlider;
        private double ProgressPosition;
        private double Diameter;
        private double rad;
        private double r;

        private double ProgressMin;
        private double ProgressMax;
        private double ProgressRange;

        private double VolumeMin = 0;
        private double VolumeMax = 96;

        private double _outerArcModifier;
        private bool _progressHasInteraction;
        private bool isVisible;
        private bool _volumeTouchedDown;
        private bool _isSelectingColor;

        private static MediaElement videoPlayer;

        private MineColorHelper MyColors = MainPage.Current.MyColors;
        private ProgressHelper MyProgressHelper;


        public VideoControls()
        {
            Current = this;


            this.InitializeComponent();


            MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            MyColors.ProgressFrameFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            MyColors.VolumeSliderFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255));
            MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            MyProgressHelper = new ProgressHelper();


            this.DrawControls();
            this.hideControls();
            videoPlayer = MainPage.Current.vidPlayer;
            videoPlayer.Volume = .5;
            MyProgressHelper.Volume = videoPlayer.Volume * VolumeMax;

            videoPlayer.MediaOpened += videoPlayer_MediaOpened;

            DisplayDebug.DataContext = MyProgressHelper;

            //DisplayDebug.DataContext = 
            PlayButton.DataContext =
            RewindButton.DataContext =
            FullscreenButton.DataContext =
            ForwardButton.DataContext =
            ColorpickerButton.DataContext = MyColors;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }
        public void DrawControls()
        {
            Diameter = 360;
            rad = Math.PI / 180;
            r = Diameter / 2;
            ProgressMin = 45;
            ProgressMax = ProgressPosition = 135;
            _outerArcModifier = 1.1;
            ProgressRange = ProgressMax - ProgressMin;


          
            ControlBackground = new Ellipse();
            ControlBackground.Fill = MyColors.HolderFillColor;
            ControlBackground.Width = Diameter;
            ControlBackground.Height = Diameter;
            ControlBackground.StrokeThickness = 1;
            ControlBackground.Stroke = MyColors.StrokeColorA;
            ControlBackground.DataContext = MyColors;

            _CenterX = (Canvas.GetLeft(PlayBackHolder) + PlayBackHolder.Width / 2);
            _CenterY = (Canvas.GetTop(PlayBackHolder) + PlayBackHolder.Height / 2);

            TranslateTransform CenterCircle = new TranslateTransform();
            CenterCircle.X = Window.Current.Bounds.Width / 2 - ControlBackground.Width / 2;
            CenterCircle.Y = Window.Current.Bounds.Height / 2 - ControlBackground.Height / 2;

            Holder.Children.Add(ControlBackground);

            Canvas.SetZIndex(ControlBackground, -600);

            this.CreateProgressElement();
            this.CreatePlaybackElements();
            this.CreateVolumeControls();
            this.CreateColorPicker();


            pointerpressedstage = new PointerEventHandler(showControls);
            //this.SetupTimer();
        }

        private void CreateProgressElement()
        {

            Binding BindingBGA = new Binding();
            BindingBGA.Path = new PropertyPath("ButtonBackGroundFillColor");
            BindingBGA.Source = MyColors;

            Binding BindingBGB = new Binding();
            BindingBGB.Path = new PropertyPath("ButtonFillColor");
            BindingBGB.Source = MyColors;

            Binding BindingSliderProgress = new Binding();
            BindingSliderProgress.Path = new PropertyPath("ProgressFrameFillColor");
            BindingSliderProgress.Source = MyColors;

            Binding BindingStrokeA = new Binding();
            BindingStrokeA.Path = new PropertyPath("StrokeColorA");
            BindingStrokeA.Source = MyColors;

            TransformGroup ProgressDotTransformGroup = new TransformGroup();
            ProgressDot = new Ellipse();
            ProgressDot.Width = Diameter / 10;
            ProgressDot.Height = Diameter / 10;
            ProgressDot.SetBinding(Shape.FillProperty, BindingBGA);

            TranslateTransform ProgressDotInitTrans = new TranslateTransform();
            ProgressDotInitTrans.X = -(Canvas.GetLeft(ProgressDot) + ProgressDot.Width / 2);
            ProgressDotInitTrans.Y = -(Canvas.GetTop(ProgressDot) + ProgressDot.Height / 2);

            ProgressDotTrans = new TranslateTransform();
            ProgressDotTrans.X = (_CenterX + (r * _outerArcModifier) * Math.Cos(ProgressMax * rad));
            ProgressDotTrans.Y = (_CenterY + (r * _outerArcModifier) * Math.Sin(ProgressMax * rad));

            ProgressDot.VerticalAlignment = VerticalAlignment.Top;
            ProgressDot.HorizontalAlignment = HorizontalAlignment.Left;


            ProgressDotTransformGroup.Children.Add(ProgressDotInitTrans);
            ProgressDotTransformGroup.Children.Add(ProgressDotTrans);
            ProgressDot.RenderTransform = ProgressDotTransformGroup;

            Debug.WriteLine(ProgressDotTrans.X);
            Debug.WriteLine(ProgressDotTrans.Y);
            
            
            ProgressSlider = new Windows.UI.Xaml.Shapes.Path();
            ProgressSlider.Data = this.Sector(_CenterX, _CenterY, Diameter - 2, ProgressPosition, ProgressMax);
            ProgressSlider.Fill = new SolidColorBrush(Color.FromArgb(168, 255, 255, 255));
            //ProgressSlider.SetBinding(Shape.FillProperty, BindingBGB);
            Canvas.SetZIndex(ProgressSlider, 1);

            ProgressSliderFrame = new Windows.UI.Xaml.Shapes.Path();
            ProgressSliderFrame.Data = this.Sector(_CenterX, _CenterY, Diameter - 2, ProgressMin, ProgressMax);
            ProgressSliderFrame.SetBinding(Shape.FillProperty, BindingSliderProgress);
            Canvas.SetZIndex(ProgressSlider, 0);

            PlayBackHolder.Children.Add(ProgressSliderFrame);
            PlayBackHolder.Children.Add(ProgressSlider);
            PlayBackHolder.Children.Add(ProgressDot);
            


            Binding posBinding = new Binding();
            posBinding.Path = new PropertyPath("VideoPosition");
            posBinding.Source = MyProgressHelper;
            // posBinding.ConverterParameter = ;
            posBinding.Converter = new PositionConverter();

            DisplayDebug.SetBinding(TextBlock.TextProperty, posBinding);


            PointerEventHandler pointerpressedhandler = new PointerEventHandler(Progress_PointerEntered);
            ProgressSliderFrame.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            ProgressSlider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            ProgressDot.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerlosthandler = new PointerEventHandler(Progress_PointerCaptureLost);
            ProgressSliderFrame.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);
            ProgressSlider.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);
            ProgressDot.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);
            MainPage.Current.vidView.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(Progress_PointerCaptureLost);
            ProgressSliderFrame.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            ProgressSlider.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            ProgressDot.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            MainPage.Current.vidView.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);

            PointerEventHandler pointerdraggedhandler = new PointerEventHandler(Progress_Dragged);
            ProgressSliderFrame.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            ProgressSlider.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            ProgressDot.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            MainPage.Current.vidView.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);

        }

        private void CreateColorPicker()
        {



        }

        private void CreatePlaybackElements()
        {


        }

        private void CreateVolumeControls()
        {
            VolumeControlHolder = new Canvas();

            Windows.UI.Xaml.Shapes.Path Volume1 = DrawVolumeControls(186, .868, 0);
            Windows.UI.Xaml.Shapes.Path Volume2 = DrawVolumeControls(213, .8859, 1);
            Windows.UI.Xaml.Shapes.Path Volume3 = DrawVolumeControls(242, .8976, 2);
            Windows.UI.Xaml.Shapes.Path Volume4 = DrawVolumeControls(270, .9071, 3);
            Windows.UI.Xaml.Shapes.Path Volume5 = DrawVolumeControls(299, .9154, 4);
            Windows.UI.Xaml.Shapes.Path Volume6 = DrawVolumeControls(328, .9216, 5);
            Windows.UI.Xaml.Shapes.Path Volume7 = DrawVolumeControls(358, .9273, 6);

            VolumeControlHolder.Children.Add(Volume1);
            VolumeControlHolder.Children.Add(Volume2);
            VolumeControlHolder.Children.Add(Volume3);
            VolumeControlHolder.Children.Add(Volume4);
            VolumeControlHolder.Children.Add(Volume5);
            VolumeControlHolder.Children.Add(Volume6);
            VolumeControlHolder.Children.Add(Volume7);

            PlayBackHolder.Children.Add(VolumeControlHolder);

            PointerEventHandler pointerpressedhandler = new PointerEventHandler(volume_PointerEntered);
            VolumeControlHolder.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerlosthandler = new PointerEventHandler(volume_PointerCaptureLost);
            VolumeControlHolder.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);
            MainPage.Current.vidView.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(volume_PointerCaptureLost);
            VolumeControlHolder.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            MainPage.Current.vidView.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);

            PointerEventHandler pointerdraggedhandler = new PointerEventHandler(volume_dragged);
            VolumeControlHolder.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            MainPage.Current.vidView.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);

        }

        private Windows.UI.Xaml.Shapes.Path DrawVolumeControls(double radian, double innerMath = .46, int idx = 0)
        {
            Binding myBinding = new Binding();
            myBinding.Path = new PropertyPath("VolumeSliderFillColor");
            myBinding.Source = MyColors;


            Binding volBinding = new Binding();
            volBinding.Path = new PropertyPath("Volume");
            volBinding.Source = MyProgressHelper;
            volBinding.Converter = new VolumeConverter();

            volBinding.ConverterParameter = idx;


            var ReturnedPath = new Windows.UI.Xaml.Shapes.Path();
            ReturnedPath.Data = this.Sector(_CenterX, _CenterY, radian, ProgressMin - 180, ProgressMax - 180, innerMath);
            ReturnedPath.SetBinding(Shape.FillProperty, myBinding);
            ReturnedPath.SetBinding(Shape.OpacityProperty, volBinding);

            return ReturnedPath;
        }

        private PathGeometry Sector(double PosX, double PosY, double Diameter, double startingAngle, double endingAngle, double innerMathOverride = 0)
        {

            r = Diameter / 2;

            //This is an arbitrary perventage value that lets us decide the radius of the inner circle based on a fraction of the outer radius
            double modifier = (innerMathOverride != 0) ? innerMathOverride : .46;
            double innerMath = Math.Floor(Diameter / 2 * modifier);

            double x1 = PosX + innerMath * Math.Cos(startingAngle * rad);
            double y1 = PosY + innerMath * Math.Sin(startingAngle * rad);
            double x2 = PosX + r * Math.Cos(startingAngle * rad);
            double y2 = PosY + r * Math.Sin(startingAngle * rad);
            double x3 = PosX + r * Math.Cos(endingAngle * rad);
            double y3 = PosY + r * Math.Sin(endingAngle * rad);
            double x4 = PosX + innerMath * Math.Cos(endingAngle * rad);
            double y4 = PosY + innerMath * Math.Sin(endingAngle * rad);


            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(x1, y1);

            LineSegment lineSegment1 = new LineSegment();
            lineSegment1.Point = new Point(x2, y2);
            pathFigure.Segments.Add(lineSegment1);

            ArcSegment arcSegment1 = new ArcSegment();
            arcSegment1.Point = new Point(x3, y3);
            arcSegment1.Size = new Windows.Foundation.Size(r, r);
            arcSegment1.SweepDirection = SweepDirection.Clockwise;
            pathFigure.Segments.Add(arcSegment1);

            LineSegment lineSegment2 = new LineSegment();
            lineSegment2.Point = new Point(x4, y4);
            pathFigure.Segments.Add(lineSegment2);

            ArcSegment arcSegment2 = new ArcSegment();
            arcSegment2.Point = new Point(x1, y1);
            arcSegment2.Size = new Windows.Foundation.Size(innerMath, innerMath);
            arcSegment2.SweepDirection = SweepDirection.Counterclockwise;
            pathFigure.Segments.Add(arcSegment2);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures = new PathFigureCollection();

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }



        #region progress Control Logic

        void Progress_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("click capture");

            videoPlayer.DefaultPlaybackRate = 0.0;



            PointerPoint unpoint = e.GetCurrentPoint(PlayBackHolder);

            double newPoint = Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 >= 0 ?
                              Math.Max(Math.Min(Math.Floor(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 - 90), ProgressMax), ProgressMin) :
                              Math.Max(Math.Min(Math.Floor(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 + 270), ProgressMax), ProgressMin);

            ProgressPosition = newPoint;

            videoPlayer.Pause();

            _progressHasInteraction = true;
        }

        void Progress_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {

            if (_progressHasInteraction)
            {
                videoPlayer.Position = TimeSpan.FromMilliseconds((ProgressMax - ProgressPosition) / (ProgressMax - ProgressMin) * videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds);
                _progressHasInteraction = false;

                videoPlayer.DefaultPlaybackRate = 1.0;
                videoPlayer.PlaybackRate = 1.0;
                videoPlayer.Play();
            }
        }

        void Progress_Dragged(object sender, PointerRoutedEventArgs e)
        {
            if (_progressHasInteraction)
            {

                videoPlayer.DefaultPlaybackRate = 0;


                PointerPoint unpoint = e.GetCurrentPoint(PlayBackHolder);

                double newPoint = Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 >= 0 ?
                                  Math.Max(Math.Min(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 - 90, ProgressMax), ProgressMin) :
                                  Math.Max(Math.Min(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 + 270, ProgressMax), ProgressMin);



                ProgressPosition = newPoint;
                videoPlayer.Position = TimeSpan.FromMilliseconds((ProgressMax - ProgressPosition) / (ProgressMax - ProgressMin) * videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds);
                MyProgressHelper.VideoPosition = videoPlayer.Position.TotalMilliseconds;

                //if (ProgressPosition >= ProgressMax) ProgressPosition = ProgressMax;
                //if (ProgressPosition <= ProgressMin) ProgressPosition = ProgressMin;
                //if (ProgressPosition >= ProgressMax || ProgressPosition <= ProgressMin) _progressHasInteraction = false;

            }
        }
        #endregion


        #region timer logic
        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            //_timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Interval = TimeSpan.FromMilliseconds(SliderFrequency(videoPlayer.NaturalDuration.TimeSpan));
            MyProgressHelper.VideoPosition = SliderFrequency(videoPlayer.NaturalDuration.TimeSpan);
            StartTimer();
        }

        private void _timer_Tick(object sender, object e)
        {
            if (!_progressHasInteraction)
            {
                MyProgressHelper.VideoPosition = videoPlayer.Position.TotalMilliseconds;
                ProgressPosition = ProgressMax - (videoPlayer.Position.TotalMilliseconds / videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds) * (ProgressMax - ProgressMin);
                //Debug.WriteLine(videoPlayer.Position.TotalMilliseconds * SliderFrequency(videoPlayer.NaturalDuration.TimeSpan));

                if (ProgressPosition >= ProgressMax) ProgressPosition = ProgressMax;
                if (ProgressPosition <= ProgressMin) ProgressPosition = ProgressMin;
            }
            ProgressSlider.Data = this.Sector((Canvas.GetLeft(ControlBackground) + ControlBackground.Width / 2), (Canvas.GetTop(ControlBackground) + ControlBackground.Height / 2), Diameter, ProgressPosition, ProgressMax);

            ProgressDotTrans.X = _CenterX + (r * _outerArcModifier) * Math.Cos(ProgressPosition * rad);
            ProgressDotTrans.Y = _CenterY + (r * _outerArcModifier) * Math.Sin(ProgressPosition * rad);


            //if (ProgressPosition >= ProgressMax) this.StopTimer();
        }

        private void StartTimer()
        {
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }
        #endregion

        #region playback Controls

        async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (videoPlayer.CurrentState == MediaElementState.Playing)
                {
                    videoPlayer.Pause();
                }
                else
                {
                    videoPlayer.Play();
                }
            }

            );
        }

        void MediaControl_PlayPressed(object sender, object e)
        {
            videoPlayer.Play();
        }

        void MediaControl_PausePressed(object sender, object e)
        {
            videoPlayer.Pause();
        }

        public void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            double absvalue = (int)Math.Round(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds, MidpointRounding.AwayFromZero);

            TimeSpan t = TimeSpan.FromMilliseconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds);

            videoPlayer.Width = Window.Current.Bounds.Width * .66;

            MyProgressHelper.Volume = videoPlayer.Volume * VolumeMax;

            SetupTimer();
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {

            if (ColorPickerHolder.Opacity.ToString() == "0" || ColorPickerHolder.Visibility.ToString() == "Collapsed")
            {
                ColorPickerHolder.Opacity = 1;
                ColorPickerHolder.Visibility = Visibility.Visible;

                PlayBackHolder.Opacity = 0;
                PlayBackHolder.Visibility = Visibility.Collapsed;
            }
            else {

                ColorPickerHolder.Opacity = 0;
                ColorPickerHolder.Visibility = Visibility.Collapsed;

                PlayBackHolder.Opacity = 1;
                PlayBackHolder.Visibility = Visibility.Visible;

            }

            //switch (CurrentColor)
            //{
            //    case "null":
            //        MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            //        MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(168, 180, 0, 0));
            //        MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            //        MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 255, 0, 0));
            //        MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            //        MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            //        CurrentColor = "red";
            //        MainPage.Current.tintView.Opacity = .22;
            //        MainPage.Current.tintView.Visibility = Visibility.Visible;

            //        break;
            //    case "red":
            //        MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            //        MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(168, 0, 180, 0));
            //        MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            //        MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 0, 255, 0));
            //        MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            //        MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            //        CurrentColor = "green";
            //        MainPage.Current.tintView.Opacity = .22;
            //        MainPage.Current.tintView.Visibility = Visibility.Visible;

            //        break;
            //    case "green":
            //        MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            //        MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(168, 0, 0, 180));
            //        MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            //        MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 0, 0, 255));
            //        MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            //        MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
            //        CurrentColor = "blue";
            //        MainPage.Current.tintView.Opacity = .22;
            //        MainPage.Current.tintView.Visibility = Visibility.Visible;

            //        break;
            //    case "blue":
            //        MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            //        MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(168, 255, 255, 255));
            //        MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            //        MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255));
            //        MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            //        MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            //        CurrentColor = "null";
            //        MainPage.Current.tintView.Opacity = .0;

            //        break;
            //}

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (videoPlayer.DefaultPlaybackRate == 0)
            {
                videoPlayer.DefaultPlaybackRate = 1.0;
                videoPlayer.PlaybackRate = 1.0;
            }

            SetupTimer();
            videoPlayer.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Stop();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.DefaultPlaybackRate = 0.0;
            videoPlayer.PlaybackRate = 4.0;
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {

            Debug.WriteLine("yes");
            videoPlayer.DefaultPlaybackRate = 0.0;
            videoPlayer.PlaybackRate = -4.0;
        }

        private double SliderFrequency(TimeSpan timevalue)
        {
            double stepfrequency = -1;

            double absvalue = (int)Math.Round(timevalue.TotalSeconds, MidpointRounding.AwayFromZero);
            stepfrequency = (int)(Math.Round(absvalue / 90));

            if (timevalue.TotalMinutes >= 10 && timevalue.TotalMinutes < 30)
            {
                stepfrequency = 10;
            }
            else if (timevalue.TotalMinutes >= 30 && timevalue.TotalMinutes < 60)
            {
                stepfrequency = 30;
            }
            else if (timevalue.TotalHours >= 1)
            {
                stepfrequency = 60;
            }

            if (stepfrequency == 0) stepfrequency += 1;

            if (stepfrequency == 1)
            {
                stepfrequency = absvalue / 90;
            }

            return stepfrequency;
        }


        public void HandleColorChange(Color obj)
        {

            string objstring = obj.ToString();

            if (!objstring.Equals("#FF767676", StringComparison.Ordinal))
            {

            MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(204, (byte)(obj.R * .69), (byte)(obj.G * .69), (byte)(obj.B * .69)));
            MyColors.ProgressFrameFillColor = new SolidColorBrush(Color.FromArgb(255, obj.R, obj.G, obj.B));
            MyColors.VolumeSliderFillColor = new SolidColorBrush(Color.FromArgb(255, obj.R, obj.G, obj.B));

            MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(204, (byte)(obj.R * .69), (byte)(obj.G * .69), (byte)(obj.B * .69)));

            MainPage.Current.tintView.Opacity = .1;
            MainPage.Current.tintView.Visibility = Visibility.Visible;
            } else {
           MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            MyColors.ProgressFrameFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            MyColors.VolumeSliderFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255));
            MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            MainPage.Current.tintView.Opacity = 0;
            MainPage.Current.tintView.Visibility = Visibility.Collapsed;}

        }


        public void FullscreenToggle()
        {
            this.IsFullscreen = !this.IsFullscreen;

            if (this.IsFullscreen)
            {

                Current.Visibility = Visibility.Collapsed;

                _previousmediasize.Height = videoPlayer.Height;
                _previousmediasize.Width = videoPlayer.Width;
                _previousmediaelementmargin = MainPage.Current.mainGrid.Margin;

                _previousBGColor = MainPage.Current.mainGrid.Background;
                MainPage.Current.mainGrid.Margin = new Thickness();
                MainPage.Current.mainGrid.Background = new SolidColorBrush(Colors.Black);
                MainPage.Current.tintView.Visibility = Visibility.Collapsed;

                videoPlayer.Width = Window.Current.Bounds.Width;
                videoPlayer.Height = double.NaN;
            }
            else
            {

                Current.Visibility = Visibility.Visible;

                MainPage.Current.mainGrid.Background = _previousBGColor;
                MainPage.Current.mainGrid.Margin = _previousmediaelementmargin;
                MainPage.Current.tintView.Visibility = Visibility.Visible;

                videoPlayer.Width = _previousmediasize.Width;
                videoPlayer.Height = _previousmediasize.Height;

            }
        }

        void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            FullscreenToggle();
        }

        void meHost_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (IsFullscreen && e.Key == Windows.System.VirtualKey.Escape)
                FullscreenToggle();
            e.Handled = true;
        }


        #endregion


        #region volume control logic

        private void volume_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint unpoint = e.GetCurrentPoint(VolumeControlHolder);

            double newVolumePosition = Math.Max(Math.Min(Math.Floor(VolumeMax - unpoint.Position.Y), VolumeMax), VolumeMin);


            videoPlayer.Volume = Math.Floor(newVolumePosition) / VolumeMax;

            //Debug.WriteLine("liteup " + newVolumePosition / 13);

            //double newPoint = ((((Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI) * 180)) - _CenterX);

            MyProgressHelper.Volume = newVolumePosition;


            _volumeTouchedDown = true;
        }

        private void volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            _volumeTouchedDown = false;
        }

        void volume_dragged(object sender, PointerRoutedEventArgs e)
        {
            if (_volumeTouchedDown)
            {
                PointerPoint unpoint = e.GetCurrentPoint(VolumeControlHolder);

                double newVolumePosition = Math.Max(Math.Min(Math.Floor(VolumeMax - unpoint.Position.Y), VolumeMax), VolumeMin);

                videoPlayer.Volume = Math.Floor(newVolumePosition) / VolumeMax;
                MyProgressHelper.Volume = newVolumePosition;


                //Debug.WriteLine("liteup " + newVolumePosition / 13);


            }
        }
        #endregion


        private void CaluclatePercentage(double min, double max, double point)
        {

        }


        private void hideControls()
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void showControls(object sender, PointerRoutedEventArgs e)
        {

            PointerPoint unpoint = e.GetCurrentPoint(PlayBackHolder);

            if (!isVisible && unpoint.Position.Y > 180)
            {
                TranslateTransform PositionOfControls = new TranslateTransform();
                PositionOfControls.X = unpoint.Position.X - 181;
                PositionOfControls.Y = unpoint.Position.Y - 181;

                //Current.RenderTransform = PositionOfControls;
                Current.Visibility = Visibility.Visible;

                Debug.WriteLine(Current.Width);
                isVisible = true;

                MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                // pointerpressedstage = new PointerEventHandler(HandleColorChange);
                // VideoDetailView.Current.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);


            }
        }

        private void Get_Pixel(object sender, PointerRoutedEventArgs e)
        {

            _isSelectingColor = true;
            PointerPoint unpoint = e.GetCurrentPoint(ColorPickerHolder);

            var ttv = ColorPickerHolder.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            if (_isSelectingColor)
            {
                Color newColor = Win32.GetPixelColor((int)screenCoords.X + (int)unpoint.Position.X, (int)screenCoords.Y + (int)unpoint.Position.Y);
                HandleColorChange(newColor);
            }

            //MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            //MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(168, 0, 0, 180));
            //MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            //MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 0, 0, 255));
            //MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            //MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
            //CurrentColor = "blue";
            //MainPage.Current.tintView.Opacity = .22;
            //MainPage.Current.tintView.Visibility = Visibility.Visible;
        }

        private void PointerUp(object sender, PointerRoutedEventArgs e)
        {
            _isSelectingColor = false;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            ColorPickerHolder.Opacity = 0;
            ColorPickerHolder.Visibility = Visibility.Collapsed;

            PlayBackHolder.Opacity = 1;
            PlayBackHolder.Visibility = Visibility.Visible;

        }

        private void ReGet_Pixel(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint unpoint = e.GetCurrentPoint(ColorPickerHolder);

            var ttv = ColorPickerHolder.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            if (_isSelectingColor)
            {
                Color newColor = Win32.GetPixelColor((int)screenCoords.X + (int)unpoint.Position.X, (int)screenCoords.Y + (int)unpoint.Position.Y);
                HandleColorChange(newColor);
            }

        }

        private void OverColorWheel(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Cross, 0);
        }


    }


    public class VolumeConverter : IValueConverter
    {

        public object Convert(object value,
  Type targetType,
  object parameter,
  string language)
        {
            //Assumes a growth of 95 with 7 steps
            double maxvolume = 96;
            double hardcodedsteps = maxvolume / 7;

            if (Math.Floor(System.Convert.ToDouble(value) / hardcodedsteps) > System.Convert.ToDouble(parameter) + 1 || System.Convert.ToDouble(value) / hardcodedsteps - (System.Convert.ToDouble(parameter)) >= .7)
            {
                return .7;
            }
            else if (Math.Floor(System.Convert.ToDouble(value) / hardcodedsteps) < System.Convert.ToDouble(parameter) || Math.Floor(System.Convert.ToDouble(value) / hardcodedsteps) == 0)
            {
                return .1;
            }
            else
            {
                return System.Convert.ToDouble(value) / hardcodedsteps - (System.Convert.ToDouble(parameter));
            }

        }

        public object ConvertBack(object value,
  Type targetType,
  object parameter,
  string language)
        {
            throw new NotImplementedException();
        }
    }


    public class PositionConverter : IValueConverter
    {
        //90degree arc

        public object Convert(object value,
  Type targetType,
  object parameter,
  string language)
        {

            TimeSpan t = TimeSpan.FromMilliseconds(System.Convert.ToDouble(value));

            return t.ToString(@"hh\:mm\:ss");

        }

        public object ConvertBack(object value,
  Type targetType,
  object parameter,
  string language)
        {
            throw new NotImplementedException();
        }
    }

}
