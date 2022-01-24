using AzgaarMap.Interfaces;
using AzgaarMap.Extensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzgaarMap.Space
{
    /// <summary>
    /// The Cell class describes atomic locations in the map and provide functionality to interact with cells.
    /// </summary>
    /// <remarks>
    /// Please notice the Cell class does not store Cell related data, but provide all functionality required to interact with it.
    /// Data itself is stores in different data strucures for performance reasons. For example, edges are stored as a Vertex Buffer.
    /// </remarks>
    [DataContract(IsReference = true)]
    public class Cell : IBoundary
    {
        /// <summary>
        /// Index of the first vertice in the Vertex Buffer.
        /// </summary>
        [DataMember] 
        public int VerticeIndex { get; protected set; }

        /// <summary>
        /// Count of vertices representing this cell.
        /// </summary>
        [DataMember] 
        public int VerticeCount { get; protected set; }

        /// <summary>
        /// Heigth of the cell
        /// </summary>
        public float Height
        {
            get
            {
                return Geometry.Vertices[VerticeIndex].HeightBioma.X;
            }
        }

        /// <summary>
        /// Biome of the cell
        /// </summary>
        public float Biome
        {
            get
            {
                return Geometry.Vertices[VerticeIndex].HeightBioma.Y;
            }
        }

        /// <summary>
        ///  Corruption level ranging from -1.0 (Cursed) to 1.0 (Blessed)
        /// </summary>
        /// <remarks>
        /// Indicates how evil a the cell is.
        /// </remarks>
        [DataMember] 
        public float Corruption { get; protected set; }

        [DataMember]
        public IGeometry Geometry { get; internal set; }

        [DataMember]
        public ICellQTree Tree { get; internal set; }

        public void SetColor(Color color, bool includeCentroid = false)
        {
            // we skip the first vertice (the centroid)
            int start = (includeCentroid) ? 1 : 0;
            start += VerticeIndex;
            int end = VerticeCount + VerticeIndex;
            for (int i = start; i < end; i++)
            {
                Geometry.Vertices[i].Color = color;
            }
        }

        /// <summary>
        /// Centroid of the cell geometry.
        /// </summary>
        /// <remarks>
        /// Always precalculated, so no performance impact.
        /// </remarks>
        public Vector3 Centroid
        {
            get
            {
                return Geometry.Vertices[VerticeIndex].Position;
            }
        }

        /// <summary>
        /// Neighboring cells with touching edges.
        /// </summary>
        /// <remarks>
        /// Neighbors are found and cached after first call.
        /// </remarks>
        public List<Cell> Neighbors
        {
            get
            {
                if(_lazyNeighbors == null)
                {
                    _lazyNeighbors = this.Tree.FindNeighbors(this);
                }
                return _lazyNeighbors;
            }
        }
        List<Cell> _lazyNeighbors;

        /// <summary>
        /// Boundaries (bounding box) of this cell.
        /// </summary>
        public BoundingBox Boundaries
        {
            get
            {
                if(_lazyBoundaries.Empty())
                {
                    //+ and - 1 used as there is no need for centroid
                    _lazyBoundaries = BoundingBox.CreateFromPoints(Geometry.GetPositions(VerticeIndex + 1, VerticeCount - 1));
                }
                return _lazyBoundaries;
            }
        }
        BoundingBox _lazyBoundaries;

        public Cell(int index, int count)
        {
            VerticeIndex = index;
            VerticeCount = count;
        }

        /// <summary>
        /// Evaluates if the given point in inside the cell
        /// </summary>
        public bool Contains(Vector3 pos)
        {
            // fastest point inside poly approach I found - // https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
            // https://stackoverflow.com/questions/217578/how-can-i-determine-whether-a-2d-point-is-within-a-polygon
            List<Vector3> edges = Geometry.GetPositions(VerticeIndex + 1, VerticeCount - 1); /*1's as there is no need for centroid*/
            bool inside = false;
            for (int i = 0, j = edges.Count - 1; i < edges.Count; j = i++)
            {
                if ((edges[i].Y > pos.Y) != (edges[j].Y > pos.Y) &&
                     pos.X < (edges[j].X - edges[i].X) * (pos.Y - edges[i].Y) / (edges[j].Y - edges[i].Y) + edges[i].X)
                {
                    inside = !inside; // flip it!
                }
            }
            return inside;
        }

        /// <summary>
        /// Check if cells share the same parent in the space quad tree partition.
        /// </summary>
        public bool ShareParent(Cell cell)
        {
            return Tree.GetNode(Centroid) == Tree.GetNode(cell.Centroid);
        }

        /// <summary>
        /// Check if cells sare neighbors (share and edge).
        /// </summary>
        public bool IsNeighbor(Cell cell)
        {
            return Neighbors.Contains(cell);
        }
    }
}
