using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderDrawLine : MonoBehaviour
{

    public LineRenderer lineRender;
    public GameObject movePrefab;
    
    private Camera _mainCamera;
    private GameObject _moveGameObject;
    private Transform _moveGameObjectTransform;
    private List<Vector3> _posList = new List<Vector3>();
    private int _posIndex = -1;
    private bool _isStartMove;
    void Start()
    {
        _mainCamera = Camera.main;
        GameObject moveGameObject = Instantiate(movePrefab);
        moveGameObject.SetActive(false);
        _moveGameObject = moveGameObject;
        _moveGameObjectTransform = moveGameObject.transform;
        ResetPosInfo();
    }

    private void ResetPosInfo()
    {
        _posList.Clear();
        _posIndex = -1;
        lineRender.positionCount = 0;
        _isStartMove = false;
        _movePosIndex = 0;
    }

    private Vector3 GetWorldPos()
    {
        var screenPos = Input.mousePosition;
        float cameraZ = _mainCamera.transform.position.z;
        Vector3 wordlPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cameraZ)));
        return wordlPos;
    }
    
    
    private void UpdateTrackLine()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ResetPosInfo();
            Vector3 wordlPos = GetWorldPos();
            _posList.Add(wordlPos);
            _posIndex++;
            lineRender.positionCount = _posIndex + 1;
            lineRender.SetPosition(_posIndex, wordlPos);
            _moveGameObjectTransform.position = wordlPos;

        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 wordlPos = GetWorldPos();
            _posList.Add(wordlPos);
            _posIndex++;
            lineRender.positionCount = _posIndex + 1;
            lineRender.SetPosition(_posIndex, wordlPos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Vector3 wordlPos = GetWorldPos();
            // _posList.Add(wordlPos);
            // _posIndex++;
            // lineRender.positionCount = _posIndex + 1;
            // lineRender.SetPosition(_posIndex, wordlPos);
            _isStartMove = true;

            //提前修正下位置坐标
            //UpdateAdjustWorldPos();

            UpdateAdjustWorldPos1();
        }
    }

    private void AddToNewList(List<Vector3> newList, Vector3 pos)
    {
        if (!newList.Contains(pos))
        {
            newList.Add(pos);
        }
    }
    private void UpdateAdjustWorldPos1()
    {

        if (_posList.Count > 0)
        {
            List<Vector3> newPosList = new List<Vector3>();
            bool isHit = false;
            Vector3 starthitPoint = Vector3.zero;
            Vector3 preHitpoint = Vector3.zero;
            for (int i = 0; i < _posList.Count - 1; i++)
            {
                Vector3 startPos = _posList[i];
                if (isHit)
                {
                    startPos = starthitPoint;
                }
                Vector3 targetPos = _posList[i + 1];
                Vector3 targetDir = targetPos - startPos;
                Vector3 rayCastdir = targetDir.normalized;
                float dis = targetDir.magnitude;
                if (Physics.Raycast(startPos, rayCastdir, out RaycastHit hitInfo, dis))
                {

                    isHit = true;
                    starthitPoint = startPos; //记录碰撞的起始点
                    Vector3 hitPoint = hitInfo.point;
    
                    Vector3 startToHitPoint = hitPoint - startPos;
                    float startToHitPointDis = startToHitPoint.magnitude;

                    Vector3 HitPointToTarget = targetPos - hitPoint;
                    float HitPointToTargetDis = HitPointToTarget.magnitude;


                    Vector3 normal = hitInfo.normal;
                    Vector3 normalN = normal.normalized;
                    Vector3 reflectDir = Vector3.Reflect(rayCastdir, normal);
                    Vector3 reflectDirT = reflectDir.normalized * HitPointToTargetDis;
                    //reflectDir = reflectDir.normalized;
                    //Vector3 reflectT = startToHitPointDis * reflectDir;
                    float nr_dot = Vector3.Dot(normal, reflectDir);
                   // float rad = Mathf.Acos(nr_dot);
                    float DisN = HitPointToTargetDis * nr_dot;
                    Vector3 Ndir = DisN * normalN;

                    Vector3 targetV = reflectDirT - Ndir;
                    Vector3 targetNewPos = targetV + hitPoint;



                    //沿碰撞面的方向向量是
                    //Vector3 targetV = startToHitPoint + Ndir;
                    //Vector3 targetNewPos = targetV + hitPoint;

                    if(!newPosList.Contains(startPos))
                        newPosList.Add(startPos);

                    if (preHitpoint == Vector3.zero)
                    {
                        //newPosList.Add(hitPoint);
                        //newPosList.Add(targetNewPos);
                        AddToNewList(newPosList, hitPoint);
                        AddToNewList(newPosList, targetNewPos);
                        preHitpoint = hitPoint;
                    }
                    else
                    {
                       
                        Vector3 prePos = _posList[i-1];
                        Vector3 start1Pos = _posList[i];
                        Vector3 nextPos = _posList[i+1];

                        Vector3 one1 = start1Pos - prePos;
                        Vector3 one2 = nextPos - start1Pos;
                        if (Vector3.Dot(one1, one2) >= 0)
                        {
                            //方向未改变， 仅仅加入方向一致的点
                            Vector3 t1 = hitPoint - preHitpoint;
                            Vector3 t2 = targetNewPos - preHitpoint;
                            if (Vector3.Dot(targetV, t1) >= 0)
                            {
                                //newPosList.Add(hitPoint);
                                //AddToNewList(newPosList, hitPoint);
                                preHitpoint = hitPoint;
                            }

                            if (Vector3.Dot(targetV, t2) >= 0)
                            {
                                newPosList.Add(targetNewPos);
                            }
                        }
                        else
                        {
                            //方向改变
                            //newPosList.Add(hitPoint);
                            //newPosList.Add(targetNewPos);
                            //AddToNewList(newPosList, hitPoint);
                            AddToNewList(newPosList, targetNewPos);
                            preHitpoint = hitPoint;
                        }
                    }

 
                }
                else
                {
                    if(isHit)
                    {
                        //之前发生过碰撞
                        //newPosList.Add(targetPos);
                        AddToNewList(newPosList, targetPos);
                    }
                    //没有碰撞不需要修正
                    isHit = false;
                    starthitPoint = Vector3.zero;
                    //if(!newPosList.Contains(startPos))
                    //    newPosList.Add(startPos);

                    AddToNewList(newPosList, startPos);
                }
            }

            _posList = newPosList;

            //lineRender.positionCount = _posList.Count;
            //lineRender.SetPositions(_posList.ToArray());
        }
    }

    private void UpdateAdjustWorldPos()
    {
        
        if (_posList.Count > 0)
        {
            List<Vector3> newPosList = new List<Vector3>();
            bool isHit = false;
            for (int i = 0; i < _posList.Count-1; i++)
            {
                Vector3 startPos = _posList[i];
                if (isHit)
                {
                    startPos = newPosList[newPosList.Count - 1];
                }

                Vector3 targetPos = _posList[i + 1];
                Vector3 targetDir = targetPos - startPos;
                Vector3 rayCastdir = targetDir.normalized;
                float dis = targetDir.magnitude;
                if (Physics.Raycast(startPos, rayCastdir, out RaycastHit hitInfo, dis))
                {
                    isHit = true;
                    //修复终点坐标
                    Vector3 hitPointWorldPos = hitInfo.point;
                    newPosList.Add(startPos);
                    newPosList.Add(hitPointWorldPos);
                    // newPosList.Add(hitPointWorldPos);
                    Vector3 offset = targetPos - hitPointWorldPos;

                    var nextTargetPox = Vector3.zero;
                    nextTargetPox.z = hitPointWorldPos.z;
                    var hDir = new Vector3(offset.x, 0, 0);
                    hDir = hDir.normalized;
                    //水平方向
                    if (Physics.Raycast(hitPointWorldPos, hDir, out RaycastHit hitInfo1,
                        Mathf.Abs(offset.x)))
                    {
                        //水平方向不能移动了
                        nextTargetPox.x = hitPointWorldPos.x;

                    }
                    else
                    {
                        //水平方向能移动了
                        nextTargetPox.x = targetPos.x;
                    }
                    var vDir = new Vector3(0, offset.y, 0);
                    vDir = vDir.normalized;
                    if (Physics.Raycast(hitPointWorldPos, vDir, out RaycastHit hitInfo2,
                        Mathf.Abs(offset.y)))
                    {
                        //竖直方向不能移动了
                        nextTargetPox.y = hitPointWorldPos.y;

                    }
                    else
                    {
                        //竖直方向能移动了
                        nextTargetPox.y = targetPos.y;
                    }
                    if(!nextTargetPox.Equals(hitPointWorldPos))
                        newPosList.Add(nextTargetPox);
                }
                else
                {
                    //没有碰撞不需要修正
                    isHit = false;
                    newPosList.Add(startPos);
                }
            }

            _posList = newPosList;

            lineRender.positionCount = _posList.Count;
            lineRender.SetPositions(_posList.ToArray());
        }
    }

    private int _movePosIndex = 0;
    private Vector3 _tempTargetPos;
    private void UpdateBallMove()
    {
        if (_posList.Count > 0)
        {
            _moveGameObject.SetActive(true);
            Vector3 curPos = _moveGameObjectTransform.position;
            if (curPos == _tempTargetPos)
            {
                //更新目标位置
                _movePosIndex++;
                if (_movePosIndex == _posList.Count)
                {
                    //移动结束
                    _moveGameObject.SetActive(false);
                    _isStartMove = false;
                    return;
                }

                _tempTargetPos = _posList[_movePosIndex];
            }
            else
            {

                Vector3 updatePos = Vector3.Slerp(curPos, _tempTargetPos, Time.deltaTime * 500);
                _moveGameObjectTransform.position = updatePos;
            }
            
            // //添加碰撞物阻挡逻辑，修正位置
            // //水平射线和竖直射线
            // Vector3 startPos = curPos;
            // Vector3 targetPos = _tempTargetPos;
            // Vector3 targetDir = targetPos - startPos;
            // Vector3 rayCastdir = targetDir.normalized;
            // float dis = targetDir.magnitude;
            // if (Physics.Raycast(startPos, rayCastdir, out RaycastHit hitInfo, dis))
            // {
            //     Vector3 hitPointWorldPos = hitInfo.point;
            //     //水平方向检测
            //     float h = hitPointWorldPos.x - startPos.x;
            //     Vector3 hrayCastdir = new Vector3(h, 0 ,0);
            //     if(Physics.Raycast(startPos, hrayCastdir, out RaycastHit hitInfo1, h))
            //     {
            //         Vector3 curPos1 = _moveGameObjectTransform.position;
            //         Vector3 updatePos1 = Vector3.Slerp(curPos1, hitInfo1.point, Time.deltaTime * 500);
            //         _moveGameObjectTransform.position = new Vector3(updatePos1.x, curPos1.y,curPos1.z);
            //     }
            //     
            //     //竖直方向检测
            //     float v = hitPointWorldPos.y - startPos.y;
            //     Vector3 vrayCastdir = new Vector3(0, v ,0);
            //     if(Physics.Raycast(startPos, vrayCastdir, out RaycastHit hitInfo2, v))
            //     {
            //         Vector3 curPos2 = _moveGameObjectTransform.position;
            //         Vector3 updatePos2 = Vector3.Slerp(curPos2, hitInfo2.point, Time.deltaTime * 500);
            //         _moveGameObjectTransform.position = new Vector3(curPos2.x, updatePos2.y,curPos2.z);
            //     }
            // }
            // else
            // {
            //     
            // }

        }
    }

    void Update()
    {
        UpdateTrackLine();
        if (_isStartMove)
        {
            UpdateBallMove();
        }

    }
}
