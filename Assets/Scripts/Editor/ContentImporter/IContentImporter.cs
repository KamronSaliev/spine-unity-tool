using System;
using Editor.Utils;

namespace Editor.ContentImporter
{
    public interface IContentImporter
    {
        void ImportContent(PathSettings settings, string path, ContentImporterParams importerParams, Action<string> doneCallback);
    }
}