using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMgr : MonoBehaviour
{
    // Update is called once per frame
    private bool isDraging = false;
    private Vector2 startPos;
    private Vector2 endPos;

    private Rect _rect;
    private Camera _camera;
    private List<Soldier> selectList = new List<Soldier>();

    private void Start()
    {
        _camera = Camera.main;
    }
    void Update()
    {
        ProcessInpout();
    }

    private void ProcessInpout()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDraging = true;
            startPos = Input.mousePosition;
        }

        if (isDraging)
        {
            endPos = Input.mousePosition;

            Vector2 center = (startPos + endPos) / 2;
            Vector2 size = new Vector2(Mathf.Abs(endPos.x - startPos.x), Mathf.Abs(endPos.y - startPos.y));
            UIMgr.Instance.SetRectTrangle(center, size);
            _rect = new Rect(center-size/2, size);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraging = false;
            UIMgr.Instance.ShowRectTrangle(false);

            ClearSelectedList();

            Transform[] allUnits= GameMgr.Instance.GetAllSoldierTransform();
            
            foreach (Transform item in allUnits)
            {
                var screenPos = _camera.WorldToScreenPoint(item.position);
                if (_rect.Contains(screenPos))
                {
                    Debug.Log($"框选了士兵===={item.name}");

                    var unit = item.gameObject.GetComponent<Soldier>();
                    AddToSelectedList(unit);
                }
            }
        }
    }

    public void AddToSelectedList(Soldier unit)
    {
        unit.SetSelected(true);
        selectList.Add(unit);
    }

    public void ClearSelectedList()
    {
        foreach (Soldier item in selectList)
        {
            item.SetSelected(false);
        }
        selectList.Clear();
    }
}
