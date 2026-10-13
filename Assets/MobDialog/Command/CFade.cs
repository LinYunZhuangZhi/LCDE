using DG.Tweening;
namespace MobDialog
{
    public class CFade : TweenCommand
    {
        [Portrait]
        public ushort portrait;
        public float alpha, time;
        public override void Execute()
        {
            tween= CharacterManager.Instance.portraits[portrait].Fade(alpha, time).OnComplete(new TweenCallback(Next));
        }

        public override void Exit()
        {
            
        }
    }
}