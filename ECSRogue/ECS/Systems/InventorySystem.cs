﻿using ECSRogue.BaseEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ECSRogue.ECS;
using ECSRogue.ProceduralGeneration;
using ECSRogue.ProceduralGeneration.Interfaces;
using ECSRogue.ECS.Systems;
using ECSRogue.ECS.Components;
using ECSRogue.BaseEngine.IO.Objects;
using ECSRogue.ECS.Components.GraphicalEffectsComponents;
using ECSRogue.ECS.Components.ItemizationComponents;
using ECSRogue.BaseEngine;
using System.Text.RegularExpressions;

namespace ECSRogue.ECS.Systems
{
    public static class InventorySystem
    {
        public static void UseItem(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Guid item, Guid user)
        {
            ItemFunctionsComponent itemInfo = spaceComponents.ItemFunctionsComponents[item];
            Vector2 target = spaceComponents.PositionComponents[user].Position;
            bool cancelledCast = false;
            if(itemInfo.Ranged)
            {
                //Call a function to get the target position.  If they cancel during this part, set cancelledCast to true.
            }
            //If the item use is successful, remove the item from inventory and delete the item
            Func<StateSpaceComponents, DungeonTile[,], Vector2, Guid, Guid, Vector2, bool> useFunction = ItemUseFunctionLookup.GetUseFunction(itemInfo.UseFunctionValue);
            if (!cancelledCast && useFunction!= null && useFunction(spaceComponents, dungeonGrid, dungeonDimensions, item, user, target))
            {
                NameComponent itemName = spaceComponents.NameComponents[item];
                itemInfo.Uses -= 1;
                //If the items' out of uses, remove it
                if(itemInfo.Uses <= 0)
                {
                    InventoryComponent inventory = spaceComponents.InventoryComponents[user];
                    inventory.Consumables.Remove(item);
                    spaceComponents.InventoryComponents[user] = inventory;
                    spaceComponents.EntitiesToDelete.Add(item);
                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + "{0} has used the last of the {1}", spaceComponents.NameComponents[user].Name, spaceComponents.NameComponents[item].Name)));
                }
                //otherwise, just report how many uses it has left.
                else
                {
                    ValueComponent itemValue = spaceComponents.ValueComponents[item];
                    itemValue.Gold = (itemValue.Gold - itemInfo.CostToUse < 0) ? 0 : itemValue.Gold - itemInfo.CostToUse;
                    spaceComponents.ValueComponents[item] = itemValue;
                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + "{0} has {1} charge(s) left.", spaceComponents.NameComponents[item].Name, itemInfo.Uses)));
                }
                PlayerComponent player = spaceComponents.PlayerComponent;
                player.PlayerTookTurn = true;
                spaceComponents.PlayerComponent = player;
                spaceComponents.ItemFunctionsComponents[item] = itemInfo;
            }
        }

        public static SkillLevelsComponent ApplyStatModifications(StateSpaceComponents spaceComponents, Guid entity, SkillLevelsComponent originalStats)
        {
            SkillLevelsComponent newStats = originalStats;
            //See if the entity has an inventory to check for artifact modifications
            if(spaceComponents.InventoryComponents.ContainsKey(entity))
            {
                InventoryComponent entityInventory = spaceComponents.InventoryComponents[entity];
                //For each artifact in the entity' inventory, look for the statmodificationcomponent, and modify stats accordingly.
                foreach (Guid id in  entityInventory.Artifacts)
                {
                    Entity inventoryItem = spaceComponents.Entities.Where(x => x.Id == id).FirstOrDefault();
                    if (inventoryItem != null && (inventoryItem.ComponentFlags & ComponentMasks.Artifact) == ComponentMasks.Artifact)
                    {
                        StatModificationComponent statsMods = spaceComponents.StatModificationComponents[id];
                        newStats.Accuracy = (newStats.Accuracy + statsMods.AccuracyChange < 0) ? 0 : newStats.Accuracy + statsMods.AccuracyChange;
                        newStats.Defense = (newStats.Defense + statsMods.DefenseChange < 0) ? 0 : newStats.Defense + statsMods.DefenseChange;
                        newStats.DieNumber = (newStats.DieNumber + statsMods.DieNumberChange < 1) ? 1 : newStats.DieNumber + statsMods.DieNumberChange;
                        newStats.Health = (newStats.Health + statsMods.HealthChange < 0) ? 0 : newStats.Health + statsMods.HealthChange;
                        newStats.MaximumDamage = (newStats.MaximumDamage + statsMods.MaximumDamageChange < 1) ? 1 : newStats.MaximumDamage + statsMods.MaximumDamageChange;
                        newStats.MinimumDamage = (newStats.MinimumDamage + statsMods.MinimumDamageChange < 1) ? 1 : newStats.MinimumDamage + statsMods.MinimumDamageChange;
                    }
                }
            }

            return newStats;
        }

        public static void TryPickupItems(StateSpaceComponents spaceComponents, DungeonTile[,] dungeongrid)
        {
            IEnumerable<Guid> entitiesThatCollided = spaceComponents.GlobalCollisionComponent.EntitiesThatCollided.Distinct();
            foreach(Guid collidingEntity in entitiesThatCollided)
            {
                Entity colliding = spaceComponents.Entities.Where(x => x.Id == collidingEntity).FirstOrDefault();
                //If the colliding entity has an inventory and messages, place the item inside the inventory.
                if(colliding != null && (colliding.ComponentFlags & ComponentMasks.InventoryPickup) == ComponentMasks.InventoryPickup)
                {
                    CollisionComponent collidingEntityCollisions = spaceComponents.CollisionComponents[collidingEntity];
                    InventoryComponent collidingEntityInventory = spaceComponents.InventoryComponents[collidingEntity];
                    NameComponent collidingEntityName = spaceComponents.NameComponents[collidingEntity];
                    PositionComponent collidingEntityPosition = spaceComponents.PositionComponents[collidingEntity];
                    EntityMessageComponent collidingEntityMessages = spaceComponents.EntityMessageComponents[collidingEntity];
                    foreach(Guid collidedEntity in collidingEntityCollisions.CollidedObjects)
                    {
                        //Check to see if the collidedEntity is a pickup item, if it is, try to place it in the inventory if it fits.
                        Entity collided = spaceComponents.Entities.Where(x => x.Id == collidedEntity).FirstOrDefault();
                        //If the collideditem is a pickup item, handle it based on pickup type.
                        if(collided != null && (collided.ComponentFlags & ComponentMasks.PickupItem) == ComponentMasks.PickupItem)
                        {
                            PickupComponent itemPickup = spaceComponents.PickupComponents[collidedEntity];
                            ValueComponent itemValue = spaceComponents.ValueComponents[collidedEntity];
                            NameComponent itemName = spaceComponents.NameComponents[collidedEntity];
                            switch(itemPickup.PickupType)
                            {
                                case ItemType.GOLD:
                                    if(spaceComponents.SkillLevelsComponents.ContainsKey(collidingEntity))
                                    {
                                        SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[collidingEntity];
                                        skills.Wealth += itemValue.Gold;
                                        spaceComponents.SkillLevelsComponents[collidingEntity] = skills;
                                        if(dungeongrid[(int)collidingEntityPosition.Position.X, (int)collidingEntityPosition.Position.Y].InRange)
                                        {
                                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + "{0} picked up {1} gold.", collidingEntityName.Name, itemValue.Gold)));
                                        }
                                        spaceComponents.EntitiesToDelete.Add(collidedEntity);
                                    }
                                    break;
                                case ItemType.CONSUMABLE:
                                    //If the entity has room in their inventory, place the item in it
                                    if(collidingEntityInventory.Consumables.Count < collidingEntityInventory.MaxConsumables)
                                    {
                                        //Remove the position component flag and add the guid to the inventory of the entity
                                        collided.ComponentFlags &= ~Component.COMPONENT_POSITION;
                                        collidingEntityInventory.Consumables.Add(collided.Id);
                                        if(dungeongrid[(int)collidingEntityPosition.Position.X, (int)collidingEntityPosition.Position.Y].InRange && collidingEntityMessages.PickupItemMessages.Length > 0)
                                        {
                                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.LootPickup,
                                                string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + collidingEntityMessages.PickupItemMessages[spaceComponents.random.Next(0, collidingEntityMessages.PickupItemMessages.Length)], collidingEntityName.Name, itemName.Name)));
                                        }
                                    }
                                    //If it can't fit in, and the entity has a message for the situation, display it.
                                    else if(spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages != null && spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages.Length > 0)
                                    {
                                        spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, "[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " +  spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages[spaceComponents.random.Next(0, spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages.Length)]));
                                    }
                                    break;
                                case ItemType.ARTIFACT:
                                    //If the entity has room in their inventory, place the item in it
                                    if (collidingEntityInventory.Artifacts.Count < collidingEntityInventory.MaxArtifacts)
                                    {
                                        //Remove the position component flag and add the guid to the inventory of the entity
                                        collided.ComponentFlags &= ~Component.COMPONENT_POSITION;
                                        collidingEntityInventory.Artifacts.Add(collided.Id);
                                        if (dungeongrid[(int)collidingEntityPosition.Position.X, (int)collidingEntityPosition.Position.Y].InRange && collidingEntityMessages.PickupItemMessages.Length > 0)
                                        {
                                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.LootPickup,
                                                string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + collidingEntityMessages.PickupItemMessages[spaceComponents.random.Next(0, collidingEntityMessages.PickupItemMessages.Length)], collidingEntityName.Name, itemName.Name)));
                                        }
                                    }
                                    //If it can't fit in, and the entity has a message for the situation, display it.
                                    else if (spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages != null && spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages.Length > 0)
                                    {
                                        spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, "[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages[spaceComponents.random.Next(0, spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages.Length)]));
                                    }
                                    break;
                                case ItemType.DOWNSTAIRS:
                                    //Tell the game to go to the next level
                                    if((spaceComponents.Entities.Where(x => x.Id == collidingEntity).FirstOrDefault().ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER)
                                    {
                                        PlayerComponent player = spaceComponents.PlayerComponent;
                                        player.GoToNextFloor = true;
                                        spaceComponents.PlayerComponent = player;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public static bool HandleInventoryInput(StateSpaceComponents spaceComponents, GameTime gameTime, KeyboardState prevKey, KeyboardState key)
        {
            bool keepInventory = true;
            Entity player = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
            if (player != null)
            {
                InventoryMenuComponent menu = spaceComponents.InventoryMenuComponent;
                InventoryComponent playerInventory = spaceComponents.InventoryComponents[player.Id];

                //Construct array of all inventory items
                List<Guid> items = new List<Guid>();
                items.AddRange(playerInventory.Artifacts);
                items.AddRange(playerInventory.Consumables);
                int itemSelection = items.IndexOf(menu.SelectedItem);
                if (itemSelection < 0)
                {
                    itemSelection = 0;
                }

                //Item selection
                //If nothing is selected, select the first item
                if ((menu.SelectedItem == Guid.Empty && items.Count > 0) || (items.Count > 0 && !items.Contains(menu.SelectedItem)))
                {
                    menu.SelectedItem = items[itemSelection];
                }

                if (key.IsKeyDown(Keys.Down) && prevKey.IsKeyUp(Keys.Down))
                {
                    itemSelection += 1;
                    if (itemSelection >= items.Count)
                    {
                        itemSelection = 0;
                    }
                }
                else if (key.IsKeyDown(Keys.Up) && prevKey.IsKeyUp(Keys.Up))
                {
                    itemSelection -= 1;
                    if (itemSelection < 0)
                    {
                        itemSelection = items.Count - 1;
                    }
                }
                else if (key.IsKeyDown(Keys.D) && prevKey.IsKeyUp(Keys.D))
                {
                    //Drop the selected item.
                    PositionComponent playerPos = spaceComponents.PositionComponents[player.Id];
                    Entity item = spaceComponents.Entities.Where(x => x.Id == items[itemSelection]).FirstOrDefault();
                    if(item != null)
                    {
                        item.ComponentFlags |= Component.COMPONENT_POSITION;
                        spaceComponents.PositionComponents[item.Id] = playerPos;
                        playerInventory.Artifacts.Remove(item.Id);
                        playerInventory.Consumables.Remove(item.Id);
                        itemSelection = 0;
                        items.Remove(item.Id);
                    }
                }
                else if (key.IsKeyDown(Keys.S) && prevKey.IsKeyUp(Keys.S))
                {
                    //Scrap the selected item
                    Entity item = spaceComponents.Entities.Where(x => x.Id == items[itemSelection]).FirstOrDefault();
                    if (item != null)
                    {
                        SkillLevelsComponent playerSkills = spaceComponents.SkillLevelsComponents[player.Id];
                        ValueComponent itemValue = spaceComponents.ValueComponents[item.Id];
                        playerSkills.Wealth += itemValue.Gold;
                        playerInventory.Artifacts.Remove(item.Id);
                        playerInventory.Consumables.Remove(item.Id);
                        spaceComponents.EntitiesToDelete.Add(item.Id);
                        spaceComponents.SkillLevelsComponents[player.Id] = playerSkills;
                        itemSelection = 0;
                        items.Remove(item.Id);
                    }
                }

                //Set new selections
                if (items.Count > 0)
                {
                    menu.SelectedItem = items[itemSelection];
                    spaceComponents.InventoryMenuComponent = menu;
                }
                else
                {
                    menu.SelectedItem = Guid.Empty;
                    spaceComponents.InventoryMenuComponent = menu;
                }
            }
            return keepInventory;
        }

        public static void DropWholeInventory(StateSpaceComponents spaceComponents, Guid droppingEntity, Vector2 entityPos)
        {
            Entity entity = spaceComponents.Entities.Where(x => x.Id == droppingEntity).FirstOrDefault();
            if(entity != null && (entity.ComponentFlags & Component.COMPONENT_INVENTORY) == Component.COMPONENT_INVENTORY)
            {
                InventoryComponent invo = spaceComponents.InventoryComponents[entity.Id];
                foreach(Guid artifact in invo.Artifacts)
                {
                    Entity item = spaceComponents.Entities.Where(x => x.Id == artifact).FirstOrDefault();
                    if (item != null)
                    {
                        item.ComponentFlags |= Component.COMPONENT_POSITION;
                        spaceComponents.PositionComponents[item.Id] = new PositionComponent() { Position = entityPos };
                        invo.Artifacts.Remove(item.Id);
                    }
                }

                foreach(Guid consumable in invo.Consumables)
                {
                    Entity item = spaceComponents.Entities.Where(x => x.Id == consumable).FirstOrDefault();
                    if (item != null)
                    {
                        item.ComponentFlags |= Component.COMPONENT_POSITION;
                        spaceComponents.PositionComponents[item.Id] = new PositionComponent() { Position = entityPos };
                        invo.Consumables.Remove(item.Id);
                    }
                }

                if((entity.ComponentFlags & Component.COMPONENT_SKILL_LEVELS) == Component.COMPONENT_SKILL_LEVELS)
                {
                    SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[entity.Id];
                    if(skills.Wealth > 0)
                    {
                        spaceComponents.DelayedActions.Add(new Action(() =>
                        {
                            Guid id = spaceComponents.CreateEntity();
                            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Drawable | ComponentMasks.GlowingOutline | ComponentMasks.PickupItem;
                            spaceComponents.DisplayComponents[id] = new DisplayComponent()
                            {
                                AlwaysDraw = false,
                                Color = Colors.Messages.Special,
                                Opacity = 1f,
                                Origin = Vector2.Zero,
                                Rotation = 0f,
                                Scale = 1f,
                                SpriteEffect = SpriteEffects.None,
                                SpriteSource = new Rectangle(0 * DevConstants.Grid.CellSize, 0 * DevConstants.Grid.CellSize, DevConstants.Grid.CellSize, DevConstants.Grid.CellSize),
                                Symbol = "$",
                                SymbolColor = Color.White
                            };
                            Vector2 position = entityPos;
                            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = position };
                            spaceComponents.OutlineComponents[id] = new OutlineComponent() { Color = Color.Purple, Opacity = 1f };
                            spaceComponents.SecondaryOutlineComponents[id] = new SecondaryOutlineComponent() { AlternateColor = Color.LightBlue, Seconds = 0f, SwitchAtSeconds = .75f };
                            spaceComponents.PickupComponents[id] = new PickupComponent() { PickupType = ItemType.GOLD };
                            spaceComponents.ValueComponents[id] = new ValueComponent() { Gold = skills.Wealth };
                            spaceComponents.NameComponents[id] = new NameComponent()
                            {
                                Name = "Gold",
                                Description = "Some people try and use fancy names for this mass of wealth. Credits, Stardust, Gil... it buys shelter and women all the same."
                            };
                            spaceComponents.CollisionComponents[id] = new CollisionComponent() { CollidedObjects = new List<Guid>(), Solid = false };
                        }));
                    }
                }
            }

        }


        public static void ShowInventoryMenu(StateSpaceComponents spaceComponents, SpriteBatch spriteBatch, Camera camera, SpriteFont messageFont, SpriteFont optionFont, Texture2D UI)
        {

            Entity player = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
            if (player != null)
            {
                int messageSpacing = (int)messageFont.MeasureString("g").Y + 1;
                int messageNumberLeft = 0;
                int messageNumberMiddle = 0;
                int messageNumberRight = 0;
                int panelWidth = (int)(camera.FullViewport.Width / 3);
                InventoryMenuComponent menu = spaceComponents.InventoryMenuComponent;
                InventoryComponent playerInvo = spaceComponents.InventoryComponents[player.Id];
                SkillLevelsComponent playerSkills = spaceComponents.SkillLevelsComponents[player.Id];

                //Draw background            
                spriteBatch.Draw(UI, camera.FullViewport.Bounds, Color.Black * .7f);

                //Draw Header
                Vector2 headerSize = optionFont.MeasureString(Messages.InventoryHeader);
                int beginningHeight = (int)headerSize.Y + messageSpacing * 2;
                spriteBatch.Draw(UI, new Rectangle(0, 0, camera.FullViewport.Width, beginningHeight), Color.DarkViolet * .3f);
                spriteBatch.DrawString(optionFont, Messages.InventoryHeader, new Vector2((int)(camera.FullViewport.Width / 2) - (int)(headerSize.X / 2), messageSpacing), Color.CornflowerBlue);
                beginningHeight += messageSpacing;

                //Draw columns
                //Left
                spriteBatch.Draw(UI, new Rectangle(0, beginningHeight, panelWidth - messageSpacing, camera.FullViewport.Height), Color.DarkBlue * .25f);
                //Middle
                spriteBatch.Draw(UI, new Rectangle(messageSpacing + panelWidth, beginningHeight, panelWidth - messageSpacing, camera.FullViewport.Height), Color.DarkBlue * .25f);
                //Right
                spriteBatch.Draw(UI, new Rectangle(messageSpacing * 2 + panelWidth * 2, beginningHeight, panelWidth - messageSpacing, camera.FullViewport.Height), Color.DarkBlue * .25f);

                //Draw item selection panel
                spriteBatch.DrawString(messageFont, string.Format("Wealth: {0}", playerSkills.Wealth), new Vector2(messageSpacing, beginningHeight), Colors.Messages.Special);
                messageNumberLeft += 2;
                spriteBatch.DrawString(messageFont, string.Format(Messages.InventoryArtifacts + " ({0}/{1})", playerInvo.Artifacts.Count, playerInvo.MaxArtifacts), new Vector2(messageSpacing, beginningHeight + (messageSpacing * messageNumberLeft)), Color.CornflowerBlue);
                messageNumberLeft += 1;
                foreach (Guid item in playerInvo.Artifacts)
                {
                    Color color = (menu.SelectedItem == item) ? Colors.Messages.LootPickup : Color.NavajoWhite;
                    string prepend = (menu.SelectedItem == item) ? "> " : string.Empty;
                    spriteBatch.DrawString(messageFont, prepend + spaceComponents.NameComponents[item].Name, new Vector2(messageSpacing, (messageSpacing * messageNumberLeft) + beginningHeight), color);
                    messageNumberLeft += 1;
                }
                spriteBatch.DrawString(messageFont, string.Format(Messages.InventoryConsumables + " ({0}/{1})", playerInvo.Consumables.Count, playerInvo.MaxConsumables), new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft) + beginningHeight), Color.CornflowerBlue);
                messageNumberLeft += 1;
                foreach (Guid item in playerInvo.Consumables)
                {
                    Color color = (menu.SelectedItem == item) ? Colors.Messages.LootPickup : Color.NavajoWhite;
                    string prepend = (menu.SelectedItem == item) ? "> " : string.Empty;
                    spriteBatch.DrawString(messageFont, prepend + spaceComponents.NameComponents[item].Name, new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft) + beginningHeight), color);
                    messageNumberLeft += 1;
                }
                messageNumberLeft += 2;

                //Gather item information panels
                if (menu.SelectedItem != Guid.Empty)
                {
                    PickupComponent itemInfo = spaceComponents.PickupComponents[menu.SelectedItem];
                    NameComponent itemName = spaceComponents.NameComponents[menu.SelectedItem];
                    ValueComponent itemValue = spaceComponents.ValueComponents[menu.SelectedItem];
                    List<Tuple<Color, string>> centerMessages = new List<Tuple<Color, string>>();
                    List<Tuple<Color, string>> rightMessages = new List<Tuple<Color, string>>();

                    //Get name, description, and value
                    rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, itemName.Name));
                    rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, itemName.Description));
                    centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("This item can be scrapped for {0} gold.", itemValue.Gold)));

                    switch (itemInfo.PickupType)
                    {
                        case ItemType.ARTIFACT:
                            //Collect item use stats for right panel
                            //Artifact Stats
                            ArtifactStatsComponent artifactStats = spaceComponents.ArtifactStatsComponents[menu.SelectedItem];
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("Upgrade Level: {0}", artifactStats.UpgradeLevel)));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("Depth found: {0}", artifactStats.FloorFound)));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Good, "STATISTICS WHILE EQUIPPED:"));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Kills: {0}", artifactStats.KillsWith)));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Maximum hit combo: {0}", artifactStats.MaximumComboWith)));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Damage given: {0}", artifactStats.DamageGivenWith)));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Damage taken: {0}", artifactStats.DamageTakenWith)));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Times dodged: {0}", artifactStats.DodgesWith)));
                            rightMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Times missed: {0}", artifactStats.MissesWith)));

                            //Draw commands for artifact on the left panel
                            spriteBatch.DrawString(messageFont, "Commands: ", new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft++) + beginningHeight), Colors.Messages.Normal);
                            spriteBatch.DrawString(messageFont, Messages.Upgrade, new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft++) + beginningHeight), Colors.Messages.Normal);
                            spriteBatch.DrawString(messageFont, Messages.Scrap, new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft++) + beginningHeight), Colors.Messages.Normal);
                            spriteBatch.DrawString(messageFont, Messages.Drop, new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft++) + beginningHeight), Colors.Messages.Normal);

                            //Collect info for middle panel
                            StatModificationComponent stats = spaceComponents.StatModificationComponents[menu.SelectedItem];
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, "This artifact affects the following stats: "));
                            if (stats.AccuracyChange != 0)
                            {
                                string sign = stats.AccuracyChange > 0 ? "+" : string.Empty;
                                Color color = stats.AccuracyChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                centerMessages.Add(new Tuple<Color, string>(color, string.Format("Accuracy {0}{1}", sign, stats.AccuracyChange)));
                            }
                            if (stats.DefenseChange != 0)
                            {
                                string sign = stats.DefenseChange > 0 ? "+" : string.Empty;
                                Color color = stats.DefenseChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                centerMessages.Add(new Tuple<Color, string>(color, string.Format("Defense {0}{1}", sign, stats.DefenseChange)));
                            }
                            if (stats.HealthChange != 0)
                            {
                                string sign = stats.HealthChange > 0 ? "+" : string.Empty;
                                Color color = stats.HealthChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                centerMessages.Add(new Tuple<Color, string>(color, string.Format("Maximum Health {0}{1}", sign, stats.HealthChange)));
                            }
                            if (stats.DieNumberChange != 0)
                            {
                                string sign = stats.DieNumberChange > 0 ? "+" : string.Empty;
                                Color color = stats.DieNumberChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                centerMessages.Add(new Tuple<Color, string>(color, string.Format("Dice Number on Attack {0}{1}", sign, stats.DieNumberChange)));
                            }
                            if (stats.MinimumDamageChange != 0)
                            {
                                string sign = stats.MinimumDamageChange > 0 ? "+" : string.Empty;
                                Color color = stats.MinimumDamageChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                centerMessages.Add(new Tuple<Color, string>(color, string.Format("Minimum Damage {0}{1}", sign, stats.MinimumDamageChange)));
                            }
                            if (stats.MaximumDamageChange != 0)
                            {
                                string sign = stats.MaximumDamageChange > 0 ? "+" : string.Empty;
                                Color color = stats.MaximumDamageChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                centerMessages.Add(new Tuple<Color, string>(color, string.Format("Maximum Damage {0}{1}", sign, stats.MaximumDamageChange)));
                            }

                            //PLACEHOLDER
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, "This artifact has the following effects: "));
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Good, "Passive 1: Kills with this item equipped generate 100 gold."));
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, "Passive 2: Enemies will not flee (LOCKED)"));
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, "Passive 3: Tiles stepped on turn to lava for 5 turns (LOCKED)"));
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, "Bonus: ??? (Kill 40 more enemies with this item to unlock)"));
                            break;
                        case ItemType.CONSUMABLE:
                            //Draw command info on left panel
                            spriteBatch.DrawString(messageFont, "Commands: ", new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft++) + beginningHeight), Colors.Messages.Normal);
                            spriteBatch.DrawString(messageFont, Messages.Scrap, new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft++) + beginningHeight), Colors.Messages.Normal);
                            spriteBatch.DrawString(messageFont, Messages.Drop, new Vector2(messageSpacing, (messageSpacing * 3) + (messageSpacing * messageNumberLeft++) + beginningHeight), Colors.Messages.Normal);

                            //Collect info for middle panel
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            ItemFunctionsComponent consumableInfo = spaceComponents.ItemFunctionsComponents[menu.SelectedItem];
                            centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Good, string.Format("This item has {0} uses left.", consumableInfo.Uses)));
                            if (consumableInfo.Uses > 1)
                            {
                                centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format("This item loses {0} value each use.", consumableInfo.CostToUse)));
                            }
                            if (consumableInfo.Ranged)
                            {
                                centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("This item is cast at a range.")));
                            }
                            else
                            {
                                centerMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("This item is used where you stand.")));
                            }
                            break;
                    }

                    //Print out all the info
                    foreach (Tuple<Color, string> message in centerMessages)
                    {
                        if (string.IsNullOrEmpty(message.Item2))
                        {
                            continue;
                        }
                        string text = MessageDisplaySystem.WordWrap(messageFont, message.Item2, panelWidth - 30 - DevConstants.Grid.TileSpriteSize);

                        float textHeight = messageFont.MeasureString(message.Item2).Y;
                        spriteBatch.DrawString(messageFont, text, new Vector2(messageSpacing * 2 + panelWidth, (int)beginningHeight + (int)textHeight + 10 + (messageNumberMiddle * messageSpacing)), message.Item1);
                        messageNumberMiddle += Regex.Matches(text, System.Environment.NewLine).Count;
                        messageNumberMiddle += 1;
                    }
                    foreach (Tuple<Color, string> message in rightMessages)
                    {
                        if (string.IsNullOrEmpty(message.Item2))
                        {
                            continue;
                        }
                        string text = MessageDisplaySystem.WordWrap(messageFont, message.Item2, panelWidth - 30 - DevConstants.Grid.TileSpriteSize);

                        float textHeight = messageFont.MeasureString(message.Item2).Y;
                        spriteBatch.DrawString(messageFont, text, new Vector2(messageSpacing * 3 + panelWidth * 2, (int)beginningHeight + (int)textHeight + 10 + (messageNumberRight * messageSpacing)), message.Item1);
                        if(text != System.Environment.NewLine)
                        {
                            messageNumberRight += Regex.Matches(text, System.Environment.NewLine).Count;
                        }
                        messageNumberRight += 1;
                    }
                }

            }
        }


        #region artifact stats update functions
        public static void IncrementKillsWithArtifact(StateSpaceComponents spaceComponents, Guid entityWithInventory)
        {
            if(spaceComponents.InventoryComponents.ContainsKey(entityWithInventory))
            {
                InventoryComponent inventory = spaceComponents.InventoryComponents[entityWithInventory];
                foreach(Guid id in inventory.Artifacts)
                {
                    ArtifactStatsComponent stats = spaceComponents.ArtifactStatsComponents[id];
                    stats.KillsWith += 1;
                    spaceComponents.ArtifactStatsComponents[id] = stats;
                }
            }
        }

        public static void IncrementDamageGivenWithArtifact(StateSpaceComponents spaceComponents, Guid entityWithInventory, int damage)
        {
            if (spaceComponents.InventoryComponents.ContainsKey(entityWithInventory))
            {
                InventoryComponent inventory = spaceComponents.InventoryComponents[entityWithInventory];
                foreach (Guid id in inventory.Artifacts)
                {
                    ArtifactStatsComponent stats = spaceComponents.ArtifactStatsComponents[id];
                    stats.DamageGivenWith += damage;
                    spaceComponents.ArtifactStatsComponents[id] = stats;
                }
            }
        }

        public static void IncrementDamageTakenWithArtifact(StateSpaceComponents spaceComponents, Guid entityWithInventory, int damage)
        {
            if (spaceComponents.InventoryComponents.ContainsKey(entityWithInventory))
            {
                InventoryComponent inventory = spaceComponents.InventoryComponents[entityWithInventory];
                foreach (Guid id in inventory.Artifacts)
                {
                    ArtifactStatsComponent stats = spaceComponents.ArtifactStatsComponents[id];
                    stats.DamageTakenWith += damage;
                    spaceComponents.ArtifactStatsComponents[id] = stats;
                }
            }
        }

        public static void IncrementTimesDodgedWithArtifact(StateSpaceComponents spaceComponents, Guid entityWithInventory)
        {
            if (spaceComponents.InventoryComponents.ContainsKey(entityWithInventory))
            {
                InventoryComponent inventory = spaceComponents.InventoryComponents[entityWithInventory];
                foreach (Guid id in inventory.Artifacts)
                {
                    ArtifactStatsComponent stats = spaceComponents.ArtifactStatsComponents[id];
                    stats.DodgesWith += 1;
                    spaceComponents.ArtifactStatsComponents[id] = stats;
                }
            }
        }

        public static void IncrementTimesMissesWithArtifact(StateSpaceComponents spaceComponents, Guid entityWithInventory)
        {
            if (spaceComponents.InventoryComponents.ContainsKey(entityWithInventory))
            {
                InventoryComponent inventory = spaceComponents.InventoryComponents[entityWithInventory];
                foreach (Guid id in inventory.Artifacts)
                {
                    ArtifactStatsComponent stats = spaceComponents.ArtifactStatsComponents[id];
                    stats.MissesWith += 1;
                    spaceComponents.ArtifactStatsComponents[id] = stats;
                }
            }
        }

        public static void UpdateMaxCombo(StateSpaceComponents spaceComponents, Guid entityWithInventory, int hitCombo)
        {
            if (spaceComponents.InventoryComponents.ContainsKey(entityWithInventory))
            {
                InventoryComponent inventory = spaceComponents.InventoryComponents[entityWithInventory];
                foreach (Guid id in inventory.Artifacts)
                {
                    ArtifactStatsComponent stats = spaceComponents.ArtifactStatsComponents[id];
                    stats.MaximumComboWith = (stats.MaximumComboWith >= hitCombo) ? stats.MaximumComboWith : hitCombo;
                    spaceComponents.ArtifactStatsComponents[id] = stats;
                }
            }
        }
        #endregion


    }
}