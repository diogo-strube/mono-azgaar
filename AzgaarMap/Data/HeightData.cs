using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzgaarMap.Data
{
    [DataContract]
    public class HeightData
    {
        [DataMember]
        public List<float> Values { get; set; } = new List<float>();
        [DataMember]
        public List<Vector3> Colors { get; set; } = new List<Vector3>();
    }
}
