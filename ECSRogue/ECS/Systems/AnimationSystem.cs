using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.GraphicalEffectsComponents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class AnimationSystem
    {
        public static void UpdateFovColors(StateSpaceComponents spaceComponents, GameTime gameTime)
        {
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.FOVColorChange) == ComponentMasks.FOVColorChange).Select(x => x.Id))
            {
                AlternateFOVColorChangeComponent altColorInfo = spaceComponents.AlternateFOVColorChangeComponents[id];
                altColorInfo.Seconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(altColorInfo.Seconds >= altColorInfo.SwitchAtSeconds)
                {
                    AIFieldOfView fovInfo = spaceComponents.AIFieldOfViewComponents[id];
                    Color temp = fovInfo.Color;
                    fovInfo.Color = altColorInfo.AlternateColor;
                    altColorInfo.AlternateColor = temp;
                    altColorInfo.Seconds = 0f;
                    spaceComponents.AIFieldOfViewComponents[id] = fovInfo;
                }
                spaceComponents.AlternateFOVColorChangeComponents[id] = altColorInfo;
            }
        }

        public static void UpdateOutlineColors(StateSpaceComponents spaceComponents, GameTime gameTime)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.GlowingOutline) == ComponentMasks.GlowingOutline).Select(x => x.Id))
            {
                SecondaryOutlineComponent altColorInfo = spaceComponents.SecondaryOutlineComponents[id];
                altColorInfo.Seconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (altColorInfo.Seconds >= altColorInfo.SwitchAtSeconds)
                {
                    OutlineComponent outline = spaceComponents.OutlineComponents[id];
                    Color temp = outline.Color;
                    outline.Color = altColorInfo.AlternateColor;
                    altColorInfo.AlternateColor = temp;
                    altColorInfo.Seconds = 0f;
                    spaceComponents.OutlineComponents[id] = outline;
                }
                spaceComponents.SecondaryOutlineComponents[id] = altColorInfo;
            }
        }

    }
}
