using AzgaarMap.Space;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AzgaarMap.Interfaces
{
    /// <summary>
    /// Any space of cells organized in a tree fashion.
    /// </summary>
    public interface ICellQTree : IQTree
    {
        /// <summary>
        /// Find all neighbor cells by traversing the tree.
        /// </summary>
        public List<Cell> FindNeighbors(Cell cell);

        /// <summary>
        /// Find the the cell containing the provided world position.
        /// </summary>
        public Cell AtPoint(Vector3 worldPos);
    }
}
