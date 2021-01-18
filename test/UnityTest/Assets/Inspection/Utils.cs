using System;
using Box2DSharp.Common;
using UnityEngine;
using Color = UnityEngine.Color;
using SVector2 = System.Numerics.Vector2;
using SVector3 = System.Numerics.Vector3;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;

namespace Box2DSharp.Testbed.Unity.Inspection
{
    public static class Utils
    {
        #region From FVector

        public static UVector2 ToUVector2(this FVector2 vector2)
        {
            return new UVector2(vector2.X.AsFloat, vector2.Y.AsFloat);
        }

        #endregion

        #region From System Vector

        public static UVector2 ToUVector2(in this SVector2 vector2)
        {
            return new UVector2(vector2.X, vector2.Y);
        }

        #endregion

        #region From Unity Vector

        public static UVector3 ToUVector3(in this UVector2 vector2)
        {
            return new UVector3(vector2.x, vector2.y, 0);
        }

        public static FVector2 ToFVector2(in this UVector3 vector3)
        {
            return new FVector2(vector3.x, vector3.y);
        }

        #endregion

        public static Color ToUnityColor(this Box2DSharp.Common.Color color)
        {
            return new Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ShowOnlyAttribute : PropertyAttribute
    { }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ShowVectorAttribute : PropertyAttribute
    { }
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyAttributeDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect rect, UnityEditor.SerializedProperty prop, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(rect, prop);
            GUI.enabled = wasEnabled;
        }
    }

    [UnityEditor.CustomPropertyDrawer(typeof(ShowVectorAttribute))]
    public class ShowVectorAttributeDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect rect, UnityEditor.SerializedProperty prop, GUIContent label)
        {
            UnityEditor.EditorGUI.PropertyField(rect, prop);
        }
    }
#endif
}