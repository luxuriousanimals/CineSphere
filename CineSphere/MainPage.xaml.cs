using CineSphere.Common;
using CineSphere.Data;
using CineSphere.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CineSphere
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public partial class MainPage : CineSphere.Common.LayoutAwarePage
    {

        public static MainPage Current;
        public Grid mainGrid;
        public Grid vidView;
        public Grid tintView;
        public VideoControls controls;
        public MediaElement vidPlayer;
        public MineColorHelper MyColors = new MineColorHelper();
        private PointerEventHandler pointerpressedMainGrid;
        private int selectedItems = 0;


        public bool IsFullscreen { set; get; }

        private Size _previousmediasize;

        public Size PreviousMediaSize
        {
            get { return _previousmediasize; }
            set { _previousmediasize = value; }
        }


        public SQLiteConnection db = new SQLiteConnection(App.DBPath);

        private MovieList _movieList { get; set; }

        private List<GroupInfoList<Video>> _source;


        public MainPage()
        {
            this.InitializeComponent();
            _movieList = new MovieList();
            itemGridView.ItemsSource = null;
            Current = this;
            vidView = this.videoView;
            vidPlayer = this.videoPlayer;
            mainGrid = this.MainGrid;
            tintView = this.TintView;

            mainGrid.Opacity = 1;
        
            controls = new VideoControls();
            controls.RefMaster.Visibility = Visibility.Collapsed;
            
            _source = new List<GroupInfoList<Video>>();

            tintView.DataContext = MyColors;

            MainGrid.Children.Add(controls);
            Grid.SetRowSpan(controls, 2);
            
            itemGridView.ItemClick += itemGridView_ItemClick;

            SetCollectionViewSource();

            collectionViewSource.Source = _source;

            }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
     
            // Current.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x77, 0x77, 0x77));

        }

        async Task SetCollectionViewSource()
        {

            if (_source.Count() != 0)
            {
                Debug.WriteLine("this captures");
                itemGridView.UpdateLayout();

                Debug.WriteLine(collectionViewSource.View.Count());


            }
            else
            {

                _source = _movieList.GetGroupsByCategory();

                itemGridView.SelectedItem = null;

                if (_source.Count() == 0)
                {

                    EmptyLibraryView.Visibility = Visibility.Visible;
                }
                else
                {

                    EmptyLibraryView.Visibility = Visibility.Collapsed;

                }
                
            }
        }

        async private void AddFolder(object sender, RoutedEventArgs e)
        {

            if (!ApplicationView.TryUnsnap())
            {
                VisualStateManager.GoToState(this, string.Format("Filled{0}", DisplayProperties.ResolutionScale), true);
            }

            await PickFolderAsync(false);
        }

        async private void AddFile(object sender, RoutedEventArgs e)
        {

            if (!ApplicationView.TryUnsnap())
            {
                VisualStateManager.GoToState(this, string.Format("Filled{0}", DisplayProperties.ResolutionScale), true);
            }

            await PickFileAsync(true);
        }

        public async Task PickFileAsync(bool multipleFiles)
        {

            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.FileTypeFilter.Add(".mp4");
            filePicker.FileTypeFilter.Add(".avi");
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;


            if (multipleFiles)
            {
                var files = await filePicker.PickMultipleFilesAsync();
                ShowProgressBar.Visibility = Visibility.Visible;

                foreach (StorageFile file in files)
                {
                    if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(file.Name))
                    {
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace(file.Name, file);
                    }
                    await ProcessFileSelection(file);

                }
                ShowProgressBar.Visibility = Visibility.Collapsed;

            }
            else
            {
                var file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    ShowProgressBar.Visibility = Visibility.Visible;
                    if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(file.Name))
                    {
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace(file.Name, file);
                    }

                    await ProcessFileSelection(file);
                    ShowProgressBar.Visibility = Visibility.Collapsed;

                }

            }

            await SetCollectionViewSource();

        }


        public async Task PickFolderAsync(bool multiFolder)
        {

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.Thumbnail;

            folderPicker.FileTypeFilter.Add(".mp4");
            folderPicker.FileTypeFilter.Add(".avi");
            folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(folder.Name) )
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Name, folder);
                }
                await ProcessFolderSelection(folder);

            }

            await SetCollectionViewSource();

        }

     
        public async Task ProcessFileSelection(StorageFile file)
        {

            if (_movieList.exists(file.Path)) return;


            var tn = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem);
            string x = await SaveImageLocal(tn, file.Name);


            Video video = new Video
            {
                Title = file.Name,
                Img = x,
                Path = file.Path
            };

            _movieList.Add(video);

                  

        }

        public async Task ProcessFolderSelection(StorageFolder folder)
        {

            var results = await folder.GetFilesAsync();

            ShowProgressBar.Visibility = Visibility.Visible;
            
            foreach (StorageFile file in results)
            {
                if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(file.Name) && (file.FileType.Contains("mp4") || file.FileType.Contains("avi")))
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(file.Name, file);
                    await ProcessFileSelection(file);
                }
            }
            ShowProgressBar.Visibility = Visibility.Collapsed;

        }



        public async Task<string> SaveImageLocal(Windows.Storage.FileProperties.StorageItemThumbnail thumb, string uniqueName)
        {

            var desiredName = string.Format("{0}.jpg", uniqueName);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(desiredName, CreationCollisionOption.GenerateUniqueName);

            using (var filestream = await file.OpenStreamForWriteAsync())
            {
                await thumb.AsStream().CopyToAsync(filestream);
                return new Uri(string.Format("ms-appdata:///local/{0}", file.Name), UriKind.Absolute).ToString();
            }

        }



        private async void click_goBack(object sender, RoutedEventArgs e)
        {
            if (vidPlayer.Position.TotalSeconds != 0)
            {
                var prevFile = await StorageApplicationPermissions.MostRecentlyUsedList.GetItemAsync("LastUsedFile");

                _movieList.Update(new Video { Title = prevFile.Name, Path = prevFile.Path, rememberFullscreen = controls.IsFullscreen, LastPosition = (int)vidPlayer.Position.TotalMilliseconds });

            }

            //await SetCollectionViewSource();
            if (controls.IsFullscreen) controls.FullscreenToggle();
            switchViews("Library");
            // this.Frame.Navigate(typeof(MainPage));

        }

        private void switchViews(String e)
        {
            switch (e)
            {
                case "Video":

                    VisualStateManager.GoToState(this, "OpenVideoView", true);
                    MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, controls.pointerpressedstage, true);
                    break;

                case "Library":
                    // VisualStateManager.GoToState(this, "Negative", useTransitions);
                    videoPlayer.Pause();

                    controls._controlsStopTimer();

                    VisualStateManager.GoToState(this, "CloseVideoView", true);
                    VisualStateManager.GoToState(controls, "CloseVideoView", true);
                    controls.isVisible = false;
                    controls.reset();

                    mainGrid.RemoveHandler(Control.PointerPressedEvent, controls.pointerpressedstage);

                    break;

                case "colorPicker":
                    VisualStateManager.GoToState(controls, "showColorPicker", true);

                    break;

                case "colorPickerClose":

                    VisualStateManager.GoToState(controls, "resetColorPicker", true);

                    break;
            }

        }

        public async void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = ((Video)e.ClickedItem);

            await SetMediaElementSourceAsync(item);
            switchViews("Video");
            //videoPlayer.Play();

        }

        private async Task SetMediaElementSourceAsync(Video file)
        {

            StorageFile video = await Windows.Storage.StorageFile.GetFileFromPathAsync(file.Path);
            var stream = await video.OpenAsync(Windows.Storage.FileAccessMode.Read);
            MediaControl.TrackName = video.DisplayName;
            videoPlayer.SetSource(stream, video.ContentType);
            videoPlayer.Play();
            if (file.LastPosition != 0)
            {
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, file.LastPosition);
                videoPlayer.Position = ts;
                controls.MyProgressHelper.VideoPosition = file.LastPosition;

            }

            StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace("LastUsedFile", video, "metadata");
            if (file.rememberFullscreen) controls.FullscreenOn();

        }


        private void colorPickerLibrary_show(object sender, RoutedEventArgs e)
        {




            controls.RefMaster.Margin = new Thickness(Window.Current.Bounds.Width / 2 - controls.RefMaster.Width / 2, Window.Current.Bounds.Height / 2 - controls.RefMaster.Height / 2, 0, 0);

            switchViews("colorPicker");
            bottomAppBar.IsOpen = false;

            pointerpressedMainGrid = new PointerEventHandler(hidePicker);
            MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, pointerpressedMainGrid, true);
        }

        private void hidePicker(object sender, PointerRoutedEventArgs e)
        {

            PointerPoint unpoint = e.GetCurrentPoint(MainPage.Current.mainGrid);

            if (unpoint.Properties.IsRightButtonPressed) return;

            var ttv = controls.RefHolder.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));
            if ((unpoint.Position.X <= screenCoords.X + controls.RefHolder.ActualWidth && unpoint.Position.X >= screenCoords.X) && (unpoint.Position.Y <= screenCoords.Y + controls.RefHolder.ActualHeight && unpoint.Position.Y >= screenCoords.Y))
            { }
            else
            {


                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                switchViews("colorPickerClose");
                MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, pointerpressedMainGrid);
            }

        }

        private async void RemoveFile(object sender, RoutedEventArgs e)
        {

            bottomAppBar.IsOpen = false;

            foreach (Video item in itemGridView.SelectedItems)
            {
                _movieList.Remove(item);
            }

            RemoveFileAppBarButton.Visibility = Visibility.Collapsed;
            await SetCollectionViewSource();

        }


        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count() == 1) selectedItems++;
            if (e.RemovedItems.Count() == 1) selectedItems--;

            if (selectedItems == 0)
            {
                RemoveFileAppBarButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RemoveFileAppBarButton.Visibility = Visibility.Visible;
            }

        }

        private void HandleOpenEvent(object sender, object e)
        {
            if (controls.RefMaster.Visibility.ToString() == "Collapsed") return;
            VisualStateManager.GoToState(controls, "hideController", true);
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

            Color color = Color.FromArgb(0xff, (byte)(pixel & 0x000000FF),
                         (byte)((pixel & 0x0000FF00) >> 8),
                         (byte)((pixel & 0x00FF0000) >> 16));
            return color;
        }

    }

}