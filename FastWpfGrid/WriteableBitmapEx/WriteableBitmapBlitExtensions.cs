#region Header
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of blit (copy) extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author$
//   Changed on:        $Date$
//   Changed in:        $Revision$
//   Project:           $URL$
//   Id:                $Id$
//
//
//   Copyright © 2009-2015 Bill Reiss, Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//
#endregion

using System;

#if NETFX_CORE
using Windows.Foundation;

namespace Windows.UI.Xaml.Media.Imaging
#else
namespace System.Windows.Media.Imaging
#endif
{
    /// <summary>
    /// Collection of blit (copy) extension methods for the WriteableBitmap class.
    /// </summary>
    public
#if WPF
    unsafe
#endif
 static partial class WriteableBitmapExtensions
    {
        private const int WhiteR = 255;
        private const int WhiteG = 255;
        private const int WhiteB = 255;

        #region Enum

        /// <summary>
        /// The blending mode.
        /// </summary>
        public enum BlendMode
        {
            /// <summary>
            /// Alpha blendiing uses the alpha channel to combine the source and destination. 
            /// </summary>
            Alpha,

            /// <summary>
            /// Additive blending adds the colors of the source and the destination.
            /// </summary>
            Additive,

            /// <summary>
            /// Subtractive blending subtracts the source color from the destination.
            /// </summary>
            Subtractive,

            /// <summary>
            /// Uses the source color as a mask.
            /// </summary>
            Mask,

            /// <summary>
            /// Multiplies the source color with the destination color.
            /// </summary>
            Multiply,

            /// <summary>
            /// Ignores the specified Color
            /// </summary>
            ColorKeying,

            /// <summary>
            /// No blending just copies the pixels from the source.
            /// </summary>
            None
        }

        #endregion

        #region Methods

        /// <summary>
        /// Copies (blits) the pixels from the WriteableBitmap source to the destination WriteableBitmap (this).
        /// </summary>
        /// <param name="bmp">The destination WriteableBitmap.</param>
        /// <param name="destRect">The rectangle that defines the destination region.</param>
        /// <param name="source">The source WriteableBitmap.</param>
        /// <param name="sourceRect">The rectangle that will be copied from the source to the destination.</param>
        /// <param name="blendMode">The blending mode <see cref="BlendMode"/>.</param>
        public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect, BlendMode blendMode)
        {
            Blit(bmp, destRect, source, sourceRect, Colors.White, blendMode);
        }

        /// <summary>
        /// Copies (blits) the pixels from the WriteableBitmap source to the destination WriteableBitmap (this).
        /// </summary>
        /// <param name="bmp">The destination WriteableBitmap.</param>
        /// <param name="destRect">The rectangle that defines the destination region.</param>
        /// <param name="source">The source WriteableBitmap.</param>
        /// <param name="sourceRect">The rectangle that will be copied from the source to the destination.</param>
        public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect)
        {
            Blit(bmp, destRect, source, sourceRect, Colors.White, BlendMode.Alpha);
        }

        /// <summary>
        /// Copies (blits) the pixels from the WriteableBitmap source to the destination WriteableBitmap (this).
        /// </summary>
        /// <param name="bmp">The destination WriteableBitmap.</param>
        /// <param name="destPosition">The destination position in the destination bitmap.</param>
        /// <param name="source">The source WriteableBitmap.</param>
        /// <param name="sourceRect">The rectangle that will be copied from the source to the destination.</param>
        /// <param name="color">If not Colors.White, will tint the source image. A partially transparent color and the image will be drawn partially transparent.</param>
        /// <param name="blendMode">The blending mode <see cref="BlendMode"/>.</param>
        public static void Blit(this WriteableBitmap bmp, Point destPosition, WriteableBitmap source, Rect sourceRect, Color color, BlendMode blendMode)
        {
            var destRect = new Rect(destPosition, new Size(sourceRect.Width, sourceRect.Height));
            Blit(bmp, destRect, source, sourceRect, color, blendMode);
        }

        /// <summary>
        /// Copies (blits) the pixels from the WriteableBitmap source to the destination WriteableBitmap (this).
        /// </summary>
        /// <param name="bmp">The destination WriteableBitmap.</param>
        /// <param name="destRect">The rectangle that defines the destination region.</param>
        /// <param name="source">The source WriteableBitmap.</param>
        /// <param name="sourceRect">The rectangle that will be copied from the source to the destination.</param>
        /// <param name="color">If not Colors.White, will tint the source image. A partially transparent color and the image will be drawn partially transparent. If the BlendMode is ColorKeying, this color will be used as color key to mask all pixels with this value out.</param>
        /// <param name="blendMode">The blending mode <see cref="BlendMode"/>.</param>
        internal static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect, Color color, BlendMode blendMode)
        {
            if (color.A == 0)
            {
                return;
            }
#if WPF
            var isPrgba = source.Format == PixelFormats.Pbgra32 || source.Format == PixelFormats.Prgba64 || source.Format == PixelFormats.Prgba128Float;
#endif
            var dw = (int)destRect.Width;
            var dh = (int)destRect.Height;

            using (var srcContext = source.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                using (var destContext = bmp.GetBitmapContext())
                {
                    var sourceWidth = srcContext.Width;
                    var dpw = destContext.Width;
                    var dph = destContext.Height;

                    var intersect = new Rect(0, 0, dpw, dph);
                    intersect.Intersect(destRect);
                    if (intersect.IsEmpty)
                    {
                        return;
                    }

                    var sourcePixels = srcContext.Pixels;
                    var destPixels = destContext.Pixels;
                    var sourceLength = srcContext.Length;

                    int sourceIdx = -1;
                    int px = (int)destRect.X;
                    int py = (int)destRect.Y;

                    int x;
                    int y;
                    int idx;
                    double ii;
                    double jj;
                    int sr = 0;
                    int sg = 0;
                    int sb = 0;
                    int dr, dg, db;
                    int sourcePixel;
                    int sa = 0;
                    int da;
                    int ca = color.A;
                    int cr = color.R;
                    int cg = color.G;
                    int cb = color.B;
                    bool tinted = color != Colors.White;
                    var sw = (int)sourceRect.Width;
                    var sdx = sourceRect.Width / destRect.Width;
                    var sdy = sourceRect.Height / destRect.Height;
                    int sourceStartX = (int)sourceRect.X;
                    int sourceStartY = (int)sourceRect.Y;
                    int lastii, lastjj;
                    lastii = -1;
                    lastjj = -1;
                    jj = sourceStartY;
                    y = py;
                    for (int j = 0; j < dh; j++)
                    {
                        if (y >= 0 && y < dph)
                        {
                            ii = sourceStartX;
                            idx = px + y * dpw;
                            x = px;
                            sourcePixel = sourcePixels[0];

                            // Scanline BlockCopy is much faster (3.5x) if no tinting and blending is needed,
                            // even for smaller sprites like the 32x32 particles. 
                            if (blendMode == BlendMode.None && !tinted)
                            {
                                sourceIdx = (int)ii + (int)jj * sourceWidth;
                                var offset = x < 0 ? -x : 0;
                                var xx = x + offset;
                                var wx = sourceWidth - offset;
                                var len = xx + wx < dpw ? wx : dpw - xx;
                                if (len > sw) len = sw;
                                if (len > dw) len = dw;
                                BitmapContext.BlockCopy(srcContext, (sourceIdx + offset) * 4, destContext, (idx + offset) * 4, len * 4);
                            }

                     // Pixel by pixel copying
                            else
                            {
                                for (int i = 0; i < dw; i++)
                                {
                                    if (x >= 0 && x < dpw)
                                    {
                                        if ((int)ii != lastii || (int)jj != lastjj)
                                        {
                                            sourceIdx = (int)ii + (int)jj * sourceWidth;
                                            if (sourceIdx >= 0 && sourceIdx < sourceLength)
                                            {
                                                sourcePixel = sourcePixels[sourceIdx];
                                                sa = ((sourcePixel >> 24) & 0xff);
                                                sr = ((sourcePixel >> 16) & 0xff);
                                                sg = ((sourcePixel >> 8) & 0xff);
                                                sb = ((sourcePixel) & 0xff);
                                                if (tinted && sa != 0)
                                                {
                                                    sa = (((sa * ca) * 0x8081) >> 23);
                                                    sr = ((((((sr * cr) * 0x8081) >> 23) * ca) * 0x8081) >> 23);
                                                    sg = ((((((sg * cg) * 0x8081) >> 23) * ca) * 0x8081) >> 23);
                                                    sb = ((((((sb * cb) * 0x8081) >> 23) * ca) * 0x8081) >> 23);
                                                    sourcePixel = (sa << 24) | (sr << 16) | (sg << 8) | sb;
                                                }
                                            }
                                            else
                                            {
                                                sa = 0;
                                            }
                                        }
                                        if (blendMode == BlendMode.None)
                                        {
                                            destPixels[idx] = sourcePixel;
                                        }
                                        else if (blendMode == BlendMode.ColorKeying)
                                        {
                                            sr = ((sourcePixel >> 16) & 0xff);
                                            sg = ((sourcePixel >> 8) & 0xff);
                                            sb = ((sourcePixel) & 0xff);

                                            if (sr != color.R || sg != color.G || sb != color.B)
                                            {
                                                destPixels[idx] = sourcePixel;
                                            }

                                        }
                                        else if (blendMode == BlendMode.Mask)
                                        {
                                            int destPixel = destPixels[idx];
                                            da = ((destPixel >> 24) & 0xff);
                                            dr = ((destPixel >> 16) & 0xff);
                                            dg = ((destPixel >> 8) & 0xff);
                                            db = ((destPixel) & 0xff);
                                            destPixel = ((((da * sa) * 0x8081) >> 23) << 24) |
                                                        ((((dr * sa) * 0x8081) >> 23) << 16) |
                                                        ((((dg * sa) * 0x8081) >> 23) << 8) |
                                                        ((((db * sa) * 0x8081) >> 23));
                                            destPixels[idx] = destPixel;
                                        }
                                        else if (sa > 0)
                                        {
                                            int destPixel = destPixels[idx];
                                            da = ((destPixel >> 24) & 0xff);
                                            if ((sa == 255 || da == 0) &&
                                                           blendMode != BlendMode.Additive
                                                           && blendMode != BlendMode.Subtractive
                                                           && blendMode != BlendMode.Multiply
                                               )
                                            {
                                                destPixels[idx] = sourcePixel;
                                            }
                                            else
                                            {
                                                dr = ((destPixel >> 16) & 0xff);
                                                dg = ((destPixel >> 8) & 0xff);
                                                db = ((destPixel) & 0xff);
                                                if (blendMode == BlendMode.Alpha)
                                                {
                                                    var isa = 255 - sa;
#if NETFX_CORE
                                                     // Special case for WinRT since it does not use pARGB (pre-multiplied alpha)
                                                     destPixel = ((da & 0xff) << 24) |
                                                                 ((((sr * sa + isa * dr) >> 8) & 0xff) << 16) |
                                                                 ((((sg * sa + isa * dg) >> 8) & 0xff) <<  8) |
                                                                  (((sb * sa + isa * db) >> 8) & 0xff);
#elif WPF
                                                    if (isPrgba)
                                                    {
                                                        destPixel = ((da & 0xff) << 24) |
                                                                    (((((sr << 8) + isa * dr) >> 8) & 0xff) << 16) |
                                                                    (((((sg << 8) + isa * dg) >> 8) & 0xff) <<  8) |
                                                                     ((((sb << 8) + isa * db) >> 8) & 0xff);
                                                    }
                                                    else
                                                    {
                                                        destPixel = ((da & 0xff) << 24) |
                                                                    (((((sr * sa) + isa * dr) >> 8) & 0xff) << 16) |
                                                                    (((((sg * sa) + isa * dg) >> 8) & 0xff) <<  8) |
                                                                     ((((sb * sa) + isa * db) >> 8) & 0xff);
                                                    }
#else
                                                        destPixel = ((da & 0xff) << 24) |
                                                                    (((((sr << 8) + isa * dr) >> 8) & 0xff) << 16) |
                                                                    (((((sg << 8) + isa * dg) >> 8) & 0xff) <<  8) |
                                                                     ((((sb << 8) + isa * db) >> 8) & 0xff);
#endif
                                                }
                                                else if (blendMode == BlendMode.Additive)
                                                {
                                                    int a = (255 <= sa + da) ? 255 : (sa + da);
                                                    destPixel = (a << 24) |
                                                       (((a <= sr + dr) ? a : (sr + dr)) << 16) |
                                                       (((a <= sg + dg) ? a : (sg + dg)) << 8) |
                                                       (((a <= sb + db) ? a : (sb + db)));
                                                }
                                                else if (blendMode == BlendMode.Subtractive)
                                                {
                                                    int a = da;
                                                    destPixel = (a << 24) |
                                                       (((sr >= dr) ? 0 : (sr - dr)) << 16) |
                                                       (((sg >= dg) ? 0 : (sg - dg)) << 8) |
                                                       (((sb >= db) ? 0 : (sb - db)));
                                                }
                                                else if (blendMode == BlendMode.Multiply)
                                                {
                                                    // Faster than a division like (s * d) / 255 are 2 shifts and 2 adds
                                                    int ta = (sa * da) + 128;
                                                    int tr = (sr * dr) + 128;
                                                    int tg = (sg * dg) + 128;
                                                    int tb = (sb * db) + 128;

                                                    int ba = ((ta >> 8) + ta) >> 8;
                                                    int br = ((tr >> 8) + tr) >> 8;
                                                    int bg = ((tg >> 8) + tg) >> 8;
                                                    int bb = ((tb >> 8) + tb) >> 8;

                                                    destPixel = (ba << 24) |
                                                                ((ba <= br ? ba : br) << 16) |
                                                                ((ba <= bg ? ba : bg) << 8) |
                                                                ((ba <= bb ? ba : bb));
                                                }

                                                destPixels[idx] = destPixel;
                                            }
                                        }
                                    }
                                    x++;
                                    idx++;
                                    ii += sdx;
                                }
                            }
                        }
                        jj += sdy;
                        y++;
                    }
                }
            }
        }

        public static void Blit(BitmapContext destContext, int dpw, int dph, Rect destRect, BitmapContext srcContext, Rect sourceRect, int sourceWidth)
        {
            const BlendMode blendMode = BlendMode.Alpha;

            int dw = (int)destRect.Width;
            int dh = (int)destRect.Height;

            Rect intersect = new Rect(0, 0, dpw, dph);
            intersect.Intersect(destRect);
            if (intersect.IsEmpty)
            {
                return;
            }
#if WPF
            var isPrgba = srcContext.Format == PixelFormats.Pbgra32 || srcContext.Format == PixelFormats.Prgba64 || srcContext.Format == PixelFormats.Prgba128Float;
#endif

            var sourcePixels = srcContext.Pixels;
            var destPixels = destContext.Pixels;
            int sourceLength = srcContext.Length;
            int sourceIdx = -1;
            int px = (int)destRect.X;
            int py = (int)destRect.Y;
            int x;
            int y;
            int idx;
            double ii;
            double jj;
            int sr = 0;
            int sg = 0;
            int sb = 0;
            int dr, dg, db;
            int sourcePixel;
            int sa = 0;
            int da;

            var sw = (int)sourceRect.Width;
            var sdx = sourceRect.Width / destRect.Width;
            var sdy = sourceRect.Height / destRect.Height;
            int sourceStartX = (int)sourceRect.X;
            int sourceStartY = (int)sourceRect.Y;
            int lastii, lastjj;
            lastii = -1;
            lastjj = -1;
            jj = sourceStartY;
            y = py;
            for (var j = 0; j < dh; j++)
            {
                if (y >= 0 && y < dph)
                {
                    ii = sourceStartX;
                    idx = px + y * dpw;
                    x = px;
                    sourcePixel = sourcePixels[0];

                    // Pixel by pixel copying
                    for (var i = 0; i < dw; i++)
                    {
                        if (x >= 0 && x < dpw)
                        {
                            if ((int)ii != lastii || (int)jj != lastjj)
                            {
                                sourceIdx = (int)ii + (int)jj * sourceWidth;
                                if (sourceIdx >= 0 && sourceIdx < sourceLength)
                                {
                                    sourcePixel = sourcePixels[sourceIdx];
                                    sa = ((sourcePixel >> 24) & 0xff);
                                    sr = ((sourcePixel >> 16) & 0xff);
                                    sg = ((sourcePixel >> 8) & 0xff);
                                    sb = ((sourcePixel) & 0xff);
                                }
                                else
                                {
                                    sa = 0;
                                }
                            }

                            if (sa > 0)
                            {
                                int destPixel = destPixels[idx];
                                da = ((destPixel >> 24) & 0xff);
                                if ((sa == 255 || da == 0))
                                {
                                    destPixels[idx] = sourcePixel;
                                }
                                else
                                {
                                    dr = ((destPixel >> 16) & 0xff);
                                    dg = ((destPixel >> 8) & 0xff);
                                    db = ((destPixel) & 0xff);
                                    var isa = 255 - sa;
#if NETFX_CORE
                                    // Special case for WinRT since it does not use pARGB (pre-multiplied alpha)
                                    destPixel = ((da & 0xff) << 24) |
                                                ((((sr * sa + isa * dr) >> 8) & 0xff) << 16) |
                                                ((((sg * sa + isa * dg) >> 8) & 0xff) <<  8) |
                                                (((sb * sa + isa * db) >> 8) & 0xff);
#elif WPF
                                    if (isPrgba)
                                    {
                                        destPixel = ((da & 0xff) << 24) |
                                                    (((((sr << 8) + isa * dr) >> 8) & 0xff) << 16) |
                                                    (((((sg << 8) + isa * dg) >> 8) & 0xff) << 8) |
                                                     ((((sb << 8) + isa * db) >> 8) & 0xff);
                                    }
                                    else
                                    {
                                        destPixel = ((da & 0xff) << 24) |
                                                    (((((sr * sa) + isa * dr) >> 8) & 0xff) << 16) |
                                                    (((((sg * sa) + isa * dg) >> 8) & 0xff) << 8) |
                                                     ((((sb * sa) + isa * db) >> 8) & 0xff);
                                    }
#else
                                    destPixel = ((da & 0xff) << 24) |
                                                (((((sr << 8) + isa * dr) >> 8) & 0xff) << 16) |
                                                (((((sg << 8) + isa * dg) >> 8) & 0xff) << 8) |
                                                 ((((sb << 8) + isa * db) >> 8) & 0xff);
#endif
                                    destPixels[idx] = destPixel;
                                }
                            }
                        }
                        x++;
                        idx++;
                        ii += sdx;
                    }
                }
                jj += sdy;
                y++;
            }

        }

        #endregion
    }
}