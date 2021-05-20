using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PathfindingInControlFlowGraph.Models
{
    public static class Extension
    {
        public static BitmapImage GetBitmapImage(
        this Uri imageAbsolutePath,
        BitmapCacheOption bitmapCacheOption = BitmapCacheOption.Default)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = bitmapCacheOption;
            image.UriSource = imageAbsolutePath;
            image.EndInit();

            return image;
        }
    }
}
