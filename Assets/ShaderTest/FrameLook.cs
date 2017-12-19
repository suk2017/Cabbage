using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLook : MonoBehaviour
{
    public Material mat;
    public MeshFilter[] meshf;

    private int length;
    private Matrix4x4 W2C;
    private Vector3[][] vertices;
    private int[][] tris;
    private Transform[] tr;

    private void Start()
    {
        length = meshf.Length;
        vertices = new Vector3[length][];
        tris = new int[length][];
        tr = new Transform[length];
        for(int i = 0; i < length; ++i)
        {
            vertices[i] = meshf[i].mesh.vertices;
            tris[i] = meshf[i].mesh.triangles;
            tr[i] = meshf[i].transform;
        }
        W2C = Camera.main.worldToCameraMatrix;
    }

    private void OnPostRender()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        
        for(int j = 0; j < length; ++j)
        {
            GL.Begin(GL.LINE_STRIP);
            int _length = tris[j].Length ;
            for (int i = 0; i < _length; i += 3)
            {
                GL.Vertex(Mtx(tr[j],vertices[j][tris[j][i]]));
                GL.Vertex(Mtx(tr[j],vertices[j][tris[j][i + 1]]));
                GL.Vertex(Mtx(tr[j],vertices[j][tris[j][i + 2]]));
                GL.Vertex(Mtx(tr[j],vertices[j][tris[j][i]]));
            }
            GL.End();
        }

        GL.PopMatrix();
    }

    private Vector3 Mtx(Transform t,Vector3 vec)
    {
        Vector3 pos = Vector3.Scale(t.position, new Vector3(1, 1, -1));

        Matrix4x4[] mtxs =
        {
            Matrix4x4.Scale(t.localScale)
            ,Matrix4x4.Rotate(new Quaternion(t.rotation.x, t.rotation.y, -t.rotation.z, -t.rotation.w))
            ,Matrix4x4.Translate(pos)
        };

        for (int i = 0; i < mtxs.Length; ++i)
        {
            vec = mtxs[i].MultiplyPoint3x4(vec);
        }
        return W2C.MultiplyVector( vec);
    }
}
