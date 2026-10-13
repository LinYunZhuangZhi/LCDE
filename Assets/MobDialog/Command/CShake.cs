using DG.Tweening;
namespace MobDialog
{
    public class CShake : TweenCommand
    {
        public float duration, strengthX, strengthY,randomness = 90;
        public int vibrato = 10;
        [Portrait]
        public byte portrait;
        public bool fadeOut = true;
        public override void Execute()
        {
            tween= CharacterManager.Instance.portraits[portrait].Shake(duration,new UnityEngine.Vector2(strengthX,strengthY),vibrato,randomness,false,fadeOut).OnComplete(new TweenCallback(Next));
        }

        public override void Exit()
        {
            
        }
    }
}