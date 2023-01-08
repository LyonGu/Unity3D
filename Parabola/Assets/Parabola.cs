using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parabola : MonoBehaviour
{
    public GameObject prefabRes;

    private GameObject _obj;

    private float _posx = 0.0f;

    private float _posy= 0.0f;

    private float _speedX= 10.0f;

    private float _speedY = 15.0f;

    private float _acceleration = -10.0f;

    //初速度，根据于水平夹角算出水平速度和竖直速度
    public float _speed;

    //与水平夹角
    public float _angle;
    // Start is called before the first frame update
    void Start()
    {
        // _obj = Instantiate(prefabRes);
        // _obj.transform.position = new Vector3(_posx,_posy, 0);
        _posList.Add(new Vector3(_posx,_posy, 0));
        InitData();
    }
        
    private void InitData()
    {
        _speedX = _speed * Mathf.Cos(Mathf.Deg2Rad*_angle);
        _speedY = _speed * Mathf.Sin(Mathf.Deg2Rad * _angle);
    }

    private float totalTime = 0;
    private List<Vector3> _posList = new List<Vector3>(256);
    private bool _isEnd;
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            _isEnd = true;
            CreateTrack();
        }
        
    }

    private void FixedUpdate()
    {
        if(_isEnd)
            return;
        totalTime += Time.deltaTime; //换成Time.fixedTime后运动轨迹会变疏
        _posx = _speedX * totalTime;
        _posy = 0.5f * _acceleration * totalTime * totalTime + _speedY * totalTime;

        // if(_posList.Count > 3)
        //     return;
        // Debug.Log($"{_posx}  {_posy}  {totalTime} {Time.deltaTime} {Time.frameCount}");
        _posList.Add(new Vector3(_posx,_posy, 0));
    }

    private void CreateTrack()
    {
        if (_posList.Count > 0)
        {
            foreach (var pos in _posList)
            {
                GameObject obj = Instantiate(prefabRes);
                obj.transform.position = pos;
            }
            _posList.Clear();
        }
    }
}
