using System.Collections;
using UnityEngine;

public class DayNightCircle : MonoBehaviour
{
    [SerializeField] private Vector3 start, end;
    [SerializeField] private Gradient upperColors, lowerColors, lightColors, fogColors, ambientEquatorColor;
    [SerializeField] private Transform lightObj;
    [SerializeField][Range(0, 2)] private float value;
    [SerializeField] private Material skyboxMat, waterMat;
    [SerializeField] private Material[] grassMats;
    [SerializeField] private Color ambientSideColor, ambientSkyColor;
    [SerializeField] private float dayTime;
    [SerializeField] private GameObject flare;
    
    private static DayNightCircle ins;
    public static float time => ins.value;
    private void Awake() {
        ins = this;
    }
    void Start()
    {
        this.value = 0;
        StartCoroutine(Circulate(dayTime));
    }

    IEnumerator Circulate(float duration)
    {
        value += Time.deltaTime / duration;
        this.ConfigMaterial();
        yield return null;
        StartCoroutine(Circulate(duration));
    }


    private void OnValidate() {
        this.ConfigMaterial();
    }

    private void ConfigMaterial(){
        float rotationParam = value > 1 ? value - 1 : value;
        lightObj.rotation = Quaternion.Lerp(Quaternion.Euler(start), Quaternion.Euler(end), rotationParam);
        lightObj.GetComponent<Light>().color = lightColors.Evaluate(value / 2);
        RenderSettings.ambientEquatorColor = ambientEquatorColor.Evaluate(value / 2);
        skyboxMat.SetFloat("_Value", value);
        skyboxMat.SetFloat("_State", rotationParam);
        skyboxMat.SetColor("_SkyColor", upperColors.Evaluate(value / 2));
        skyboxMat.SetColor("_GroundColor", lowerColors.Evaluate(value / 2));
        skyboxMat.SetFloat("_SunMoonState", value / 2);
        RenderSettings.fogColor = fogColors.Evaluate(value / 2);

        float smoothnessParam = rotationParam > 0.5 ? Mathf.InverseLerp(0.5f, 1, rotationParam) : 1 - Mathf.InverseLerp(0, 0.5f, rotationParam);
        foreach (var i in grassMats)
        {
            i.SetFloat("_SmoothnessState", smoothnessParam);
        }
        
        var alteredAngle = Mathf.Lerp(-30, 30, rotationParam);
        var currentAngle = Vector3.Angle(-transform.forward, -Vector3.forward);
        currentAngle += alteredAngle;
        var flareDirection = new Vector3(0, Mathf.Sin(currentAngle * Mathf.Deg2Rad), Mathf.Cos(currentAngle * Mathf.Deg2Rad));
        flareDirection *= 1000;
        //flare.transform.position = flareDirection;
        flare.transform.localRotation = Quaternion.Euler(alteredAngle, 0, 0);
        flare.GetComponent<LensFlare>().color = lightObj.GetComponent<Light>().color;

        this.flare.gameObject.SetActive(value < 1 && value > 0.14f);

        if (value >= 2) value = 0;
    }
}
