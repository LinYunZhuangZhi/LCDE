using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class BGShakePos : TweenCommand
    {
        public float duration;
        public float strengthX, strengthY;  
        public int vibrato = 10;
        public float randomness = 90;
        public bool fadeOut = true;
        public bool random = true;
        public bool withCharacter = false;
        public override void Execute()
        {
            tween = DialogManager.Instance.bg.ShakePos(duration, new Vector2(strengthX, strengthY), vibrato, randomness, fadeOut, random, withCharacter)
                .OnComplete(() => Next());
        }
        public override void Exit() { }
    }
}