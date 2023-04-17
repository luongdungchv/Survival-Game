using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static UnityEvent OnSettingChange;
    public static Settings ins;
    private SettingData currentData;
    [SerializeField] private TMP_Dropdown resolutionSetting, afSetting, aaSetting, shadowSetting, lodSetting;
    [SerializeField] private GameObject settingUI;
    [SerializeField] private bool hideCursorOnClose;


    private void Awake()
    {
        OnSettingChange = new UnityEvent();
        ins = this;
    }
    private void Start()
    {
        afSetting.onValueChanged.AddListener(this.OnAnisotropicChange);
        aaSetting.onValueChanged.AddListener(this.OnAntiAliasingChange);
        shadowSetting.onValueChanged.AddListener(this.OnShadowSettingChange);
        lodSetting.onValueChanged.AddListener(this.OnLODSettingChange);
        resolutionSetting.onValueChanged.AddListener(this.OnResolutionSettingChange);
        
        string settingJson = PlayerPrefs.GetString("setting", "");
        Debug.Log(settingJson);
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

    public void ToggleSetting()
    {
        bool state = !settingUI.activeSelf;
        settingUI.SetActive(state);
        if(UIManager.ins != null) state = state || UIManager.ins.isUIOpen; 
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
        resolutionSetting.value = currentData.resolution;
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
    public void OnResolutionSettingChange(int val){
        currentData.resolution = val;
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
        
        switch (this.currentData.resolution)
        {
            case 1:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                break;
            case 2:
                Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);
                break; 
            case 3:
                Screen.SetResolution(960, 540, FullScreenMode.FullScreenWindow);
                break;
            case 0:
                var nativeResolution = Screen.resolutions[Screen.resolutions.Length - 1];
                Screen.SetResolution(nativeResolution.width, nativeResolution.height, FullScreenMode.FullScreenWindow);
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
    public int resolution;
}
