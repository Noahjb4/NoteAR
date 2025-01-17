using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.MARS
{
    // Derived from Decorators in PostProcessing

    /// <summary>
    /// Used to draw a 'RangeAttribute' Decorator.
    /// </summary>
    [Decorator(typeof(RangeAttribute))]
    public sealed class RangeDecorator : AttributeDecorator
    {
        public override bool OnGUI(SerializedProperty property, GUIContent label, Attribute attribute)
        {
            var attr = (RangeAttribute)attribute;
            Assert.IsTrue(property.propertyType == SerializedPropertyType.Float || property.propertyType == SerializedPropertyType.Integer);
            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = EditorGUILayout.Slider(label, property.floatValue, attr.min, attr.max);
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUILayout.IntSlider(label, property.intValue, (int)attr.min, (int)attr.max);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Used to draw a 'MinAttribute' Decorator.
    /// </summary>
    [Decorator(typeof(MinAttribute))]
    public sealed class MinDecorator : AttributeDecorator
    {
        public override bool OnGUI(SerializedProperty property, GUIContent label, Attribute attribute)
        {
            var attr = (MinAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                float v = EditorGUILayout.DelayedFloatField(label, property.floatValue);
                property.floatValue = Mathf.Max(v, attr.min);
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int v = EditorGUILayout.DelayedIntField(label, property.intValue);
                property.intValue = Mathf.Max(v, (int)attr.min);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Used to draw a 'MaxDecorator' Decorator.
    /// </summary>
    [Decorator(typeof(MaxAttribute))]
    public sealed class MaxDecorator : AttributeDecorator
    {
        public override bool OnGUI(SerializedProperty property, GUIContent label, Attribute attribute)
        {
            var attr = (MaxAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                float v = EditorGUILayout.DelayedFloatField(label, property.floatValue);
                property.floatValue = Mathf.Min(v, attr.max);
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int v = EditorGUILayout.DelayedIntField(label, property.intValue);
                property.intValue = Mathf.Min(v, (int)attr.max);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Used to draw a 'MinMaxAttribute' Decorator.
    /// </summary>
    [Decorator(typeof(MinMaxAttribute))]
    public sealed class MinMaxDecorator : AttributeDecorator
    {
        public override bool OnGUI(SerializedProperty property, GUIContent label, Attribute attribute)
        {
            var attr = (MinMaxAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                float v = EditorGUILayout.DelayedFloatField(label, property.floatValue);
                property.floatValue = Mathf.Clamp(v, attr.min, attr.max);
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int v = EditorGUILayout.DelayedIntField(label, property.intValue);
                property.intValue = Mathf.Clamp(v, (int)attr.min, (int)attr.max);
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                var v = property.vector2Value;
                EditorGUILayout.MinMaxSlider(label, ref v.x, ref v.y, attr.min, attr.max);
                property.vector2Value = v;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Used to draw a 'ColorUsageAttribute' Decorator.
    /// </summary>
    [Decorator(typeof(ColorUsageAttribute))]
    public sealed class ColorUsageDecorator : AttributeDecorator
    {
        public override bool OnGUI(SerializedProperty property, GUIContent label, Attribute attribute)
        {
            var attr = (ColorUsageAttribute)attribute;

            if (property.propertyType != SerializedPropertyType.Color)
                return false;

#if UNITY_2018_1_OR_NEWER
            property.colorValue = EditorGUILayout.ColorField(label, property.colorValue, true, attr.showAlpha, attr.hdr);
#else
            ColorPickerHDRConfig hdrConfig = null;

            if (attr.hdr)
            {
                hdrConfig = new ColorPickerHDRConfig(
                    attr.minBrightness,
                    attr.maxBrightness,
                    attr.minExposureValue,
                    attr.maxExposureValue
                );
            }

            property.colorValue = EditorGUILayout.ColorField(label, property.colorValue, true, attr.showAlpha, attr.hdr, hdrConfig);
#endif

            return true;
        }
    }
    
    /// <summary>
    /// Used to draw a 'DelayedAttribute' Decorator.
    /// </summary>
    [Decorator(typeof(DelayedAttribute))]
    public sealed class DelayedDecorator : AttributeDecorator
    {
        public override bool OnGUI(SerializedProperty property, GUIContent label, Attribute attribute)
        {
            Assert.IsTrue(property.propertyType == SerializedPropertyType.Float
                || property.propertyType == SerializedPropertyType.Integer
                || property.propertyType == SerializedPropertyType.String);

            switch (property.propertyType) {
                case SerializedPropertyType.Float:
                    property.floatValue = EditorGUILayout.DelayedFloatField(label, property.floatValue);
                    return true;
                case SerializedPropertyType.Integer:
                    property.intValue = EditorGUILayout.DelayedIntField(label, property.intValue);
                    return true;
                case SerializedPropertyType.String:
                    property.stringValue = EditorGUILayout.DelayedTextField(label, property.stringValue);
                    return true;
                default:
                    return false;
            }
        }
    }

}
