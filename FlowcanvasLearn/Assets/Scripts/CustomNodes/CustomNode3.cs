using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///第一个泛型参数是节点的返回类型和您需要覆盖的Invoke方法，而其余的泛型参数是要使用的函数的值输入参数
[Category("CustomNode")]
public class CustomNode3 : PureFunctionNode<bool, float, float>
{
    // Start is called before the first frame update
    public override bool Invoke(float a, float b)
    {
        if (a > b)
        {
            return true;
        }
        return false;
    }
}
