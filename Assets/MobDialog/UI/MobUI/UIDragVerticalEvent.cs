using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
public class UIDragVerticalEvent : MonoBehaviour, IDragHandler,IEndDragHandler
{
    public UnityEvent<float> onDrag;
    public UnityEvent onEnd;
    public void OnDrag(PointerEventData eventData)
    {
        float y = eventData.delta.y;
        if (Mathf.Approximately(y, 0f))
            return;

        onDrag?.Invoke(y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEnd?.Invoke();
    }
}
