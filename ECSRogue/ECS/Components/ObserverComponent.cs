using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct ObserverComponent
    {
        public Guid SelectedItem;
        public List<Guid> Observed;
    }
}
