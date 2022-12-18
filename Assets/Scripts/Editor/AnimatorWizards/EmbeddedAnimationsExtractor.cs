using System.IO;
using Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor.AnimatorWizards
{
    public class EmbeddedAnimationsExtractor : ScriptableWizard
    {
        [SerializeField] private AnimatorController _controller;

        [MenuItem("Tools/EmbeddedAnimationsExtractor", false, 100)]
        private static void CreateWizard()
        {
            DisplayWizard($"{nameof(EmbeddedAnimationsExtractor)}",
                typeof(EmbeddedAnimationsExtractor),
                "Extract");
        }

        private void OnWizardUpdate()
        {
            helpString =
                $"{nameof(EmbeddedAnimationsExtractor)} extracts embedded animation clips from animator controller, " +
                "creates separate files for further manipulations";
        }

        private void OnWizardCreate()
        {
            var currentDirectoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_controller));
            AnimatorUtils.ExtractEmbeddedAnimationClips(_controller, currentDirectoryPath);
        }
    }
}