using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour {

    public Material mat;//材质
    public Vector2 center;//中心点
    public float radius;//半径
    public int segmentCount = 12;//分段数

    private float deltaAngle;//角度变化量
    private float step = 0;//当前角度
    private bool catched = false;//如果多个圆 那么就不用这个了 仿照Bezier中的index即可
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float distance = Vector2.Distance(Vector3.Scale(new Vector3(1.0f / Screen.width, 1.0f / Screen.height, 0), Input.mousePosition), center) - radius;
            if (distance < 0.02f)//±0.05 但是为了效率所以前面加了 此处减少一个判断或取绝对值
            {
                catched = true;
            }
        }
         if (Input.GetMouseButton(0) && catched)
        {
            radius = Vector2.Distance(Vector3.Scale(new Vector3(1.0f / Screen.width, 1.0f / Screen.height, 0), Input.mousePosition), center);
            print(radius);
        }
         if (Input.GetMouseButtonUp(0) && catched)
        {
            catched = false;
        }
    }

    private void OnPostRender()
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        mat.SetPass(0);

        deltaAngle = Mathf.Deg2Rad * 360 / segmentCount;

        step = 0;
        GL.Begin(GL.LINE_STRIP);
        for(int i = 0; i < segmentCount; ++i)
        {
            float cosA = Mathf.Cos(step);
            float sinA = Mathf.Sin(step);
            GL.Vertex(center + new Vector2(radius * cosA, radius * sinA));
            step += deltaAngle;
        }
        //GL.Vertex(center + new Vector2(radius, 0));
        GL.End();

        GL.PopMatrix();
    }
}
