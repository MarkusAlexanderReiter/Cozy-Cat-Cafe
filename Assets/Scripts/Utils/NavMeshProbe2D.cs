using System.Collections;
using UnityEngine;
using UnityEngine.AI;          // NavMesh / NavMeshAgent
using Unity.AI.Navigation;     // NavMeshSurface  <-- add this

public class NavMeshProbe2D : MonoBehaviour
{
    public NavMeshSurface surface;

    private IEnumerator Start()
    {
        surface.RemoveData();
        surface.BuildNavMesh();       // or surface.AddData() if you baked offline
        yield return null;

        var tri = NavMesh.CalculateTriangulation();
        Debug.Log($"NavMesh verts: {tri.vertices.Length}, tris: {tri.indices.Length / 3}");
    }
}
