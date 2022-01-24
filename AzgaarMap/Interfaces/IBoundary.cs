using Microsoft.Xna.Framework;

namespace AzgaarMap.Interfaces
{
    /// <summary>
    /// Any space that has a boundary
    /// </summary>
    public interface IBoundary
    {
        /// <summary>
        /// Boundaries (bounding box) of the element.
        /// </summary>
        public BoundingBox Boundaries { get; }

        /// <summary>
        /// Evaluates if the given point in inside the element.
        /// </summary>
        public bool Contains(Vector3 pos);
    }
}
