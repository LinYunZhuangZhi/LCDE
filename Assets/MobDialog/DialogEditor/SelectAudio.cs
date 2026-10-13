using System;
using System.Collections.Generic;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectAudio : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Dropdown bankDropdown;
    public TMP_Dropdown eventDropdown;
    public Button confirmButton;
    public TextMeshProUGUI pathText;
    public Button closeButton;

    private Action<string> dele;
    private bool isBgmMode;
    private Dictionary<string, List<string>> bankEvents = new();
    private string selectedPath;

    public void SetUI(List<Bank> banks, bool isBgm = false)
    {
        gameObject.SetActive(true);
        AudioManager.Instance.StopBgm();
        isBgmMode = isBgm;
        bankEvents.Clear();
        selectedPath = null;

        bankDropdown.onValueChanged.RemoveAllListeners();
        eventDropdown.onValueChanged.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();

        bankDropdown.ClearOptions();
        eventDropdown.ClearOptions();

        var bankNames = new List<string>();

        foreach (var bank in banks)
        {
            if (!bank.isValid()) continue;

            bank.getPath(out string fmodPath);
            string displayName = fmodPath.StartsWith("bank:/")
                ? fmodPath.Substring("bank:/".Length)
                : fmodPath;

            bank.getEventCount(out int count);
            if (count == 0) continue;
            bank.getEventList(out EventDescription[] events);

            var paths = new List<string>();
            if (events != null)
            {
                foreach (var ev in events)
                {
                    if (!ev.isValid()) continue;
                    ev.getPath(out string path);
                    if (!string.IsNullOrEmpty(path) && !path.StartsWith("snapshot:/"))
                        paths.Add(path);
                }
            }

            if (paths.Count > 0)
            {
                bankEvents[displayName] = paths;
                bankNames.Add(displayName);
            }
        }

        bankDropdown.AddOptions(bankNames);
        bankDropdown.onValueChanged.AddListener(OnBankChanged);
        eventDropdown.onValueChanged.AddListener(OnEventChanged);
        confirmButton.onClick.AddListener(OnConfirm);
        if (closeButton != null) closeButton.onClick.AddListener(OnClose);

        if (bankNames.Count > 0)
        {
            bankDropdown.value = 0;
            OnBankChanged(0);
            confirmButton.interactable = true;
        }
        else
        {
            pathText.text = "无可用音频";
            confirmButton.interactable = false;
        }
    }
    public void SetUI(bool isBgm)
    {
        SetUI(isBgm?AudioManager.Instance.bgmBanks:AudioManager.Instance.sfxBanks, isBgm);
    }
    void OnBankChanged(int index)
    {
        string bankName = bankDropdown.options[index].text;
        eventDropdown.ClearOptions();

        if (bankEvents.TryGetValue(bankName, out var paths))
        {
            eventDropdown.AddOptions(paths);
            eventDropdown.value = 0;
            OnEventChanged(0);
        }
    }

    void OnEventChanged(int index)
    {
        selectedPath = eventDropdown.options[index].text;
        pathText.text = selectedPath;

        if (isBgmMode)
            AudioManager.Instance.PlayBgm(selectedPath);
        else
            AudioManager.Instance.PlaySfx(selectedPath);
    }

    void OnConfirm()
    {
        if (string.IsNullOrEmpty(selectedPath)) return;
        dele?.Invoke(selectedPath);
        OnClose();
    }

    void OnClose()
    {
        if (isBgmMode) AudioManager.Instance.StopBgm();
        gameObject.SetActive(false);
    }

    public void SetDele(Action<string> action) => dele = action;

    void OnDisable()
    {
        if (isBgmMode) AudioManager.Instance.StopBgm();
    }
}