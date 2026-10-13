using DG.Tweening;
using UnityEngine;

namespace MobDialog
{
    public class Say : TweenCommand,IWaitClick
    {
        [Character]
        [Tooltip("若需无角色则把此值设为超过角色数量，最大65535")]
        public ushort character=ushort.MaxValue;
        [TextArea]
        public string conversation;
        public bool hideOthers=true;
        public TMPro.HorizontalAlignmentOptions horizontal=TMPro.HorizontalAlignmentOptions.Left;
        public TMPro.VerticalAlignmentOptions vertical=TMPro.VerticalAlignmentOptions.Top;

        public bool Complete => complete;
        bool complete = false;

        public override void Execute()
        {
            DialogManager.Instance.conversationPanel.SetActive(true);
            DialogManager.Instance.conversation.horizontalAlignment = horizontal;
            DialogManager.Instance.conversation.verticalAlignment = vertical;
            if (character< DialogManager.Instance.flow.characters.Length)
            {
                DialogManager.Instance.namePanel.SetActive(true);
                DialogManager.Instance.SetCharacter(character);
                if (hideOthers)
                    foreach (var p in CharacterManager.Instance.portraits)
                    {
                        if (p.character == DialogManager.Instance.flow.characters[character])
                            p.rectTransform.SetAsLastSibling();
                        else
                            p.image.color = Color.gray;
                    }
            }
            else
            {
                DialogManager.Instance.namePanel.SetActive(false);
                if (hideOthers)
                {
                    foreach (var p in CharacterManager.Instance.portraits)
                    {
                        p.image.color = Color.gray;
                    }
                }
            }
            DialogManager.Instance.conversation.text = conversation;
            DialogManager.Instance.conversation.ForceMeshUpdate();
            DialogManager.Instance.conversation.maxVisibleCharacters = 0;
            int characterCount = DialogManager.Instance.conversation.textInfo.characterCount;
            float time = characterCount * Setting.instance.CharDelay;
            tween =DOTween.To(() => 0, x => DialogManager.Instance.conversation.maxVisibleCharacters = x, characterCount,time).SetEase(Ease.Linear).SetTarget(DialogManager.Instance.conversation.text);
            tween.OnComplete(()=>complete=true);
        }
        
        public override void Exit()
        {
            DialogManager.Instance.conversationPanel.SetActive(false);
            foreach (var p in CharacterManager.Instance.portraits)
            {
                p.image.color = Color.white;
            }
        }

    }
}