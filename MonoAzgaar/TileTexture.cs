using AzgaarMap.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace MonoAzgaar
{
    /// <summary>
    /// Texture representing a tile of the Azgaar world.
    /// </summary>
    public class TileTexture : IBoundary
    {
        /// <summary>
        /// Boundaries (bounding box) of the tile.
        /// </summary>
        public BoundingBox Boundaries { get; protected set; }

        /// <summary>
        /// Evaluates if the given point in inside the tile.
        /// </summary>
        public bool Contains(Vector3 pos)
        {
            return (Boundaries.Contains(pos) != ContainmentType.Disjoint);
        }

        /// <summary>
        /// Texture representing this tile.
        /// </summary>
        public Texture2D Texture { get; protected set; }

        /// <summary>
        /// Construct a tile texture (protected, call FromDir instead).
        /// </summary>
        protected TileTexture(float x, float y, Texture2D texture) : base()
        {
            Boundaries = new BoundingBox(new Vector3(x, y, 0f), new Vector3(x + texture.Width, y + texture.Height, 0f));
            Texture = texture;
        }

        /// <summary>
        /// Load all tile textures inside a directory.
        /// </summary>
        public static List<TileTexture> FromDir(string dir, string id)
        {
            List<TileTexture> tiles = new List<TileTexture>();
            var files = Directory.EnumerateFiles(dir, $"*.png");
            foreach (var file in files)
            {
                string file_name = Path.GetFileNameWithoutExtension(file);
                if (file_name.Contains(id))
                {
                    string[] tile_data = file_name.Split(id)[1].Split('_');
                    int w = Convert.ToInt32(tile_data[1]);
                    int h = Convert.ToInt32(tile_data[2]);
                    tiles.Add(new TileTexture(w, h, Texture2D.FromFile(MonoAzgaar.Instance.GraphicsDevice, file)));
                }
            }
            return tiles;
        }
    }
}
