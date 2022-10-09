using UnityEngine;
public class FPSSetter : MonoBehaviour
{
    [SerializeField] private int fps;
    // Start is called before the first frame update
    void Awake()
    {
        //QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
