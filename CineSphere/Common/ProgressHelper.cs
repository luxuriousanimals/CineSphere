using System.ComponentModel;

namespace CineSphere.Common
{
    public class ProgressHelper : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private double _videoPosition;
        public double VideoPosition
        {
            get { return _videoPosition; }
            set
            {
                _videoPosition = value;
                NotifyPropertyChanged("VideoPosition");

            }
        }


        private double _volume;
        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                NotifyPropertyChanged("Volume");

            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                NotifyPropertyChanged("Progress");

            }
        }


        private double _currentPlaybackRate;
        public double CurrentPlaybackRate
        {
            get { return _currentPlaybackRate; }
            set
            {
                _currentPlaybackRate = value;
                NotifyPropertyChanged("CurrentPlaybackRate");

            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
