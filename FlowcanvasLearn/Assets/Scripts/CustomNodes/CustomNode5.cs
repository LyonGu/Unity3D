using FlowCanvas;
using FlowCanvas.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEventNodeExample : EventNode
{

    private FlowOutput raised;

    public Action MyEvent { get; private set; }

    public override void OnGraphStarted()
    {
        //Subscribe to the event here. For example:
       MyEvent += EventRaised;
    }


    public override void OnGraphStoped()
    {
        //Unsubscribe here. For example:
        MyEvent -= EventRaised;
    }

    protected override void RegisterPorts()
    {
        raised = AddFlowOutput("Out");
    }

    void EventRaised()
    {
        raised.Call(new Flow());
    }
}
