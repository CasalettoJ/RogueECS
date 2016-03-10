using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.GraphicalEffectsComponents;
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
        public static void DrawDungeonEntities(StateSpaceComponents spaceComponents, Camera camera, SpriteBatch spriteBatch, Texture2D spriteSheet, int cellSize, DungeonTile[,] dungeonGrid, SpriteFont font)
        {
            Matrix cameraMatrix = camera.GetMatrix();
            IEnumerable<Guid> drawableEntities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Drawable) == ComponentMasks.Drawable).Select(x => x.Id);
            foreach(Guid id in drawableEntities)
            {
                DisplayComponent display = spaceComponents.DisplayComponents[id];
                Vector2 position = new Vector2(spaceComponents.PositionComponents[id].Position.X * cellSize, spaceComponents.PositionComponents[id].Position.Y * cellSize);
                if(dungeonGrid[(int)spaceComponents.PositionComponents[id].Position.X, (int)spaceComponents.PositionComponents[id].Position.Y].InRange || display.AlwaysDraw)
                {
                    Vector2 bottomRight = Vector2.Transform(new Vector2((position.X) + cellSize, (position.Y) + cellSize), cameraMatrix);
                    Vector2 topLeft = Vector2.Transform(new Vector2(position.X, position.Y), cameraMatrix);
                    Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                    if (camera.IsInView(cameraMatrix, cameraBounds) )
                    {
                        spriteBatch.Draw(spriteSheet, position, display.SpriteSource, display.Color * display.Opacity, display.Rotation, display.Origin, display.Scale, display.SpriteEffect, 0f);
                        if (!string.IsNullOrEmpty(spaceComponents.DisplayComponents[id].Symbol))
                        {
                            Vector2 size = font.MeasureString(spaceComponents.DisplayComponents[id].Symbol);
                            spriteBatch.DrawString(font, display.Symbol, new Vector2(((int)position.X + (int)display.SpriteSource.Center.X), ((int)position.Y + (int)display.SpriteSource.Center.Y)), 
                                display.SymbolColor, 0f, new Vector2((int)(size.X/2), (int)(size.Y/2)-3), 1f, SpriteEffects.None, 0f);
                        }
                    }
                }
            }
        }

        public static void DrawTiles(Camera camera, SpriteBatch spriteBatch, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellSize, Texture2D spriteSheet, DungeonColorInfo colorInfo, DijkstraMapTile[,] mapToPlayer, SpriteFont font)
        {
            Matrix cameraMatrix = camera.GetMatrix();
            Vector2 origin = new Vector2(DevConstants.Grid.TileBorderSize, DevConstants.Grid.TileBorderSize);
            for (int i = 0; i < (int)dungeonDimensions.X; i++)
            {
                for (int j = 0; j < (int)dungeonDimensions.Y; j++)
                {
                    Vector2 tile = new Vector2((int)i * cellSize, (int)j * cellSize);
                    Rectangle floor = new Rectangle(0 * cellSize, 0 * cellSize, cellSize, cellSize); //Need to be moved eventually
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
                                    spriteBatch.Draw(spriteSheet, position: tile,  color: colorInfo.Floor * .3f, origin: origin);
                                    break;
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, position: tile,  color: colorInfo.Wall * .3f, origin: origin);
                                    break;
                                case TileType.TILE_TALLGRASS:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.TallGrass * .3f, origin: origin);
                                    break;
                                case TileType.TILE_WATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.Water * .3f, origin: origin);
                                    break;
                                case TileType.TILE_DEEPWATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.DeepWater * .3f, origin: origin);
                                    break;
                            }
                            if (!string.IsNullOrEmpty(dungeonGrid[i, j].Symbol))
                            {
                                Vector2 size = font.MeasureString(dungeonGrid[i, j].Symbol);
                                spriteBatch.DrawString(font, dungeonGrid[i, j].Symbol, new Vector2(i * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2),
                                    (j * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2))),
                                    dungeonGrid[i, j].SymbolColor * .5f, 0f, new Vector2((int)(size.X / 2), (int)(size.Y / 2)), 1f, SpriteEffects.None, 0f);
                            }
                        }
                        else if (dungeonGrid[i, j].InRange && !dungeonGrid[i, j].NewlyFound)
                        {
                            int weight = mapToPlayer[i, j].Weight;
                            double opacity = 1 - (.035 * weight);
                            opacity = (opacity < .4) ? .4 : opacity;
                            bool isWall = false;
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.FloorInRange * (float)opacity, origin: origin);
                                    break;
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, position: tile,  color: colorInfo.WallInRange * .85f, origin: origin);
                                    isWall = true;
                                    break;
                                case TileType.TILE_TALLGRASS:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.TallGrassInRange * (float)opacity, origin: origin);
                                    break;
                                case TileType.TILE_WATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.WaterInRange * (float)opacity, origin: origin);
                                    break;
                                case TileType.TILE_DEEPWATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.DeepWaterInRange * (float)opacity, origin: origin);
                                    break;
                            }
                            if(!isWall)
                            {
                                if (!string.IsNullOrEmpty(dungeonGrid[i, j].Symbol))
                                {
                                    Vector2 size = font.MeasureString(dungeonGrid[i, j].Symbol);
                                    spriteBatch.DrawString(font, dungeonGrid[i, j].Symbol, new Vector2(i * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2),
                                        (j * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2))),
                                        dungeonGrid[i, j].SymbolColor * (float)opacity, 0f, new Vector2((int)(size.X / 2), (int)(size.Y / 2)), 1f, SpriteEffects.None, 0f);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(dungeonGrid[i, j].Symbol))
                                {
                                    Vector2 size = font.MeasureString(dungeonGrid[i, j].Symbol);
                                    spriteBatch.DrawString(font, dungeonGrid[i, j].Symbol, new Vector2(i * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2),
                                        (j * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2))),
                                        dungeonGrid[i, j].SymbolColor * .85f, 0f, new Vector2((int)(size.X / 2), (int)(size.Y / 2)), 1f, SpriteEffects.None, 0f);
                                }
                            }
                        }
                        else if (dungeonGrid[i, j].NewlyFound)
                        {
                            float opacity = dungeonGrid[i, j].Opacity;
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, position: tile,  color: colorInfo.FloorInRange * opacity, origin: origin);
                                    break;
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, position: tile,  color: colorInfo.WallInRange * opacity, origin: origin);
                                    break;
                                case TileType.TILE_TALLGRASS:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.TallGrassInRange * opacity, origin: origin);
                                    break;
                                case TileType.TILE_WATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.WaterInRange * opacity, origin: origin);
                                    break;
                                case TileType.TILE_DEEPWATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: colorInfo.DeepWaterInRange * opacity, origin: origin);
                                    break;
                            }
                            if (!string.IsNullOrEmpty(dungeonGrid[i, j].Symbol))
                            {
                                Vector2 size = font.MeasureString(dungeonGrid[i, j].Symbol);
                                spriteBatch.DrawString(font, dungeonGrid[i, j].Symbol, new Vector2(i * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2),
                                    (j * DevConstants.Grid.CellSize + (int)(DevConstants.Grid.CellSize / 2))),
                                    dungeonGrid[i, j].SymbolColor * opacity, 0f, new Vector2((int)(size.X / 2), (int)(size.Y / 2)), 1f, SpriteEffects.None, 0f);
                            }
                            if (dungeonGrid[i, j].Opacity > .5)
                            {
                                dungeonGrid[i, j].NewlyFound = false;
                                dungeonGrid[i, j].Found = true;
                            }
                        }
                    }

                   
                }
            }


        }

        public static void DrawOutlines(StateSpaceComponents spaceComponents, Camera camera, SpriteBatch spriteBatch, Texture2D rectangleTexture, DungeonTile[,] dungeonGrid)
        {
            Matrix cameraMatrix = camera.GetMatrix();
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.DrawableOutline) == ComponentMasks.DrawableOutline).Select(x => x.Id))
            {
                OutlineComponent outline = spaceComponents.OutlineComponents[id];
                PositionComponent position = spaceComponents.PositionComponents[id];
                Vector2 tile = new Vector2((int)position.Position.X * DevConstants.Grid.CellSize, (int)position.Position.Y * DevConstants.Grid.CellSize);

                Vector2 bottomRight = Vector2.Transform(new Vector2((tile.X + DevConstants.Grid.CellSize), (tile.Y + DevConstants.Grid.CellSize)), cameraMatrix);
                Vector2 topLeft = Vector2.Transform(tile, cameraMatrix);
                Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                if (dungeonGrid[(int)position.Position.X, (int)position.Position.Y].InRange && camera.IsInView(cameraMatrix, cameraBounds))
                {
                    if (spaceComponents.SecondaryOutlineComponents.ContainsKey(id))
                    {
                        SecondaryOutlineComponent altColorInfo = spaceComponents.SecondaryOutlineComponents[id];
                        spriteBatch.Draw(rectangleTexture, position: tile, color: Color.Lerp(outline.Color, altColorInfo.AlternateColor, altColorInfo.Seconds / altColorInfo.SwitchAtSeconds) * outline.Opacity, origin: new Vector2(DevConstants.Grid.TileBorderSize, DevConstants.Grid.TileBorderSize));
                    }
                    else
                    {
                        //origin is 4,4 because the tile texture is 40x40 and the grid is 32x32.  If size of grid changes, change this -- and then don't hardcode it anymore!!!
                        spriteBatch.Draw(rectangleTexture, position: tile, color: outline.Color * outline.Opacity, origin: new Vector2(DevConstants.Grid.TileBorderSize, DevConstants.Grid.TileBorderSize));
                    }
                }
            }
        }


        public static void DrawAIFieldOfViews(StateSpaceComponents spaceComponents, Camera camera, SpriteBatch spriteBatch, Texture2D rectangleTexture, int cellSize, DungeonTile[,] dungeonGrid)
        {
            Matrix cameraMatrix = camera.GetMatrix();
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.AIView) == ComponentMasks.AIView).Select(x => x.Id))
            {
                AIFieldOfView fovInfo = spaceComponents.AIFieldOfViewComponents[id];
                if(fovInfo.DrawField)
                {
                    foreach(Vector2 tilePosition in fovInfo.SeenTiles)
                    {
                        Vector2 tile = new Vector2((int)tilePosition.X * cellSize, (int)tilePosition.Y * cellSize);

                        Vector2 bottomRight = Vector2.Transform(new Vector2((tile.X + cellSize), (tile.Y + cellSize)), cameraMatrix);
                        Vector2 topLeft = Vector2.Transform(tile, cameraMatrix);
                        Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                        if(dungeonGrid[(int)tilePosition.X, (int)tilePosition.Y].InRange && camera.IsInView(cameraMatrix, cameraBounds))
                        {
                            if(spaceComponents.AlternateFOVColorChangeComponents.ContainsKey(id))
                            {
                                AlternateFOVColorChangeComponent altColorInfo = spaceComponents.AlternateFOVColorChangeComponents[id];
                                spriteBatch.Draw(rectangleTexture, position: tile, color: Color.Lerp(fovInfo.Color,altColorInfo.AlternateColor,altColorInfo.Seconds/altColorInfo.SwitchAtSeconds) * fovInfo.Opacity, origin: new Vector2(DevConstants.Grid.TileBorderSize, DevConstants.Grid.TileBorderSize));
                            }
                            else
                            {
                                //origin is 4,4 because the tile texture is 40x40 and the grid is 32x32.  If size of grid changes, change this -- and then don't hardcode it anymore!!!
                                spriteBatch.Draw(rectangleTexture, position: tile, color: fovInfo.Color * fovInfo.Opacity, origin: new Vector2(DevConstants.Grid.TileBorderSize, DevConstants.Grid.TileBorderSize));
                            }
                        }
                    }
                }
            }
        }

    }
}
