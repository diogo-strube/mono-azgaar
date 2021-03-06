# Mono Azgaar
Mono Azgaar is a tool for rendering huge maps created with (Azgaar Fantasy Map Generator)[https://azgaar.github.io/Fantasy-Map-Generator/].
This tool uses (MonoGame)[https://www.monogame.net/] to render a preprocessed map with performance as the main goal. As gaming/graphics technologies are used, cool animations and effects can be added in the future.

## How to use it
Mono Azgaar requires parsing your map first so that a 3d-optimized representation is created. You can parse your map using AzgaarMapParsing.exe tool:
![AzgaarMapParser_example](https://user-images.githubusercontent.com/19294348/150850790-db282c55-f245-4a9a-a58a-f57297c80c49.png)

With the package created, you can use Mono Azgaar to visualize and navigate your huge maps:
![MonoAzgaar_example](https://user-images.githubusercontent.com/19294348/150850819-c34803c7-e680-4285-91be-f79432943515.png)

This video tells why the project was created and how to use it:
[![Video talking about the project](https://img.youtube.com/vi/Ge0c0MR2h8g/0.jpg)](https://www.youtube.com/watch?v=Ge0c0MR2h8g)

## Attention
Please notice this project is in a very early stage! It originally started as a module for the guild simulation gaming I am coding, but after noticing the potential of having an Azgaar Map viewer using MonoGames, I separated this logic to start a dedicated project with the Azgaar community.

## Issues
There are a few known issues:
- Label of Capitals shows twice (one drawn with the small font and another with the large font).
- Water animation is not affecting the background dark-blue ocean.
- Order layers are toggled in Azgaar change the SVG and affect rendered tiles.
- Tiles with a very small amount of water are being culled off.

## Technical Disclaimer
This project source code was separated from the guild simulation game I am making. So, some unrelated comments or ambiguous class/entity names may still be lingering around.
