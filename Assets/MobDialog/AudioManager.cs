using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.IO;
public class AudioManager : MonoSingleton<AudioManager>
{
    public List<Bank> bgmBanks = new List<Bank>(),sfxBanks=new List<Bank>(),masterBanks=new List<Bank>(2);
    EventInstance currentBgm;
    /// <summary>
    /// 加载指定文件夹下的所有bank文件，仅加载Master、BGM、SFX开头的bank文件
    /// </summary>
    /// <param name="path">使用者提供的文件夹路径</param>
    public void LoadBanksFromFolder(string path)
    {
        if (string.IsNullOrEmpty(path)||!Directory.Exists(path))
        {
            return;
        }
        string[] bankFiles = Directory.GetFiles(path, "*.bank");
        if (bankFiles.Length == 0)
        {
            return;
        }
        List<string> filteredFiles = new List<string>();
        foreach (string file in bankFiles)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.StartsWith("Master", System.StringComparison.OrdinalIgnoreCase))
            {
                filteredFiles.Add(file);
            }
        }
        foreach (string file in filteredFiles)
        {
            Bank bank;
            if (RuntimeManager.StudioSystem.loadBankFile(file, LOAD_BANK_FLAGS.NORMAL, out bank) == RESULT.OK)
                masterBanks.Add(bank);
        }
        filteredFiles.Clear();
        foreach (string file in bankFiles)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.StartsWith("BGM", System.StringComparison.OrdinalIgnoreCase))
            {
                filteredFiles.Add(file);
            }
        }
        foreach (string file in filteredFiles)
        {
            Bank bank;
            if (RuntimeManager.StudioSystem.loadBankFile(file, LOAD_BANK_FLAGS.NORMAL, out bank) == RESULT.OK)
                bgmBanks.Add(bank);
        }
        filteredFiles.Clear();
        foreach (string file in bankFiles)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.StartsWith("SFX", System.StringComparison.OrdinalIgnoreCase))
            {
                filteredFiles.Add(file);
            }
        }
        foreach (string file in filteredFiles)
        {
            Bank bank;
            if (RuntimeManager.StudioSystem.loadBankFile(file, LOAD_BANK_FLAGS.NORMAL, out bank) == RESULT.OK)
                sfxBanks.Add(bank);
        }
        RuntimeManager.CoreSystem.setDSPBufferSize(2048, 4);
    }
    public void UnloadAll()
    {
        foreach (var b in bgmBanks) b.unload();
        foreach (var b in sfxBanks) b.unload();
        foreach (var b in masterBanks) b.unload();
        bgmBanks.Clear();
        sfxBanks.Clear();
        masterBanks.Clear();
    }
    public void PlayBgm(string eventPath)
    {
        if (string.IsNullOrEmpty(eventPath)) return;
        StopBgm();

        RuntimeManager.StudioSystem.getEvent(eventPath, out EventDescription desc);
        if (!desc.isValid())
        {
            return;
        }

        desc.createInstance(out currentBgm); desc.getParameterDescriptionCount(out int count);
        for (int i = 0; i < count; i++)
        {
            desc.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION pd);
            string name = pd.name;
            currentBgm.setParameterByName(name, 1f);
        }
        currentBgm.start();
    }

    public void StopBgm(bool allowFadeout = true)
    {
        if (!currentBgm.isValid()) return;
        currentBgm.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
        currentBgm.release();
        currentBgm = default;
    }

    public void PauseBgm(bool pause)
    {
        if (currentBgm.isValid()) currentBgm.setPaused(pause);
    }

    public bool IsBgmPlaying()
    {
        if (!currentBgm.isValid()) return false;
        currentBgm.getPlaybackState(out PLAYBACK_STATE state);
        return state == PLAYBACK_STATE.PLAYING;
    }
    public bool avoidSfx;
    public void PlaySfx(string eventPath)
    {
        if(avoidSfx) return;
        if (!string.IsNullOrEmpty(eventPath))
            RuntimeManager.PlayOneShot(eventPath);
    }
}
