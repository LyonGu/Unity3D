using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.EventSystems.EventTrigger))]

public class EventTriggerHandler : MonoBehaviour
{
    public static EventTriggerHandler instance;
    public GameObject exchange;

    // Use this for initialization
    void Start ()
    {
        instance = this;
        exchange = GameObject.Find("exchange");     //动态找到panel上的exchange物体

        //Button btn = this.GetComponent<Button>();
        //Sprite spr = this.GetComponent<Sprite>();       //找到脚本
        EventTrigger trigger = GetComponent<EventTrigger>();   //获取自身的EventTrigger组件
        EventTrigger.Entry entry1 = new EventTrigger.Entry();

        //鼠标点击事件
        entry1.eventID = EventTriggerType.PointerClick;
        //鼠标进入事件 
        //entry2.eventID = EventTriggerType.PointerEnter;
        //鼠标滑出事件 
        //entry3.eventID = EventTriggerType.PointerExit;

        entry1.callback = new EventTrigger.TriggerEvent();   //委托添加要执行的方法(回调函数)
        entry1.callback.AddListener(OnClick);      //执行
        trigger.triggers.Add(entry1);
    }

    private void OnClick(BaseEventData pointData)
    {
        Transform son = this.gameObject.transform.GetChild(0);      //找到当前精灵的第一个孩子，yellow选择框
        int j = exchange.GetComponent<ChangeAnimals>().i; //每次点击完成i会自增，超过2就回到0  exchange类似于一个全局的管理类

        if (son.GetComponent<SpriteRenderer>().sortingLayerName != "Frog")  
        { 
            son.GetComponent<SpriteRenderer>().sortingLayerName = "Frog";  //显示黄色边框  //直接改变层级不是也行吗？？？？
            if (j < 2)
            {
                exchange.GetComponent<ChangeAnimals>().exchange[j] = this.gameObject.transform;   //把当前点击的精灵放进数组
            }

            if (exchange.GetComponent<ChangeAnimals>().exchange[1] != null)  
            {
                float animal_x = exchange.GetComponent<ChangeAnimals>().exchange[0].position.x;
                float animal_y = exchange.GetComponent<ChangeAnimals>().exchange[0].position.y;
                float animal1_x = exchange.GetComponent<ChangeAnimals>().exchange[1].position.x;
                float animal1_y = exchange.GetComponent<ChangeAnimals>().exchange[1].position.y;
                
                if (animal_x < 0 || animal1_x < 0 || animal_y < 0 || animal1_y < 0)
                {
                    animal_x = -animal_x;
                    animal1_x = -animal1_x;
                    animal_y = -animal_y;
                    animal1_y = -animal1_y;
                }

                float x = animal1_x - animal_x;
                float y = animal1_y - animal_y;

                if (x < 0 || y < 0)
                {
                    x = -x;
                    y = -y;
                }

                if (animal1_y == animal_y && x == 210)  //水平交换
                {
                    exchange.GetComponent<ChangeAnimals>().ExchangeAnimals();      //调用交换精灵的方法
                    exchange.GetComponent<ChangeAnimals>().Clear();               //调用清空数组的方法
                }
                else if(animal1_x == animal_x && y==205)
                {
                    //竖直交换
                    exchange.GetComponent<ChangeAnimals>().ExchangeAnimals();
                    exchange.GetComponent<ChangeAnimals>().Clear();
                }
                else
                {
                    exchange.GetComponent<ChangeAnimals>().Clear();
                }
            }
        }
        else
        {
            son.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        }

        exchange.GetComponent<ChangeAnimals>().exchangei();    
    }

    /*private void OnMouseEnter(BaseEventData pointData)
    {
    }

    private void OnMouseExit(BaseEventData pointData)
    {
    }*/

    // Update is called once per frame
    void Update ()
    {
		
	}
}
