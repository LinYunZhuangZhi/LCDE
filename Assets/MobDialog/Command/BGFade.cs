using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class BGFade : BGChange
    {
        public float r, g, b, time;
        public override void Execute()
        {
            Sprite sprite = DialogManager.Instance.flow.BGs[bgIndex];
            Sequence seq = DialogManager.Instance.bg.Fade(sprite, r, g, b, time);
            seq.OnComplete(() => Next());
            tween = seq;
        }
        public override void Exit() { }
    }
}