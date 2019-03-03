
/***
 *
 *  Title: "3DRunner" 项目
 *         描述：场景创建管理器
 *
 *  Description:
 *        功能：
 *        1：大型场景道具的创建与删除
 *        2：小型道具的创建与删除
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScenesCreateMgr : BasePropItem {

    //场景建筑预设
    public GameObject BuildingPrefab = null;

    //场景预设的父节点
    public Transform ParentNodeByBuildPrefab = null; 

    //场景生成点预设
    public GameObject BuildingCreatePrefab = null;

    //金币预设
    public GameObject CoinPrefabs = null;

    //障碍物预设集合
    public GameObject[] ObstaclesPrefabsArray = null;

    //魔法道具预设集合
    public GameObject[] MagicProPrefabsArray = null;

    //生成道具的父节点
    public Transform ParentNodeByProp = null;
    //道具定位参考坐标点
    public Transform _PropRefPostion = null;
    //是否已经生成金币
    private bool _IsProduceCoin = false;   

	// Use this for initialization
	void Start () {
		//参数检查
        //if (BuildingPrefab == null || BuildingCreatePrefab == null || _PropRefPostion == null)
        //{
        //    Debug.LogError(GetType() + "/Start()/脚本参数有错误，请改正。");
        //}

        //初始道具集合
        CreateProps();
        //初始建筑集合
        CreateBuildings();

	}

    void Update()
    {
        if (Global.IsCreateBuildings == true)
        {
            CreateProps();
            CreateBuildings();
            Global.IsCreateBuildings = false;
        }
    }

    /// <summary>
    /// 生成道具预设
    /// 核心算法：
    /// 	 A： 一个“建筑物群” ，长度32米。一次生成2个，所以就一次生成64米的“场景道路”。
    ///      B： 一般以6米作为一个生成道具的基础单位。 所以每一次生成道具，都循环10次。64/6=10（次）
    ///      C:  每个跑道，对于金币、道具(障碍物/魔法道具)都各占1/3的生成概率。
    /// </summary>
    private void CreateProps()
    {
        for (int i = 0; i < 10;i++ )
        {
             //(本循环)道具Z轴长度数值
            float floZLengthNumber = 0F;
            _IsProduceCoin = false;
            floZLengthNumber = Global.ZposByCurrentBuilds + 30F + i * Global.ZLengthEveryFloor;

            //左边跑道
            switch (base.GetRandomNum(0,3))
            {
                case 0:
                    if(!_IsProduceCoin)
                    {
                        //生成金币道具
                        ProduceCoins(base.GetRandomNum(2, 4), new Vector3(Global.LeftTrackX, _PropRefPostion.position.y, floZLengthNumber));
                        _IsProduceCoin = true;
                    }
                    break;
                case 1:
                    //生成障碍物道具
                    ProduceObstaclesProp(new Vector3(Global.LeftTrackX, _PropRefPostion.position.y, floZLengthNumber));
                    //生成魔法道具
                    ProduceMagicProp(new Vector3(Global.LeftTrackX, _PropRefPostion.position.y, floZLengthNumber));
                    break;
                case 2:
                    break;
                case 3:
                    if (!_IsProduceCoin)
                    {
                        //生成金币道具
                        ProduceCoins(base.GetRandomNum(3, 4), new Vector3(Global.LeftTrackX, _PropRefPostion.position.y, floZLengthNumber));
                        _IsProduceCoin = true;
                    }
                    break;
                default:
                    break;
            }

            //中间跑道
            switch (base.GetRandomNum(0, 3))
            {
                case 0:
                    if (!_IsProduceCoin)
                    {
                        //生成金币道具
                        ProduceCoins(base.GetRandomNum(3, 4), new Vector3(0f, _PropRefPostion.position.y, floZLengthNumber));
                        _IsProduceCoin = true;
                    }
                    break;
                case 1:
                    //生成障碍物道具
                    ProduceObstaclesProp(new Vector3(0F, _PropRefPostion.position.y, floZLengthNumber));
                    //生成魔法道具
                    ProduceMagicProp(new Vector3(0F, _PropRefPostion.position.y, floZLengthNumber));
                    break;
                case 2:
                    if (!_IsProduceCoin)
                    {
                        //生成金币道具
                        ProduceCoins(base.GetRandomNum(3, 4), new Vector3(0f, _PropRefPostion.position.y, floZLengthNumber));
                        _IsProduceCoin = true;
                    }
                    break;
                case 3:
                    //空内容
                    break;
                default:
                    break;
            }
            //右边跑道
            switch (base.GetRandomNum(0, 2))
            {
                case 1:
                    if (!_IsProduceCoin)
                    {
                        //生成金币道具
                        ProduceCoins(base.GetRandomNum(2, 4), new Vector3(Global.RightTrackX, _PropRefPostion.position.y, floZLengthNumber));
                        _IsProduceCoin = true;
                    }
                    break;
                case 0:
                    //生成障碍物道具
                    ProduceObstaclesProp(new Vector3(Global.RightTrackX, _PropRefPostion.position.y, floZLengthNumber));
                    //生成魔法道具
                    ProduceMagicProp(new Vector3(Global.RightTrackX, _PropRefPostion.position.y, floZLengthNumber));
                    break;
                case 2:
                    //这里就是什么也不生成，空场地。
                    break;
                default:
                    break;
            }

          
        }
    }

    /// 生成场景建筑预设
    private void CreateBuildings()
    {
        for (int i = 1; i <= 2; i++)
        {
            //通过预设动态生成gameobject
            GameObject goBuildingClone = Instantiate(BuildingPrefab,new Vector3(0,0,Global.ZposByCurrentBuilds + Global.ZLengthByBuildPrefab*i), Quaternion.identity);
            //确定父子节点
            if (ParentNodeByBuildPrefab != null)
            {
                //加入到父节点里,坐标就是new Vector3(0,0,Global.ZposByCurrentBuilds + Global.ZLengthByBuildPrefab*i) ==》相对于父节点坐标
                goBuildingClone.transform.parent = ParentNodeByBuildPrefab;
            }

            if (i == 1)
            {
                //场景的预设点 触发器
                GameObject goBuildTriggerPosClone = Instantiate(BuildingCreatePrefab,
                        new Vector3(0, 0, Global.ZposByCurrentBuilds + Global.ZLengthByBuildPrefab * i), Quaternion.identity);
                goBuildTriggerPosClone.transform.parent = ParentNodeByBuildPrefab;
            }
        
        }
        //更新建筑物当前长度数值
        Global.ZposByCurrentBuilds += Global.ZLengthByBuildPrefab * 2;
    }


    /// <summary>
    /// 生成金币
    /// </summary>
    /// <param name="produceNum">生成数量</param>
    /// <param name="pos">金币方位</param>
    /// <param name="parentNode">父节点</param>
    private void ProduceCoins(int produceNum, Vector3 pos)
    {
        //参数检查
        if (CoinPrefabs == null || pos == Vector3.zero || produceNum <= 0 || ParentNodeByProp == null)
        {
            Debug.LogError(GetType() + "/ProduceCoins()/参数有误， 请检查。");
        }

        for (int i = 0; i < produceNum; i++)
        {
            base.ClonePrefabs(CoinPrefabs, new Vector3(pos.x, pos.y, pos.z + i * Global.IntervalOfCoins), ParentNodeByProp, 0, Global.Coin);
        }
    }

    /// <summary>
    /// 生成障碍物道具
    /// </summary>
    /// <param name="pos"></param>
    private void ProduceObstaclesProp(Vector3 pos)
    {
        //参数检查
        if (ObstaclesPrefabsArray == null || ParentNodeByProp == null || pos == Vector3.zero)
        {
            Debug.LogError(GetType() + "/ProduceObstaclesProp()/参数有误， 请检查。");
            return;
        }
        else if (ObstaclesPrefabsArray.Length < 3)
        {
            Debug.Log(GetType() + "/ProduceObstaclesProp()/障碍物道具数量少， 请检查。");
            return;
        }
        base.ClonePrefabs(ObstaclesPrefabsArray[base.GetRandomNum(0, 3)], pos, ParentNodeByProp,0, "Obstacle");
    }

    /// <summary>
    /// 生成魔法道具
    /// </summary>
    /// <param name="pos"></param>
    private void ProduceMagicProp(Vector3 pos)
    {
        //参数检查
        if (MagicProPrefabsArray == null || pos == Vector3.zero)
        {
            Debug.LogError(GetType() + "/ProduceMagicProp()/参数有误， 请检查。");
            return;
        }
        else if (MagicProPrefabsArray.Length < 2)
        {
            Debug.Log(GetType() + "/魔法道具数量少， 请检查。");
            return;
        }

        //六分之一的概率
        if (base.GetProbability(6))
        {
            //Z轴添加一个偏移量，是保证与障碍物道具保持一定距离。
            GameObject goOrigin = MagicProPrefabsArray[base.GetRandomNum(0, 2)];
            base.ClonePrefabs(goOrigin, new Vector3(pos.x, pos.y, pos.z + Global.IntervalOfProp), ParentNodeByProp, 0, goOrigin.name);
        }
    }


}

