using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace MobDialog
{
    public class CommandListEditor : MonoSingleton<CommandListEditor>
    {
        [SerializeField] private RectTransform itemContainer;          // 列表项的父物体
        public CommandItem commandItemPrefab;     // CommandItem 预制体
        public TMPro.TMP_Dropdown typeDropdown;           // 下拉选择命令类型
        [SerializeField] private Button addButton;               // 追加末尾
        [SerializeField] private CommandDynamicFieldUI detailFieldUI;   // 用于显示选中命令的字段
        private List<CommandItem> commandItems = new List<CommandItem>();
        public int selectedIndex = -1;
        public Command selectCommand;

        private EditorManager editor => EditorManager.Instance;

        void OnEnable()
        {
            typeDropdown.ClearOptions();
            typeDropdown.AddOptions(Save.commandTypeDisplayTexts.ToList());
            typeDropdown.value = 0;
            typeDropdown.RefreshShownValue();
            RefreshUI();
        }

        /// <summary>
        /// 在末尾添加新命令
        /// </summary>

        public void AddCommand()
        {
            Type selectedType = Save.commandType[typeDropdown.value];
            Command newCmd = (Command)Activator.CreateInstance(selectedType);
            editor.AddCommand(newCmd);
            RefreshUI();
            SelectItem(editor.flow.commands.Count - 1);
        }

        private void ClearAll()
        {
            // 清空所有命令，好像用不上
            editor.flow.commands.Clear();
            RefreshUI();
            SelectItem(-1);
        }

        /// <summary>
        /// 在指定索引之前插入新命令
        /// </summary>
        public void InsertBefore(int index)
        {
            if (index < 0 || index > editor.flow.commands.Count) return;
            Type selectedType = Save.commandType[typeDropdown.value];
            Command newCmd = (Command)Activator.CreateInstance(selectedType);
            editor.InsertCommand(newCmd, index);
            RefreshUI();
            SelectItem(index);
        }

        /// <summary>
        /// 删除指定索引的命令
        /// <param name="index">要删除的命令索引</param>
        /// </summary>
        public void DeleteItem(int index)
        {
            if (index < 0 || index >= editor.flow.commands.Count) return;
            editor.RemoveCommand(index);
            RefreshUI();
            if (selectedIndex == index)
                SelectItem(-1);
            else if (selectedIndex > index)
                selectedIndex--;
        }

        /// <summary>
        /// UI 刷新与选中
        /// </summary>

        private void RefreshUI()
        {
            // 清除旧项
            foreach (var item in commandItems)
                Destroy(item.gameObject);
            commandItems.Clear();

            var commandList = editor.flow.commands;

            // 重建
            for (int i = 0; i < commandList.Count; i++)
            {
                CommandItem item = Instantiate(commandItemPrefab, itemContainer);
                if (item == null) { Debug.LogError("CommandItem 组件缺失"); continue; }

                item.Initialize(commandList[i], i, this);
                commandItems.Add(item);
            }

            // 如果选中索引超出，重置
            if (selectedIndex >= commandItems.Count)
                SelectItem(-1);
        }

        /// <summary>
        /// 选中某个索引的命令，并在右侧显示其字段
        /// <param name="index">要选中的命令索引</param>
        /// </summary>
        public void SelectItem(int index)
        {
            // 取消旧高亮
            if (selectedIndex >= 0 && selectedIndex < commandItems.Count)
                commandItems[selectedIndex].SetHighlight(false);

            selectedIndex = index;
            editor.SetProcess(index);
            // 应用新高亮
            if (selectedIndex >= 0 && selectedIndex < commandItems.Count)
            {
                selectCommand = editor.flow.commands[selectedIndex];
                commandItems[selectedIndex].SetHighlight(true);
                detailFieldUI.BuildUI(selectCommand);
            }
            else
            {
                detailFieldUI.ClearUI();
            }
        }
    }
}