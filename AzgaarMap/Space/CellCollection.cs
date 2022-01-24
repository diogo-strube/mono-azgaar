using AzgaarMap.Interfaces;
using AzgaarMap.Render;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AzgaarMap.Space
{
    /// <summary>
    /// An organized and queryable  collection of cells.
    /// </summary>
    [DataContract(IsReference = true)]
    [KnownType(typeof(IndexQuadTree))]
    public class CellCollection : IGeometry, ICellQTree
    {
        /// <summary>
        /// Parent of this node.
        /// </summary>
        public IQTree Parent { get { return null; } }

        /// <summary>
        /// Collection of children.
        /// </summary>
        /// <remarks>
        /// Always 4 or nothing as we use quad-trees.
        /// </remarks>
        public List<IQTree> Children { get { return QuadTree.Children; } }

        /// <summary>
        /// Boundaries (bounding box) of the element.
        /// </summary>
        public BoundingBox Boundaries { get { return QuadTree.Boundaries; } }

        /// <summary>
        /// Every single Cell in the world
        /// </summary>
        /// <remarks>
        /// A cell is the polygon representing the voronoi split of the map space.
        /// </remarks>
        [DataMember] 
        public Cell[] Cells { get; protected set; }

        /// <summary>
        /// Every single Cells
        /// </summary>
        /// <remarks>
        /// A cell is the polygon representing the voronoi split of the map space.
        /// </remarks>
        [DataMember] 
        public IndexQuadTree QuadTree { get; protected set; }

        /// <summary>
        /// Vertex Buffer
        /// </summary>
        /// <remarks>
        /// Vertex are duplicated, meaning each Cell points to ther unique vertice instead of sharing with neighbors.
        /// </remarks>
        [DataMember] 
        public CellVertex[] Vertices { get; protected set; }

        /// <summary>
        /// Index Buffer.
        /// </summary>
        /// <remarks>
        /// Using triangle list (3 indices per triangle).
        /// </remarks>
        [DataMember] 
        public int[] Indices { get; protected set; }

        public CellCollection(CellVertex[] vertices, int[] indices, Cell[] cells, int step)
        {
            Vertices = vertices;
            Indices = indices;
            Cells = cells;

            // link all cells to the geometry (needed for the quad tree)
            foreach (Cell cell in cells)
            {
                cell.Geometry = this;
            }

            QuadTree = IndexQuadTree.CreateFromBoundaries(Cells.ToList<IBoundary>(), step);

            // link all cells to quadtree
            foreach (Cell cell in cells)
            {
                cell.Tree = this;
            }
        }

        /// <summary>
        /// Retreve a collection of positions from the Vertex Buffer
        /// </summary>
        /// <remarks>
        /// Do not use for render.
        /// Returned collection of positions is not optmized as focus is to keep vertices packed togheter for fast rendering.
        /// </remarks>
        public List<Vector3> GetPositions(int start, int length)
        {
            List<Vector3> positions = new List<Vector3>(length);
            for (int i = 0; i < length; i++)
            {
                positions.Add(Vertices[start + i].Position);
            }
            return positions;
        }

        /// <summary>
        /// Find the neighbooring Cells to any given cell
        /// </summary>
        /// <remarks>
        /// Do not use for render.
        /// Prefer use during map creation or load; may take a while depending on the number of cells.
        /// </remarks>
        public List<Cell> FindNeighbors(Cell cell)
        {
            List<Cell> neighbors = new List<Cell>();
            List<IQTree> neighborhoods = QuadTree.GetNodes(cell.Boundaries);
            foreach (IndexQuadTree neighborhood in neighborhoods)
            {
                foreach (int index in neighborhood.Indexes)
                {
                    Cell other = Cells[index];
                    if (other != cell && other.Boundaries.Intersects(cell.Boundaries))
                    {
                        foreach (Vector3 edge in GetPositions(other.VerticeIndex, other.VerticeCount))
                        {
                            // if an edge is withing the boundaries, we got our neightbor!
                            ContainmentType check = cell.Boundaries.Contains(edge);
                            if (check == ContainmentType.Contains || check == ContainmentType.Intersects)
                            {
                                neighbors.Add(other);
                                break;
                            }
                        }
                    }
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Return the cell containig the given point (in world coordinates)
        /// </summary>
        public Cell AtPoint(Vector3 worldPos)
        {
            IndexQuadTree neighborhood = QuadTree.GetNode(worldPos) as IndexQuadTree;
            if (neighborhood != null)
            {
                foreach (int index in neighborhood.Indexes)
                {
                    Cell cell = Cells[index];
                    // start checking with the bounding box
                    ContainmentType check = cell.Boundaries.Contains(worldPos);
                    if (check != ContainmentType.Disjoint)
                    {
                        // and after check the if inside polygon of any neighbor for resolution (as bounding box may mislead us)
                        foreach (Cell neighbor in cell.Neighbors)
                        {
                            if (neighbor.Contains(worldPos))
                            {
                                return neighbor;
                            }
                        }
                        return cell; // if is no neighbor, it is ourselves
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get the node covering the given position;
        /// </summary>
        public IQTree GetNode(Vector3 pos)
        {
            return QuadTree.GetNode(pos);
        }

        /// <summary>
        /// Get all nodes overlapping with the given Bounding Box;
        /// </summary>
        public List<IQTree> GetNodes(BoundingBox bb)
        {
            return QuadTree.GetNodes(bb);
        }

        public List<IQTree> GetLeaves()
        {
            return QuadTree.GetLeaves();
        }

        /// <summary>
        /// Evaluates if the given point in inside the the node.
        /// </summary>
        public bool Contains(Vector3 pos)
        {
            return (QuadTree.Boundaries.Contains(pos) != ContainmentType.Disjoint);
        }
    }
}
