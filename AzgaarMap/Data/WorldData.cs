using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzgaarMap.Data
{
    [DataContract]
    public class WorldData
    {
        [DataMember]
        public EdgeData Edges { get; set; } = new EdgeData();
        [DataMember]
        public HeightData Heights { get; set; } = new HeightData();
        [DataMember]
        public BiomeData Biomes { get; set; } = new BiomeData();
        [DataMember]
        public List<float> States { get; set; } = new List<float>();
    }
}
