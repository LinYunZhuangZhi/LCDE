using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class CJump : TweenCommand
    {
        public float high;
        public float time;
        [Tooltip("单数跳起不落下，上去下来各占一次。")]
        public int vibrato = 1;
        [Portrait]
        public byte portrait;

        public override void Execute()
        {
            if (time>0)
            {
                tween = CharacterManager.Instance.portraits[portrait].Jump(high, time, vibrato)
                    .OnComplete(new TweenCallback(Next));
            }
            else
            {
                if (vibrato%2==1)
                {
                    RectTransform rectTransform = CharacterManager.Instance.portraits[portrait].image.rectTransform;
                    rectTransform.anchoredPosition=new(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y+ high);
                }
                Next();
            }
        }

        public override void Exit()
        {

        }
    }
}