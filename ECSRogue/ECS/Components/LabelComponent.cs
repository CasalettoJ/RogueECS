﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct LabelComponent
    {
        public string Text;
        public Color Color;
        public float Scale;
        public Vector2 Origin;
        public SpriteEffects SpriteEffect;
        public float Rotation;
    }
}
