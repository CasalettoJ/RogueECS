using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.GraphicalEffectsComponents;
using ECSRogue.ECS.Components.ItemizationComponents;
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
        COMPONENT_OBSERVER = 1 << 17,
        COMPONENT_AI_COMBAT = 1 << 18,
        COMPONENT_AI_ALIGNMENT = 1 << 19,
        COMPONENT_AI_STATE = 1 << 20,
        COMPONENT_ENTITY_MESSAGES = 1 << 21,
        COMPONENT_AI_FIELDOFVIEW = 1 << 22,
        COMPONENT_ALTERNATE_FOV_COLOR = 1 << 23,
        COMPONENT_AI_SLEEP = 1 << 24,
        COMPONENT_AI_ROAM = 1 << 25,
        COMPONENT_AI_FLEE = 1 << 26,
        COMPONENT_HEALTH_REGENERATION = 1 << 27,
        COMPONENT_OUTLINE = 1 << 28,
        COMPONENT_OUTLINE_SECONDARY = 1 << 29,
        COMPONENT_PICKUP = 1 << 30,
        COMPONENT_STAT_MODIFICATION = 1ul << 31,
        COMPONENT_INVENTORY = 1ul << 32,
        COMPONENT_VALUE = 1ul << 33,
        COMPONENT_ITEM_FUNCTIONS = 1ul << 34,
        COMPONENT_PASSIVES = 1ul << 35,
        COMPONENT_ARTIFACT_STATS = 1ul << 36
    }

    public struct ComponentMasks
    {
        //Actors and AI
        public const Component Player = Component.COMPONENT_POSITION | Component.COMPONENT_DISPLAY | Component.COMPONENT_SIGHTRADIUS
            | Component.COMPONENT_GAMEPLAY_INFO | Component.COMPONENT_SKILL_LEVELS | Component.COMPONENT_COLLISION | Component.COMPONENT_NAME
            | Component.COMPONENT_PLAYER | Component.COMPONENT_AI_ALIGNMENT | Component.COMPONENT_ENTITY_MESSAGES;

        public const Component CombatReadyAI = Component.COMPONENT_AI_ALIGNMENT | Component.COMPONENT_AI_COMBAT | Component.COMPONENT_AI_STATE
            | Component.COMPONENT_SKILL_LEVELS | Component.COMPONENT_COLLISION | Component.COMPONENT_AI_SLEEP | Component.COMPONENT_AI_ROAM | Component.COMPONENT_AI_FLEE
            | Component.COMPONENT_NAME | Component.COMPONENT_POSITION | Component.COMPONENT_ENTITY_MESSAGES;

        public const Component Observer = Component.COMPONENT_POSITION | Component.COMPONENT_COLLISION | Component.COMPONENT_DISPLAY | Component.COMPONENT_INPUTMOVEMENT | Component.COMPONENT_OBSERVER;
        public const Component AIView = Component.COMPONENT_AI_FIELDOFVIEW | Component.COMPONENT_POSITION;
        public const Component Collidable = Component.COMPONENT_POSITION | Component.COMPONENT_COLLISION;
        public const Component InputMoveable = Component.COMPONENT_POSITION | Component.COMPONENT_INPUTMOVEMENT;

        //Observer
        public const Component Observable = Component.COMPONENT_POSITION | Component.COMPONENT_NAME;
        public const Component ObservableItem = Component.COMPONENT_POSITION | Component.COMPONENT_NAME | Component.COMPONENT_PICKUP;
        public const Component ObservableSkillModifications = Component.COMPONENT_POSITION | Component.COMPONENT_STAT_MODIFICATION;
        public const Component ObservableValue = Component.COMPONENT_POSITION | Component.COMPONENT_VALUE;
        public const Component ObservableUsage = Component.COMPONENT_POSITION | Component.COMPONENT_ITEM_FUNCTIONS;
        public const Component ObservableAI = Component.COMPONENT_POSITION | Component.COMPONENT_NAME | ComponentMasks.CombatReadyAI;

        //Inventory
        public const Component PickupItem = Component.COMPONENT_PICKUP | Component.COMPONENT_VALUE | Component.COMPONENT_NAME | Component.COMPONENT_COLLISION;
        public const Component Consumable = ComponentMasks.PickupItem | Component.COMPONENT_ITEM_FUNCTIONS;
        public const Component Artifact = ComponentMasks.PickupItem | Component.COMPONENT_STAT_MODIFICATION | Component.COMPONENT_PASSIVES | Component.COMPONENT_ARTIFACT_STATS;
        public const Component InventoryPickup = Component.COMPONENT_INVENTORY | Component.COMPONENT_ENTITY_MESSAGES | Component.COMPONENT_NAME | Component.COMPONENT_POSITION;

        //Animations
        public const Component MovingEntity = Component.COMPONENT_POSITION | Component.COMPONENT_VELOCITY | Component.COMPONENT_TARGET_POSITION;
        public const Component IndefiniteMovingEntity = Component.COMPONENT_POSITION | Component.COMPONENT_VELOCITY | Component.COMPONENT_DIRECTION;
        public const Component Animated = Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION | Component.COMPONENT_ANIMATION; //Not implemented
        public const Component GlowingOutline = Component.COMPONENT_OUTLINE | Component.COMPONENT_OUTLINE_SECONDARY | Component.COMPONENT_POSITION;

        //Drawable
        public const Component Drawable = Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION;
        public const Component DrawableLabel = Component.COMPONENT_LABEL | Component.COMPONENT_POSITION;
        public const Component FOVColorChange = Component.COMPONENT_ALTERNATE_FOV_COLOR | Component.COMPONENT_AI_FIELDOFVIEW;
        public const Component DrawableOutline = Component.COMPONENT_OUTLINE | Component.COMPONENT_POSITION;

        //Effects        
        public const Component HealthRegen = Component.COMPONENT_HEALTH_REGENERATION | Component.COMPONENT_SKILL_LEVELS;



    }

    public class StateComponents
    {
        public GameplayInfoComponent GameplayInfo { get; set; }
        public SkillLevelsComponent PlayerSkillLevels { get; set; }
    }

    public class StateSpaceComponents
    {
        //Containers
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
        public Dictionary<Guid, AICombat> AICombatComponents { get; private set; }
        public Dictionary<Guid, AIAlignment> AIAlignmentComponents { get; private set; }
        public Dictionary<Guid, AIState> AIStateComponents { get; private set; }
        public Dictionary<Guid, AIFieldOfView> AIFieldOfViewComponents { get; private set; }
        public Dictionary<Guid, AISleep> AISleepComponents { get; private set; }
        public Dictionary<Guid, AIRoam> AIRoamComponents { get; private set; }
        public Dictionary<Guid, AIFlee> AIFleeComponents { get; private set; }
        public Dictionary<Guid, InputMovementComponent> InputMovementComponents { get; private set; }
        public Dictionary<Guid, EntityMessageComponent> EntityMessageComponents { get; private set; }
        public Dictionary<Guid, AlternateFOVColorChangeComponent> AlternateFOVColorChangeComponents { get; private set; }
        public Dictionary<Guid, HealthRegenerationComponent> HealthRegenerationComponents { get; private set; }
        public Dictionary<Guid, OutlineComponent> OutlineComponents { get; private set; }
        public Dictionary<Guid, SecondaryOutlineComponent> SecondaryOutlineComponents { get; private set; }
        public Dictionary<Guid, InventoryComponent> InventoryComponents { get; private set; }
        public Dictionary<Guid, PickupComponent> PickupComponents { get; private set; }
        public Dictionary<Guid, StatModificationComponent> StatModificationComponents { get; private set; }
        public Dictionary<Guid, ValueComponent> ValueComponents { get; private set; }
        public Dictionary<Guid, ItemFunctionsComponent> ItemFunctionsComponents { get; private set; }
        public Dictionary<Guid, PassivesComponent> PassivesComponents { get; private set; }
        public Dictionary<Guid, ArtifactStatsComponent> ArtifactStatsComponents { get; private set; }

        public List<Action> DelayedActions { get; private set; }
        public PlayerComponent PlayerComponent { get; set; }
        public GameMessageComponent GameMessageComponent { get; set; }
        public GameplayInfoComponent GameplayInfoComponent { get; set; }
        public GlobalCollisionComponent GlobalCollisionComponent { get; set; }
        public ObserverComponent ObserverComponent { get; set; }
        public InventoryMenuComponent InventoryMenuComponent { get; set; }
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
            AICombatComponents = new Dictionary<Guid, AICombat>();
            AIAlignmentComponents = new Dictionary<Guid, AIAlignment>();
            AIStateComponents = new Dictionary<Guid, AIState>();
            AIFieldOfViewComponents = new Dictionary<Guid, AIFieldOfView>();
            AISleepComponents = new Dictionary<Guid, AISleep>();
            AIRoamComponents = new Dictionary<Guid, AIRoam>();
            AIFleeComponents = new Dictionary<Guid, AIFlee>();
            InputMovementComponents = new Dictionary<Guid, InputMovementComponent>();
            EntityMessageComponents = new Dictionary<Guid, EntityMessageComponent>();
            AlternateFOVColorChangeComponents = new Dictionary<Guid, AlternateFOVColorChangeComponent>();
            HealthRegenerationComponents = new Dictionary<Guid, HealthRegenerationComponent>();
            OutlineComponents = new Dictionary<Guid, OutlineComponent>();
            SecondaryOutlineComponents = new Dictionary<Guid, SecondaryOutlineComponent>();
            InventoryComponents = new Dictionary<Guid, InventoryComponent>();
            PickupComponents = new Dictionary<Guid, PickupComponent>();
            StatModificationComponents = new Dictionary<Guid, StatModificationComponent>();
            ValueComponents = new Dictionary<Guid, ValueComponent>();
            ItemFunctionsComponents = new Dictionary<Guid, ItemFunctionsComponent>();
            PassivesComponents = new Dictionary<Guid, PassivesComponent>();
            ArtifactStatsComponents = new Dictionary<Guid, ArtifactStatsComponent>();

            GlobalCollisionComponent = new GlobalCollisionComponent() { EntitiesThatCollided = new List<Guid>() };
            PlayerComponent = new PlayerComponent();
            GameMessageComponent = new GameMessageComponent();
            GameplayInfoComponent = new GameplayInfoComponent();
            ObserverComponent = new ObserverComponent() { Observed = new List<Guid>() };
            InventoryMenuComponent = new InventoryMenuComponent();
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
                AICombatComponents.Remove(id);
                AIAlignmentComponents.Remove(id);
                AIStateComponents.Remove(id);
                AIFieldOfViewComponents.Remove(id);
                AISleepComponents.Remove(id);
                AIRoamComponents.Remove(id);
                AIFleeComponents.Remove(id);
                InputMovementComponents.Remove(id);
                EntityMessageComponents.Remove(id);
                AlternateFOVColorChangeComponents.Remove(id);
                HealthRegenerationComponents.Remove(id);
                OutlineComponents.Remove(id);
                SecondaryOutlineComponents.Remove(id);
                InventoryComponents.Remove(id);
                PickupComponents.Remove(id);
                StatModificationComponents.Remove(id);
                ValueComponents.Remove(id);
                ItemFunctionsComponents.Remove(id);
                PassivesComponents.Remove(id);
                ArtifactStatsComponents.Remove(id);
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
