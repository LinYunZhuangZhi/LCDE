#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
using SFB;
#else
using SimpleFileBrowser;
#endif
using UnityEngine.Pool;
/// <summary>
/// 文件浏览器工具类，封装了不同平台的文件浏览器调用
/// </summary>
public static class FileBrowser
{
    public static void OpenFile(System.Action<string> onSuccess, System.Action onCancel = null,
        string title = "选择文件", string initialPath = "", string[] filters = null)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        var extFilters = BuildFilters(filters);
        string[] paths = StandaloneFileBrowser.OpenFilePanel(title, initialPath, extFilters,false);
        if (paths != null && paths.Length > 0)
            onSuccess?.Invoke(paths[0]);
        else
            onCancel?.Invoke();
#else
        if (SimpleFileBrowser.FileBrowser.IsOpen) return;
        if(filters != null && filters.Length > 0)
            for (int i = 0; i < filters.Length; i++)
            {
                filters[i] = filters[i].StartsWith(".") ? filters[i] : "." + filters[i];
            }
        SimpleFileBrowser.FileBrowser.SetFilters(false, filters);
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(
            (paths) => onSuccess?.Invoke(paths[0]),
            () => onCancel?.Invoke(),
            SimpleFileBrowser.FileBrowser.PickMode.Files,
            false,
            initialPath,
            title:title
        );
#endif
    }
    public static void OpenFile(string filter, System.Action<string> onSuccess, System.Action onCancel = null,
        string title = "选择文件", string initialPath = "")
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        string[] paths = StandaloneFileBrowser.OpenFilePanel(title, initialPath, filter,false);
        if (paths != null && paths.Length > 0)
            onSuccess?.Invoke(paths[0]);
        else
            onCancel?.Invoke();
#else
        if (SimpleFileBrowser.FileBrowser.IsOpen) return;
        if (string.IsNullOrEmpty(filter))
            SimpleFileBrowser.FileBrowser.SetFilters(true, "*");
        else
            SimpleFileBrowser.FileBrowser.SetFilters(false, filter.StartsWith(".") ? filter : "." + filter);
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(
            (paths) => onSuccess?.Invoke(paths[0]),
            () => onCancel?.Invoke(),
            SimpleFileBrowser.FileBrowser.PickMode.Files,
            false,
            initialPath,
            title:title
        );
#endif
    }
    public static void OpenFolder(System.Action<string> onSuccess, System.Action onCancel = null,
        string title = "选择文件夹", string initialPath = "")
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        string[] paths = StandaloneFileBrowser.OpenFolderPanel(title, initialPath, false);
        if (paths != null && paths.Length > 0)
            onSuccess?.Invoke(paths[0]);
        else
            onCancel?.Invoke();
#else
        if (SimpleFileBrowser.FileBrowser.IsOpen) return;
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(
            (paths) => onSuccess?.Invoke(paths[0]),
            () => onCancel?.Invoke(),
            SimpleFileBrowser.FileBrowser.PickMode.Folders,
            false,
            initialPath,
            title:title
        );
#endif
    }

    public static void SaveFile(System.Action<string> onSuccess, System.Action onCancel = null,
        string title = "保存文件", string initialPath = "", string defaultName = "新文件",
        string filter = null)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        string path = StandaloneFileBrowser.SaveFilePanel(title, initialPath, defaultName, filter);
        if (!string.IsNullOrEmpty(path))
            onSuccess?.Invoke(path);
        else
            onCancel?.Invoke();
#else
        if (SimpleFileBrowser.FileBrowser.IsOpen) return;
        if (string.IsNullOrEmpty(filter))
            SimpleFileBrowser.FileBrowser.SetFilters(true, "*");
        else
            SimpleFileBrowser.FileBrowser.SetFilters(false, filter.StartsWith(".") ? filter : "." + filter);
        SimpleFileBrowser.FileBrowser.ShowSaveDialog(
            (paths) => onSuccess?.Invoke(paths[0]),
            () => onCancel?.Invoke(),
            SimpleFileBrowser.FileBrowser.PickMode.Files,
            false,
            initialPath,
            defaultName,
            title
        );
#endif
    }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
    private static ExtensionFilter[] BuildFilters(string[] filters)
    {
        if (filters == null || filters.Length == 0) return null;
        using var pooledObject= ListPool<ExtensionFilter>.Get(out var list);
        foreach (string ext in filters)
        {
            string clean = ext.StartsWith(".") ? ext.Substring(1) : ext;
            list.Add(new ExtensionFilter(clean.ToUpper() + " 文件", clean));
        }
        return list.ToArray();
    }
#endif
}