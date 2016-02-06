﻿using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.MeleeMessageComponents;
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
        COMPONENT_TARGET_POSITION = 1 << 11,
        COMPONENT_DIRECTION = 1 << 12,
        COMPONENT_TIME_TO_LIVE = 1 << 13,
        COMPONENT_PLAYER = 1 << 14,
        COMPONENT_COLLISION = 1 << 15,
        COMPONENT_NAME = 1 << 16,
        COMPONENT_AI_MOVEMENT = 1 << 17,
        COMPONENT_AI_COMBAT = 1 << 18,
        COMPONENT_AI_ALIGNMENT = 1 << 19,
        COMPONENT_AI_STATE = 1 << 20,
        COMPONENT_MELEE_ATTACK_NPC_MESSAGES = 1 << 21,
        COMPONENT_MELEE_ATTACK_PLAYER_MESSAGES = 1 << 22,
        COMPONENT_MELEE_DODGE_MESSAGES = 1 << 23,
        COMPONENT_MELEE_TAKE_DAMAGE_MESSAGES = 1 << 24,
        COMPONENT_AI_FIELDOFVIEW = 1 << 25
    }



    public struct ComponentMasks
    {
        public struct SubMasks
        {

            public const Component MeleeMessageComponents = Component.COMPONENT_MELEE_ATTACK_NPC_MESSAGES | Component.COMPONENT_MELEE_ATTACK_PLAYER_MESSAGES | Component.COMPONENT_MELEE_DODGE_MESSAGES | Component.COMPONENT_MELEE_TAKE_DAMAGE_MESSAGES;
        }

        public const Component Player = Component.COMPONENT_POSITION | Component.COMPONENT_DISPLAY | Component.COMPONENT_SIGHTRADIUS
            | Component.COMPONENT_GAMEPLAY_INFO | Component.COMPONENT_SKILL_LEVELS | Component.COMPONENT_COLLISION | Component.COMPONENT_NAME
            | Component.COMPONENT_PLAYER | Component.COMPONENT_AI_ALIGNMENT | ComponentMasks.SubMasks.MeleeMessageComponents;

        public const Component CombatReadyAI = Component.COMPONENT_AI_ALIGNMENT | Component.COMPONENT_AI_COMBAT | Component.COMPONENT_AI_STATE
            | Component.COMPONENT_SKILL_LEVELS | Component.COMPONENT_COLLISION | Component.COMPONENT_NAME | Component.COMPONENT_POSITION 
            | ComponentMasks.SubMasks.MeleeMessageComponents;

        public const Component AIView = Component.COMPONENT_AI_FIELDOFVIEW | Component.COMPONENT_AI_STATE | Component.COMPONENT_AI_ALIGNMENT | Component.COMPONENT_POSITION;

        public const Component MovingAI = Component.COMPONENT_POSITION | Component.COMPONENT_AI_MOVEMENT; //Not Implemented

        public const Component InputMoveable = Component.COMPONENT_POSITION | Component.COMPONENT_INPUTMOVEMENT;

        public const Component Drawable = Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION;
        public const Component DrawableLabel = Component.COMPONENT_LABEL | Component.COMPONENT_POSITION;
       

        public const Component Animated = Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION | Component.COMPONENT_ANIMATION; //Not implemented
        public const Component Collidable = Component.COMPONENT_POSITION | Component.COMPONENT_COLLISION;

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
        public Dictionary<Guid, SkillLevelsComponent> SkillLevelsComponents { get; private set; }
        public Dictionary<Guid, TargetPositionComponent> TargetPositionComponents { get; private set; }
        public Dictionary<Guid, DirectionComponent> DirectionComponents { get; private set; }
        public Dictionary<Guid, TimeToLiveComponent> TimeToLiveComponents { get; private set; }
        public Dictionary<Guid, CollisionComponent> CollisionComponents { get; private set; }
        public Dictionary<Guid, NameComponent> NameComponents { get; private set; }
        public Dictionary<Guid, AIMovement> AIMovementComponents { get; private set; }
        public Dictionary<Guid, AICombat> AICombatComponents { get; private set; }
        public Dictionary<Guid, AIAlignment> AIAlignmentComponents { get; private set; }
        public Dictionary<Guid, AIState> AIStateComponents { get; private set; }
        public Dictionary<Guid, AIFieldOfView> AIFieldOfViewComponents { get; private set; }
        public Dictionary<Guid, InputMovementComponent> InputMovementComponents { get; private set; }
        public Dictionary<Guid, MeleeAttackNPCMessageComponent> MeleeAttackNPCMessageComponents { get; private set; }
        public Dictionary<Guid, MeleeAttackPlayerMessageComponent> MeleeAttackPlayerMessageComponents { get; private set; }
        public Dictionary<Guid, TakeMeleeDamageMesageComponent> TakeMeleeDamageMesageComponents { get; private set; }
        public Dictionary<Guid, DodgeMeleeMessageComponent> DodgeMeleeMessageComponents { get; private set; }

        public List<Action> DelayedActions { get; private set; }
        public PlayerComponent PlayerComponent { get; set; }
        public GameMessageComponent GameMessageComponent { get; set; }
        public GameplayInfoComponent GameplayInfoComponent { get; set; }
        public GlobalCollisionComponent GlobalCollisionComponent { get; set; }
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
            SkillLevelsComponents = new Dictionary<Guid, SkillLevelsComponent>();
            TargetPositionComponents = new Dictionary<Guid, TargetPositionComponent>();
            DirectionComponents = new Dictionary<Guid, DirectionComponent>();
            TimeToLiveComponents = new Dictionary<Guid, TimeToLiveComponent>();
            CollisionComponents = new Dictionary<Guid, CollisionComponent>();
            NameComponents = new Dictionary<Guid, NameComponent>();
            AIMovementComponents = new Dictionary<Guid, AIMovement>();
            AICombatComponents = new Dictionary<Guid, AICombat>();
            AIAlignmentComponents = new Dictionary<Guid, AIAlignment>();
            AIStateComponents = new Dictionary<Guid, AIState>();
            AIFieldOfViewComponents = new Dictionary<Guid, AIFieldOfView>();
            InputMovementComponents = new Dictionary<Guid, InputMovementComponent>();
            MeleeAttackNPCMessageComponents = new Dictionary<Guid, MeleeAttackNPCMessageComponent>();
            MeleeAttackPlayerMessageComponents = new Dictionary<Guid, MeleeAttackPlayerMessageComponent>();
            TakeMeleeDamageMesageComponents = new Dictionary<Guid, TakeMeleeDamageMesageComponent>();
            DodgeMeleeMessageComponents = new Dictionary<Guid, DodgeMeleeMessageComponent>();

            GlobalCollisionComponent = new GlobalCollisionComponent() { EntitiesThatCollided = new List<Guid>() };
            PlayerComponent = new PlayerComponent();
            GameMessageComponent = new GameMessageComponent();
            GameplayInfoComponent = new GameplayInfoComponent();
            DelayedActions = new List<Action>();
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
                SkillLevelsComponents.Remove(id);
                TargetPositionComponents.Remove(id);
                DirectionComponents.Remove(id);
                TimeToLiveComponents.Remove(id);
                CollisionComponents.Remove(id);
                NameComponents.Remove(id);
                AIMovementComponents.Remove(id);
                AICombatComponents.Remove(id);
                AIAlignmentComponents.Remove(id);
                AIStateComponents.Remove(id);
                AIFieldOfViewComponents.Remove(id);
                InputMovementComponents.Remove(id);
                MeleeAttackPlayerMessageComponents.Remove(id);
                TakeMeleeDamageMesageComponents.Remove(id);
                MeleeAttackNPCMessageComponents.Remove(id);
                DodgeMeleeMessageComponents.Remove(id);
            }
        }

        public void InvokeDelayedActions()
        {
            foreach(Action action in this.DelayedActions)
            {
                action();
            }
            this.DelayedActions = new List<Action>();
        }
    }
}
