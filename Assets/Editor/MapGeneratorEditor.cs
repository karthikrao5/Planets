using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MapGenerator generator = (MapGenerator) target;

            if (DrawDefaultInspector())
            {
                if (generator.autoUpdate )
                {
                    generator.DrawMapInEditor();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                generator.DrawMapInEditor();
            }
        }
    }
}