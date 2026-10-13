using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MobUI
{
    public class DynamicFieldUIBase : MonoBehaviour
    {
        [SerializeField] protected GameObject headerPrefab;
        [SerializeField] protected GameObject intPrefab;
        [SerializeField] protected GameObject floatPrefab;
        [SerializeField] protected GameObject boolPrefab;
        [SerializeField] protected GameObject stringPrefab;
        [SerializeField] protected GameObject textAreaPrefab;
        [SerializeField] protected GameObject enumPrefab;
        [SerializeField] protected GameObject arrayPrefab;
        [SerializeField] protected GameObject objectPrefab;
        [SerializeField] protected TextMeshProUGUI tooltipText;
        [SerializeField] protected GameObject tooltipInstance;
        [SerializeField] protected RectTransform tooltipRect;
        [SerializeField] protected RectTransform uiContainer;

        protected Dictionary<GameObject, string> fieldTooltips = new Dictionary<GameObject, string>();

        protected object targetObject;

        public virtual void BuildUI(object target)
        {
            if (target == null)
                return; 
            targetObject = target;
            ClearUI();
            GenerateFields(target.GetType(), uiContainer, target);
        }

        public virtual void ClearUI()
        {
            foreach (Transform child in uiContainer)
                Destroy(child.gameObject);
            fieldTooltips.Clear();
            HideTooltip();
        }

        // ----------------------- 核心生成 -----------------------

        protected virtual void GenerateFields(Type type, Transform parent, object obj)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<HideInInspector>() != null)
                    continue;

                var header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null)
                    CreateHeader(header.header, parent);

                // 让子类有机会处理特殊属性（如 BG、Character 等）
                if (TryHandleSpecialAttribute(field, parent, obj))
                    continue;

                CreateFieldControl(field, obj, parent);
            }
        }

        protected virtual bool TryHandleSpecialAttribute(FieldInfo field, Transform parent, object obj)
        {
            // 基类不做特殊处理，返回 false
            return false;
        }

        protected void CreateHeader(string text, Transform parent)
        {
            if (headerPrefab == null) return;
            var go = Instantiate(headerPrefab, parent);
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = text;
        }

        protected virtual void CreateFieldControl(FieldInfo field, object obj, Transform parent)
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
                    Debug.LogWarning($"不支持的类型: {type}，字段 {field.Name} 被忽略");
                    return;
                }
            }

            GameObject instance = Instantiate(prefab, parent);
            TextMeshProUGUI label = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = GetFieldDisplayName(field.Name);

            // Tooltip
            TooltipAttribute tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
            if (tooltipAttr != null)
            {
                fieldTooltips[instance] = tooltipAttr.tooltip;
                AddTooltipEvents(instance);
            }

            BindControl(instance, field, obj);
        }

        /// <summary>
        /// 子类可重写此方法以提供字段名的本地化或翻译。
        /// </summary>
        protected virtual string GetFieldDisplayName(string fieldName) => fieldName;

        protected void AddTooltipEvents(GameObject target)
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

        protected void ShowTooltip(GameObject target)
        {
            if (tooltipInstance == null || !fieldTooltips.TryGetValue(target, out string tip)) return;
            tooltipText.text = tip;
            tooltipInstance.SetActive(true);
            if (tooltipRect != null)
                tooltipRect.position = target.transform.position + new Vector3(0, 50);
        }

        protected void HideTooltip()
        {
            if (tooltipInstance != null) tooltipInstance.SetActive(false);
        }

        // ----------------------- 预制体选择 -----------------------

        protected virtual GameObject GetPrefabForType(Type type)
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

        // ----------------------- 控件绑定 -----------------------

        protected virtual void BindControl(GameObject control, FieldInfo field, object obj)
        {
            Type type = field.FieldType;
            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();

            // ----- 整数 -----
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

            // ----- 浮点 -----
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

            // ----- 布尔 -----
            else if (type == typeof(bool))
            {
                var toggle = control.GetComponentInChildren<Toggle>();
                if (toggle == null) return;
                toggle.isOn = (bool)field.GetValue(obj);
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((bool val) => field.SetValue(obj, val));
            }

            // ----- 字符串 -----
            else if (type == typeof(string))
            {
                var input = control.GetComponentInChildren<TMP_InputField>();
                if (input == null) return;
                input.text = (string)field.GetValue(obj);
                input.onValueChanged.RemoveAllListeners();
                input.onValueChanged.AddListener((string val) => field.SetValue(obj, val));
            }

            // ----- 枚举 -----
            else if (type.IsEnum)
            {
                var dropdown = control.GetComponentInChildren<TMP_Dropdown>();
                if (dropdown == null) return;

                string[] names = Enum.GetNames(type);
                Array values = Enum.GetValues(type);

                dropdown.ClearOptions();
                dropdown.AddOptions(new List<string>(names));

                object currentEnum = field.GetValue(obj);
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

            // ----- 数组 -----
            else if (type.IsArray)
            {
                BindArray(control, field, obj);
            }

            // ----- 嵌套对象 -----
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

        // ----------------------- 数组处理 -----------------------

        protected virtual void BindArray(GameObject control, FieldInfo field, object obj)
        {
            Transform content = control.transform.Find("Content");
            Button addBtn = control.GetComponentInChildren<Button>();
            if (content == null || addBtn == null) return;

            foreach (Transform child in content) Destroy(child.gameObject);

            Array array = (Array)field.GetValue(obj);
            Type elemType = field.FieldType.GetElementType();

            // 让子类有机会处理特殊数组类型（如 Command[]）
            if (array != null && TryHandleSpecialArray(array, field, obj, content, addBtn))
                return;

            if (array != null)
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

        /// <summary>
        /// 子类可重写此方法以处理特殊数组（如 Command[]），返回 true 表示已处理，不再执行默认逻辑。
        /// </summary>
        protected virtual bool TryHandleSpecialArray(Array array, FieldInfo field, object obj, Transform content, Button addBtn)
        {
            return false;
        }

        protected virtual void BindArrayElement(GameObject control, Type elemType, object elem, Array array, int index)
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

        // ----------------------- 辅助方法 -----------------------

        protected int GetMinValue(Type type)
        {
            FieldInfo field = type.GetField("MinValue");
            return field != null ? Convert.ToInt32(field.GetValue(null)) : 0;
        }

        protected int GetMaxValue(Type type)
        {
            FieldInfo field = type.GetField("MaxValue");
            return field != null ? Convert.ToInt32(field.GetValue(null)) : int.MaxValue;
        }
    }
}