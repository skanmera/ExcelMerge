#region Header
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of internal anti-aliasing helper methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-02-24 20:36:41 +0100 (Di, 24 Feb 2015) $
//   Changed in:        $Revision: 112951 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapTransformationExtensions.cs $
//   Id:                $Id: WriteableBitmapTransformationExtensions.cs 112951 2015-02-24 19:36:41Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//
#endregion
using System;

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace Windows.UI.Xaml.Media.Imaging
#else
namespace System.Windows.Media.Imaging
#endif
{
    /// <summary>
    /// Collection of draw extension methods for the Silverlight WriteableBitmap class.
    /// </summary>
    public
#if !SILVERLIGHT 
       unsafe 
#endif
 static partial class WriteableBitmapExtensions
    {
        private static readonly int[] leftEdgeX = new int[8192];
        private static readonly int[] rightEdgeX = new int[8192];

        private static void AAWidthLine(int width, int height, BitmapContext context, float x1, float y1, float x2, float y2, float lineWidth, Int32 color)
        {
            // Perform cohen-sutherland clipping if either point is out of the viewport
            if (!CohenSutherlandLineClip(new Rect(0, 0, width, height), ref x1, ref y1, ref x2, ref y2)) return;

            if (lineWidth <= 0) return;

            var buffer = context.Pixels;

            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }

            if (x1 == x2)
            {
                x1 -= (int)lineWidth / 2;
                x2 += (int)lineWidth / 2;

                if (x1 < 0)
                    x1 = 0;
                if (x2 < 0)
                    return;

                if (x1 >= width)
                    return;
                if (x2 >= width)
                    x2 = width - 1;

                if (y1 >= height || y2 < 0)
                    return;

                if (y1 < 0)
                    y1 = 0;
                if (y2 >= height)
                    y2 = height - 1;

                for (var x = (int)x1; x <= x2; x++)
                {
                    for (var y = (int)y1; y <= y2; y++)
                    {
                        var a = (byte)((color & 0xff000000) >> 24);
                        var r = (byte)((color & 0x00ff0000) >> 16);
                        var g = (byte)((color & 0x0000ff00) >> 8);
                        var b = (byte)((color & 0x000000ff) >> 0);

                        byte rs, gs, bs;
                        byte rd, gd, bd;

                        int d;

                        rs = r;
                        gs = g;
                        bs = b;

                        d = buffer[y * width + x];

                        rd = (byte)((d & 0x00ff0000) >> 16);
                        gd = (byte)((d & 0x0000ff00) >> 8);
                        bd = (byte)((d & 0x000000ff) >> 0);

                        rd = (byte)((rs * a + rd * (0xff - a)) >> 8);
                        gd = (byte)((gs * a + gd * (0xff - a)) >> 8);
                        bd = (byte)((bs * a + bd * (0xff - a)) >> 8);

                        buffer[y * width + x] = (0xff << 24) | (rd << 16) | (gd << 8) | (bd << 0);
                    }
                }

                return;
            }
            if (y1 == y2)
            {
                if (x1 > x2) Swap(ref x1, ref x2);

                y1 -= (int)lineWidth / 2;
                y2 += (int)lineWidth / 2;

                if (y1 < 0) y1 = 0;
                if (y2 < 0) return;

                if (y1 >= height) return;
                if (y2 >= height) x2 = height - 1;

                if (x1 >= width || y2 < 0) return;

                if (x1 < 0) x1 = 0;
                if (x2 >= width) x2 = width - 1;

                for (var x = (int)x1; x <= x2; x++)
                {
                    for (var y = (int)y1; y <= y2; y++)
                    {
                        var a = (byte)((color & 0xff000000) >> 24);
                        var r = (byte)((color & 0x00ff0000) >> 16);
                        var g = (byte)((color & 0x0000ff00) >> 8);
                        var b = (byte)((color & 0x000000ff) >> 0);

                        Byte rs, gs, bs;
                        Byte rd, gd, bd;

                        Int32 d;

                        rs = r;
                        gs = g;
                        bs = b;

                        d = buffer[y * width + x];

                        rd = (byte)((d & 0x00ff0000) >> 16);
                        gd = (byte)((d & 0x0000ff00) >> 8);
                        bd = (byte)((d & 0x000000ff) >> 0);

                        rd = (byte)((rs * a + rd * (0xff - a)) >> 8);
                        gd = (byte)((gs * a + gd * (0xff - a)) >> 8);
                        bd = (byte)((bs * a + bd * (0xff - a)) >> 8);

                        buffer[y * width + x] = (0xff << 24) | (rd << 16) | (gd << 8) | (bd << 0);
                    }
                }

                return;
            }

            y1 += 1;
            y2 += 1;

            float slope = (y2 - y1) / (x2 - x1);
            float islope = (x2 - x1) / (y2 - y1);

            float m = slope;
            float w = lineWidth;

            float dx = x2 - x1;
            float dy = y2 - y1;

            var xtot = (float)(w * dy / Math.Sqrt(dx * dx + dy * dy));
            var ytot = (float)(w * dx / Math.Sqrt(dx * dx + dy * dy));

            float sm = dx * dy / (dx * dx + dy * dy);

            // Center it.

            x1 += xtot / 2;
            y1 -= ytot / 2;
            x2 += xtot / 2;
            y2 -= ytot / 2;

            //
            //

            float sx = -xtot;
            float sy = +ytot;

            var ix1 = (int)x1;
            var iy1 = (int)y1;

            var ix2 = (int)x2;
            var iy2 = (int)y2;

            var ix3 = (int)(x1 + sx);
            var iy3 = (int)(y1 + sy);

            var ix4 = (int)(x2 + sx);
            var iy4 = (int)(y2 + sy);

            if (lineWidth == 2)
            {
                if (Math.Abs(dy) < Math.Abs(dx))
                {
                    if (x1 < x2)
                    {
                        iy3 = iy1 + 2;
                        iy4 = iy2 + 2;
                    }
                    else
                    {
                        iy1 = iy3 + 2;
                        iy2 = iy4 + 2;
                    }
                }
                else
                {
                    ix1 = ix3 + 2;
                    ix2 = ix4 + 2;
                }
            }

            int starty = Math.Min(Math.Min(iy1, iy2), Math.Min(iy3, iy4));
            int endy = Math.Max(Math.Max(iy1, iy2), Math.Max(iy3, iy4));

            if (starty < 0) starty = -1;
            if (endy >= height) endy = height + 1;

            for (int y = starty + 1; y < endy - 1; y++)
            {
                leftEdgeX[y] = -1 << 16;
                rightEdgeX[y] = 1 << 16 - 1;
            }


            AALineQ1(width, height, context, ix1, iy1, ix2, iy2, color, sy > 0, false);
            AALineQ1(width, height, context, ix3, iy3, ix4, iy4, color, sy < 0, true);

            if (lineWidth > 1)
            {
                AALineQ1(width, height, context, ix1, iy1, ix3, iy3, color, true, sy > 0);
                AALineQ1(width, height, context, ix2, iy2, ix4, iy4, color, false, sy < 0);
            }

            if (x1 < x2)
            {
                if (iy2 >= 0 && iy2 < height) rightEdgeX[iy2] = Math.Min(ix2, rightEdgeX[iy2]);
                if (iy3 >= 0 && iy3 < height) leftEdgeX[iy3] = Math.Max(ix3, leftEdgeX[iy3]);
            }
            else
            {
                if (iy1 >= 0 && iy1 < height) rightEdgeX[iy1] = Math.Min(ix1, rightEdgeX[iy1]);
                if (iy4 >= 0 && iy4 < height) leftEdgeX[iy4] = Math.Max(ix4, leftEdgeX[iy4]);
            }

            //return;

            for (int y = starty + 1; y < endy - 1; y++)
            {
                leftEdgeX[y] = Math.Max(leftEdgeX[y], 0);
                rightEdgeX[y] = Math.Min(rightEdgeX[y], width - 1);

                for (int x = leftEdgeX[y]; x <= rightEdgeX[y]; x++)
                {
                    var a = (byte)((color & 0xff000000) >> 24);
                    var r = (byte)((color & 0x00ff0000) >> 16);
                    var g = (byte)((color & 0x0000ff00) >> 8);
                    var b = (byte)((color & 0x000000ff) >> 0);

                    Byte rs, gs, bs;
                    Byte rd, gd, bd;

                    Int32 d;

                    rs = r;
                    gs = g;
                    bs = b;

                    d = buffer[y * width + x];

                    rd = (byte)((d & 0x00ff0000) >> 16);
                    gd = (byte)((d & 0x0000ff00) >> 8);
                    bd = (byte)((d & 0x000000ff) >> 0);

                    rd = (byte)((rs * a + rd * (0xff - a)) >> 8);
                    gd = (byte)((gs * a + gd * (0xff - a)) >> 8);
                    bd = (byte)((bs * a + bd * (0xff - a)) >> 8);

                    buffer[y * width + x] = (0xff << 24) | (rd << 16) | (gd << 8) | (bd << 0);
                }
            }
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        private static void AALineQ1(int width, int height, BitmapContext context, int x1, int y1, int x2, int y2, Int32 color, bool minEdge, bool leftEdge)
        {
            Byte off = 0;

            if (minEdge) off = 0xff;

            if (x1 == x2) return;
            if (y1 == y2) return;

            var buffer = context.Pixels;

            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }

            int deltax = (x2 - x1);
            int deltay = (y2 - y1);

            if (x1 > x2) deltax = (x1 - x2);

            int x = x1;
            int y = y1;

            UInt16 m = 0;

            if (deltax > deltay) m = (ushort)(((deltay << 16) / deltax));
            else m = (ushort)(((deltax << 16) / deltay));

            UInt16 e = 0;

            var a = (byte)((color & 0xff000000) >> 24);
            var r = (byte)((color & 0x00ff0000) >> 16);
            var g = (byte)((color & 0x0000ff00) >> 8);
            var b = (byte)((color & 0x000000ff) >> 0);

            Byte rs, gs, bs;
            Byte rd, gd, bd;

            Int32 d;

            Byte ta = a;

            e = 0;

            if (deltax >= deltay)
            {
                while (deltax-- != 0)
                {
                    if ((UInt16)(e + m) <= e) // Roll
                    {
                        y++;
                    }

                    e += m;

                    if (x1 < x2) x++;
                    else x--;

                    if (y < 0 || y >= height) continue;

                    if (leftEdge) leftEdgeX[y] = Math.Max(x + 1, leftEdgeX[y]);
                    else rightEdgeX[y] = Math.Min(x - 1, rightEdgeX[y]);

                    if (x < 0 || x >= width) continue;

                    //

                    ta = (byte)((a * (UInt16)(((((UInt16)(e >> 8))) ^ off))) >> 8);

                    rs = r;
                    gs = g;
                    bs = b;

                    d = buffer[y * width + x];

                    rd = (byte)((d & 0x00ff0000) >> 16);
                    gd = (byte)((d & 0x0000ff00) >> 8);
                    bd = (byte)((d & 0x000000ff) >> 0);

                    rd = (byte)((rs * ta + rd * (0xff - ta)) >> 8);
                    gd = (byte)((gs * ta + gd * (0xff - ta)) >> 8);
                    bd = (byte)((bs * ta + bd * (0xff - ta)) >> 8);

                    buffer[y * width + x] = (0xff << 24) | (rd << 16) | (gd << 8) | (bd << 0);

                    //
                }
            }
            else
            {
                off ^= 0xff;

                while (--deltay != 0)
                {
                    if ((UInt16)(e + m) <= e) // Roll
                    {
                        if (x1 < x2) x++;
                        else x--;
                    }

                    e += m;

                    y++;

                    if (x < 0 || x >= width) continue;
                    if (y < 0 || y >= height) continue;

                    //

                    ta = (byte)((a * (UInt16)(((((UInt16)(e >> 8))) ^ off))) >> 8);

                    rs = r;
                    gs = g;
                    bs = b;

                    d = buffer[y * width + x];

                    rd = (byte)((d & 0x00ff0000) >> 16);
                    gd = (byte)((d & 0x0000ff00) >> 8);
                    bd = (byte)((d & 0x000000ff) >> 0);

                    rd = (byte)((rs * ta + rd * (0xff - ta)) >> 8);
                    gd = (byte)((gs * ta + gd * (0xff - ta)) >> 8);
                    bd = (byte)((bs * ta + bd * (0xff - ta)) >> 8);

                    buffer[y * width + x] = (0xff << 24) | (rd << 16) | (gd << 8) | (bd << 0);

                    if (leftEdge) leftEdgeX[y] = x + 1;
                    else rightEdgeX[y] = x - 1;
                }
            }
        }
    }
}