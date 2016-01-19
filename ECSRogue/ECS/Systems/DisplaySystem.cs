using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
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
            Matrix cameraMatrix = camera.GetMatrix();
            IEnumerable<Guid> drawableEntities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Drawable) == ComponentMasks.Drawable).Select(x => x.Id);
            foreach(Guid id in drawableEntities)
            {
                DisplayComponent display = spaceComponents.DisplayComponents[id];
                Vector2 position = new Vector2(spaceComponents.PositionComponents[id].Position.X * cellSize, spaceComponents.PositionComponents[id].Position.Y * cellSize);

                Vector2 bottomRight = Vector2.Transform(new Vector2((position.X) + cellSize, (position.Y) + cellSize), cameraMatrix);
                Vector2 topLeft = Vector2.Transform(new Vector2(position.X, position.Y), cameraMatrix);
                Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                if(camera.IsInView(cameraMatrix, cameraBounds))
                {
                    spriteBatch.Draw(spriteSheet, position, display.SpriteSource, display.Color, display.Rotation, display.Origin, display.Scale, display.SpriteEffect, 0f);
                }
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
                    Rectangle floor = new Rectangle(1 * cellSize, 0 * cellSize, cellSize, cellSize); //Need to be moved eventually
                    Rectangle wall = new Rectangle(0 * cellSize, 0 * cellSize, cellSize, cellSize); //Need to be moved eventually

                    Vector2 bottomRight = Vector2.Transform(new Vector2((i * cellSize) + cellSize, (j * cellSize) + cellSize), cameraMatrix);
                    Vector2 topLeft = Vector2.Transform(new Vector2(i * cellSize, j * cellSize), cameraMatrix);
                    Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                    if (camera.IsInView(cameraMatrix, cameraBounds)) // check if in view
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
