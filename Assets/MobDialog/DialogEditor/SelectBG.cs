using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace MobDialog
{
    public class SelectBG : MonoBehaviour
    {
        public List<Image> bgs;
        public RectTransform content;
        public TMPro.TextMeshProUGUI message;
        public Action<ushort> dele;
        public void Start()
        {
            bgs[0].GetComponent<Button>().onClick.AddListener(() => dele?.Invoke(0));
        }
        public void SetUI()
        {
            gameObject.SetActive(true);
            message.text = "选择所需背景";
            foreach (var h in bgs)
            {
                h.gameObject.SetActive(false);
            }
            for (int i = bgs.Count; i < EditorManager.Instance.flow.BGs.Count; i++)
            {
                var h = Instantiate(bgs[0]);
                h.rectTransform.SetParent(content,false);
                ushort o = (ushort)i;
                Button button = h.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => dele?.Invoke(o));
                bgs.Add(h);
            }
            for (int i = 0; i < EditorManager.Instance.flow.BGs.Count; i++)
            {
                bgs[i].gameObject.SetActive(true);
                bgs[i].sprite = EditorManager.Instance.flow.BGs[i];
            }
        }
        /*public void AddBG()
        {
            Load(StandaloneFileBrowser.OpenFilePanel("选择文件", Path.Combine(Application.persistentDataPath, "BG"),Save.imageExtensions, false));
            void Load(string[] paths)
            {
                if (paths != null)
                {
                    if (paths.Length > 0)
                    {
                        string s = Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "BG"), paths[0]);
                        if (EditorManager.Instance.flow.BGPath.Contains(s))
                        {
                            TipForm.ShowForm("该背景已选择，请勿重复选择", type: false);
                            return;
                        }
                        EditorManager.Instance.AddBG(s);
                        SetUI();
                    }
                }
            }
        }*/
        public void AddBG()
        {
            FileBrowser.OpenFile(
                path =>
                {
                    if (string.IsNullOrEmpty(path))
                        return;

                    string s = Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "BG"), path);
                    if (EditorManager.Instance.flow.BGPath.Contains(s))
                    {
                        TipForm.ShowForm("该背景已选择，请勿重复选择", type: false);
                        return;
                    }

                    EditorManager.Instance.AddBG(s);
                    SetUI();
                },
                null,
                "选择文件",
                Path.Combine(Application.persistentDataPath, "BG"),
                Save.allowedImageExtensions);
        }
        public void RemoveBG()
        {
            message.text = "选择所删背景";
            for (int i = 0; i < EditorManager.Instance.flow.BGs.Count; i++)
            {
                var b = bgs[i].GetComponent<Button>();
                b.onClick.RemoveAllListeners();
                int o = i;
                b.onClick.AddListener(() => TipForm.ShowForm("", "确定删除？", true,
                    () =>
                    {
                        EditorManager.Instance.RemoveBG(o);
                        for (ushort j = 0; j < bgs.Count; j++)
                        {
                            ushort k = j;
                            var h = bgs[k].GetComponent<Button>();
                            h.onClick.RemoveAllListeners();
                            h.onClick.AddListener(() => dele?.Invoke(k));
                        }
                        SetUI();
                    }));
            }
        }
        public void SetDele(Action<ushort> action)
        {
            dele = action;
            dele += a => gameObject.SetActive(false);
        }
    }
}
