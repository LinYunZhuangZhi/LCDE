using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIDragEvent : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public UnityEvent<Vector2> onDrag;
    public UnityEvent onEnd;
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.delta==Vector2.zero)
            return;

        onDrag?.Invoke(eventData.delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEnd?.Invoke();
    }
}
