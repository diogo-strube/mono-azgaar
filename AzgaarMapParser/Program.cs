using AzgaarMap.Data;
using AzgaarMap.Space;
using AzgaarMap.Extensions;
using CommandLine;
using ImageMagick;
using Microsoft.Xna.Framework;
using Svg;
using SvgPathProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AzgaarMapParser
{
    class Program
    {
        /// <summary>
        /// Main routing arguments to the Command Line parser (returns text help/info if invalid args)
        /// </summary>
        static void Main(string[] args)
        {
#if DEBUG // In debug allow unhandled exception for visibility
            CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
            Console.WriteLine($"Map successfully created :)");
            ParseHelper.Dispose();
#else // In release avoid crashes and share details to the user
            try
            {
                CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
                Console.WriteLine($"Map successfully created :)");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                ParseHelper.Dispose();
            }
#endif
        }

        /// <summary>
        /// Parse an Azgaar map with the provided arguments (aka Options).
        /// </summary>
        internal static void Run(Options opts)
        {
            Console.WriteLine($"Executing with arguments\n{opts}");

            // creates a temporary folder where we place all generated files before we package (zip) it
            CreateTmpFolder(opts);

            // clean the .map file (by exporting the embedded svg)
            CleanMap(opts);

            // 1. export tiles first as image related operations already validate the final width and height of the map
            // doing this first gives us more flexibility with the aspect ratio and the canvas size used on Azgaar
            Vector2 map_size = ParseMapTiles(opts);
            Vector2 map_scale = CalculateScale(opts, map_size);
            ParseReliefTiles(opts, map_size);

            // 2. extract world contours/boundaries for land (continents and islands), lakes and states
            // we will use this data to parse cells and group points of interest (like cities)
            List<List<Vector2>> land_edges = ExtractContour(opts, "sea_island");
            List<List<Vector2>> lake_edges = ExtractContour(opts, "lakes");
            List<List<Vector2>> state_edges = ExtractContour(opts, "statesBody", new Regex("gap[0-9]*"));

            // 3. extract info about the world (including voronoi cells):
            WorldData world_data = new WorldData();
            // 3.1 - extract all the points making the thousands of cells in the Azgaar map
            ExtractCellEdges(opts, ref world_data);
            // 3.1.1 - filter cells in undesired positions as a separate step
            // TODO: this should be optional, we never know if folks will want cells in water or other places
            FilterCellEdges(opts, ref world_data, land_edges, lake_edges);
            // 3.2 - extract the height for each cell
            ExtractCellHeights(opts, world_data);
            // 3.3 - extract the biome for each cell
            ExtractCellBiomes(opts, world_data);

            // 4. extract all cities (including towns)
            List<CityData> city_info = CreateCities(opts, map_scale);

            // 5. create our 3D representation of the Azgaar Map \o/
            World map = new World(map_size, map_scale, opts.Tile, world_data, city_info);
            string path_to_map = Path.Combine(opts.TmpPath, "world.bin");
            map.Save(path_to_map);

            // 6. zip it to the output and delete tmp files
            CreateMapPkg(opts);
        }

        /// <summary>
        /// Create a temporary directory to be used during the map creation process.
        /// </summary>
        internal static void CreateTmpFolder(Options opts)
        {
            // generate path if none was provided
            if (opts.TmpPath == null || opts.TmpPath == string.Empty)
            {
                opts.TmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            }

            // avoid messing up with existing files
            if (Directory.Exists(opts.TmpPath))
            {
                throw new Exception($"Temporary folder '{opts.TmpPath}' already exists, stopping execution to avoid deleting existing files");
            }

            // create temp directory
            Directory.CreateDirectory(opts.TmpPath);
            Console.WriteLine($"Temporary path created = '{opts.TmpPath}'");
        }

        /// <summary>
        /// Create and .svg file with the svg content that is embedded inside the Azgaar .map file.
        /// </summary>
        /// <remarks>
        /// Required as some of the information may be only available in the .map file as the .svg may have layers disabled for visual reasons.
        /// </remarks>
        internal static void CleanMap(Options opts)
        {
            string map_content = File.ReadAllText(opts.Map);
            int start = map_content.IndexOf("<svg");
            int end = map_content.IndexOf("</svg", start + 1);
            string cleaned_map_content = map_content.Substring(start, (end + 6) - start);
            opts.CleanMap = Path.Combine(opts.TmpPath, "clean_map.svg");
            File.WriteAllText(opts.CleanMap, cleaned_map_content);
            Console.WriteLine($"Clean map created = '{opts.CleanMap}'");
        }

        /// <summary>
        /// Render the SVG file and tile it so a collection of textures are created.
        /// </summary>
        internal static Vector2 ParseMapTiles(Options opts)
        {
            try
            {
                // create the full resolution map by splitting it into several tiles
                // but we start by loading the SVG as an image with ImageMagick, as it will allow making operations in the image later.
                var settings = new MagickReadSettings();
                settings.Width = opts.Width;
                settings.Height = opts.Height;
                settings.Format = MagickFormat.Svg;
                using (var land_map = new MagickImage(opts.Svg, settings))
                {
                    // let's list  all the widths we will step through.
                    var computed_w = new List<int>();
                    for (int w = 0; w < land_map.BaseWidth; w += opts.Tile)
                    {
                        computed_w.Add(w);
                    }
                    // so we can run in parallel the tile generation
                    Parallel.ForEach(computed_w,
                       w =>
                       {
                           for (int h = 0; h < land_map.BaseHeight; h += opts.Tile)
                           {
                               // crop a land tile in the coords (w,h) from the image
                               IMagickImage<byte> tile = ParseHelper.CropAndCheck(land_map, w, h, opts.Tile, 7, MagickColor.FromRgb(70, 110, 171) /*ocean color*/);
                               if (tile != null)
                               {   // filter any water and save it to disk if we had a valid tile in this coords (w,h)
                                   tile.Format = MagickFormat.Png;
                                   tile.ColorFuzz = new Percentage(7);
                                   tile.Transparent(MagickColor.FromRgb(70, 110, 171)); // ocean color, same as (ushort)17990
                                   tile.Write(Path.Combine(opts.TmpPath, $"land_tile_{w}_{h}.png"), MagickFormat.Png);
                               }

                           }
                       });
                    Vector2 map_size = new Vector2(land_map.Width, land_map.Height);
                    Console.WriteLine($"All land tiles created for a total dimensios of {map_size}");
                    return map_size;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create land tiles: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Calculate the map scale factor using the SVG canvas size.
        /// </summary>
        internal static Vector2 CalculateScale(Options opts, Vector2 size)
        {
            SvgDocument map_svg = SvgDocument.Open<SvgDocument>(opts.Svg);
            string svg_width, svg_height;
            if (map_svg.TryGetAttribute("width", out svg_width) && map_svg.TryGetAttribute("height", out svg_height))
            {
                Vector2 scale = new Vector2(
                    (float)size.X / (float)Convert.ToDouble(svg_width),
                    (float)size.Y / (float)Convert.ToDouble(svg_height)
                );
                Console.WriteLine($"Map scale factor is {scale}");
                return scale;
            }
            throw new Exception("No Canvas Width information was found in the provided SVG file");
        }

        /// <summary>
        /// Create the compressed map package.
        /// </summary>
        internal static void CreateMapPkg(Options opts)
        {
            string zip_path = Path.Combine(opts.Output, "map.zip");
            if (File.Exists(zip_path))
            {   // replace file if needed
                File.Delete(zip_path);
                Console.WriteLine($"Deleted existing file '{zip_path}'");
            }

            ZipFile.CreateFromDirectory(opts.TmpPath, zip_path, CompressionLevel.Optimal, false);
            Console.WriteLine($"Created map package at'{zip_path}'");

            // delete temp data
            Directory.Delete(opts.TmpPath, true);
            Console.WriteLine($"Deleted temporary folder '{opts.TmpPath}'");
        }

        static List<CityData> CreateCities(Options opts, Vector2 map_scale)
        {
            var labels = ParseHelper.GetNodeChildrens(opts.CleanMap, "burgLabels");
            List<CityData> cities = new List<CityData>();
            int population = 100; // difference between citiy and town is population
            for (int i = 0; i < labels.Count; i++)
            {
                // one node for cities, another for towns
                for (int j = 0; j < labels[i].ChildNodes.Count; j++)
                {
                    //string id = circles[i].Attributes["id"].Value.Replace("burg", "burgLabel");
                    CityData city = new CityData();
                    city.Name = labels[i].ChildNodes[j].InnerText;
                    float x = (float)Convert.ToDouble(labels[i].ChildNodes[j].Attributes["x"].Value) * map_scale.X;
                    float y = (float)Convert.ToDouble(labels[i].ChildNodes[j].Attributes["y"].Value) * map_scale.Y;
                    city.Location = new Vector2(x, y);
                    city.Population = population;
                    cities.Add(city);
                }
                population /= 2;
            }
            return cities;
        }

        /// <summary>
        /// Reads a contour element from the cleaned .map file
        /// </summary>
        /// <remarks>
        /// Elements representing a contour have a collection of 'path' children.
        /// </remarks>
        private static List<List<Vector2>> ExtractContour(Options opts, string id, Regex reject_paths = default(Regex))
        {
            List<List<Vector2>> result = new List<List<Vector2>>();
            XmlNodeList elements = ParseHelper.GetNodeChildrens(opts.CleanMap, id);
            foreach (XmlNode ele in elements)
            {   // traverse all the children for the element with the given 'id'
                if (ele.Name.ToLower() != "path")
                {   // if this is not a path, we keep recursion going
                    if (ele.Attributes["id"] != null)
                    {
                        result.AddRange(ExtractContour(opts, ele.Attributes["id"].Value));
                    }
                }
                else
                {
                    if (reject_paths == null || ele.Attributes["id"] == null || !reject_paths.IsMatch(ele.Attributes["id"].Value))
                    {
                        // whenever we find a path that is not to be rejected, we parse it
                        SVGPathProperties ele_path = new SVGPathProperties(ele.Attributes["d"].Value);
                        List<Vector2> ele_edges = new List<Vector2>((int)ele_path.GetTotalLength() / 2);
                        for (int i = 0; i < ele_path.GetTotalLength(); i += 2) // using += 2 as full resolution is not needed
                        {   // we traverse the SVG Path to make sure all the SVG supported command and curves are respected
                            SvgPathProperties.Base.Point point = ele_path.GetPointAtLength(i);
                            ele_edges.Add(new Vector2((float)point.X, (float)point.Y));
                        }
                        result.Add(ele_edges);
                    }
                }
            }
            Console.WriteLine($"Extracted {result.Count} contours from {id}");
            return result;
        }

        /// <summary>
        /// Extract the the geometry for each cell.
        /// </summary>
        /// <remarks>
        /// The geometry is made of a centroid vertice, followed by a vertice for each edge of the voronoi cell.
        /// Notice that if a region is provided, only cells inside the region will be extracted.
        /// </remarks>
        private static void ExtractCellEdges(Options opts, ref WorldData info, List<List<Vector2>> regions = null)
        {
            // go over all edges described in the svg 'd' attributes
            XmlNode cells_element = ParseHelper.GetNodeChildrens(opts.CleanMap, "cells")[0];
            string d_element = cells_element.Attributes["d"].Value.ToLower();
            string[] sub_paths = d_element.Split('m');
            foreach (var path in sub_paths)
            {
                if (path != string.Empty)
                {
                    string[] coords = path.Split(',');
                    float[] converted_coords = Array.ConvertAll(coords, s => (float)Convert.ToDouble(s));
                    List<Vector2> edges2d = new List<Vector2>(converted_coords.Length / 2);
                    for (int i = 0; i < converted_coords.Length; i += 2)
                    {
                        edges2d.Add(new Vector2(converted_coords[i], converted_coords[i + 1]));
                    }
                    info.Edges.Add(edges2d, edges2d.CalculateCentroid());
                }
            }
        }

        /// <summary>
        /// Filter cells according to provide geometries.
        /// </summary>
        private static void FilterCellEdges(Options opts, ref WorldData info, List<List<Vector2>> accept, List<List<Vector2>> reject = null)
        {
            int index = 0; // traversal with a while as we will remove elements on the fly (during loop)
            while (index < info.Edges.Centroids.Count)
            {
                bool keep = false;
                Vector2 centroid = info.Edges.Centroids[index];

                // start checking if we are inside the accepted regions
                foreach (List<Vector2> region in accept)
                {
                    if (ParseHelper.Inside(region, centroid))
                    {
                        keep = true;
                        break;
                    }
                }

                // and check if we are inside rejected regions
                if (keep && reject != null)
                {
                    foreach (List<Vector2> region in reject)
                    {
                        if (ParseHelper.Inside(region, centroid))
                        {
                            keep = false;
                            break;
                        }
                    }
                }

                // and increase index or remove as needed
                if (keep)
                {
                    index++;
                }
                else
                {
                    info.Edges.Corners.RemoveAt(index);
                    info.Edges.Centroids.RemoveAt(index);
                }
            }
        }

        /// <summary>
        ///  Extract the height information for all world cells.
        /// </summary>
        static void ExtractCellHeights(Options opts, WorldData info)
        {
            // create a dictionary mapping the known heights to the colores set in the svg fill
            Dictionary<System.Drawing.Color, int> height_color_map = new Dictionary<System.Drawing.Color, int>();
            XmlNodeList height_elements = ParseHelper.GetNodeChildrens(opts.CleanMap, "terrs");
            foreach (XmlNode element in height_elements)
            {
                if (element.Name == "path")
                {
                    string[] rgb_string = element.Attributes["fill"].Value.Split('(', ')', ',');
                    Vector3 rgb_vector = new Vector3(
                        Convert.ToInt32(rgb_string[1].Trim()),
                        Convert.ToInt32(rgb_string[2].Trim()),
                        Convert.ToInt32(rgb_string[3].Trim())
                    );
                    System.Drawing.Color color = System.Drawing.Color.FromArgb((int)rgb_vector.X, (int)rgb_vector.Y, (int)rgb_vector.Z);
                    info.Heights.Colors.Add(rgb_vector / 255.0f); // normalize between 0..1
                    int height = Convert.ToInt32(element.Attributes["data-height"].Value);
                    height_color_map.Add(color, height);
                }
            }

            // draw heightmap image in memory
            Bitmap heightmap_bitmap = ParseHelper.BitmapFromElement(opts.CleanMap, "terrs");

            // check each cell centroid agains the heightmap bitmap
            foreach (Vector2 centroid in info.Edges.Centroids)
            {
                System.Drawing.Color color = heightmap_bitmap.GetPixel((int)centroid.X, (int)centroid.Y);
                info.Heights.Values.Add((float)height_color_map[color] / 100.0f); // normalize height to a 0..1 interval
            }
        }

        /// <summary>
        /// Extract the biome information for all world cells.
        /// </summary>
        static void ExtractCellBiomes(Options opts, WorldData info)
        {
            // Extract biome colors and names from the map header data
            List<string> map_lines = new List<string>(File.ReadLines(opts.Map));
            string[] biome_data = map_lines[3].Split("|");

            // parse names
            info.Biomes.Names.AddRange(biome_data[2].Split(","));
            info.Biomes.Names.Add("Undefined"); // undefined covering any cell in water or mysterious places

            // parse hex colors (and map colors back t the biome)
            string[] biome_colors = biome_data[0].Split(",");
            Dictionary<System.Drawing.Color, float> biome_color_map = new Dictionary<System.Drawing.Color, float>();
            foreach (string hex_color in biome_colors)
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hex_color);
                biome_color_map.Add(color, (float)info.Biomes.Colors.Count * 0.5f); // index steping 0.5f (supportin 20 biomes max)
                info.Biomes.Colors.Add(new Vector3(color.R, color.G, color.B) / 255.0f);
            }
            System.Drawing.Color black = System.Drawing.Color.Black;
            biome_color_map.Add(black, (float)info.Biomes.Colors.Count * 0.5f);
            info.Biomes.Colors.Add(new Vector3(black.R, black.G, black.B)); // black for undefined

            // draw biomes in memory
            Bitmap biomes_bitmap = ParseHelper.BitmapFromElement(opts.CleanMap, "biomes");
            biomes_bitmap.Save(Path.Combine(opts.TmpPath, "test_biome.png"), System.Drawing.Imaging.ImageFormat.Png);

            // check each cell centroid agains the biome bitmap
            foreach (Vector2 centroid in info.Edges.Centroids)
            {
                System.Drawing.Color color = biomes_bitmap.GetPixel((int)centroid.X, (int)centroid.Y);
                if (biome_color_map.ContainsKey(color))
                {
                    info.Biomes.Values.Add(biome_color_map[color]);
                }
                else
                {
                    info.Biomes.Values.Add(biome_color_map[System.Drawing.Color.Black]); // undefined
                }
            }
        }

        /// <summary>
        /// Parse relief information as tiles (texture images).
        /// </summary>
        static void ParseReliefTiles(Options opts, Vector2 map_size)
        {
            // render only the reliefs and save to disk disk
            string path_full_relief = Path.Combine(opts.TmpPath, "relief_map.png");
            Bitmap relief_bitmap = ParseHelper.BitmapFromElement(opts.CleanMap, "terrain", map_size);
            relief_bitmap.Save(path_full_relief, System.Drawing.Imaging.ImageFormat.Png);

            // create the full resolution map by splitting it into several tiles
            // but we start by loading the SVG as an image with ImageMagick, as it will allow making operations in the image later.
            var settings = new MagickReadSettings();
            settings.Width = (int)map_size.X;
            settings.Height = (int)map_size.Y;
            settings.Format = MagickFormat.Png;

            using (var relief_map = new MagickImage(path_full_relief, settings))
            {
                // let's decide for all the widths we will step through.
                var computed_w = new List<int>();
                for (int w = 0; w < relief_map.BaseWidth; w += opts.Tile)
                {
                    computed_w.Add(w);
                }
                // so we can run in parallel the tile generation
                Parallel.ForEach(computed_w,
                   w =>
                   {
                       for (int h = 0; h < relief_map.BaseHeight; h += opts.Tile)
                       {
                           // crop a relief tile in the coords (w,h) from the image
                           IMagickImage<byte> tile = ParseHelper.CropAndCheck(relief_map, w, h, opts.Tile, 7, MagickColors.Black /*no reliefs inside*/);
                           if (tile != null)
                           {   // and save it to disk if we had a valid tile in this coords (w,h)
                               tile.Write(Path.Combine(opts.TmpPath, $"relief_tile_{w}_{h}.png"), MagickFormat.Png);
                           }
                       }
                   });
            }

            Console.WriteLine($"All relief tiles created.");
        }
    }
}
