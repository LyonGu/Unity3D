using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    //test
    public float maxHp;

    [Range(0,1500)]
    public float curHp;

    private float gridHp = 200;

    private List<GridHpData> gridHpList = new List<GridHpData>();

    private float _curGridNum = 0;



    void Start()
    {

        curHp = maxHp;
        originGameObject.SetActive(false);
        CreateGridHp();

    }

    //生成格子血条
    void CreateGridHp()
    {
        /*
            1 根据最大血量算出需要格子数
            2 根据宽度算出，每个格子的真实大小
        */

        //调整真实大小
        int num = Mathf.RoundToInt(maxHp/gridHp);
        _curGridNum = num;
        float sizeW = this.GetComponent<RectTransform>().sizeDelta.x;
        float gridHpW = sizeW/num;
        float gridRealHpValue = maxHp/num;
        RectTransform originRectTransform = originGameObject.GetComponent<RectTransform>();
        Rect originRect = originRectTransform.rect;
        Vector2 sizeDelta = new Vector2(gridHpW, originRect.height);
        originRectTransform.sizeDelta = sizeDelta;

        float posX = 0;
        float posY = originRectTransform.anchoredPosition.y;
        Vector2 pos = new Vector2(posX, posY);
        for (int i = 0; i < num; i++)
        {
            //创建血条格子数
            GameObject imageObj = GameObject.Instantiate(originGameObject);
            imageObj.name = (i + 1).ToString();


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
            gridHpList.Add(data);
        }
    }

    void UpdateGridHp(int num)
    {
        //待优化 重复利用 隐藏
        foreach (var item in gridHpList)
        {
            item.Obj.SetActive(false);
            item.isUse = false;
        }

        float sizeW = this.GetComponent<RectTransform>().sizeDelta.x;
        float gridHpW = sizeW/num;
        float gridRealHpValue = maxHp/num;
        RectTransform originRectTransform = originGameObject.GetComponent<RectTransform>();
        Rect originRect = originRectTransform.rect;

        Vector2 sizeDelta = new Vector2(gridHpW, originRect.height);
        originRectTransform.sizeDelta = sizeDelta;
        float posY = originRectTransform.anchoredPosition.y;
        Vector2 pos = new Vector2(0, posY);
        int curCount = gridHpList.Count;
        if(curCount > num)
        {
            //池子里的直接够用
            for (int i = 0; i < num; i++)
            {
                //复用同一个对象
                GridHpData data = gridHpList[i];
                pos.x = i* gridHpW;
                data.isUse = true;
                data.startHp = i * gridRealHpValue;
                data.endHp = gridRealHpValue * (i + 1);
                UpdateSingleGridHp(data.Obj, pos, sizeDelta);
            }

        }
        else
        {
            //池子里的不够用，先用完池子里的再创建新的
            for (int i = 0; i < curCount; i++)
            {
                //复用同一个对象
                GridHpData data = gridHpList[i];
                pos.x = i* gridHpW;
                data.isUse = true;
                data.startHp = i * gridRealHpValue;
                data.endHp = gridRealHpValue * (i + 1);
                UpdateSingleGridHp(data.Obj, pos, sizeDelta);
            }

            for (int i = curCount; i < num; i++)
            {

                GameObject imageObj = GameObject.Instantiate(originGameObject);
                imageObj.name = (i + 1).ToString();
                pos.x = i* gridHpW;
                UpdateSingleGridHp(imageObj, pos, sizeDelta, this.gameObject.transform);
                GridHpData data = new GridHpData();
                data.Obj = imageObj;
                data.isUse = true;
                data.startHp = i * gridRealHpValue;
                data.endHp = gridRealHpValue * (i + 1);
                gridHpList.Add(data);
            }
        }

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
        int num = Mathf.RoundToInt(maxHp/gridHp);
        if(num != _curGridNum)
        {
            _curGridNum = num;
            //curHp = maxHp;
            UpdateGridHp(num);
        }

        //更新当前血量
        foreach (var item in gridHpList)
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

}
