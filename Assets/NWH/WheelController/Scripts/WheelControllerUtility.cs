using System;
using UnityEngine;

namespace NWH.WheelController3D
{
    public class WheelControllerUtility : MonoBehaviour
    {
        // Function under MIT license from: https://github.com/GlitchEnzo/UnityProceduralPrimitives/blob/master/Assets/Procedural%20Primitives/Scripts/Primitive.cs
        /// <summary>
        /// Creates a <see cref="Mesh"/> filled with vertices forming a cylinder.
        /// </summary>
        /// <remarks>
        /// The values are as follows:
        /// Vertex Count    = slices * (stacks + 1) + 2
        /// Primitive Count = slices * (stacks + 1) * 2
        /// </remarks>
        /// <param name="bottomRadius">Radius at the negative Y end. Value should be greater than or equal to 0.0f.</param>
        /// <param name="topRadius">Radius at the positive Y end. Value should be greater than or equal to 0.0f.</param>
        /// <param name="length">Length of the cylinder along the Y-axis.</param>
        /// <param name="slices">Number of slices about the Y axis.</param>
        /// <param name="stacks">Number of stacks along the Y axis.</param>
        public static Mesh CreateWheelMesh(float radius, float length, bool topHalf)
        {
            float bottomRadius = radius;
            float topRadius = radius;

            // if both the top and bottom have a radius of zero, just return null, because invalid
            if (bottomRadius <= 0 && topRadius <= 0)
            {
                return null;
            }

            Mesh mesh = new Mesh();
            if (topHalf) mesh.name = "TopWheelMesh";
            else mesh.name = "BottomWheelMesh";
            int slices = 8;
            int stacks = 1;
            float sliceStep = (float)Math.PI * 2.0f / slices;
            float heightStep = length / stacks;
            float radiusStep = (topRadius - bottomRadius) / stacks;
            float currentHeight = -length / 2;
            int vertexCount = (stacks + 1) * slices + 2; //cone = stacks * slices + 1
            int triangleCount = (stacks + 1) * slices * 2; //cone = stacks * slices * 2 + slices
            int indexCount = triangleCount * 3;
            float currentRadius = bottomRadius;

            Vector3[] cylinderVertices = new Vector3[vertexCount];
            Vector3[] cylinderNormals = new Vector3[vertexCount];
            Vector2[] cylinderUVs = new Vector2[vertexCount];

            // Start at the bottom of the cylinder            
            int currentVertex = 0;
            cylinderVertices[currentVertex] = new Vector3(currentHeight, 0, 0);
            cylinderNormals[currentVertex] = Vector3.right;
            currentVertex++;

            for (int i = 0; i <= stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    if (!topHalf && (j < 2 || j > 6)) continue;
                    if (topHalf && j > 2 && j < 6) continue; // Could be implemented better.

                    float sliceAngle = j * sliceStep;
                    float x = currentHeight;
                    float y = currentRadius * (float)Math.Cos(sliceAngle);
                    float z = currentRadius * (float)Math.Sin(sliceAngle);

                    Vector3 position = new Vector3(x, y, z);
                    cylinderVertices[currentVertex] = position;
                    cylinderNormals[currentVertex] = Vector3.Normalize(position);
                    cylinderUVs[currentVertex] =
                        new Vector2((float)(Math.Sin(cylinderNormals[currentVertex].x) / Math.PI + 0.5f),
                            (float)(Math.Sin(cylinderNormals[currentVertex].y) / Math.PI + 0.5f));

                    currentVertex++;

                }
                currentHeight += heightStep;
                currentRadius += radiusStep;
            }
            cylinderVertices[currentVertex] = new Vector3(length / 2, 0, 0);
            cylinderNormals[currentVertex] = Vector3.up;
            currentVertex++;

            mesh.vertices = cylinderVertices;
            mesh.normals = cylinderNormals;
            mesh.uv = cylinderUVs;
            mesh.triangles = CreateIndexBuffer(vertexCount, indexCount, slices);

            return mesh;
        }


        /// <summary>
        /// Creates an index buffer for spherical shapes like Spheres, Cylinders, and Cones.
        /// </summary>
        /// <param name="vertexCount">The total number of vertices making up the shape.</param>
        /// <param name="indexCount">The total number of indices making up the shape.</param>
        /// <param name="slices">The number of slices about the Y axis.</param>
        /// <returns>The index buffer containing the index data for the shape.</returns>
        private static int[] CreateIndexBuffer(int vertexCount, int indexCount, int slices)
        {
            int[] indices = new int[indexCount];
            int currentIndex = 0;

            // Bottom circle/cone of shape
            for (int i = 1; i <= slices; i++)
            {
                indices[currentIndex++] = i;
                indices[currentIndex++] = 0;
                if (i - 1 == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
            }

            // Middle sides of shape
            for (int i = 1; i < vertexCount - slices - 1; i++)
            {
                indices[currentIndex++] = i + slices;
                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;

                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;
            }

            // Top circle/cone of shape
            for (int i = vertexCount - slices - 1; i < vertexCount - 1; i++)
            {
                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
                indices[currentIndex++] = vertexCount - 1;
            }

            return indices;
        }
    }
}
