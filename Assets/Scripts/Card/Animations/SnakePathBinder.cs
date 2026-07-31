using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SnakePathBinder : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] workingVertices;

    public float maxLength;

    public float speed;
    public float amplitude;
    public float frequency;

    private Vector3[] currentPath;
    private float progress = 0f;
    private float totalPathLength = 0f;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = Instantiate(mf.sharedMesh);
        mf.mesh = mesh;
        baseVertices = mesh.vertices;
        workingVertices = new Vector3[baseVertices.Length];
        mesh.MarkDynamic();
    }

    public void SetState(Vector3[] path, float p)
    {
        currentPath = path;
        progress = p;

        if (totalPathLength <= 0 && path != null)
        {
            totalPathLength = 0;
            for (int i = 0; i < path.Length - 1; i++)
                totalPathLength += Vector3.Distance(path[i], path[i + 1]);
        }
    }

    void LateUpdate()
    {
        if (currentPath == null || currentPath.Length < 2 || totalPathLength <= 0) return;

        float headDist = progress * totalPathLength;
        float timeOffset = Time.time * speed;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 vertice = baseVertices[i];
            float vertexDistOnPath = headDist + vertice.y;

            if (vertexDistOnPath < 0) vertexDistOnPath = 0;

            Vector3 basePos = GetPointOnPath(vertexDistOnPath);
            Vector3 forward = GetDirectionOnPath(vertexDistOnPath);
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            float slitherWave = Mathf.Sin(vertexDistOnPath * frequency + timeOffset);
            float headFade = Mathf.Clamp01(Mathf.Abs(vertice.y) * 2f);
            Vector3 slitherOffset = right * (slitherWave * amplitude * headFade);
            Vector3 finalWorldPos = basePos + slitherOffset + (right * vertice.x) + (up * vertice.z);

            workingVertices[i] = transform.InverseTransformPoint(finalWorldPos);
        }

        mesh.vertices = workingVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private Vector3 GetPointOnPath(float dist)
    {
        float current = 0;
        for (int i = 0; i < currentPath.Length - 1; i++)
        {
            float step = Vector3.Distance(currentPath[i], currentPath[i + 1]);
            if (current + step >= dist)
                return Vector3.Lerp(currentPath[i], currentPath[i + 1], (dist - current) / step);
            current += step;
        }
        return currentPath[currentPath.Length - 1];
    }

    private Vector3 GetDirectionOnPath(float dist)
    {
        float current = 0;
        for (int i = 0; i < currentPath.Length - 1; i++)
        {
            float step = Vector3.Distance(currentPath[i], currentPath[i + 1]);
            if (current + step >= dist)
                return (currentPath[i + 1] - currentPath[i]).normalized;
            current += step;
        }
        return (currentPath[currentPath.Length - 1] - currentPath[currentPath.Length - 2]).normalized;
    }
}