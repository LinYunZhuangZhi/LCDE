using DG.Tweening;
using UnityEngine;
namespace MobDialog
{
    public class CIn:TweenCommand
    {
        [Character(true)]
        public ushort character;
        public byte face;
        public float place, time;
        public override void Execute()
        {
            CharacterManager.Instance.CreateCharacter(DialogManager.Instance.flow.characters[character], face, place);
            if (time<=0)
            {
                CharacterManager.Instance.portraits[CharacterManager.Instance.portraits.Count-1].image.color = Color.white;
                Next();
            }
            else
            {
                CharacterManager.Instance.portraits[CharacterManager.Instance.portraits.Count-1].image.color= new Color(1,1,1,0);
                tween = CharacterManager.Instance.portraits[CharacterManager.Instance.portraits.Count-1].Fade(1, time).OnComplete(new TweenCallback(Next));
            }
        }
        public override void Exit()
        {
            
        }
    }
}