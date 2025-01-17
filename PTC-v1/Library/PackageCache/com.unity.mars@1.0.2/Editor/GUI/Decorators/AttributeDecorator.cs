using System;
using UnityEditor;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Base class that wraps a property drawer with attribute defined decorator.
    /// </summary>
    // Derived from AttributeDecorator from PostProcessing
    public abstract class AttributeDecorator
    {
        /// <summary>
        /// Called to draw the decorator.
        /// </summary>
        /// <param name="property">Property to be drawn.</param>
        /// <param name="label">Label of the property</param>
        /// <param name="attribute">Attributes being applied to decorator.</param>
        /// <returns>Returns 'true' if decorator was applied.</returns>
        public abstract bool OnGUI(SerializedProperty property, GUIContent label, Attribute attribute);
    }
}
