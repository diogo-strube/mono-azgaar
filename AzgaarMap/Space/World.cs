using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using AzgaarMap.Data;
using AzgaarMap.Render;
using Microsoft.Xna.Framework;

namespace AzgaarMap.Space
{
    [DataContract]
    public class World
    {
        [DataMember]
        public CellCollection Cells { get; protected set; }
        [DataMember]
        public Vector3[] HeightColors { get; protected set; }
        [DataMember]
        public Vector3[] BiomeColors { get; protected set; }
        [DataMember]
        public string[] BiomeNames { get; protected set; }
        [DataMember]
        public CityData[] Cities { get; protected set; }
        [DataMember]
        public Vector2 Size { get; protected set; }
        [DataMember]
        public Vector2 Scale { get; protected set; }
        [DataMember]
        public int Step { get; protected set; }

        public static World FromFile(string file)
        {
            using (FileStream stream = File.Open(file, FileMode.Open))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(World));
                return (World)serializer.ReadObject(stream);
            }
        }

        public World(Vector2 size, Vector2 scale, int step, WorldData world_info, List<CityData> city_info)
        {
            Size = size;
            Scale = scale;
            Step = step;
            Cities = city_info.ToArray();

            // build all cells, from the geometry to a quadtree for optimized basic traversal
            Cells = BuildCellCollecton(world_info);

            // store cell layer related data used for visualization and UI
            HeightColors = world_info.Heights.Colors.ToArray();
            BiomeColors = world_info.Biomes.Colors.ToArray();
            BiomeNames = world_info.Biomes.Names.ToArray();
        }

        public CellCollection BuildCellCollecton(WorldData world_info)
        {
            // build vertices from points
            CellVertex[] vertices = new CellVertex[world_info.Edges.VertexCount];
            List<int> indices = new List<int>();
            int index = 0;
            List<Cell> cellList = new List<Cell>();
            for (int i = 0; i < world_info.Edges.Corners.Count; i++)
            {
                List<Vector2> cell_edges = world_info.Edges.Corners[i];
                int start = index;

                // add corner centroid first
                float height = world_info.Heights.Values[i];
                float biome = world_info.Biomes.Values[i];
                Vector2 centroid = world_info.Edges.Centroids[i];
                vertices[index++] = BuildVertice(centroid.X, centroid.Y, Color.Black, height, biome);

                // followed by all edges around the cell boundaries
                foreach (Vector2 edge in cell_edges)
                {
                    height = world_info.Heights.Values[i];
                    biome = world_info.Biomes.Values[i];
                    vertices[index++] = BuildVertice(edge.X, edge.Y, Color.Black, height, biome);
                }
                vertices[start].Color = Color.Black; // set centroid as black

                // build indices from vertices (triangle list)
                int length = index - start;
                indices.AddRange(BuildIndices(start, length));

                // add the newest cell to the list
                cellList.Add(new Cell(start, length));
            }

            return new CellCollection(vertices, indices.ToArray(), cellList.ToArray(), Step);
        }

        /// <summary>
        /// Build triangles using a triangle list approach.
        /// </summary>
        protected int[] BuildIndices(int start, int length)
        {
            //    [start + (j - 1)]
            //          /  \
            //         /    \
            //        /      \
            // [start]--------[start + j]
            int[] indices = new int[(length - 1) * 3];
            int index = 0;
            // TODO: we should be checking the posotions to make sure we build everyhting clockwise.
            // This way the CounterClockwise culling would work.
            for (int j = 2; j < length; j++)
            {
                indices[index++] = start;
                indices[index++] = start + j;
                indices[index++] = start + (j - 1);
            }
            indices[index++] = start;
            indices[index++] = start + 1;
            indices[index++] = start + (length - 1);
            return indices;
        }

        protected CellVertex BuildVertice(float x, float y, Color color, float height = 0f, float bioma = 0f, float state = 0f)
        {
            return new CellVertex(
                new Vector3(x * Scale.X, y * Scale.Y, 0f), // TODO: should we scale here or when we are creating the edges?
                color,
                new Vector2(x * Scale.X / (float)Size.X, y * Scale.Y / (float)Size.Y),
                new Vector2(height, bioma) // , state * 255.0f, 255.0f
            );
        }

        public void Save(string file)
        {
            using (FileStream stream = File.Create(file))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(World));
                serializer.WriteObject(stream, this);
            }
        }
    }
}
