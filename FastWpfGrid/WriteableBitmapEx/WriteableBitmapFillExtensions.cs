#region Header
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
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
using System.Collections.Generic;

#if NETFX_CORE
namespace Windows.UI.Xaml.Media.Imaging
#else
namespace System.Windows.Media.Imaging
#endif
{
    /// <summary>
    /// Collection of extension methods for the WriteableBitmap class.
    /// </summary>
    public
#if WPF
    unsafe
#endif
 static partial class WriteableBitmapExtensions
    {
        #region Methods

        #region Fill Shapes

        #region Rectangle

        /// <summary>
        /// Draws a filled rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static void FillRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillRectangle(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// Draws a filled rectangle with or without alpha blending (default = false).
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        /// <param name="doAlphaBlend">True if alpha blending should be performed or false if not.</param>
        public static void FillRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color, bool doAlphaBlend = false)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                var w = context.Width;
                var h = context.Height;

                int sa = ((color >> 24) & 0xff);
                int sr = ((color >> 16) & 0xff);
                int sg = ((color >> 8) & 0xff);
                int sb = ((color) & 0xff);

                bool noBlending = !doAlphaBlend || sa == 255;

                var pixels = context.Pixels;

                // Check boundaries
                if ((x1 < 0 && x2 < 0) || (y1 < 0 && y2 < 0)
                 || (x1 >= w && x2 >= w) || (y1 >= h && y2 >= h))
                {
                    return;
                }

                // Clamp boundaries
                if (x1 < 0) { x1 = 0; }
                if (y1 < 0) { y1 = 0; }
                if (x2 < 0) { x2 = 0; }
                if (y2 < 0) { y2 = 0; }
                if (x1 >= w) { x1 = w - 1; }
                if (y1 >= h) { y1 = h - 1; }
                if (x2 >= w) { x2 = w - 1; }
                if (y2 >= h) { y2 = h - 1; }

                //swap values
                if (y1 > y2)
                {
                    y2 -= y1;
                    y1 += y2;
                    y2 = (y1 - y2);
                }

                // Fill first line
                var startY = y1 * w;
                var startYPlusX1 = startY + x1;
                var endOffset = startY + x2;
                for (var idx = startYPlusX1; idx < endOffset; idx++)
                {
                    pixels[idx] = noBlending ? color : AlphaBlendColors(pixels[idx], sa, sr, sg, sb);
                }

                // Copy first line
                var len = (x2 - x1);
                var srcOffsetBytes = startYPlusX1 * SizeOfArgb;
                var offset2 = y2 * w + x1;
                for (var y = startYPlusX1 + w; y < offset2; y += w)
                {
                    if (noBlending)
                    {
                        BitmapContext.BlockCopy(context, srcOffsetBytes, context, y * SizeOfArgb, len * SizeOfArgb);
                        continue;
                    }

                    // Alpha blend line
                    for (int i = 0; i < len; i++)
                    {
                        int idx = y + i;
                        pixels[idx] = AlphaBlendColors(pixels[idx], sa, sr, sg, sb);
                    }
                }
            }
        }

        private static int AlphaBlendColors(int pixel, int sa, int sr, int sg, int sb)
        {
            // Alpha blend
            int destPixel = pixel;
            int da = ((destPixel >> 24) & 0xff);
            int dr = ((destPixel >> 16) & 0xff);
            int dg = ((destPixel >> 8) & 0xff);
            int db = ((destPixel) & 0xff);

            destPixel = ((sa + (((da * (255 - sa)) * 0x8081) >> 23)) << 24) |
                                     ((sr + (((dr * (255 - sa)) * 0x8081) >> 23)) << 16) |
                                     ((sg + (((dg * (255 - sa)) * 0x8081) >> 23)) << 8) |
                                     ((sb + (((db * (255 - sa)) * 0x8081) >> 23)));

            return destPixel;
        }


        #endregion

        #region Ellipse

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing filled ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color for the line.</param>
        public static void FillEllipse(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillEllipse(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing filled ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color for the line.</param>
        public static void FillEllipse(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Calc center and radius
            int xr = (x2 - x1) >> 1;
            int yr = (y2 - y1) >> 1;
            int xc = x1 + xr;
            int yc = y1 + yr;
            bmp.FillEllipseCentered(xc, yc, xr, yr, color);
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing filled ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        public static void FillEllipseCentered(this WriteableBitmap bmp, int xc, int yc, int xr, int yr, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillEllipseCentered(xc, yc, xr, yr, col);
        }


        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing filled ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf  
        /// With or without alpha blending (default = false).
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="doAlphaBlend">True if alpha blending should be performed or false if not.</param>
        public static void FillEllipseCentered(this WriteableBitmap bmp, int xc, int yc, int xr, int yr, int color, bool doAlphaBlend = false)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            using (var context = bmp.GetBitmapContext())
            {
                var pixels = context.Pixels;
                int w = context.Width;
                int h = context.Height;

                // Avoid endless loop
                if (xr < 1 || yr < 1)
                {
                    return;
                }

                // Init vars
                int uh, lh, uy, ly, lx, rx;
                int x = xr;
                int y = 0;
                int xrSqTwo = (xr * xr) << 1;
                int yrSqTwo = (yr * yr) << 1;
                int xChg = yr * yr * (1 - (xr << 1));
                int yChg = xr * xr;
                int err = 0;
                int xStopping = yrSqTwo * xr;
                int yStopping = 0;

                int sa = ((color >> 24) & 0xff);
                int sr = ((color >> 16) & 0xff);
                int sg = ((color >> 8) & 0xff);
                int sb = ((color) & 0xff);

                bool noBlending = !doAlphaBlend || sa == 255;

                // Draw first set of points counter clockwise where tangent line slope > -1.
                while (xStopping >= yStopping)
                {
                    // Draw 4 quadrant points at once
                    // Upper half
                    uy = yc + y;
                    // Lower half
                    ly = yc - y;

                    // Clip
                    if (uy < 0) uy = 0;
                    if (uy >= h) uy = h - 1;
                    if (ly < 0) ly = 0;
                    if (ly >= h) ly = h - 1;

                    // Upper half
                    uh = uy * w;
                    // Lower half
                    lh = ly * w;

                    rx = xc + x;
                    lx = xc - x;

                    // Clip
                    if (rx < 0) rx = 0;
                    if (rx >= w) rx = w - 1;
                    if (lx < 0) lx = 0;
                    if (lx >= w) lx = w - 1;

                    // Draw line
                    for (int i = lx; i <= rx; i++)
                    {
                        // Quadrant II to I (Actually two octants)
                        pixels[i + uh] = noBlending ? color : AlphaBlendColors(pixels[i + uh], sa, sr, sg, sb);

                        // Quadrant III to IV
                        pixels[i + lh] = noBlending ? color : AlphaBlendColors(pixels[i + lh], sa, sr, sg, sb);
                    }

                    y++;
                    yStopping += xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                    if ((xChg + (err << 1)) > 0)
                    {
                        x--;
                        xStopping -= yrSqTwo;
                        err += xChg;
                        xChg += yrSqTwo;
                    }
                }

                // ReInit vars
                x = 0;
                y = yr;

                // Upper half
                uy = yc + y;
                // Lower half
                ly = yc - y;

                // Clip
                if (uy < 0) uy = 0;
                if (uy >= h) uy = h - 1;
                if (ly < 0) ly = 0;
                if (ly >= h) ly = h - 1;

                // Upper half
                uh = uy * w;
                // Lower half
                lh = ly * w;

                xChg = yr * yr;
                yChg = xr * xr * (1 - (yr << 1));
                err = 0;
                xStopping = 0;
                yStopping = xrSqTwo * yr;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xStopping <= yStopping)
                {
                    // Draw 4 quadrant points at once
                    rx = xc + x;
                    lx = xc - x;

                    // Clip
                    if (rx < 0) rx = 0;
                    if (rx >= w) rx = w - 1;
                    if (lx < 0) lx = 0;
                    if (lx >= w) lx = w - 1;

                    // Draw line
                    for (int i = lx; i <= rx; i++)
                    {
                        pixels[i + uh] = color; // Quadrant II to I (Actually two octants)
                        pixels[i + lh] = color; // Quadrant III to IV
                    }

                    x++;
                    xStopping += yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                    if ((yChg + (err << 1)) > 0)
                    {
                        y--;
                        uy = yc + y; // Upper half
                        ly = yc - y; // Lower half
                        if (uy < 0) uy = 0; // Clip
                        if (uy >= h) uy = h - 1; // ...
                        if (ly < 0) ly = 0;
                        if (ly >= h) ly = h - 1;
                        uh = uy * w; // Upper half
                        lh = ly * w; // Lower half
                        yStopping -= xrSqTwo;
                        err += yChg;
                        yChg += xrSqTwo;
                    }
                }
            }
        }

        #endregion

        #region Polygon, Triangle, Quad

        /// <summary>
        /// Draws a filled polygon. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polygon in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public static void FillPolygon(this WriteableBitmap bmp, int[] points, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillPolygon(points, col);
        }

        /// <summary>
        /// Draws a filled polygon with or without alpha blending (default = false). 
        /// Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polygon in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="doAlphaBlend">True if alpha blending should be performed or false if not.</param>
        public static void FillPolygon(this WriteableBitmap bmp, int[] points, int color, bool doAlphaBlend = false)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;

                int sa = ((color >> 24) & 0xff);
                int sr = ((color >> 16) & 0xff);
                int sg = ((color >> 8) & 0xff);
                int sb = ((color) & 0xff);

                bool noBlending = !doAlphaBlend || sa == 255;

                var pixels = context.Pixels;
                int pn = points.Length;
                int pnh = points.Length >> 1;
                int[] intersectionsX = new int[pnh];

                // Find y min and max (slightly faster than scanning from 0 to height)
                int yMin = h;
                int yMax = 0;
                for (int i = 1; i < pn; i += 2)
                {
                    int py = points[i];
                    if (py < yMin) yMin = py;
                    if (py > yMax) yMax = py;
                }
                if (yMin < 0) yMin = 0;
                if (yMax >= h) yMax = h - 1;


                // Scan line from min to max
                for (int y = yMin; y <= yMax; y++)
                {
                    // Initial point x, y
                    float vxi = points[0];
                    float vyi = points[1];

                    // Find all intersections
                    // Based on http://alienryderflex.com/polygon_fill/
                    int intersectionCount = 0;
                    for (int i = 2; i < pn; i += 2)
                    {
                        // Next point x, y
                        float vxj = points[i];
                        float vyj = points[i + 1];

                        // Is the scanline between the two points
                        if (vyi < y && vyj >= y
                         || vyj < y && vyi >= y)
                        {
                            // Compute the intersection of the scanline with the edge (line between two points)
                            intersectionsX[intersectionCount++] = (int)(vxi + (y - vyi) / (vyj - vyi) * (vxj - vxi));
                        }
                        vxi = vxj;
                        vyi = vyj;
                    }

                    // Sort the intersections from left to right using Insertion sort 
                    // It's faster than Array.Sort for this small data set
                    int t, j;
                    for (int i = 1; i < intersectionCount; i++)
                    {
                        t = intersectionsX[i];
                        j = i;
                        while (j > 0 && intersectionsX[j - 1] > t)
                        {
                            intersectionsX[j] = intersectionsX[j - 1];
                            j = j - 1;
                        }
                        intersectionsX[j] = t;
                    }

                    // Fill the pixels between the intersections
                    for (int i = 0; i < intersectionCount - 1; i += 2)
                    {
                        int x0 = intersectionsX[i];
                        int x1 = intersectionsX[i + 1];

                        // Check boundary
                        if (x1 > 0 && x0 < w)
                        {
                            if (x0 < 0) x0 = 0;
                            if (x1 >= w) x1 = w - 1;

                            // Fill the pixels
                            for (int x = x0; x <= x1; x++)
                            {
                                int idx = y * w + x;

                                pixels[idx] = noBlending ? color : AlphaBlendColors(pixels[idx], sa, sr, sg, sb);
                            }
                        }
                    }
                }
            }
        }


        #region Multiple (possibly nested) Polygons
        /// <summary>
        /// Helper class for storing the data of an edge.
        /// </summary>
        /// <remarks>
        /// The following is always true: 
        /// <code>edge.StartY &lt; edge.EndY</code>
        /// </remarks>
        private class Edge : IComparable<Edge>
        {
            /// <summary>
            /// X coordinate of starting point of edge.
            /// </summary>
            public readonly int StartX;
            /// <summary>
            /// Y coordinate of starting point of edge.
            /// </summary>
            public readonly int StartY;
            /// <summary>
            /// X coordinate of ending point of edge.
            /// </summary>
            public readonly int EndX;
            /// <summary>
            /// Y coordinate of ending point of edge.
            /// </summary>
            public readonly int EndY;
            /// <summary>
            /// The sloap of the edge.
            /// </summary>
            public readonly float Sloap;

            /// <summary>
            /// Initializes a new instance of the <see cref="Edge"/> class.
            /// </summary>
            /// <remarks>
            /// The constructor may swap start and end point to fulfill the guarantees of this class.
            /// </remarks>
            /// <param name="startX">The X coordinate of the start point of the edge.</param>
            /// <param name="startY">The Y coordinate of the start point of the edge.</param>
            /// <param name="endX">The X coordinate of the end point of the edge.</param>
            /// <param name="endY">The Y coordinate of the end point of the edge.</param>
            public Edge(int startX, int startY, int endX, int endY)
            {
                if (startY > endY)
                {
                    // swap direction
                    StartX = endX;
                    StartY = endY;
                    EndX = startX;
                    EndY = startY;
                }
                else
                {
                    StartX = startX;
                    StartY = startY;
                    EndX = endX;
                    EndY = endY;
                }
                Sloap = (EndX - StartX) / (float)(EndY - StartY);
            }

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public int CompareTo(Edge other)
            {
                return StartY == other.StartY
                    ? StartX.CompareTo(other.StartX)
                    : StartY.CompareTo(other.StartY);
            }
        }

        /// <summary>
        /// Draws filled polygons using even-odd filling, therefore allowing for holes.
        /// </summary>
        /// <remarks>
        /// Polygons are implicitly closed if necessary.
        /// </remarks>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="polygons">Array of polygons. 
        /// The different polygons are identified by the first index, 
        /// while the points of each polygon are in x and y pairs indexed by the second index, 
        /// therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).
        /// </param>
        /// <param name="color">The color for the polygon.</param>
        public static void FillPolygonsEvenOdd(this WriteableBitmap bmp, int[][] polygons, Color color)
        {
            var col = ConvertColor(color);
            FillPolygonsEvenOdd(bmp, polygons, col);
        }

        /// <summary>
        /// Draws filled polygons using even-odd filling, therefore allowing for holes.
        /// </summary>
        /// <remarks>
        /// Polygons are implicitly closed if necessary.
        /// </remarks>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="polygons">Array of polygons. 
        /// The different polygons are identified by the first index, 
        /// while the points of each polygon are in x and y pairs indexed by the second index, 
        /// therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).
        /// </param>
        /// <param name="color">The color for the polygon.</param>
        public static void FillPolygonsEvenOdd(this WriteableBitmap bmp, int[][] polygons, int color)
        {
            #region Algorithm

            // Algorithm:
            // This is using a scanline algorithm which is kept similar to the one the FillPolygon() method is using,
            // but it is only comparing the edges with the scanline which are currently intersecting the line.
            // To be able to do this it first builds a list of edges (var edges) from the polygons, which is then 
            // sorted via by their minimal y coordinate. During the scanline run only the edges which can intersect 
            // the current scanline are intersected to get the X coordinate of the intersection. These edges are kept 
            // in the list named currentEdges.
            // Especially for larger sane(*) polygons this is a lot faster then the algorithm used in the FillPolygon() 
            // method which is always comparing all edges with the scan line.
            // And sorry: the constraint to explicitly make the polygon close before using the FillPolygon() method is 
            // stupid, as filling an unclosed polygon is not very useful.
            //
            // (*) sane: the polygons in the FillSample speed test are not sane, because they contain a lot of very long 
            //     nearly vertical lines. A sane example would be a letter 'o', in which case the currentEdges list is 
            //     containing no more than 4 edges at any moment, regardless of the smoothness of the rendering of the 
            //     letter into two polygons.

            #endregion

            int polygonCount = polygons.Length;
            if (polygonCount == 0)
            {
                return;
            }
            // could use single polygon fill if count is 1, but it the algorithm used there is slower (at least for larger polygons)


            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;
                var pixels = context.Pixels;

                // Register edges, and find y max
                List<Edge> edges = new List<Edge>();
                int yMax = 0;
                foreach (int[] points in polygons)
                {
                    int pn = points.Length;
                    if (pn < 6)
                    {
                        // sanity check: don't care for lines or points or empty polygons
                        continue;
                    }
                    int lastX;
                    int lastY;
                    int start;
                    if (points[0] != points[pn - 2]
                        || points[1] != points[pn - 1])
                    {
                        start = 0;
                        lastX = points[pn - 2];
                        lastY = points[pn - 1];
                    }
                    else
                    {
                        start = 2;
                        lastX = points[0];
                        lastY = points[1];
                    }
                    for (int i = start; i < pn; i += 2)
                    {
                        int px = points[i];
                        int py = points[i + 1];
                        if (py != lastY)
                        {
                            Edge edge = new Edge(lastX, lastY, px, py);
                            if (edge.StartY < h && edge.EndY >= 0)
                            {
                                if (edge.EndY > yMax) yMax = edge.EndY;
                                edges.Add(edge);
                            }
                        }
                        lastX = px;
                        lastY = py;
                    }
                }
                if (edges.Count == 0)
                {
                    // sanity check
                    return;
                }

                if (yMax >= h) yMax = h - 1;

                edges.Sort();
                int yMin = edges[0].StartY;
                if (yMin < 0) yMin = 0;

                int[] intersectionsX = new int[edges.Count];

                LinkedList<Edge> currentEdges = new LinkedList<Edge>();
                int e = 0;

                // Scan line from min to max
                for (int y = yMin; y <= yMax; y++)
                {
                    // Remove edges no longer intersecting
                    LinkedListNode<Edge> node = currentEdges.First;
                    while (node != null)
                    {
                        LinkedListNode<Edge> nextNode = node.Next;
                        Edge edge = node.Value;
                        if (edge.EndY <= y)
                        {
                            // using = here because the connecting edge will be added next
                            // remove edge
                            currentEdges.Remove(node);
                        }
                        node = nextNode;
                    }
                    // Add edges starting to intersect
                    while (e < edges.Count &&
                           edges[e].StartY <= y)
                    {
                        currentEdges.AddLast(edges[e]);
                        ++e;
                    }
                    // Calculate intersections
                    int intersectionCount = 0;
                    foreach (Edge currentEdge in currentEdges)
                    {
                        intersectionsX[intersectionCount++] =
                            (int)(currentEdge.StartX + (y - currentEdge.StartY) * currentEdge.Sloap);
                    }

                    // Sort the intersections from left to right using Insertion sort 
                    // It's faster than Array.Sort for this small data set
                    for (int i = 1; i < intersectionCount; i++)
                    {
                        int t = intersectionsX[i];
                        int j = i;
                        while (j > 0 && intersectionsX[j - 1] > t)
                        {
                            intersectionsX[j] = intersectionsX[j - 1];
                            j = j - 1;
                        }
                        intersectionsX[j] = t;
                    }

                    // Fill the pixels between the intersections
                    for (int i = 0; i < intersectionCount - 1; i += 2)
                    {
                        int x0 = intersectionsX[i];
                        int x1 = intersectionsX[i + 1];

                        if (x0 < 0) x0 = 0;
                        if (x1 >= w) x1 = w - 1;
                        if (x1 < x0)
                        {
                            continue;
                        }

                        // Fill the pixels
                        int index = y * w + x0;
                        for (int x = x0; x <= x1; x++)
                        {
                            pixels[index++] = color;
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Draws a filled quad.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="x4">The x-coordinate of the 4th point.</param>
        /// <param name="y4">The y-coordinate of the 4th point.</param>
        /// <param name="color">The color.</param>
        public static void FillQuad(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillQuad(x1, y1, x2, y2, x3, y3, x4, y4, col);
        }

        /// <summary>
        /// Draws a filled quad.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="x4">The x-coordinate of the 4th point.</param>
        /// <param name="y4">The y-coordinate of the 4th point.</param>
        /// <param name="color">The color.</param>
        public static void FillQuad(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, int color)
        {
            bmp.FillPolygon(new int[] { x1, y1, x2, y2, x3, y3, x4, y4, x1, y1 }, color);
        }

        /// <summary>
        /// Draws a filled triangle.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="color">The color.</param>
        public static void FillTriangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillTriangle(x1, y1, x2, y2, x3, y3, col);
        }

        /// <summary>
        /// Draws a filled triangle.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="color">The color.</param>
        public static void FillTriangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3, int color)
        {
            bmp.FillPolygon(new int[] { x1, y1, x2, y2, x3, y3, x1, y1 }, color);
        }

        #endregion

        #region Beziér

        /// <summary>
        /// Draws a filled, cubic Beziér spline defined by start, end and two control points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="cx1">The x-coordinate of the 1st control point.</param>
        /// <param name="cy1">The y-coordinate of the 1st control point.</param>
        /// <param name="cx2">The x-coordinate of the 2nd control point.</param>
        /// <param name="cy2">The y-coordinate of the 2nd control point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color.</param>
        /// <param name="context">The context with the pixels.</param>
        /// <param name="w">The width of the bitmap.</param>
        /// <param name="h">The height of the bitmap.</param> 
        [Obsolete("Obsolete, left for compatibility reasons. Please use List<int> ComputeBezierPoints(int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2) instead.")]
        private static List<int> ComputeBezierPoints(int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2, int color, BitmapContext context, int w, int h)
        {
            return ComputeBezierPoints(x1, y1, cx1, cy1, cx2, cy2, x2, y1);
        }

        /// <summary>
        /// Draws a filled, cubic Beziér spline defined by start, end and two control points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="cx1">The x-coordinate of the 1st control point.</param>
        /// <param name="cy1">The y-coordinate of the 1st control point.</param>
        /// <param name="cx2">The x-coordinate of the 2nd control point.</param>
        /// <param name="cy2">The y-coordinate of the 2nd control point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        private static List<int> ComputeBezierPoints(int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            var minX = Math.Min(x1, Math.Min(cx1, Math.Min(cx2, x2)));
            var minY = Math.Min(y1, Math.Min(cy1, Math.Min(cy2, y2)));
            var maxX = Math.Max(x1, Math.Max(cx1, Math.Max(cx2, x2)));
            var maxY = Math.Max(y1, Math.Max(cy1, Math.Max(cy2, y2)));

            // Get slope
            var lenx = maxX - minX;
            var len = maxY - minY;
            if (lenx > len)
            {
                len = lenx;
            }

            // Prevent divison by zero
            var list = new List<int>();
            if (len != 0)
            {
                // Init vars
                var step = StepFactor / len;
                int tx = x1;
                int ty = y1;

                // Interpolate
                for (var t = 0f; t <= 1; t += step)
                {
                    var tSq = t * t;
                    var t1 = 1 - t;
                    var t1Sq = t1 * t1;

                    tx = (int)(t1 * t1Sq * x1 + 3 * t * t1Sq * cx1 + 3 * t1 * tSq * cx2 + t * tSq * x2);
                    ty = (int)(t1 * t1Sq * y1 + 3 * t * t1Sq * cy1 + 3 * t1 * tSq * cy2 + t * tSq * y2);

                    list.Add(tx);
                    list.Add(ty);
                }

                // Prevent rounding gap
                list.Add(x2);
                list.Add(y2);
            }
            return list;
        }

        /// <summary>
        /// Draws a series of filled, cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therfore the inital curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="color">The color for the spline.</param>
        public static void FillBeziers(this WriteableBitmap bmp, int[] points, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillBeziers(points, col);
        }

        /// <summary>
        /// Draws a series of filled, cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therfore the inital curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="color">The color for the spline.</param>
        public static void FillBeziers(this WriteableBitmap bmp, int[] points, int color)
        {
            // Compute Bezier curve
            int x1 = points[0];
            int y1 = points[1];
            int x2, y2;
            var list = new List<int>();
            for (int i = 2; i + 5 < points.Length; i += 6)
            {
                x2 = points[i + 4];
                y2 = points[i + 5];
                list.AddRange(ComputeBezierPoints(x1, y1, points[i], points[i + 1], points[i + 2], points[i + 3], x2, y2));
                x1 = x2;
                y1 = y2;
            }

            // Fill
            bmp.FillPolygon(list.ToArray(), color);
        }

        #endregion

        #region Cardinal

        /// <summary>
        /// Computes the discrete segment points of a Cardinal spline (cubic) defined by four control points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the 1st control point.</param>
        /// <param name="y1">The y-coordinate of the 1st control point.</param>
        /// <param name="x2">The x-coordinate of the 2nd control point.</param>
        /// <param name="y2">The y-coordinate of the 2nd control point.</param>
        /// <param name="x3">The x-coordinate of the 3rd control point.</param>
        /// <param name="y3">The y-coordinate of the 3rd control point.</param>
        /// <param name="x4">The x-coordinate of the 4th control point.</param>
        /// <param name="y4">The y-coordinate of the 4th control point.</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color.</param>
        /// <param name="context">The context with the pixels.</param>
        /// <param name="w">The width of the bitmap.</param>
        /// <param name="h">The height of the bitmap.</param> 
        [Obsolete("Obsolete, left for compatibility reasons. Please use List<int> ComputeSegmentPoints(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, float tension) instead.")]
        private static List<int> ComputeSegmentPoints(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, float tension, int color, BitmapContext context, int w, int h)
        {
            return ComputeSegmentPoints(x1, y1, x2, y2, x3, y3, x4, y4, tension);
        }

        /// <summary>
        /// Computes the discrete segment points of a Cardinal spline (cubic) defined by four control points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the 1st control point.</param>
        /// <param name="y1">The y-coordinate of the 1st control point.</param>
        /// <param name="x2">The x-coordinate of the 2nd control point.</param>
        /// <param name="y2">The y-coordinate of the 2nd control point.</param>
        /// <param name="x3">The x-coordinate of the 3rd control point.</param>
        /// <param name="y3">The y-coordinate of the 3rd control point.</param>
        /// <param name="x4">The x-coordinate of the 4th control point.</param>
        /// <param name="y4">The y-coordinate of the 4th control point.</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        private static List<int> ComputeSegmentPoints(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, float tension)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            var minX = Math.Min(x1, Math.Min(x2, Math.Min(x3, x4)));
            var minY = Math.Min(y1, Math.Min(y2, Math.Min(y3, y4)));
            var maxX = Math.Max(x1, Math.Max(x2, Math.Max(x3, x4)));
            var maxY = Math.Max(y1, Math.Max(y2, Math.Max(y3, y4)));

            // Get slope
            var lenx = maxX - minX;
            var len = maxY - minY;
            if (lenx > len)
            {
                len = lenx;
            }

            // Prevent divison by zero
            var list = new List<int>();
            if (len != 0)
            {
                // Init vars
                var step = StepFactor / len;

                // Calculate factors
                var sx1 = tension * (x3 - x1);
                var sy1 = tension * (y3 - y1);
                var sx2 = tension * (x4 - x2);
                var sy2 = tension * (y4 - y2);
                var ax = sx1 + sx2 + 2 * x2 - 2 * x3;
                var ay = sy1 + sy2 + 2 * y2 - 2 * y3;
                var bx = -2 * sx1 - sx2 - 3 * x2 + 3 * x3;
                var by = -2 * sy1 - sy2 - 3 * y2 + 3 * y3;

                // Interpolate
                for (var t = 0f; t <= 1; t += step)
                {
                    var tSq = t * t;

                    int tx = (int)(ax * tSq * t + bx * tSq + sx1 * t + x2);
                    int ty = (int)(ay * tSq * t + by * tSq + sy1 * t + y2);

                    list.Add(tx);
                    list.Add(ty);
                }

                // Prevent rounding gap
                list.Add(x3);
                list.Add(y3);
            }
            return list;
        }

        /// <summary>
        /// Draws a filled Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void FillCurve(this WriteableBitmap bmp, int[] points, float tension, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillCurve(points, tension, col);
        }

        /// <summary>
        /// Draws a filled Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void FillCurve(this WriteableBitmap bmp, int[] points, float tension, int color)
        {
            // First segment
            var list = ComputeSegmentPoints(points[0], points[1], points[0], points[1], points[2], points[3], points[4],
                points[5], tension);

            // Middle segments
            int i;
            for (i = 2; i < points.Length - 4; i += 2)
            {
                list.AddRange(ComputeSegmentPoints(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2],
                    points[i + 3], points[i + 4], points[i + 5], tension));
            }

            // Last segment
            list.AddRange(ComputeSegmentPoints(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2],
                points[i + 3], points[i + 2], points[i + 3], tension));

            // Fill
            bmp.FillPolygon(list.ToArray(), color);
        }

        /// <summary>
        /// Draws a filled, closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void FillCurveClosed(this WriteableBitmap bmp, int[] points, float tension, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillCurveClosed(points, tension, col);
        }

        /// <summary>
        /// Draws a filled, closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void FillCurveClosed(this WriteableBitmap bmp, int[] points, float tension, int color)
        {
            int pn = points.Length;

            // First segment
            var list = ComputeSegmentPoints(points[pn - 2], points[pn - 1], points[0], points[1], points[2], points[3],
                points[4], points[5], tension);

            // Middle segments
            int i;
            for (i = 2; i < pn - 4; i += 2)
            {
                list.AddRange(ComputeSegmentPoints(points[i - 2], points[i - 1], points[i], points[i + 1],
                    points[i + 2], points[i + 3], points[i + 4], points[i + 5], tension));
            }

            // Last segment
            list.AddRange(ComputeSegmentPoints(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2],
                points[i + 3], points[0], points[1], tension));

            // Last-to-First segment
            list.AddRange(ComputeSegmentPoints(points[i], points[i + 1], points[i + 2], points[i + 3], points[0],
                points[1], points[2], points[3], tension));

            // Fill
            bmp.FillPolygon(list.ToArray(), color);
        }

        #endregion

        #endregion

        #endregion
    }
}