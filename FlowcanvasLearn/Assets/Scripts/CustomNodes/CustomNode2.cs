using FlowCanvas.Nodes;
using System.Collections;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;




//第一个泛型参数表示返回值的类型,后面几个是参数，而您要重写的Invoke方法将需要返回该类型声明。
[Category("CustomNode")]
public class CustomNode2 : CallableFunctionNode<string, string, string>
{
    // Start is called before the first frame update
    public override string Invoke(string a, string b)
    {
        return a + b;
    }
}
