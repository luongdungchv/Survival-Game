using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils
{
        public static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVector(Vector3 dir)
    {
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        //int angle = Mathf.RoundToInt(n);

        return angle;
    }
    public static float GetAngleFromVectorZX(Vector3 dir, bool negative = false){
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        if (!negative && angle < 0) angle += 360;
        else if(negative && angle > 0) angle -= 360;
        //int angle = Mathf.RoundToInt(n);

        return angle;
    }
    public static Vector3 RotateVector3D(Vector3 input, float angle){
        var initialVectorAngle = GetAngleFromVector(input);
        initialVectorAngle += angle;
        var x = Mathf.Cos(initialVectorAngle);
        var z = Mathf.Sin(initialVectorAngle);
        return new Vector3(x, input.y, z);
    }

    public static Vector3 Set(this Vector3 vector3, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x: x == null ? vector3.x : (float)x,
                           y: y == null ? vector3.y : (float)y,
                           z: z == null ? vector3.z : (float)z);
    }

    public static Vector3 Move(this Vector3 vector3, Vector3 direction)
    {
        return vector3 + direction;
    }


    public static Vector2 Set(this Vector2 vector2, float? x = null, float? y = null)
    {
        return new Vector2(x: x == null ? vector2.x : (float)x,
                           y: y == null ? vector2.y : (float)y);
    }

    public static Vector2 ToVectorXZ(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }
    
        
    public static Vector3 ToVectorXZ(this Vector2 vector2)
    {
        return new Vector3(vector2.x, 0, vector2.y);
    }
}
