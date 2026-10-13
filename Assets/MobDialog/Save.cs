using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace MobDialog
{
    public static class Save
    {
        public static string[] allowedImageExtensions = new string[] { ".png",".jpg", ".jpeg",  ".bmp", ".tiff", ".webp" };
        public static ExtensionFilter imageExtension = new ExtensionFilter("Image", new string[] { "jpg", "jpeg", "png", "bmp", "tiff", "webp" });
        public static ExtensionFilter[] imageExtensions = new ExtensionFilter[] { imageExtension };
        //可用的命令类型
        public static readonly Type[] commandType = new Type[]
        {typeof(Say),typeof(Synchronization),typeof(Queue)
            ,typeof(WaitClick),typeof(WaitSecond)
            ,typeof(CChange),typeof(CIn), typeof(CJump), typeof(CMove), typeof(COut), typeof(CShake)
            ,typeof(BGChange), typeof(BGFade), typeof(BGFadeBG), typeof(BGFadeIn), typeof(BGFadeOut), typeof(BGName), typeof(BGMove), typeof(BGScale) , typeof(BGShakePos), typeof(BGShakeRotation), typeof(BGShakeScale)
            ,typeof(BGM),typeof(SFX),typeof(CFront),typeof(CEmo)};
        //可用的命令类型显示文本
        public static readonly string[] commandTypeDisplayTexts = new[]
{
    "说话",
    "同步",
    "队列",
    "等待点击",
    "等待秒数",
    "角色立绘",
    "角色进入",
    "角色跳跃",
    "角色移动",
    "角色退出",
    "角色震动",
    "切换背景",
    "背景淡入淡出",
    "背景渐变",
    "背景淡入",
    "背景淡出",
    "背景名称",
    "背景移动",
    "背景缩放",
    "背景位置震动",
    "背景旋转震动",
    "背景缩放震动",
    "背景音乐",
    "音效",
    "角色前置",
    "角色表情"
};
        static Dictionary<string, (Sprite, Sprite[], Rect[])> portraits = new();
        //Debug内容
        public static DebugText debug=new();

        public static void BeginDebug(string path)
        {
            debug.Begin(Path.GetFileNameWithoutExtension(path) + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }
        public static void Load(TextAsset asset)
        {
            var b = asset.bytes;
            //TODO
        }
        /// <summary>
        /// 加载剧情文件
        /// </summary>
        public static Flow Load(BinaryReader binary)
        {
            var flow = new Flow();
            Check(binary);
            LoadAllBG(binary, flow);
            LoadAllCharacter(binary, flow);
            LoadAllCommand(binary, flow);
            return flow;
        }
        public static void Check(BinaryReader binary)
        {
            //检查文件头，但是没写
        }
        /// <summary>
        /// 加载所有角色
        /// </summary>
        public static void LoadAllCharacter(BinaryReader binary, Flow flow)
        {
            ushort c = binary.ReadUInt16();//角色数量
            flow.characters = new Character[c];
            for (int i = 0; i < c; i++)
            {
                flow.characters[i] = LoadCharacter(binary.ReadString());
            }
        }
        /// <summary>
        /// 加载角色文件
        /// </summary>
        /// <param name="path">角色文件路径</param>
        /// <returns>加载的角色对象</returns>
        public static Character LoadCharacter(string path)
        {
            var c = new Character();
            string a = Path.ChangeExtension(Path.Combine(Application.persistentDataPath, "Character", path),".ch");
            //角色路径不存在时，返回一个默认的角色
            if (!File.Exists(a))
            {
                c.name =c.camp = "读取失败";
                ColorUtility.TryParseHtmlString("#DDB994", out c.nameColor);
                ColorUtility.TryParseHtmlString("#76471A", out c.campColor);
                c.express=new(0f, 300f);
                c.portrait = new Sprite[1] { Black };
                c.rect= new Rect[1] { new Rect(0f, 100f, 90f, 300f) };
                debug.Write("角色路径不存在:"+a);
                return c;
            }
            string d = Path.GetDirectoryName(a);
            using StreamReader sr = new StreamReader(a, Encoding.UTF8);
            c.name = sr.ReadLine();
            c.camp = sr.ReadLine();
            ColorUtility.TryParseHtmlString("#" + sr.ReadLine(), out c.nameColor);
            ColorUtility.TryParseHtmlString("#" + sr.ReadLine(), out c.campColor);
            if (portraits.ContainsKey(d))
            {
                (c.head, c.portrait, c.rect) = portraits[d];//如果已经加载过了，就直接使用缓存
            }
            else
            {
                //读取Rect文件
                using var br = new BinaryReader(File.OpenRead(Path.Combine(d, "Rect")));
                c.express.x = br.ReadSingle();
                c.express.y = br.ReadSingle();
                int l = (int)(br.BaseStream.Length - br.BaseStream.Position) / 16;
                c.rect = new Rect[l];
                for (int i = 0; i < l; i++)
                {
                    c.rect[i] = new Rect(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                }
                //读取立绘图片
                List<Sprite> sprites = ListPool<Sprite>.Get();
                foreach (var p in Directory.EnumerateFiles(Path.Combine(d), "*.*", SearchOption.AllDirectories).Where(file => allowedImageExtensions.Contains(Path.GetExtension(file).ToLower())).OrderBy(file => file))
                {
                    sprites.Add(LoadImage(p));
                }
                c.portrait = sprites.ToArray();
                ListPool<Sprite>.Release(sprites);
                //读取头像图片
                string th = Path.GetDirectoryName(Path.Combine(Application.persistentDataPath, "Character", "Heads", path));
                foreach (var e in allowedImageExtensions)
                {
                    if (File.Exists(th + e))
                    {
                        c.head = LoadImage(th+e);
                        break;
                    }
                }
                portraits.Add(d, (c.head, c.portrait, c.rect));
            }
            return c;
        }
        /// <summary>
        /// 加载所有背景图片
        /// </summary>
        public static void LoadAllBG(BinaryReader binary, Flow flow)
        {
            ushort b = binary.ReadUInt16();//背景数量
            flow.BGs = new Sprite[b];
            for (int i = 0; i < b; i++)
            {
                string path = Path.Combine(Application.persistentDataPath, "BG", binary.ReadString());
                if (File.Exists(path))
                    flow.BGs[i] = LoadImage(path);
                else
                {
                    flow.BGs[i] = Black;
                    debug.Write("背景路径不存在:" + path);
                }
            }
        }
        /// <summary>
        /// 返回一个黑色的Sprite，用于占位
        /// </summary>
        public static Sprite Black
        {
            get
            {
                if (_black == null)
                {
                    Texture2D texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.black);
                    texture.Apply(false, true);
                    _black = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                return _black;
            }
        }
        private static Sprite _black;
        /// <summary>
        /// 加载图片文件并返回Sprite对象
        /// </summary>
        /// <param name="path">图片文件路径</param>
        /// <returns>加载的Sprite对象</returns>
        public static Sprite LoadImage(string path)
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            texture.Apply(false, true);
            return sprite;
        }
        /// <summary>
        /// 加载所有命令
        /// </summary>
        public static void LoadAllCommand(BinaryReader binary, Flow flow)
        {
            ushort c = binary.ReadUInt16();//命令数量
            flow.commands = new Command[c];
            try
            {
                for (int i = 0; i < c; i++)
                {
                    flow.commands[i] = LoadCommand(binary);
                }
            }
            catch (Exception)
            {
                debug.Write("指令读取异常，可能是版本不匹配。");
            }
        }
        /// <summary>
        /// 加载单个命令
        /// </summary>
        /// <returns>加载的命令对象</returns>
        public static Command LoadCommand(BinaryReader binary)
        {
            ushort c = binary.ReadUInt16();//命令类型索引
            Command command = (Command)Activator.CreateInstance(commandType[c]);//根据索引创建命令对象
            //如果是GroupCommand类型，则递归加载子命令
            if (command is GroupCommand groupCommand)
            {
                LoadGroupCommand(groupCommand, binary);
            }
            FieldInfo[] infos = commandType[c].GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(i => i.MetadataToken).ToArray();//获取命令对象的所有公共实例字段，并按元数据标记排序
            //根据字段类型读取数据并赋值给命令对象
            for (int i = 0; i < infos.Length; i++)
            {
                var type = infos[i].FieldType;
                if (type.IsEnum)
                {
                    //读取枚举底层类型的值并赋值给命令对象
                    Type underlying = Enum.GetUnderlyingType(type);
                    if (underlying == typeof(byte)) infos[i].SetValue(command, binary.ReadByte());
                    else if (underlying == typeof(sbyte)) infos[i].SetValue(command, binary.ReadSByte());
                    else if (underlying == typeof(short)) infos[i].SetValue(command, binary.ReadInt16());
                    else if (underlying == typeof(ushort)) infos[i].SetValue(command, binary.ReadUInt16());
                    else if (underlying == typeof(int)) infos[i].SetValue(command, binary.ReadInt32());
                    else if (underlying == typeof(uint)) infos[i].SetValue(command, binary.ReadUInt32());
                    else if (underlying == typeof(long)) infos[i].SetValue(command, binary.ReadInt64());
                    else if (underlying == typeof(ulong)) infos[i].SetValue(command, binary.ReadUInt64());
                    continue;
                }
                if (type == typeof(byte)) infos[i].SetValue(command, binary.ReadByte());
                else if (type == typeof(sbyte)) infos[i].SetValue(command, binary.ReadSByte());
                else if (type == typeof(short)) infos[i].SetValue(command, binary.ReadInt16());
                else if (type == typeof(ushort)) infos[i].SetValue(command, binary.ReadUInt16());
                else if (type == typeof(int)) infos[i].SetValue(command, binary.ReadInt32());
                else if (type == typeof(uint)) infos[i].SetValue(command, binary.ReadUInt32());
                else if (type == typeof(long)) infos[i].SetValue(command, binary.ReadInt64());
                else if (type == typeof(ulong)) infos[i].SetValue(command, binary.ReadUInt64());
                else if (type == typeof(float)) infos[i].SetValue(command, binary.ReadSingle());
                else if (type == typeof(double)) infos[i].SetValue(command, binary.ReadDouble());
                else if (type == typeof(bool)) infos[i].SetValue(command, binary.ReadBoolean());
                else if (type == typeof(char)) infos[i].SetValue(command, binary.ReadChar());
                else if (type == typeof(decimal)) infos[i].SetValue(command, binary.ReadDecimal());
                else if (type == typeof(string)) infos[i].SetValue(command, binary.ReadString());
            }
            return command;
        }
        static void LoadGroupCommand(GroupCommand command, BinaryReader binary)
        {
            ushort l = binary.ReadUInt16();//子命令数量
            command.commands = new Command[l];
            for (ushort i = 0; i < l; i++)
            {
                command.commands[i] = LoadCommand(binary);
            }
        }
        /// <summary>
        /// 加载编辑器用的剧情文件
        /// </summary>
        /// <returns>加载的编辑器剧情</returns>
        public static EditorFlow LoadEditor(BinaryReader binary)
        {
            var flow = new EditorFlow();
            Check(binary);
            LoadAllBGEditor(binary, flow);
            LoadAllCharacterEditor(binary, flow);
            LoadAllCommandEditor(binary, flow);
            return flow;
        }
        /// <summary>
        /// 加载编辑器用的所有角色并保存路径
        /// </summary>
        public static void LoadAllCharacterEditor(BinaryReader binary, EditorFlow flow)
        {
            ushort c = binary.ReadUInt16();//角色数量
            for (int i = 0; i < c; i++)
            {
                string s = binary.ReadString();
                flow.characterPath.Add(s);
                flow.characters.Add(LoadCharacter(s));
            }
        }
        /// <summary>
        /// 加载编辑角色用的角色文件
        /// </summary>
        /// <param name="path">角色文件路径</param>
        /// <param name="spritePaths">角色图片路径数组</param>
        /// <returns>加载的角色对象</returns>
        public static Character LoadCharacterEditor(string path,out string[] spritePaths)
        {
            var c = new Character();
            string a = Path.Combine(Application.persistentDataPath, "Character", path);
            string d = Path.GetDirectoryName(a);
            using StreamReader sr = new StreamReader(a, Encoding.UTF8);
            c.name = sr.ReadLine();
            c.camp = sr.ReadLine();
            ColorUtility.TryParseHtmlString("#"+sr.ReadLine(), out c.nameColor);
            ColorUtility.TryParseHtmlString("#"+sr.ReadLine(), out c.campColor);
            if (File.Exists(Path.Combine(d, "Rect")))
            {
                portraits.Remove(d);
                using var br = new BinaryReader(File.OpenRead(Path.Combine(d, "Rect")));
                c.express.x = br.ReadSingle();
                c.express.y = br.ReadSingle();
                int l = (int)(br.BaseStream.Length - br.BaseStream.Position) / 16;
                c.rect = new Rect[l];
                for (int i = 0; i < l; i++)
                {
                    c.rect[i] = new Rect(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                }
            }
            List<Sprite> sprites = ListPool<Sprite>.Get();
            spritePaths = Directory.EnumerateFiles(d, "*.*", SearchOption.AllDirectories).Where(file => allowedImageExtensions.Contains(Path.GetExtension(file).ToLower())).ToArray();
            foreach (var p in spritePaths)
            {
                sprites.Add(LoadImage(p));
            }
            c.portrait = sprites.ToArray();
            ListPool<Sprite>.Release(sprites);
            if (c.portrait.Length>c.rect.Length)
            {
                List<Rect> rects = ListPool<Rect>.Get();
                rects.AddRange(c.rect);
                for (int i = c.rect.Length; i < c.portrait.Length; i++)
                {
                    Rect rect = c.portrait[i].rect;
                    float f = 400 / rect.height;
                    c.rect[i] = new(rect.center * f, rect.size * f);
                    c.rect[i].y -= 50;
                    rects.Add(rect);
                }
                c.rect = rects.ToArray();
                ListPool<Rect>.Release(rects);
            }
            string th = Path.GetDirectoryName(Path.Combine(Application.persistentDataPath, "Character", "Heads", path));
            foreach (var e in allowedImageExtensions)
            {
                if (File.Exists(th + e))
                {
                    c.head = LoadImage(th+e);
                    break;
                }
            }
            if (c.rect == null)
            {
                c.rect = new Rect[c.portrait.Length];
                for (int i = 0; i < c.rect.Length; i++)
                {
                    Rect rect = c.portrait[i].rect;
                    float f = 400/rect.height;
                    c.rect[i] = new(rect.center * f, rect.size * f);
                    c.rect[i].y-=50;
                }
                using var r = new BinaryWriter(File.Create(Path.Combine(d, "Rect")));
                r.Write(0f);
                r.Write(300f);
                foreach (var rect in c.rect)
                {
                    r.Write(rect.x);
                    r.Write(rect.y);
                    r.Write(rect.width);
                    r.Write(rect.height);
                }
            }
            return c;
        }
        /// <summary>
        /// 加载编辑器所有背景图片并保存路径
        /// </summary>
        static void LoadAllBGEditor(BinaryReader binary, EditorFlow flow)
        {
            ushort b = binary.ReadUInt16();//背景数量
            for (int i = 0; i < b; i++)
            {
                string s = binary.ReadString();
                flow.BGPath.Add(s);
                flow.BGs.Add(LoadImage(Path.Combine(Application.persistentDataPath, "BG", s)));
            }
        }
        /// <summary>
        /// 加载命令
        /// </summary>
        static void LoadAllCommandEditor(BinaryReader binary, EditorFlow flow)
        {
            ushort c = binary.ReadUInt16();//命令数量
            for (int i = 0; i < c; i++)
            {
                var command = LoadCommand(binary);
                flow.commands.Add(command);
            }
        }
        /// <summary>
        /// 保存角色
        /// </summary>
        /// <param name="path">Character文件夹下相对路径</param>
        public static void SaveCharacter(string path, string name, string camp, Color nameColor, Color campColor, Vector2 express, Rect[] rects)
        {
            string s = Path.ChangeExtension(Path.Combine(Application.persistentDataPath, "Character", path),".ch");
            File.Delete(s);
            var writer = File.CreateText(s);
            writer.WriteLine(name);
            writer.WriteLine(camp);
            writer.WriteLine(ColorUtility.ToHtmlStringRGB(nameColor));
            writer.WriteLine(ColorUtility.ToHtmlStringRGB(campColor));
            writer.Dispose();
            using var r = new BinaryWriter(File.Create(Path.Combine(Path.GetDirectoryName(s), "Rect")));
            r.Write(express.x);
            r.Write(express.y);
            foreach (var rect in rects)
            {
                r.Write(rect.x);
                r.Write(rect.y);
                r.Write(rect.width);
                r.Write(rect.height);
            }
        }
        //保存的顺序与读取相同
        /// <summary>
        /// 保存剧情
        /// </summary>
        public static void SaveFlow(EditorFlow flow)
        {
            string path = Path.Combine(Application.persistentDataPath, "Dialog", flow.title);
            using FileStream fs = File.Create(path);
            using BinaryWriter binary = new BinaryWriter(fs);
            //Check
            binary.Write((ushort)flow.BGPath.Count);
            for (int i = 0; i < flow.BGPath.Count; i++)
            {
                binary.Write(flow.BGPath[i]);
            }
            binary.Write((ushort)flow.characterPath.Count);
            for (int i = 0; i < flow.characterPath.Count; i++)
            {
                binary.Write(flow.characterPath[i]);
            }
            SaveAllCommand(binary, flow);
        }

        private static void SaveAllCommand(BinaryWriter binary, EditorFlow flow)
        {
            ushort count = (ushort)flow.commands.Count;
            binary.Write(count);
            for (int i = 0; i < count; i++)
            {
                SaveCommand(binary, flow.commands[i]);
            }
        }
        private static void SaveCommand(BinaryWriter binary, Command command)
        {
            Type type = command.GetType();
            int typeIndex = Array.IndexOf(commandType, type);
            binary.Write((ushort)typeIndex);
            if (command is GroupCommand groupCommand)
            {
                SaveGroupCommand(binary, groupCommand);
            }
            FieldInfo[] infos = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                                   .OrderBy(f => f.MetadataToken)
                                   .ToArray();

            foreach (FieldInfo field in infos)
            {
                Type fieldType = field.FieldType;
                object value = field.GetValue(command);
                if (fieldType.IsEnum)
                {
                    Type underlying = Enum.GetUnderlyingType(fieldType);
                    WriteValueByType(binary, underlying, value);
                    continue;
                }
                WriteValueByType(binary, fieldType, value);
            }
        }

        private static void SaveGroupCommand(BinaryWriter binary, GroupCommand groupCommand)
        {
            ushort count = (ushort)groupCommand.commands.Length;
            binary.Write(count);
            for (int i = 0; i < count; i++)
            {
                SaveCommand(binary, groupCommand.commands[i]);
            }
        }
        private static void WriteValueByType(BinaryWriter binary, Type type, object value)
        {
            if (type == typeof(byte)) binary.Write((byte)value);
            else if (type == typeof(sbyte)) binary.Write((sbyte)value);
            else if (type == typeof(short)) binary.Write((short)value);
            else if (type == typeof(ushort)) binary.Write((ushort)value);
            else if (type == typeof(int)) binary.Write((int)value);
            else if (type == typeof(uint)) binary.Write((uint)value);
            else if (type == typeof(long)) binary.Write((long)value);
            else if (type == typeof(ulong)) binary.Write((ulong)value);
            else if (type == typeof(float)) binary.Write((float)value);
            else if (type == typeof(double)) binary.Write((double)value);
            else if (type == typeof(bool)) binary.Write((bool)value);
            else if (type == typeof(char)) binary.Write((char)value);
            else if (type == typeof(decimal)) binary.Write((decimal)value);
            else if (type == typeof(string))
            {
                binary.Write((string)value ?? string.Empty);
            }
        }
    }
}
