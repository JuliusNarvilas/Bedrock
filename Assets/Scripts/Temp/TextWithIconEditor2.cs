using UI.Sources;
using UnityEditor;
using UnityEngine;
/*
using TextEditor = UnityEditor.UI.TextEditor;

[CustomEditor(typeof(TextWithIcon2))]
public class TextWithIconEditor2 : UnityEditor.UI. TextEditor
{

    private SerializedProperty ImageScalingFactorProp;
    private SerializedProperty iconList;

    protected override void OnEnable()
    {
        base.OnEnable();
        ImageScalingFactorProp = serializedObject.FindProperty("ImageScalingFactor");
        iconList = serializedObject.FindProperty("inspectorIconList");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(ImageScalingFactorProp, new GUIContent("Image Scaling Factor"));
        EditorGUILayout.PropertyField(iconList, new GUIContent("Icon List"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
*/