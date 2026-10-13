using DG.Tweening;
namespace MobDialog
{
    public class CMove : TweenCommand
    {
        [Portrait]
        public byte portrait;
        public float position, time;
        public Ease ease;
        public override void Execute()
        {
            if (time<=0)
            {
                CharacterManager.Instance.portraits[portrait].MoveTo(position);
                Next();
            }
            else
            {
                 tween = CharacterManager.Instance.portraits[portrait].MoveTo(position, time).OnComplete(new TweenCallback(Next)).SetEase(ease);
            }
        }

        public override void Exit()
        {
            
        }

    }
}