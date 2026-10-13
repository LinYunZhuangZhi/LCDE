using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MobDialog
{
    /// <summary>
    /// 播放界面右上角的UI
    /// </summary>
    public class SelectUI : MonoBehaviour
    {
        public Button  setting, hide, log, skip;
        public Toggle select,auto;
        public HorizontalLayoutGroup group;
        public RectTransform streamer;
        public void Select(bool isOn)
        {
            /*setting.gameObject.SetActive(isOn);
            hide.gameObject.SetActive(isOn);
            log.gameObject.SetActive(isOn);
            auto.gameObject.SetActive(isOn);
            skip.gameObject.SetActive(isOn);*/
            if (isOn)
            {
                DOTween.To(Spacing, SetSpacing, 0, 0.2f);
                streamer.DOSizeDelta(new(239,20.7f), 0.2f);
            }
            else
            {
                DOTween.To(Spacing, SetSpacing, -237.6f, 0.2f);
                streamer.DOSizeDelta(new(36,20.7f), 0.2f);
            }
            float Spacing()
            {
                return group.spacing;
            }
            void SetSpacing(float value)
            {
                group.spacing = value;
            }
        }
        public void ShowSetting()
        {
            DialogManager.Instance.enabled = false;
            var settingUI=UIManager.Instance.OpenUI<SettingUI>("SettingUI");
            settingUI.ok.onClick.AddListener(Click);
            void Click()
            {
                DialogManager.Instance.enabled = true;
                settingUI.ok.onClick.RemoveListener(Click);
            }
        }
        public void Hide()
        {
            DialogManager.Instance.bg.BGNamePanel.SetActive(false);
            DialogManager.Instance.conversationPanel.SetActive(false);
            DialogManager.Instance.enabled=false;
            gameObject.SetActive(false);
            DialogManager.Instance.bg.StartCoroutine(Show());
            IEnumerator Show()
            {
                while (!Touch())
                {
                    yield return null;
                }
                DialogManager.Instance.enabled = true;
                DialogManager.Instance.bg.BGNamePanel.SetActive(true);
                DialogManager.Instance.conversationPanel.SetActive(true);
                gameObject.SetActive(true);
                bool Touch()
                {
#if UNITY_STANDALONE_WIN
                    return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
#else
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        return true;
                    }
                }
                return false;
#endif
                }
            }
        }
        public void Log()
        {
            // 打开对话记录界面，没做
        }
        Coroutine coroutine;
        public void Auto(bool isAuto)
        {
            if (isAuto)
            {
                coroutine=StartCoroutine(AutoEnumerator());
            }
            else
            {
                StopCoroutine(coroutine);
            }
            IEnumerator AutoEnumerator()
            {
                var w=new WaitForSeconds(Setting.instance.AutoDelay);
                while (true)
                {
                    yield return null;
                    if (gameObject.activeInHierarchy)
                    {
                        if (DialogManager.Instance.flow.commands[DialogManager.Instance.index] is IWaitClick wait&&wait.Complete)
                        {
                            int i = DialogManager.Instance.index;
                            yield return w;
                            if (i == DialogManager.Instance.index)
                            {
                                DialogManager.Instance.flow.commands[DialogManager.Instance.index].Next();
                            }
                        }
                    }
                }
            }
        }
        public void Skip()
        {
            Command command = DialogManager.Instance.flow.commands[DialogManager.Instance.index];
            command.Next = () => { };
            DialogManager.Instance.bg.Overlay.rectTransform.SetAsLastSibling();
            DOTween.Clear();
            DialogManager.Instance.bg.FadeOut(null, 0, 0, 0, 0.5f).OnComplete(new(DialogManager.Instance.Complete));
            return;
        }
    }
}