using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Utils
{
    [Serializable]
    public class PathSettings : ScriptableObject
    {
        public static readonly string SettingsPath = "Assets/Editor/PathSettings.asset";

        [SerializeField] private string _contentData = "Game Content";
        public string ContentData => _contentData;
        public string ContentDataPath => Path.Combine("Assets", _contentData);

        [SerializeField] private string _prefabsContent = "Prefabs";
        public string PrefabsContent => _prefabsContent;
        public string PrefabsContentPath => Path.Combine(ContentDataPath, _prefabsContent);

        [SerializeField] private string _animationContent = "Animations";
        public string AnimationContent => _animationContent;
        public string AnimationContentPath => Path.Combine(ContentDataPath, _animationContent);

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        public static PathSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PathSettings>(SettingsPath);

            if (settings != null)
            {
                return settings;
            }

            if (!Directory.Exists("Assets/Editor/"))
            {
                Directory.CreateDirectory("Assets/Editor/");
            }

            settings = CreateInstance<PathSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();

            return settings;
        }

        public void CreateHierarchy()
        {
            CreateHierarchyDir(Path.Combine("Assets", _contentData));
            CreateHierarchyDir(Path.Combine(ContentDataPath, _prefabsContent));
            CreateHierarchyDir(Path.Combine(ContentDataPath, _animationContent));

            AssetDatabase.Refresh();
        }

        private void CreateHierarchyDir(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                return;
            }

            Directory.CreateDirectory(dirPath);
        }
    }
}