#if UNITY_EDITOR
using CMPM146.MapGenerator;
using UnityEditor;
using UnityEngine;


namespace CMPM146.Editor {
    [CustomEditor(typeof(RoomArchetype))]
    public class RoomEditor : UnityEditor.Editor {
        SerializedProperty _roomsProp;
        SerializedProperty _widthProp;
        SerializedProperty _heightProp;
        SerializedProperty _occupancyProp;

        void OnEnable() {
            _roomsProp     = serializedObject.FindProperty("rooms");
            _widthProp     = serializedObject.FindProperty("Width");
            _heightProp    = serializedObject.FindProperty("Height");
            _occupancyProp = serializedObject.FindProperty("Occupancy");
        }
       
        public override void OnInspectorGUI() {
            RoomArchetype archetype = (RoomArchetype)target;
            serializedObject.Update();

            EditorGUILayout.LabelField("Room Relations", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_roomsProp, true);
            
            EditorGUILayout.LabelField("Room Grid", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_widthProp);
            EditorGUILayout.PropertyField(_heightProp);
            
            if (archetype.Width * archetype.Height != archetype.Occupancy.Length) {
                Undo.RecordObject(archetype, "Resize Room Grid");
                archetype.ResizeGrid(archetype.Width, archetype.Height);
                //EditorUtility.SetDirty(room);
            }
            DrawOccupancyGrid(archetype);

            serializedObject.ApplyModifiedProperties();
        }

        static void DrawOccupancyGrid(RoomArchetype archetype) {
            int    width     = archetype.Width;
            int    height    = archetype.Height;
            bool[] occupancy = archetype.Occupancy;
            
            EditorGUILayout.LabelField("Occupancy Grid:");
            for (int y = 0; y < height; y++) {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < width; x++) {
                    int index = y * width + x;
                    occupancy[index] = EditorGUILayout.Toggle(occupancy[index], GUILayout.Width(20));
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorUtility.SetDirty(archetype);
        }
    }
}
#endif
