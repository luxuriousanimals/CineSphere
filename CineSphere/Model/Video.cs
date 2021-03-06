﻿using CineSphere.Common;
using CineSphere;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CineSphere.Model
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
