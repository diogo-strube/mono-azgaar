using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AzgaarMap.Interfaces
{
    /// <summary>
    /// Any space organized in a tree fashion.
    /// </summary>
    public interface IQTree : IBoundary
    {
        /// <summary>
        /// Parent of this node.
        /// </summary>
        IQTree Parent { get; }

        /// <summary>
        /// Collection of children.
        /// </summary>
        /// <remarks>
        /// Always 4 or nothing as we use quad-trees.
        /// </remarks>
        public List<IQTree> Children { get; }

        /// <summary>
        /// Get the smallest node containing the given position.
        /// </summary>
        public IQTree GetNode(Vector3 pos);

        /// <summary>
        /// Get all nodes contained or overlapping by the provided bounding box.
        /// </summary>
        public List<IQTree> GetNodes(BoundingBox bb);

        /// <summary>
        /// Get all leave nodes under this current node.
        /// </summary>
        /// <returns></returns>
        public List<IQTree> GetLeaves();
    }
}
