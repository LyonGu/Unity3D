using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawImage : Image
{
    public List<Vector2> pointer = new List<Vector2>();
    public Vector2 offset = new Vector2(5, 5);
    public float LineWidth = 10.0f;
    private List<Vector2> tempPointer;
    private int headPointCount = 3; //头部顶点数量
    private VertexHelper toFill;

    protected override void Awake()
    {
        base.Awake();
        tempPointer = new List<Vector2>();
        pointer.Clear();
        //Vector2 center = new Vector2(100, 100);
        //AddRectPoint(center, this.offset);

        //Vector2 center1 = new Vector2(100, 125);
        //AddRectPoint(center1, this.offset);

        //Vector2 center2 = new Vector2(100, 150);
        //AddRectPoint(center2, this.offset);

        //Vector2 center3 = new Vector2(100, 175);
        //AddRectPoint(center3, this.offset);


        //Vector2 center4 = new Vector2(125, 200);
        ////center4 = RotatePoint(center4, 45);
        ////float angle = GetAngle(center3, center4);
        ////Debug.Log($"angle ========{angle}");

        ////AddRectPoint(center3, this.offset);
        //AddRectPoint(center4, this.offset);

        //var dir = center4 - center3;


        //Vector2 startUP = RotateVector2(90, center4, center3);
        //Vector2 startDown = RotateVector2(-90, center4, center3);

        //var dir_starUp = startUP - center3;
        //var dir_starDown = startDown - center3;
        //startUP = dir_starUp.normalized *2f + center3;
        //startDown = dir_starDown.normalized * 2f + center3;

        //Vector2 endUP = dir + startUP;
        //Vector2 endDown = dir + startDown;




        ////AddRectPoint(startUP, this.offset);
        ////AddRectPoint(startDown, this.offset);
        ////AddRectPoint(endUP, this.offset);
        ////AddRectPoint(endDown, this.offset);

        //pointer.Add(startUP);
        //pointer.Add(endUP);
        //pointer.Add(endDown);
        //pointer.Add(startDown);

        //AddRectPointByAngle(center4, this.offset,angle);


        //AddRectPoint(center4, this.offset);

        ////Line
        //center = new Vector2(100, 50);
        //center1 = new Vector2(150, 100);
        //center = center3;
        //center1 = center4;



        //DrawLine(center3, center4);

        //center = new Vector2(300, 200);
        //center1 = new Vector2(350, 250);
        //DrawLine(center, center1);

        //center = new Vector2(300, 200);
        //center1 = new Vector2(300, 250);
        //DrawLine(center, center1);

        //center = new Vector2(300, 200);
        //center1 = new Vector2(350, 150);
        //DrawLine(center, center1);

        //center = new Vector2(300, 200);
        //center1 = new Vector2(300, 150);
        //DrawLine(center, center1);

        //center = new Vector2(300, 200);
        //center1 = new Vector2(250, 150);
        //DrawLine(center, center1);

        //center = new Vector2(300, 200);
        //center1 = new Vector2(250, 200);
        //DrawLine(center, center1);

        //center = new Vector2(300, 200);
        //center1 = new Vector2(250, 250);
        //DrawLine(center, center1);

        //var newCenter = (center + center1) * 0.5f;
        //var offsetX = Mathf.Abs(center.x - center1.x) * 0.5f;
        //var offsetY = Mathf.Abs(center.y - center1.y) * 0.5f;

        //var newOffset = new Vector2(offsetX, offsetY);
        //float angle = GetAngle(center, center1);
        //AddRectPointByAngle(newCenter, newOffset, angle);

    }

    public void DrawLineEx(Vector2 start, Vector2 end)
    {
        if (start.Equals(end))
            return;
        Vector2 startUP = RotateVector2(90, end, start);
        Vector2 startDown = RotateVector2(-90, end, start);
        var dir = end - start;
        var dir_starUp = startUP - start;
        var dir_starDown = startDown - start;
        startUP = dir_starUp.normalized * LineWidth + start;
        startDown = dir_starDown.normalized * LineWidth + start;

        Vector2 endUP = dir + startUP;
        Vector2 endDown = dir + startDown;

        pointer.Add(startUP);
        pointer.Add(endUP);
        pointer.Add(endDown);
        pointer.Add(startDown);


        //添加左边头部
        float angel = 180 / (headPointCount + 1);
        tempPointer.Add(startDown);
        for (float i = -angel; i > -180; i -= angel)
        {
            tempPointer.Add(RotateVector2(i, startDown, start));
        }

        tempPointer.Add(startUP);

        for (int i = 1; i < tempPointer.Count; i++)
        {
            AddVert(this.toFill, tempPointer[i - 1], tempPointer[i], start);
        }

        //添加右边头部
        tempPointer.Clear();
        tempPointer.Add(endUP);
        for (float i = -angel; i > -180; i -= angel)
        {
            tempPointer.Add(RotateVector2(-i, endUP, end));
        }

        tempPointer.Add(endDown);
        for (int i = 1; i < tempPointer.Count; i++)
        {
            AddVert(this.toFill, tempPointer[i - 1], tempPointer[i], end);
        }

        tempPointer.Clear();


        SetVerticesDirty();
    }

    void AddVert(VertexHelper vh, Vector2 pos1, Vector2 pos2, Vector2 pos3)
    {
        AddVert(vh, CreateEmptyVertex(pos1), CreateEmptyVertex(pos2), CreateEmptyVertex(pos3));
    }

    void AddVert(VertexHelper vh, UIVertex v1, UIVertex v2, UIVertex v3)
    {
        int index = vh.currentVertCount;
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);
        vh.AddTriangle(index, index + 1, index + 2);
    }

    public void DrawLine(Vector2 point1, Vector2 point2)
    {
        if (point1.Equals(point2))
            return;
        Vector2 LeftUp1 = point1 + new Vector2(-offset.x, offset.y);
        Vector2 LeftDown1 = point1 + new Vector2(-offset.x, -offset.y);
        Vector2 RightUp1 = point1 + new Vector2(offset.x, offset.y);
        Vector2 RightDown1 = point1 + new Vector2(offset.x, -offset.y);

        Vector2 LeftUp2 = point2 + new Vector2(-offset.x, offset.y);
        Vector2 LeftDown2 = point2 + new Vector2(-offset.x, -offset.y);
        Vector2 RightUp2 = point2 + new Vector2(offset.x, offset.y);
        Vector2 RightDown2 = point2 + new Vector2(offset.x, -offset.y);


        if (point2.y > point1.y)
        {
            pointer.Add(LeftUp2);
            pointer.Add(RightUp2);
            pointer.Add(RightDown1);
            pointer.Add(LeftDown1);
        }
        else
        {
            if (point2.y < point1.y)
            {
                pointer.Add(LeftUp1);
                pointer.Add(RightUp1);
                pointer.Add(RightDown2);
                pointer.Add(LeftDown2);
            }
            else
            {
                if (point2.x > point1.x)
                {
                    pointer.Add(LeftUp1);
                    pointer.Add(RightUp2);
                    pointer.Add(RightDown2);
                    pointer.Add(LeftDown1);
                }
                else
                {
                    pointer.Add(LeftUp2);
                    pointer.Add(RightUp1);
                    pointer.Add(RightDown1);
                    pointer.Add(LeftDown2);
                }
            }
        }
        SetVerticesDirty();
    }

    //逆时针
    private float GetAngle(Vector2 org, Vector2 target, bool isReturnAngle = true)
    {
        float fx = target.x - org.x;
        float fy = target.y - org.y;
        if (fx == 0)
        {
            return 0;
        }
        float value = fy / fx;
        float angle = Mathf.Atan(value);
        if (isReturnAngle)
            angle = angle * Mathf.Rad2Deg;
        return angle;
    }

    public Vector2 RotateVector2(float angle, Vector2 target, Vector2 org)
    {
        float tx, ty; tx = target.x - org.x; ty = target.y - org.y;
        float thata = Mathf.Deg2Rad * angle;//弧度
        //逆时针旋转矩阵
        float x = tx * Mathf.Cos(thata) - ty * Mathf.Sin(thata) + org.x;
        float y = tx * Mathf.Sin(thata) + ty * Mathf.Cos(thata) + org.y;
        return new Vector2(x, y);
    }

    public Vector2 RotatePoint(Vector2 point, float angle)
    {
        float tx = point.x;
        float ty = point.y;
        float thata = Mathf.Deg2Rad * angle;//弧度
        //逆时针旋转矩阵
        float x = tx * Mathf.Cos(thata)- ty*Mathf.Sin(thata);
        float y = tx * Mathf.Sin(thata) + ty * Mathf.Cos(thata);
        return new Vector2(x, y);
    }

    public void AddRectPointByAngle(Vector2 center, Vector2 offset, float angle)
    {
        
        Vector2 LeftUp = center + new Vector2(-offset.x, offset.y);
        Vector2 LeftDown = center + new Vector2(-offset.x, -offset.y);
        Vector2 RightUp = center + new Vector2(offset.x, offset.y);
        Vector2 RightDown = center + new Vector2(offset.x, -offset.y);
        LeftUp = RotatePoint(LeftUp, angle);
        LeftDown = RotatePoint(LeftDown, angle);
        RightUp = RotatePoint(RightUp, angle);
        RightDown = RotatePoint(RightDown, angle);

        Vector2 center1 = RotatePoint(center, angle);
        Vector2 dir = center - center1;
        LeftUp += dir;
        LeftDown += dir;
        RightUp += dir;
        RightDown += dir;

        pointer.Add(LeftUp);
        pointer.Add(RightUp);
        pointer.Add(RightDown);
        pointer.Add(LeftDown);
        SetVerticesDirty();
    }


    public void AddRectPoint(Vector2 center, Vector2 offset)
    {
        Vector2 LeftUp = center + new Vector2(-offset.x, offset.y);
        Vector2 LeftDown = center + new Vector2(-offset.x, -offset.y);
        Vector2 RightUp = center + new Vector2(offset.x, offset.y);
        Vector2 RightDown = center + new Vector2(offset.x, -offset.y);

        pointer.Add(LeftUp);
        pointer.Add(RightUp);
        pointer.Add(RightDown);
        pointer.Add(LeftDown);

        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (this.toFill == null)
            this.toFill = toFill;
        toFill.Clear();
        int count = pointer.Count;
        if (count < 4)
        {
            return;
        }
        for (int i = 0; i < pointer.Count; i +=4)
        {
            AddQuad(toFill, pointer[i], pointer[i + 1], pointer[i + 2], pointer[i + 3]);
        }
    }

    void AddQuad(VertexHelper vh, Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4)
    {
        AddQuad(vh, CreateEmptyVertex(pos1), CreateEmptyVertex(pos2), CreateEmptyVertex(pos3), CreateEmptyVertex(pos4));
    }

    UIVertex CreateEmptyVertex(Vector2 pos)
    {
        UIVertex v = new UIVertex();
        v.position = pos;
        //需要修改颜色，在这里改就可以了
        v.color = color;
        v.uv0 = Vector2.zero;
        return v;
    }

    void AddQuad(VertexHelper vh, UIVertex v1, UIVertex v2, UIVertex v3, UIVertex v4)
    {
        int index = vh.currentVertCount;
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);
        vh.AddVert(v4);
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index);
    }


    protected override void OnDestroy()
    {
        pointer.Clear();
    }

}
