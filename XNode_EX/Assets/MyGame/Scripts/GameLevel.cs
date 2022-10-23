using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityStandardAssets.Utility;
public class GameLevel :GameInst<GameLevel>
{
    public GameStateGraph gameStateGraph;
    public WaypointCircuit curvySpline;
    
    void Start()
    {
        gameStateGraph.startNod.GameStart(); //手动调用的
    }
    public void StartCoroutineCreateEnermys(int count, WaypointCircuit spline,CreateEnermyNode enermyNode)
    {
        StartCoroutine(CreateEnermys(count, spline, enermyNode));
    }
    void Update()
    {
        
    }
    public IEnumerator CreateEnermys(int count, WaypointCircuit spline, CreateEnermyNode enermyNode)
    {
            for (int i = 0; i < count; i++)
            {
            yield return new WaitForSeconds(1f);
            CreateEnermy( spline, enermyNode, Vector3.zero,Vector3.zero);
        }   
    }
    Dictionary<string, WaypointCircuit> splines=new Dictionary<string, WaypointCircuit>();
    public Transform CreateEnermy(WaypointCircuit spline, CreateEnermyNode enermyNode,Vector3 pos,Vector3 angle)
    {
        Transform enermy = CreateEnermy(enermyNode);
        Enermy splineController = enermy.GetComponent<Enermy>();

        WaypointCircuit curSpline = null;
        string splineName = "";
        splineName = spline.name;
        if (splines.ContainsKey(splineName))
        {
            curSpline = splines[splineName];
            
        } else
        {
            curSpline = Instantiate(spline) as WaypointCircuit;
            splines.Add(splineName, curSpline);
        }
        
        splineController.spline = curSpline;
        if (pos == Vector3.zero)
        {
            splineController.offset = Vector3.zero;
        }
        else
        {
            Vector3 pp = curSpline.GetRoutePoint(0).position;
            Debug.Log(pp);
            splineController.offset = pos - pp;
        }
        enermy.position = pos;
        enermy.eulerAngles = angle;
        splineController.Init();
        return enermy;
    }
    Transform CreateEnermy(CreateEnermyNode enermyNode)
    {
        Transform enermy = Instantiate(Resources.Load<GameObject>( enermyNode._mPlane.plane.name)).transform;
        enermy.GetComponent<Enermy>().enermyNode = enermyNode;
        enermy.GetComponent<Enermy>().blood = enermy.GetComponent<Enermy>().maxBlood;
        return enermy;
    }
    public IEnumerator DelayCreateEnermys(CreateEnermyNode enermyNode)
    {
        for (int i = 0; i < enermyNode.positionNode.line; i++)
        {
            
            for (int j = (enermyNode.positionNode.line - 1 - i) * enermyNode.positionNode.ishs.Count / enermyNode.positionNode.line; j < (enermyNode.positionNode.line - i) * enermyNode.positionNode.ishs.Count / enermyNode.positionNode.line; j++)
            {
                if (enermyNode.positionNode.ishs[j])
                {
                    int posIndex = j % (enermyNode.positionNode.ishs.Count / enermyNode.positionNode.line);
                    int pos=(enermyNode.positionNode.line - 1 ) * enermyNode.positionNode.ishs.Count / enermyNode.positionNode.line+ posIndex;
                    Transform enermy = CreateEnermy(enermyNode.spline, enermyNode, enermyNode.positionNode.posDic[pos], new Vector3(0, 0, 90));
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void StartCoroutineDelayCreateEnermys(CreateEnermyNode enermyNode)
    {
        StartCoroutine(DelayCreateEnermys(enermyNode));
    }
    public void EnterDelayNode(float delayTime,DelayNode delayNode)
    {
        vp_Timer.Handle Timer = new vp_Timer.Handle();
        vp_Timer.In(delayTime, new vp_Timer.Callback(() => { Timer.Cancel(); delayNode.MoveNext(); }), Timer);
    }
}
