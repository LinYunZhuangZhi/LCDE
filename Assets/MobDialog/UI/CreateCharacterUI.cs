using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MobDialog
{
    public class CreateCharacterUI : MonoBehaviour
    {
        public Character character;
        public Portrait portrait,example;
        public string path, rootPath;
        public TMP_InputField inputName,inputCamp,inputConfig;
        public TextMeshProUGUI textName,textCamp;
        public List<InputRectUI> inputRect;
        public List<TextMeshProUGUI> spriteName;
        public ColorPicker nameColorPicker, campColorPicker;
        public int living=1;
        string[] spritePaths;
        public void ColorChange()
        {
            character.nameColor = nameColorPicker.color;
            character.campColor = campColorPicker.color;
            textName.color = character.nameColor;
            textName.rectTransform.parent.GetComponent<Image>().color = character.campColor;
        }
        public void SaveCharacter()
        {
            if(character != null)
                TipForm.ShowForm("确定保存?", "保存", okAction: () =>
                {
                    ColorChange();
                    Save.SaveCharacter(path, character.name, character.camp, character.nameColor, character.campColor, character.express, character.rect);
                    TipForm.ShowForm("", "保存完成", false);
                });

        }
        /*public void SelectCharacter()
        {
            TipForm.ShowFormC("新建","修改", "新建时选择角色立绘文件夹创建文件，修改选择已有名称文件", "选择角色",true,
            ()=>Create(StandaloneFileBrowser.SaveFilePanel("创建文件", Path.Combine(Application.persistentDataPath,"Character"),"Config","ch"))

            , ()=> {
                var l = StandaloneFileBrowser.OpenFilePanel("选择文件", Path.Combine(Application.persistentDataPath, "Character"), "ch", false);
                if(l != null&&l.Length>0)
                    Load(l[0]); 
            });
            void Load(string path)
            {
                if (!string.IsNullOrEmpty(path))
                {
                     SetCharacter(Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Character"), path));
                }
            }
            void Create(string path)
            {
                if (!string.IsNullOrEmpty(path))
                { 
                    var p=Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Character"), path);
                    var w=File.CreateText(path); 
                    w.WriteLine("???");
                    w.WriteLine("???");
                    w.WriteLine("DDB994");
                    w.WriteLine("76471A");
                    w.Dispose();
                    SetCharacter(p);
                }
            }
        }*/
        public void SelectCharacter()
        {
            TipForm.ShowFormC(
                "新建",
                "修改",
                "新建时选择角色立绘文件夹创建文件，修改选择已有名称文件",
                "选择角色",
                true,
                () => FileBrowser.SaveFile(
                    path => Create(path),
                    title: "创建文件",
                    initialPath: Path.Combine(Application.persistentDataPath, "Character"),
                    defaultName: "Config",
                    filter:"ch" ),
                () => FileBrowser.OpenFile(
                    "ch",
                    path => Load(path),
                    title: "选择文件",
                    initialPath: Path.Combine(Application.persistentDataPath, "Character"))
            );

            void Load(string path)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    SetCharacter(Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Character"), path));
                }
            }

            void Create(string path)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var p = Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Character"), path);
                    using (var w = File.CreateText(path))
                    {
                        w.WriteLine("???");
                        w.WriteLine("???");
                        w.WriteLine("DDB994");
                        w.WriteLine("76471A");
                    }

                    SetCharacter(p);
                }
            }
        }
        public void Example()
        {
            FileBrowser.OpenFile(
                    "ch",
                    path => Load(path),
                    title: "选择文件",
                    initialPath: Path.Combine(Application.persistentDataPath, "Character"));
            void Load(string path)
            {
                example.character = Save.LoadCharacter(path);
                if (example.character.portrait.Length>0)
                {
                    example.gameObject.SetActive(true);
                    example.ChangePortrait(0);
                }
            }
        }
        public void SetCharacter(string path)
        {
            this.path = path;
            rootPath = Path.GetDirectoryName(path);
            character = Save.LoadCharacterEditor(path, out spritePaths);
            textName.text = inputName.text = character.name;
            textCamp.text = inputCamp.text = character.camp;
            Color nameColor = character.nameColor;
            textName.color = nameColor;
            Color campColor = character.campColor;
            textName.rectTransform.parent.GetComponent<Image>().color = campColor;
            nameColorPicker.color = nameColor;
            campColorPicker.color = campColor;
            portrait.character = character;
            inputConfig.text = Path.GetFileNameWithoutExtension(path);
            rootPath = Path.GetDirectoryName(path);
            if (character.portrait.Length == 0)
            {
                portrait.gameObject.SetActive(false);
                return;
            }
            portrait.gameObject.SetActive(true);
            portrait.ChangePortrait(0);
            inputRect[0].OnChange = (Rect r) => {character.rect[0] = r;portrait.ChangePortrait(0); }; 
            portrait.express.rectTransform.anchoredPosition = character.express;
            SetRect();
        }
        public void OnNameChange(string name)
        {
            textName.text=character.name= name;

        }
        public void OnCampChange(string camp)
        {
            textCamp.text = character.camp = camp;

        }
        public void OnConfigChange(string config)
        {
            path = Path.ChangeExtension(Path.Combine(rootPath,config),".ch");
        }
        public void SetRect()
        { 
            var l=character.portrait.Length;
            if (living>l)
            {
                for (int i = l; i <living; i++)
                {
                    spriteName[i].gameObject.SetActive(false);
                    inputRect[i].gameObject.SetActive(false);
                }
                living = l;
            }
            else if (living<l)
            {
                while (inputRect.Count<l)
                {
                    var t = Instantiate(spriteName[0]);
                    t.rectTransform.SetParent(spriteName[0].rectTransform.parent);
                    t.transform.localScale = Vector3.one;
                    spriteName.Add(t);
                    var r=Instantiate(inputRect[0]);
                    r.transform.SetParent(inputRect[0].transform.parent);
                    r.transform.localScale = Vector3.one;
                    inputRect.Add(r);
                }
                for (int i = living; i < l; i++)
                {
                    spriteName[i].gameObject.SetActive(true);
                    inputRect[i].gameObject.SetActive(true);
                    int c = i;
                    inputRect[c].OnChange = (Rect r) => { character.rect[c] = r; portrait.ChangePortrait(c); };
                }
                living = l;
            }
            for (int i = 0; i < l; i++)
            {
                spriteName[i].text = Path.GetFileNameWithoutExtension(spritePaths[i]);
                inputRect[i].SetRect(character.rect[i]);
            }
        }
        public void ChangeScale(float f)
        {
            float scale = 1+f/portrait.image.rectTransform.rect.height;
            for (int i = 0; i < character.rect.Length; i++)
            {
                Rect rect = character.rect[i];
                Rect newRect = new Rect(rect.x*scale, rect.y * scale, rect.width * scale, rect.height * scale);
                character.rect[i] = newRect;
            }
            for (int i = 0; i < character.portrait.Length; i++)
            {
                if (character.portrait[i]==portrait.image.sprite)
                {
                    Rect rect = character.rect[i];
                    portrait.image.rectTransform.anchoredPosition = new Vector2(rect.x, rect.y);
                    portrait.image.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                    return;
                }
            }
        }
        public void ChangeEmo(Vector2 p)
        {
            p *= 0.75f;
            character.express += p;
            portrait.express.rectTransform.anchoredPosition += p;
        }
        public void ResetEmo()
        {
            character.express = new Vector2(0f, 300f);
            portrait.express.rectTransform.anchoredPosition = new Vector2(0f, 300f);
        }
        public void ChangeY(float f)
        {
            for (int i = 0; i < character.rect.Length; i++)
            {
                Rect rect = character.rect[i];
                Rect newRect = new Rect(rect.x, rect.y + f, rect.width, rect.height);
                character.rect[i] = newRect;
            }
            for (int i = 0; i < character.portrait.Length; i++)
            {
                if (character.portrait[i] == portrait.image.sprite)
                {
                    Rect rect = character.rect[i];
                    portrait.image.rectTransform.anchoredPosition = new Vector2(rect.x, rect.y);
                    portrait.image.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                    return;
                }
            }
        }
        public static void EditCharacter(string path)
        {
            UIManager.Instance.OpenUI<UIBase>("CreateCharacter").GetComponent<CreateCharacterUI>().SetCharacter(path);
        }
        public static void EditCharacter()
        {
            UIManager.Instance.OpenUI<UIBase>("CreateCharacter").GetComponent<CreateCharacterUI>();
        }
        public void Exit()
        {
            UIManager.Instance.CloseUI(name,true);
        }
    }
}
