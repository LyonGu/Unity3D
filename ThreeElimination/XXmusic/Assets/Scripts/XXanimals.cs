using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum State    //枚举动物状态机
{
    Static, //静止状态
    Move, //移动状态
}
public class XXanimals : MonoBehaviour
{
    //声明状态机初始为静态
    public State sta = State.Static;
    //水平动物数组
    public List<RaycastHit2D> horizontal;
    //垂直动物数组
    public List<RaycastHit2D> vertical; 
    //发射射线标记
    public bool IsRaycast = true;
    //动物移动速度  
    public int speed;
    //动物要去的目标点星星
    public GameObject stars;
    public GameObject passpanel;

    void OnEnable()
    {
        speed = 1500;
        stars = GameObject.Find("Stars");
        passpanel = GameObject.Find("PassPanel");
        horizontal = new List<RaycastHit2D>();
        vertical = new List<RaycastHit2D>();
    }
    void Update()
    {
        if (sta == State.Move)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<SpriteRenderer>().sortingLayerName = "Frog";
            Destroy(GetComponent<BoxCollider2D>());
            Invoke("AnimalMovement", 0.2f);

        }
        
        if (passpanel.GetComponent<PassPanel>().Isshow2 == true && passpanel.GetComponent<PassPanel>().Isshow4 == true && passpanel.GetComponent<PassPanel>().Isshow5 == true)
        {
            SuccessPanel.instance.gameObject.SetActive(true);     //闯关成功面板
        }

        XXRaycast();
    }
    public void XXRaycast()       //向左右上下发射射线
    {
        if (IsRaycast == true)
        {
            var left = Physics2D.RaycastAll(transform.position, Vector3.left, 200);     //向左发射一条200的射线(会射到自己)
            var right = Physics2D.RaycastAll(transform.position, Vector3.right, 200);
            var up = Physics2D.RaycastAll(transform.position, Vector3.up, 200);
            var down = Physics2D.RaycastAll(transform.position, Vector3.down, 200);

            AddList(horizontal, left, right);
            AddList(vertical, up, down);
        }
    }

    //向水平垂直数组里添加动物
    public void AddList(List<RaycastHit2D> list, RaycastHit2D[] array1, RaycastHit2D[] array2)   //3个变量，第一个为数组变量，后两个为方向变量
    {
        if (list.Count < 3)
        {
            foreach (var a in array1)
            {
                if (check(list, a))      //如果a在当前数组里面没有,就添加进去
                {
                    list.Add(a);
                }
            }
            foreach (var b in array2)
            {
                if (check(list, b))
                {
                    list.Add(b);
                }
            }
        }
        else
        {
            if (FF(list))
            {
                foreach (var item in list)
                {
                    //如果FF返回值为True就把数组里面的动物状态机都改为Move
                    item.transform.GetComponent<XXanimals>().sta = State.Move;
                }
            }
        }
    }

    public bool FF(List<RaycastHit2D> all)      //判断数组里动物的tag是否一样
    {
        for (int i = 0; i < all.Count; i++)
        {
            try
            {
                if (all[i].transform.tag != gameObject.tag)   //如果有一个与自己的tag不一样就返回false
                    return false;
            }
            catch
            { return false; }
        }
        return true;
    }

    public void AnimalMovement()        //动物移动
    {
        IsRaycast = false;
        //当前物体向这某一个物体移动
        transform.position = Vector3.MoveTowards(transform.position, stars.transform.position, speed * Time.deltaTime);
        //两个坐标相减是方向,用sqrMagnitude获取方向的距离
        if ((transform.position - stars.transform.position).sqrMagnitude < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    public bool check(List<RaycastHit2D> list, RaycastHit2D obj)    //判断当前数组里是否有该动物
    {
        foreach (var item in list)
        {
            try
            {
                if (item.transform.gameObject == obj.transform.gameObject)
                    return false;
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    void OnDisable()            //OnDisable当被销毁时执行一帧
    {
        if (this.gameObject.tag == "2")
        {
            passpanel.GetComponent<PassPanel>().number2++;
            passpanel.GetComponent<PassPanel>().PassText();
            if (passpanel.GetComponent<PassPanel>().number2 >= 6)
            {
                passpanel.GetComponent<PassPanel>().photo2.transform.gameObject.SetActive(true);
                passpanel.GetComponent<PassPanel>().test2.GetComponent<Text>().color = Color.green;
                passpanel.GetComponent<PassPanel>().test22.GetComponent<Text>().color = Color.green;
                passpanel.GetComponent<PassPanel>().Isshow2 = true;
            }
        }

        if (this.gameObject.tag == "4")
        {
            passpanel.GetComponent<PassPanel>().number4++;
            
            passpanel.GetComponent<PassPanel>().PassText();
            if (passpanel.GetComponent<PassPanel>().number4 >= 10)
            {
                passpanel.GetComponent<PassPanel>().photo4.transform.gameObject.SetActive(true);
                passpanel.GetComponent<PassPanel>().test4.GetComponent<Text>().color = Color.green;
                passpanel.GetComponent<PassPanel>().test44.GetComponent<Text>().color = Color.green;
                passpanel.GetComponent<PassPanel>().Isshow4 = true;
            }
        }

        if (this.gameObject.tag == "5")
        {
            passpanel.GetComponent<PassPanel>().number5++;
            passpanel.GetComponent<PassPanel>().PassText();
            if (passpanel.GetComponent<PassPanel>().number5 >= 9)
            {
                passpanel.GetComponent<PassPanel>().photo5.transform.gameObject.SetActive(true);
                passpanel.GetComponent<PassPanel>().test5.GetComponent<Text>().color = Color.green;
                passpanel.GetComponent<PassPanel>().test55.GetComponent<Text>().color = Color.green;
                passpanel.GetComponent<PassPanel>().Isshow5 = true;
            }
        }
    }

    //StartCoroutine(AnimalMovement());     //开启携程
    //IEnumerator AnimalMovement()   //携程
    //{
    //    yield return new WaitForSeconds(2f);  //延迟5秒执行

    //    for (int j = 0; j < 30; )
    //    {
    //        this.transform.position = new Vector3(this.transform.position.x + j, this.transform.position.y + j, 0);
    //        yield return new WaitForFixedUpdate();    //等一帧再执行

    //        if (j == 20)
    //        {
    //            StopAllCoroutines();    //停止所有携程
    //            DestroyAll();           //销毁动物
    //        }
    //        AllMovement();
    //        j = j + 1;
    //    }
    //}
}
