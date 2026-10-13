using DG.Tweening;
namespace MobDialog
{
    public class COut:TweenCommand
    {
        [Portrait]
        public ushort portrait;
        public float time;
        public override void Execute()
        {
            Portrait p;
            if (time<=0)
            {
                p = CharacterManager.Instance.portraits[portrait];
                CharacterManager.Instance.portraitPool.Release(p);
                Next();
            }
            else
            {
                p = CharacterManager.Instance.portraits[portrait];
                CharacterManager.Instance.portraitPool.Release(p);
                p.gameObject.SetActive(true);
                tween = p.Fade(0, time).OnComplete(()=> p.gameObject.SetActive(false)).OnComplete(new TweenCallback(Next));
            }
        }
        public override void Exit()
        {
        }
    }
}