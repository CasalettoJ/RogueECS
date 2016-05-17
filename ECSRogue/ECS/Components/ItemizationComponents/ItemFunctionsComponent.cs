using ECSRogue.BaseEngine;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.ItemizationComponents
{
    public struct ItemFunctionsComponent
    {
        public int CostToUse;
        public int Uses;
        public bool Ranged;
        public ItemUseFunctions UseFunctionValue; 
    }
}
