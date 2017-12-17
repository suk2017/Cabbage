using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour
{
    public float gizmoWidth = 1;//锚点框的宽度
    public Material Mat;//用于显示辅助线的材质
    public Material BezierMat;//用于显示贝塞尔线的材质
    public Vector4 rect = new Vector4(0.1f, 0.9f, 0.1f, 0.9f);//防止锚点被移动到错误的地方的边框
    public int oldLength = 120;//上一帧的length
    public int length = 120;//完成一次绘制的帧数 帧/次
    public Vector2[] vec;//锚点

    private int step = 0;//指示当前过程的步子
    private List<Vector2> Lines;//记录辅助线
    private List<Vector2> BezierLine;//记录贝塞尔线
    private int index = -1;//被选中的锚点序号
    private bool showBase = true;//显示框线
    private bool showAuxiliaryLine = true;//显示辅助线
    private bool showAnchor = true;//显示锚点
    private bool showBezier = true;//显示贝塞尔曲线

    //初始化时调用一次
    private void Start()
    {
        //初始化
        Lines = new List<Vector2>();
        BezierLine = new List<Vector2>();
    }

    //每帧调用一次
    private void Update()
    {
        //获取鼠标位置 并对单位进行统一
        float u = Input.mousePosition.x / Screen.width;
        float v = Input.mousePosition.y / Screen.height;
        Vector2 m = new Vector2(u, v);

        //如果按下鼠标左键：选取某个锚点
        if (Input.GetMouseButtonDown(0))
        {
            float nearest = 0.01f;
            index = -1;
            for (int i = 0; i < vec.Length; ++i)
            {
                float len = (vec[i] - m).sqrMagnitude;
                if (len < nearest)
                {
                    nearest = len;
                    index = i;
                }
            }
        }
        //如果拖动鼠标左键：拖动某个锚点
        if (Input.GetMouseButton(0))
        {
            if (index >= 0 && index < vec.Length)
            {
                //防止越过边框
                if (m.x < rect.x)
                {
                    m.x = rect.x + 0.01f;
                }
                else if (m.x > rect.y)
                {
                    m.x = rect.y - 0.01f;
                }
                if (m.y < rect.z)
                {
                    m.y = rect.z + 0.01f;
                }
                else if (m.y > rect.w)
                {
                    m.y = rect.w - 0.01f;
                }
                vec[index] = m;
            }
        }
        //如果按下ESC键：退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            //如果是在调试模式下 则停止调试
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }

    //绘制图形界面 每帧调用一次
    private void OnGUI()
    {
        GUILayout.Label("执行帧数（越大越慢）：");
        //获得length内容 若获得失败则赋值为10
        try
        {
            oldLength = length;
            length = int.Parse(GUILayout.TextField(length + ""));
        }
        catch (System.Exception)
        {
            length = 10;
        }

        //若length太小 则赋值为10
        if (length <= 5)
        {
            length = 10;
        }

        //若改变过length 则重绘画面
        if (oldLength != length)
        {
            clear();
        }
        if (GUILayout.Button("手动清屏"))
        {
            clear();
        }
        if (GUILayout.Button("加一个点"))
        {
            Vector2[] newVec = new Vector2[vec.Length + 1];
            for(int i = 0; i < vec.Length; ++i)
            {
                newVec[i] = vec[i];
            }
            newVec[newVec.Length - 1] = new Vector2(0.5f, 0.5f);
            vec = newVec;
            clear();
        }
        if (GUILayout.Button("减一个点"))
        {
            if(vec.Length > 2)
            {
                Vector2[] newVec = new Vector2[vec.Length - 1];
                for (int i = 0; i < newVec.Length; ++i)
                {
                    newVec[i] = vec[i];
                }
                vec = newVec;
                clear();
            }
        }
        showBase = GUILayout.Toggle(showBase, "绘制框线");
        showAuxiliaryLine = GUILayout.Toggle(showAuxiliaryLine, "绘制辅助线");
        showAnchor = GUILayout.Toggle(showAnchor, "绘制锚点");
        showBezier = GUILayout.Toggle(showBezier, "贝塞尔曲线");

        GUILayout.Label("按Esc退出");
        GUILayout.Label("拖动小框移动锚点");
    }

    //绘制OpenGL内容 每帧调用一次
    private void OnPostRender()
    {
        //GL.Clear(true, true, Color.black);//如果使用这个方法清屏 那么将同时清除GUI！
        GL.PushMatrix();//矩阵入栈
        GL.LoadOrtho();//使用二维世界

        
        ++step;//控制过程步
        step %= length;//若绘制完成则重绘

        Lines.Clear();//每帧清空框线

        BezierLine.Add(_Bezier(vec, step * 1.0f / length));//载入当前帧新计算出的贝塞尔曲线
        if (BezierLine.Count == length)//若绘制完成则清空
        {
            BezierLine.Clear();
        }

        //绘制可控制点
        if (showAnchor)
        {
            Mat.SetPass(0);//设置材质
            for (int i = 0; i < vec.Length; ++i)
            {
                GL.Begin(GL.LINE_STRIP);//绘制直线 按序列：12 23 34 45 ...
                GL.Vertex(vec[i] + new Vector2(-gizmoWidth / 2, -gizmoWidth / 2));
                GL.Vertex(vec[i] + new Vector2(-gizmoWidth / 2, gizmoWidth / 2));
                GL.Vertex(vec[i] + new Vector2(gizmoWidth / 2, gizmoWidth / 2));
                GL.Vertex(vec[i] + new Vector2(gizmoWidth / 2, -gizmoWidth / 2));
                GL.Vertex(vec[i] + new Vector2(-gizmoWidth / 2, -gizmoWidth / 2));
                GL.End();
            }
        }

        //显示边框 可控制的点不能越过边框s
        Vector2 LeftBottom = new Vector2(rect.x, rect.z);
        Vector2 LeftTop = new Vector2(rect.x, rect.w);
        Vector2 RightTop = new Vector2(rect.y, rect.w);
        Vector2 RightBottom = new Vector2(rect.y, rect.z);
        GL.Begin(GL.LINE_STRIP);
        GL.Vertex(LeftBottom + new Vector2(-gizmoWidth / 2, -gizmoWidth / 2));
        GL.Vertex(LeftTop + new Vector2(-gizmoWidth / 2, gizmoWidth / 2));
        GL.Vertex(RightTop + new Vector2(gizmoWidth / 2, gizmoWidth / 2));
        GL.Vertex(RightBottom + new Vector2(gizmoWidth / 2, -gizmoWidth / 2));
        GL.Vertex(LeftBottom + new Vector2(-gizmoWidth / 2, -gizmoWidth / 2));
        GL.End();

        //显示辅助线
        GL.Begin(GL.LINES);//绘制直线 按序列：12 34 56 78 ...
        for (int i = 0; i < Lines.Count - 1; i += 2)
        {
            GL.Vertex(Lines[i]);
            GL.Vertex(Lines[i + 1]);
        }
        GL.End();


        //显示贝塞尔曲线
        if (showBezier)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.Color(new Color(0.5f, 0.7f, 1));
            for (int i = 0; i < BezierLine.Count; ++i)
            {
                GL.Vertex(BezierLine[i]);
            }
            GL.End();
        }

        GL.PopMatrix();//矩阵出栈

    }


    //清空重绘
    private void clear()
    {
        step = 0;
        Lines.Clear();
        BezierLine.Clear();
    }

    //绘制任意阶的贝塞尔曲线 阶数由v的长度控制
    private Vector2 _Bezier(Vector2[] v, float t)
    {
        if (v.Length == 2)
        {
            if (showBase)//绘制不移动的框线
            {
                Lines.Add(v[0]);
                Lines.Add(v[1]);
            }
            return (1 - t) * v[0] + t * v[1];
        }
        Vector2[] v1 = new Vector2[v.Length - 1];
        Vector2[] v2 = new Vector2[v.Length - 1];
        for (int i = 0; i < v.Length - 1; ++i)
        {
            v1[i] = v[i];
            v2[i] = v[i + 1];
        }
        //分别对前(n-1)和后(n-1)个点进行低一阶的计算
        Vector2 u1 = _Bezier(v1, t);
        Vector2 u2 = _Bezier(v2, t);
        if (showAuxiliaryLine)//绘制移动的框线（辅助线
        {
            Lines.Add(u1);
            Lines.Add(u2);
        }
        return (1 - t) * u1 + t * u2;
    }

}
