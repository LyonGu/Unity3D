
using UnityEngine;

/// <summary>
/// 鸟
/// </summary>
public class Bird : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CircleCollider2D col;
    [HideInInspector] public Vector3 pos
    {
        get { return transform.position; }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

    }

    /// <summary>
    /// 给鸟一个推力，将鸟推出去
    /// </summary>
    /// <param name="speed">速度向量</param>
    public void Push(Vector2 speed)
    {
        rb.AddForce(speed, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 激活物理
    /// </summary>
    public void ActivateRb()
    {
        rb.isKinematic = false;
    }

    /// <summary>
    /// 禁用物理
    /// </summary>
    public void DesActivateRb()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
    }
}
