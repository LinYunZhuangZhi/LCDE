using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MobDialog
{
    public class Portrait : MonoBehaviour
    {
        public Character character;
        public RectTransform rectTransform;
        public Image image,express;

        public DG.Tweening.Core.TweenerCore<Vector2, Vector2, DG.Tweening.Plugins.Options.VectorOptions> MoveTo(float p,float t)
        {
            if (t<=0)
            {
                var v=rectTransform.anchoredPosition;
                v.x = p;
                rectTransform.anchoredPosition =v;
                return null;
            }
            else
            {
                return rectTransform.DOAnchorPosX(p,t);
            }
        }

        public void MoveTo(float p)
        {
            var v = rectTransform.anchoredPosition;
            v.x = p;
            rectTransform.anchoredPosition = v;
        }
        public void ChangePortrait(int i)
        {
            if (i>=character.portrait.Length)
            {
                return;
            }
            image.sprite = character.portrait[i];
            RectTransform rt = image.rectTransform;
            Rect rect = character.rect[i];
            rt.anchoredPosition = new Vector2(rect.x, rect.y);
            rt.sizeDelta = new Vector2(rect.width, rect.height);
        }
        public Tweener Shake(float duration, Vector2 strength, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true)
        {
            return image.rectTransform.DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping, fadeOut);
        }
        public DG.Tweening.Core.TweenerCore<Vector2, Vector2, DG.Tweening.Plugins.Options.VectorOptions> Jump(float high,float time,int vibrato=1)
        {
             return image.rectTransform.DOAnchorPosY(image.rectTransform.anchoredPosition.y+high,time*0.5f).SetEase(Ease.OutCirc).SetLoops(vibrato,LoopType.Yoyo);
        }
        public DG.Tweening.Core.TweenerCore<Color, Color, DG.Tweening.Plugins.Options.ColorOptions> Fade(float a,float t)
        {
            return image.DOFade(a, t);
        }
    }
}