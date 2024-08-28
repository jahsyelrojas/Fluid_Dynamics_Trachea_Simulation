using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TubeGenerator : MonoBehaviour
{
    public float radius = 0.5f;
    public float height = 5f;
    public int radialSegments = 16;
    public int heightSegments = 20;
    public Material tubeMaterial; // Material for the tube

    private void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        mf.mesh = CreateTube();
        if (tubeMaterial != null)
        {
            mr.material = tubeMaterial;
        }
        else
        {
            Debug.LogError("Please assign a material to the TubeGenerator.");
        }

        // Position the tube vertically
        transform.position = new Vector3(transform.position.x, height / 2, transform.position.z);
    }

    private Mesh CreateTube()
    {
        Mesh mesh = new Mesh();
        
        int vertexCount = (radialSegments + 1) * (heightSegments + 1);
        int triangleCount = radialSegments * heightSegments * 6;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        float angleStep = 2 * Mathf.PI / radialSegments;
        float heightStep = height / heightSegments;

        int vertexIndex = 0;
        for (int i = 0; i <= heightSegments; i++)
        {
            float y = i * heightStep;
            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = j * angleStep;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                
                vertices[vertexIndex] = new Vector3(x, y, z);
                normals[vertexIndex] = new Vector3(x, 0, z).normalized;
                uvs[vertexIndex] = new Vector2((float)j / radialSegments, (float)i / heightSegments);

                if (i < heightSegments && j < radialSegments)
                {
                    int start = vertexIndex;
                    int next = start + radialSegments + 1;

                    triangles[6 * (i * radialSegments + j)] = start;
                    triangles[6 * (i * radialSegments + j) + 1] = next;
                    triangles[6 * (i * radialSegments + j) + 2] = start + 1;
                    triangles[6 * (i * radialSegments + j) + 3] = start + 1;
                    triangles[6 * (i * radialSegments + j) + 4] = next;
                    triangles[6 * (i * radialSegments + j) + 5] = next + 1;
                }

                vertexIndex++;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        return mesh;
    }
}
