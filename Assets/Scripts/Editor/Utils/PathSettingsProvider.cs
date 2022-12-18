using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Utils
{
    public class PathSettingsProvider : SettingsProvider
    {
        private PathSettings _settings;

        public PathSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            if (!IsSettingsAvailable())
            {
                return null;
            }

            var provider = new PathSettingsProvider("Project/Spine Content Importer", SettingsScope.Project)
            {
                keywords = new[] { "Spine Content Importer" }
            };

            return provider;
        }

        private static bool IsSettingsAvailable()
        {
            return File.Exists(PathSettings.SettingsPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = PathSettings.GetOrCreateSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.LabelField("Paths:");

            EditorGUILayout.LabelField("Root Dir: ", _settings.ContentData);
            EditorGUILayout.LabelField("Animation Dir: ", _settings.AnimationContent);
            EditorGUILayout.LabelField("Prefabs Dir: ", _settings.PrefabsContent);

            if (GUILayout.Button("Create Hierarchy"))
            {
                _settings.CreateHierarchy();
            }
        }
    }
}