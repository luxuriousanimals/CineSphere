
namespace CineSphere.Data

{
    using CineSphere;
    using CineSphere.Common;
    using CineSphere.Model;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;



    public class ItemCollection : IEnumerable<Video>
    {
        private static readonly string _dbPath = App.DBPath;

        private System.Collections.ObjectModel.ObservableCollection<Video> _itemCollection = new System.Collections.ObjectModel.ObservableCollection<Video>();

        public IEnumerator<Video> GetEnumerator()
        {
            return _itemCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Video video)
        {
            _itemCollection.Add(video);

          //  using (var connection = new SQLiteConnection(_dbPath))
          //  {
          //      var existingItem = (connection.Table<Video>().Where(
          //v => v.Id == video.Id)).SingleOrDefault();

          //      if (existingItem == null)
          //      {
          //          connection.Insert(video);
          //      }
          //      else
          //      {
          //          //update logic  
          //      }


          //  }

        }

        public void Remove(Video video)
        {

            _itemCollection.Remove(video);
            Debug.WriteLine("DONE");
        //    string result;
        //    using (var connection = new SQLiteConnection(_dbPath))
        //    {
        //        var existingItem = (connection.Table<Video>().Where(
        //      v => v.Id == video.Id)).Single();

        //        if (connection.Delete(existingItem) > 0)
        //        {
        //            result = "Success";


        //        }
        //        else
        //        {
        //            result = "This project was not removed";
        //        }

        //    }
        }
        
    }
}
