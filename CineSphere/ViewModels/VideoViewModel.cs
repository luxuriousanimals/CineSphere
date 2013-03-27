using Windows.Storage;
namespace CineSphere.ViewModels
{
    public class VideoViewModel : ViewModelBase
    {
        #region Properties

        private int id = 0;
        public int Id
        {
            get
            { return id; }

            set
            {
                if (id == value)
                { return; }

                id = value;
                RaisePropertyChanged("Id");
            }
        }

        private string title = string.Empty;
        public string Title
        {
            get
            { return title; }

            set
            {
                if (title == value)
                { return; }

                title = value;
                isDirty = true;
                RaisePropertyChanged("Title");
            }
        }

        private string description = string.Empty;
        public string Description
        {
            get
            { return description; }

            set
            {
                if (description == value)
                { return; }

                description = value;
                isDirty = true;
                RaisePropertyChanged("Description");
            }
        }

        private string subtitle = string.Empty;
        public string Subtitle
        {
            get
            { return subtitle; }

            set
            {
                if (subtitle == value)
                { return; }

                subtitle = value;
                isDirty = true;
                RaisePropertyChanged("Subtitle");
            }
        }

        private string image = string.Empty;
        public string Img
        {
            get
            { return image; }

            set
            {
                if (image == value)
                { return; }

                image = value;
                isDirty = true;
                RaisePropertyChanged("Image");
            }
        }

        private string path = string.Empty;
        public string Path
        {
            get
            { return path; }

            set
            {
                if (path == value)
                { return; }

                path = value;
                isDirty = true;
                RaisePropertyChanged("Path");
            }
        }

        public StorageFile File { get; set; }
        
        private string meta = string.Empty;
        public string Meta
        {
            get
            { return meta; }

            set
            {
                if (meta == value)
                { return; }

                meta = value;
                isDirty = true;
                RaisePropertyChanged("Meta");
            }
        }
      
        private bool isDirty = false;
        public bool IsDirty
        {
            get
            {
                return isDirty;
            }

            set
            {
                isDirty = value;
                RaisePropertyChanged("IsDirty");
            }
        }


        private bool _isMRU = false;
        public bool isMRU
        {
            get
            {
                return _isMRU;
            }

            set
            {
                _isMRU = value;
                RaisePropertyChanged("isMRU");
            }
        }

        private int _size = 0;
        public int Size
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;
                RaisePropertyChanged("Size");
            }
        }

        #endregion "Properties"

        //public VideoViewModel GetVideo(int videoId)
        //{
        //    var video = new VideoViewModel();
        //    using (var db = new SQLiteConnection(App.DBPath))
        //    {
        //        var _video = (db.Table<Video>().Where(
        //            v => v.Id == videoId)).Single();
        //        video.Id = _video.Id;
               
        //    }
        //    return video;
        //}

        //public string SaveVideo(VideoViewModel video)
        //{
        //    string result = string.Empty;
        //    using (var db = new SQLiteConnection(App.DBPath))
        //    {
        //        Debug.WriteLine("here");
        //        try
        //        {
        //            var existingVideo = (db.Table<Video>().Where(
        //                v => v.Id == video.Id)).SingleOrDefault();

        //            if (existingVideo != null)
        //            {

        //                int success = db.Update(existingVideo);
        //            }
        //            else
        //            {
        //                Debug.WriteLine("no here");

        //                int success = db.Insert(new Video()
        //                {
                            
        //                });
        //            }
        //                result = "Success";
        //        }
        //        catch
        //        {
        //            result = "This video was not saved.";
        //        }
        //    }
        //    return result;
        //}

        //public string DeleteVideo(int videoId)
        //{
        //    string result = string.Empty;
        //    using (var db = new SQLiteConnection(App.DBPath))
        //    {
              
        //    }
        //    return result;
        //}

    }
}
