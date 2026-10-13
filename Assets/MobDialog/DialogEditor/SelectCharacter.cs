using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace MobDialog
{
    public class SelectCharacter : MonoBehaviour
    {
        public CharacterHead head;
        public List<CharacterHead> heads;
        public RectTransform content;
        public TMPro.TextMeshProUGUI message;
        public Action<ushort> dele;
        public void SetUI()
        {
            gameObject.SetActive(true);
            message.text = "选择所需角色";
            foreach (var h in heads)
            {
                h.gameObject.SetActive(false);
            }
            for(int i=heads.Count; i< EditorManager.Instance.flow.characters.Count;i++)
            {
                var h=Instantiate(head);
                h.transform.SetParent(content,false);
                ushort o = (ushort)i;
                h.GetComponent<Button>().onClick.AddListener(()=>dele?.Invoke(o));
                heads.Add(h);
            }
            for (int i = 0; i < EditorManager.Instance.flow.characters.Count; i++)
            {
                heads[i].gameObject.SetActive(true);
                heads[i].image.sprite = EditorManager.Instance.flow.characters[i].head;
                heads[i].text.text = EditorManager.Instance.flow.characters[i].name;
            }
        }
        /*public void AddCharacter()
        {
            Load(StandaloneFileBrowser.OpenFilePanel("选择文件", Path.Combine(Application.persistentDataPath, "Character"), "ch", false));
            void Load(string[] paths)
            {
                if (paths != null)
                {
                    if (paths.Length > 0)
                    {
                        string s=Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Character"), paths[0]);
                        if (EditorManager.Instance.flow.characterPath.Contains(s))
                        {
                            TipForm.ShowForm("该角色已选择，请勿重复选择",type:false);
                            return;
                        }
                        EditorManager.Instance.AddCharacter(s);
                        SetUI();
                    }
                }
            }
        }*/
        public void AddCharacter()
        {
            FileBrowser.OpenFile(
                "ch",
                path =>
                {
                    if (string.IsNullOrEmpty(path))
                        return;

                    string s = Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Character"), path);
                    if (EditorManager.Instance.flow.characterPath.Contains(s))
                    {
                        TipForm.ShowForm("该角色已选择，请勿重复选择", type: false);
                        return;
                    }

                    EditorManager.Instance.AddCharacter(s);
                    SetUI();
                },
                title: "选择文件",
                initialPath: Path.Combine(Application.persistentDataPath, "Character"));
        }
        public void RemoveCharacter()
        {
            message.text = "选择所删角色";
            for (int i = 0; i < EditorManager.Instance.flow.characters.Count; i++)
            {
                var b=heads[i].GetComponent<Button>();
                b.onClick.RemoveAllListeners();
                int o = i;
                b.onClick.AddListener(()=>TipForm.ShowForm("","确定删除？",true,
                    () =>
                    {
                        EditorManager.Instance.RemoveCharacter(o);
                        for (ushort j = 0; j < heads.Count; j++)
                        {
                            ushort k = j;
                            var h = heads[k].GetComponent<Button>();
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
