using System.Collections.Generic;
using Editor.Utils;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace Editor.AnimatorWizards
{
    public class AnimationClipDurationChecker : ScriptableWizard
    {
        [SerializeField] private SkeletonDataAsset _skeletonDataAsset;

        [SerializeField] private List<AnimationClip> _animationClips;

        [MenuItem("Tools/AnimationClipDurationChecker", false, 103)]
        private static void CreateWizard()
        {
            DisplayWizard($"{nameof(AnimationClipDurationChecker)}",
                typeof(AnimationClipDurationChecker),
                "Check clip durations");
        }

        private void OnWizardUpdate()
        {
            helpString =
                $"{nameof(AnimationClipDurationChecker)} compares and logs durations of animation clips " +
                "and related Spine animation from SkeletonDataAsset";
        }

        private void OnWizardCreate()
        {
            AnimatorUtils.CheckAnimationClipDurations(_skeletonDataAsset, _animationClips);
        }
    }
}