using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check : MonoBehaviour
{
    public Check instance;
    CreateAnimals animal = new CreateAnimals();
    CreateAnimals yellowcheck = new CreateAnimals();

    // Use this for initialization
    void Start()
    {
        instance = this;
    }

    public void CreateSon()          //判断盒子是否有孩子
    {
        if(transform.childCount == 0)
        {
            GameObject sonAnimal= animal.CreateAnimal();  //调用创建动物方法
            sonAnimal.transform.parent = transform;
            sonAnimal.transform.position = transform.position;

            GameObject sonYellowcheck = yellowcheck.CreateYellowcheck();     //调用创建黄框方法
            sonYellowcheck.transform.parent = sonAnimal.transform;
            sonYellowcheck.transform.position = sonAnimal.transform.position;

            var allAnimals = FindObjectsOfType<XXanimals>();   //找到所有挂载了XXanimals脚本的动物身上的XXanimals脚本
            foreach (var item in allAnimals)
            {
                item.horizontal = new List<RaycastHit2D>();   //更新水平数组的动物
                item.vertical = new List<RaycastHit2D>();     //更新垂直数组的动物
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
        CreateSon();
    }
}
