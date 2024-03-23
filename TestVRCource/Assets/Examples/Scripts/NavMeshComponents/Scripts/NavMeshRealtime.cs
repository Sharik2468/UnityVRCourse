using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshRealtime : MonoBehaviour
{
    public float DelayForBake;
    NavMeshSurface navMeshSurface;
    // Start is called before the first frame update
    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        Invoke("UpdateNavMesh", DelayForBake);
    }

    private void UpdateNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
}
