﻿using System;
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
        private static readonly string _dbPath = App.DBPath;
        // public ObservableCollection<VideoViewModel> list = new ObservableCollection<VideoViewModel>();


        public void Add(Video video)
        {
            using (var connection = new SQLiteConnection(_dbPath))
            {
                var existingItem = (connection.Table<Video>().Where(
          v => v.Id == video.Id)).SingleOrDefault();

                if (existingItem == null)
                {
                    connection.Insert(video);
                }
                else
                {
                    //update logic  
                }


            }
        }

        public async void Remove(Video video)
        {
            string result;
            using (var connection = new SQLiteConnection(_dbPath))
            {
                var existingItem = (connection.Table<Video>().Where(
              v => v.Id == video.Id)).Single();

                if (connection.Delete(existingItem) > 0)
                {
                    result = "Success";


                }
                else
                {
                    result = "This project was not removed";
                }

            }
        }

        public void Update(Video video)
        {
            using (var connection = new SQLiteConnection(_dbPath))
            {
                var existingItem = (connection.Table<Video>().Where(
                      v => v.Path == video.Path)).Single();

                existingItem.rememberFullscreen = video.rememberFullscreen;
                existingItem.LastPosition = video.LastPosition;

                connection.Update(existingItem);

            }
        }

        public ObservableCollection<Video> GetAll(string e = null)
        {
            var list = new ObservableCollection<Video>();
            using (var db = new SQLiteConnection(_dbPath))
            {

                if (e == null)
                {
                    var query = db.Table<Video>().OrderBy(v => v.Id);
                    foreach (var _video in query)
                    {
                        var videes = new Video()
                        {
                            Id = _video.Id,
                            Title = _video.Title,
                            Img = pathToImage(_video.Img),
                            Path = _video.Path,
                            LastPosition = _video.LastPosition,
                            rememberFullscreen = _video.rememberFullscreen

                        };
                        list.Add(videes);
                    }
                }
                else
                {
                    var query = db.Table<Video>().Where(v => v.Path == e);
                    foreach (var _video in query)
                    {
                        var videes = new Video()
                        {
                            Id = _video.Id,
                            Title = _video.Title,
                            Img = pathToImage(_video.Img),
                            Path = _video.Path,
                            LastPosition = _video.LastPosition,
                            rememberFullscreen = _video.rememberFullscreen
                        };
                        list.Add(videes);
                    }


                    var rest = db.Table<Video>().Where(v => v.Path != e);
                    foreach (var _video in rest)
                    {
                        var videes = new Video()
                        {
                            Id = _video.Id,
                            Title = _video.Title,
                            Img = pathToImage(_video.Img),
                            Path = _video.Path,
                            LastPosition = _video.LastPosition,
                            rememberFullscreen = _video.rememberFullscreen

                        };
                        list.Add(videes);
                    }

                }
            }
            return list;

        }

        public bool exists(String videopath)
        {

            using (var connection = new SQLiteConnection(_dbPath))
            {
                var existingItem = (connection.Table<Video>().Where(
                      v => v.Path == videopath)).SingleOrDefault();

               if (existingItem == null)
                {
                    return false;
                }
                else {
                    return true;
                }

            }
        }


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