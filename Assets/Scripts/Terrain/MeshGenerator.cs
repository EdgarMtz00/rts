using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public static class MeshGenerator
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier,
            AnimationCurve heightCurve, int levelOfDetail)
        {
            AnimationCurve curve = new AnimationCurve(heightCurve.keys);
            
            int simplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

            int borderSize = heightMap.GetLength(0);
            int meshSize = borderSize - 2 * simplificationIncrement;
            int unsimplifiedMeshSize = borderSize - 2;

            float topLeftX = (unsimplifiedMeshSize - 1) / -2f;
            float topLeftZ = (unsimplifiedMeshSize - 1) / 2f;

            int verticesPerLine = (meshSize - 1) / simplificationIncrement + 1;

            MeshData meshData = new MeshData(verticesPerLine);
            int[][] vertexIndicesMap = new int[borderSize][];
            for (int index = 0; index < borderSize; index++)
            {
                vertexIndicesMap[index] = new int[borderSize];
            }

            int meshVertexIndex = 0;
            int borderVertexIndex = -1;

            for (int y = 0; y < borderSize; y += simplificationIncrement)
            {
                for (int x = 0; x < borderSize; x += simplificationIncrement)
                {
                    bool isBorderVertex = y == 0 || y == borderSize - 1 || x == 0 || x == borderSize - 1;
                    if (isBorderVertex)
                    {
                        vertexIndicesMap[x][y] = borderVertexIndex;
                        borderVertexIndex--;
                    }
                    else
                    {
                        vertexIndicesMap[x][y] = meshVertexIndex;
                        meshVertexIndex++;
                    }
                }
            }

            for (int y = 0; y < borderSize; y += simplificationIncrement)
            {
                for (int x = 0; x < borderSize; x += simplificationIncrement)
                {
                    int vertexIndex = vertexIndicesMap[x][y];

                    Vector2 percent = new Vector2((x - simplificationIncrement) / (float) meshSize,
                        (y - simplificationIncrement) / (float) meshSize);
                    float height = curve.Evaluate(heightMap[x, y]) * heightMap[x, y] * heightMultiplier;
                    Vector3 vertexPosition = new Vector3(topLeftX + percent.x * unsimplifiedMeshSize, height,
                        topLeftZ - percent.y * unsimplifiedMeshSize);

                    meshData.AddVertex(vertexPosition, percent, vertexIndex);
                    
                    if (x < borderSize - 1 && y < borderSize - 1)
                    {
                        int a = vertexIndicesMap[x][y];
                        int b = vertexIndicesMap[x + simplificationIncrement][y];
                        int c = vertexIndicesMap[x][y + simplificationIncrement];
                        int d = vertexIndicesMap[x + simplificationIncrement][y + simplificationIncrement];
                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }
                }
            }

            meshData.BakeNormals();
            return meshData;
        }
    }

    public class MeshData
    {
        private readonly Vector3[] _vertices;
        private readonly int[] _triangles;
        private readonly Vector2[] _uvs;
        private Vector3[] _bakedNormals;

        private Vector3[] _borderVertices;
        private int[] _borderTriangles;
        
        private int _triangleIndex;
        private int _borderTriangleIndex;

        public MeshData(int verticesPerLine)
        {
            _vertices = new Vector3[verticesPerLine * verticesPerLine];
            _triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];
            _uvs = new Vector2[verticesPerLine * verticesPerLine];

            _borderVertices = new Vector3[verticesPerLine * 4 + 4];
            _borderTriangles = new int[24 * verticesPerLine];
        }

        public void AddVertex(Vector3 position, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0)
            {
                _borderVertices[-vertexIndex - 1] = position;
            }
            else
            {
                _vertices[vertexIndex] = position;
                _uvs[vertexIndex] = uv;
            }
        }

        public void AddTriangle(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
            {
                _borderTriangles[_borderTriangleIndex] = a;
                _borderTriangles[_borderTriangleIndex + 1] = b;
                _borderTriangles[_borderTriangleIndex + 2] = c;
                _borderTriangleIndex += 3;
            }
            else
            {
                _triangles[_triangleIndex] = a;
                _triangles[_triangleIndex + 1] = b;
                _triangles[_triangleIndex + 2] = c;
                _triangleIndex += 3;
            }
        }

        Vector3[] CalculateNormals()
        {
            Vector3[] vertexNormals = new Vector3[_vertices.Length];
            int triangleCount = _triangles.Length / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = _triangles[normalTriangleIndex];
                int vertexIndexB = _triangles[normalTriangleIndex + 1];
                int vertexIndexC = _triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }
            
            int borderTriangleCount = _borderTriangles.Length / 3;
            for (int i = 0; i < borderTriangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = _borderTriangles[normalTriangleIndex];
                int vertexIndexB = _borderTriangles[normalTriangleIndex + 1];
                int vertexIndexC = _borderTriangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                if (vertexIndexA >= 0)
                {
                    vertexNormals[vertexIndexA] += triangleNormal;
                }

                if (vertexIndexB >= 0)
                {
                    vertexNormals[vertexIndexB] += triangleNormal;
                }

                if (vertexIndexC >= 0)
                {
                    vertexNormals[vertexIndexC] += triangleNormal;
                }
            }

            foreach (Vector3 normal in vertexNormals)
            {
                normal.Normalize();
            }

            return vertexNormals;
        }

        Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = (indexA < 0) ? _borderVertices[-indexA - 1] : _vertices[indexA];
            Vector3 pointB = (indexB < 0) ? _borderVertices[-indexB - 1] : _vertices[indexB];
            Vector3 pointC = (indexC < 0) ? _borderVertices[-indexC - 1] : _vertices[indexC];

            Vector3 sideAb = pointB - pointA;
            Vector3 sideAc = pointC - pointA;
            return Vector3.Cross(sideAb, sideAc).normalized;
        }

        public void BakeNormals()
        {
            _bakedNormals = CalculateNormals();
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = _vertices;
            mesh.triangles = _triangles;
            mesh.uv = _uvs;
            mesh.normals = _bakedNormals;
            return mesh;
        }
    }
}