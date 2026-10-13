using System;
using TMPro;
using UnityEngine;
namespace MobDialog
{
    public class InputRectUI:MonoBehaviour
    {
        public TMP_InputField x,y,width,height;
        public Action<Rect> OnChange;
        public void SetRect(Rect rect)
        {
            x.text=rect.x.ToString();
            y.text=rect.y.ToString();
            width.text=rect.width.ToString();
            height.text=rect.height.ToString();
        }
        public void Change()
        {
            if (float.TryParse(x.text, out float xVal) &&
    float.TryParse(y.text, out float yVal) &&
    float.TryParse(width.text, out float widthVal) &&
    float.TryParse(height.text, out float heightVal))
            {
                OnChange?.Invoke(new(xVal, yVal, widthVal, heightVal));
            }
        }
    }
}
