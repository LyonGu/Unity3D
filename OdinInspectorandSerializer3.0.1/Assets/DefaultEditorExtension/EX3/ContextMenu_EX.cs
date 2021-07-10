using UnityEngine;
using UnityEditor;
public class ContextMenu_EX : MonoBehaviour
{
    [ContextMenu("FunctionName")]
    public void FunctionName()
    {
        Debug.Log("FunctionName");
    }


    [ContextMenuItem("Handle", "HandleHealth")] //右键点击出现handle，点击handle调用HandleHealth方法
    public float health;

    private void HandleHealth()
    {
        Debug.Log("HandleHealth");
    }
}
