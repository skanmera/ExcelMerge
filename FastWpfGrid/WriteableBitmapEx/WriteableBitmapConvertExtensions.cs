#region Header
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of interchange extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author$
//   Changed on:        $Date$
//   Changed in:        $Revision$
//   Project:           $URL$
//   Id:                $Id$
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//
#endregion

using System;
using System.IO;
using System.Reflection;

#if NETFX_CORE
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Windows.UI.Xaml.Media.Imaging
#else
namespace System.Windows.Media.Imaging
#endif
{
    /// <summary>
    /// Collection of interchange extension methods for the WriteableBitmap class.
    /// </summary>
    public
#if WPF
 unsafe
#endif
 static partial class WriteableBitmapExtensions
    {
        #region Methods

        #region Byte Array

        /// <summary>
        /// Copies the Pixels from the WriteableBitmap into a ARGB byte array starting at a specific Pixels index.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="offset">The starting Pixels index.</param>
        /// <param name="count">The number of Pixels to copy, -1 for all</param>
        /// <returns>The color buffer as byte ARGB values.</returns>
        public static byte[] ToByteArray(this WriteableBitmap bmp, int offset, int count)
        {
            using (var context = bmp.GetBitmapContext())
            {
                if (count == -1)
                {
                    // Copy all to byte array
                    count = context.Length;
                }

                var len = count * SizeOfArgb;
                var result = new byte[len]; // ARGB
                BitmapContext.BlockCopy(context, offset, result, 0, len);
                return result;
            }
        }

        /// <summary>
        /// Copies the Pixels from the WriteableBitmap into a ARGB byte array.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="count">The number of pixels to copy.</param>
        /// <returns>The color buffer as byte ARGB values.</returns>
        public static byte[] ToByteArray(this WriteableBitmap bmp, int count)
        {
            return bmp.ToByteArray(0, count);
        }

        /// <summary>
        /// Copies all the Pixels from the WriteableBitmap into a ARGB byte array.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <returns>The color buffer as byte ARGB values.</returns>
        public static byte[] ToByteArray(this WriteableBitmap bmp)
        {
            return bmp.ToByteArray(0, -1);
        }

        /// <summary>
        /// Copies color information from an ARGB byte array into this WriteableBitmap starting at a specific buffer index.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="offset">The starting index in the buffer.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <param name="buffer">The color buffer as byte ARGB values.</param>
        /// <returns>The WriteableBitmap that was passed as parameter.</returns>
        public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer, int offset, int count)
        {
            using (var context = bmp.GetBitmapContext())
            {
                BitmapContext.BlockCopy(buffer, offset, context, 0, count);
                return bmp;
            }
        }

        /// <summary>
        /// Copies color information from an ARGB byte array into this WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <param name="buffer">The color buffer as byte ARGB values.</param>
        /// <returns>The WriteableBitmap that was passed as parameter.</returns>
        public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer, int count)
        {
            return bmp.FromByteArray(buffer, 0, count);
        }

        /// <summary>
        /// Copies all the color information from an ARGB byte array into this WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="buffer">The color buffer as byte ARGB values.</param>
        /// <returns>The WriteableBitmap that was passed as parameter.</returns>
        public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer)
        {
            return bmp.FromByteArray(buffer, 0, buffer.Length);
        }

        #endregion

        #region TGA File

        /// <summary>
        /// Writes the WriteableBitmap as a TGA image to a stream. 
        /// Used with permission from Nokola: http://nokola.com/blog/post/2010/01/21/Quick-and-Dirty-Output-of-WriteableBitmap-as-TGA-Image.aspx
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="destination">The destination stream.</param>
        public static void WriteTga(this WriteableBitmap bmp, Stream destination)
        {
            using (var context = bmp.GetBitmapContext())
            {
                int width = context.Width;
                int height = context.Height;
                var pixels = context.Pixels;
                byte[] data = new byte[context.Length * SizeOfArgb];

                // Copy bitmap data as BGRA
                int offsetSource = 0;
                int width4 = width << 2;
                int width8 = width << 3;
                int offsetDest = (height - 1) * width4;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Account for pre-multiplied alpha
                        int c = pixels[offsetSource];
                        var a = (byte)(c >> 24);

                        // Prevent division by zero
                        int ai = a;
                        if (ai == 0)
                        {
                            ai = 1;
                        }

                        // Scale inverse alpha to use cheap integer mul bit shift
                        ai = ((255 << 8) / ai);
                        data[offsetDest + 3] = (byte)a;                                // A
                        data[offsetDest + 2] = (byte)((((c >> 16) & 0xFF) * ai) >> 8); // R
                        data[offsetDest + 1] = (byte)((((c >> 8) & 0xFF) * ai) >> 8);  // G
                        data[offsetDest] = (byte)((((c & 0xFF) * ai) >> 8));           // B

                        offsetSource++;
                        offsetDest += SizeOfArgb;
                    }
                    offsetDest -= width8;
                }

                // Create header
                var header = new byte[]
         {
            0, // ID length
            0, // no color map
            2, // uncompressed, true color
            0, 0, 0, 0,
            0,
            0, 0, 0, 0, // x and y origin
            (byte)(width & 0x00FF),
            (byte)((width & 0xFF00) >> 8),
            (byte)(height & 0x00FF),
            (byte)((height & 0xFF00) >> 8),
            32, // 32 bit bitmap
            0 
         };

                // Write header and data
                using (var writer = new BinaryWriter(destination))
                {
                    writer.Write(header);
                    writer.Write(data);
                }
            }
        }

        #endregion

        #region Resource
