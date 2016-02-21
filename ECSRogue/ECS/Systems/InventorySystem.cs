using ECSRogue.BaseEngine.Interfaces;
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
                InventoryComponent inventory = spaceComponents.InventoryComponents[user];
                inventory.Consumables.Remove(item);
                spaceComponents.InventoryComponents[user] = inventory;
                spaceComponents.EntitiesToDelete.Add(item);
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
                                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("{0} picked up {1} gold.", collidingEntityName.Name, itemValue.Gold)));
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
                                                string.Format(collidingEntityMessages.PickupItemMessages[spaceComponents.random.Next(0, collidingEntityMessages.PickupItemMessages.Length)], collidingEntityName.Name, itemName.Name)));
                                        }
                                    }
                                    //If it can't fit in, and the entity has a message for the situation, display it.
                                    else if(spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages != null && spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages.Length > 0)
                                    {
                                        spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages[spaceComponents.random.Next(0, spaceComponents.EntityMessageComponents[collidingEntity].ConsumablesFullMessages.Length)]));
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
                                                string.Format(collidingEntityMessages.PickupItemMessages[spaceComponents.random.Next(0, collidingEntityMessages.PickupItemMessages.Length)], collidingEntityName.Name, itemName.Name)));
                                        }
                                    }
                                    //If it can't fit in, and the entity has a message for the situation, display it.
                                    else if (spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages != null && spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages.Length > 0)
                                    {
                                        spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages[spaceComponents.random.Next(0, spaceComponents.EntityMessageComponents[collidingEntity].ArtifactsFullMessages.Length)]));
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }


    }
}