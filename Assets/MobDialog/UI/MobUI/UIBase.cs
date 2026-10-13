using UnityEngine;
/// <summary>
/// UI的基类，所有UI都继承自这个类
/// </summary>
public class UIBase : MonoBehaviour
{

    public Canvas canvas;
    /// <summary>
    /// UI初始化时调用
    /// </summary>
    public virtual void OnInit() { }
    /// <summary>
    /// UI打开时调用
    /// </summary>
    /// <param name="param">传递给UI的参数</param>
    public virtual void OnOpen(object param = null)
    {
        gameObject.SetActive(true);
    }
    /// <summary>
    /// UI关闭时调用
    /// </summary>
    public virtual void OnClose()
    {
        gameObject.SetActive(false);
    }

}