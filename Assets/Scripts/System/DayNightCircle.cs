using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCircle : MonoBehaviour
{
    [SerializeField] private Vector3 start, end;
    [SerializeField] private Gradient upperColors, lowerColors, lightColors;
    [SerializeField] private Transform lightObj;
    [SerializeField][Range(0, 2)] private float value;
    [SerializeField] private Material skyboxMat, waterMat;
    [SerializeField] private Material[] grassMats;
    [SerializeField] private Color ambientSideColor, ambientSkyColor;
    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(Circulate(30));
        Debug.Log(Mathf.Atan2(1 / 2, Mathf.Sqrt(3) / 2));
    }

    // Update is called once per frame
    void Update()
    {
        // lightObj.rotation = Quaternion.Lerp(Quaternion.Euler(start), Quaternion.Euler(end), value);
        // skyboxMat.SetColor("_SkyColor", upperColors.Evaluate(value));
        // skyboxMat.SetColor("_GroundColor", lowerColors.Evaluate(value));

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

        float smoothnessParam = rotationParam > 0.5 ? Mathf.InverseLerp(0.5f, 1, rotationParam) : 1 - Mathf.InverseLerp(0, 0.5f, rotationParam);
        foreach (var i in grassMats)
        {
            i.SetFloat("_SmoothnessState", smoothnessParam);
        }
        //waterMat.SetFloat("_SmoothnessState", smoothnessParam);
        //RenderSettings.ambientEquatorColor = Color.Lerp(ambientSideColor, Color.black, smoothnessParam);
        //RenderSettings.ambientSkyColor = Color.Lerp(ambientSkyColor, Color.black, smoothnessParam);


        if (value >= 2) value = 0;
        yield return null;
        StartCoroutine(Circulate(duration));
    }
}
