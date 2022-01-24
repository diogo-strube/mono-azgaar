using AzgaarMap.Interfaces;
using AzgaarMap.Extensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AzgaarMap.Space
{
    /// <summary>
    /// A generic Quad Tree implementation that stores indexes (integers) in the leaf nodes, allowing the tree to point to any desired storage.
    /// </summary>
    /// <remarks>
    /// The idea of storing indexes allows the tree to be used with majority of containers, such as for example with an array of Mobiles or an array of Cells.
    /// </remarks>
    [DataContract(IsReference = true)]
    [KnownType(typeof(IndexQuadTree))]
    public class IndexQuadTree : IQTree
    {
        /// <summary>
        /// Collection of indexes for this node all all its childrens.
        /// </summary>
        /// <remarks>
        /// Traverse the tree if the current node is not a leaf node.
        /// </remarks>
        [DataMember] 
        public List<int> Indexes { get; protected set; }

        /// <summary>
        /// Boundaries (bounding box) of the node.
        /// </summary>
        [DataMember] 
        public BoundingBox Boundaries { get; protected set; }

        /// <summary>
        /// Our parent says: "go to bed!"
        /// </summary>
        [DataMember]
        public IQTree Parent { get; protected set; }

        /// <summary>
        /// Four children.
        /// </summary>
        [DataMember] 
        public List<IQTree> Children { get; protected set; }

        /// <summary>
        /// Get the node covering the given position;
        /// </summary>
        public IQTree GetNode(Vector3 pos)
        {
            if (Boundaries.Contains(pos) != ContainmentType.Disjoint)
            {
                if (Children.Count > 0)
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        IQTree node = Children[i].GetNode(pos);
                        if (node != null)
                        {
                            return node; // we found it \o/
                        }
                    }
                }
                else return this;
            }

            return null; // nothing so far, so its a nop
        }

        /// <summary>
        /// Get all nodes overlapping with the given Bounding Box;
        /// </summary>
        public List<IQTree> GetNodes(BoundingBox bb)
        {
            List<IQTree> result = new List<IQTree>();
            if (Boundaries.Contains(bb) != ContainmentType.Disjoint)
            {
                if (Children.Count == 0)
                {
                    result.Add(this);
                }
                else
                { 
                    for (int i = 0; i < Children.Count; i++)
                    {
                        result.AddRange(Children[i].GetNodes(bb));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Evaluates if the given point in inside the the node.
        /// </summary>
        public bool Contains(Vector3 pos)
        {
            return (Boundaries.Contains(pos) != ContainmentType.Disjoint);
        }

        public IndexQuadTree(List<int> indexes, BoundingBox boundaries, IList<IndexQuadTree> children)
        {
            Indexes = indexes;
            Boundaries = boundaries;

            // set parent if needed (in concrete type, as interface does not allow Sets)
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Parent = this;
                }
            }

            Children = children.ToList<IQTree>(); // cast the collection to the interface
        }

        /// <summary>
        /// Return all leaves inside this node.
        /// </summary>
        /// <remarks>
        /// This may not be real-time friendly.
        /// </remarks>
        public List<IQTree> GetLeaves()
        {
            List<IQTree> leaves = new List<IQTree>();
            if (Children.Count > 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    leaves.AddRange(Children[i].GetLeaves());
                }
            }
            else
            {
                leaves.Add(this);
            }
            return leaves;
        }

        /// <summary>
        /// Creates a quad tree from the provided boundaries with leaf nodes having a size  close (not exact) to the provided value.
        /// </summary>
        /// <remarks>
        /// Indexes will match positions of the provided boundaries colletion (if index[0] == 1, this is boundaries[1]).
        /// </remarks>
        public static IndexQuadTree CreateFromBoundaries(IList<IBoundary> boundaries, float desiredLeafSize, BoundingBox desiredBox = default)
        {
            // default case is everyhting
            bool useRealBox = (desiredBox == default);
            if (useRealBox)
            {
                desiredBox = new BoundingBox(new Vector3(float.MinValue), new Vector3(float.MaxValue));
            }

            // build indexes and bounding bos for the root by traversing all
            List<int> rootIndexes = new List<int>();
            BoundingBox realBox = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue)); // set as nothing
            for (int i = 0; i < boundaries.Count; i++)
            {
                if(desiredBox.Contains(boundaries[i].Boundaries) != ContainmentType.Disjoint)
                {   // ATTENTION and yes, the same index can be repeated in multiple nodes when in the borders
                    rootIndexes.Add(i);
                    realBox = BoundingBox.CreateMerged(realBox, boundaries[i].Boundaries);
                }
            }

            // if we are still bigger than the desired size, recursion go go go!
            List<IndexQuadTree> rootChildren = new List<IndexQuadTree>();
            BoundingBox[] childrenBoxes = (useRealBox) ? realBox.Divide() : desiredBox.Divide();
            if (childrenBoxes[0].Width() > desiredLeafSize && childrenBoxes[0].Height() > desiredLeafSize)
            {
                for (int i = 0; i < 4; i++)
                {
                    rootChildren.Add(CreateFromBoundaries(boundaries, desiredLeafSize, childrenBoxes[i]));
                }
            }
            
            // return root
            return new IndexQuadTree(rootIndexes, (useRealBox) ? realBox : desiredBox, rootChildren);
        }
    }
}
