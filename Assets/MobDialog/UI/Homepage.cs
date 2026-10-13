using System;
using System.IO;
using TMPro;
using UnityEngine;
#if UNITY_ANDROID || PLATFORM_ANDROID
using UnityEngine.Android;
using NativeGalleryNamespace;
#endif
using UnityEngine.UI;
namespace MobDialog
{
    public class Homepage:UIBase
    {
        public TextMeshProUGUI title,path,version;
        public RectTransform content;
        public Button button;
        public void Awake()
        {
            version.text = $"Version:{Application.version}";
        }
        /// <summary>
        /// 初始化主页，在点击开始按钮时调用
        /// </summary>
        public void Init()
        {
            for (int i = 1; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
            button.gameObject.SetActive(true);
            path.text = Application.persistentDataPath;
            foreach (var path in Directory.EnumerateFiles(Path.Combine(Application.persistentDataPath,"Dialog")))
            {
                var p=Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Dialog"), path);
                Button b =Instantiate(button, content);
                b.onClick.AddListener(()=>SetDialog(p));
                b.GetComponentInChildren<TextMeshProUGUI>().text = p;
            }
            button.gameObject.SetActive(false);
            Setting.Init(Path.Combine(Application.persistentDataPath,"Setting.json"));
            AudioManager.Instance.LoadBanksFromFolder(Setting.instance.BankPath);
            Setting.RefreshVolume();
        }
        /// <summary>
        /// 检查必要的文件夹是否存在，如果不存在则创建
        /// </summary>
        public void Check()
        {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Dialog")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Dialog"));
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "BG")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "BG"));
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Character", "Heads")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Character", "Heads"));
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "Debug")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Debug"));
#if UNITY_ANDROID || PLATFORM_ANDROID
            if (!(Permission.HasUserAuthorizedPermission("android.permission.READ_EXTERNAL_STORAGE") &&
               Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE")))
                Permission.RequestUserPermissions(new string[]
                { "android.permission.READ_EXTERNAL_STORAGE", "android.permission.WRITE_EXTERNAL_STORAGE" });
#endif
        }
        public void OpenWebsite(string url)
        {
            Application.OpenURL(url);
        }
        public void SetDialog(string t)
        {
            title.text = t;
        }
        /*public void AddDialog()
        {
            string path;
            path = StandaloneFileBrowser.SaveFilePanel("创建文件", Path.Combine(Application.persistentDataPath, "Dialog"), "NewStory", "");
            if (path!=null)
            {
                Save.SaveFlow(new() {title= Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Dialog"), path) });
                Init();
            }
        }*/
        public void AddDialog()
        {
            FileBrowser.SaveFile(
                path =>
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        Save.SaveFlow(new() { title = Path.GetRelativePath(Path.Combine(Application.persistentDataPath, "Dialog"), path) });
                        Init();
                    }
                },
                title: "创建文件",
                initialPath: Path.Combine(Application.persistentDataPath, "Dialog"),
                defaultName: "NewStory");
        }
        public void Edit()
        {
            if(!string.IsNullOrEmpty(title.text))
                EditorManager.Enter(title.text);
        }
        public void Play()
        {
            if (!string.IsNullOrEmpty(title.text))
                DialogManager.EnterDialogAsync(title.text,null);
        }
        public void Remove()
        {
            if (!string.IsNullOrEmpty(title.text))
                TipForm.ShowForm($"确认删除{title.text}?删除后无法恢复。", okAction: () => {
                    File.Delete(Path.Combine(Application.persistentDataPath,"Dialog",title.text));
                    title.text = string.Empty;
                    Init();
                });
        }
        public void EditCharacter()
        {
            CreateCharacterUI.EditCharacter();
        }
        /// <summary>
        /// 打开存档路径，Windows下直接打开文件夹，安卓下选择图片导入到存档路径
        /// </summary>
        public void OpenPath()
        {
#if UNITY_STANDALONE_WIN || PLATFORM_STANDALONE_WIN || UNITY_EDITOR
            Application.OpenURL("file:///" + Application.persistentDataPath);
#elif UNITY_ANDROID || PLATFORM_ANDROID
            if (NativeGallery.IsMediaPickerBusy())
                return;
            if (!NativeGallery.CheckPermission(NativeGallery.PermissionType.Read,NativeGallery.MediaType.Image))
            {
                NativeGallery.RequestPermissionAsync(a=>{ if(a==NativeGallery.Permission.Granted)OpenPath(); }, NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
                return;
            }
            NativeGallery.GetImagesFromGallery(SetImages,"选择要导入的图片");
            void SetImages(string[] images)
            {
                if (images==null||images.Length==0)
                    return;
                FileBrowser.OpenFolder(Copy,initialPath: Application.persistentDataPath);
                void Copy(string path)
                {
                    foreach (string srcPath in images)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(srcPath);
                            string destPath = Path.Combine(path, fileName);
                            File.Copy(srcPath, destPath);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
#endif
        }
        public void OpenSetting()
        {
            UIManager.Instance.OpenUI<SettingUI>("SettingUI");
        }
    }
}