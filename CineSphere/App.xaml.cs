using CineSphere.Common;
using CineSphere.Model;

using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace CineSphere
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static string DBPath = string.Empty;


        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Set the application background Image
                rootFrame.Background = new ImageBrush
                {
                    Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill,
                    ImageSource =
                        new BitmapImage { UriSource = new Uri("ms-appx:///Assets/background.jpg") }
                };

                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }
                // Get a reference to the SQLite database
                DBPath = Path.Combine(
                    Windows.Storage.ApplicationData.Current.LocalFolder.Path, "video.s3db");

                using (var db = new SQLiteConnection(DBPath))
                {
                    db.CreateTable<Video>();
                }
                ResetData();


                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }


        private void ResetData()
        {
            using (var db = new SQLiteConnection(DBPath))
            {

               //db.DeleteAll<Video>();


            }
        }


        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
