using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
namespace MobDialog
{
    public class CommandDynamicFieldUI : MonoBehaviour
    {
        [SerializeField] private GameObject headerPrefab;
        [SerializeField] private GameObject intPrefab;
        [SerializeField] private GameObject floatPrefab;
        [SerializeField] private GameObject boolPrefab;
        [SerializeField] private GameObject stringPrefab;
        [SerializeField] private GameObject textAreaPrefab;
        [SerializeField] private GameObject enumPrefab;
        [SerializeField] private GameObject arrayPrefab;
        [SerializeField] private GameObject objectPrefab;
        [SerializeField] private GameObject bgButtonPrefab;
        [SerializeField] private GameObject characterButtonPrefab;
        [SerializeField] private ScrollRect scrollRectPrefab;
        [SerializeField] private GameObject portraitPrefab;
        [SerializeField] private RectTransform uiContainer;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private GameObject tooltipInstance;
        [SerializeField] private RectTransform tooltipRect;
        [SerializeField] private Image emoImage;
        private Dictionary<GameObject, string> fieldTooltips = new Dictionary<GameObject, string>();
        public Dictionary<string, string> translate = new Dictionary<string, string>()
        {
            { "commands", "命令列表" },
            { "action", "动作" },
            { "bgIndex", "背景编号" },
            { "r", "红色" },
            { "g", "绿色" },
            { "b", "蓝色" },
            { "time", "时长" },
            { "targetX", "目标 X" },
            { "targetY", "目标 Y" },
            { "duration", "持续时间" },
            { "withCharacter", "跟随角色" },
            { "name", "名称" },
            { "strengthX", "横向强度" },
            { "strengthY", "纵向强度" },
            { "strength", "强度" },
            { "vibrato", "震动频率" },
            { "randomness", "随机度" },
            { "fadeOut", "淡出" },
            { "random", "随机" },
            { "portrait", "立绘" },
            { "face", "表情" },
            { "character", "角色" },
            { "place", "位置" },
            { "high", "高度" },
            { "position", "位置" },
            { "alpha", "透明度" },
            { "conversation", "对话内容" },
            { "hideOthers", "隐藏其他角色" },
            { "horizontal", "水平对齐" },
            { "vertical", "垂直对齐" },
            { "skipable", "可跳过" },
            { "ease", "缓动方式" },
            { "path", "路径" },
            { "emo", "表情" }
        };
        private object targetObject;

        /// <summary>
        /// 根据目标对象的字段动态生成UI控件
        /// </summary>
        /// <param name="target">目标对象</param>
        public void BuildUI(object target)
        {
            if (target == null) { Debug.LogError("目标对象为空"); return; }
            targetObject = target;
            ClearUI();
            GenerateFields(target.GetType(), uiContainer, target);
        }
        /// <summary>
        /// 清除当前UI控件
        /// </summary>
        public void ClearUI()
        {
            foreach (Transform child in uiContainer)
                Destroy(child.gameObject);
            fieldTooltips.Clear();
            HideTooltip();
        }

        private void GenerateFields(Type type, Transform parent, object obj)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<HideInInspector>() != null)
                    continue;

