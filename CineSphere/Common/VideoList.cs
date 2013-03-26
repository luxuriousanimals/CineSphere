using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Storage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Videorb.Common;

namespace Videorb
{
    public class VideoList : ObservableCollection<VideoItem>
    {
        public VideoList() :base()
        {
           
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Subtitle { get; set; }
        public string Image { get; set; }
        public string Path { get; set; }
        public StorageFile File { get; set; }
        public VideoInfoHelper Meta { get; set; }

        private List<VideoItem> _Items = new List<VideoItem>();
        public List<VideoItem> Items
        {
            get
            {
                return this._Items;
            }
        }
    }

    public class VideoItem : BindableStorageFile
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Subtitle { get; set; }
        public string Image { get; set; }
        public string Path { get; set; }
        public StorageFile File { get; set; }
        public VideoInfoHelper Meta { get; set; }

    }
}
