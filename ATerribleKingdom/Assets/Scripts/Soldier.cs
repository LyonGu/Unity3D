using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : BaseUnits
{
    // Start is called before the first frame update

    public SpriteRenderer renderer;
    public NavMeshAgent _agent;
    private Transform _moveTarget;
    private Transform _attackTarget;

    private void Start()
    {
        SetSelected(false);
    }


    private void Update()
    {
        if (_moveTarget != null)
        {
            if (_agent.remainingDistance < 0.3f)
            {
                _attackTarget = _moveTarget;
                _moveTarget = null;
                //距离目标点小于0.3f
                _agent.isStopped = true;
                StartAttack();
            }
           
        }
    }

    private void StartAttack()
    {
        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        var unit = _attackTarget.GetComponent<BaseUnits>();
        while (_attackTarget != null)
        {
            unit.DoDamage(this.Attack);
            yield return new WaitForSeconds(this.AttackInterval);
        }
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
        _attackTarget = null;
        _moveTarget = null;
        _agent.isStopped = false;
        _agent.SetDestination(worldPos);
    }

    public void MoveToTarget(Transform target, Vector3 worldPos)
    {
        _moveTarget = target;
        _agent.isStopped = false;
        _agent.SetDestination(worldPos);
    }
}
