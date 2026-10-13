using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class UIHoverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent onEnter;
    public UnityEvent onExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit?.Invoke();
    }
}