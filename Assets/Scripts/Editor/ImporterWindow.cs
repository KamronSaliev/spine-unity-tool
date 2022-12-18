using Editor.ContentImporter;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ImporterWindow : EditorWindow
    {
        private string AnimationLastSelectedFolder
        {
            get => EditorPrefs.GetString(nameof(AnimationLastSelectedFolder), "");
            set => EditorPrefs.SetString(nameof(AnimationLastSelectedFolder), value);
        }

        private string PrefabLastSelectedFolder
        {
            get => EditorPrefs.GetString(nameof(PrefabLastSelectedFolder), "");
            set => EditorPrefs.SetString(nameof(PrefabLastSelectedFolder), value);
        }

        private string _selectedPath;
        private ContentImporter.ContentImporter _contentImporter;
        private ImportResultType _result;

        private DefaultAsset _targetAnimationFolder;
        private DefaultAsset _targetPrefabFolder;

        private bool _addNewAnimationClipsToAnimator;
        private bool _createPrefabsForSpine;

        private readonly GUILayoutOption _elementMaxWidth = GUILayout.Width(300);

        [MenuItem("Tools/Spine Content Importer")]
        private static void Init()
        {
            var window = (ImporterWindow)GetWindow(typeof(ImporterWindow));
            window.titleContent = new GUIContent("Content Importer");
            window.minSize = new Vector2(350, 500);
            window.Show();
        }

        private void OnEnable()
        {
            _contentImporter = new ContentImporter.ContentImporter();
            _contentImporter.ImportDone += ContentImporterOnImportDone;
        }

        private void OnDisable()
        {
            _contentImporter.ImportDone -= ContentImporterOnImportDone;
        }

        private void ContentImporterOnImportDone(ImportResultType result)
        {
            _result = result;
        }

        private void OnGUI()
        {
            GUILayout.Label("Select Spine directory");

            if (GUILayout.Button("Select", _elementMaxWidth))
            {
                _selectedPath = EditorUtility.OpenFolderPanel("Select Spine directory", Application.dataPath, "");
            }

            if (!string.IsNullOrEmpty(_selectedPath))
            {
                GUILayout.Label($"Spine assets source path: {_selectedPath}");

                EditorGUILayout.Space();

                AnimationLastSelectedFolder = SelectFolder(ref _targetAnimationFolder, AnimationLastSelectedFolder,
                    "Animation import folder");

                _addNewAnimationClipsToAnimator = GUILayout.Toggle(_addNewAnimationClipsToAnimator,
                    "Add new animation clips to Animator");

                if (_addNewAnimationClipsToAnimator)
                {
                    EditorGUILayout.HelpBox("New Animation will be added to the Animator", MessageType.Info);
                }

                _createPrefabsForSpine = GUILayout.Toggle(_createPrefabsForSpine, "Create prefabs");

                if (_createPrefabsForSpine)
                {
                    PrefabLastSelectedFolder = SelectFolder(ref _targetPrefabFolder, PrefabLastSelectedFolder,
                        "Prefab create folder");
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Import", _elementMaxWidth))
                {
                    var importerParams = new ContentImporterParams
                    {
                        AddNewAnimationClips = _addNewAnimationClipsToAnimator,
                        CreatePrefabs = _createPrefabsForSpine,
                        AnimationImportFolder = AnimationLastSelectedFolder,
                        PrefabCreateFolder = PrefabLastSelectedFolder
                    };

                    _contentImporter.ImportContent<SpineContentImporter>(
                        _selectedPath + ";Anim;SpineAnimation", importerParams);
                }
            }

            EditorGUILayout.Space();

            OnImportDone();
        }

        private void OnImportDone()
        {
            switch (_result)
            {
                case ImportResultType.None:
                {
                    break;
                }
                case ImportResultType.InProcess:
                {
                    GUILayout.Label("In progress");
                    break;
                }
                case ImportResultType.Error:
                {
                    GUILayout.Label("Failed");
                    break;
                }
                default:
                {
                    GUILayout.Label("Success");
                    break;
                }
            }
        }

        private string SelectFolder(ref DefaultAsset folder, string pathFolder, string label)
        {
            if (!string.IsNullOrEmpty(pathFolder))
            {
                folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(pathFolder);
            }

            folder = (DefaultAsset)EditorGUILayout.ObjectField(label, folder, typeof(DefaultAsset), false);

            return _targetAnimationFolder != null ? AssetDatabase.GetAssetPath(folder) : "";
        }
    }
}