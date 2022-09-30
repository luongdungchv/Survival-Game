using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimiter : MonoBehaviour
{
    public Transform camTransform;
    public Transform originalPosTransform;
    public LayerMask mask;

    public float lessenDivisor;

   
    RaycastHit hitInfo;
    
    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, camTransform.position);
        if(Physics.Linecast(transform.position, originalPosTransform.position, out hitInfo, mask))
        {
            camTransform.position = hitInfo.point;
        }
        else
        {
            if(camTransform.position != originalPosTransform.position)
            {
                camTransform.position = originalPosTransform.position;
            }
        }
    }
}
