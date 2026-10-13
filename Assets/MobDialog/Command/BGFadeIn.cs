using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class BGFadeIn : BGChange
    {
        public float r, g, b, time;
        public override void Execute()
        {
            Sprite sprite = DialogManager.Instance.flow.BGs[bgIndex];
            tween = DialogManager.Instance.bg.FadeIn(sprite, r, g, b, time).OnComplete(() => Next());
        }
        public override void Exit() { }
    }
}