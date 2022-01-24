using ImageMagick;
using Microsoft.Xna.Framework;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using AzgaarMap.Extensions;

namespace AzgaarMapParser
{
    /// <summary>
    /// A collection of helper methods for parsing the Azgaar .map and .svg files.
    /// </summary>
    internal class ParseHelper
    {
        /// <summary>
        /// Cache opened documents to avoid extensive IO
        /// </summary>
        private static Dictionary<string, XmlDocument> CachedXmlDocuments = new Dictionary<string, XmlDocument>();
        private static Dictionary<string, SvgDocument> CachedSvgDocuments = new Dictionary<string, SvgDocument>();

        /// <summary>
        /// Returns an Svg document from the given path.
        /// </summary>
        public static SvgDocument GetSvg(string svg_path)
        {
            // check cached svg documents
            SvgDocument svg;
            if (!CachedSvgDocuments.ContainsKey(svg_path))
            {
                svg = SvgDocument.Open(svg_path);
                CachedSvgDocuments.Add(svg_path, svg);
            }
            else
            {
                svg = CachedSvgDocuments[svg_path];
            }
            return svg;
        }

        /// <summary>
        /// Returns an Xml document from the given path.
        /// </summary>
        public static XmlDocument GetXml(string xml_path)
        {
            // check cached xml documents
            XmlDocument doc;
            if (!CachedXmlDocuments.ContainsKey(xml_path))
            {
                doc = new XmlDocument();
                doc.Load(xml_path);
                CachedXmlDocuments.Add(xml_path, doc);
            }
            else
            {
                doc = CachedXmlDocuments[xml_path];
            }
            return doc;
        }

        /// <summary>
        /// Render a given SVG element to a System Bitmap.
        /// </summary>
        public static Bitmap BitmapFromElement(string svg_path, string element_name, Vector2 texture_size = default(Vector2))
        {
            // get document
            SvgDocument svg_doc = GetSvg(svg_path);

            // render bitmap
            SizeF bitmap_size = (texture_size == default(Vector2)) ? svg_doc.GetDimensions() : new SizeF(texture_size.X, texture_size.Y);
            Bitmap bitmap = new Bitmap((int)bitmap_size.Width, (int)bitmap_size.Height);
            using (var renderer = SvgRenderer.FromImage(bitmap))
            {
                SizeF svg_size = svg_doc.GetDimensions();
                SizeF imageSize = svg_size;
                svg_doc.RasterizeDimensions(ref imageSize, (int)bitmap_size.Width, (int)bitmap_size.Height);
                renderer.ScaleTransform(imageSize.Width / svg_size.Width, imageSize.Height / svg_size.Height);
                renderer.SetBoundable(new GenericBoundable(0, 0, svg_size.Width, svg_size.Height));
                svg_doc.GetElementById(element_name).RenderElement(renderer);
            }
            return bitmap;
        }

        /// <summary>
        /// Get a XmlNodeList from a given element
        /// </summary>
        public static XmlNodeList GetNodeChildrens(string doc_path, string node_id, string parent_id = "")
        {
            // get document
            XmlDocument doc = GetXml(doc_path);

            // return xpath query
            if (parent_id != string.Empty)
            {
                return doc.SelectSingleNode($"//*[@id='{parent_id}']").SelectSingleNode($".//*[@id='{node_id}']").ChildNodes;
            }
            else
            {
                return doc.SelectSingleNode($"//*[@id='{node_id}']").ChildNodes;
            }
        }

        /// <summary>
        /// Crop a given rectangular portion of an image and check if color patterns match expected values.
        /// </summary>
        public static IMagickImage<byte> CropAndCheck(MagickImage img, BoundingBox box, int fuzzy = 7, params IMagickColor<byte>[] colors)
        {
            // we crop the given tile using the provided bounding box
            var tile = img.Clone();
            tile.Crop(new MagickGeometry((int)box.Min.X, (int)box.Min.Y, (int)box.Width(), (int)box.Height()));
            // ATTENTION: due to aspect ratio and resize, the very corner may not be the size of the step... but we can ignore this fact

            // we filter tiles that are made of a single color
            // for example, we don't need tiles made just of water, so we use the trick with resize to check it
            var img_check = tile.Clone();
            img_check.Resize(1, 1);
            var pixel_value = img_check.GetPixelsUnsafe().GetPixel(0, 0);
            foreach (IMagickColor<byte> color in colors)
            {
                if (color.FuzzyEquals(pixel_value.ToColor(), new Percentage(fuzzy)))
                {
                    return null; // if it matched one of the colors to filter, return nothing
                }
            }

            return tile; // looks like there was no match in the given colors, we lets return the tile \o/
        }

        /// <summary>
        /// Crop a given rectangular portion of an image and check if color patterns match expected values.
        /// </summary>
        public static IMagickImage<byte> CropAndCheck(MagickImage img, int w, int h, int step, int fuzzy, params IMagickColor<byte>[] colors)
        {
            return CropAndCheck(img, new BoundingBox(new Vector3(w, h, 0), new Vector3(w + step, h + step, 0)), fuzzy, colors);
        }

        /// <summary>
        /// Check if a collection of points (geometry) contains the given point.
        /// </summary>
        public static bool Inside(List<Vector2> points, Vector2 point)
        {
            // fastest point inside poly approach I found - // https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
            // https://stackoverflow.com/questions/217578/how-can-i-determine-whether-a-2d-point-is-within-a-polygon
            bool inside = false;
            for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                if ((points[i].Y > point.Y) != (points[j].Y > point.Y) &&
                        point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X)
                {
                    inside = !inside; // flip it!
                }
            }
            return inside;
        }

        /// <summary>
        /// Release unhandled resources, file handles, and force garbage collection.
        /// </summary>
        public static void Dispose()
        {
            // releasing XMlDocument has to be using GC, as by https://stackoverflow.com/questions/11015965/c-sharp-the-close-method-of-xml-loadfile
            // and the same happened with the SvgDocument as we used the path favor of Open() - https://github.com/svg-net/SVG/blob/master/Source/SvgDocument.cs 
            CachedXmlDocuments.Clear();
            CachedSvgDocuments.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
