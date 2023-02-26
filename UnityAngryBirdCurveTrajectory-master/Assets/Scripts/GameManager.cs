
using UnityEngine;


/// <summary>
/// ��Ϸ������
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// ��
    /// </summary>
    public Bird bird;
    /// <summary>
    /// �켣Ԥ����
    /// </summary>
    public Trajectory trajectory;

    /// <summary>
    /// �������
    /// </summary>
    private Camera m_cam;

    /// <summary>
    /// ����С
    /// </summary>
    [SerializeField]
    private float m_speedFactor = 4f;

    /// <summary>
    /// �Ƿ�������
    /// </summary>
    private bool m_isDragging = false;
    /// <summary>
    /// ��ָ����ʼ��
    /// </summary>
    private Vector2 m_startPoint;
    /// <summary>
    /// ��ָ�Ľ�����
    /// </summary>
    private Vector2 m_endPoint;
    /// <summary>
    /// ��ʼ��ͽ�����ľ���
    /// </summary>
    private float m_distance;
    /// <summary>
    /// �����������ӽ�����ָ����ʼ��Ĺ�һ������
    /// </summary>
    private Vector2 m_direction;
    /// <summary>
    /// ������
    /// </summary>
    private Vector2 m_pushSpeed;


    private void Start()
    {
        m_cam = Camera.main;
        bird.DesActivateRb();
    }

    private void Update()
    {
        // ������/��ָ��Ϊ
        if(Input.GetMouseButtonDown(0))
        {
            m_isDragging = true;
            OnDragStart();
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_isDragging = false;
            OnDragEnd();
        }

        if (m_isDragging)
        {
            OnDrag();
        }
    }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    private void OnDragStart()
    {
        // ��������
        bird.DesActivateRb();
        // ��ʼ��
        m_startPoint = m_cam.ScreenToWorldPoint(Input.mousePosition);
        // ��ʾ�켣
        trajectory.Show();
    }

    /// <summary>
    /// ����
    /// </summary>
    private void OnDrag()
    {
        m_endPoint = m_cam.ScreenToWorldPoint(Input.mousePosition);
        m_distance = Vector2.Distance(m_startPoint, m_endPoint);
        m_direction = (m_startPoint - m_endPoint).normalized;
        m_pushSpeed = m_direction * m_distance * m_speedFactor;

        trajectory.UpdateDots(bird.pos, m_pushSpeed);
    }

    /// <summary>
    /// ������
    /// </summary>
    private void OnDragEnd()
    {
        bird.ActivateRb();
        bird.Push(m_pushSpeed);
        // ���ع켣
        trajectory.Hide();
    }
}
