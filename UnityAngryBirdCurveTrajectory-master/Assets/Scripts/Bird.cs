
using UnityEngine;

/// <summary>
/// ��
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
    /// ����һ�������������Ƴ�ȥ
    /// </summary>
    /// <param name="speed">�ٶ�����</param>
    public void Push(Vector2 speed)
    {
        rb.AddForce(speed, ForceMode2D.Impulse);
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void ActivateRb()
    {
        rb.isKinematic = false;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void DesActivateRb()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
    }
}
