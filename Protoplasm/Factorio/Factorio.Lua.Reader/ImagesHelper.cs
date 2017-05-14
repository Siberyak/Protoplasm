using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DevExpress.Utils;

namespace Factorio.Lua.Reader
{
    public class ImagesHelper
    {
        public static readonly ImageCollection Images32 = new ImageCollection() { ImageSize = new Size(32, 32) };
        public static readonly ImageCollection Images64 = new ImageCollection() { ImageSize = new Size(64, 64) };

        private static readonly Dictionary<object, int> _indexes32 = new Dictionary<object, int>();
        private static readonly Dictionary<object, int> _indexes64 = new Dictionary<object, int>();

        public delegate Image ImageLoaderDelegate<T>(T key);

        public static int GetIndex32<T>(T key, ImageLoaderDelegate<T> loader)
        {
            return GetIndex(key, loader, _indexes32, Images32);
        }
        public static int GetIndex64<T>(T key, ImageLoaderDelegate<T> loader)
        {
            return GetIndex(key, loader, _indexes64, Images64);
        }

        private static int GetIndex<T>(T key, ImageLoaderDelegate<T> loader, Dictionary<object, int> indexes, ImageCollection images)
        {
            if (key == null)
                return -1;

            lock (indexes)
            {
                if (indexes.ContainsKey(key))
                    return indexes[key];

                var image = loader?.Invoke(key);
                var index = -1;
                if (image != null)
                {
                    index = images.Images.Add(image);
                }

                indexes.Add(key, index);
                return index;
            }
        }

        public static Image LoadImage(Storage storage, string key)
        {
            var parts = key.Split('/');

            key = parts[0].Replace("__", "");

            Mod mod;
            if (!storage.Mods.TryGetValue(key, out mod))
                return null;


            parts[0] = mod.dir;
            var path = Path.Combine(parts);
            var image = System.Drawing.Image.FromFile(path);
            return image;
        }
    }
}