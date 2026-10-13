using DG.Tweening;
namespace MobDialog
{
    public class BGShakeRotation : TweenCommand
    {
        public float duration;
        public float strength;
        public int vibrato = 10;
        public float randomness = 90;
        public bool fadeOut = true;
        public bool random = true;
        public bool withCharacter = false;
        public override void Execute()
        {
            tween = DialogManager.Instance.bg.ShakeRotation(duration, strength, vibrato, randomness, fadeOut, random, withCharacter)
                .OnComplete(() => Next());
        }
        public override void Exit() { }
    }
}