using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace MobDialog
{
    public class CommandItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI typeNameText;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button insertButton;
        [SerializeField] private Image backgroundImage;
        private Action onPointerClick;
        //主列表用的
        public void Initialize(Command cmd, int idx, CommandListEditor editor)
        {

            typeNameText.text = Save.commandTypeDisplayTexts[Array.IndexOf<Type>(Save.commandType, cmd.GetType())];

            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => editor.DeleteItem(idx));

            insertButton.onClick.RemoveAllListeners();
            insertButton.onClick.AddListener(() => editor.InsertBefore(idx));
            onPointerClick=()=>editor.SelectItem(idx);
        }
        //命令组列表用的
        public void Initialize(Command cmd,UnityAction delete,UnityAction insert,Action click)
        {
            typeNameText.text = cmd.GetType().Name;
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(delete);

            insertButton.onClick.RemoveAllListeners();
            insertButton.onClick.AddListener(insert);
            onPointerClick = click;
        }

        public void SetHighlight(bool highlight)
        {
            backgroundImage.color = highlight ? Color.grey : Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onPointerClick?.Invoke();
        }
    }
}