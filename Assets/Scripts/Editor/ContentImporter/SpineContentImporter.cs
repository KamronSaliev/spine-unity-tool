#if USE_SPINE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Utils;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Editor.ContentImporter
{
    public class SpineContentImporter : IContentImporter
    {
        private PathSettings _settings;
        private ContentImporterParams _importerParams;

        private bool _isReimport;
        private SkeletonDataAsset _skeletonDataAsset;
        private AnimatorController _newController;
        private AnimatorController _oldController;
        private GameObject _tempSpineObject;
        private string _newControllerName;
        private string _newControllerPath;
        private string _oldControllerPath;

        private const int DefaultLayerIndex = 0;
        private const string OldControllerPostfix = "_old";
        private const string DefaultClipName = "DefaultClip";

        public void ImportContent(PathSettings settings, string path, ContentImporterParams importerParams,
            Action<string> doneCallback)
        {
            _settings = settings;
            _importerParams = importerParams;

            var spineDirPath = path.Split(';')[0];
            var contextId = path.Split(';')[1];
            var group = path.Split(';')[2];
            var spineAnimDirName = new DirectoryInfo(spineDirPath).Name;
            var spineDir = string.IsNullOrEmpty(_importerParams?.AnimationImportFolder)
                ? Path.Combine(_settings.AnimationContentPath, contextId, group, spineAnimDirName)
                : _importerParams.AnimationImportFolder;

            Debug.Log($"[{nameof(SpineContentImporter)}] From {spineDirPath} to {spineDir}");

            CopySpineContent(spineDirPath, spineDir);
            CreateSkeletonMecanim(spineDir);
            ImportAnimationClips(spineDir);

            if (_importerParams.CreatePrefabs)
            {
                var prefabsDir = string.IsNullOrEmpty(_importerParams.PrefabCreateFolder)
                    ? Path.Combine(_settings.PrefabsContentPath, contextId, group)
                    : _importerParams.PrefabCreateFolder;
                CreatePrefab(_tempSpineObject, spineAnimDirName, prefabsDir);
            }
            else
            {
                Object.DestroyImmediate(_tempSpineObject);
            }

            doneCallback(string.Empty);
        }

        private void CopySpineContent(string spineDirPath, string contentDir)
        {
            if (!Directory.Exists(contentDir))
            {
                Directory.CreateDirectory(contentDir);
            }

            Utils.FileUtils.CopyFiles("*", spineDirPath, contentDir, true);
            AssetDatabase.Refresh
                ();
        }

        private void CreateSkeletonMecanim(string spineAnimDir)
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(SkeletonDataAsset)}", new[] { spineAnimDir });
            var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);

            _skeletonDataAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetPath);

            // Nullifies the object reference in order to manipulate with the controller
            _skeletonDataAsset.controller = null;

            var animControllerAssets =
                AssetDatabase.FindAssets($"t:{typeof(AnimatorController)}", new[] { spineAnimDir });
            Assert.IsFalse(animControllerAssets.Length > 1, "Multiple AnimatorControllers were found");

            _isReimport = animControllerAssets.Length != 0;

            if (_isReimport)
            {
                var path = AssetDatabase.GUIDToAssetPath(animControllerAssets[0]);
                _oldController = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

                AssetDatabase.RenameAsset(path, $"{_oldController.name}{OldControllerPostfix}");
                _oldControllerPath = AssetDatabase.GetAssetPath(_oldController);
            }

            // Creates animator controller, assigns to runtime controller of imported skeletonDataAsset
            _tempSpineObject = EditorInstantiation.InstantiateSkeletonMecanim(_skeletonDataAsset).gameObject;

            _newController = (AnimatorController)_skeletonDataAsset.controller;
            
            Assert.IsNotNull(_newController, "SkeletonDataAsset does not have RuntimeController");

            // ReSharper disable once PossibleNullReferenceException
            _newControllerName = _newController.name;
            _newControllerPath = AssetDatabase.GetAssetPath(_newController);
        }

        private void ImportAnimationClips(string spineAnimDir)
        {
            var newControllerClips = AnimatorUtils.GetEmbeddedAnimationClips(_newController);
            var oldControllerClips = new List<AnimationClip>();

            if (_isReimport)
            {
                var oldControllerClipFiles = Directory.GetFiles(spineAnimDir, "*.anim", SearchOption.TopDirectoryOnly);
                oldControllerClips =
                    Array.ConvertAll(oldControllerClipFiles, AssetDatabase.LoadAssetAtPath<AnimationClip>).ToList();

                CopyAnimationClipSettings(oldControllerClips, newControllerClips);
            }

            if (_importerParams.CheckAnimationClips)
            {
                AnimatorUtils.CheckAnimationClipDurations(_skeletonDataAsset, newControllerClips);
            }

            if (_importerParams.AddNewAnimationClips)
            {
                var controller = _isReimport ? _oldController : _newController;
                AddNewAnimationClipsToAnimator(controller, oldControllerClips, newControllerClips);
            }

            if (_isReimport)
            {
                foreach (var oldClip in oldControllerClips)
                {
                    if (oldClip.name.Contains(DefaultClipName))
                    {
                        continue;
                    }

                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(oldClip));
                }
            }

            AnimatorUtils.ExtractEmbeddedAnimationClips(_newController, spineAnimDir);

            if (_isReimport)
            {
                AssetDatabase.DeleteAsset(_newControllerPath);
                AssetDatabase.RenameAsset(_oldControllerPath, _newControllerName);

                AnimatorUtils.DeleteEmbeddedAnimationClips(_oldController);
                AnimatorUtils.AssignMotions(_oldController, spineAnimDir);
                AnimatorUtils.CheckMotions(_oldController);

                _skeletonDataAsset.controller = _oldController;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void AddNewAnimationClipsToAnimator(AnimatorController controller,
            List<AnimationClip> oldControllerClips, List<AnimationClip> newControllerClips)
        {
            var newClips = new List<AnimationClip>();
            foreach (var newControllerClip in newControllerClips)
            {
                var isNew = !oldControllerClips.Any(clip => newControllerClip.name.Equals(clip.name));
                if (!isNew)
                {
                    continue;
                }

                controller.AddMotion(newControllerClip, DefaultLayerIndex);
                newClips.Add(newControllerClip);
            }

            Debug.Log($"[{nameof(SpineContentImporter)}] Added new clips: {string.Join(" ", newClips)}");
        }

        private void CopyAnimationClipSettings(List<AnimationClip> oldControllerClips,
            List<AnimationClip> newControllerClips)
        {
            foreach (var oldControllerClip in oldControllerClips)
            {
                var oldAnimationEvents = AnimationUtility.GetAnimationEvents(oldControllerClip);
                var oldAnimationClipSettings = AnimationUtility.GetAnimationClipSettings(oldControllerClip);

                foreach (var newControllerClip in newControllerClips)
                {
                    if (!oldControllerClip.name.Equals(newControllerClip.name))
                    {
                        continue;
                    }

                    AnimationUtility.SetAnimationEvents(newControllerClip, oldAnimationEvents);

                    var newAnimationClipSettings = AnimationUtility.GetAnimationClipSettings(newControllerClip);
                    newAnimationClipSettings.loopTime = oldAnimationClipSettings.loopTime;

                    AnimationUtility.SetAnimationClipSettings(newControllerClip, newAnimationClipSettings);
                }
            }
        }

        private void CreatePrefab(GameObject spineAnimObject, string prefabName, string prefabsDir)
        {
            if (!Directory.Exists(prefabsDir))
            {
                Directory.CreateDirectory(prefabsDir);
            }

            var meshRenderer = spineAnimObject.GetComponent<MeshRenderer>();
            meshRenderer.receiveShadows = false;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

            var fullPrefabPath = Path.Combine(prefabsDir, prefabName + ".prefab");

            if (File.Exists(fullPrefabPath))
            {
                File.Delete(fullPrefabPath);
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(spineAnimObject, fullPrefabPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(spineAnimObject);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

#endif