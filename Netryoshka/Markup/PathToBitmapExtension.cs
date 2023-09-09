using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Netty
{

    public class PathToBitmapExtension : MarkupExtension
    {
        private static readonly Dictionary<string, ImageSource> _cache = new();
        public string ImagePath { get; set; }

        public PathToBitmapExtension(string imagePath)
        {
            this.ImagePath = imagePath;
        }

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
                return null;

            if (_cache.TryGetValue(ImagePath, out var cachedImage))
            {
                return cachedImage;
            }

            var image = new BitmapImage(new Uri(ImagePath, UriKind.RelativeOrAbsolute));
            _cache[ImagePath] = image;

            return image;
        }

    }

}
