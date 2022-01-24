using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzgaarMap.Data
{
    [DataContract]
    public class BiomeData
    {
        [DataMember]
        public List<float> Values { get; set; } = new List<float>();
        [DataMember]
        public List<Vector3> Colors { get; set; } = new List<Vector3>();
        [DataMember]
        public List<string> Names { get; set; } = new List<string>();
    }
}
