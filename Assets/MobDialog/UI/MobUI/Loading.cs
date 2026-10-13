using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : UIBase
{
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private string[] tips;
    [SerializeField] private Image[] progressImages;
    [SerializeField] private Sprite off,on;
    public override void OnInit()
    {
        base.OnInit();
        DontDestroyOnLoad(gameObject);
        SetProgress(0);
        SetTip();
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        foreach (var image in progressImages)
        {
            image.sprite = off;
        }
        // 如果没有传入参数，则模拟加载进度
        if (param==null)
        {
            StartCoroutine(enumerator());
            return;
            IEnumerator enumerator()
            {
                for (int i = 0; i < 16; i++)
                {
                    yield return null;
                    SetProgress(i/15f);
                    yield return null;
                    yield return null;
                    yield return null;
                }
                UIManager.Instance.CloseUI(name, false);
            }
        }
        if (param is Action action)
        {
            StartCoroutine(enumerator());
            return;
            IEnumerator enumerator()
            {
                for (int i = 0; i < 11; i++)
                {
                    SetProgress(i * 0.1f);
                    yield return null;
                }
                action?.Invoke();
                UIManager.Instance.CloseUI(name, false);
            }
        }
        if (!(param is ILoadingTask task))
        {
            Debug.LogWarning("LoadingUI: 参数未实现 ILoadingTask 接口");
            return;
        }

        // 启动协程，传入进度回调和完成回调
        StartCoroutine(task.Execute(SetProgress, () =>
        {
            // 加载完成，关闭 UI
            UIManager.Instance.CloseUI(name, false);
        }));
    }

    //进度更新
    private void SetProgress(float progress)
    {
        float clamped = Mathf.Clamp01(progress);
        int index = Mathf.FloorToInt(clamped * progressImages.Length);
        for (int i = 0; i < index; i++)
        {
            progressImages[i].sprite = on;
        }
        for (int i = index; i < progressImages.Length; i++)
        {
            progressImages[i].sprite = off;
        }
    }
    /// <summary>
    /// 设置随机提示文本
    /// </summary>
    private void SetTip()
    {
        if (tipText != null && tips != null && tips.Length > 0)
            tipText.text = tips.Random();
    }
}
public struct SceneLoadData : ILoadingTask
{
    public string sceneName;
    public LoadSceneMode loadSceneMode;
    public Action onComplete; // 外部完成回调

    public SceneLoadData(string sceneName, Action onComplete = null)
    {
        this.sceneName = sceneName;
        this.loadSceneMode = LoadSceneMode.Single;
        this.onComplete = onComplete;
    }

    public SceneLoadData(string sceneName, LoadSceneMode mode, Action onComplete = null)
    {
        this.sceneName = sceneName;
        this.loadSceneMode = mode;
        this.onComplete = onComplete;
    }

    public IEnumerator Execute(Action<float> onProgress, Action onComplete)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            onProgress?.Invoke(progress);

            if (async.progress >= 0.9f)
            {
                // 加载完成，等待一帧后激活
                yield return null;
                async.allowSceneActivation = true;
                break;
            }
            yield return null;
        }

        // 等待场景激活
        yield return null;
        this.onComplete?.Invoke();        // 外部回调
        onComplete?.Invoke();    // UI 完成回调
    }
}

public struct AssetBundleLoadData : ILoadingTask
{
    public string bundlePath;
    public Action<AssetBundle> onComplete; // 外部回调，传入加载好的 Bundle

    public AssetBundleLoadData(string bundlePath, Action<AssetBundle> onComplete = null)
    {
        this.bundlePath = bundlePath;
        this.onComplete = onComplete;
    }

    public IEnumerator Execute(Action<float> onProgress, Action onComplete)
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
        while (!request.isDone)
        {
            onProgress?.Invoke(request.progress);
            yield return null;
        }

        AssetBundle bundle = request.assetBundle;
        if (bundle == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"加载 AssetBundle 失败: {bundlePath}");
#endif
            onComplete?.Invoke();
            yield break;
        }

        onProgress?.Invoke(1f);
        this.onComplete?.Invoke(bundle);
        onComplete?.Invoke();
    }
}
public interface ILoadingTask
{
    /// <summary>
    /// 执行加载的协程
    /// </summary>
    /// <param name="onProgress">进度回调 (0~1)</param>
    /// <param name="onComplete">完成回调</param>
    IEnumerator Execute(Action<float> onProgress, Action onComplete);
}