using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    // Start is called before the first frame update

    public SpriteRenderer renderer;
    public NavMeshAgent _agent;

    private void Start()
    {
        SetSelected(false);
    }


    // Update is called once per frame
    public void SetSelected(bool isSelect)
    {
        Color oldColor = renderer.color;
        oldColor.a = isSelect ? 1.0f : 0.1f;
        renderer.color = oldColor;
    }

    private void OnDestroy()
    {
        SetSelected(false);
    }

    public void SetDestination(Vector3 worldPos)
    {
        _agent.isStopped = false;
        _agent.SetDestination(worldPos);
    }
}
