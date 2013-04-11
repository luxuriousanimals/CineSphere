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
        private Brush _originalBG;
        private Size _previousmediasize;
        private static VideoControls Current;
        private Canvas VolumeControlHolder;
        private DispatcherTimer _timer;
        private DispatcherTimer _controlsTimer;
        private Ellipse ControlBackground;
        // private Image ProgressDot;
        private Canvas ProgressDot;
        private double _CenterX;
        private double _CenterY;
        private TranslateTransform ProgressDotTrans;
        private RotateTransform ProgressDotRot;
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
        public bool isVisible;
        private bool _volumeTouchedDown;
        private bool _isSelectingColor;

        int timesTicked = 1;
        int timesToTick = 15;

        private static MediaElement videoPlayer;

        private MineColorHelper MyColors = MainPage.Current.MyColors;
        public ProgressHelper MyProgressHelper;
        public Grid RefHolder;
        public Grid RefMaster;

        public VideoControls()
        {

            this.InitializeComponent();

            Current = this;
            RefHolder = Holder;
            RefMaster = videoControllerGrid;

            MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            MyColors.ProgressFrameFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
            MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            MyColors.VolumeSliderFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255));
            MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            MyProgressHelper = new ProgressHelper();
            ProgressDot = this.ProgressDots;

            this.DrawControls();
            this.hideControls();
            this.setUpControlTimer();
            videoPlayer = MainPage.Current.vidPlayer;
            videoPlayer.Volume = .5;
            MyProgressHelper.Volume = videoPlayer.Volume * VolumeMax;

            videoPlayer.MediaOpened += videoPlayer_MediaOpened;

            DisplayTime.DataContext = MyProgressHelper;

            PlayButton.DataContext =
            RewindButton.DataContext =
            FullscreenButton.DataContext =
            ForwardButton.DataContext =
            ColorpickerButton.DataContext = MyColors;
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

            _originalBG = MainPage.Current.mainGrid.Background;

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
            this.CreateVolumeControls();


            pointerpressedstage = new PointerEventHandler(showControls);
            PointerEventHandler pointerovercontrols = new PointerEventHandler(_resetTimerHandler);
            Holder.AddHandler(Control.PointerMovedEvent, pointerovercontrols, true);
        }

        public void reset() {
            RewindButtonText.Text = "";
            ForwardButtonText.Text = "";
            ForwardButtonGraphic.Opacity = 1;
            RewindButtonGraphic.Opacity = 1;
            videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = 1.0;
            videoPlayer.DefaultPlaybackRate = 1.0;


            ForwardButtonText.Text = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? "" : Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() + "x";
            ForwardButtonGraphic.Opacity = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? 1 : 0;
            RewindButtonText.Text = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? "" : Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() + "x";
            RewindButtonGraphic.Opacity = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? 1 : 0;
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

            ProgressDot.Width = 60;
            ProgressDot.Height = 40;
            //ProgressDot.SetBinding(Shape.FillProperty, BindingBGA);

            TranslateTransform ProgressDotInitTrans = new TranslateTransform();
            ProgressDotInitTrans.X = -(Canvas.GetLeft(ProgressDot) + ProgressDot.Width / 2);
            ProgressDotInitTrans.Y = -(Canvas.GetTop(ProgressDot) + ProgressDot.Height / 2);

            ProgressDotTrans = new TranslateTransform();
            ProgressDotTrans.X = (_CenterX + (r * _outerArcModifier) * Math.Cos(ProgressMax * rad));
            ProgressDotTrans.Y = (_CenterY + (r * _outerArcModifier) * Math.Sin(ProgressMax * rad));

            ProgressDot.VerticalAlignment = VerticalAlignment.Top;
            ProgressDot.HorizontalAlignment = HorizontalAlignment.Left;

            ProgressDotRot = new RotateTransform();
            ProgressDotRot.Angle = (Double.IsNaN(ProgressPosition)) ? ProgressMax + 90 : ProgressPosition + 90;
            ProgressDotRot.CenterX = 0;
            ProgressDotRot.CenterY = 0;

            ProgressDotTransformGroup.Children.Add(ProgressDotInitTrans);
            ProgressDotTransformGroup.Children.Add(ProgressDotRot);
            ProgressDotTransformGroup.Children.Add(ProgressDotTrans);
            ProgressDot.RenderTransform = ProgressDotTransformGroup;

            ProgressSlider = new Windows.UI.Xaml.Shapes.Path();
            ProgressSlider.Data = this.Sector(_CenterX, _CenterY, Diameter - 2, ProgressPosition, ProgressMax);
            ProgressSlider.Fill = new SolidColorBrush(Color.FromArgb(168, 255, 255, 255));
            Canvas.SetZIndex(ProgressSlider, 1);

            ProgressSliderFrame = new Windows.UI.Xaml.Shapes.Path();
            ProgressSliderFrame.Data = this.Sector(_CenterX, _CenterY, Diameter - 2, ProgressMin, ProgressMax);
            ProgressSliderFrame.SetBinding(Shape.FillProperty, BindingSliderProgress);
            Canvas.SetZIndex(ProgressSlider, 0);

            PlayBackHolder.Children.Add(ProgressSliderFrame);
            PlayBackHolder.Children.Add(ProgressSlider);

            Binding posBinding = new Binding();
            posBinding.Path = new PropertyPath("VideoPosition");
            posBinding.Source = MyProgressHelper;
            // posBinding.ConverterParameter = ;
            posBinding.Converter = new PositionConverter();

            DisplayTime.SetBinding(TextBlock.TextProperty, posBinding);

            PointerEventHandler pointerpressedhandler = new PointerEventHandler(Progress_PointerEntered);
            ProgressSliderFrame.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            ProgressSlider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            ProgressDot.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerlosthandler = new PointerEventHandler(Progress_PointerCaptureLost);
            ProgressSliderFrame.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);
            ProgressSlider.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);
            ProgressDot.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);
            MainPage.Current.mainGrid.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(Progress_PointerCaptureLost);
            ProgressSliderFrame.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            ProgressSlider.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            ProgressDot.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            MainPage.Current.mainGrid.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);

            PointerEventHandler pointerdraggedhandler = new PointerEventHandler(Progress_Dragged);
            ProgressSliderFrame.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            ProgressSlider.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            ProgressDot.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            MainPage.Current.mainGrid.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);

        }

        private void CreateVolumeControls()
        {
            VolumeControlHolder = new Canvas();

            Windows.UI.Xaml.Shapes.Path Volume1 = DrawVolumeControls(186, .88, 0);
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
            MainPage.Current.mainGrid.AddHandler(Control.PointerCaptureLostEvent, pointerlosthandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(volume_PointerCaptureLost);
            VolumeControlHolder.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);
            MainPage.Current.mainGrid.AddHandler(Control.PointerReleasedEvent, pointerreleasedhandler, true);

            PointerEventHandler pointerdraggedhandler = new PointerEventHandler(volume_dragged);
            VolumeControlHolder.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);
            MainPage.Current.mainGrid.AddHandler(Control.PointerMovedEvent, pointerdraggedhandler, true);

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

            videoPlayer.DefaultPlaybackRate = 0.0;

            PointerPoint unpoint = e.GetCurrentPoint(PlayBackHolder);
            if (unpoint.Properties.IsRightButtonPressed) return;

            double newPoint = Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 >= 0 ?
                              Math.Max(Math.Min(Math.Floor(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 - 90), ProgressMax), ProgressMin) :
                              Math.Max(Math.Min(Math.Floor(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 + 270), ProgressMax), ProgressMin);

            ProgressPosition = newPoint;

            videoPlayer.Pause();

            _progressHasInteraction = true;
            _controlsStopTimer();
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

                _resetTimer();
            }
        }

        void Progress_Dragged(object sender, PointerRoutedEventArgs e)
        {
            if (_progressHasInteraction)
            {

                videoPlayer.DefaultPlaybackRate = 0;
                
                PointerPoint unpoint = e.GetCurrentPoint(PlayBackHolder);
                if (unpoint.Properties.IsRightButtonPressed) return;

                double newPoint = Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 >= 0 ?
                                  Math.Max(Math.Min(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 - 90, ProgressMax), ProgressMin) :
                                  Math.Max(Math.Min(Math.Atan2(unpoint.Position.X - _CenterX, _CenterY - unpoint.Position.Y) / Math.PI * 180 + 270, ProgressMax), ProgressMin);
                
                ProgressPosition = newPoint;
                videoPlayer.Position = TimeSpan.FromMilliseconds((ProgressMax - ProgressPosition) / (ProgressMax - ProgressMin) * videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds);
                MyProgressHelper.VideoPosition = videoPlayer.Position.TotalMilliseconds;


            }
        }
        #endregion

        #region timer logic

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(SliderFrequency(videoPlayer.NaturalDuration.TimeSpan));
            StartTimer();
        }

        private void _timer_Tick(object sender, object e)
        {


            if (videoPlayer.CurrentState == MediaElementState.Playing)
            {
                if (videoPlayer.DefaultPlaybackRate == 0)
                {
                    ActualPause.Opacity = 0;
                    ActualPlay.Opacity = 1;
                }
                else {
                    RewindButtonText.Text = "";
                    ForwardButtonText.Text = "";
                    ForwardButtonGraphic.Opacity = 1;
                    RewindButtonGraphic.Opacity = 1;
                    ActualPause.Opacity = 1;
                    ActualPlay.Opacity = 0;
                }
                
            }
            else
            {
                ActualPause.Opacity = 0;
                ActualPlay.Opacity = 1;
            }

            if (!_progressHasInteraction)
            {

                MyProgressHelper.VideoPosition = (videoPlayer.Position.TotalMilliseconds == 0) ? MyProgressHelper.VideoPosition : videoPlayer.Position.TotalMilliseconds;
                ProgressPosition = ProgressMax - (videoPlayer.Position.TotalMilliseconds / videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds) * (ProgressMax - ProgressMin);

                if (ProgressPosition >= ProgressMax) ProgressPosition = ProgressMax;
                if (ProgressPosition <= ProgressMin) ProgressPosition = ProgressMin;
            }
            else if (_progressHasInteraction)
            {

                _resetTimer();
            }
            ProgressSlider.Data = this.Sector((Canvas.GetLeft(ControlBackground) + ControlBackground.Width / 2), (Canvas.GetTop(ControlBackground) + ControlBackground.Height / 2), Diameter, ProgressPosition, ProgressMax);

            ProgressDotRot.Angle = (Double.IsNaN(ProgressPosition)) ? ProgressMax + 90 : ProgressPosition + 90;

            ProgressDotTrans.X = _CenterX + (r * _outerArcModifier) * Math.Cos(ProgressPosition * rad);
            ProgressDotTrans.Y = _CenterY + (r * _outerArcModifier) * Math.Sin(ProgressPosition * rad);

        }

        private void StartTimer()
        {
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }
        #endregion

        #region playback Controls

        async void PlayPauseTogglePressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (videoPlayer.CurrentState == MediaElementState.Playing && videoPlayer.DefaultPlaybackRate == 1.0)
                {
                    videoPlayer.Pause();
                    ActualPause.Opacity = 0;
                    ActualPlay.Opacity = 1;
                }
                else
                {
                    if (videoPlayer.DefaultPlaybackRate != 1.0)
                    {
                        videoPlayer.DefaultPlaybackRate = 1.0;
                        videoPlayer.PlaybackRate = 1.0;
                    }

                    videoPlayer.Play();
                    ActualPause.Opacity = 1;
                    ActualPlay.Opacity = 0;
                    MyProgressHelper.CurrentPlaybackRate = 1.0;
                }
            }
            );

        }

      
        public void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {

            double absvalue = (int)Math.Round(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds, MidpointRounding.AwayFromZero);

            TimeSpan t = TimeSpan.FromMilliseconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds);

            videoPlayer.Width = Window.Current.Bounds.Width * .66;

            MyProgressHelper.Volume = videoPlayer.Volume * VolumeMax;
            MyProgressHelper.CurrentPlaybackRate = 1;


            if (MyProgressHelper.VideoPosition != 0)
            {
                videoPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)MyProgressHelper.VideoPosition);
            }

            SetupTimer();
            if (IsFullscreen) FullscreenOn();

            MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
            pointerpressedstage = new PointerEventHandler(showControls);
            MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);
            videoPlayer.Play();
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
           // _controlsStopTimer();

            if (ColorPickerHolder.Opacity.ToString() == "0" || ColorPickerHolder.Visibility.ToString() == "Collapsed")
            {

                VisualStateManager.GoToState(this, "showColorPicker", true);
            
            }
            else
            {

                VisualStateManager.GoToState(this, "hideColorPicker", true);

            }

        }

      
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (videoPlayer.CurrentState == MediaElementState.Paused) videoPlayer.Play();
            videoPlayer.DefaultPlaybackRate = 0.0;
            _controlsStopTimer();
            switch ((int)MyProgressHelper.CurrentPlaybackRate)
            {
                case 1:
                default: 
                videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = 4.0;
                    RewindButtonText.Text = "";
                    ForwardButtonText.Text = "";
                    ForwardButtonGraphic.Opacity = 1;
                    RewindButtonGraphic.Opacity = 1;
                break;
                case 4:
                    videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = 8.0;
                break;
                case 8:
                    videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = 16.0;
                break;
                case 16:
                    videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = 1.0;
                    videoPlayer.DefaultPlaybackRate = 1.0;

                break;

            }
            ForwardButtonText.Text = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? "" : Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() + "x";
            ForwardButtonGraphic.Opacity = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? 1 : 0;
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            if (videoPlayer.CurrentState == MediaElementState.Paused) videoPlayer.Play();
            videoPlayer.DefaultPlaybackRate = 0.0;
            _controlsStopTimer();
            switch ((int)MyProgressHelper.CurrentPlaybackRate)
            {
                case 1:
                default:
                    videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = -4.0;
                    RewindButtonText.Text = "";
                    ForwardButtonText.Text = "";
                    ForwardButtonGraphic.Opacity = 1;
                    RewindButtonGraphic.Opacity = 1;
                    break;
                case -4:
                    videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = -8.0;
                    break;
                case -8:
                    videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = -16.0;
                    break;
                case -16:
                    videoPlayer.PlaybackRate = MyProgressHelper.CurrentPlaybackRate = 1.0;
                    videoPlayer.DefaultPlaybackRate = 1.0;

                    break;

            }
            RewindButtonText.Text = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? "" : Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString()+"x";
            RewindButtonGraphic.Opacity = (Math.Abs((int)MyProgressHelper.CurrentPlaybackRate).ToString() == "1") ? 1 : 0;

           
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


            if (!objstring.Equals("#FF5E5E5E", StringComparison.Ordinal) && !objstring.Equals("#FF000000", StringComparison.Ordinal) && !objstring.Equals("#FFFFFFFF", StringComparison.Ordinal))
            {

                MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(204, (byte)(obj.R * .69), (byte)(obj.G * .69), (byte)(obj.B * .69)));
                MyColors.ProgressFrameFillColor = new SolidColorBrush(Color.FromArgb(255, obj.R, obj.G, obj.B));
                MyColors.VolumeSliderFillColor = new SolidColorBrush(Color.FromArgb(255, obj.R, obj.G, obj.B));

                MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(204, (byte)(obj.R * .69), (byte)(obj.G * .69), (byte)(obj.B * .69)));

                MainPage.Current.tintView.Opacity = (MainPage.Current.mainGrid.Background.ToString().Contains("SolidColorBrush")) ? 0 : .1;
                MainPage.Current.tintView.Visibility = Visibility.Visible;
            }
            else
            {
                MyColors.ButtonBackGroundFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
                MyColors.ProgressFrameFillColor = new SolidColorBrush(Color.FromArgb(41, 255, 255, 255));
                MyColors.ButtonFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                MyColors.VolumeSliderFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                MyColors.HolderFillColor = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
                MyColors.StrokeColorA = new SolidColorBrush(Color.FromArgb(102, 255, 255, 255));
                MyColors.StrokeColorB = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                MyColors.StrokeColorC = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                MainPage.Current.tintView.Opacity = 0;
                MainPage.Current.tintView.Visibility = Visibility.Collapsed;
            }
            _controlsStopTimer();
        }


        public void FullscreenToggle()
        {
            this.IsFullscreen = !this.IsFullscreen;

            if (this.IsFullscreen)
            {

                hideControls();

                _previousmediasize.Height = videoPlayer.Height;
                _previousmediasize.Width = videoPlayer.Width;
                _previousmediaelementmargin = MainPage.Current.mainGrid.Margin;

                MainPage.Current.mainGrid.Margin = new Thickness();
                MainPage.Current.mainGrid.Background = new SolidColorBrush(Colors.Black);
                MainPage.Current.tintView.Visibility = Visibility.Collapsed;

                videoPlayer.Width = Window.Current.Bounds.Width;
                videoPlayer.Height = double.NaN;
            }
            else
            {
                hideControls();

                MainPage.Current.mainGrid.Background = _originalBG;
                MainPage.Current.mainGrid.Margin = _previousmediaelementmargin;
                MainPage.Current.tintView.Visibility = Visibility.Visible;

                videoPlayer.Width = _previousmediasize.Width;
                videoPlayer.Height = _previousmediasize.Height;

            }
            _resetTimer();
        }

        public void FullscreenOff()
        {
                this.IsFullscreen = false;

                hideControls();

                MainPage.Current.mainGrid.Background = _originalBG;
                MainPage.Current.mainGrid.Margin = _previousmediaelementmargin;
                MainPage.Current.tintView.Visibility = Visibility.Visible;

                videoPlayer.Width = _previousmediasize.Width;
                videoPlayer.Height = _previousmediasize.Height;
        
        }


        public void FullscreenOn()
        {
            this.IsFullscreen = true;

            hideControls();

            _previousmediasize.Height = videoPlayer.Height;
            _previousmediasize.Width = videoPlayer.Width;
            _previousmediaelementmargin = MainPage.Current.mainGrid.Margin;

            MainPage.Current.mainGrid.Margin = new Thickness();
            MainPage.Current.mainGrid.Background = new SolidColorBrush(Colors.Black);
            MainPage.Current.tintView.Visibility = Visibility.Collapsed;

            videoPlayer.Width = Window.Current.Bounds.Width;
            videoPlayer.Height = double.NaN;
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
            if (unpoint.Properties.IsRightButtonPressed) return;

            double newVolumePosition = Math.Max(Math.Min(Math.Floor(VolumeMax - unpoint.Position.Y), VolumeMax), VolumeMin);

            videoPlayer.Volume = Math.Floor(newVolumePosition) / VolumeMax;

            MyProgressHelper.Volume = newVolumePosition;

            _volumeTouchedDown = true;
            _controlsStopTimer();
        }

        private void volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (_volumeTouchedDown)
            {
                _resetTimer();

                _volumeTouchedDown = false;
            }

        }

        void volume_dragged(object sender, PointerRoutedEventArgs e)
        {
            if (_volumeTouchedDown)
            {
                PointerPoint unpoint = e.GetCurrentPoint(VolumeControlHolder);
                if (unpoint.Properties.IsRightButtonPressed) return;

                double newVolumePosition = Math.Max(Math.Min(Math.Floor(VolumeMax - unpoint.Position.Y), VolumeMax), VolumeMin);

                videoPlayer.Volume = Math.Floor(newVolumePosition) / VolumeMax;
                MyProgressHelper.Volume = newVolumePosition;



            }
        }
        #endregion

        #region Control Visible Logic

        private void hideControls()
        {
            if (isVisible)
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

                isVisible = false;


                if (Current.Opacity.ToString().Equals("0", StringComparison.Ordinal)) return;

                _controlsStopTimer();

                if (MainPage.Current.vidView.Opacity.ToString().Equals("0", StringComparison.Ordinal)) return;
                VisualStateManager.GoToState(this, "hideController", true);

                MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                pointerpressedstage = new PointerEventHandler(showControls);
                MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);
            }

        }

        private void hideControlsE(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            PointerPoint unpoint = e.GetCurrentPoint(MainPage.Current.mainGrid);

            if (unpoint.Properties.IsRightButtonPressed) return;

            var ttv = Holder.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            if ((unpoint.Position.X <= screenCoords.X + Holder.ActualWidth && unpoint.Position.X >= screenCoords.X) && (unpoint.Position.Y <= screenCoords.Y + Holder.ActualHeight && unpoint.Position.Y >= screenCoords.Y))
            {
                //_controlsStopTimer();
                //return;
            }
            else
            {
                if (isVisible)
                {

                    if (ColorPickerHolder.Opacity.ToString() == "1")
                    {
                        VisualStateManager.GoToState(this, "closeColorPicker", true);
                        if (PlayBackHolder.Opacity.ToString() != "0") _resetTimer();
                        isVisible = false;
                        isVisible = false;
                        MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                        pointerpressedstage = new PointerEventHandler(showControls);
                        MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);
                        _controlsStopTimer();
                    }
                    else
                    {

                        if (Current.Opacity.ToString().Equals("0", StringComparison.Ordinal)) return;

                        if (MainPage.Current.vidView.Opacity.ToString().Equals("0", StringComparison.Ordinal)) return;

                        VisualStateManager.GoToState(this, "hideControllerFast", true);
                        isVisible = false;
                        MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                        pointerpressedstage = new PointerEventHandler(showControls);
                        MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);
                        _controlsStopTimer();
                    }
                }

            }
        }

        private void showControls(object sender, PointerRoutedEventArgs e)
        {

            PointerPoint unpoint = e.GetCurrentPoint(MainPage.Current.mainGrid);
            
            if (unpoint.Properties.IsRightButtonPressed) return;
            if (MainPage.Current.vidView.Opacity.ToString().Equals("0", StringComparison.Ordinal)) return;
            if (ColorPickerHolder.Opacity.ToString().Equals("1", StringComparison.Ordinal)) return;

            double xPos =
                (unpoint.Position.X - videoControllerGrid.Width / 2 < Window.Current.Bounds.Left + videoControllerGrid.Width / 2) ?
                  Window.Current.Bounds.Left :
                       ((unpoint.Position.X > Window.Current.Bounds.Right - videoControllerGrid.Width / 2) ?
                     Window.Current.Bounds.Right - videoControllerGrid.Width :
                     unpoint.Position.X - videoControllerGrid.Width / 2);

            double yPos =
               (unpoint.Position.Y < Window.Current.Bounds.Top + videoControllerGrid.Height / 2) ?
                 Window.Current.Bounds.Top :
                      ((unpoint.Position.Y > Window.Current.Bounds.Bottom - videoControllerGrid.Height) ?
                    Window.Current.Bounds.Bottom - videoControllerGrid.Height - 30 :
                    unpoint.Position.Y - videoControllerGrid.Height / 2);

            if (unpoint.Position.Y < 80 && unpoint.Position.Y < 80) return;

            if (!isVisible)
            {
                videoControllerGrid.Margin = new Thickness(xPos, yPos, 0, 0);

                VisualStateManager.GoToState(this, "showController", true);

                isVisible = true;

                MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                pointerpressedstage = new PointerEventHandler(hideControlsE);
                MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);

                this._resetTimer();

            }

        }

        public void setUpControlTimer()
        {
            _controlsTimer = new DispatcherTimer();
            _controlsTimer.Interval = TimeSpan.FromMilliseconds(200);
        }

        private void _controlsStartTimer()
        {
            _controlsTimer.Tick += _controlsTimer_Tick;
            _controlsTimer.Start();
        }

        public void _controlsStopTimer()
        {
            _controlsTimer.Stop();
            _controlsTimer.Tick -= _controlsTimer_Tick;
            timesTicked = 1;

        }

        private void _resetTimer()
        {
            _controlsTimer.Stop();
            timesTicked = 1;
            _controlsTimer.Tick -= _controlsTimer_Tick;
            _controlsStartTimer();

        }

        private void _resetTimerHandler(object sender, PointerRoutedEventArgs e)
        {
            _controlsTimer.Stop();
            timesTicked = 1;
            _controlsTimer.Tick -= _controlsTimer_Tick;
            _controlsStartTimer();

        }

        private void _controlsTimer_Tick(object sender, object e)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            timesTicked++;

            if (timesTicked > timesToTick)
            {
                _controlsStopTimer();
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                if (!isVisible) return;

                if (ColorPickerHolder.Visibility.ToString() == "Visible")
                {
                    VisualStateManager.GoToState(this, "resetColorPicker", true);
                    MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                    pointerpressedstage = new PointerEventHandler(showControls);
                    MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);
                    isVisible = false;
                }
                else
                {
                    MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                    pointerpressedstage = new PointerEventHandler(showControls);
                    MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);
                    isVisible = false;

                    if (Holder.Opacity.ToString().Equals("0", StringComparison.Ordinal)) return;
                    if (isVisible) VisualStateManager.GoToState(this, "hideController", true);

                }
              
            }

        }
        #endregion

        private void Get_Pixel(object sender, PointerRoutedEventArgs e)
        {

            _isSelectingColor = true;
            PointerPoint unpoint = e.GetCurrentPoint(ColorPickerHolder);
            if (unpoint.Properties.IsRightButtonPressed) return;

            double D = Math.Sqrt(Math.Pow(_CenterX - unpoint.Position.X, 2) + Math.Pow(_CenterY - unpoint.Position.Y, 2));
            if (D <= Diameter / 5) return;

            var ttv = ColorPickerHolder.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            if (_isSelectingColor)
            {
                
                Color newColor = Win32.GetPixelColor((int)screenCoords.X + (int)unpoint.Position.X, (int)screenCoords.Y + (int)unpoint.Position.Y);
                HandleColorChange(newColor);
            
            }


        }

        private void PointerUp(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            if (MainPage.Current.vidView.Visibility.ToString() == "Collapsed")
            {
                VisualStateManager.GoToState(this, "resetColorPicker", true);
            }
            else if (_isSelectingColor)
            {
                VisualStateManager.GoToState(this, "hideColorPicker", true);
                MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedstage);
                pointerpressedstage = new PointerEventHandler(showControls);
                MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedstage, true);
            }
            _isSelectingColor = false;
            isVisible = false;
        }

        private void ReGet_Pixel(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint unpoint = e.GetCurrentPoint(ColorPickerHolder);
            if (unpoint.Properties.IsRightButtonPressed) return;

            var ttv = ColorPickerHolder.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            double D = Math.Sqrt(Math.Pow(_CenterX - unpoint.Position.X, 2) + Math.Pow(_CenterY - unpoint.Position.Y, 2));
            if (D <= Diameter / 5) return;

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


    public class PlaybackConverter : IValueConverter
    {

        public object Convert(object value,
  Type targetType,
  object parameter,
  string language)
        {



            return "4";

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
