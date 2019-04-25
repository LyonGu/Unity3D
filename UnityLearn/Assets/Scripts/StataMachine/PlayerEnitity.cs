using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerEnitity:BaseEnitity  {


    public PlayerMode _mode;
    private GameObject _rootObj;
    public GameObject _gameObject;

    public PlayerEnitity()
    {
        _mode = new PlayerMode();
        _mode._file = "Models/SwordsMan/GreateWarrior";

        

    }
    override public void initGameObject()
    {
        _rootObj = GameObject.Find("StartGame");
        if (_rootObj)
        {
            _gameObject = getGameObject(_mode._file, "GreateWarrior", _rootObj, Vector3.zero);
            _gameObject.transform.localScale = new Vector3(20.0f, 20.0f, 20.0f);
            _gameObject.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0);
        }
    }



}
