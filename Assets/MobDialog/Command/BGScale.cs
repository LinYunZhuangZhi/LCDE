using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class BGScale : TweenCommand
    {
        public float targetX=1, targetY=1; 
        public float duration;
        public bool withCharacter;
        public override void Execute()
        {
            tween = DialogManager.Instance.bg.Scale(new Vector2(targetX, targetY), duration, withCharacter).OnComplete(() => Next());
        }
        public override void Exit() { }
    }
}