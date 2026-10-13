using DG.Tweening;

namespace MobDialog
{
    public abstract class TweenCommand: Command,ISkipable
    {
        internal Tween tween;
        public virtual void Skip()
        {
            tween.Complete(true);
        }
    }
}