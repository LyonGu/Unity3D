/***
 *
 *  Title: 
 *         第27章:  预加载与对象缓冲池技术
 *         
 *                  对象缓冲池管理器        
 * 
 *  Description:
 *        基本原理：
 *        通过池管理思路，在游戏初始化的时候，生成一个初始的池存放我们要复用的元素。
 *        当要用到游戏对象时，从池中取出；不再需要的时候，不直接删除对象而是把对象重新回收到“池”中。
 *        这样就避免了对内存中大量对象的反复实例化与回收垃圾处理，提高了资源的利用率。      
 *
 *  Date: 2017
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */
using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour{
        public GameObject ObjPrefab;                                           //池中所使用的元素预设
        public Transform TranObjPrefabParent;                                  //池中所使用的元素预设的父对象
        public int InitialCapacity;                                            //初始容量

        private int _startCapacityIndex;                                       //初始下标
        private List<int> _avaliableIndex;                                     //可用“池”游戏对象下标
        private Dictionary<int, GameObject> _totalObjList;                     //池中全部元素的容器


        /// <summary>
        /// 初始化缓冲池
        /// </summary>
        void Awake(){
            _avaliableIndex = new List<int>(InitialCapacity);
            _totalObjList = new Dictionary<int, GameObject>(InitialCapacity);
            //初始化池
            expandPool();
        }

        /// <summary>
        /// 取得游戏对象。
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<int, GameObject> PickObj(){
            //容量不够，进行“池”扩展
            if (_avaliableIndex.Count == 0)
                expandPool();
            //取得一个可用的池下标数值
            int id = _avaliableIndex[0];
            //“可用池下标”集合，删除对应下标
            _avaliableIndex.Remove(id);
            //设置“池”对象可用。
            _totalObjList[id].SetActive(true);
            //从“池”中提取一个对象返回。
            return new KeyValuePair<int, GameObject>(id, _totalObjList[id]);
        }

        /// <summary>
        /// 回收游戏对象
        /// </summary>
        /// <param name="id"></param>
        public void RecyleObj(int id) {
            //设置对应对象不可用（即：放回池操作）
            _totalObjList[id].SetActive(false);
            //指定Id的游戏对象下标，重行进入可用“池”下标集合中
            _avaliableIndex.Add(id);
        }

        /// <summary>
        /// 扩展池
        /// </summary>
        private void expandPool(){
            int start = _startCapacityIndex;
            int end = _startCapacityIndex + InitialCapacity;

            for (int i = start; i < end; i++){
                //加入验证判断，避免在多个请求同时触发扩展池需求
                if (_totalObjList.ContainsKey(i))
                continue;
                GameObject newObj = Instantiate(ObjPrefab) as GameObject;
                //生成的池对象增加父对象，容易查看与检查。
                newObj.transform.parent = TranObjPrefabParent.transform;
                //每一个生成的对象，设置暂时不可用。
                newObj.SetActive(false);
                //下标记入“池”可用下标集合中。
                _avaliableIndex.Add(i);
                //新产生的对象，并入本池容器集合中。
                _totalObjList.Add(i, newObj);
            }
            //扩展“初始下标”。
            _startCapacityIndex = end;
        }
    }//Class_end