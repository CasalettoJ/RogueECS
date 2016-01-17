using ECSRogue.BaseEngine;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class DisplaySystem
    {
        public static void DrawDungeonEntities(StateSpaceComponents spaceComponents, Camera camera, SpriteBatch spriteBatch, Texture2D spriteSheet, int cellSize)
        {
            IEnumerable<Guid> drawableEntities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Drawable) == ComponentMasks.Drawable).Select(x => x.Id);
            foreach(Guid id in drawableEntities)
            {
                Vector2 position = new Vector2(spaceComponents.PositionComponents[id].Position.X * cellSize, spaceComponents.PositionComponents[id].Position.Y * cellSize);
                spriteBatch.Draw(spriteSheet, position, spaceComponents.DisplayComponents[id].SpriteSource, spaceComponents.DisplayComponents[id].Color);
            }
        }

        public static void DrawTiles(Camera camera, SpriteBatch spriteBatch, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellSize, Texture2D spriteSheet, DungeonColorInfo colorInfo)
        {
            Matrix cameraMatrix = camera.GetMatrix();
            for (int i = 0; i < (int)dungeonDimensions.X; i++)
            {
                for (int j = 0; j < (int)dungeonDimensions.Y; j++)
                {
                    Vector2 tile = new Vector2((int)i * cellSize, (int)j * cellSize);
                    Rectangle floor = new Rectangle(7 * cellSize, 26 * cellSize, cellSize, cellSize);
                    Rectangle wall = new Rectangle(2 * cellSize, 26 * cellSize, cellSize, cellSize);

                    if (camera.IsInView(cameraMatrix, new Vector2(i * cellSize + cellSize, j * cellSize + cellSize), new Vector2(i * cellSize, j * cellSize))) // check if in view
                    {
                        if (dungeonGrid[i, j].Found && !dungeonGrid[i, j].InRange)
                        {
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, position: tile, sourceRectangle: floor, color: colorInfo.Floor);
                                    break;
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, tile, wall, colorInfo.Wall);
                                    break;
                            }
                        }
                        else if (dungeonGrid[i, j].InRange && !dungeonGrid[i, j].NewlyFound)
                        {
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, tile, floor, colorInfo.FloorInRange);
                                    break;
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, tile, wall, colorInfo.WallInRange);
                                    break;
                            }
                        }
                        else if (dungeonGrid[i, j].NewlyFound)
                        {
                            float opacity = dungeonGrid[i, j].Opacity;
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, tile, floor, colorInfo.FloorInRange * opacity);
                                    break;
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, tile, wall, colorInfo.WallInRange * opacity);
                                    break;
                            }
                            dungeonGrid[i, j].Opacity += .21f;
                            if (dungeonGrid[i, j].Opacity > 1)
                            {
                                dungeonGrid[i, j].NewlyFound = false;
                                dungeonGrid[i, j].Found = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
