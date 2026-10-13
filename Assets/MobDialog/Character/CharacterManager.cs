using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace MobDialog
{
    public class CharacterManager:MonoSingleton<CharacterManager>
    {
        public ObjectPool<Portrait> portraitPool;
        public List<Portrait> portraits;
        public Portrait portrait;
        public Sprite[] emos;
        protected void Start()
        {
            portraitPool = new(CreatePortrait,GetPortrait,ReleasePortrait);
            portraitPool.Release(CreatePortrait());
            portraitPool.Release(CreatePortrait());
        }
        public Portrait CreatePortrait()
        {
            return Instantiate(portrait,DialogManager.Instance.backParent.parent);
        }
        public void GetPortrait(Portrait portrait)
        {
            portrait.gameObject.SetActive(true);
            portraits.Add(portrait);
        }
        public void ReleasePortrait(Portrait portrait)
        {
            portraits.Remove(portrait);
            portrait.gameObject.SetActive(false);
        }
        public Portrait CreateCharacter(Character character)
        {
            var p = portraitPool.Get();
            p.character = character;
            p.express.rectTransform.anchoredPosition = character.express;
            return p;
        }
        public Portrait CreateCharacter(Character character,int i)
        {
            var p = portraitPool.Get();
            p.character = character;
            p.express.rectTransform.anchoredPosition = character.express;
            p.ChangePortrait(i);
            return p;
        }
        public Portrait CreateCharacter(Character character, int i,float a)
        {
            var p = portraitPool.Get();
            p.character = character;
            p.express.rectTransform.anchoredPosition = character.express;
            p.ChangePortrait(i);
            p.MoveTo(a);
            return p;
        }
        public void Emo(byte p,byte e)
        {
            if (p<portraits.Count&&e<emos.Length)
            {
                Image emo = portraits[p].express;
                emo.sprite = emos[e];
                DOTween.Sequence(emo).Append(emo.DOFade(1, 0.3f)).Append(emo.DOFade(1, 0.6f)).Append(emo.DOFade(0, 0.3f));
            }
        }
    }
}