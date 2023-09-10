using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace Netryoshka
{

    public class PathToImageSourceExtension : MarkupExtension
    {
        private static readonly Dictionary<string, ImageSource> _cache = new();
        public string ImagePath { get; set; }

        public PathToImageSourceExtension(string imagePath)
        {
            this.ImagePath = imagePath;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ImageSource image;
            if (_cache.TryGetValue(ImagePath, out var cachedImage))
            {
                image = cachedImage;
            }
            else
            {
                image = new BitmapImage(new Uri(ImagePath, UriKind.RelativeOrAbsolute));
                _cache[ImagePath] = image;
            }

            var imgIcon = new ImageIcon
            {
                Source = image
            };


            return imgIcon;
        }

    }

}
