using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class TextureTerrainParser : MonoBehaviour
{
    [SerializeField]
    private Texture2D terrainTexture;
    [SerializeField]
    private Vector3 resolution = new Vector3(100, 100, 30);

    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void CreateTerrain()
    {
        int width = Mathf.CeilToInt(resolution.x);
        int height = Mathf.CeilToInt(resolution.y);

        //Generate vertices
        Vector3[] vertices = new Vector3[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int u = Mathf.FloorToInt(x / (float)width * terrainTexture.width);
                int v = Mathf.FloorToInt(y / (float)height * terrainTexture.height);
             
                float z = Mathf.Lerp(0, resolution.z, terrainTexture.GetPixel(u, v).grayscale);
                vertices[x + y * width] = new Vector3(x, z, y);
            }
        }

        int[] indices = new int[(width - 1) * (height - 1) * 6];
        int index = 0;
        for (int x = 0; x < (width - 1); x++)
        {
            for (int y = 0; y < (height - 1); y++)
            {
                //First Triangle
                indices[index++] = x + 1 + (y + 1) * (width);
                indices[index++] = x + 1 + y * (width);
                indices[index++] = x + y * (width);
                //Second Triangle
                indices[index++] = x + y * (width);
                indices[index++] = x + (y + 1) * (width);
                indices[index++] = x + 1 + (y + 1) * (width);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = indices;

        meshFilter.sharedMesh = mesh;
    }
}
