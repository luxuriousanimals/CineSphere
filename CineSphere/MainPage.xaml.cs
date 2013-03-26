using CineSphere.Common;
using CineSphere.Data;
using CineSphere.Model;
using CineSphere.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
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
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        //public ObservableCollection<VideoViewModel> videosGrid = null;
        //public VideoViewModel video = null;

        public bool IsFullscreen { set; get; }

        private Size _previousmediasize;

        public Size PreviousMediaSize
        {
            get { return _previousmediasize; }
            set { _previousmediasize = value; }
        }


        public SQLiteConnection db = new SQLiteConnection(App.DBPath);

        private MovieList _movieList { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            _movieList = new MovieList();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Current = this;
            vidView = this.videoView;
            vidPlayer = this.videoPlayer;
            mainGrid = this.MainGrid;
            tintView = this.TintView;

            //video = new VideosViewModel();
            //videosGrid = video.GetVideos();
            SetCollectionViewSource();

            base.OnNavigatedTo(e);
            itemGridView.ItemClick += itemGridView_ItemClick;

            //Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Cross, 0);
            //  itemGridView.PointerEntered += HoverOn;


            controls = new VideoControls();

            tintView.DataContext = MyColors;

            videoView.Children.Add(controls);

            // Current.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x77, 0x77, 0x77));

        }

        async Task SetCollectionViewSource()
        {
            IStorageItem mru = null;
            if (StorageApplicationPermissions.MostRecentlyUsedList.ContainsItem("LastUsedFile"))
            {
                mru = await StorageApplicationPermissions.MostRecentlyUsedList.GetItemAsync("LastUsedFile");
                // mruGridView.ItemsSource = _movieList.GetMRU(mru.Path);
                //mruGridView.SelectedItem = null;
                itemGridView.ItemsSource = _movieList.GetAll(mru.Path);
            }
            else {
                itemGridView.ItemsSource = _movieList.GetAll();
            }

            itemGridView.SelectedItem = null;

            if (_movieList.GetAll().Count() == 0)
            {
                EmptyLibraryView.Visibility = Visibility.Visible;
            }
            else {
                EmptyLibraryView.Visibility = Visibility.Collapsed;
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





        public async Task PickFileAsync(bool multiplieFiles)
        {

            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.FileTypeFilter.Add(".mp4");
            filePicker.FileTypeFilter.Add(".avi");
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            if (multiplieFiles)
            {
                var files = await filePicker.PickMultipleFilesAsync();

                foreach (StorageFile file in files)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(file.Name, file);
                    await ProcessFileSelection(file);
                }
            }
            else
            {
                var file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(file.Name, file);
                    await ProcessFileSelection(file);
                }
            }

            SetCollectionViewSource();

        }


        public async Task PickFolderAsync(bool multiFolder)
        {

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.Thumbnail;

            folderPicker.FileTypeFilter.Add(".mp4");
            folderPicker.FileTypeFilter.Add(".avi");
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {

                StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Path, folder);
                ProcessFolderSelection(folder);
            }

            SetCollectionViewSource();

        }

        public async Task ProcessFileSelection(StorageFile file)
        {


            var tn = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
            string x = await SaveImageLocal(tn, file.Name);



            _movieList.Add(new Video { Title = file.Name, Subtitle = file.FileType, Img = x, Path = file.Path });
        }

        public void ProcessFolderSelection(StorageFolder folder)
        {

        }



        public async Task<string> SaveImageLocal(Windows.Storage.FileProperties.StorageItemThumbnail thumb, string uniqueName)
        {

            var desiredName = string.Format("{0}.jpg", uniqueName);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting);

            using (var filestream = await file.OpenStreamForWriteAsync())
            {
                await thumb.AsStream().CopyToAsync(filestream);
                return new Uri(string.Format("ms-appdata:///local/{0}.jpg", uniqueName), UriKind.Absolute).ToString();
            }

        }



        private void click_goBack(object sender, RoutedEventArgs e)
        {
            if (controls.IsFullscreen) controls.FullscreenToggle();
            switchViews("Library");

        }

        private void switchViews(String e)
        {
            switch (e)
            {
                case "Video":
                    //VisualStateManager.GoToState(this, "Negative", useTransitions);
                    itemGridView.Visibility = Visibility.Collapsed;
                    pageTitle.Visibility = Visibility.Collapsed;

                    videoView.Visibility = Visibility.Visible;
                    backButton.Visibility = Visibility.Visible;
                    MainPage.Current.mainGrid.AddHandler(Control.PointerPressedEvent, controls.pointerpressedstage, true);

                    break;

                case "Library":
                    // VisualStateManager.GoToState(this, "Negative", useTransitions);
                    videoPlayer.Stop();

                    itemGridView.Visibility = Visibility.Visible;
                    pageTitle.Visibility = Visibility.Visible;

                    videoView.Visibility = Visibility.Collapsed;
                    backButton.Visibility = Visibility.Collapsed;
                    MainPage.Current.mainGrid.RemoveHandler(Control.PointerPressedEvent, controls.pointerpressedstage);

                    break;
            }

        }

        public async void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {


            var item = ((VideoViewModel)e.ClickedItem);
            await SetMediaElementSourceAsync(item);
            switchViews("Video");

        }

        private async Task SetMediaElementSourceAsync(VideoViewModel file)
        {

            StorageFile video = await Windows.Storage.StorageFile.GetFileFromPathAsync(file.Path);
            var stream = await video.OpenAsync(Windows.Storage.FileAccessMode.Read);
            MediaControl.TrackName = video.DisplayName;
            videoPlayer.SetSource(stream, video.ContentType);

            videoPlayer.Play();

            Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace("LastUsedFile", video, "metadata");

        }
        private void HoverOn(object sender, PointerRoutedEventArgs e)
        {
            //(sender as GridViewItem).Opacity = 0;
            //(sender as Panel).Children[0].Visibility = Visibility.Collapsed;
            Debug.WriteLine("pointer  " + (sender as GridView).Name);
        }

        private void colorPickerLibrary_show(object sender, RoutedEventArgs e)
        {
            //
        }

        private void RemoveFile(object sender, RoutedEventArgs e)
        {

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