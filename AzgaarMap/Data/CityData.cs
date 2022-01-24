using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using AzgaarMap.Space;

namespace AzgaarMap.Data
{
    [DataContract(IsReference = true)]
    public class CityData
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Vector2 Location { get; set; }
        [DataMember]
        public int Population { get; set; }
    }
}
