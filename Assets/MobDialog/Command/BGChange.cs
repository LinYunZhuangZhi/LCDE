using UnityEngine;
namespace MobDialog
{
    public class BGChange : TweenCommand
    {
        [BG]
        public ushort bgIndex;              
        public override void Execute()
        {
            Sprite sprite = DialogManager.Instance.flow.BGs[bgIndex];
            DialogManager.Instance.bg.ChangeBG(sprite);
            Next();
        }
        public override void Exit() { }
    }
}