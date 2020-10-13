using System.Collections.Generic;

namespace Unity.MARS
{
    /// <summary>
    /// Represents all traits required by the Conditions and Actions on a single Proxy
    /// </summary>
    public class ProxyTraitRequirements : HashSet<TraitRequirement>
    {
        public ProxyTraitRequirements(IEnumerable<TraitRequirement> requirements)
        {
            foreach (var r in requirements)
            {
                Add(r);
            }
        }
    }
}
