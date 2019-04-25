using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public static class GlobalParams{

    public static int gameObjId = 0;
};



/*
    实例基类

*/
public class BaseEnitity  {

    public int _id;         //唯一标识id
    public string file;     //模型路径文件或者预设路径文件
    public BaseEnitity()
    {
        _id = GlobalParams.gameObjId;
        GlobalParams.gameObjId++;
    }
}