#if !NETFX_CORE
      /// <summary>
      /// Loads an image from the applications resource file and returns a new WriteableBitmap. The passed WriteableBitmap is not used.
      /// </summary>
      /// <param name="bmp">The WriteableBitmap.</param>
      /// <param name="relativePath">Only the relative path to the resource file. The assembly name is retrieved automatically.</param>
      /// <returns>A new WriteableBitmap containing the pixel data.</returns>
      public static WriteableBitmap FromResource(this WriteableBitmap bmp, string relativePath)
      {
         var fullName = Assembly.GetCallingAssembly().FullName;
         var asmName = new AssemblyName(fullName).Name;
         return bmp.FromContent(asmName + ";component/" + relativePath);
      }
#endif

#if NETFX_CORE
        /// <summary>
        /// Loads an image from the applications content and returns a new WriteableBitmap. The passed WriteableBitmap is not used.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="uri">The URI to the content file.</param>
        /// <param name="pixelFormat">The pixel format of the stream data. If Unknown is provided as param, the default format of the BitmapDecoder is used.</param>
        /// <returns>A new WriteableBitmap containing the pixel data.</returns>
        public static async Task<WriteableBitmap> FromContent(this WriteableBitmap bmp, Uri uri, BitmapPixelFormat pixelFormat = BitmapPixelFormat.Unknown)
        {
            // Decode pixel data
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                return await FromStream(bmp, stream);
            }
        }

        /// <summary>
        /// Loads the data from an image stream and returns a new WriteableBitmap. The passed WriteableBitmap is not used.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="stream">The stream with the image data.</param>
        /// <param name="pixelFormat">The pixel format of the stream data. If Unknown is provided as param, the default format of the BitmapDecoder is used.</param>
        /// <returns>A new WriteableBitmap containing the pixel data.</returns>
        public static async Task<WriteableBitmap> FromStream(this WriteableBitmap bmp, Stream stream, BitmapPixelFormat pixelFormat = BitmapPixelFormat.Unknown)
        {
            using (var dstStream = new InMemoryRandomAccessStream())
            {
                await RandomAccessStream.CopyAsync(stream.AsInputStream(), dstStream);
                return await FromStream(bmp, dstStream);
            }
        }

        /// <summary>
        /// Loads the data from an image stream and returns a new WriteableBitmap. The passed WriteableBitmap is not used.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="stream">The stream with the image data.</param>
        /// <param name="pixelFormat">The pixel format of the stream data. If Unknown is provided as param, the default format of the BitmapDecoder is used.</param>
        /// <returns>A new WriteableBitmap containing the pixel data.</returns>
        public static async Task<WriteableBitmap> FromStream(this WriteableBitmap bmp, IRandomAccessStream stream, BitmapPixelFormat pixelFormat = BitmapPixelFormat.Unknown)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var transform = new BitmapTransform();
            if (pixelFormat == BitmapPixelFormat.Unknown)
            {
                pixelFormat = decoder.BitmapPixelFormat;
            }
            var pixelData = await decoder.GetPixelDataAsync(pixelFormat, decoder.BitmapAlphaMode, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
            var pixels = pixelData.DetachPixelData();

            // Copy to WriteableBitmap
            bmp = new WriteableBitmap((int)decoder.OrientedPixelWidth, (int)decoder.OrientedPixelHeight);
            using (var bmpStream = bmp.PixelBuffer.AsStream())
            {
                bmpStream.Seek(0, SeekOrigin.Begin);
                bmpStream.Write(pixels, 0, (int)bmpStream.Length);
                return bmp;
            }
        }

        /// <summary>
        /// Encodes the data from a WriteableBitmap into a stream.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="destinationStream">The stream which will take the image data.</param>
        /// <param name="encoderId">The encoder GUID to use like BitmapEncoder.JpegEncoderId etc.</param>
        public static async Task ToStream(this WriteableBitmap bmp, IRandomAccessStream destinationStream, Guid encoderId)
        {
            // Copy buffer to pixels
            byte[] pixels;
            using (var stream = bmp.PixelBuffer.AsStream())
            {
                pixels = new byte[(uint)stream.Length];
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            // Encode pixels into stream
            var encoder = await BitmapEncoder.CreateAsync(encoderId, destinationStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, 96, 96, pixels);
            await encoder.FlushAsync();
        }

        /// <summary>
        /// Encodes the data from a WriteableBitmap as JPEG into a stream.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="destinationStream">The stream which will take the JPEG image data.</param>
        public static async Task ToStreamAsJpeg(this WriteableBitmap bmp, IRandomAccessStream destinationStream)
        {
            await ToStream(bmp, destinationStream, BitmapEncoder.JpegEncoderId);
        }

        /// <summary>
        /// Loads the data from a pixel buffer like the RenderTargetBitmap provides and returns a new WriteableBitmap. The passed WriteableBitmap is not used.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="pixelBuffer">The source pixel buffer with the image data.</param>
        /// <param name="width">The width of the image data.</param>
        /// <param name="height">The height of the image data.</param>
        /// <returns>A new WriteableBitmap containing the pixel data.</returns>
        public static async Task<WriteableBitmap> FromPixelBuffer(this WriteableBitmap bmp, IBuffer pixelBuffer, int width, int height)
        {
            // Copy to WriteableBitmap
            bmp = new WriteableBitmap(width, height);
            using (var srcStream = pixelBuffer.AsStream())
            {
                using (var destStream = bmp.PixelBuffer.AsStream())
                {
                    srcStream.Seek(0, SeekOrigin.Begin);
                    await srcStream.CopyToAsync(destStream);
                }
                return bmp;
            }
        }
#else
      /// <summary>
      /// Loads an image from the applications content and returns a new WriteableBitmap. The passed WriteableBitmap is not used.
      /// </summary>
      /// <param name="bmp">The WriteableBitmap.</param>
      /// <param name="relativePath">Only the relative path to the content file.</param>
      /// <returns>A new WriteableBitmap containing the pixel data.</returns>
      public static WriteableBitmap FromContent(this WriteableBitmap bmp, string relativePath)
      {
        using (var bmpStream = Application.GetResourceStream(new Uri(relativePath, UriKind.Relative)).Stream)
         {
            return FromStream(bmp, bmpStream);
         }
      }

      /// <summary>
      /// Loads the data from an image stream and returns a new WriteableBitmap. The passed WriteableBitmap is not used.
      /// </summary>
      /// <param name="bmp">The WriteableBitmap.</param>
      /// <param name="stream">The stream with the image data.</param>
      /// <returns>A new WriteableBitmap containing the pixel data.</returns>
      public static WriteableBitmap FromStream(this WriteableBitmap bmp, Stream stream)
      {
         var bmpi = new BitmapImage();
#if SILVERLIGHT
         bmpi.SetSource(stream);
         bmpi.CreateOptions = BitmapCreateOptions.None;
#elif WPF
         bmpi.BeginInit();
         bmpi.CreateOptions = BitmapCreateOptions.None;
         bmpi.StreamSource = stream;
         bmpi.EndInit();
#endif
         bmp = new WriteableBitmap(bmpi);   
         bmpi.UriSource = null;
         return bmp;
      }
#endif

        #endregion

        #endregion
    }
}