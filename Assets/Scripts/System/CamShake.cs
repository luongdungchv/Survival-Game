using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    public static CamShake ins;
    [SerializeField] private float size, duration;
    private void Awake()
    {
        if (ins == null) ins = this;
    }
    // Start is called before the first frame update
    public void Shake(float _size, float _duration) => StartCoroutine(ShakeEnum(_size, _duration));
    public void Shake() => StartCoroutine(ShakeEnum(size, duration));
    IEnumerator ShakeEnum(float _size, float _duration)
    {
        float t = 0;
        var basePos = transform.position;
        while (t < 1)
        {
            t += Time.deltaTime / _duration;
            float randX = Random.Range(-_size, _size);
            float randY = Random.Range(-_size, _size);
            var randPos = transform.up * randY + transform.right * randX;
            transform.position = basePos + randPos;
            yield return null;
        }
        transform.position = basePos;
    }
}
