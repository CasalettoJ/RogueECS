using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.AIComponents
{
    public enum AIAlignments
    {
        ALIGNMENT_NONE,
        ALIGNMENT_FRIENDLY,
        ALIGNMENT_HOSTILE
    }

    public struct AIAlignment
    {
        public AIAlignments Alignment;
    }
}
