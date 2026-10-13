using Newtonsoft.Json;
using System;
using System.IO;

[Serializable]
public class Setting
{
    public static Setting instance;
    public float CharDelay { get; set; }
    public float AutoDelay { get; set; }
    public string BankPath { get; set; }
    public byte Volume 
    { get
        {
            return volume;
        }
        set{
            volume = value;
            try
            {
                var bus = FMODUnity.RuntimeManager.GetBus("bus:/");
                if (bus.isValid())
                    bus.setVolume(value / 100f);
            }
            catch { }
        }
    }
    byte volume;
    public byte BGMVolume
    {
        get
        {
            return bgmvolume;
        }
        set
        {
            bgmvolume = value;
            try
            {
                var vca = FMODUnity.RuntimeManager.GetVCA("vca:/BGM");
                if (vca.isValid())
                    vca.setVolume(value / 100f);
            }
            catch { }
        }
    }
    byte bgmvolume;
    public byte SFXVolume
    {
        get
        {
            return sfxvolume;
        }
        set
        {
            sfxvolume = value;
            try
            {
                var vca = FMODUnity.RuntimeManager.GetVCA("vca:/SFX");
                if (vca.isValid())
                    vca.setVolume(value / 100f);
            }
            catch { }
        }
    }
    byte sfxvolume;
    public byte VoiceVolume
    {
        get
        {
            return voicevolume;
        }
        set
        {
            voicevolume = value;
            try
            {
                var vca = FMODUnity.RuntimeManager.GetVCA("vca:/Voice");
                if (vca.isValid())
                    vca.setVolume(value / 100f);
            }
            catch { }
        }
    }
    byte voicevolume;
    public static bool Init(string path)
    {
        if (File.Exists(path))
        {
            instance=Load(path);
            return true;
        }
        else
        {
            instance = Default();
            Save(path);
            return false;
        }
    }
    public static void RefreshVolume()
    {
        instance.Volume = instance.Volume;
        instance.BGMVolume = instance.BGMVolume;
        instance.SFXVolume = instance.SFXVolume;
        instance.VoiceVolume = instance.VoiceVolume;
    }
    public static Setting Default()
    {
        Setting setting = new Setting();
        setting.CharDelay = 0.07f;
        setting.AutoDelay = 2f;
        setting.Volume = 90;
        setting.BGMVolume = 80;
        setting.SFXVolume = 70;
        setting.VoiceVolume = 90;
        return setting;
    }
    public static void Save(string path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(instance));
    }
    public static Setting Load(string path)
    {
        if (File.Exists(path))
        {
            return JsonConvert.DeserializeObject<Setting>(File.ReadAllText(path));
        }
        return null;
    }
}