using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
namespace MobDialog
{
    public class EditorManager : MonoSingleton<EditorManager>
    {
        public SelectCharacter selectCharacter;
        public SelectBG selectBG;
        public SelectAudio selectAudio;
        public TextMeshProUGUI title;
        public string[] commandName;
        public EditorFlow flow;
        public void AddBG(string path)
        {
            if (flow.BGPath.Contains(path))
            {
                return;
            }
            flow.BGPath.Add(path);
            flow.BGs.Add(Save.LoadImage(Path.Combine(Application.persistentDataPath, "BG", path)));
        }
        public void RemoveBG(int index)
        {
            flow.BGs.RemoveAt(index);
            flow.BGPath.RemoveAt(index);
            foreach (var command in flow.commands)
            {
                if (command is BGChange change)
                {
                    if (change.bgIndex==index)
                    {
                        change.bgIndex = 0;
                    }
                    if (change.bgIndex>index)
                    {
                        change.bgIndex--;
                    }
                }
            }
        }
        public void AddCharacter(string path)
        {
            if (flow.characterPath.Contains(path))
            {
                return;
            }
            flow.characterPath.Add(path);
            flow.characters.Add(Save.LoadCharacter(path));
        }
        public void RemoveCharacter(int index)
        {
            flow.characters.RemoveAt(index);
            flow.characterPath.RemoveAt(index);
            foreach (var command in flow.commands)
            {
                if (command is CIn cIn)
                {
                    if (cIn.character == index)
                    {
                        cIn.character = 0;
                    }
                    if (cIn.character > index)
                    {
                        cIn.character--;
                    }
                }
                if (command is Say say)
                {
                    if (say.character == index)
                    {
                        say.character = 0;
                    }
                    if (say.character > index)
                    {
                        say.character--;
                    }
                }
            }
        }
        /*public void OnCommandChange(int index,Command newCommand)
        {
            if (flow.commands[index] is COut cOut&&newCommand is COut ncOut)
            {
                for (int i = index+1; i < flow.commands.Count; i++)
                {
                    if (flow.commands[i] is GroupCommand group)
                    {
                        foreach (var c in group.commands)
                        {
                            
                        }
                    }
                    if (flow.commands[i] is IPortrait ip)
                    {
                        var a=ip.Portrait;
                        ip.Portrait = (ushort)(a +(cOut.portrait<=a?1:0)-(ncOut.portrait <= a ? 1 : 0));
                    }
                }
            }
        }*/
        public void AddCommand(Command command)
        {
            flow.commands.Add(command);
        }
        public void InsertCommand(Command command,int index)
        {
            flow.commands.Insert(index,command);
        }
        public void AddCommand()
        {
            var c=new Synchronization();
            c.commands = new Command[0];
            flow.commands.Add(c);
        }
        public void InsertCommand(int index)
        {
            var c = new Synchronization();
            c.commands = new Command[0];
            flow.commands.Insert(index,c);
        }
        public void RemoveCommand(int index)
        {
            flow.commands.RemoveAt(index);
        }
        public void Init()
        {
            for (int i = CharacterManager.Instance.portraits.Count - 1; i >= 0; i--)
            {
                CharacterManager.Instance.portraitPool.Release(CharacterManager.Instance.portraits[i]);
            }
            DialogManager.Instance.conversationPanel.SetActive(false);
            DialogManager.Instance.bg.BGNamePanel.SetActive(false);
            DialogManager.Instance.flow = new();
            title.text = flow.title;
            GetComponentInChildren<CommandListEditor>().enabled=true;
        }
        /// <summary>
        /// 初始化对话流程到指定命令
        /// </summary>
        /// <param name="index">要初始化到的命令索引</param>
        public void SetProcess(int index)
        {
            AudioManager.Instance.avoidSfx = true;
            DG.Tweening.DOTween.Clear();
            DialogManager.Instance.flow.BGs=flow.BGs.ToArray();
            DialogManager.Instance.flow.characters = flow.characters.ToArray();
            DialogManager.Instance.flow.commands = flow.commands.ToArray();
            for (int i = CharacterManager.Instance.portraits.Count - 1; i >= 0; i--)
            {
                CharacterManager.Instance.portraitPool.Release(CharacterManager.Instance.portraits[i]);
            }
            DialogManager.Instance.conversationPanel.SetActive(false);
            DialogManager.Instance.bg.BGNamePanel.SetActive(false);
            foreach (var bg in DialogManager.Instance.bg.BGs)
            {
                bg.sprite = null;
            }
            DialogManager.Instance.bg.rect.Rotate(Vector3.zero, Space.Self);
            DialogManager.Instance.bg.rect.anchoredPosition = Vector2.one;
            DialogManager.Instance.bg.rect.localScale = Vector2.one;
            DialogManager.Instance.bg.rectP.Rotate(Vector3.zero, Space.Self);
            DialogManager.Instance.bg.rectP.anchoredPosition = Vector2.one;
            DialogManager.Instance.bg.rectP.localScale = Vector2.one;
            //从头执行到指定命令，跳过BGM、SFX和Queue命令
            for (int i = 0; i < index; i++)
            {
                if (flow.commands[i]is BGM or SFX or Queue)
                {
                    continue;
                }
                else if(flow.commands[i] is Synchronization synchronization)
                {
                    //执行同步命令中的第一层命令
                    foreach (var command in synchronization.commands)
                    {
                        if (command is BGM or SFX or GroupCommand)
                        {
                            continue;
                        }
                        try
                        {
                            command.Next = () => { };
                            command.Execute();
                            if (command is ISkipable skipable)
                                skipable.Skip();
                            command.Exit();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    continue;
                }
                try
                {
                    flow.commands[i].Next = () => { };
                    flow.commands[i].Execute();
                    if (flow.commands[i] is ISkipable skipable)
                        skipable.Skip();
                    flow.commands[i].Exit();
                }
                catch (Exception)
                {
                }
            }
            AudioManager.Instance.avoidSfx = false;
            try
            {
                //执行指定命令
                flow.commands[index].Next = () => { };
                flow.commands[index].Execute();
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 获取指定命令索引的在场角色索引
        /// </summary>
        /// <param name="index">要获取在场角色索引的命令索引</param>
        /// <returns>角色索引数组</returns>
        public ushort[] GetExistingCharacter(int index)
        {
            var list=ListPool<ushort>.Get();
            for (int i = 0; i < index; i++)
            {
                CheckCommand(flow.commands[i]);
            }
            var a = list.ToArray();
            ListPool<ushort>.Release(list);
            return a;
            void CheckCommand(Command command)
            {
                if (command is CIn cIn)
                {
                    if (cIn.character < flow.characters.Count)
                        list.Add(cIn.character);
                }
                if (command is COut cOut)
                {
                    list.RemoveAt(cOut.portrait);
                }
                if (command is GroupCommand group)
                {
                    foreach (var c in group.commands)
                    {
                        CheckCommand(c);
                    }
                }
            }
        }
        /// <summary>
        /// 获取指定命令索引的在场角色索引
        /// </summary>
        /// <param name="index">要获取在场角色索引的命令索引</param>
        /// <param name="list">用于存储角色索引的列表</param>
        public void GetExistingCharacter(int index,List<ushort> list)
        {
            for (int i = 0; i < index; i++)
            {
                if (flow.commands[i] is GroupCommand group)
                {
                    foreach (var command in group.commands)
                    {
                        if (command is CIn cIn)
                        {
                            list.Add(cIn.character);
                        }
                        if (command is COut cOut)
                        {
                            list.RemoveAt(cOut.portrait);
                        }
                    }
                }
            }
        }
        public void SaveFlow()
        {
            try
            {
                Save.SaveFlow(flow);
                TipForm.ShowForm("", "保存成功", false);
            }
            catch (Exception e)
            {
                TipForm.ShowForm(e.StackTrace,e.Message, false);
            }
        }
        public void Exit()
        {
            TipForm.ShowForm("退出前请保存",okAction:()=> { UIManager.Instance.CloseUI(name, true);
                AudioManager.Instance.StopBgm();
            });
        }
        public static void Enter(string path)
        {
            var p = Path.Combine(Application.persistentDataPath, "Dialog", path);
            var d = UIManager.Instance.OpenUI<UIBase>("DialogEditor").GetComponent<EditorManager>();
            if (File.Exists(p))
            {
                var s = File.OpenRead(p);
                var b = new BinaryReader(s);
                Save.BeginDebug(path);
                var f = Save.LoadEditor(b);
                if (Save.debug.hasBug)
                    TipForm.ShowForm($"具体错误可见{Save.debug.debugPath}", "加载错误", false);
                Save.debug.End();
                s.Dispose();
                b.Dispose();
                d.flow = f;
                d.flow.title = path;
            }
            else
            {
                d.flow = new()
                {
                    title = path
                };
            }
            d.Init();
        }
    }
    /// <summary>
    /// 对话编辑器的对话流程数据结构，与Flow相比，使用List方便编辑器操作，并且存储路径
    /// </summary>
    public class EditorFlow
    {
        public string title;
        public List<Sprite> BGs;
        public List<Character> characters;
        public List<Command> commands;
        public List<string> BGPath, characterPath;

        public EditorFlow()
        {
            BGs = new();
            characters = new();
            commands = new();
            BGPath = new();
            characterPath= new();
        }
    }
}