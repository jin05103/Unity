using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NetworkedAnnulusMesh : NetworkBehaviour
{
    MeshFilter mf;

    public NetworkVariable<float> innerRadius = new(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> outerRadius = new(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Vector2> zoneCenter = new(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [Range(3, 128)] public int segments = 64;

    float prevInner, prevOuter;
    Vector2 prevCenter;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        prevInner = innerRadius.Value;
        prevOuter = outerRadius.Value;
        prevCenter = zoneCenter.Value;
        UpdateMesh();
    }

    void Update()
    {
        if (prevInner != innerRadius.Value || prevOuter != outerRadius.Value)
        {
            UpdateMesh();
            prevInner = innerRadius.Value;
            prevOuter = outerRadius.Value;
        }
        
        if (prevCenter != zoneCenter.Value)
        {
            transform.position = new Vector3(zoneCenter.Value.x, zoneCenter.Value.y, 0);
            prevCenter = zoneCenter.Value;
        }
    }

    void UpdateMesh()
    {
        mf.mesh = MakeAnnulus(innerRadius.Value, outerRadius.Value, segments);
    }

    Mesh MakeAnnulus(float r0, float r1, int segs)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segs * 2];
        int[] triangles = new int[segs * 6];
        float angleStep = Mathf.PI * 2f / segs;
        for (int i = 0; i < segs; i++)
        {
            float a = i * angleStep;
            float ca = Mathf.Cos(a), sa = Mathf.Sin(a);
            vertices[i * 2] = new Vector3(ca * r0, sa * r0, 0);
            vertices[i * 2 + 1] = new Vector3(ca * r1, sa * r1, 0);

            int ni = (i + 1) % segs;
            int ti = i * 6;
            triangles[ti] = i * 2;
            triangles[ti + 1] = ni * 2 + 1;
            triangles[ti + 2] = ni * 2;
            triangles[ti + 3] = i * 2;
            triangles[ti + 4] = i * 2 + 1;
            triangles[ti + 5] = ni * 2 + 1;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}