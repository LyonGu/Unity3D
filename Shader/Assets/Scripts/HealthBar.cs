using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HealthBar : MonoBehaviour
{

    public class GridHpData
    {
        public GameObject Obj;
        public bool isUse;
        public float endHp;
        public float startHp;

    }

    public GameObject originGameObject;

    public GameObject originShieldGameObject;  //护盾UI

    //test
    public float maxHp;

    public float shieldHP;

    [Range(0,3000)]
    public float curHp;

    private float gridHp = 200;

    private List<GridHpData> gridMaxHpList = new List<GridHpData>();
     private List<GridHpData> gridShieldHpList = new List<GridHpData>();

    private float _curGridNum = 0;

    private string maxHpPre = "MaxHp_";
    private string shieldHPPre = "ShieldHp_";



    void Start()
    {

        curHp = maxHp + shieldHP;
        originGameObject.SetActive(false);
        originShieldGameObject.SetActive(false);
        CreateGridHp();

    }

    //参数：d表示要四舍五入的数；i表示要保留的小数点后为数。
    double Round(double d, int i)
    {
    　　if(d >=0)
    　　{
    　　　　d += 5 * Mathf.Pow(10, -(i + 1));
    　　}
    　　else
    　　{
    　　　　d += -5 * Mathf.Pow(10, -(i + 1));
    　　}
    　　string str = d.ToString();
    　　string[] strs = str.Split('.');
    　　int idot = str.IndexOf('.');
    　　string prestr = strs[0];
    　　string poststr = strs[1];
    　　if(poststr.Length > i)
    　　{
    　　　　poststr = str.Substring(idot + 1, i);
    　　}
    　　string strd = prestr + "." + poststr;
    　　d = Double.Parse(strd);
    　　return d;
    }

    //生成格子血条
    void CreateGridHp()
    {
        /*
            1 根据最大血量算出需要格子数
            2 根据宽度算出，每个格子的真实大小
        */

        //调整真实大小
        float totalHp = maxHp + shieldHP;
        int maxHpGridNum = (int)Round(maxHp/gridHp, 0);
        int shieldHpGridNum = (int)Round(shieldHP/gridHp, 0);

        int num = maxHpGridNum + shieldHpGridNum;
        _curGridNum = num;

        float sizeW = this.GetComponent<RectTransform>().sizeDelta.x;
        float gridHpW = sizeW/num;
        float gridRealHpValue = totalHp/num;
        RectTransform originRectTransform = originGameObject.GetComponent<RectTransform>();
        Rect originRect = originRectTransform.rect;
        Vector2 sizeDelta = new Vector2(gridHpW, originRect.height);
        originRectTransform.sizeDelta = sizeDelta;

        float posX = 0;
        float posY = originRectTransform.anchoredPosition.y;
        Vector2 pos = new Vector2(posX, posY);
        for (int i = 0; i < num; i++)
        {

            GameObject imageObj;
            bool isShield = false;
            if(i <= maxHpGridNum-1)
            {
                //创建血条格子数
                imageObj = GameObject.Instantiate(originGameObject);
                imageObj.name = maxHpPre + (i + 1).ToString();
            }
            else
            {
                //创建护盾格子数
                imageObj = GameObject.Instantiate(originShieldGameObject);
                isShield = true;
                imageObj.name = shieldHPPre + (i + 1).ToString();
            }


            //创建材质 作为备选方案
            // Material material = new Material(Shader.Find ("Common/EnimyHp"));
            // material.hideFlags = HideFlags.DontSave;
            // imageObj.GetComponent<Image>().material = material;

            posX = i* gridHpW;
            pos.x = posX;
            UpdateSingleGridHp(imageObj, pos, sizeDelta, this.gameObject.transform);

            GridHpData data = new GridHpData();
            data.Obj = imageObj;
            data.isUse = true;
            data.startHp = i * gridRealHpValue;
            data.endHp = gridRealHpValue * (i + 1);

            if(!isShield)
            {
                gridMaxHpList.Add(data);
            }
            else
            {
                gridShieldHpList.Add(data);
            }

        }

    }



    void RefreshHP(List<GridHpData> hpList, GameObject originGameObject, int gridNum, int offsetIndex, string namePre)
    {
        foreach (var item in hpList)
        {
            item.Obj.SetActive(false);
            item.isUse = false;
        }

        float sizeW = this.GetComponent<RectTransform>().sizeDelta.x;
        float gridHpW = sizeW/_curGridNum;
        float gridRealHpValue = (maxHp + shieldHP)/_curGridNum;
        RectTransform originRectTransform = originGameObject.GetComponent<RectTransform>();
        Rect originRect = originRectTransform.rect;

        Vector2 sizeDelta = new Vector2(gridHpW, originRect.height);
        originRectTransform.sizeDelta = sizeDelta;
        float posY = originRectTransform.anchoredPosition.y;
        Vector2 pos = new Vector2(0, posY);
        int curCount = hpList.Count;
        if(curCount > gridNum)
        {
            //池子里的直接够用
            for (int i = 0; i < gridNum; i++)
            {
                //复用同一个对象
                int index = offsetIndex + i;
                GridHpData data = hpList[i];
                pos.x = index* gridHpW;
                data.isUse = true;
                data.startHp = index * gridRealHpValue;
                data.endHp = gridRealHpValue * (index + 1);
                UpdateSingleGridHp(data.Obj, pos, sizeDelta);
            }

        }
        else
        {
            //池子里的不够用，先用完池子里的再创建新的
            for (int i = 0; i < curCount; i++)
            {
                //复用同一个对象
                int index = offsetIndex + i;
                GridHpData data = hpList[i];
                pos.x = index* gridHpW;
                data.isUse = true;
                data.startHp = index * gridRealHpValue;
                data.endHp = gridRealHpValue * (index + 1);
                UpdateSingleGridHp(data.Obj, pos, sizeDelta);
            }

            for (int i = curCount; i < gridNum; i++)
            {

                int index = offsetIndex + i;
                GameObject imageObj = GameObject.Instantiate(originGameObject);
                imageObj.name = namePre + (index + 1).ToString();
                pos.x = index* gridHpW;
                UpdateSingleGridHp(imageObj, pos, sizeDelta, this.gameObject.transform);
                GridHpData data = new GridHpData();
                data.Obj = imageObj;
                data.isUse = true;
                data.startHp = index * gridRealHpValue;
                data.endHp = gridRealHpValue * (index + 1);
                hpList.Add(data);
            }
        }

    }

    void UpdateGridHp(int maxHpNum, int curHpNum, int shieldHPNum)
    {
        RefreshHP(gridMaxHpList, originGameObject, curHpNum, 0, maxHpPre);
        if (shieldHP > 0)
        {
            RefreshHP(gridShieldHpList, originShieldGameObject, shieldHPNum, curHpNum, shieldHPPre);
        }
    }

    void RefreshCurHP(List<GridHpData> hpList)
    {
        foreach (var item in hpList)
        {
            if(item.isUse)
            {
                if(curHp > item.endHp)
                {
                    item.Obj.GetComponent<Image>().fillAmount = 1.0f;
                }
                else
                {
                    float startHp = item.startHp;
                    float ratio = (curHp - startHp)/(item.endHp - startHp);
                    item.Obj.GetComponent<Image>().fillAmount = ratio;
                }
            }
            else
            {
                break;
            }
        }
    }
    void UpdateCurGridHp()
    {
        if(shieldHP > 0)
        {
            RefreshCurHP(gridShieldHpList);
        }
        RefreshCurHP(gridMaxHpList);
    }

    void UpdateSingleGridHp(GameObject obj, Vector2 anchoredPosition, Vector2 sizeDelta, Transform parent = null)
    {
        obj.SetActive(true);
        RectTransform t = obj.GetComponent<RectTransform>();
        if(parent!=null)
        {
            t.parent = parent;
        }
        t.anchoredPosition = anchoredPosition;
        t.sizeDelta = sizeDelta;

    }

    void Update()
    {
        float totalHp = curHp + shieldHP;
        int maxHpGridNum = (int)Round(maxHp/gridHp, 0);
        int shieldHpGridNum = (int)Round(shieldHP/gridHp, 0);
        int num = maxHpGridNum + shieldHpGridNum;
        if(num != _curGridNum)
        {
            _curGridNum = num;
            int curHpGridNum = (int)Round(curHp / gridHp, 0);
            curHp = totalHp;
            
            UpdateGridHp(maxHpGridNum, curHpGridNum, shieldHpGridNum);
        }

        // //更新当前血量
        UpdateCurGridHp();

    }

}
