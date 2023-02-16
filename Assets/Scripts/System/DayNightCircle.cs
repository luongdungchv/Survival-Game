using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class DayNightCircle : MonoBehaviour
{
    [SerializeField] private Vector3 start, end;
    [SerializeField] private Gradient upperColors, lowerColors, lightColors, fogColors;
    [SerializeField] private Transform lightObj;
    [SerializeField][Range(0, 2)] private float value;
    [SerializeField] private Material skyboxMat, waterMat;
    [SerializeField] private Material[] grassMats;
    [SerializeField] private Color ambientSideColor, ambientSkyColor;
    
    private static DayNightCircle ins;
    public static float time => ins.value;
    // Start is called before the first frame update
    private void Awake() {
        ins = this;
    }
    void Start()
    {

        StartCoroutine(Circulate(30));
    }

    // Update is called once per frame
    void Update()
    {
        

    }
    IEnumerator Circulate(float duration)
    {
        value += Time.deltaTime / duration;
        float rotationParam = value > 1 ? value - 1 : value;
        lightObj.rotation = Quaternion.Lerp(Quaternion.Euler(start), Quaternion.Euler(end), rotationParam);
        lightObj.GetComponent<Light>().color = lightColors.Evaluate(value / 2);
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
        


        if (value >= 2) value = 0;
        yield return null;
        StartCoroutine(Circulate(duration));
    }
}
