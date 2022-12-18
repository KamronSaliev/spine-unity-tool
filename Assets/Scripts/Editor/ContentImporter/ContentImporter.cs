using System;
using Editor.Utils;
using UnityEngine;

namespace Editor.ContentImporter
{
    public class ContentImporter
    {
        public event Action<ImportResultType> ImportDone = delegate { };

        private readonly PathSettings _settings;
        private string _path;
        private ContentImporterParams _importerParams;

        public ContentImporter()
        {
            _settings = PathSettings.GetOrCreateSettings();
        }

        public void ImportContent<T>(string path, ContentImporterParams importerParams)
        {
            _path = path;
            _importerParams = importerParams;
            
            ApplyContentImporters<T>(_settings);
        }

        private void ApplyContentImporters<T>(PathSettings settings)
        {
            var importerInstance = (IContentImporter)Activator.CreateInstance(typeof(T));
            importerInstance.ImportContent(settings, _path, _importerParams, ContentImportComplete);
        }

        private void ContentImportComplete(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[{nameof(ContentImporter)}] Fail: " + error);
                
                ImportDone(ImportResultType.Error);
                return;
            }

            ImportDone(ImportResultType.Done);
        }
    }
}