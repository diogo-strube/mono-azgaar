using CommandLine;

namespace AzgaarMapParser
{
    /// <summary>
    /// Collection of options exposed as arguments to the caller.
    /// </summary>
    internal class Options
    {
        /// <summary>
        /// Path to the .map file saved from Azgaar.
        /// </summary>
        [Option('m', "map", Required = true, HelpText = "Path to Azgaar map.")]
        public string Map { get; set; }

        /// <summary>
        /// Path to the .svg file saved from Azgaar.
        /// </summary>
        /// <remarks>
        /// Currently required due to rendering issues in the SVG embedded in .map file.
        /// </remarks>
        [Option('s', "svg", Required = true, HelpText = "Path to and SVG exported by Azgaar.")]
        public string Svg { get; set; }

        /// <summary>
        /// Folder where output will be saved to
        /// </summary>
        [Option('o', "output", Required = true, HelpText = "Output folder where map will be placed.")]
        public string Output { get; set; }

        /// <summary>
        /// Desired width for the outputed map.
        /// </summary>
        /// <remarks>
        /// Maps to the rendered SVG canvas size and the total width (in pixel) of the output.
        /// </remarks>
        [Option('w', "width", Required = false, HelpText = "Desired width for the map (may be different to respect aspect ratio)[default 12288].")]
        public int Width { get; set; } = 12288 /*1024 * 12*/; // perfect for a canvas of width 1536

        /// <summary>
        /// Desired height for the outputed map.
        /// </summary>
        /// <remarks>
        /// Maps to the rendered SVG canvas size and the total height (in pixel) of the output.
        /// </remarks>
        [Option('h', "height", Required = false, HelpText = "Desired height for the map (may be different to respect aspect ratio)[default 8192].")]
        public int Height { get; set; } = 8192 /*1024 * 8*/; // perfect for a canvas of height 1024

        /// <summary>
        /// Size used for the tile (quad-tree divided space used for rendering).
        /// </summary>
        [Option('t', "tile", Required = false, HelpText = "Size of the tile (used as Step in several operations)[default 512].")]
        public int Tile { get; set; } = 512;

        /// <summary>
        /// Name of the cleaned svg extracted from the Azgaar .map file.
        /// </summary>
        public string CleanMap { get; set; } = "clean_map.svg";

        /// <summary>
        /// Temporary path used to store all the parsing artifacts.
        /// </summary>
        public string TmpPath { get; set; }

        /// <summary>
        /// Overriding ToString for debugging quality of life.
        /// </summary>
        public override string ToString()
        {
            return $"Map = {Map}\nSvg = {Svg}\nOutput = {Output}\nWidth = {Width}\nHeight = {Height}\nTile = {Tile}";
        }
    }
}
