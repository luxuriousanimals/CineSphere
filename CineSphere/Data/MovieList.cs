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

    
        public void Add(Video video)
        {
            _Collection.Add(video);

            using (var connection = new SQLiteConnection(App.DBPath))
            {
                var existingItem = (connection.Table<Video>().Where(
          v => v.Id == video.Id)).SingleOrDefault();

                if (existingItem == null)
                {
                    connection.Insert(video);
                }
            }
        }

        public void Remove(Video video)
        {

            _Collection.Remove(video);

            using (var connection = new SQLiteConnection(App.DBPath))
            {
                var existingItem = (connection.Table<Video>().Where(
              v => v.Id == video.Id)).Single();

                if (connection.Delete(existingItem) > 0)
                {

                }

            }

        }

        public void Update(Video video)
        {
         
        }
        private ItemCollection _Collection = new ItemCollection();

        public ItemCollection Collection
        {
            get
            {
                return this._Collection;
            }
        }

        internal List<GroupInfoList<Video>> GetGroupsByCategory()
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
            
            List<GroupInfoList<Video>> groups = new List<GroupInfoList<Video>>();
            
            var query = from video in Collection
                        orderby video.isMRU
                        group video by video.isMRU into g
                        select new { GroupName = g.Key.ToString(), Video = g };

            foreach (var g in query)
            {
            
                
                GroupInfoList<Video> info = new GroupInfoList<Video>
                {
                    Key = (g.GroupName.ToLower() == "false") ? "false" : "Recently Used"
                };

                foreach (Video video in g.Video)
                {
                    info.Add(video);
                }

                groups.Add(info);
            }

            return groups;

        }


        public bool exists(String videopath)
        {

            using (var connection = new SQLiteConnection(App.DBPath))
            {
                var existingItem = (connection.Table<Video>().Where(
                      v => v.Path == videopath)).SingleOrDefault();

                if (existingItem == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }


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