                var header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null)
                    CreateHeader(header.header, parent);
                if (field.GetCustomAttribute<BGAttribute>() != null)
                {
                    var go = Instantiate(bgButtonPrefab, parent);
                    BindBGButton(go, field, obj);
                }
                else if (field.GetCustomAttribute<CharacterAttribute>() != null)
                {
                    var go = Instantiate(characterButtonPrefab, parent);
                    BindCharacterButton(go, field, obj);
                }
                else if (field.GetCustomAttribute<PortraitAttribute>() != null)
                {
                    CreateHeader(string.Join(',', EditorManager.Instance.GetExistingCharacter(CommandListEditor.Instance.selectedIndex).Select((value, index) => $"{index}:{EditorManager.Instance.flow.characters[value].name}")), parent);
                }
                else if (field.Name=="emo")
                {
                    Instantiate(emoImage, parent);
                }
                else
                {
                    var a = field.GetCustomAttribute<AudioAttribute>();
                    if (a != null)
                    {
                        var go = Instantiate(characterButtonPrefab, parent);
                        BindAudioButton(go, field, obj, a.IsBGM);
                    }
                }
                CreateFieldControl(field, obj, parent);
            }
        }

        private void CreateHeader(string text, Transform parent)
        {
            if (headerPrefab == null) return;
            var go = Instantiate(headerPrefab, parent);
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = text;
        }

        private void CreateFieldControl(FieldInfo field, object obj, Transform parent)
        {
            Type type = field.FieldType;
            GameObject prefab = null;
            if (field.GetCustomAttribute<TextAreaAttribute>() != null)
                prefab = textAreaPrefab;
            else
                prefab = GetPrefabForType(type);

            if (prefab == null)
            {
                if (!type.IsPrimitive && type != typeof(string) && !type.IsEnum && !type.IsArray)
                    prefab = objectPrefab;
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"不支持的类型: {type}，字段 {field.Name} 被忽略");
#endif
                    return;
                }
            }

            GameObject instance = Instantiate(prefab, parent);
            TextMeshProUGUI label = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                if (translate.ContainsKey(field.Name))
                    label.text = translate[field.Name];
                else
                    label.text = field.Name;
            }

            // Tooltip
            TooltipAttribute tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
            if (tooltipAttr != null)
            {
                fieldTooltips[instance] = tooltipAttr.tooltip;
                AddTooltipEvents(instance);
            }
            PortraitAttribute portraitAttr = field.GetCustomAttribute<PortraitAttribute>();
            CharacterAttribute characterAttr = field.GetCustomAttribute<CharacterAttribute>();
            if (portraitAttr != null && portraitAttr.Show)
            {
                CreatePortrait((int i) => EditorManager.Instance.flow.characters[EditorManager.Instance.GetExistingCharacter(CommandListEditor.Instance.selectedIndex)[i]].portrait);
            }
            if (characterAttr != null && characterAttr.Show)
            {
                CreatePortrait((int i) => EditorManager.Instance.flow.characters[i].portrait);
            }
            void CreatePortrait(Func<int, Sprite[]> onChange)
            {
                try
                {
                    var portraits = onChange(Convert.ToInt32(field.GetValue(obj)));
                    TMP_InputField inputField = instance.GetComponentInChildren<TMP_InputField>();
                    ScrollRect scrollRect = Instantiate(scrollRectPrefab, parent);
                    var rect = scrollRect.transform.GetComponentInChildren<HorizontalLayoutGroup>().transform;
                    List<GameObject> ps = new List<GameObject>();
                    inputField.onEndEdit.AddListener(OnChange);
                    OnChange(Convert.ToInt32(field.GetValue(obj)).ToString());
                    void OnChange(string input)
                    {
                        if (int.TryParse(input, out int index))
                        {
                            foreach (var p in ps)
                            {
                                p.SetActive(false);
                            }
                            try
                            {
                                for (int i = ps.Count; i < portraits.Length; i++)
                                {
                                    ps.Add(Instantiate(portraitPrefab, rect));
                                    ps[i].GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
                                }
                                for (int i = 0; i < portraits.Length; i++)
                                {
                                    ps[i].SetActive(true);
                                    ps[i].GetComponent<Image>().sprite = portraits[i];
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception)
                { }
            }
            BindControl(instance, field, obj);
        }

        private void AddTooltipEvents(GameObject target)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null) trigger = target.AddComponent<EventTrigger>();

            EventTrigger.Entry enter = new EventTrigger.Entry();
            enter.eventID = EventTriggerType.PointerEnter;
            enter.callback.AddListener((data) => ShowTooltip(target));
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener((data) => HideTooltip());
            trigger.triggers.Add(exit);
        }

        private void ShowTooltip(GameObject target)
        {
            if (tooltipInstance == null || !fieldTooltips.TryGetValue(target, out string tip)) return;
            tooltipText.text = tip;
            tooltipInstance.SetActive(true);
            tooltipRect.position = target.transform.position + new Vector3(0, 50);
        }

        private void HideTooltip()
        {
            if (tooltipInstance != null) tooltipInstance.SetActive(false);
        }

        private GameObject GetPrefabForType(Type type)
        {
            if (type == typeof(int) || type == typeof(byte) || type == typeof(ushort) || type == typeof(short) ||
                type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
                return intPrefab;
            if (type == typeof(float) || type == typeof(double))
                return floatPrefab;
            if (type == typeof(bool))
                return boolPrefab;
            if (type == typeof(string))
                return stringPrefab;
            if (type.IsEnum)
                return enumPrefab;
            if (type.IsArray)
                return arrayPrefab;
            return null;
        }
        private void BindAudioButton(GameObject control, FieldInfo field, object obj,bool isBGM)
        {
            Button btn = control.GetComponent<Button>();
            if (btn == null) return;
            TextMeshProUGUI label = control.GetComponent<TextMeshProUGUI>();
            if (label == null) return;

            void UpdateDisplay()
            {
                string path = (string)field.GetValue(obj);
                var flow = EditorManager.Instance.flow;
                if (string.IsNullOrEmpty(path))
                {
                    label.text = "无音频";
                }
                else
                {
                    label.text = path;
                }
            }
            UpdateDisplay();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                var selectAudio = EditorManager.Instance.selectAudio;
                if (selectAudio != null)
                {
                    selectAudio.SetUI(isBGM);
                    selectAudio.SetDele((string selectedPath) =>
                    {
                        field.SetValue(obj, selectedPath);
                        UpdateDisplay();
                    });
                }
            });
        }
        private void BindBGButton(GameObject control, FieldInfo field, object obj)
        {
            Button btn = control.GetComponent<Button>();
            if (btn == null) return;
            Image img = control.GetComponent<Image>();
            if (img == null) return;
            // 更新显示
            void UpdateDisplay()
            {
                ushort index = (ushort)field.GetValue(obj);
                var flow = EditorManager.Instance.flow;
                if (index < flow.BGs.Count && flow.BGs[index] != null)
                {
                    img.sprite = flow.BGs[index];
                    img.enabled = true;
                }
                else
                {
                    img.sprite = null;
                }
            }
            UpdateDisplay();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                // 打开背景选择面板
                var selectBG = EditorManager.Instance.selectBG;
                if (selectBG != null)
                {
                    selectBG.SetUI();
                    selectBG.SetDele((ushort selectedIndex) =>
                    {
                        field.SetValue(obj, (ushort)selectedIndex);
                        UpdateDisplay();
                    });
                }
            });
        }

        private void BindCharacterButton(GameObject control, FieldInfo field, object obj)
        {
            Button btn = control.GetComponent<Button>();
            if (btn == null) return;
            TextMeshProUGUI label = control.GetComponent<TextMeshProUGUI>();
            if (label == null) return;

            void UpdateDisplay()
            {
                ushort index = (ushort)field.GetValue(obj);
                var flow = EditorManager.Instance.flow;
                if (index < flow.characters.Count && flow.characters[index] != null)
                {
                    label.text = flow.characters[index].name;
                }
                else
                {
                    label.text = "无角色";
                }
            }
            UpdateDisplay();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                var selectChar = EditorManager.Instance.selectCharacter;
                if (selectChar != null)
                {
                    selectChar.SetUI();
                    selectChar.SetDele((ushort selectedIndex) =>
                    {
                        field.SetValue(obj, selectedIndex);
                        UpdateDisplay();
                    });
                }
            });
        }

        private void BindControl(GameObject control, FieldInfo field, object obj)
        {
            Type type = field.FieldType;
            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

            //整数
            if (type == typeof(int) || type == typeof(byte) || type == typeof(ushort) || type == typeof(short) ||
                type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
            {
                var input = control.GetComponentInChildren<TMP_InputField>();
                if (input == null) return;
                int current = Convert.ToInt32(field.GetValue(obj));
                input.text = current.ToString();

                int min, max;
                if (range != null) { min = (int)range.min; max = (int)range.max; }
                else { min = GetMinValue(type); max = GetMaxValue(type); }

                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((string newVal) =>
                {
                    if (int.TryParse(newVal, out int parsed))
                    {
                        parsed = Math.Clamp(parsed, min, max);
                        field.SetValue(obj, Convert.ChangeType(parsed, type));
                        input.text = parsed.ToString();
                    }
                    else
                    {
                        int cur = Convert.ToInt32(field.GetValue(obj));
                        input.text = cur.ToString();
                    }
                });
            }

            //浮点
            else if (type == typeof(float) || type == typeof(double))
            {
                var input = control.GetComponentInChildren<TMP_InputField>();
                if (input == null) return;
                float current = Convert.ToSingle(field.GetValue(obj));
                input.text = current.ToString("F2");

                float min, max;
                if (range != null) { min = range.min; max = range.max; }
                else { min = float.MinValue; max = float.MaxValue; }

                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((string newVal) =>
                {
                    if (float.TryParse(newVal, out float parsed))
                    {
                        parsed = Math.Clamp(parsed, min, max);
                        field.SetValue(obj, Convert.ChangeType(parsed, type));
                        input.text = parsed.ToString("F2");
                    }
                    else
                    {
                        float cur = Convert.ToSingle(field.GetValue(obj));
                        input.text = cur.ToString("F2");
                    }
                });
            }

            //布尔
            else if (type == typeof(bool))
            {
                var toggle = control.GetComponentInChildren<Toggle>();
                if (toggle == null) return;
                toggle.isOn = (bool)field.GetValue(obj);
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((bool val) => field.SetValue(obj, val));
            }

            //字符串
            else if (type == typeof(string))
            {
                var input = control.GetComponentInChildren<TMP_InputField>();
                if (input == null) return;
                input.text = (string)field.GetValue(obj);
                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((string val) => field.SetValue(obj, val));
            }

            //枚举
            else if (type.IsEnum)
            {
                var dropdown = control.GetComponentInChildren<TMP_Dropdown>();
                if (dropdown == null) return;

                // 获取所有枚举名称和值（顺序一致）
                string[] names = Enum.GetNames(type);
                Array values = Enum.GetValues(type);

                dropdown.ClearOptions();
                dropdown.AddOptions(new List<string>(names));

                // 当前枚举值
                object currentEnum = field.GetValue(obj);
                // 找到对应索引
                int currentIndex = Array.IndexOf(values, currentEnum);
                if (currentIndex < 0) currentIndex = 0;

                dropdown.value = currentIndex;
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener((int idx) =>
                {
                    object enumVal = values.GetValue(idx);
                    field.SetValue(obj, enumVal);
                });
            }
            //数组
            else if (type.IsArray)
            {
                BindArray(control, field, obj);
            }
            else
            {
                Transform content = control.transform.Find("Content");
                if (content == null) return;
                object child = field.GetValue(obj);
                if (child == null)
                {
                    try { child = Activator.CreateInstance(type); }
                    catch { Debug.LogError($"无法创建 {type} 实例"); return; }
                    field.SetValue(obj, child);
                }
                GenerateFields(type, content, child);
            }
        }

        //数组处理

        private void BindArray(GameObject control, FieldInfo field, object obj)
        {
            Transform content = control.transform.Find("Content");
            Button addBtn = control.GetComponentInChildren<Button>();
            if (content == null || addBtn == null) return;

            foreach (Transform child in content) Destroy(child.gameObject);

            Array array = (Array)field.GetValue(obj);
            Type elemType = field.FieldType.GetElementType();
            if (array is Command[] commands)
            {
                for (int i = 0; i < commands.Length; i++)
                {
                    var index = i;
                    var commandItem = Instantiate(CommandListEditor.Instance.commandItemPrefab, content);
                    commandItem.Initialize(commands[index], () =>
                    {
                        var list = ListPool<Command>.Get();
                        list.AddRange(commands);
                        list.RemoveAt(index);
                        field.SetValue(obj, list.ToArray());
                        ListPool<Command>.Release(list);
                        BindArray(control, field, obj);
                    }, () =>
                    {
                        var list = ListPool<Command>.Get();
                        list.AddRange(commands);
                        list.Insert(index, (Command)Activator.CreateInstance(Save.commandType[CommandListEditor.Instance.typeDropdown.value]));
                        field.SetValue(obj, list.ToArray());
                        ListPool<Command>.Release(list);
                        BindArray(control, field, obj);
                    }, () => BuildUI(commands[index])
                    );
                }
                addBtn.onClick.RemoveAllListeners();
                addBtn.onClick.AddListener(() =>
                {
                    Command[] oldArray = (Command[])field.GetValue(obj);
                    int oldLen = oldArray?.Length ?? 0;
                    Command[] newArray = new Command[oldLen + 1];
                    if (oldArray != null) Array.Copy(oldArray, newArray, oldLen);
                    newArray[oldLen] = (Command)Activator.CreateInstance(Save.commandType[CommandListEditor.Instance.typeDropdown.value]);
                    field.SetValue(obj, newArray);
                    BindArray(control, field, obj);
                });
                return;
            }
            else if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object elem = array.GetValue(i);
                    GameObject elemPrefab = GetPrefabForType(elemType);
                    if (elemPrefab == null) elemPrefab = objectPrefab;
                    GameObject elemGo = Instantiate(elemPrefab, content);
                    TextMeshProUGUI label = elemGo.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null) label.text = $"[{i}]";

                    // 删除按钮
                    GameObject delBtnGO = new GameObject("DeleteBtn", typeof(RectTransform), typeof(Button), typeof(Image));
                    delBtnGO.transform.SetParent(elemGo.transform, false);
                    var btnRect = delBtnGO.GetComponent<RectTransform>();
                    btnRect.anchorMin = new Vector2(1, 0.5f);
                    btnRect.anchorMax = new Vector2(1, 0.5f);
                    btnRect.pivot = new Vector2(1, 0.5f);
                    btnRect.sizeDelta = new Vector2(30, 30);
                    btnRect.anchoredPosition = new Vector2(-5, 0);

                    Button btn = delBtnGO.GetComponent<Button>();
                    int idx = i;
                    btn.onClick.AddListener(() =>
                    {
                        Array oldArray = (Array)field.GetValue(obj);
                        int oldLen = oldArray.Length;
                        Array newArray = Array.CreateInstance(elemType, oldLen - 1);
                        if (idx > 0)
                            Array.Copy(oldArray, 0, newArray, 0, idx);
                        if (idx < oldLen - 1)
                            Array.Copy(oldArray, idx + 1, newArray, idx, oldLen - idx - 1);
                        field.SetValue(obj, newArray);
                        BindArray(control, field, obj);
                    });

                    // 绑定元素值
                    BindArrayElement(elemGo, elemType, elem, array, i);
                }
            }
            addBtn.onClick.RemoveAllListeners();
            addBtn.onClick.AddListener(() =>
            {
                Array oldArray = (Array)field.GetValue(obj);
                int oldLen = oldArray?.Length ?? 0;
                Array newArray = Array.CreateInstance(elemType, oldLen + 1);
                if (oldArray != null) Array.Copy(oldArray, newArray, oldLen);
                newArray.SetValue(Activator.CreateInstance(elemType), oldLen);
                field.SetValue(obj, newArray);
                BindArray(control, field, obj);
            });
        }

        private void BindArrayElement(GameObject control, Type elemType, object elem, Array array, int index)
        {
            Func<object> getter = () => array.GetValue(index);
            Action<object> setter = (val) => array.SetValue(val, index);

            if (elemType == typeof(int) || elemType == typeof(byte) || elemType == typeof(ushort) || elemType == typeof(short) ||
                elemType == typeof(uint) || elemType == typeof(long) || elemType == typeof(ulong))
            {
                var input = control.GetComponentInChildren<TMP_InputField>();
                if (input == null) return;
                int current = Convert.ToInt32(getter());
                input.text = current.ToString();
                int min = GetMinValue(elemType);
                int max = GetMaxValue(elemType);
                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((string newVal) =>
                {
                    if (int.TryParse(newVal, out int parsed))
                    {
                        parsed = Math.Clamp(parsed, min, max);
                        setter(Convert.ChangeType(parsed, elemType));
                        input.text = parsed.ToString();
                    }
                    else
                    {
                        int cur = Convert.ToInt32(getter());
                        input.text = cur.ToString();
                    }
                });
            }
            else if (elemType == typeof(float) || elemType == typeof(double))
            {
                var input = control.GetComponentInChildren<TMP_InputField>();
                if (input == null) return;
                float current = Convert.ToSingle(getter());
                input.text = current.ToString("F2");
                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((string newVal) =>
                {
                    if (float.TryParse(newVal, out float parsed))
                    {
                        setter(Convert.ChangeType(parsed, elemType));
                        input.text = parsed.ToString("F2");
                    }
                    else
                    {
                        float cur = Convert.ToSingle(getter());
                        input.text = cur.ToString("F2");
                    }
                });
            }
            else if (elemType == typeof(bool))
            {
                var toggle = control.GetComponentInChildren<Toggle>();
                if (toggle == null) return;
                toggle.isOn = (bool)getter();
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((bool val) => setter(val));
            }
            else if (elemType == typeof(string))
            {
                var input = control.GetComponentInChildren<TMP_InputField>();
                if (input == null) return;
                input.text = (string)getter();
                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((string val) => setter(val));
            }
            else if (elemType.IsEnum)
            {
                var dropdown = control.GetComponentInChildren<TMP_Dropdown>();
                if (dropdown == null) return;

                string[] names = Enum.GetNames(elemType);
                Array values = Enum.GetValues(elemType);

                dropdown.ClearOptions();
                dropdown.AddOptions(new List<string>(names));

                object currentEnum = getter();
                int currentIndex = Array.IndexOf(values, currentEnum);
                if (currentIndex < 0) currentIndex = 0;

                dropdown.value = currentIndex;
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener((int idx) =>
                {
                    object enumVal = values.GetValue(idx);
                    setter(enumVal);
                });
            }
            else
            {
                Transform content = control.transform.Find("Content");
                if (content != null)
                {
                    object child = getter();
                    if (child == null)
                    {
                        try { child = Activator.CreateInstance(elemType); }
                        catch { Debug.LogError($"无法创建元素实例 {elemType}"); return; }
                        setter(child);
                    }
                    GenerateFields(elemType, content, child);
                }
            }
        }

        //获取数值类型的最小值和最大值

        private int GetMinValue(Type type)
        {
            FieldInfo field = type.GetField("MinValue");
            return field != null ? Convert.ToInt32(field.GetValue(null)) : 0;
        }

        private int GetMaxValue(Type type)
        {
            FieldInfo field = type.GetField("MaxValue");
            return field != null ? Convert.ToInt32(field.GetValue(null)) : int.MaxValue;
        }
    }
}