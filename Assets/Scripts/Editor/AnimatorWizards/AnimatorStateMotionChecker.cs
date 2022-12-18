using Editor.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor.AnimatorWizards
{
    public class AnimatorStateMotionChecker : ScriptableWizard
    {
        [SerializeField] private AnimatorController _controller;

        [MenuItem("Tools/AnimatorStateMotionChecker", false, 102)]
        private static void CreateWizard()
        {
            DisplayWizard($"{nameof(AnimatorStateMotionChecker)}",
                typeof(AnimatorStateMotionChecker),
                "Check motions");
        }

        private void OnWizardUpdate()
        {
            helpString = $"{nameof(AnimatorStateMotionChecker)} analyzes animator controller " +
                         "and logs all states with no clips or different animation clips in name";
        }

        private void OnWizardCreate()
        {
            AnimatorUtils.CheckMotions(_controller);
        }
    }
}