using AzgaarMap.Data;
using AzgaarMap.Space;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAzgaar
{
    /// <summary>
    /// Handles all city names and text rendering
    /// </summary>
    public class CityCollection
    {
        /// <summary>
        /// Font used for rendering the name of cities.
        /// </summary>
        public SpriteFont FontCity { get; protected set; }

        /// <summary>
        /// Font used for rendering the name of towns.
        /// </summary>
        public SpriteFont FontTown { get; protected set; }

        /// <summary>
        /// All the cities (including towns, villages, etc) in the world.
        /// </summary>
        public CityData[] Cities { get; protected set; }

        /// <summary>
        /// Construct collection of Cities from the World data.
        /// </summary>
        public CityCollection(World world)
        {
            FontCity = MonoAzgaar.Instance.Content.Load<SpriteFont>("font-city");
            FontTown = MonoAzgaar.Instance.Content.Load<SpriteFont>("font-town");
            Cities = world.Cities;
        }

        /// <summary>
        /// Render the name of all cities in the world.
        /// </summary>
        public void Render(GameTime time)
        {
            // culling is handles by Sprite Batch, so we just call Draw and relax
            MonoAzgaar.Instance.SpriteBatch.Begin(
                sortMode: SpriteSortMode.Immediate,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.AnisotropicClamp,
                depthStencilState: DepthStencilState.Default,
                transformMatrix: MonoAzgaar.Instance.Camera.Transform // use camera tranformation
            );
            foreach (CityData city in Cities)
            {
                SpriteFont font = (city.Population > 50) ? FontCity : FontTown;
                float x_offset = font.MeasureString(city.Name).X / 2;
                MonoAzgaar.Instance.SpriteBatch.DrawString(font, city.Name, new Vector2(city.Location.X - x_offset, city.Location.Y), Color.Black);
            }
            MonoAzgaar.Instance.SpriteBatch.End();
        }
    }
}
