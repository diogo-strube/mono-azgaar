using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MonoAzgaar
{
    /// <summary>
    /// Handles the user mouse input and its relation to the world.
    /// </summary>
    public class MouseInput
    {
        /// <summary>
        /// Position of the mouse in the screen.
        /// </summary>
        public Vector3 MouseScreenPos { get; protected set; }

        /// <summary>
        /// Position of the mouse in the world.
        /// </summary>
        public Vector3 MouseWorldPos { get; protected set; }

        public void Update()
        {
            // update mouse position to screen and world
            MouseScreenPos = new Vector3(Mouse.GetState().Position.ToVector2(), 0f);
            MouseWorldPos = Vector3.Transform(MouseScreenPos, Matrix.Invert(MonoAzgaar.Instance.Camera.Transform));
        }
    }
}
