using UnityEngine;

namespace MobDialog
{
    public class CFront : Command
    {
        [Portrait]
        public byte portrait;
        [Tooltip("如果隐藏其他角色，则说话后或下次调用角色前置不选择隐藏恢复")]
        public bool hideOthers;
        public override void Execute()
        {
            if (hideOthers)
            {
                foreach (var p in CharacterManager.Instance.portraits)
                {
                    p.image.color = Color.gray;
                }
            }
            else
            {
                foreach (var p in CharacterManager.Instance.portraits)
                {
                    p.image.color = Color.white;
                }
            }
            if (CharacterManager.Instance.portraits.Count>portrait)
            { 
                CharacterManager.Instance.portraits[portrait].rectTransform.SetAsLastSibling();
                CharacterManager.Instance.portraits[portrait].image.color=Color.white;
            }
            Next();
        }

        public override void Exit()
        {
        }
    }
}