using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static UnityEvent OnSettingChange;
    public static Settings ins;
    private SettingData currentData;
    [SerializeField] private TMP_Dropdown afSetting, aaSetting, shadowSetting, lodSetting;
    [SerializeField] private GameObject settingUI;
    [SerializeField] private bool hideCursorOnClose;


    private void Awake()
    {
        OnSettingChange = new UnityEvent();
    }
    private void Start()
    {
        afSetting.onValueChanged.AddListener(this.OnAnisotropicChange);
        aaSetting.onValueChanged.AddListener(this.OnAntiAliasingChange);
        shadowSetting.onValueChanged.AddListener(this.OnShadowSettingChange);
        lodSetting.onValueChanged.AddListener(this.OnLODSettingChange);

        string settingJson = PlayerPrefs.GetString("setting", "");
        if (settingJson == "")
        {
            currentData = new SettingData();
        }
        else
        {
            currentData = JsonUtility.FromJson<SettingData>(settingJson);
        }
        SaveData();
        ToggleSetting();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSetting();
        }
    }

    public void ToggleSetting()
    {
        bool state = !settingUI.activeSelf;
        settingUI.SetActive(state);
        if (!state)
        {
            if (hideCursorOnClose) GameFunctions.ins?.HideCursor();
            return;
        }
        GameFunctions.ins?.ShowCursor();

        string settingJson = PlayerPrefs.GetString("setting", "");
        if (settingJson == "")
        {
            currentData = new SettingData();
        }
        else
        {
            currentData = JsonUtility.FromJson<SettingData>(settingJson);
        }
        afSetting.value = currentData.anisotropicFiltering;
        aaSetting.value = currentData.antiAliasing;
        shadowSetting.value = currentData.shadow;
        lodSetting.value = currentData.lod;
    }
    public void OnAnisotropicChange(int val)
    {
        currentData.anisotropicFiltering = val;
    }
    public void OnAntiAliasingChange(int val)
    {
        currentData.antiAliasing = val;
    }
    public void OnShadowSettingChange(int val)
    {
        currentData.shadow = val;
    }
    public void OnLODSettingChange(int val)
    {
        currentData.lod = val;
    }
    public void SaveData()
    {
        ToggleSetting();
        PlayerPrefs.SetString("setting", JsonUtility.ToJson(this.currentData));
        switch (this.currentData.anisotropicFiltering)
        {
            case 0:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                break;
            case 1:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                break;
            case 2:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                break;
            default:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                break;
        }

        QualitySettings.antiAliasing = this.currentData.antiAliasing * 2;



        switch (this.currentData.shadow)
        {
            case 0:
                QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                QualitySettings.shadows = ShadowQuality.Disable;
                break;
            case 1:
                QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 25;
                QualitySettings.shadowCascades = 0;
                break;
            case 2:
                QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.shadowDistance = 50;
                QualitySettings.shadowCascades = 2;
                break;
            case 3:
                QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.shadowDistance = 100;
                QualitySettings.shadowCascades = 4;
                break;
            default:
                QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                QualitySettings.shadows = ShadowQuality.Disable;
                break;
        }


        switch (this.currentData.lod)
        {
            case 0:
                QualitySettings.lodBias = 1;
                break;
            case 1:
                QualitySettings.lodBias = 1.5f;
                break;
            case 2:
                QualitySettings.lodBias = 2;
                break;
            default:
                QualitySettings.lodBias = 1;
                break;
        }
        OnSettingChange.Invoke();
    }
}
[Serializable]
public class SettingData
{
    public int anisotropicFiltering;
    public int antiAliasing;
    public int shadow;
    public int lod;
}
