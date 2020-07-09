using FlowCanvas.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;

/*
 使用以下方法注册端口：

FlowInput : AddFlowInput(string name, Action pointer)
name: 端口的名称。
pointer:调用此流输入时将调用的委托。

FlowOutput: AddFlowOutput(string name)
name: 端口的名称。
要执行端口，您必须在返回的FlowOutput对象上调用Call（），以及接收到的当前Flow对象。

ValueInput<T> : AddValueInput<T>(string name)
name: 端口的名称。
要获取连接到端口的值，只需调用返回的对象的.value属性。

ValueOutput<T> : AddValueOutput<T>(string name, Func<T> getter)
name: 端口的名称。
getter:将被调用以获取类型T的值的委托。
 */

[Color("ff0000")]
[Name("Switch Condition")]
[Category("Custom/Switchers")]
public class CustomNode4 : FlowControlNode
{
    protected override void RegisterPorts()
    {
        var condition = AddValueInput<bool>("Condition");
        var trueOut = AddFlowOutput("True");
        var falseOut = AddFlowOutput("False");

        AddFlowInput("In", (f) =>{

            if (condition.value)
            {
                trueOut.Call(f);
            }
            else
            {
                falseOut.Call(f);
            }
        });
    }
}
