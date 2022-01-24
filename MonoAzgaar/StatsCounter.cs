using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAzgaar
{
    /// <summary>
    /// Measure and draws performance counters
    /// </summary>
    public class StatsCounter
    {
        private double _frames = 0;
        private double _updates = 0;
        private double _elapsed = 0;
        private double _last = 0;
        private double _now = 0;
        public string _msg = "";

        /// <summary>
        /// Updates performance counter
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // as implemented by community at
            // https://community.monogame.net/t/a-simple-monogame-fps-display-class/10545
            _now = gameTime.TotalGameTime.TotalSeconds;
            _elapsed = (double)(_now - _last);
            if (_elapsed > 1.0f)
            {
                _msg = "Move with AWSD keys, zoom with mouse wheel, quit with ESC." +
                    "\nFps: " + (_frames / _elapsed).ToString() +
                    "\n Elapsed time: " + _elapsed.ToString() +
                    "\n Updates: " + _updates.ToString() +
                    "\n Frames: " + _frames.ToString();
                _elapsed = 0;
                _frames = 0;
                _updates = 0;
                _last = _now;
            }
            _updates++;
        }

        /// <summary>
        /// Draws performance counters
        /// </summary>
        public void DrawStats()
        {
            MonoAzgaar.Instance.SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.AnisotropicClamp, depthStencilState: DepthStencilState.Default);
            MonoAzgaar.Instance.SpriteBatch.DrawString(MonoAzgaar.Instance.Cities.FontTown, _msg, new Vector2(10, 10), Color.OrangeRed);
            MonoAzgaar.Instance.SpriteBatch.End();
            _frames++;
        }
    }
}
