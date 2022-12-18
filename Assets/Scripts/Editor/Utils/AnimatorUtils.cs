using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Editor.Utils
{
    public static class AnimatorUtils
    {
        private const string AssetsDirectoryName = "Assets";

        public static void AssignMotions(AnimatorController controller, string animationClipsPath)
        {
            var animationClipFiles = Directory.GetFiles(animationClipsPath, "*.anim", SearchOption.TopDirectoryOnly);

            var animationClipFileRelativePaths = Array.ConvertAll(animationClipFiles,
                input => input.Replace(Application.dataPath, AssetsDirectoryName));

            InvokeForAnimatorController(controller, (layerName, animatorState) =>
            {
                foreach (var animationClipFileRelativePath in animationClipFileRelativePaths)
                {
                    var animationClipFileName = Path.GetFileNameWithoutExtension(animationClipFileRelativePath);
                    if (animatorState.name.Equals(animationClipFileName))
                    {
                        animatorState.motion = AssetDatabase.LoadAssetAtPath<Motion>(animationClipFileRelativePath);
                    }
                }
            });

            Debug.Log($"[{nameof(AnimatorUtils)}] Assigned motions for {controller.name}");
        }

        public static void CheckMotions(AnimatorController controller)
        {
            var missingMotionAnimatorStateNames = new List<string>();
            var unequalMotionAnimatorStateNames = new List<string>();

            InvokeForAnimatorController(controller, (layerName, animatorState) =>
            {
                if (animatorState.motion == null)
                {
                    missingMotionAnimatorStateNames.Add($"{layerName}_{animatorState.name}");
                }
                else if (!animatorState.name.Equals(animatorState.motion.name))
                {
                    unequalMotionAnimatorStateNames.Add($"{layerName}_{animatorState.name}");
                }
            });

            Debug.Log($"[{nameof(AnimatorUtils)}] Animator states with null motion " +
                      $"({missingMotionAnimatorStateNames.Count}): {string.Join(" ", missingMotionAnimatorStateNames)}");

            Debug.Log($"[{nameof(AnimatorUtils)}] Animator states with unequal motion names" +
                      $"({unequalMotionAnimatorStateNames.Count}): {string.Join(" ", unequalMotionAnimatorStateNames)}");
        }

        private static void InvokeForAnimatorController(
            AnimatorController controller, Action<string, AnimatorState> action = null)
        {
            var layers = controller.layers;
            foreach (var layer in layers)
            {
                InvokeForChildAnimatorStates(layer.name, layer.stateMachine.states, action);

                var subStateMachines = layer.stateMachine.stateMachines;
                foreach (var subStateMachine in subStateMachines)
                {
                    InvokeForChildAnimatorStates(layer.name, subStateMachine.stateMachine.states, action);
                }
            }
        }

        private static void InvokeForChildAnimatorStates(
            string layerName, ChildAnimatorState[] childAnimatorStates, Action<string, AnimatorState> action = null)
        {
            foreach (var childAnimatorState in childAnimatorStates)
            {
                var animatorState = childAnimatorState.state;
                action?.Invoke(layerName, animatorState);
            }
        }

        public static void ExtractEmbeddedAnimationClips(AnimatorController controller, string animationClipsPath)
        {
            var clips = GetEmbeddedAnimationClips(controller);
            foreach (var clip in clips)
            {
                AssetDatabase.RemoveObjectFromAsset(clip);
                AssetDatabase.CreateAsset(clip, $"{animationClipsPath}/{clip.name}.anim");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[{nameof(AnimatorUtils)}] Extracted {clips.Count} animations for {controller.name}");
        }

        public static void DeleteEmbeddedAnimationClips(AnimatorController controller)
        {
            var clips = GetEmbeddedAnimationClips(controller);
            foreach (var clip in clips)
            {
                AssetDatabase.RemoveObjectFromAsset(clip);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[{nameof(AnimatorUtils)}] Deleted {clips.Count} " +
                      $" embedded animations for {controller.name}");
        }

        public static void CheckAnimationClipDurations(SkeletonDataAsset skeletonDataAsset, List<AnimationClip> clips)
        {
            foreach (var clip in clips)
            {
                var spineClip = skeletonDataAsset.GetSkeletonData(false).Animations
                    .Find(a => a.Name.Equals(clip.name));

                Assert.IsNotNull(spineClip, $"Not find spine Animations by name {clip.name}");

                var result = Math.Abs(clip.length - spineClip.Duration) > 0.05f ? "NOT" : "";

                Debug.Log($"[{nameof(AnimatorUtils)}] AnimationClip {clip.name} is {result} equal by duration: " +
                          $"Clip - {clip.length}, Spine - {spineClip.Duration}");
            }
        }

        public static List<AnimationClip> GetEmbeddedAnimationClips(AnimatorController controller)
        {
            var path = AssetDatabase.GetAssetPath(controller);
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().ToList();
        }
    }
}