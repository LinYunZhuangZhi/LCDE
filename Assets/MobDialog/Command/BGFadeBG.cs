using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class BGFadeBG : BGChange
    {
        public float time;
        public override void Execute()
        {
            Sprite sprite = DialogManager.Instance.flow.BGs[bgIndex];
            tween = DialogManager.Instance.bg.FadeBG(sprite, time).OnComplete(() => Next());
        }
        public override void Exit() { }
    }
}