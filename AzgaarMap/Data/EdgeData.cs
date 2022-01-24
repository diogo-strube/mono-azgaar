using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzgaarMap.Data
{
    [DataContract]
    public class EdgeData
    {
        [DataMember]
        public int VertexCount { get; protected set; }
        [DataMember]
        public List<List<Vector2>> Corners { get; protected set; } = new List<List<Vector2>>();
        [DataMember]
        public List<Vector2> Centroids { get; protected set; } = new List<Vector2>();

        public void Add(List<Vector2> edges, Vector2 centroids)
        {
            VertexCount += edges.Count + 1 /*centroid*/;
            Corners.Add(edges);
            Centroids.Add(centroids);
        }
    }
}
