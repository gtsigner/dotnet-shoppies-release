using System.Windows.Media;

namespace JpGoods.Bean
{
    public class ImgShow
    {
        private string _title;
        private ImageSource _image;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public ImageSource Image
        {
            get => _image;
            set => _image = value;
        }
    }
}