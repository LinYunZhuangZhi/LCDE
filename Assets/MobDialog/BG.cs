using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
public class BG : MonoBehaviour
{
    public int index;
    public Image[] BGs;
    public RectTransform rect,rectP;
    public TMPro.TextMeshProUGUI BGName;
    public GameObject BGNamePanel;
    public VideoPlayer video;
    public Image Overlay;
    public void ChangeBG(Sprite bg)
    {
        if (bg!=null)
            BGs[index].sprite = bg;
    }
    public Tweener FadeBG(Sprite bg, float time)
    {
        BGs[index].rectTransform.SetAsLastSibling();
        BGs[1-index].sprite = bg;
        index = 1 - index;
        var c = BGs[index].color;
        c.a = 1;
        BGs[index].color = c;
        return BGs[1-index].DOFade(0,time);
    }
    public Sequence Fade(Sprite sprite,float r,float g,float b,float time)
    {
        Overlay.gameObject.SetActive(true);
        Overlay.color=new Color(r,g,b,0);
        return DOTween.Sequence(BGs[index]).Append(Overlay.DOFade(1, time).OnComplete(
            ()=> ChangeBG(sprite))).Append(Overlay.DOFade(0, time).OnComplete(()=>Overlay.gameObject.SetActive(false)));
    }
    public Tweener FadeOut(Sprite sprite, float r, float g, float b, float time)
    {
        Overlay.gameObject.SetActive(true);
        Overlay.color = new Color(r, g, b, 0);
        return Overlay.DOFade(1, time).OnComplete(() => { ChangeBG(sprite); Overlay.gameObject.SetActive(false); });
    }
    public Tweener FadeIn(Sprite sprite, float r, float g, float b, float time)
    {
        Overlay.gameObject.SetActive(true);
        Overlay.color = new Color(r, g, b, 1);
        ChangeBG(sprite);
        return Overlay.DOFade(0, time).OnComplete(() => Overlay.gameObject.SetActive(false));
    }
    public Tweener Move(Vector2 targetPos, float duration,bool withCharacter)
    {
        return (withCharacter?rectP:rect).DOAnchorPos(targetPos, duration);
    }

    public Tweener Scale(Vector2 targetScale, float duration, bool withCharacter)
    {
        return (withCharacter ? rectP : rect).DOScale(targetScale, duration);
    }
    public Tweener ShakePos(float duration, Vector2 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true, bool random=true, bool withCharacter=false)
    {
        return (withCharacter ? rectP : rect).DOShakeAnchorPos(duration,strength,vibrato,randomness,false,fadeOut,random?ShakeRandomnessMode.Full:ShakeRandomnessMode.Harmonic);
    }
    public Tweener ShakeRotation(float duration, float strength, int vibrato = 10, float randomness = 90, bool fadeOut = true, bool random = true, bool withCharacter = false)
    {
        return (withCharacter ? rectP : rect).DOShakeRotation(duration, strength, vibrato, randomness, fadeOut, random ? ShakeRandomnessMode.Full : ShakeRandomnessMode.Harmonic);
    }
    public Tweener ShakeScale(float duration, Vector2 strength, int vibrato = 10, float randomness = 90, bool fadeOut = true, bool random = true, bool withCharacter = false)
    {
        return (withCharacter ? rectP : rect).DOShakeScale(duration, strength, vibrato, randomness, fadeOut, random ? ShakeRandomnessMode.Full : ShakeRandomnessMode.Harmonic);
    }
    //这个方法是用来播放视频的，但是目前没有实现
    /*public void PlayVideo()
    {
        video.url
    }*/

}
