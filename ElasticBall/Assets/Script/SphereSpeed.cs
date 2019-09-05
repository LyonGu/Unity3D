using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereSpeed : MonoBehaviour
{
    private Rigidbody rigid;
    [SerializeField]
    private  Transform _rightSuperBounce;
    [SerializeField]
    private Transform _leftSuperBounce;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider collision)
    {
   
        if (collision.transform.name == "Die")
        {
            StartCoroutine(Relive());
        }
        if (collision.transform.name == "LeftSuperBounce")
        {

            /*
                Force
                Add a continuous force to the rigidbody, using its mass.
                添加一个可持续力到刚体，使用它的质量。
           
             
                Acceleration
                Add a continuous acceleration to the rigidbody, ignoring its mass.
                添加一个可持续加速度到刚体，忽略它的质量。
             
                Impulse
                Add an instant force impulse to the rigidbody, using its mass.
                添加一个瞬间冲击力到刚体，使用它的质量。
             
                VelocityChange
                Add an instant velocity change to the rigidbody, ignoring its mass.
                添加一个瞬间速率变化给刚体，忽略它的质量
             * 
             * 
             * 功能：力的作用方式。枚举类型,有四个枚举成员

                计算公式：    Ft = mv(t) 即 v(t) = Ft/m

                (1)ForceMode.Force : 持续施加一个力，与重力mass有关，t = 每帧间隔时间，m = mass

                (2)ForceMode.Impulse : 瞬间施加一个力，与重力mass有关，t = 1.0f，m = mass

                (3)ForceMode.Acceleration：持续施加一个力，与重力mass无关，t = 每帧间隔时间，m = 1.0f

                (4)ForceMode.VelocityChange：瞬间施加一个力，与重力mass无关，t = 1.0f，m = 1.0f

             * 
             * 
             * 
             AddRelativeForce:
                Adds a force to the rigidbody relative to its coordinate system.
                Force can be applied only to an active rigidbody. If a GameObject is inactive, AddRelativeForce has no effect.
                Wakes up the Rigidbody by default. If the force size is zero then the Rigidbody will not be woken up.
             
             */


            rigid.AddRelativeForce(-_leftSuperBounce.right * 10, ForceMode.Impulse);
        }
        if (collision.transform.name == "RightSuperBounce")
        {
            rigid.AddRelativeForce(_rightSuperBounce.right * 10, ForceMode.Impulse);
        }
        if (collision.transform.name == "Cylinder")
        {
            rigid.AddRelativeForce(new Vector3(transform.position.x - collision.transform.position.x, transform.position.y - collision.transform.position.y, 0) * 20, ForceMode.Impulse);
        }
    }

    IEnumerator Relive()
    {
        yield return new WaitForSeconds(2);
        transform.position = new Vector3(-9.3f, 0, 0);
    }
}
