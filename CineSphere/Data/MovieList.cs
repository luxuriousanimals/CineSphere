using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CineSphere;
using CineSphere.Common;
using CineSphere.Model;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;

using System.Diagnostics;
using Windows.Storage;

namespace CineSphere.Data
{



    public class Video : System.ComponentModel.INotifyPropertyChanged
    {

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value; OnPropertyChanged("Title");
                }
            }
        }

        private string _img = string.Empty;
        public string Img
        {
            get { return _img; }
            set
            {
                if (_img != value)
                {
                    _img = value; OnPropertyChanged("Img");
                }
            }
        }
        private string _path = string.Empty;
        public string Path
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    _path = value; OnPropertyChanged("Path");
                }
            }
        }
        private bool _rememberFullscreen;
        public bool rememberFullscreen
        {
            get { return _rememberFullscreen; }
            set
            {
                if (_rememberFullscreen != value)
                {
                    _rememberFullscreen = value; OnPropertyChanged("rememberFullscreen");
                }
            }
        }
        private int _lastPosition = 0;
        public int LastPosition
        {
            get { return _lastPosition; }
            set
            {
                if (_lastPosition != value)
                {
                    _lastPosition = value; OnPropertyChanged("LastPosition");
                }
            }
        }
        private int _size = 140;
        public int Size
        {
            get { return this._size; }
            set
            {
                if (_size != value)
                {
                    _size = value; OnPropertyChanged("Size");
                }
            }
        }

        private bool _isMRU;
        public bool isMRU
        {
            get { return this._isMRU; }
            set
            {
                if (_isMRU != value)
                {
                    _isMRU = value; OnPropertyChanged("isMRU");
                }
            }
        }



        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

    }

    public class GroupInfoList<T> : List<object>
    {

        public object Key { get; set; }

        public new IEnumerator<object> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<object>)base.GetEnumerator();
        }
    }

    public class ItemCollection : IEnumerable<Object>
    {
        private System.Collections.ObjectModel.ObservableCollection<Video> itemCollection = new System.Collections.ObjectModel.ObservableCollection<Video>();

        public IEnumerator<Object> GetEnumerator()
        {
            return itemCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return CineSphere.Data.ItemCollection.GetEnumerator();
        }

        public void Add(Video video)
        {
            itemCollection.Add(video);
        }
    }

    public class MovieList
    {
        public MovieList() {
            private ItemCollection _Collection = new ItemCollection();

            public ItemCollection Collection
                {
                    get
                    {
                        return this._Collection;
                    }
                }

            internal List<GroupInfoList<object>> GetGroupsByCategory()
                {
                    List<GroupInfoList<object>> groups = new List<GroupInfoList<object>>();

                    //var query = from item in Collection
                    //            orderby ((Video)item).Category
                    //            group item by ((Item)item).Category into g
                    //            select new { GroupName = g.Key, Items = g };
                    //foreach (var g in query)
                    //{
                    //    GroupInfoList<object> info = new GroupInfoList<object>();
                    //    info.Key = g.GroupName;
                    //    foreach (var item in g.Items)
                    //    {
                    //        info.Add(item);
                    //    }
                    //    groups.Add(info);
                    //}

                    return groups;

                }

          
        }
    

}