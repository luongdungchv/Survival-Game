using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dacodelaac.Utils
{
    public static class SimpleMath
    {
        public static bool InRange(Vector3 p, Vector3 c, float r, out float sqrDst, bool compareY = false)
        {
            if (!compareY) p.y = c.y;
            sqrDst = SqrDist(p, c);
            return sqrDst <= r * r;
        }

        public static bool InRange(Vector3 p, Vector3 c, float r, bool compareY = false)
        {
            if (!compareY) p.y = c.y;
            var sqrDst = SqrDist(p, c);
            return sqrDst <= r * r;
        }

        public static bool InRange2D(Vector2 p, Vector2 c, float r)
        {
            var sqrDst = SqrDist(p, c);
            return sqrDst <= r * r;
        }

        public static float SqrDist(Vector3 a, Vector3 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
        }

        public static float SqrDistXZ(Vector3 a, Vector3 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z);
        }

        public static float DistXZ(Vector3 a, Vector3 b)
        {
            return Mathf.Sqrt(SqrDistXZ(a, b));
        }

        public static Quaternion RandomRotation(bool onlyY = false)
        {
            if (onlyY) return Quaternion.Euler(0, Random.value * 360f, 0);
            return Quaternion.Euler(RandomVector3() * 180f);
        }

        public static Vector3 RandomInCircleXZ()
        {
            Vector3 rd = Random.insideUnitCircle;
            rd.z = rd.y;
            rd.y = 0;
            return rd;
        }

        public static Vector3 RandomOnCircleXZ()
        {
            Vector3 rd = Random.insideUnitCircle;
            rd.Normalize();
            rd.z = rd.y;
            rd.y = 0;
            return rd;
        }

        public static Quaternion GetRotationXZ(Vector3 a, Vector3 b)
        {
            var dir = b - a;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                return Quaternion.LookRotation(dir);
            }
            else
            {
                return Quaternion.identity;
            }
        }

        public static Quaternion AngVelToDeriv(Quaternion current, Vector3 angVel)
        {
            var spin = new Quaternion(angVel.x, angVel.y, angVel.z, 0f);
            var result = spin * current;
            return new Quaternion(0.5f * result.x, 0.5f * result.y, 0.5f * result.z, 0.5f * result.w);
        }

        public static Vector3 DerivToAngVel(Quaternion current, Quaternion deriv)
        {
            var result = deriv * Quaternion.Inverse(current);
            return new Vector3(2f * result.x, 2f * result.y, 2f * result.z);
        }

        public static Quaternion IntegrateRotation(Quaternion rotation, Vector3 angularVelocity, float deltaTime)
        {
            if (deltaTime < Mathf.Epsilon) return rotation;
            var deriv = AngVelToDeriv(rotation, angularVelocity);
            var pred = new Vector4(
                rotation.x + deriv.x * deltaTime,
                rotation.y + deriv.y * deltaTime,
                rotation.z + deriv.z * deltaTime,
                rotation.w + deriv.w * deltaTime
            ).normalized;
            return new Quaternion(pred.x, pred.y, pred.z, pred.w);
        }

        public static Vector3 CircularInterpolate(Vector3 start, Vector3 end, float t, Vector3 upVector, float centerOffset)
        {
            var circleCenter = (start + end) / 2 - upVector.normalized * centerOffset;
            
            var startDir = start - circleCenter;
            var endDir = end - circleCenter;
            
            return circleCenter + Vector3.Slerp(startDir, endDir, t);
        }

        public static Vector2 EllipseIntersection(float a, float b, Vector2 input, out bool inside, out float degAngle)
        {
            var angle = Mathf.Atan2(input.y, input.x);
            degAngle = angle * Mathf.Rad2Deg;
            var posOnEllipse = new Vector2(a * Mathf.Cos(angle), b * Mathf.Sin(angle));
            if (input.sqrMagnitude > posOnEllipse.sqrMagnitude)
            {
                inside = false;
                return posOnEllipse;
            }

            inside = true;
            return input;
        }

        public static Vector4 WorldToHClip(Camera mainCamera, Vector3 worldPosition)
        {
            // Get the combined View-Projection matrix from the camera
            Matrix4x4 viewProjectionMatrix = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;

            // Convert world position to HClip space (homogeneous clip space)
            Vector4 hclipPosition = viewProjectionMatrix *
                                    new Vector4(worldPosition.x, worldPosition.y, worldPosition.z, 1.0f);

            return hclipPosition;
        }

        public static Vector2 WorldToCanvasPosition(Vector3 worldPosition, Canvas mainCanvas, Camera camera)
        {
            var positionCS = WorldToHClip(camera, worldPosition);
            positionCS.x /= Mathf.Abs(positionCS.w);
            positionCS.y /= Mathf.Abs(positionCS.w);
            var positionCS2D = new Vector2(positionCS.x, positionCS.y);
            var canvasPos = positionCS2D * mainCanvas.GetComponent<RectTransform>().sizeDelta / 2f;
            return canvasPos;
        }

        public static Quaternion QuaternionSmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv,
            float time)
        {
            if (Time.deltaTime < Mathf.Epsilon) return rot;
            // account for double-cover
            var dot = Quaternion.Dot(rot, target);
            var multi = dot > 0f ? 1f : -1f;
            target.x *= multi;
            target.y *= multi;
            target.z *= multi;
            target.w *= multi;
            // smooth damp (nlerp approx)
            var result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;

            // ensure deriv is tangent
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        public static Vector3 RandomVector3(bool zeroY = false)
        {
            var result = Random.insideUnitSphere;
            if (zeroY) result.y = 0;
            return result;
        }

        public static int GetNearestIndex(Vector3 p, float r, Vector3[] list, bool compareY = false)
        {
            var minDist = Mathf.Infinity;
            var index = -1;
            var dist = 0f;
            for (var i = 0; i < list.Length; i++)
            {
                if (InRange(p, list[i], r, out dist, compareY))
                {
                    if (dist <= minDist)
                    {
                        minDist = dist;
                        index = i;
                    }
                }
            }

            return index;
        }

        public static Vector3 RandomBetween(Vector3 a, Vector3 b)
        {
            return a + (b - a).normalized * Random.value * (b - a).magnitude;
        }

        // public static T GetRandomElement<T>(this IEnumerable<T> collection, int start = 0, int end = -1)
        // {
        //     var array = (collection is T[] arr  ? arr : collection.ToArray());
        //     end = end == -1 ? array.Length : end;
        //     if (array.Length == 0) return default;
        //     return array[CryptoRandom.Range(start, end)];
        // }
        //
        // public static T GetRandomElement<T>(this IEnumerable<T> collection, System.Random random, int start = 0, int end = -1)
        // {
        //     var array = (collection is T[] arr  ? arr : collection.ToArray());
        //     end = end == -1 ? array.Length : end;
        //     if (array.Length == 0) return default;
        //     
        //     if(random != null) return array[random.Next(start, end)];
        //     return array[CryptoRandom.Range(start, end)];
        // }
        
        public static List<T> ShuffleCollection<T>(this IEnumerable<T> input)
        {
            var enumerable = input as T[] ?? input.ToArray();
            var res = enumerable.OrderBy(x => CryptoRandom.Range(0, enumerable.Length));
            return res.ToList();
        }
        
        public static List<T> ShuffleCollection<T>(this IEnumerable<T> input, System.Random random)
        {
            var enumerable = input as T[] ?? input.ToArray();
            var res = enumerable.OrderBy(x => random.Next(0, enumerable.Length));
            return res.ToList();
        }

        public static string NewGuid()
        {
            var encoded = Convert.ToBase64String(System.Guid.NewGuid().ToByteArray());
            encoded = encoded.Replace("/", "_").Replace("+", "-");
            return encoded.Substring(0, 22);
        }


        public static Vector3 DirectionFromAngle(float angleInDegrees)
        {
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        public static float GlobalAngleFromDir(Vector3 dir)
        {
            dir.y = 0;
            return Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
        }

        public static T[] SelectRandomFromArray<T>(T[] items, int count, bool cryptoRandom)
        {
            if (items.Length < count)
            {
                return items;
            }

            var list = items.ToList();
            var result = new T[count];
            while (list.Count > 0 && count > 0)
            {
                count--;
                result[count] = list[cryptoRandom ? CryptoRandom.Range(0, list.Count) : Random.Range(0, list.Count)];
                list.Remove(result[count]);
            }

            return result;
        }

        public static T[] GetWeightedRandomItems<T>(T[] items, int[] weights, int count, bool cryptoRandom)
        {
            var result = new List<T>();
            var itemList = items.ToList();
            var weightList = weights != null ? weights.ToList() : Enumerable.Repeat(1, itemList.Count).ToList();
            var totalWeight = 0;

            for (var i = 0; i < itemList.Count; i++)
            {
                totalWeight += weightList[i];
            }

            count = Mathf.Clamp(count, 0, itemList.Count);

            for (var i = 0; i < count; i++)
            {
                var randomValue =
                    cryptoRandom ? CryptoRandom.Range(1, totalWeight + 1) : Random.Range(1, totalWeight + 1);

                for (var j = 0; j < itemList.Count; j++)
                {
                    randomValue -= weightList[j];

                    if (randomValue <= 0)
                    {
                        var item = itemList[j];
                        result.Add(item);
                        totalWeight -= weightList[j];
                        weightList.RemoveAt(j);
                        itemList.RemoveAt(j);
                        break;
                    }
                }
            }

            return result.ToArray();
        }
        
        public static T[] GetWeightedRandomItems<T>(T[] items, int[] weights, int count, System.Random random)
        {
            var result = new List<T>();
            var itemList = items.ToList();
            var weightList = weights != null ? weights.ToList() : Enumerable.Repeat(1, itemList.Count).ToList();
            var totalWeight = 0;

            for (var i = 0; i < itemList.Count; i++)
            {
                totalWeight += weightList[i];
            }

            count = Mathf.Clamp(count, 0, itemList.Count);

            for (var i = 0; i < count; i++)
            {
                var randomValue = random.Next(1, totalWeight + 1);

                for (var j = 0; j < itemList.Count; j++)
                {
                    randomValue -= weightList[j];

                    if (randomValue <= 0)
                    {
                        var item = itemList[j];
                        result.Add(item);
                        totalWeight -= weightList[j];
                        weightList.RemoveAt(j);
                        itemList.RemoveAt(j);
                        break;
                    }
                }
            }

            return result.ToArray();
        }

        public static Vector3[] GetCirclePoint(Vector3 center, float radius, float step = 0.1f)
        {
            var points = new List<Vector3>();
            var theta = 0f;
            var x = radius * Mathf.Cos(theta);
            var y = radius * Mathf.Sin(theta);
            points.Add(center + new Vector3(x, 0, y));
            for (theta = step; theta < Mathf.PI * 2; theta += step)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                points.Add(center + new Vector3(x, 0, y));
            }

            return points.ToArray();
        }

        public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec,
            Vector3 planeNormal, Vector3 planePoint)
        {
            float length;
            float dotNumerator;
            float dotDenominator;
            Vector3 vector;
            intersection = Vector3.zero;

            //calculate the distance between the linePoint and the line-plane intersection point
            dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
            dotDenominator = Vector3.Dot(lineVec, planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;

                //create a vector from the linePoint to the intersection point
                vector = SetVectorLength(lineVec, length);

                //get the coordinates of the line-plane intersection point
                intersection = linePoint + vector;

                return true;
            }
            //output not valid
            else
            {
                return false;
            }
        }

        public static bool AreLineSegmentsCrossing(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2)
        {
            Vector3 closestPointA;
            Vector3 closestPointB;
            int sideA;
            int sideB;

            Vector3 lineVecA = pointA2 - pointA1;
            Vector3 lineVecB = pointB2 - pointB1;

            bool valid = ClosestPointsOnTwoLines(out closestPointA, out closestPointB, pointA1, lineVecA.normalized,
                pointB1, lineVecB.normalized);

            //lines are not parallel
            if (valid)
            {
                sideA = PointOnWhichSideOfLineSegment(pointA1, pointA2, closestPointA);
                sideB = PointOnWhichSideOfLineSegment(pointB1, pointB2, closestPointB);

                if ((sideA == 0) && (sideB == 0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static Vector3 GetRandomPositionInsideTriangle(params Vector3[] points)
        {
            var rand1 = CryptoRandom.Range(0, 1f);
            var rand2 = CryptoRandom.Range(0, 1f);
            
            var p1 = Vector3.Lerp(points[0], points[1], rand1);
            var p2 = Vector3.Lerp(points[1], points[2], rand2);
            
            var rand3 = CryptoRandom.Range(0, 1f);
            return Vector3.Lerp(p1, p2, rand3);
        }

        public static bool IntersectLineSegments2D(Vector2 p1start, Vector2 p1end, Vector2 p2start, Vector2 p2end,
            out Vector2 intersection)
        {
            // Consider:
            //   p1start = p
            //   p1end = p + r
            //   p2start = q
            //   p2end = q + s
            // We want to find the intersection point where :
            //  p + t*r == q + u*s
            // So we need to solve for t and u
            var p = p1start;
            var r = p1end - p1start;
            var q = p2start;
            var s = p2end - p2start;
            var qminusp = q - p;

            float cross_rs = CrossProduct2D(r, s);

            if (Approximately(cross_rs, 0f))
            {
                // Parallel lines
                if (Approximately(CrossProduct2D(qminusp, r), 0f))
                {
                    // Co-linear lines, could overlap
                    float rdotr = Vector2.Dot(r, r);
                    float sdotr = Vector2.Dot(s, r);
                    // this means lines are co-linear
                    // they may or may not be overlapping
                    float t0 = Vector2.Dot(qminusp, r / rdotr);
                    float t1 = t0 + sdotr / rdotr;
                    if (sdotr < 0)
                    {
                        // lines were facing in different directions so t1 > t0, swap to simplify check
                        Swap(ref t0, ref t1);
                    }

                    if (t0 <= 1 && t1 >= 0)
                    {
                        // Nice half-way point intersection
                        float t = Mathf.Lerp(Mathf.Max(0, t0), Mathf.Min(1, t1), 0.5f);
                        intersection = p + t * r;
                        return true;
                    }
                    else
                    {
                        // Co-linear but disjoint
                        intersection = Vector2.zero;
                        return false;
                    }
                }
                else
                {
                    // Just parallel in different places, cannot intersect
                    intersection = Vector2.zero;
                    return false;
                }
            }
            else
            {
                // Not parallel, calculate t and u
                float t = CrossProduct2D(qminusp, s) / cross_rs;
                float u = CrossProduct2D(qminusp, r) / cross_rs;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    intersection = p + t * r;
                    return true;
                }
                else
                {
                    // Lines only cross outside segment range
                    intersection = Vector2.zero;
                    return false;
                }
            }
        }

        public static bool Approximately(float a, float b, float tolerance = 1e-5f)
        {
            return Mathf.Abs(a - b) <= tolerance;
        }

        public static float CrossProduct2D(Vector2 a, Vector2 b)
        {
            return a.x * b.y - b.x * a.y;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2,
            Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {
                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }

            else
            {
                return false;
            }
        }

        public static Vector2 GetDirectionFromAngle(float degreeAngle)
        {
            var radAngle = degreeAngle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radAngle), Mathf.Sin(radAngle));
        }

        public static Vector3 GetDirectionFromAngleXZ(float degreeAngle)
        {
            var dir = GetDirectionFromAngle(degreeAngle);
            return new Vector3(dir.x, 0, dir.y).normalized;
        }

        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            Vector3 lineVec = linePoint2 - linePoint1;
            Vector3 pointVec = point - linePoint1;

            float dot = Vector3.Dot(pointVec, lineVec);

            //point is on side of linePoint2, compared to linePoint1
            if (dot > 0)
            {
                //point is on the line segment
                if (pointVec.magnitude <= lineVec.magnitude)
                {
                    return 0;
                }

                //point is not on the line segment and it is on the side of linePoint2
                else
                {
                    return 2;
                }
            }

            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
            else
            {
                return 1;
            }
        }

        public static Vector3 SetVectorLength(Vector3 vector, float size)
        {
            //normalize the vector
            var vectorNormalized = Vector3.Normalize(vector);

            //scale the vector
            return vectorNormalized *= size;
        }
        
        public static string IntToRoman(int num)
        {
            if (num < 1 || num > 3999)
                throw new ArgumentOutOfRangeException("Value must be in the range 1 - 3999.");

            var romanMap = new (int value, string symbol)[]
            {
                (1000, "M"),
                (900,  "CM"),
                (500,  "D"),
                (400,  "CD"),
                (100,  "C"),
                (90,   "XC"),
                (50,   "L"),
                (40,   "XL"),
                (10,   "X"),
                (9,    "IX"),
                (5,    "V"),
                (4,    "IV"),
                (1,    "I")
            };

            var result = new System.Text.StringBuilder();

            foreach (var (value, symbol) in romanMap)
            {
                while (num >= value)
                {
                    result.Append(symbol);
                    num -= value;
                }
            }

            return result.ToString();
        }

        public static Color DotColor(Color a, Color b)
        {
            var vA = new Vector3(a.r, a.g, a.b);
            var vB = new Vector3(b.r, b.g, b.b);
            var vC = Vector3.Dot(vA, vB);
            return new Color(vC, vC, vC);
        }

        public static T[] GetASetFromCollection<T>(T[] collections, int count, bool cryptoRandom)
        {
            count = Mathf.Min(count, collections.Length);
            var list = collections.ToList();
            var result = new T[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = list[cryptoRandom ? CryptoRandom.Range(0, list.Count) : Random.Range(0, list.Count)];
                list.Remove(result[i]);
            }

            return result;
        }

        public static Color GammaToLinearSpace(Color color)
        {
            return new Color(Mathf.GammaToLinearSpace(color.r), Mathf.GammaToLinearSpace(color.g),
                Mathf.GammaToLinearSpace(color.b), color.a);
        }

        public static bool IsDirInsideAngle(Vector3 x, Vector3 a, float angle)
        {
            return Vector3.Angle(x, a) <= angle * 0.5f;
        }

        public static bool IsDirInside2Dir(Vector3 x, Vector3 a, Vector3 b)
        {
            var angle = Vector3.SignedAngle(a, b, Vector3.down);
            var angle1 = Vector3.SignedAngle(a, x, Vector3.down);
            return angle > 0 && angle1 > 0 && angle > angle1 || angle < 0 && angle1 < 0 && angle < angle1;
        }
    }

    public struct PathTraverser
    {
        Vector3[] points;
        float[] accumulatedDistances;
        float totalDistance;
        
        public float TotalDistance => totalDistance;

        public void SetUp(Vector3[] points)
        {
            if (points.Length == 0) return;
            this.points = points;
            accumulatedDistances = new float[points.Length];
            accumulatedDistances[0] = 0;
            totalDistance = 0;
            for (int i = 1; i < points.Length; i++)
            {
                totalDistance += Vector3.Distance(points[i - 1], points[i]);
                accumulatedDistances[i] = totalDistance;
            }
        }

        public Vector3 Interpolate(float t)
        {
            if (points.Length == 0) return default;
            if(t < 0) return points[0];
            if(t > 1) return points[^1];
            
            var dist = Mathf.Lerp(0, totalDistance, t);
            for (int i = 1; i < points.Length; i++)
            {
                if (dist <= accumulatedDistances[i])
                {
                    var interpolate = Mathf.InverseLerp(accumulatedDistances[i - 1], accumulatedDistances[i], dist);
                    return Vector3.Lerp(points[i - 1], points[i], interpolate);
                }
            }

            return points[0];
        }
    }
}