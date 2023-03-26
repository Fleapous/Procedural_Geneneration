using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeMatrix : MonoBehaviour
{
    [SerializeField] private int size;
    private Mesh mesh;
    private List<Vector3> vectorList;
    private List<int> tris;


    void MakeVertex(int n, List<Vector3> vectors)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                vectors.Add(new Vector3(j, 0, i));
            }
        }
    }

    void MakeTris(int n, List<int> ints)
    {
        int k = 0;
        for (int i = 0; i < Mathf.Pow(n -1, 2); i++)
        {
            ints.Add(k);
            ints.Add(k + 1);
            ints.Add(k + n);
            
            ints.Add(k + 1);
            ints.Add(k + n + 1);
            ints.Add(k + n);

            if ((k + 1) % n == n - 1)
            {
                k++;
            }

            k++;
        }
    }
    void Update()
    {
        vectorList = new List<Vector3>();
        tris = new List<int>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        MakeVertex(size, vectorList);
        MakeTris(size, tris);
        
        Vector3[] vectorArr = vectorList.ToArray();
        int[] trisArr = tris.ToArray();
        
        mesh.Clear();
        mesh.vertices = vectorArr;
        mesh.triangles = trisArr;
        mesh.RecalculateNormals();

    }
}
