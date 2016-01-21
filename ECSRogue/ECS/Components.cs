using ECSRogue.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS
{
    [Flags]
    public enum Component : ulong
    {
        NONE = 0,
        COMPONENT_POSITION = 1 << 0,
        COMPONENT_VELOCITY = 1 << 1,
        COMPONENT_DISPLAY = 1 << 2,
        COMPONENT_LABEL = 1 << 3,
        COMPONENT_ANIMATION = 1 << 4,
        COMPONENT_INPUTMOVEMENT = 1 << 6,
        COMPONENT_SIGHTRADIUS = 1 << 7,
        COMPONENT_GAMEMESSAGE = 1 << 8,
        COMPONENT_GAMEPLAY_INFO = 1 << 9,
        COMPONENT_SKILL_LEVELS = 1 << 10,
        COMPONENT_AI = 1 << 11,
        COMPONENT_TARGET_POSITION = 1 << 12,
        COMPONENT_DIRECTION = 1 << 13,
        COMPONENT_TIME_TO_LIVE = 1  << 14,
        COMPONENT_PLAYER = 1 << 15
    }



    public struct ComponentMasks
    {
        public const Component Player = Component.COMPONENT_POSITION | Component.COMPONENT_DISPLAY | Component.COMPONENT_SIGHTRADIUS 
            | Component.COMPONENT_GAMEPLAY_INFO | Component.COMPONENT_SKILL_LEVELS | Component.COMPONENT_PLAYER;

        public const Component Enemy = Component.COMPONENT_POSITION | Component.COMPONENT_DISPLAY | Component.COMPONENT_SIGHTRADIUS 
            | Component.COMPONENT_SKILL_LEVELS | Component.COMPONENT_AI;

        public const Component InputMoveable = Component.COMPONENT_POSITION | Component.COMPONENT_INPUTMOVEMENT;

        public const Component Drawable = Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION;
        public const Component DrawableLabel = Component.COMPONENT_LABEL | Component.COMPONENT_POSITION; 

        public const Component Animated = Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION | Component.COMPONENT_ANIMATION; //Not implemented

        public const Component MovingEntity = Component.COMPONENT_POSITION | Component.COMPONENT_VELOCITY | Component.COMPONENT_TARGET_POSITION; 
        public const Component IndefiniteMovingEntity = Component.COMPONENT_POSITION | Component.COMPONENT_VELOCITY | Component.COMPONENT_DIRECTION; 
    }

    public class StateComponents
    {
        public GameplayInfoComponent GameplayInfo { get; set; }
        public SkillLevelsComponent PlayerSkillLevels { get; set; }
    }

    public class StateSpaceComponents
    {
        public List<Entity> Entities { get; private set; }
        public List<Guid> EntitiesToDelete { get; private set; }
        public Dictionary<Guid, PositionComponent> PositionComponents { get; private set; }
        public Dictionary<Guid, VelocityComponent> VelocityComponents { get; private set; }
        public Dictionary<Guid, DisplayComponent> DisplayComponents { get; private set; }
        public Dictionary<Guid, AnimationComponent> AnimationComponents { get; private set; }
        public Dictionary<Guid, SightRadiusComponent> SightRadiusComponents { get; private set; }
        public Dictionary<Guid, LabelComponent> LabelComponents { get; private set; }
        public Dictionary<Guid, GameMessageComponent> GameMessageComponents { get; private set; }
        public Dictionary<Guid, GameplayInfoComponent> GameplayInfoComponents { get; private set; }
        public Dictionary<Guid, SkillLevelsComponent> SkillLevelsComponents { get; private set; }
        public Dictionary<Guid, TargetPositionComponent> TargetPositionComponents { get; private set; }
        public Dictionary<Guid, DirectionComponent> DirectionComponents { get; private set; }
        public Dictionary<Guid, TimeToLiveComponent> TimeToLiveComponents { get; private set; }
        public Random random { get; private set; }

        public StateSpaceComponents()
        {
            EntitiesToDelete = new List<Guid>();
            Entities = new List<Entity>();
            PositionComponents = new Dictionary<Guid, PositionComponent>();
            VelocityComponents = new Dictionary<Guid, VelocityComponent>();
            DisplayComponents = new Dictionary<Guid, DisplayComponent>();
            AnimationComponents = new Dictionary<Guid, AnimationComponent>();
            LabelComponents = new Dictionary<Guid, LabelComponent>();
            SightRadiusComponents = new Dictionary<Guid, SightRadiusComponent>();
            GameMessageComponents = new Dictionary<Guid, GameMessageComponent>();
            GameplayInfoComponents = new Dictionary<Guid, GameplayInfoComponent>();
            SkillLevelsComponents = new Dictionary<Guid, SkillLevelsComponent>();
            TargetPositionComponents = new Dictionary<Guid, TargetPositionComponent>();
            DirectionComponents = new Dictionary<Guid, DirectionComponent>();
            TimeToLiveComponents = new Dictionary<Guid, TimeToLiveComponent>();
            random = new Random();
        }

        public Guid CreateEntity()
        {
            Entity newEntity = new Entity();
            this.Entities.Add(newEntity);
            return newEntity.Id;
        }

        public void DestroyEntity(Guid id)
        {
            Entity removal = this.Entities.Where(x => x.Id == id).FirstOrDefault();
            if (removal != null)
            {
                Entities.Remove(removal);
                PositionComponents.Remove(id);
                VelocityComponents.Remove(id);
                DisplayComponents.Remove(id);
                AnimationComponents.Remove(id);
                SightRadiusComponents.Remove(id);
                LabelComponents.Remove(id);
                GameMessageComponents.Remove(id);
                GameplayInfoComponents.Remove(id);
                SkillLevelsComponents.Remove(id);
                TargetPositionComponents.Remove(id);
                DirectionComponents.Remove(id);
                TimeToLiveComponents.Remove(id);
            }
        }
    }
}
