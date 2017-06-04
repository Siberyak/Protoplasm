using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using DevExpress.Utils;

namespace Factorio.Lua.Reader
{
    public static class ImagesHelper
    {
        public class Helper
        {
            public ImageCollection Images { get; }

            readonly Dictionary<object, int> _indexes = new Dictionary<object, int>();

            public Helper(int width, int? height = null)
            {
                Images = new ImageCollection {ImageSize = new Size(width, height ?? width) };
            }

            public int? GetIndex<TData, T>(TData data, T key, ImageLoaderDelegate<TData> loader)
            {
                return GetIndex(key, k => loader(data), _indexes, Images);
            }

            public int GetIndex(string key)
            {
                return GetIndex(key, k => LoadImage(Storage.Current, k)) ?? -1;
            }

            public int? GetIndex<T>(T key, ImageLoaderDelegate<T> loader)
            {
                return GetIndex(key, loader, _indexes, Images);
            }

            private static int? GetIndex<T>(T key, ImageLoaderDelegate<T> loader, Dictionary<object, int> indexes, ImageCollection images)
            {
                if (key == null)
                    return null;

                lock (indexes)
                {
                    if (indexes.ContainsKey(key))
                        return indexes[key];

                    var image = loader?.Invoke(key);
                    if (image == null)
                        return null;

                    var index = images.Images.Add(image);
                    indexes.Add(key, index);
                    return index;
                }
            }
        }

        public delegate Image ImageLoaderDelegate<T>(T key);

        #region 16x16

        public static Helper x16 = new Helper(16);


        public static ImageCollection Images16
        {
            get { return x16.Images; }
        }

        public static int? GetIndex16<TData, T>(TData data, T key, ImageLoaderDelegate<TData> loader)
        {
            return x16.GetIndex(data, key, loader);
        }

        public static int GetIndex16(string key)
        {
            return x16.GetIndex(key);
        }

        public static int? GetIndex16<T>(T key, ImageLoaderDelegate<T> loader)
        {
            return x16.GetIndex(key, loader);
        }

        #endregion

        #region 32x32

        public static readonly Helper x32 = new Helper(32);

        public static ImageCollection Images32
        {
            get { return x32.Images; }
        }

        public static int? GetIndex32<TData, T>(TData data, T key, ImageLoaderDelegate<TData> loader)
        {
            return x32.GetIndex(data, key, loader);
        }

        public static int GetIndex32(string key)
        {
            return x32.GetIndex(key);
        }

        public static int? GetIndex32<T>(T key, ImageLoaderDelegate<T> loader)
        {
            return x32.GetIndex(key, loader);
        }

        #endregion

        #region 64x54

        public static readonly Helper x64 = new Helper(64);

        public static ImageCollection Images64
        {
            get { return x64.Images; }
        }

        public static int? GetIndex64<TData, T>(TData data, T key, ImageLoaderDelegate<TData> loader)
        {
            return x64.GetIndex(data, key, loader);
        }

        public static int GetIndex64(string key)
        {
            return x64.GetIndex(key);
        }

        public static int? GetIndex64<T>(T key, ImageLoaderDelegate<T> loader)
        {
            return x64.GetIndex(key, loader);
        }

        #endregion


        
        
        public static Bitmap LoadImage(Storage storage, string key)
        {
            var parts = key.Split('/');

            key = parts[0].Replace("__", "");

            Mod mod;
            if (!storage.Mods.TryGetValue(key, out mod))
                return null;


            parts[0] = mod.dir;
            var path = Path.Combine(parts);
            var image = Image.FromFile(path);
            return new Bitmap(image);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            DrawImageTo(image, destImage, destRect);

            return destImage;
        }

        public static void DrawImageTo(Image image, Bitmap destImage, Rectangle destRect)
        {
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
        }

        public static Image IconLoader(Storage storage, string key)
        {
            return ImagesHelper.LoadImage(storage, key);
        }

        public static Image Image32<T>(this T key, ImageLoaderDelegate<T> loader)
        {
            var index32 = GetIndex32(key, loader);
            return index32.HasValue ? Images32.Images[index32.Value] : null;
        }

        public static Image Image32(this IIconed data)
        {
            var index32 = data.ImageIndex32();
            return index32 == -1 ? null : Images32.Images[index32];
        }

        public static Image Image32(this Recipe recipe)
        {
            var index32 = recipe.ImageIndex32();
            return index32 == -1 ? null : Images32.Images[index32];
        }

        public static Image Image32(this Item part)
        {
            var index32 = part.ImageIndex32();
            return index32 == -1 ? null : Images32.Images[index32];
        }

        public static int ImageIndex32(this IIconed data)
        {
            var imageIndex = GetIndex32(data._Icon, k => IconLoader(data.Storage, k)) ?? -1;
            return imageIndex;
        }

        public static int ImageIndex32(this Recipe data)
        {
            var imageIndex = ImagesHelper.GetIndex32(data._Icon, k => IconLoader(data.Storage, k))
                             ?? ImagesHelper.GetIndex32(data.Icons, "MixedIcon: " + data.Name, d => MixedIconLoader(data.Storage, d))
                             ?? ImagesHelper.GetIndex32(data.Icon, k => IconLoader(data.Storage, k))
                             ?? -1;
            return imageIndex;
        }

        public static int ImageIndex32(this Item data)
        {
            var imageIndex = ImagesHelper.GetIndex32(data._Icon, k => IconLoader(data.Storage, k))
                             ?? ImagesHelper.GetIndex32(data.Icons, "MixedIcon: " + data.Name, d => MixedIconLoader(data.Storage, d))
                             ?? -1;
            return imageIndex;
        }

        public static Image MixedIconLoader(Storage storage, IconInfo[] data)
        {
            if(data == null || data.Length == 0)
                return null;

            Bitmap result = new Bitmap(32, 32);
            result.MakeTransparent();

            foreach (var iconInfo in data)
            {
                var dest = new Rectangle(0, 0, 32, 32);

                var image = ImagesHelper.LoadImage(storage, iconInfo.Icon);
                image.MakeTransparent(image.GetPixel(0, 0));

                if (iconInfo.Tint != null)
                {
                    var bitmap = new Bitmap(image);
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        for (var y = 0; y < bitmap.Height; y++)
                        {
                            bitmap.SetPixel(x, y, iconInfo.Tint.Transform(bitmap.GetPixel(x, y)));
                        }
                    }
                    image = bitmap;
                }


                if (iconInfo.Scale.HasValue)
                {
                    var width = Convert.ToInt32(Math.Round(image.Width * iconInfo.Scale.Value));
                    var height = Convert.ToInt32(Math.Round(image.Height * iconInfo.Scale.Value));
                    image = ImagesHelper.ResizeImage(image, width, height);

                    dest = new Rectangle((result.Width-width)/2, (result.Height-height)/2, width, height);
                }

                if (iconInfo.Shift != null)
                {
                    if (iconInfo.Shift.Length != 2)
                    {
                    }
                    else
                    {
                        dest = new Rectangle(dest.X+Convert.ToInt32(iconInfo.Shift[0]), dest.Y + Convert.ToInt32(iconInfo.Shift[1]), dest.Width, dest.Height);
                    }
                }

                ImagesHelper.DrawImageTo(image, result, dest);
            }

            return result;
        }
    }
}