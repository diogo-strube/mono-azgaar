# Azgaar Map Parser
This command-line tool can parse Azgaar worlds (.map and .svg files) to be viewed with MonoAzgaar.
The parsing process saves all the world information in data structures optimized for 3d rendering.
MonoAzgaar can then be used for rendering very large parsedAzgaar worlds with great performance.

## Tutorial
Just export the .map and .svg files from your awesome Azgaar Map and run this tool. Here is a video showing the process:

### Attention
This Azgaar Map loader has a few limitations that need to be considered when exporting maps from Azgaar:
- Both the Map file and the SVG files are need to exported (due to errors when trying to render the SVG embeed in the Map file)
 * When exporting the Map, at least the following Layers have to be selected: Heightmap, Biomes, Cells, Rivers, Relief, States, Borders, Routes, Labels, Icons
 * When exporting the SVG, at least the following Layers have to be selected: Texture, Rivers, Relief.

Also, please notice that the parsing process goes heavy on CPU and Memory, taking a few minutes to execute and using up to 6Gb of RAM.
Nevertheless, rendering the parsed map with MonoAzgaar is fast and usually uses less than 20% of the CPU and less than 100 Mb of RAM.

## Dependencies
The following third party projects are used (thanks for all heroes involved):
- [CommandLineParser](https://github.com/commandlineparser/commandline) for parsing all arguments provided to our tool.
- [Magick.NET-](https://github.com/dlemstra/Magick.NET) for rendering the full map in a tiled HD texture and perform color comparisons.
- [MonoGame](https://github.com/MonoGame/MonoGame) for definition of all core and base types.
- [Svg](https://github.com/svg-net/SVG) for traversing SVG elements and rendering only selected elements.
- [SvgPathProperties](https://github.com/zHaytam/SvgPathProperties) for converting all SVG paths to basic rendering geometries.