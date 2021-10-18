using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineDrawer : Image
{
    
    public float radio = 1.0f;
    public Vector2 offset = new Vector2(5, 5);
    public float lineWidth = 1.0f;

    private int headPointCount = 3; //头部顶点数量
    private List<Vector2> tempPointer;
    private List<PointData> rectPointer;
    private VertexHelper toFill;
    public struct PointData {
        public int index;
        public Vector2 center;
        public bool isDrawLineDone;
        public bool isDelete;
    }

    public List<PointData> pointer = new List<PointData>();
    protected override void Awake()
    {
        base.Awake();
        tempPointer = new List<Vector2>();
        rectPointer = new List<PointData>();
    }

    private int curStartIndex;
    private int curEndIndex;
    public void DrawLine(int startIndex, int endIndex)
    {
        if (pointer.Count >= 2)
        {
            curStartIndex = startIndex;
            curEndIndex = endIndex;
            ReDraw();

            //PointData pointData1 = pointer[startIndex];
            //PointData pointData2 = pointer[endIndex];
            //RemoveRectPoint(pointData2.index);
            //RemoveRectPoint(pointData1.index);
            //Draw(toFill, pointData1.center, pointData2.center);
            //pointData1.isDrawLineDone = true;

        }
           
    }

    public int GetFirstLinePoint()
    {
        if (pointer.Count >= 2)
        {
            for (int i = 0; i < pointer.Count; i++)
            {
                PointData pointData = pointer[i];
                if (!pointData.isDrawLineDone)
                    return i;
            }
        }
        return -1;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (this.toFill == null)
            this.toFill = toFill;
        toFill.Clear();

      

        for (int i = 1; i <= curEndIndex; i++)
        {
            //移除两个端点
            PointData pointData1 = pointer[i - 1];
            PointData pointData2 = pointer[i];
            RemoveRectPoint(pointData2.index);
            RemoveRectPoint(pointData1.index);
            pointData1.isDrawLineDone = true;
            pointer[i-1] = pointData1;
            Draw(toFill, pointData1.center, pointData2.center);
        }
    

    //if (pointer.Count >= 2 && curStartIndex < curEndIndex)
    //{

    //    for (int i = curStartIndex; i < curEndIndex; i++)
    //    {
    //        //移除两个端点
    //        PointData pointData1 = pointer[i];
    //        PointData pointData2 = pointer[i + 1];
    //        RemoveRectPoint(pointData2.index);
    //        RemoveRectPoint(pointData1.index);
    //        pointData1.isDrawLineDone = true;
    //        pointer[i] = pointData1;

    //        Draw(toFill, pointData1.center, pointData2.center);
    //    }
    //}

        //画点
        int count = rectPointer.Count;
        if (count < 4)
        {
            return;
        }
        for (int i = 0; i < rectPointer.Count; i += 4)
        {
            if(!rectPointer[i].isDelete)
                AddPointQuad(toFill, rectPointer[i].center, rectPointer[i + 1].center, rectPointer[i + 2].center, rectPointer[i + 3].center);
        }


    }
    public void RemoveRectPoint(int index)
    {
        if (index < rectPointer.Count - 3)
        {
            for (int i = index; i <= index+3; i++)
            {
                PointData data = rectPointer[i];
                data.isDelete = true;
                rectPointer[i] = data;
            }
        }
                //rectPointer.RemoveRange(index, 4);
    }


    public void AddRectPoint(Vector2 center, Vector2 offset)
    {
        Vector2 LeftUp = center + new Vector2(-offset.x, offset.y);
        Vector2 LeftDown = center + new Vector2(-offset.x, -offset.y);
        Vector2 RightUp = center + new Vector2(offset.x, offset.y);
        Vector2 RightDown = center + new Vector2(offset.x, -offset.y);

        int index = rectPointer.Count;
        PointData pointData = new PointData
        {
            center = LeftUp,
            index = index,
            isDrawLineDone = false,
            isDelete = false
        };

        rectPointer.Add(pointData);

        pointData.center = RightUp;
        pointData.index = index + 1;
        rectPointer.Add(pointData);

        pointData.center = RightDown;
        pointData.index = index + 2;
        rectPointer.Add(pointData);

        pointData.center = LeftDown;
        pointData.index = index + 3;

        rectPointer.Add(pointData);

        pointData = new PointData
        {
            center = center,
            index = index,
            isDrawLineDone = false,
            isDelete = false
        };
        this.pointer.Add(pointData);
        ReDraw();
    }




    public void AddPointer(Vector2 pointer)
    {
        //PointData pointData = new PointData
        //{
        //    center = pointer,
        //    index = 0
        //};
        //this.pointer.Add(pointData);
        //ReDraw();
    }

    public void ReDraw()
    {
        SetVerticesDirty();
    }

 

    void Draw(VertexHelper vh, Vector2 start, Vector2 end)
    {
        Vector2 to = end - start;
        Vector2 nor_to = to.normalized * radio + start; //当ratio为1时，nor_to就是 end
      
        Vector2 up = RotateVector2(90, nor_to, start); // start 到 end的向量 逆时针旋转90度
        Vector2 down = RotateVector2(-90, nor_to, start);//逆时针旋转-90度

        var dir_starUp = up - start;
        var dir_starDown = down - start;
        up = dir_starUp.normalized * lineWidth + start;
        down = dir_starDown.normalized * lineWidth + start;

        Vector2 up_end = up + to;
        Vector2 down_end = down + to;


        //添加直线
        AddQuad(vh, up, down, up_end, down_end);

        //添加左边头部
        float angel = 180 / (headPointCount + 1);
        tempPointer.Add(down);
        for (float i = -angel; i > -180; i -= angel)
        {
            tempPointer.Add(RotateVector2(i, down, start));
        }

        tempPointer.Add(up);

        for (int i = 1; i < tempPointer.Count; i++)
        {
            AddVert(vh, tempPointer[i - 1], tempPointer[i], start);
        }

        //添加右边头部
        tempPointer.Clear();
        tempPointer.Add(up_end);
        for (float i = -angel; i > -180; i -= angel)
        {
            tempPointer.Add(RotateVector2(-i, up_end, end));
        }

        tempPointer.Add(down_end);
        for (int i = 1; i < tempPointer.Count; i++)
        {
            AddVert(vh, tempPointer[i - 1], tempPointer[i], end);
        }

        tempPointer.Clear();
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

    void AddQuad(VertexHelper vh, Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4)
    {
        AddQuad(vh, CreateEmptyVertex(pos1), CreateEmptyVertex(pos2), CreateEmptyVertex(pos3), CreateEmptyVertex(pos4));
    }

    void AddPointQuad(VertexHelper vh, Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4)
    {
        AddPointQuad(vh, CreateEmptyVertex(pos1), CreateEmptyVertex(pos2), CreateEmptyVertex(pos3), CreateEmptyVertex(pos4));
    }

    void AddQuad(VertexHelper vh, UIVertex v1, UIVertex v2, UIVertex v3, UIVertex v4)
    {
        int index = vh.currentVertCount;
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);
        vh.AddVert(v4);
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index + 1);

        
    }

    void AddPointQuad(VertexHelper vh, UIVertex v1, UIVertex v2, UIVertex v3, UIVertex v4)
    {
        int index = vh.currentVertCount;
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);
        vh.AddVert(v4);
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index);
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

    public Vector2 RotateVector2(float angle, Vector2 target, Vector2 org)
    {
        float tx, ty; tx = target.x - org.x; ty = target.y - org.y;
        float thata = Mathf.Deg2Rad * angle;//弧度
        //逆时针旋转矩阵
        float x = tx * Mathf.Cos(thata) - ty * Mathf.Sin(thata) + org.x;
        float y = tx * Mathf.Sin(thata) + ty * Mathf.Cos(thata) + org.y;
        return new Vector2(x, y);
    }
}
