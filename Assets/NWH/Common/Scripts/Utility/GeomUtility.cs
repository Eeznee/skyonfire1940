using UnityEngine;

namespace NWH.Common.Utility
{
    public static class GeomUtility
    {
        public static bool NearEqual(this Vector3 a, Vector3 b, float threshold = 0.01f)
        {
            return Vector3.SqrMagnitude(a - b) < threshold;
        }


        public static bool Equal(this Quaternion a, Quaternion b)
        {
            return Mathf.Abs(Quaternion.Angle(a, b)) < 0.1f;
        }


        public static Vector3 RoundedMax(this Vector3 v)
        {
            int maxIndex = -1;
            float maxValue = -Mathf.Infinity;
            for (int i = 0; i < 3; i++)
            {
                float value = Mathf.Abs(v[i]);
                if (value > maxValue)
                {
                    maxValue = value;
                    maxIndex = i;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                v[i] = i == maxIndex ? Mathf.Sign(v[i]) * 1f : 0f;
            }

            return v;
        }






        public static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
        {
            lineDir.Normalize(); //this needs to be a unit vector
            Vector3 v = pnt - linePnt;
            float d = Vector3.Dot(v, lineDir);
            return linePnt + lineDir * d;
        }


        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        public static float FindDistanceToSegment(Vector3 pt, Vector3 p1, Vector3 p2)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            if (dx == 0 && dy == 0)
            {
                // It's a point not a line segment.
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) /
                      (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
            }
            else if (t > 1)
            {
                dx = pt.x - p2.x;
                dy = pt.y - p2.y;
            }
            else
            {
                Vector3 closest = new Vector3(p1.x + t * dx, p1.y + t * dy);
                dx = pt.x - closest.x;
                dy = pt.y - closest.y;
            }

            return Mathf.Sqrt(dx * dx + dy * dy);
        }


        public static float SquareDistance(Vector3 a, Vector3 b)
        {
            float x = a.x - b.x;
            float y = a.y - b.y;
            float z = a.z - b.z;
            return x * x + y * y + z * z;
        }


        public static Vector3 LinePlaneIntersection(Vector3 planePoint, Vector3 planeNormal, Vector3 linePoint,
            Vector3 lineDirection)
        {
            if (Vector3.Dot(planeNormal, lineDirection.normalized) == 0)
            {
                return Vector3.zero;
            }

            float t = (Vector3.Dot(planeNormal, planePoint) - Vector3.Dot(planeNormal, linePoint)) /
                      Vector3.Dot(planeNormal, lineDirection.normalized);
            return linePoint + lineDirection.normalized * t;
        }


        public static Vector3 FindChordLine(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float chordPercent)
        {
            return QuadLerp(a, b, c, d, 0.5f, chordPercent);
        }


        public static Vector3 FindSpanLine(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float spanPercent)
        {
            return QuadLerp(a, b, c, d, spanPercent, 0.5f);
        }


        public static float FindArea(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
        {
            return TriArea(A, B, D) + TriArea(B, C, D);
        }


        public static Vector3 FindCenter(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            if (a == d)
            {
                return (a + b + c) / 4f;
            }

            return (a + b + c + d) / 4f;
        }


        public static float DistanceAlongNormal(Vector3 a, Vector3 b, Vector3 normal)
        {
            Vector3 dir = b - a;
            return Vector3.Project(dir, normal).magnitude;
        }

        public static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P, float dotThreshold = 0.001f)
        {
            if (SameSide(P, A, B, C) && SameSide(P, B, A, C) && SameSide(P, C, A, B))
            {
                Vector3 vc1 = Vector3.Cross(B - A, C - A).normalized;
                if (Mathf.Abs(Vector3.Dot(P - A, vc1)) <= dotThreshold)
                {
                    return true;
                }
            }

            return false;
        }


        private static bool SameSide(Vector3 p1, Vector3 p2, Vector3 A, Vector3 B)
        {
            Vector3 cp1 = Vector3.Cross(B - A, p1 - A).normalized;
            Vector3 cp2 = Vector3.Cross(B - A, p2 - A).normalized;
            if (Vector3.Dot(cp1, cp2) > 0)
            {
                return true;
            }

            return false;
        }


        public static bool PointIsInsideRect(Vector2 point)
        {
            return new Rect(0, 0, Screen.width, Screen.height).Contains(point);
        }


        public static bool NearlyEqual(this float a, float b, double epsilon)
        {
            return Mathf.Abs(a - b) < epsilon;
        }


        public static float AreaFromThreePoints(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 u, v;

            u.x = p2.x - p1.x;
            u.y = p2.y - p1.y;
            u.z = p2.z - p1.z;

            v.x = p3.x - p1.x;
            v.y = p3.y - p1.y;
            v.z = p3.z - p1.z;

            Vector3 crossUV = Vector3.Cross(u, v);
            return Mathf.Sqrt(crossUV.x * crossUV.x + crossUV.y * crossUV.y + crossUV.z * crossUV.z) * 0.5f;
        }


        public static float AreaFromFourPoints(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            return AreaFromThreePoints(p1, p2, p4) + AreaFromThreePoints(p2, p3, p4);
        }


        /// <summary>
        ///     Calculates area of a single triangle from it's three points.
        /// </summary>
        public static float TriArea(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 u, v, crossUV;

            u.x = p2.x - p1.x;
            u.y = p2.y - p1.y;
            u.z = p2.z - p1.z;

            v.x = p3.x - p1.x;
            v.y = p3.y - p1.y;
            v.z = p3.z - p1.z;

            crossUV = Vector3.Cross(u, v);
            return Mathf.Sqrt(crossUV.x * crossUV.x + crossUV.y * crossUV.y + crossUV.z * crossUV.z) * 0.5f;
        }


