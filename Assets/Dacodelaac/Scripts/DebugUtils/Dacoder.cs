using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dacodelaac.DebugUtils
{
    public static class Dacoder
    {
        public static void Log(Object context, params object[] os)
        {
            var s = os.Aggregate(string.Empty, (c, o) => string.IsNullOrEmpty(c) ? c + o : c + ", " + o);
            Debug.Log($"{GetMethod()}{s}", context);
        }
        
        public static void LogError(Object context, params object[] os)
        {
            var s = os.Aggregate(string.Empty, (c, o) => string.IsNullOrEmpty(c) ? c + o : c + ", " + o);
            Debug.LogError($"{GetMethod()}{s}", context);
        }
        
        public static void Log(params object[] os)
        {
            var s = os.Aggregate(string.Empty, (c, o) => string.IsNullOrEmpty(c) ? c + o : c + ", " + o);
            Debug.LogFormat("{0}{1}", GetMethod(), s);
        }

        public static void LogError(params object[] os)
        {
            var s = os.Aggregate(string.Empty, (c, o) => string.IsNullOrEmpty(c) ? c + o : c + ", " + o);
            Debug.LogErrorFormat("{0}{1}", GetMethod(), s);
        }

        public static void LogFormat(string format, params object[] os)
        {
            Debug.LogFormat("{0}{1}", GetMethod(), string.Format(ValidateFormat(format), os));
        }

        public static void LogErrorFormat(string format, params object[] os)
        {
            Debug.LogErrorFormat("{0}{1}", GetMethod(true), string.Format(ValidateFormat(format), os));
        }

        static string GetMethod(bool e = false)
        {
#if UNITY_EDITOR || ENABLE_MONO
            var stackTrace = new StackTrace();

            var mth = stackTrace.GetFrame(2).GetMethod();
            var cls = mth.ReflectedType;

            var clsName = cls == null ? "NoClassName" : cls.Name;
            var mthName = mth.Name;

            return e ? $"<color=red>{clsName}.{mthName}</color>: " : $"<color=#CD5C08>{clsName}.{mthName}</color>: ";
#else
            return string.Empty;
#endif
        }

        static string ValidateFormat(string format)
        {
            if (string.IsNullOrEmpty(format)) format = "{0}";
            return format;
        }

        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);

            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                            new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                           new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }

        public static void DrawArrow(Vector3 pos, Vector3 direction, float duration, Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color, duration);

            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                        new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                       new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color, duration);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color, duration);
        }

        public static void DrawArrowGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                        new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                       new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void DrawArrowGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                        new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                       new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        
        public static void DrawCircle(Vector3 center, float radius, Color color, float duration)
        {
            var theta = 0f;
            var x = radius * Mathf.Cos(theta);
            var y = radius * Mathf.Sin(theta);
            var pos = center + new Vector3(x, 0, y);
            var newPos = pos;
            var lastPos = pos;
            for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = center + new Vector3(x, 0, y);
                Debug.DrawLine(pos, newPos, color, duration);
                pos = newPos;
            }
            Debug.DrawLine(pos, lastPos, color, duration);
        }

        public static void DrawCircleGizmo(Vector3 center, float radius, int pointNum = 60)
        {
            var theta = 0f;
            var x = radius * Mathf.Cos(theta);
            var y = radius * Mathf.Sin(theta);
            var pos = center + new Vector3(x, 0, y);
            var newPos = pos;
            var lastPos = pos;
            var step = Mathf.PI * 2 / pointNum;
            for (theta = step; theta < Mathf.PI * 2; theta += step)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = center + new Vector3(x, 0, y);
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
            }
            Gizmos.DrawLine(pos, lastPos);
        }

        public static void DrawRectangleGizmo(Vector3 center, Vector3 halfExtents, float angle)
        {
            // Calculate the half extents of the rectangle

            // Calculate the four corners of the rectangle relative to the center
            Vector3 topLeft = new Vector3(-halfExtents.x, 0, halfExtents.z);
            Vector3 topRight = new Vector3(halfExtents.x, 0, halfExtents.z);
            Vector3 bottomLeft = new Vector3(-halfExtents.x, 0, -halfExtents.z);
            Vector3 bottomRight = new Vector3(halfExtents.x, 0, -halfExtents.z);

            // Create the rotation quaternion for the Y-axis rotation
            Quaternion rotation = Quaternion.Euler(0, angle, 0);

            // Rotate the corners around the center
            topLeft = center + rotation * topLeft;
            topRight = center + rotation * topRight;
            bottomLeft = center + rotation * bottomLeft;
            bottomRight = center + rotation * bottomRight;

            // Draw the rectangle by connecting the corners with lines
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

        public static void DrawArcGizmo(Vector3 center, float radius, float angle1, float angle2, int resolution, bool arrow)
        {
            if (resolution == 0) return;
            
            angle1 *= Mathf.Deg2Rad;
            angle2 *= Mathf.Deg2Rad;
            var step = (angle2 - angle1) / resolution;
            
            var x1 = Mathf.Sin(angle1);
            var z1 = Mathf.Cos(angle1);
            var x2 = Mathf.Sin(angle2);
            var z2 = Mathf.Cos(angle2);
            var pos1 = center + new Vector3(x1, 0, z1) * radius;
            var pos2 = center + new Vector3(x2, 0, z2) * radius;
            
            Gizmos.DrawLine(center, pos1);
            Gizmos.DrawLine(center, pos2);
            
            for (var theta = angle1 + step; angle1 < angle2 ? theta < angle2 : theta > angle2; theta += step)
            {
                var x = radius * Mathf.Sin(theta);
                var z = radius * Mathf.Cos(theta);
                var newPos = center + new Vector3(x, 0, z);
                if (arrow)
                {
                    DrawArrowGizmo(pos1, newPos - pos1);
                }
                else
                {
                    Gizmos.DrawLine(pos1, newPos);
                }
                pos1 = newPos;
            }

            if (arrow && pos1 != pos2)
            {
                DrawArrowGizmo(pos1, pos2 - pos1);
            }
            else
            {
                Gizmos.DrawLine(pos1, pos2);
            }
        }

        public static void DrawWireSphere(Vector3 center, float radius, Color color, float duration, int quality = 3)
        {
            quality = Mathf.Clamp(quality, 1, 10);

            var segments = quality << 2;
            var subdivisions = quality << 3;
            var halfSegments = segments >> 1;
            var strideAngle = 360F / subdivisions;
            var segmentStride = 180F / segments;

            Vector3 first;
            Vector3 next;
            for (var i = 0; i < segments; i++)
            {
                first = (Vector3.forward * radius);
                first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.right) * first;

                for (var j = 0; j < subdivisions; j++)
                {
                    next = Quaternion.AngleAxis(strideAngle, Vector3.up) * first;
                    UnityEngine.Debug.DrawLine(first + center, next + center, color, duration);
                    first = next;
                }
            }

            Vector3 axis;
            for (var i = 0; i < segments; i++)
            {
                first = (Vector3.forward * radius);
                first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.up) * first;
                axis = Quaternion.AngleAxis(90F, Vector3.up) * first;

                for (var j = 0; j < subdivisions; j++)
                {
                    next = Quaternion.AngleAxis(strideAngle, axis) * first;
                    UnityEngine.Debug.DrawLine(first + center, next + center, color, duration);
                    first = next;
                }
            }
        }
    }
}