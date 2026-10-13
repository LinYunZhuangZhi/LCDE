using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : UIBase
{
    public Slider Volume, BGMVolume, SFXVolume;
    public TMP_InputField CharDelay, AutoDelay, BankPath;
    public Button ok, cancel;
    public override void OnInit()
    {
        Volume.value = Setting.instance.Volume;
        BGMVolume.value = Setting.instance.BGMVolume;
        SFXVolume.value = Setting.instance.SFXVolume;
        CharDelay.text = Setting.instance.CharDelay.ToString();
        AutoDelay.text = Setting.instance.AutoDelay.ToString();
        BankPath.text = Setting.instance.BankPath;
    }
    public void Save()
    {
        Setting.instance.Volume = Convert.ToByte(Volume.value);
        Setting.instance.BGMVolume=Convert.ToByte(BGMVolume.value);
        Setting.instance.SFXVolume=Convert.ToByte(SFXVolume.value);
        float d;
        if(float.TryParse(CharDelay.text, out d))
            Setting.instance.CharDelay=d;
        if (float.TryParse(AutoDelay.text, out d))
            Setting.instance.AutoDelay = d;
        Setting.instance.BankPath = BankPath.text;
        Setting.Save(Path.Combine(Application.persistentDataPath, "Setting.json"));
    }
    public void ReloadAudio()
    {
        Setting.instance.BankPath = BankPath.text;
        AudioManager.Instance.LoadBanksFromFolder(Setting.instance.BankPath);
    }
    public void Exit()
    {
        UIManager.Instance.CloseUI("SettingUI");
    }
}
