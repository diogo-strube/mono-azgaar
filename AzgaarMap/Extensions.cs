using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AzgaarMap.Extensions
{
    /// <summary>
    /// Extensions created to better handling XNA types
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Divides the Bounding Box in 4 equaly sized Bounding Boxes.
        /// </summary>
        public static BoundingBox[] Divide(this BoundingBox bb)
        {
            //            b
            //   2 +------+------+ 3
            //     |      |      |
            //     |      |      |
            //   a + -----x------+ c
            //     |      |      |
            //     |      |      |
            //   1 +------+------+ 4
            //            d
            Vector3 x = (bb.Min + bb.Max) / 2;
            Vector3 a = new Vector3(bb.Min.X, x.Y, bb.Min.Z);
            Vector3 b = new Vector3(x.X, bb.Max.Y, bb.Min.Z);
            Vector3 c = new Vector3(bb.Max.X, x.Y, bb.Min.Z);
            Vector3 d = new Vector3(x.X, bb.Min.Y, bb.Min.Z);
            BoundingBox[] boundaries = new BoundingBox[4] {
                new BoundingBox(bb.Min /*1*/, x),
                new BoundingBox(a, b),
                new BoundingBox(x, bb.Max /*3*/),
                new BoundingBox(d, c),
            };
            return boundaries;
        }

        /// <summary>
        /// Center of the Box.
        /// </summary>
        public static Vector3 Center(this BoundingBox bb)
        {
            return (bb.Min + bb.Max) / 2;
        }

        /// <summary>
        /// Size of the bounding box as the lenght (distance) from Min to Max.
        /// </summary>
        public static float Size(this BoundingBox bb)
        {
            return Vector3.Distance(bb.Min, bb.Max);
        }

        /// <summary>
        /// Width of the bounding box.
        /// </summary>
        public static float Width(this BoundingBox bb)
        {
            return Math.Abs(bb.Max.X - bb.Min.X);
        }

        /// <summary>
        /// Height of the bounding box.
        /// </summary>
        public static float Height(this BoundingBox bb)
        {
            return Math.Abs(bb.Max.Y - bb.Min.Y);
        }

        /// <summary>
        /// Check if a bounding box is empty (was created but no values were ever set).
        /// </summary>
        public static bool Empty(this BoundingBox bb)
        {
            return (bb.Min == Vector3.Zero && bb.Max == Vector3.Zero);
        }

        /// <summary>
        /// Calculates the centroid for this collection of points.
        /// </summary>
        public static Vector2 CalculateCentroid(this List<Vector2> points)
        {
            // Simple centroid calculation from https://stackoverflow.com/questions/9815699/how-to-calculate-centroid
            float accumulatedArea = 0.0f;
            float centerX = 0.0f;
            float centerY = 0.0f;

            for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                float temp = points[i].X * points[j].Y - points[j].X * points[i].Y;
                accumulatedArea += temp;
                centerX += (points[i].X + points[j].X) * temp;
                centerY += (points[i].Y + points[j].Y) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
                return Vector2.Zero;  // Avoid division by zero

            accumulatedArea *= 3f;
            return new Vector2(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        /// <summary>
        /// Calculates the centroid for this collection of points.
        /// </summary>
        public static Vector2 Calculate2DCentroid(this List<double> points)
        {
            // simple centroid calculation from https://stackoverflow.com/questions/9815699/how-to-calculate-centroid
            double accumulatedArea = 0.0f;
            double centerX = 0.0f;
            double centerY = 0.0f;

            // with minimal change to handle points
            int i = 0;
            int j = points.Count - 2;
            while (i < points.Count)
            {
                double temp = points[i] * points[j + 1] - points[j] * points[i + 1];
                accumulatedArea += temp;
                centerX += (points[i] + points[j]) * temp;
                centerY += (points[i + 1] + points[j + 1]) * temp;
                j = i;
                i += 2;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
                return Vector2.Zero;  // Avoid division by zero

            accumulatedArea *= 3f;
            return new Vector2((float)(centerX / accumulatedArea), (float)(centerY / accumulatedArea));
        }

        /// <summary>
        /// Transform a list of Float numbers to a list of Vector2
        /// </summary>
        public static List<Vector2> ToVector2(this List<float> points)
        {
            List<Vector2> list = new List<Vector2>(points.Count / 2);
            for (int j = 0; j < points.Count; j += 2)
            {
                list.Add(new Vector2(points[j], points[j + 1]));
            }
            return list;
        }

        /// <summary>
        /// Transform a list of Double numbers to a list of Vector2
        /// </summary>
        public static List<Vector2> ToVector2(this List<double> points)
        {
            List<Vector2> list = new List<Vector2>(points.Count / 2);
            for (int j = 0; j < points.Count; j += 2)
            {
                list.Add(new Vector2((float)points[j], (float)points[j + 1]));
            }
            return list;
        }

        /// <summary>
        /// Transform a list of Double numbers to a list of Vector3
        /// </summary>
        public static List<Vector3> ToVector3(this List<double> points)
        {
            List<Vector3> list = new List<Vector3>(points.Count / 3);
            for (int j = 0; j < points.Count; j += 3)
            {
                list.Add(new Vector3((float)points[j], (float)points[j + 1], (float)points[j + 2]));
            }
            return list;
        }

        /// <summary>
        /// Transform a list of Float numbers to a list of Vector3
        /// </summary>
        public static List<Vector3> ToVector3(this List<float> points)
        {
            List<Vector3> list = new List<Vector3>(points.Count / 3);
            for (int j = 0; j < list.Count; j += 3)
            {
                list.Add(new Vector3(points[j], points[j + 1], points[j + 2]));
            }
            return list;
        }

        /// <summary>
        /// Convert Color to Grayscale floating number
        /// </summary>
        public static float ToGrayscale(this System.Drawing.Color color)
        {
            return (float)color.R * 0.299f + (float)color.G * 0.587f + (float)color.B * 0.114f;
        }
    }
}
