using AzgaarMap.Render;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AzgaarMap.Interfaces
{
    /// <summary>
    /// Any space with a geometry.
    /// </summary>
    public interface IGeometry
    {
        /// <summary>
        /// Vertex Buffer.
        /// </summary>
        /// <remarks>
        /// Using an array to guarantee continuity and no changes in size.
        /// </remarks>
        public CellVertex[] Vertices { get; }

        /// <summary>
        /// Index Buffer.
        /// </summary>
        /// <remarks>
        /// Using an array to guarantee continuity and no changes in size.
        /// </remarks>
        public int[] Indices { get; }

        /// <summary>
        /// Retreve a collection of positions from the Vertex Buffer.
        /// </summary>
        /// <remarks>
        /// Do not use for render.
        /// Returned collection of positions is not optimized as the focus is to keep vertices packed together for fast rendering.
        /// </remarks>
        public List<Vector3> GetPositions(int start, int length);
    }
}
