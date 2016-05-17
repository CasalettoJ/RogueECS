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
        public static void DrawDungeonEntities(StateSpaceComponents spaceComponents, Camera camera, SpriteBatch spriteBatch, Texture2D spriteSheet, int cellSize, DungeonTile[,] dungeonGrid, SpriteFont font, DungeonColorInfo colorInfo)
        {
            Matrix cameraMatrix = camera.GetMatrix();
            List<Entity> drawableEntities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Drawable) == ComponentMasks.Drawable).ToList();

            Entity player = spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
            bool inWater = false;
            bool inFire = false;
            if (player != null)
            {
                Vector2 playerPos = spaceComponents.PositionComponents[spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).First().Id].Position;
                inWater = dungeonGrid[(int)playerPos.X, (int)playerPos.Y].Type == TileType.TILE_WATER;
                inFire = dungeonGrid[(int)playerPos.X, (int)playerPos.Y].Type == TileType.TILE_FIRE;
            }
            //Draw items
            List<Guid> items = drawableEntities.Where(x => (x.ComponentFlags & ComponentMasks.PickupItem) == ComponentMasks.PickupItem).Select(x => x.Id).ToList();
            DisplaySystem.DrawEntities(items, spaceComponents, dungeonGrid, cameraMatrix, inWater, inFire, spriteBatch, spriteSheet, font, camera, colorInfo);

            //Draw everything else
            List<Guid> nonItems = drawableEntities.Where(x => (x.ComponentFlags & ComponentMasks.PickupItem) != ComponentMasks.PickupItem).Select(x => x.Id).ToList();
            DisplaySystem.DrawEntities(nonItems, spaceComponents, dungeonGrid, cameraMatrix, inWater, inFire, spriteBatch, spriteSheet, font, camera, colorInfo);

        }

        private static void DrawEntities(List<Guid> drawableEntities, StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Matrix cameraMatrix, bool inWater, bool inFire, SpriteBatch spriteBatch, Texture2D spriteSheet, SpriteFont font, Camera camera, DungeonColorInfo colorInfo)
        {
            foreach (Guid id in drawableEntities)
            {
                DisplayComponent display = spaceComponents.DisplayComponents[id];
                Vector2 gridPos = spaceComponents.PositionComponents[id].Position;
                Vector2 position = new Vector2(spaceComponents.PositionComponents[id].Position.X * DevConstants.Grid.CellSize, spaceComponents.PositionComponents[id].Position.Y * DevConstants.Grid.CellSize);
                if (dungeonGrid[(int)spaceComponents.PositionComponents[id].Position.X, (int)spaceComponents.PositionComponents[id].Position.Y].InRange || display.AlwaysDraw)
                {
                    Vector2 bottomRight = Vector2.Transform(new Vector2((position.X) + DevConstants.Grid.CellSize, (position.Y) + DevConstants.Grid.CellSize), cameraMatrix);
                    Vector2 topLeft = Vector2.Transform(new Vector2(position.X, position.Y), cameraMatrix);
                    Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                    if (camera.IsInView(cameraMatrix, cameraBounds))
                    {
                        //If the item is in water, you need to tint it, and if the player is in water and the object isn't (or vice versa) it must be hidden unless it's the observer.
                        display = dungeonGrid[(int)gridPos.X, (int)gridPos.Y].Type == TileType.TILE_WATER == inWater || (spaceComponents.Entities.Where(x => (x.Id == id)).First().ComponentFlags & ComponentMasks.Observer) == ComponentMasks.Observer ? display : DevConstants.ConstantComponents.UnknownDisplay;
                        Color displayColor = dungeonGrid[(int)gridPos.X, (int)gridPos.Y].Type == TileType.TILE_WATER || inWater ? Color.Lerp(display.Color, colorInfo.WaterInRange, .5f) : display.Color;
                        if(!inWater && dungeonGrid[(int)gridPos.X, (int)gridPos.Y].Type != TileType.TILE_WATER)
                        {
                            displayColor = dungeonGrid[(int)gridPos.X, (int)gridPos.Y].FireIllumination || inFire ? Color.Lerp(display.Color, colorInfo.FireInRange, .5f) : display.Color;
                        }

                        spriteBatch.Draw(spriteSheet, position, display.SpriteSource, displayColor * display.Opacity, display.Rotation, display.Origin, display.Scale, display.SpriteEffect, 0f);
                        if (!string.IsNullOrEmpty(display.Symbol))
                        {
                            Vector2 size = font.MeasureString(display.Symbol);
                            spriteBatch.DrawString(font, display.Symbol, new Vector2(((int)position.X + (int)display.SpriteSource.Center.X), ((int)position.Y + (int)display.SpriteSource.Center.Y)),
                                display.SymbolColor, 0f, new Vector2((int)(size.X / 2), (int)(size.Y / 2) - 3), 1f, SpriteEffects.None, 0f);
                        }
                    }
                }
            }
        }

        public static void DrawTiles(Camera camera, SpriteBatch spriteBatch, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellSize, Texture2D spriteSheet, DungeonColorInfo colorInfo, DijkstraMapTile[,] mapToPlayer, SpriteFont font, StateSpaceComponents spaceComponents)
        {
            Entity player = spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
            bool inWater = false;
            bool inFire = false;
            if (player != null)
            {
                Vector2 playerPos = spaceComponents.PositionComponents[spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).First().Id].Position;
                inWater = dungeonGrid[(int)playerPos.X, (int)playerPos.Y].Type == TileType.TILE_WATER;
                inFire = dungeonGrid[(int)playerPos.X, (int)playerPos.Y].Type == TileType.TILE_FIRE || (player.ComponentFlags & ComponentMasks.BurningStatus) == ComponentMasks.BurningStatus;
            }

            Matrix cameraMatrix = camera.GetMatrix();
            Vector2 origin = new Vector2(DevConstants.Grid.TileBorderSize, DevConstants.Grid.TileBorderSize);
            for (int i = 0; i < (int)dungeonDimensions.X; i++)
            {
                for (int j = 0; j < (int)dungeonDimensions.Y; j++)
                {
                    bool tintFire = inFire || dungeonGrid[i, j].FireIllumination;
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

                            Color colorToLerp = tintFire ? colorInfo.Fire : colorInfo.Water; //Default to water, if fire then change it.
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLATTENEDGRASS:
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.Floor, colorToLerp, .3f) : colorInfo.Floor) * .3f, origin: origin);
                                    break;
                                case TileType.TILE_ROCK:
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.Wall, colorToLerp, .3f) : colorInfo.Wall) * .3f, origin: origin);
                                    break;
                                case TileType.TILE_TALLGRASS:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.TallGrass, colorToLerp, .3f) : colorInfo.TallGrass) * .3f, origin: origin);
                                    break;
                                case TileType.TILE_ASH:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.Ash, colorToLerp, .3f) : colorInfo.Ash) * .3f, origin: origin);
                                    break;
                                case TileType.TILE_WATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (tintFire ? Color.Lerp(colorInfo.Water, colorToLerp, .3f) : colorInfo.Water) * .3f, origin: origin);
                                    break;
                                case TileType.TILE_FIRE:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater ? Color.Lerp(colorInfo.Fire, colorToLerp, .3f) : colorInfo.Fire) * .3f, origin: origin);
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
                            opacity = dungeonGrid[i, j].FireIllumination ? .85f : opacity;
                            bool isWall = false;

                            Color colorToLerp = tintFire ? colorInfo.FireInRange : colorInfo.WaterInRange; //Default to water, if fire then change it.
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLATTENEDGRASS:
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.FloorInRange, colorToLerp, .55f) : colorInfo.FloorInRange) * (float)opacity, origin: origin);
                                    break;
                                case TileType.TILE_ROCK:
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.WallInRange, colorToLerp, .55f) : colorInfo.WallInRange) * .85f, origin: origin);
                                    isWall = true;
                                    break;
                                case TileType.TILE_TALLGRASS:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.TallGrassInRange, colorToLerp, .55f) : colorInfo.TallGrassInRange) * (float)opacity, origin: origin);
                                    break;
                                case TileType.TILE_WATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (tintFire ? Color.Lerp(colorInfo.WaterInRange, colorToLerp, .55f) : colorInfo.WaterInRange) * (float)opacity, origin: origin);
                                    break;
                                case TileType.TILE_ASH:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.AshInRange, colorToLerp, .55f) : colorInfo.AshInRange) * (float)opacity, origin: origin);
                                    break;
                                case TileType.TILE_FIRE:
                                    opacity = 0f + (.1f * dungeonGrid[i, j].TurnsToBurn);
                                    opacity = (opacity > 1f) ? 1f : opacity;
                                    opacity = (opacity < .3f) ? .3f : opacity;
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater ? Color.Lerp(colorInfo.FireInRange, colorToLerp, .55f) : colorInfo.FireInRange) * (float)opacity, origin: origin);
                                    break;
                            }
                            if (!isWall)
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
                            Color colorToLerp = tintFire ? colorInfo.FireInRange : colorInfo.WaterInRange; //Default to water, if fire then change it.
                            switch (dungeonGrid[i, j].Type)
                            {
                                case TileType.TILE_FLATTENEDGRASS:
                                case TileType.TILE_FLOOR:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.FloorInRange, colorToLerp, .55f) : colorInfo.FloorInRange) * opacity, origin: origin);
                                    break;
                                case TileType.TILE_ROCK:
                                case TileType.TILE_WALL:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.WallInRange, colorToLerp, .55f) : colorInfo.WallInRange) * opacity, origin: origin);
                                    break;
                                case TileType.TILE_TALLGRASS:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.TallGrassInRange, colorToLerp, .55f) : colorInfo.TallGrassInRange) * opacity, origin: origin);
                                    break;
                                case TileType.TILE_ASH:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater || tintFire ? Color.Lerp(colorInfo.AshInRange, colorToLerp, .55f) : colorInfo.AshInRange) * opacity, origin: origin);
                                    break;
                                case TileType.TILE_WATER:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (tintFire ? Color.Lerp(colorInfo.WaterInRange, colorToLerp, .55f) : colorInfo.WaterInRange) * opacity, origin: origin);
                                    break;
                                case TileType.TILE_FIRE:
                                    spriteBatch.Draw(spriteSheet, position: tile, color: (inWater ? Color.Lerp(colorInfo.FireInRange, colorToLerp, .55f) : colorInfo.FireInRange), origin: origin);
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
            Entity player = spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
            bool inWater = false;
            if (player != null)
            {
                Vector2 playerPos = spaceComponents.PositionComponents[spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).First().Id].Position;
                inWater = dungeonGrid[(int)playerPos.X, (int)playerPos.Y].Type == TileType.TILE_WATER;
            }

            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.DrawableOutline) == ComponentMasks.DrawableOutline).Select(x => x.Id))
            {
                Entity observer = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Observer) == ComponentMasks.Observer).FirstOrDefault();
                bool isObserver = false;
                if (observer != null)
                {
                    isObserver = observer.Id == id;
                }

                OutlineComponent outline = spaceComponents.OutlineComponents[id];
                PositionComponent position = spaceComponents.PositionComponents[id];

                bool outlineInWater = dungeonGrid[(int)position.Position.X, (int)position.Position.Y].Type == TileType.TILE_WATER;
                Vector2 tile = new Vector2((int)position.Position.X * DevConstants.Grid.CellSize, (int)position.Position.Y * DevConstants.Grid.CellSize);

                Vector2 bottomRight = Vector2.Transform(new Vector2((tile.X + DevConstants.Grid.CellSize), (tile.Y + DevConstants.Grid.CellSize)), cameraMatrix);
                Vector2 topLeft = Vector2.Transform(tile, cameraMatrix);
                Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                if (dungeonGrid[(int)position.Position.X, (int)position.Position.Y].InRange && camera.IsInView(cameraMatrix, cameraBounds) && (inWater == outlineInWater || isObserver))
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
                if (fovInfo.DrawField)
                {
                    foreach (Vector2 tilePosition in fovInfo.SeenTiles)
                    {
                        Vector2 tile = new Vector2((int)tilePosition.X * cellSize, (int)tilePosition.Y * cellSize);

                        Vector2 bottomRight = Vector2.Transform(new Vector2((tile.X + cellSize), (tile.Y + cellSize)), cameraMatrix);
                        Vector2 topLeft = Vector2.Transform(tile, cameraMatrix);
                        Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                        if (dungeonGrid[(int)tilePosition.X, (int)tilePosition.Y].InRange && camera.IsInView(cameraMatrix, cameraBounds))
                        {
                            if (spaceComponents.AlternateFOVColorChangeComponents.ContainsKey(id))
                            {
                                AlternateFOVColorChangeComponent altColorInfo = spaceComponents.AlternateFOVColorChangeComponents[id];
                                spriteBatch.Draw(rectangleTexture, position: tile, color: Color.Lerp(fovInfo.Color, altColorInfo.AlternateColor, altColorInfo.Seconds / altColorInfo.SwitchAtSeconds) * fovInfo.Opacity, origin: new Vector2(DevConstants.Grid.TileBorderSize, DevConstants.Grid.TileBorderSize));
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
