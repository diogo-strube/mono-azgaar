using Svg;
using System.Drawing;

// as implemented in official repo - https://github.com/svg-net/SVG/blob/3daf279fc9357225eb151a263683c0885ebf2e63/Source/Painting/GenericBoundable.cs

namespace AzgaarMapParser
{
    /// <summary>
    /// Specialized implementation to be used with SvgRenderer so we can customize the boundaries
    /// </summary>
    internal class GenericBoundable : ISvgBoundable
    {
        private RectangleF _rect;

        public GenericBoundable(RectangleF rect)
        {
            _rect = rect;
        }
        public GenericBoundable(float x, float y, float width, float height)
        {
            _rect = new RectangleF(x, y, width, height);
        }

        public System.Drawing.PointF Location
        {
            get { return _rect.Location; }
        }

        public System.Drawing.SizeF Size
        {
            get { return _rect.Size; }
        }
        public System.Drawing.RectangleF Bounds
        {
            get { return _rect; }
        }
    }
}
