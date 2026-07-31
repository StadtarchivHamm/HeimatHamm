using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ImprovedToggle))]
public class ImprovedToggleEditor : ToggleEditor 
{
    SerializedProperty _toggleImageProp;
    SerializedProperty _offSpriteProp;
    SerializedProperty _onSpriteProp;
    SerializedProperty _offColorProp;
    SerializedProperty _onColorProp;
    SerializedProperty _onToggleOnProp;
    SerializedProperty _onToggleOffProp;


    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(_toggleImageProp, new GUIContent("Toggle Image"));
        EditorGUILayout.PropertyField(_offSpriteProp, new GUIContent("Off Sprite"));
        EditorGUILayout.PropertyField(_onSpriteProp, new GUIContent("On Sprite"));
        EditorGUILayout.PropertyField(_offColorProp, new GUIContent("Off Color"));
        EditorGUILayout.PropertyField(_onColorProp, new GUIContent("On Color"));
        EditorGUILayout.PropertyField(_onToggleOnProp, new GUIContent("On Toggled On"));
        EditorGUILayout.PropertyField(_onToggleOffProp, new GUIContent("On Toggled Off"));
        serializedObject.ApplyModifiedProperties();
    }

    protected override void OnEnable() 
    {
        base.OnEnable();
        _toggleImageProp = serializedObject.FindProperty("_toggleImage");

        _offSpriteProp = serializedObject.FindProperty("_offSprite");
        _onSpriteProp = serializedObject.FindProperty("_onSprite");

        _offColorProp = serializedObject.FindProperty("_offColor");
        _onColorProp = serializedObject.FindProperty("_onColor");

        _onToggleOnProp = serializedObject.FindProperty("_onToggleOn");
        _onToggleOffProp = serializedObject.FindProperty("_onToggleOff");
    }
}