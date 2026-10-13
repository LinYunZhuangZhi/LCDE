using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class BGMove : TweenCommand
    {
        public float targetX, targetY;
        public float duration;
        public bool withCharacter;
        public override void Execute()
        {
            tween = DialogManager.Instance.bg.Move(new Vector2(targetX, targetY), duration, withCharacter).OnComplete(() => Next());
        }
        public override void Exit() { }
    }
}