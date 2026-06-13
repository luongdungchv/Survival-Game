using UnityEngine;

namespace BoxHead2.Utils
{
    public static class ColorExtensions
    {
        public static Color ToLinear(this Color color)
        {
            return new Color(Mathf.GammaToLinearSpace(color.r), Mathf.GammaToLinearSpace(color.g),
                Mathf.GammaToLinearSpace(color.b), Mathf.GammaToLinearSpace(color.a));
        }
        
        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }
    }
}