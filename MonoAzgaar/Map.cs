using AzgaarMap.Space;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MonoAzgaar
{
    public class Map
    {
        /// <summary>
        /// The world representation parsed by AzgaarMapParser.
        /// </summary>
        public World World { get; protected set; }

        /// <summary>
        /// A list containing all the land texture tiles.
        /// </summary>
        /// <remarks>
        /// Areas without land (just ocean water) don't exist in this collection.
        /// </remarks>
        public List<TileTexture> Tiles { get; protected set; }

        /// <summary>
        /// A list containing all the relief texture tiles.
        /// </summary>
        /// <remarks>
        /// Areas without reliefs don't exist in this collection.
        /// </remarks>
        public List<TileTexture> Reliefs { get; protected set; }

        /// <summary>
        /// Effects (shader) used for rendering relif tiles.
        /// </summary>
        private Effect _reliefFx;

        /// <summary>
        /// Effects (shader) used for rendering land tiles.
        /// </summary>
        private Effect _tileFx;

        /// <summary>
        /// Construct a Map (protected, call FromFile instead).
        /// </summary>
        protected Map(World world, List<TileTexture> tiles, List<TileTexture> reliefs)
        {
            // set all the content fro thr world
            World = world;
            Tiles = tiles;
            Reliefs = reliefs;

            // initialize all we need for rendering
            _tileFx = MonoAzgaar.Instance.Content.Load<Effect>($"tile");
            _reliefFx = MonoAzgaar.Instance.Content.Load<Effect>($"relief");
            _tileFx.Parameters["OceanTexture"].SetValue(MonoAzgaar.Instance.Content.Load<Texture2D>($"water_deep"));
        }

        /// <summary>
        /// Read the map file created with Azgaar Map Parser.
        /// </summary>
        public static Map FromFile(string map_zip_file_path)
        {
            // create temporary folder
            string package_path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            while (Directory.Exists(package_path)) // Murphy stopper... no way the file will exist multiple time with different GUIDs!
            {
                package_path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            }
            Directory.CreateDirectory(package_path);

            // uncompress package files (created by Azgaar Map Parser) to the tmp folder
            ZipFile.ExtractToDirectory(map_zip_file_path, package_path);

            // read all the parsed data about the world and return it as a Map
            return new Map(
                World.FromFile(Path.Combine(package_path, "world.bin")),
                TileTexture.FromDir(package_path, "land_tile"),
                TileTexture.FromDir(package_path, "relief_tile")
            );
        }

        /// <summary>
        /// Render all land tiles
        /// </summary>
        public void RenderLand(GameTime time)
        {
            // set shader ocean water animation value
            _tileFx.Parameters["animationTime"].SetValue((float)time.TotalGameTime.TotalSeconds);

            // render all tiles and leave culling to the Sprite Batch
            MonoAzgaar.Instance.SpriteBatch.Begin(
                sortMode: SpriteSortMode.Immediate,
                blendState: BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                transformMatrix: MonoAzgaar.Instance.Camera.Transform, // use camera tranformation
                effect: _tileFx // use land tile shader
            );  
            foreach (var tile in Tiles)
            {
                MonoAzgaar.Instance.SpriteBatch.Draw(tile.Texture, new Rectangle((int)tile.Boundaries.Min.X, (int)tile.Boundaries.Min.Y, World.Step, World.Step), Color.White);
            }
            MonoAzgaar.Instance.SpriteBatch.End();
        }

        /// <summary>
        /// Render all relief tiles
        /// </summary>
        public void RenderReliefs(GameTime time)
        {
            MonoAzgaar.Instance.SpriteBatch.Begin(
                sortMode: SpriteSortMode.Immediate,
                blendState: BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                transformMatrix: MonoAzgaar.Instance.Camera.Transform, // use camera tranformation
                effect: _reliefFx // use relief tile shader
            );
            foreach (var tile in Reliefs)
            {
                MonoAzgaar.Instance.SpriteBatch.Draw(tile.Texture, new Rectangle((int)tile.Boundaries.Min.X, (int)tile.Boundaries.Min.Y, World.Step, World.Step), Color.White);
            }
            MonoAzgaar.Instance.SpriteBatch.End();
        }
    }
}
