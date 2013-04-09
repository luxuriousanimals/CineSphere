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
    public class MovieList
        {

            private readonly ItemCollection _collection = new ItemCollection();

            ObservableCollection<GroupInfoCollection<Video>> groups;

            public void Add(Video video)
            {
               // Debug.WriteLine("ERERE");

                _collection.Add(video);
            }

            public void Remove(Video video)
            {

                _collection.Remove(video);
              
            }

            public void Update(Video video)
            {
                //using (var connection = new SQLiteConnection(_dbPath))
                //{
                //    var existingItem = (connection.Table<Video>().Where(
                //          v => v.Path == video.Path)).Single();

                //    existingItem.rememberFullscreen = video.rememberFullscreen;
                //    existingItem.LastPosition = video.LastPosition;

                //    connection.Update(existingItem);

                //}
            }
            public ItemCollection Collection
            {
                get
                {
                    return _collection;
                }
            }

         
            internal ObservableCollection<GroupInfoCollection<Video>> GetGroupsByCategory()
            {

                using (var db = new SQLiteConnection(App.DBPath))
                {
                    var pull = db.Table<Video>().OrderBy(v => v.Id);
                    foreach (var _video in pull)
                    {
                        var videes = new Video()
                        {
                            Id = _video.Id,
                            Title = _video.Title,
                            Img = pathToImage(_video.Img),
                            Path = _video.Path,
                            isMRU = _video.isMRU,
                            LastPosition = _video.LastPosition,
                            rememberFullscreen = _video.rememberFullscreen

                        };
                        Collection.Add(videes);
                    }
                }

                groups = new ObservableCollection<GroupInfoCollection<Video>>();

                var query = from video in Collection
                            orderby video.isMRU
                            group video by video.isMRU into g
                            select new { GroupName = g.Key.ToString(), Items = g };

                foreach (var g in query)
                {
                    GroupInfoCollection<Video> info = new GroupInfoCollection<Video>
                    {
                        Key = (g.GroupName.ToLower()=="false") ? "" : "Recently Used"
                    };

                  //  Debug.WriteLine("name " + g.GroupName);

                    foreach (Video video in g.Items)
                    {
                        info.Add(video);
                    //    Debug.WriteLine("video " + video.Title);

                    }

                    groups.Add(info);
                }

                return groups;
            }
            //public ObservableCollection<Video> GetAll(string e = null)
            //{
            //    var list = new ObservableCollection<Video>();
            //            using (var db = new SQLiteConnection(_dbPath))
            //            {

            //                if (e == null)
            //                {
            //                    var query = db.Table<Video>().OrderBy(v => v.Id);
            //                    foreach (var _video in query)
            //                    {
            //                        var videes = new Video()
            //                        {
            //                            Id = _video.Id,
            //                            Title = _video.Title,
            //                            Img = pathToImage(_video.Img),
            //                            Path = _video.Path,
            //                            LastPosition = _video.LastPosition,
            //                            rememberFullscreen = _video.rememberFullscreen

            //                        };
            //                        list.Add(videes);
            //                    }
            //                } else {
            //                    var query = db.Table<Video>().Where(v => v.Path == e);
            //                    foreach (var _video in query)
            //                    {
            //                        var videes = new Video()
            //                        {
            //                            Id = _video.Id,
            //                            Title = _video.Title,
            //                            Img = pathToImage(_video.Img),
            //                            Path = _video.Path,
            //                            LastPosition = _video.LastPosition,
            //                            rememberFullscreen = _video.rememberFullscreen
            //                        };
            //                        list.Add(videes);
            //                    }


            //                    var rest = db.Table<Video>().Where(v => v.Path != e);
            //                    foreach (var _video in rest)
            //                    {
            //                        var videes = new Video()
            //                        {
            //                            Id = _video.Id,
            //                            Title = _video.Title,
            //                            Img = pathToImage(_video.Img),
            //                            Path = _video.Path,
            //                            LastPosition = _video.LastPosition,
            //                            rememberFullscreen = _video.rememberFullscreen

            //                        };
            //                        list.Add(videes);
            //                    }

            //                }
            //            }
            //            return list;
                    
            //}


            //public ObservableCollection<VideoViewModel> GetMRU(string e)
            //{
             

            //}

            public string pathToImage(string path)
            {
                return path;
            }


        
            //public IEnumerable<IGrouping<string, VideoViewModel>> GetAllGrouped()
            //{
            //   // return GetAll().OrderBy(x => x.Title).GroupBy(x => x.CategoryName);
            //}

        }
    
}