        /// <summary>
        ///     Calculates area of a complete mesh.
        /// </summary>
        public static float MeshArea(Mesh mesh)
        {
            if (mesh.vertices.Length == 0)
            {
                return 0;
            }

            float area = 0;

            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            for (int i = 0; i < tris.Length; i += 3)
            {
                area += TriArea(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]);
            }

            return area;
        }


        /// <summary>
        ///     Calculates area of a mesh as viewed from the direction vector.
        /// </summary>
        public static float ProjectedMeshArea(Mesh mesh, Vector3 direction)
        {
            float area = 0;

            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;
            Vector3[] normals = mesh.normals;

            int count = 0;
            for (int i = 0; i < tris.Length; i += 3)
            {
                area += TriArea(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]], direction);
                count++;
            }

            return area;
        }


        public static float RectArea(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            return TriArea(p1, p2, p4) + TriArea(p2, p3, p4);
        }


        /// <summary>
        ///     Find mesh center by averaging. Returns local center.
        /// </summary>
        public static Vector3 FindMeshCenter(Mesh mesh)
        {
            if (mesh.vertices.Length == 0)
            {
                return Vector3.zero;
            }

            Vector3 sum = Vector3.zero;
            int count = 0;
            if (mesh != null)
            {
                foreach (Vector3 vert in mesh.vertices)
                {
                    sum += vert;
                    count++;
                }
            }

            return sum / count;
        }


        public static float TriArea(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 view)
        {
            Vector3 u, v, crossUV, normal;
            float crossMagnitude;

            u.x = p2.x - p1.x;
            u.y = p2.y - p1.y;
            u.z = p2.z - p1.z;

            v.x = p3.x - p1.x;
            v.y = p3.y - p1.y;
            v.z = p3.z - p1.z;

            crossUV = Vector3.Cross(u, v);
            crossMagnitude = Mathf.Sqrt(crossUV.x * crossUV.x + crossUV.y * crossUV.y + crossUV.z * crossUV.z);

            // Normal
            if (crossMagnitude == 0)
            {
                normal.x = normal.y = normal.z = 0f;
            }
            else
            {
                normal.x = crossUV.x / crossMagnitude;
                normal.y = crossUV.y / crossMagnitude;
                normal.z = crossUV.z / crossMagnitude;
            }

            float angle = Vector3.Angle(normal, view);
            float cos = Mathf.Cos(angle);

            if (cos < 0)
            {
                return 0;
            }

            return Mathf.Sqrt(crossUV.x * crossUV.x + crossUV.y * crossUV.y + crossUV.z * crossUV.z) * 0.5f * cos;
        }


        public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float v321 = p3.x * p2.y * p1.z;
            float v231 = p2.x * p3.y * p1.z;
            float v312 = p3.x * p1.y * p2.z;
            float v132 = p1.x * p3.y * p2.z;
            float v213 = p2.x * p1.y * p3.z;
            float v123 = p1.x * p2.y * p3.z;
            return 1.0f / 6.0f * (-v321 + v231 + v312 - v132 - v213 + v123);
        }


        public static float VolumeOfMesh(Mesh mesh)
        {
            float volume = 0;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 p1 = vertices[triangles[i + 0]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];
                volume += SignedVolumeOfTriangle(p1, p2, p3);
            }

            return Mathf.Abs(volume);
        }


        public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
        {
            return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).MultiplyPoint3x4(position);
        }


        public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
        {
            return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse
                            .MultiplyPoint3x4(position);
        }


        public static void ChangeLayersRecursively(this Transform trans, string name)
        {
            trans.gameObject.layer = LayerMask.NameToLayer(name);
            foreach (Transform child in trans)
            {
                child.ChangeLayersRecursively(name);
            }
        }


        public static void ChangeObjectColor(GameObject gameObject, Color color)
        {
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        }


        public static void ChangeObjectAlpha(GameObject gameObject, float alpha)
        {
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            Color currentColor = mr.material.GetColor("_Color");
            currentColor.a = alpha;
            mr.material.SetColor("_Color", currentColor);
        }


        public static Vector3 Vector3Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }


        public static Vector3 Vector3RoundToInt(Vector3 v)
        {
            return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }


        public static Vector3 Vector3OneOver(Vector3 v)
        {
            return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
        }


        public static float RoundToStep(float value, float step)
        {
            return Mathf.Round(value / step) * step;
        }


        public static float RoundToStep(int value, int step)
        {
            return Mathf.RoundToInt(Mathf.Round(value / step) * step);
        }


        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }


        public static Vector3 QuadLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u, float v)
        {
            Vector3 abu = Vector3Lerp(a, b, u);
            Vector3 dcu = Vector3Lerp(d, c, u);
            return Vector3Lerp(abu, dcu, v);
        }

        public static Vector3 Vector3Lerp(Vector3 v1, Vector3 v2, float value)
        {
            if (value > 1.0f)
            {
                return v2;
            }

            if (value < 0.0f)
            {
                return v1;
            }

            return new Vector3(v1.x + (v2.x - v1.x) * value,
                               v1.y + (v2.y - v1.y) * value,
                               v1.z + (v2.z - v1.z) * value);
        }

        public static float QuaternionMagnitude(Quaternion q)
        {
            return Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
        }


    }
}