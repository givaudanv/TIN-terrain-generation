using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

class TextureInfo
{
    public string uuid;
    public string imagePath;
    public string infoPath;
}

public class TINMapGenerator : MonoBehaviour
{
    [SerializeField]
    private string resourcePath;

    [SerializeField]
    private GameObject chunkPrefab;

    public void DeleteChildren()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }
    }

    public void GenerateMap()
    {
        //Make sure there is no duplicate
        DeleteChildren();

        FileInfo gml = new FileInfo(Application.dataPath + "/Resources/" + resourcePath + ".gml");
        FileStream stream = gml.OpenRead();
        
        using (XmlReader reader = XmlReader.Create(stream))
        {

            Dictionary<string, TextureInfo> textures = new Dictionary<string, TextureInfo>();
            //Textures Context Variables
            TextureInfo textureInfo = null;
            bool parsingSurface = false;

            //Context Variables
            GameObject child = null;
            MeshFilter meshFilter = null;
            MeshRenderer meshRenderer = null;
            List<Vector3> vertices = null;
            List<Vector2> uvs = null;

            reader.MoveToContent();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //Image Node
                        if (reader.Name == "app:GeoreferencedTexture")
                        {
                            textureInfo = new TextureInfo();
                        }

                        //Image Path
                        if (reader.Name == "app:imageURI")
                        {
                            textureInfo.imagePath = reader.ReadInnerXml();
                            textureInfo.infoPath = Path.ChangeExtension(textureInfo.imagePath, ".jgw");
                        }

                        //Image UUID
                        if (reader.Name == "app:target")
                        {
                            textureInfo.uuid = reader.ReadInnerXml().Substring(1); // ignore #
                        }

                        //New Chunk
                        if (reader.Name == "cityObjectMember")
                        {
                            child = Instantiate(chunkPrefab, transform);
                            meshRenderer = child.GetComponent<MeshRenderer>();
                            meshFilter = child.GetComponent<MeshFilter>();
                        }

                        //New Chunk Data (Texture in attribute and triangles in children)
                        if (reader.Name == "gml:TriangulatedSurface")
                        {
                            vertices = new List<Vector3>();
                            uvs = new List<Vector2>();
                            parsingSurface = true;
                        }

                        if (reader.Name == "gml:posList")
                        {
                            //Extract Vertices
                            string[] positions = reader.ReadInnerXml().Split(' ');
                            int index = 0;
                            Vector3 a = ParseVector3(positions[index++], positions[index++], positions[index++]);
                            Vector3 b = ParseVector3(positions[index++], positions[index++], positions[index++]);
                            Vector3 c = ParseVector3(positions[index++], positions[index++], positions[index++]);
                            vertices.Add(c);
                            vertices.Add(b);
                            vertices.Add(a);

                            if (textureInfo != null)
                            {
                                FileInfo imagePath = new FileInfo(gml.Directory.FullName + "/" + textureInfo.imagePath);
                                FileInfo infoPath = new FileInfo(gml.Directory.FullName + "/" + textureInfo.infoPath);

                                if (infoPath.Exists && imagePath.Exists)
                                {
                                    using (StreamReader infoReader = new StreamReader(infoPath.OpenRead()))
                                    {
                                        /*Vector2 dx = new Vector2(
                                            float.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture),
                                            float.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture));*/
                                        float d = float.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture);
                                        //Debug.Log(d);
                                        infoReader.ReadLine();
                                        
                                        Vector2 dy = new Vector2(
                                            float.Parse(infoReader.ReadLine(),  System.Globalization.CultureInfo.InvariantCulture),
                                            float.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture));

                                        /*Vector2 offset = new Vector2(
                                            float.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture),
                                            float.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture));*/
                                        double offx = double.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture);
                                        double offy = double.Parse(infoReader.ReadLine(), System.Globalization.CultureInfo.InvariantCulture);
                                        Vector3 offset = new Vector3(offx, offy);
                                        Debug.Log(offx);

                                        /*uvs.Add(ComputeUV(a, offset, dx, dy));
                                        uvs.Add(ComputeUV(b, offset, dx, dy));
                                        uvs.Add(ComputeUV(c, offset, dx, dy));*/
                                    }

                                    meshRenderer.sharedMaterial.SetTexture("_MainTex",Resources.Load<Texture2D>(imagePath.FullName));
                                }
                            }
                        }
                        break;

                    case XmlNodeType.Attribute:
                        if (parsingSurface && reader.Name == "gml:id")
                        {
                            if (textures.ContainsKey(reader.Value))
                            {
                                textureInfo = textures[reader.Value];
                            }
                            else
                            {
                                textureInfo = null;
                            }
                        }
                    break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == "app:GeoreferencedTexture")
                        {
                            textures.Add(textureInfo.uuid, textureInfo);
                        }

                        if (reader.Name == "gml:TriangulatedSurface")
                        {
                            parsingSurface = false;
                            textureInfo = null;

                            int[] indices = new int[vertices.Count];
                            for (var i = 0; i < vertices.Count; i++)
                            {
                                indices[i] = i;
                            }

                            Mesh mesh = new Mesh();
                            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                            mesh.vertices = vertices.ToArray();
                            mesh.uv = uvs.ToArray();
                            mesh.triangles = indices;
                            //if (mesh.uv[0] != null) Debug.Log(mesh.uv[0]);

                            meshFilter.sharedMesh = mesh;
                            child = null;
                        }
                    break;
                }
            }
        }
    }

    private static Vector2 ComputeUV(Vector3 position, Vector2 texturePosition, Vector2 dx, Vector2 dy)
    {
        Vector2 dp = new Vector2(position.x, position.y) - texturePosition;
        return dp.x * dx + dp.y * dy;
    }

    private static Vector3 ParseVector3(string x, string y, string z)
    {
        return new Vector3(
            float.Parse(x, System.Globalization.CultureInfo.InvariantCulture),
            float.Parse(z, System.Globalization.CultureInfo.InvariantCulture),
            float.Parse(y, System.Globalization.CultureInfo.InvariantCulture)
            );
    }
}