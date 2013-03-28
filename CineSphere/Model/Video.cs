using CineSphere.Common;
using CineSphere;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CineSphere.Model
{


    public class Video
    {
        public MineColorHelper MyColors = new MineColorHelper();

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Subtitle { get; set; }
        public string Img { get; set; }
        public string Path { get; set; }
        public string Meta { get; set; }
        public bool rememberFullscreen { get; set; }
        public int LastPosition { get; set; }
    }
}
