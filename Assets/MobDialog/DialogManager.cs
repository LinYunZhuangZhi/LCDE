using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobDialog
{
    public class DialogManager:MonoSingleton<DialogManager>
    {
        public BG bg;
        public TextMeshProUGUI cName, camp,conversation;
        public GameObject namePanel,conversationPanel;
        public Transform backParent;
        public Flow flow;
        public int index;
        public Action Complete;
        public void Update()
        {
            // 检测点击事件
            // 如果当前命令是等待点击的命令，并且已经完成，则执行下一条命令
            // 如果当前命令是可跳过的命令，则跳过该命令
            if (Touch())
            {
                if (flow.commands[index] is IWaitClick wait)
                    if (wait.Complete)
                    {
                        flow.commands[index].Next();
                        return;
                    }
                if (flow.commands[index] is ISkipable skipable)
                    skipable.Skip();
            }
            bool Touch()
            {
#if UNITY_STANDALONE_WIN
                return (Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1))&&Input.mousePosition.y<0.85f*Screen.height;
#else
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began&&touch.rawPosition.y<0.85f*Screen.height)
                    {
                        return true;
                    }
                }
                return false;
#endif
            }
        }
        /// <summary>
        /// 设置当前对话的角色信息
        /// </summary>
        /// <param name="character">要设置的角色</param>
        public void SetCharacter(Character character)
        {
            cName.text = character.name;
            camp.text = character.camp;
            cName.color = character.nameColor;
            namePanel.GetComponent<Image>().color = character.campColor;
        }
        /// <summary>
        /// 设置当前对话的角色信息
        /// </summary>
        /// <param name="character">要设置的角色索引</param>
        public void SetCharacter(ushort character)
        {
            cName.text = flow.characters[character].name;
            camp.text = flow.characters[character].camp;
            cName.color = flow.characters[character].nameColor;
            namePanel.GetComponent<Image>().color = flow.characters[character].campColor;
        }
        public void Begin()
        {
            //添加结束的回调
            Complete += () => UIManager.Instance.CloseUI(name,true);
            Complete+=() =>AudioManager.Instance.StopBgm();
            Complete += () => UIManager.Instance.OpenUI<Loading>("Loading");
            //清空
            for (int i = CharacterManager.Instance.portraits.Count - 1; i >= 0; i--)
            {
                Portrait portrait = CharacterManager.Instance.portraits[i];
                CharacterManager.Instance.portraitPool.Release(portrait);
            }
            bg.BGNamePanel.SetActive(false);
            conversationPanel.SetActive(false);
            foreach (var bg in bg.BGs)
            {
                bg.sprite = null;
            }
            bg.rect.Rotate(Vector3.zero, Space.Self);
            bg.rect.anchoredPosition = Vector2.one;
            bg.rect.localScale = Vector2.one;
            bg.rectP.Rotate(Vector3.zero, Space.Self);
            bg.rectP.anchoredPosition = Vector2.one;
            bg.rectP.localScale = Vector2.one;
            index = 0;
            bg.Overlay.gameObject.SetActive(true);
            bg.Overlay.color = Color.black;
            //开始
            bg.Overlay.DOFade(1,0.5f).OnComplete(() => {
            bg.Overlay.rectTransform.SetSiblingIndex(4);
            EnterCommand();
            bg.Overlay.DOFade(0, 0.01f); });
        }
        public void Next()
        {
            flow.commands[index].Exit();
            flow.commands[index].Next -= Next;
            index++;
            EnterCommand();
        }

        private void EnterCommand()
        {
            //如果命令执行完毕，则结束对话
            if (index>=flow.commands.Length)
            {
                bg.Overlay.rectTransform.SetAsLastSibling();
                bg.FadeOut(null, 0, 0, 0, 0.5f).OnComplete(new(Complete));
                return;
            }
            flow.commands[index].Next += Next;
            flow.commands[index].Execute();
        }
        /// <summary>
        /// 进入对话
        /// </summary>
        /// <param name="path">对话文件路径</param>
        /// <param name="complete">回调</param>
        public static void EnterDialog(string path,Action complete)
        {
            var s = File.OpenRead(Path.Combine(Application.persistentDataPath, "Dialog", path));
            var b = new BinaryReader(s);
            Save.BeginDebug(path);
            var f = Save.Load(b);
            Save.debug.End();
            s.Dispose();
            b.Dispose();
            var d= UIManager.Instance.OpenUI<UIBase>("Dialog").GetComponent<DialogManager>();
            d.flow = f;
            d.Complete= complete;
            d.Begin();
        }
        /// <summary>
        /// 进入对话
        /// </summary>
        /// <param name="flow">对话流程</param>
        /// <param name="complete">回调</param>
        public static void EnterDialog(Flow flow, Action complete)
        {
            var d = UIManager.Instance.OpenUI<UIBase>("Dialog").GetComponent<DialogManager>();
            d.flow = flow;
            d.Complete = complete;
            d.Begin();
        }
        /// <summary>
        /// 进入对话（异步加载）
        /// </summary>
        /// <param name="path">对话文件路径</param>
        /// <param name="complete">回调</param>
        public static void EnterDialogAsync(string path, Action complete)
        {
            UIManager.Instance.OpenUI<Loading>("Loading", new DialogLoadData(path,complete));
        }
        public struct DialogLoadData : ILoadingTask
        {
            public string path;
            public Action complete;

            public DialogLoadData(string path, Action complete)
            {
                this.path = path;
                this.complete = complete;
            }

            public IEnumerator Execute(Action<float> onProgress, Action onComplete)
            {
                Save.BeginDebug(path);
                using var s = File.OpenRead(Path.Combine(Application.persistentDataPath, "Dialog", path));
                using var binary = new BinaryReader(s);
                var flow = new Flow();
                Save.Check(binary);
                yield return null;
                onProgress?.Invoke(0.1f);
                Save.LoadAllBG(binary, flow);
                onProgress?.Invoke(0.3f);
                yield return null; 
                ushort c = binary.ReadUInt16();
                flow.characters = new Character[c];
                for (int i = 0; i < c; i++)
                {
                    flow.characters[i] = Save.LoadCharacter(binary.ReadString());
                    onProgress?.Invoke(0.3f + 0.6f * (i + 1) / c);
                    yield return null;
                }
                Save.LoadAllCommand(binary, flow);
                onProgress?.Invoke(0.9666667f);
                yield return null;
                var dialog=UIManager.Instance.OpenUI<UIBase>("Dialog").GetComponent<DialogManager>();
                dialog.flow = flow;
                dialog.Complete = complete;
                onProgress?.Invoke(1f);
                dialog.Begin();
                yield return null;
                onComplete?.Invoke();
                if (Save.debug.hasBug)
                    TipForm.ShowForm($"具体错误可见{Save.debug.debugPath}", "加载错误", false);
                Save.debug.End();
            }
        }
    }
}
