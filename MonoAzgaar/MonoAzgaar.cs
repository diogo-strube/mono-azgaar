using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;

namespace MonoAzgaar
{
    /// <summary>
    /// Render large Azgaar worlds as a 3d game.
    /// </summary>
    public class MonoAzgaar : Game
    {
        /// <summary>
        /// Singleton to provide shared access to all components.
        /// </summary>
        public static MonoAzgaar Instance { get; protected set; }

        /// <summary>
        /// Batch used for optimized rendering of tiles.
        /// </summary>
        public SpriteBatch SpriteBatch { get; protected set; }

        /// <summary>
        /// Pan-like camera to navigate the world.
        /// </summary>
        public Camera Camera { get; protected set; }

        /// <summary>
        /// Manages user mouse input and maps it to the world coordinates.
        /// </summary>
        public MouseInput Input { get; protected set; }

        /// <summary>
        /// Map containing all data parsed by the Azgaar Map Parser
        /// </summary>
        public Map Map { get; protected set; }

        /// <summary>
        /// Collecton all cities (including towns, villages, etc) in the map.
        /// </summary>
        public CityCollection Cities { get; protected set; }

        /// <summary>
        /// UI Desktop Manager
        /// </summary>
        public Desktop Desktop { get; protected set; }

        /// <summary>
        /// Stats counter used to evaluate and draw performance
        /// </summary>
        private StatsCounter _statsCounter;

        /// <summary>
        /// Internal interface to the graphic device used for rendering.
        /// </summary>
        private GraphicsDeviceManager _graphics;

        /// <summary>
        /// Path to the parsed map pkg (.zip file)
        /// </summary>
        private string _mapPath;

        public MonoAzgaar(string[] args)
        {
            // keep constructor at minimal
            _mapPath = args[0];
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
        }

        protected override void Initialize()
        {
            // male game almost full screen
            _graphics.PreferredBackBufferWidth = (int)((double)GraphicsDevice.DisplayMode.Width * 0.75);
            _graphics.PreferredBackBufferHeight = (int)((double)GraphicsDevice.DisplayMode.Height * 0.75);

            // disable v-sync
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            // initalize batch with updated graphics device
            _graphics.ApplyChanges();
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // initialize UI and input
            MyraEnvironment.Game = this;
            Desktop = new Desktop();
            Input = new MouseInput();

            // load map from disk
            Map = Map.FromFile(_mapPath);
            Cities = new CityCollection(Map.World);

            // start camera at middle of the map
            Camera = new Camera(GraphicsDevice.Viewport, new Vector2(Map.World.Size.X / 2, Map.World.Size.Y / 2));

            // performance counters to evaluate if we are doing a good job rendering huge maps
            _statsCounter = new StatsCounter();
        }


        protected override void Update(GameTime gameTime)
        {
            // exit application if desired
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // update camera and handle input
            Camera.UpdateCamera(GraphicsDevice.Viewport);
            Input.Update();

            // measue performance
            _statsCounter.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(70, 110, 171)); // deep ocean blue color

            // render the world
            Map.RenderLand(gameTime);
            Map.RenderReliefs(gameTime);

            // then draw all labels
            Cities.Render(gameTime);

            // overlay the UI
            Desktop.Render();

            // and finally show the performance counters
            _statsCounter.DrawStats();

            base.Draw(gameTime);
        }
    }
}
