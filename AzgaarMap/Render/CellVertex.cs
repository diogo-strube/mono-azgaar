using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace AzgaarMap.Render
{
    /// <summary>
    /// Vertex Definition used for rendering.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DataContract]
    public struct CellVertex : IVertexType
    {
        /// <summary>
        /// World position of vertex.
        /// </summary>
        /// <remarks>
        /// Between (0, 0) and (map_width, map_height).
        /// </remarks>
        [DataMember]
        public Vector3 Position;

        /// <summary>
        /// Color of the vertex.
        /// </summary>
        /// <remarks>
        /// Texture is used for render, but color is leveraged for alpha and mouse over effects.
        /// </remarks>
        [DataMember]
        public Color Color;

        /// <summary>
        /// Texture coordinate based on the world position.
        /// </summary>
        /// <remarks>
        /// Start (top left) of the world are tx (0, 0) and end of world (top down) are tx (map_width, map_height).
        /// </remarks>
        [DataMember]
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Height and Biome packed as a secondary texture coordinate to be used in shader.
        /// </summary>
        [DataMember]
        public Vector2 HeightBioma;
        public static readonly VertexDeclaration VertexDeclaration;

        public CellVertex(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
            HeightBioma = Vector2.Zero;
        }

        public CellVertex(Vector3 position, Color color, Vector2 textureCoordinate, Vector2 heightBiomaState)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
            HeightBioma = heightBiomaState;
        }
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
                hashCode = (hashCode * 397) ^ HeightBioma.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position +
                " Color:" + this.Color +
                " TextureCoordinate:" + this.TextureCoordinate +
                " HeightBiomaState:" + this.HeightBioma + "}}";
        }

        public static bool operator ==(CellVertex left, CellVertex right)
        {
            return (((left.Position == right.Position) &&
                (left.Color == right.Color)) &&
                (left.TextureCoordinate == right.TextureCoordinate) &&
                (left.HeightBioma == right.HeightBioma));
        }

        public static bool operator !=(CellVertex left, CellVertex right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == ((CellVertex)obj));
        }

        static CellVertex()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
