using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    public const string uiPrefabPath = "";
    public int maxSortOrder = 10000;

    private Dictionary<string, UIBase> uiCache = new Dictionary<string, UIBase>();
    [SerializeField]private List<UIBase> openedUIList = new List<UIBase>();
    private int currentSortOrder = 0;

    //排序管理
    private int GetNextSortOrder()
    {
        if (currentSortOrder > maxSortOrder)
        {
            int order = 0;
            foreach (var ui in openedUIList)
            {
                if (ui != null && ui.canvas != null)
                    ui.canvas.sortingOrder = order++;
            }
            currentSortOrder = openedUIList.Count;
        }
        return ++currentSortOrder;
    }

    //同步打开 UI
    public T OpenUI<T>(string uiName, object param = null) where T : UIBase
    {
        UIBase ui;
        if (uiCache.ContainsKey(uiName))
        {
            ui = uiCache[uiName];
            ui.gameObject.SetActive(true);
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(uiPrefabPath + uiName);
            if (prefab == null)
            {
                Debug.LogError($"UI Prefab not found: {uiPrefabPath + uiName}");
                return null;
            }
            GameObject go = Instantiate(prefab);
            ui = go.GetComponent<T>();
            if (ui == null)
            {
                Debug.LogError($"Prefab {uiName} does not have component of type {typeof(T)}");
                Destroy(go);
                return null;
            }
            ui.name = uiName;
            ui.OnInit();
            uiCache.Add(uiName, ui);
        }

        ui.canvas.sortingOrder = GetNextSortOrder();
        if (!openedUIList.Contains(ui))
            openedUIList.Add(ui);

        ui.OnOpen(param);
        return ui as T;
    }

    //异步打开 UI
    public void OpenUIAsync<T>(string uiName, object param = null, Action<T> onOpened = null) where T : UIBase
    {
        // 如果已缓存，直接同步打开
        if (uiCache.ContainsKey(uiName))
        {
            T ui = OpenUI<T>(uiName, param);
            Action action=() => onOpened?.Invoke(ui);
            OpenUI<Loading>("Loading",action);
            return;
        }

        // 通过 LoadingUI 异步加载
        var loadData = new UILoadData(uiPrefabPath + uiName, (prefab) =>
        {
            if (prefab == null)
            {
                Debug.LogError($"异步加载 UI 失败: {uiName}");
                return;
            }

            UIBase ui = Instantiate(prefab);
            if (ui == null)
            {
                Debug.LogError($"Prefab {uiName} 缺少组件 {typeof(T)}");
                Destroy(ui.gameObject);
                return;
            }

            ui.name = uiName;
            ui.OnInit();
            uiCache.Add(uiName, ui);
            ui.canvas.sortingOrder = GetNextSortOrder();
            if (!openedUIList.Contains(ui))
                openedUIList.Add(ui);
            ui.OnOpen(param);

            onOpened?.Invoke(ui as T);
        });

        // 打开 LoadingUI 并传入加载任务
        OpenUI<Loading>("Loading", loadData);
    }

    // 关闭
    public void CloseUI(string uiName, bool destroy = false)
    {
        if (!uiCache.ContainsKey(uiName))
        {
            Debug.LogWarning($"UI {uiName} not in cache");
            return;
        }

        UIBase ui = uiCache[uiName];
        if (destroy)
        {
            uiCache.Remove(uiName);
            openedUIList.Remove(ui);
            Destroy(ui.gameObject);
        }
        else
        {
            ui.OnClose();
            openedUIList.Remove(ui);
        }
    }

    public void PopUI()
    {
        if (openedUIList.Count > 0)
        {
            UIBase top = openedUIList[openedUIList.Count - 1];
            CloseUI(top.name, false);
        }
    }

    public void BringToFront(string uiName)
    {
        if (!uiCache.ContainsKey(uiName))
        {
            Debug.LogWarning($"UI {uiName} not in cache");
            return;
        }

        UIBase ui = uiCache[uiName];
        if (!ui.gameObject.activeSelf)
        {
            Debug.LogWarning($"UI {uiName} is not active, cannot bring to front");
            return;
        }
        openedUIList.Remove(ui);
        openedUIList.Add(ui);
        ui.canvas.sortingOrder = GetNextSortOrder();
    }

    public T GetUI<T>(string uiName) where T : UIBase
    {
        if (uiCache.ContainsKey(uiName))
            return uiCache[uiName] as T;
        return null;
    }

    public UIBase GetTopUI()
    {
        if (openedUIList.Count > 0)
            return openedUIList[openedUIList.Count - 1];
        return null;
    }

    public void ClearAll()
    {
        var list = new List<UIBase>(uiCache.Values);
        foreach (var ui in list)
        {
            if (ui != null)
                Destroy(ui.gameObject);
        }
        uiCache.Clear();
        openedUIList.Clear();
        currentSortOrder = 0;
    }
    private struct UILoadData : ILoadingTask
    {
        private string resourcePath;
        private Action<UIBase> onComplete;

        public UILoadData(string path, Action<UIBase> onComplete)
        {
            this.resourcePath = path;
            this.onComplete = onComplete;
        }

        public System.Collections.IEnumerator Execute(Action<float> onProgress, Action onTaskComplete)
        {
            var request = Resources.LoadAsync<UIBase>(resourcePath);
            while (!request.isDone)
            {
                onProgress?.Invoke(request.progress);
                yield return null;
            }

            UIBase prefab = request.asset as UIBase;
            onComplete?.Invoke(prefab);
            onTaskComplete?.Invoke();
        }
    }
}