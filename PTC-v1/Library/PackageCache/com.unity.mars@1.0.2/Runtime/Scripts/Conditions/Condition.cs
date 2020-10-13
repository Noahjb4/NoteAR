using System;
using Unity.MARS.Data;
using Unity.MARS.Query;
using UnityEngine;

namespace Unity.MARS
{
    public abstract class Condition : ConditionBase, ICondition
    {
        public string traitName
        {
            get
            {
                var traits = GetRequiredTraits();
                if (traits != null && traits.Length > 0)
                    return traits[0].TraitName;

                const string requiredTraitsFieldName = IRequiresTraitsMethods.StaticRequiredTraitsFieldName;
                Debug.LogError(
                    $"{GetType().Name}.GetRequiredTraits() must return at least one TraitRequirement. The first one defines " +
                     "the trait this Condition tests against, and any after that define an additional requirement for the simple presence of a trait." +
                    $" Your implementation of GetRequiredTraits() must return the field {requiredTraitsFieldName}.");

                return "";
            }
        }

        public abstract TraitRequirement[] GetRequiredTraits();
    }

    public abstract class Condition<T> : Condition, ICondition<T>
    {
        public abstract float RateDataMatch(ref T data);
    }

    /// <summary>
    /// Base class for conditions that interact with two traits at once
    /// </summary>
    /// <typeparam name="T1">The type of the first trait</typeparam>
    /// <typeparam name="T2">The type of the second trait</typeparam>
    public abstract class Condition<T1, T2> : ConditionBase, ICondition<T1, T2>
        where T1: struct
        where T2: struct
    {
        public string TraitName1 { get; }
        public string TraitName2 { get; }

        public abstract float RateDataMatch(ref T1 trait1, ref T2 trait2);
    }

    /// <summary>
    /// Base class for conditions that interact with three traits at once
    /// </summary>
    /// <typeparam name="T1">The type of the first trait</typeparam>
    /// <typeparam name="T2">The type of the second trait</typeparam>
    /// <typeparam name="T3">The type of the third trait</typeparam>
    public abstract class Condition<T1, T2, T3> : ConditionBase, ICondition<T1, T2, T3>
        where T1: struct
        where T2: struct
        where T3: struct
    {
        public string TraitName1 { get; }
        public string TraitName2 { get; }
        public string TraitName3 { get; }

        public abstract float RateDataMatch(ref T1 trait1, ref T2 trait2, ref T3 trait3);
    }
}
