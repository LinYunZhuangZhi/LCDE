using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TipForm : UIBase
{
    [SerializeField]
    private Text Title;
    [SerializeField]
    private Text Message;

    [SerializeField]
    private Button btnOK;
    [SerializeField]
    private Button btnCancel;
    [SerializeField]
    private RectTransform BG;

    private Action m_OkAction;

    private Action m_CancelAction;


    protected void Awake()
    {
        btnOK.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseUI("TipForm");
            m_OkAction?.Invoke();
        });
        btnCancel.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseUI("TipForm");
            m_CancelAction?.Invoke();
        });
    }

    public static void ShowForm(string message = "", string title = "提示", bool type = true, Action okAction = null, Action cancelAction = null)
    {
        TipForm formTip = UIManager.Instance.OpenUI<TipForm>("TipForm");
        formTip.SetUI(message, title, type, okAction, cancelAction);
    }



    private void SetUI(string message = "", string title = "提示", bool type = true, Action okAction = null, Action cancelAction = null)
    {
        btnOK.GetComponentInChildren<Text>().text ="确定";
        btnCancel.GetComponentInChildren<Text>().text ="取消";
        Title.text = title;
        Message.text = message;
        if (type)
        {
            btnCancel.gameObject.SetActive(true);
            btnOK.gameObject.SetActive(true);
        }
        else
        {
            btnOK.gameObject.SetActive(true);
            btnCancel.gameObject.SetActive(false);
        }

        m_OkAction = okAction;
        m_CancelAction = cancelAction;
        BG.localScale = Vector3.zero;
        BG.DOScale(1,0.2f);
    }
    public static void ShowFormC(string ok="",string cancel="",string message = "", string title = "提示", bool type = true, Action okAction = null, Action cancelAction = null)
    {
        TipForm formTip = UIManager.Instance.OpenUI<TipForm>("TipForm");
        formTip.SetUIC(ok,cancel,message, title, type, okAction, cancelAction);
    }



    private void SetUIC(string ok = "", string cancel = "",string message = "", string title = "提示", bool type = true, Action okAction = null, Action cancelAction = null)
    {
        Title.text = title;
        Message.text = message;
        btnOK.GetComponentInChildren<Text>().text = ok;
        btnCancel.GetComponentInChildren<Text>().text = cancel;
        if (type)
        {
            btnCancel.gameObject.SetActive(true);
            btnOK.gameObject.SetActive(true);
        }
        else
        {
            btnOK.gameObject.SetActive(true);
            btnCancel.gameObject.SetActive(false);
        }

        m_OkAction = okAction;
        m_CancelAction = cancelAction;
        BG.localScale = Vector3.zero;
        BG.DOScale(1, 0.2f);
    }
    /*public static void Skip(string message = "")
    {
        TipForm formTip = Resources.Load<TipForm>("TipForm");
        formTip.SetUI(message, "确认跳过？", true,gameManager.Process.Next);
    }*/
}