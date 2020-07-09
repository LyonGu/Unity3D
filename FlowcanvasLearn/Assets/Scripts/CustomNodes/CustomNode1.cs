using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Color("ff0000")]
//[Name("CustomNode1")]
[Category("CustomNode")]


// CallableActionNode 它们有1个流量输入，1个流量输出，最多5个值输入参数，根本没有值输出
public class CustomNode1 : CallableActionNode<object>
{
    public override void Invoke(object a) //装箱具有
    {
        Debug.Log($"CustomNode1========{a}");
    }

    // Start is called before the first frame update
   
}
