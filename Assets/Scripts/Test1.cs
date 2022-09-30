using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Test1 : MonoBehaviour
{
    public Texture2D normalTex;
    public Texture2D diffuseTex;
    public Color col;
    public Material mat;
    private void Start()
    {
        // for (int x = 0; x < 5; x++)
        // {
        //     for (int y = 0; y < 5; x++)
        //     {
        //         Debug.Log($"{diffuseTex.GetPixel(x, y)} || {normalTex.GetPixel(x, y)}");
        //     }
        // }
        Debug.Log($"{diffuseTex.GetPixel(0, 0)} || {normalTex.GetPixel(0, 0)}");
        Debug.Log($"{diffuseTex.GetPixel(789, 908)} || {normalTex.GetPixel(789, 908)}");
        Debug.Log($"{diffuseTex.GetPixel(134, 456)} || {normalTex.GetPixel(134, 456)}");
        Debug.Log($"{diffuseTex.GetPixel(255, 100)} || {normalTex.GetPixel(255, 100)}");
        Debug.Log($"{diffuseTex.GetPixel(456, 3)} || {normalTex.GetPixel(456, 3)}");
        Debug.Log($"{diffuseTex.GetPixel(712, 712)} || {normalTex.GetPixel(712, 712)}");
        Debug.Log($"{diffuseTex.GetPixel(987, 34)} || {normalTex.GetPixel(987, 34)}");
    }

}
