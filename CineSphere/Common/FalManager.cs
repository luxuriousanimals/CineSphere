using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.AccessCache;
using System.Collections.ObjectModel;

namespace Videorb
{
    public class FALManager
    {
        //MainPage rootPage;
        //public event PropertyChangedEventHandler PropertyChanged;

        public FALManager() 
        {

        }



        //private void NotifyPropertyChanged(String info)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(info));
        //    }
        //}

        public void AddToList(StorageFile file)
        {
            if (file != null)
            {
                var listToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                Debug.WriteLine(listToken);
            }
        }

        private void ShowList()
        {

        }

        //private async void OpenFromList()
        //{
        //    try
        //    {
        //        if (rootPage.sampleFile != null)
        //        {
        //                if (rootPage.falToken != null)
        //                {
        //                    // Open the file via the token that was stored when adding this file into the FAL list
        //                    StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(rootPage.falToken);

        //                    // Read the file
        //                    string fileContent = await FileIO.ReadTextAsync(file);
        //                    OutputTextBlock.Text = "The file '" + file.Name + "' was opened by a stored token from the FAL list, it contains the following text:" + Environment.NewLine + Environment.NewLine + fileContent;
        //                }
        //                else
        //                {
        //                    OutputTextBlock.Text = "The FAL list is empty, please select 'Future Access List' list and click 'Add to List' to add a file to the FAL list.";
        //                }
        //        }
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        rootPage.NotifyUserFileNotExist();
        //    }
        //}

        public void CheckList()
        {
            AccessListEntryView entries = StorageApplicationPermissions.FutureAccessList.Entries;
                if (entries.Count > 0)
                {
                    foreach (AccessListEntry entry in entries)
                    {
                        Debug.WriteLine(entry + " found");
                    }

                }
                else
                {
                    Debug.WriteLine("none found");
                }
        }
    }
        

        public class FeedDataSource
        {
            private ObservableCollection<VideoList> _Feeds = new ObservableCollection<VideoList>();
            public ObservableCollection<VideoList> Feeds
            {
                get
                {
                    return this._Feeds;
                }
            }

        }

    
}
