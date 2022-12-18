using System.IO;
using Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor.AnimatorWizards
{
    public class AnimatorStateMotionAssigner : ScriptableWizard
    {
        [SerializeField] private AnimatorController _controller;

        [SerializeField] private string _selectedPath;

        [MenuItem("Tools/AnimatorStateMotionAssigner", false, 101)]
        private static void CreateWizard()
        {
            DisplayWizard($"{nameof(AnimatorStateMotionAssigner)}",
                typeof(AnimatorStateMotionAssigner),
                "Assign motions", "Select directory");
        }

        private void OnWizardUpdate()
        {
            helpString = $"{nameof(AnimatorStateMotionAssigner)} analyzes animator controller " +
                         "and assigns animation clips to the states with no clip";
        }

        private void OnWizardOtherButton()
        {
            var path = _controller != null
                ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(_controller))
                : Application.dataPath;

            _selectedPath = EditorUtility.OpenFolderPanel("Select animations directory", path, "");
        }

        private void OnWizardCreate()
        {
            AnimatorUtils.AssignMotions(_controller, _selectedPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}