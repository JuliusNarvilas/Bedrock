using UnityEditor;
using UnityEngine;


namespace Common.Text
{
    [CustomPropertyDrawer(typeof(IntelligentTextId))]
    public class IntelligentTextIdDrawer : PropertyDrawer
    {
        private Rect m_ContentArea;

        // Draw the property inside the given rect
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            m_ContentArea = pos;
            EditorGUI.BeginProperty(m_ContentArea, label, prop);
            m_ContentArea.height = EditorGUIExtensions.DEFAULT_GUI_HEIGHT;

            SerializedProperty idProperty = prop.FindPropertyRelative("Name");
            if (idProperty != null)
            {
                string newId = EditorGUI.TextField(m_ContentArea, "Id", idProperty.stringValue);
                idProperty.stringValue = newId;
                SerializedProperty hashProperty = prop.FindPropertyRelative("Hash");
                if (hashProperty != null)
                {
                    hashProperty.intValue = string.IsNullOrEmpty(newId) ? 0 : newId.GetHashCode();
                }
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return EditorGUIExtensions.DEFAULT_GUI_LINE_HEIGHT;
        }
    }
}
