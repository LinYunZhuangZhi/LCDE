using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class BGFadeOut : BGChange
    {
        public float r, g, b, time;
        public override void Execute()
        {
            Sprite sprite = DialogManager.Instance.flow.BGs[bgIndex];
            tween = DialogManager.Instance.bg.FadeOut(sprite, r, g, b, time).OnComplete(new(Next));
        }
        public override void Exit() { }
    }
}