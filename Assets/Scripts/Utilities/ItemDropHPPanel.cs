using UnityEngine;

public class ItemDropHPPanel : MonoBehaviour
{
    private Quaternion originalRotation;
    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = Camera.main.transform.rotation * originalRotation;
    }
}
