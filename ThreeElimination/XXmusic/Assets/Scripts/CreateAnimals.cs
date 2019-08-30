using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.EventSystems.EventTrigger))]
public class CreateAnimals : MonoBehaviour
{
    public static CreateAnimals Instance;

    public struct animals
    {
        public GameObject pointobject;    //盒子
        public int x;
        public int y;
    }

    public animals[,] m_animals;//动物的数组

    private int m_x = 6;//定义列
    private int m_y = 6;//定义行

    // Use this for initialization
    void Start ()
    {
        Instance = this;
        m_animals = new animals[m_x, m_y];  //创建数组
    }
	
	// Update is called once per frame
	void Update ()
    {
     
    }

    public void InitiLayer()
    {
        //初始化创建盒子
        for (int j = 0; j < m_y; j++)
        {
            for (int i = 0; i < m_x; i++)
            {
                animals ani = m_animals[i, j];

                //盒子
                ani.pointobject = Resources.Load("Prefabs/check") as GameObject;  //没有指明父节点 就默认父节点为场景跟节点
                GameObject check = Instantiate(ani.pointobject);

                check.transform.localPosition = new Vector3(i * 210 - 521, j * 205 * (-1) + 500, 0);

                ani.x = i;
                ani.y = j;
            }
        }
    }

    public GameObject CreateAnimal()            //创建动物
    {
        int num = Random.Range(1, 7);
        GameObject animal = Resources.Load("Prefabs/" + num) as GameObject;
        animal = Instantiate(animal);
        return animal;
    }

    public GameObject CreateYellowcheck()            //创建黄框
    {
        GameObject yellowcheck = Resources.Load("Prefabs/yellowcheck") as GameObject;
        yellowcheck = Instantiate(yellowcheck);
        return yellowcheck;
    }
}