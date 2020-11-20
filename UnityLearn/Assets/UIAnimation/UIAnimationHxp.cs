using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UGameObject = UnityEngine.GameObject;

public class UIAnimationHxp : MonoBehaviour
{

    #region 配置数据结构定义

    [Serializable]
    //动画类型
    public enum AnimationType : byte
    {
        MoveTo,
        MoveBy,
        ScaleTo,
        ScaleBy,
        RotateTo,
        RotateBy,
        Fade,
        Color,
        GradientColor,
    }

    [Serializable]
    //动画控制类型
    public enum UIAnimationControlType:byte
    {
        AnimationCurve = 100,
        Unset = Ease.Unset,
        Linear = Ease.Linear,
        InSine = Ease.InSine,
        OutSine = Ease.OutSine,
        InOutSine = Ease.InOutSine,
        InQuad = Ease.InQuad,
        OutQuad = Ease.OutQuad,
        InOutQuad = Ease.InOutQuad,
        InCubic = Ease.InCubic,
        OutCubic = Ease.OutCubic,
        InOutCubic = Ease.InOutCubic,
        InQuart = Ease.InQuart,
        OutQuart = Ease.OutQuart,
        InOutQuart = Ease.InOutQuart,
        InQuint = Ease.InQuint,
        OutQuint = Ease.OutQuint,
        InOutQuint = Ease.InOutQuint,
        InExpo = Ease.InExpo,
        OutExpo = Ease.OutExpo,
        InOutExpo = Ease.InOutExpo,
        InCirc = Ease.InCirc,
        OutCirc = Ease.OutCirc,
        InOutCirc = Ease.InOutCirc,
        InElastic = Ease.InElastic,
        OutElastic = Ease.OutElastic,
        InOutElastic = Ease.InOutElastic,
        InBack = Ease.InBack,
        OutBack = Ease.OutBack,
        InOutBack = Ease.InOutBack,
        InBounce = Ease.InBounce,
        OutBounce = Ease.OutBounce,
        InOutBounce = Ease.InOutBounce,
        Flash = Ease.Flash,
        InFlash = Ease.InFlash,
        OutFlash = Ease.OutFlash,
        InOutFlash = Ease.InOutFlash,

    }

    [Serializable]
    public class AniUnitData
    {
        public int animationId;
        public AnimationType animationType = AnimationType.MoveTo;
        public UIAnimationControlType uIAnimationControlType = UIAnimationControlType.Linear;
        public AnimationCurve animationCurve;
        public UGameObject obj;
        public float animationTime;
        public Vector3 startValue;
        public Vector3 endValue;
        public Color startColor;
        public Color endColor;
        public Gradient gradientColor;
        public float startAlpha;
        public float endAlpha;
        public float delayTime = 0;
        public bool isLoop;
        public LoopType loopType;
    }

    [Serializable]
    public class SequenceAniUnitData
    {
        public int animationId;

    }


    #endregion

    public List<AniUnitData> animationDataConfig = new List<AniUnitData>();
    public List<SequenceAniUnitData> SequenceAniList = new List<SequenceAniUnitData>();
    public Dictionary<int, int> SequenceAniDic = new Dictionary<int, int>();
    private Dictionary<int, AniUnitData> animationDic = new Dictionary<int, AniUnitData>();

    private int endtotalAniCount = 0;
    private int realyAniCount = 0;  // 真是的动作总数: 循环的需要剔除

    private bool isInit = false;
    public bool isAutoPlay = false;
    public int debugAnimationId = 0;

    public float maxAnimationTime = 5;  //动作最大执行时间 单位秒


    private void InitlogicData()
    {
        if (!isInit)
        {
            isInit = true;

            //统计动作总数
            if (animationDataConfig.Count > 0)
            {
                foreach (var item in animationDataConfig)
                {
                    UGameObject uGameObj = item.obj;
                    bool isLoop = item.isLoop;
                    if (uGameObj!= null && !isLoop)
                    {
                        realyAniCount++;

                    }
                }
            }

            if (SequenceAniList.Count > 0)
            {
                foreach (var item in SequenceAniList)
                {
                    if (!SequenceAniDic.ContainsKey(item.animationId))
                    {
                        SequenceAniDic.Add(item.animationId, item.animationId);
                    }
                }
            }

        }

    }
    private void Awake()
    {
        InitlogicData();
        InitAnimMapData();
    }

    private void Start()
    {
        Play();
    }

    private void OnEnable()
    {
        InitlogicData();
        InitAnimMapData();
    }
    private void InitAnimMapData()
    {
        if (animationDic.Count == 0)
        {
            if (animationDataConfig.Count > 0)
            {
                foreach (var item in animationDataConfig)
                {
                    if (!animationDic.ContainsKey(item.animationId))
                    {
                        animationDic.Add(item.animationId, item);
                    }
                    else
                    {
                        Debug.LogError($"InitAnimMapData 配置数据里 动作ID {item.animationId} 有重复，请检查配置信息");
                    }
                }
            }
        }
    }

    public bool CheckDatas(out string msg)
    {
        bool result = true;
        msg = string.Empty;
        var checkDic = new Dictionary<int, bool>();
        if (animationDataConfig.Count > 0)
        {
            foreach (var item in animationDataConfig)
            {
                if (item.obj == null)
                {
                    result = false;
                    msg = $"动作Id为: {item.animationId} 的配置数据里 [目标对象] 为空，请检查配置信息!";
                    break;
                }
                if (!checkDic.ContainsKey(item.animationId))
                {
                    checkDic.Add(item.animationId, true);
                }
                else
                {
                    result = false;
                    msg = $"动作Id {item.animationId} 有重复，请检查配置信息!";
                    break;
                }
            }
        }
        return result;
    }

    private void AnimationEnd(int animId)
    {
        Debug.Log($"animId: {animId} 执行完毕=====");
        endtotalAniCount++;
        if (endtotalAniCount >= realyAniCount)
        {
            Debug.Log($"所有动作执行完毕===== {endtotalAniCount} -- {realyAniCount}");
            endtotalAniCount = 0;
        }

    }

    public void PlayById(int animID)
    {
        if (animID <= 0)
            return;

        foreach (var item in animationDataConfig)
        {
            if (item.animationId == animID)
            {
                this.Play(item);
                break;
            }
        }

    }

    private Tween Play(AniUnitData data)
    {
        Tween twe = null;
        UGameObject uGameObj = data.obj;
        if (uGameObj == null)
        {
            Debug.LogError($"配置里有错误数据 动作ID:{data.animationId}");
            return twe;
        }
        
        AnimationType animationT = data.animationType;
        switch (animationT)
        {
            case AnimationType.MoveTo:
                twe = this._DOMoveTo(data);
                break;
            case AnimationType.MoveBy:
                twe = this._DOMoveBy(data);
                break;
            case AnimationType.ScaleTo:
                twe = this._DOScaleTo(data);
                break;
            case AnimationType.ScaleBy:
                twe = this._DOScaleBy(data);
                break;
            case AnimationType.RotateTo:
                twe = this._DORotateTo(data);
                break;
            case AnimationType.RotateBy:
                twe = this._DORotateBy(data);
                break;
            case AnimationType.Fade:
                twe = this._DOFade(data);
                break;
            case AnimationType.Color:
                twe = this._DOColor(data);
                break;
            case AnimationType.GradientColor:
                twe = this._DoGradientColor(data);
                break;
        }
        return twe;
    }
    public void Play()
    {
        
        if (animationDataConfig.Count > 0)
        {
            foreach (var item in animationDataConfig)
            {
                UGameObject uGameObj = item.obj;
                if (uGameObj == null)
                {
                    Debug.LogError($"配置里有错误数据 动作ID:{item.animationId}");
                    continue;
                }

                //如果在SequenceAniList里就不单独执行
                if(!SequenceAniDic.ContainsKey(item.animationId))
                    this.Play(item);
            }


            //顺序动作
            if (SequenceAniList.Count > 0)
            {
                Sequence seq = DOTween.Sequence();
                foreach (var item in SequenceAniList)
                {
                    int animationId = item.animationId;
                    if (animationDic.ContainsKey(animationId))
                    {
                        AniUnitData aData = animationDic[animationId];
                        var tween = this.Play(aData);
                        seq.Append(tween);
                    }
                }
                seq.Play();
            }
           
        }
    }



    private Tween _DOMoveTo(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DOMoveTo(uGameObj, item.endValue, item.animationTime, () =>{AnimationEnd(item.animationId);}, item.uIAnimationControlType, item.startValue, false, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOMoveBy(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DOMoveBy(uGameObj, item.endValue, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, false, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOScaleTo(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DOScaleTo(uGameObj, item.endValue, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startValue, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOScaleBy(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DOScaleBy(uGameObj, item.endValue, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DORotateTo(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DORotateTo(uGameObj, item.endValue, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startValue, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DORotateBy(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DORotateBy(uGameObj, item.endValue, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOFade(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DOFade(uGameObj, item.endAlpha, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startAlpha, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DOColor(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DOColor(uGameObj, item.endColor, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.startColor, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }

    private Tween _DoGradientColor(AniUnitData item)
    {
        Tween twe = null;
        UGameObject uGameObj = item.obj;
        return this._DOGradientColor(uGameObj, item.gradientColor, item.animationTime, () => { AnimationEnd(item.animationId); }, item.uIAnimationControlType, item.isLoop, item.delayTime, item.animationCurve, item.loopType);
    }


    #region 原子接口

    private Tween _DOMoveTo(GameObject obj, Vector3 endPos, float time , Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear,Vector3 startPos = default(Vector3), bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        
        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            if (startPos != null)
            {
                tran.position = startPos;
            }
            twe = tran.DOMove(endPos, time);
        }
        else
        {
            if (startPos != null)
            {
                tran.localPosition = startPos;
            }
            //局部坐标移动
            twe = tran.DOLocalMove(endPos, time);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if(isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() => {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOMoveBy(GameObject obj, Vector3 offsetPos, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            twe = tran.DOMove(offsetPos, time).SetRelative(true);
        }
        else
        {
            //局部坐标移动
            twe = tran.DOMove(offsetPos, time).SetRelative(true);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOScaleTo(GameObject obj, Vector3 endScale, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startScale = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (startScale != null)
        {
            tran.localScale = startScale;
        }
        twe = tran.DOScale(endScale, time); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOScaleBy(GameObject obj, Vector3 endScale, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        
        twe = tran.DOScale(endScale, time).SetRelative(true); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DORotateTo(GameObject obj, Vector3 endEulerAngles, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startEulerAngles = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
       
        Tween twe = null;
        if (startEulerAngles != null)
        {
            tran.eulerAngles = startEulerAngles;
        }
        twe = tran.DORotate(endEulerAngles, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }
    private Tween _DORotateBy(GameObject obj, Vector3 endEulerAngles, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        twe = tran.DORotate(endEulerAngles, time).SetRelative(true);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }


    private Tween _DOFade(GameObject obj, float endAlpha, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, float startAlpha = 0, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();;
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }


        Tween twe = null;
        if (startAlpha != null)
        {
            canvasGroup.alpha = startAlpha;
        }
        twe = canvasGroup.DOFade(endAlpha, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOColor(GameObject obj, Color endColor, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Color startColor = default(Color), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
            return twe;
        
        if (startColor != null)
        {
            imgCom.color = startColor;
        }
        twe = imgCom.DOColor(endColor, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    private Tween _DOGradientColor(GameObject obj, Gradient gradient, float time, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
            return twe;

        twe = imgCom.DOGradientColor(gradient, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        
        return twe;
    }

    #endregion

    #region 暴露给Lua接口
        public static void Play(Tween tween)
        {
            tween.Play();
        }

        public static void Stop(Tween tween)
        {
            tween.Kill();
        }

        public static void StopAll()
        {
            DOTween.KillAll();
        }

        public static void Pause(Tween tween)
        {
            tween.Pause();
        }
        public static void Resume(Tween tween)
        {
            if (!tween.IsPlaying())
                tween.Play();
        }

        public static void Restart(Tween tween)
        {
            tween.Restart();
        }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endPos">目标位置</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="startPos">起始位置</param>
    /// <param name="isWorld">是否是世界坐标移动</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOMoveTo(GameObject obj, Vector3 endPos, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startPos = default(Vector3), bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            if (startPos != null)
            {
                tran.position = startPos;
            }
            twe = tran.DOMove(endPos, time);
        }
        else
        {
            if (startPos != null)
            {
                tran.localPosition = startPos;
            }
            //局部坐标移动
            twe = tran.DOLocalMove(endPos, time);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endPos">目标位置</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="offsetPos">相对位移</param>
    /// <param name="isWorld">是否是世界坐标移动</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOMoveBy(GameObject obj, Vector3 offsetPos, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isWorld = false, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (isWorld)
        {
            //世界坐标移动
            twe = tran.DOMove(offsetPos, time).SetRelative(true);
        }
        else
        {
            //局部坐标移动
            twe = tran.DOMove(offsetPos, time).SetRelative(true);
        }
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }


    /// <param name="obj"> 目标对象</param>
    /// <param name="endScale">目标大小</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="startScale">起始大小</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOScaleTo(GameObject obj, Vector3 endScale, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startScale = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        if (startScale != null)
        {
            tran.localScale = startScale;
        }
        twe = tran.DOScale(endScale, time); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endScale">目标大小</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DOScaleBy(GameObject obj, Vector3 endScale, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;

        twe = tran.DOScale(endScale, time).SetRelative(true); //修改的就是localScale
        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    /// <param name="obj"> 目标对象</param>
    /// <param name="endEulerAngles">目标欧拉角</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="startEulerAngles">起始欧拉角</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DORotateTo(GameObject obj, Vector3 endEulerAngles, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Vector3 startEulerAngles = default(Vector3), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;

        Tween twe = null;
        if (startEulerAngles != null)
        {
            tran.eulerAngles = startEulerAngles;
        }
        twe = tran.DORotate(endEulerAngles, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    // <param name="obj"> 目标对象</param>
    /// <param name="endEulerAngles">目标欧拉角</param>
    /// <param name="time">动作时间</param>
    /// <param name="endCallBack">结束回调</param>
    /// <param name="controltype">控制类型</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="delayT">延迟时间</param>
    /// <returns></returns>
    public static Tween DORotateBy(GameObject obj, Vector3 endEulerAngles, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {

        Transform tran = obj.transform;
        Tween twe = null;
        twe = tran.DORotate(endEulerAngles, time).SetRelative(true);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }


    public static Tween DOFade(GameObject obj, float endAlpha, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, float startAlpha = 0, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>(); ;
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }


        Tween twe = null;
        if (startAlpha != null)
        {
            canvasGroup.alpha = startAlpha;
        }
        twe = canvasGroup.DOFade(endAlpha, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    public static Tween DOColor(GameObject obj, Color endColor, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, Color startColor = default(Color), bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
            return twe;

        if (startColor != null)
        {
            imgCom.color = startColor;
        }
        twe = imgCom.DOColor(endColor, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    public static Tween DOGradientColor(GameObject obj, Gradient gradient, float time, bool isAutokill = true, Action endCallBack = null, UIAnimationControlType controltype = UIAnimationControlType.Linear, bool isLoop = false, float delayT = 0, AnimationCurve animationCurve = null, LoopType loopType = LoopType.Restart)
    {
        Image imgCom = obj.GetComponent<Image>();
        Tween twe = null;
        if (imgCom == null)
            return twe;

        twe = imgCom.DOGradientColor(gradient, time);

        if (controltype != UIAnimationControlType.AnimationCurve)
        {
            twe.SetEase((Ease)controltype);
        }
        else
        {
            if (animationCurve != null)
            {
                twe.SetEase(animationCurve);
            }
        }

        if (isLoop)
            twe.SetLoops(-1, loopType);
        if (delayT > 0)
        {
            twe.SetDelay(delayT);
        }

        if (endCallBack != null)
        {
            twe.OnComplete(() =>
            {
                endCallBack();
            });
        }
        twe.SetAutoKill(isAutokill);
        return twe;
    }

    #endregion


    #region 退出相关操作
    private void OnDestroy()
    {
    
        animationDataConfig.Clear();
        SequenceAniList.Clear();
        SequenceAniDic.Clear();
        animationDic.Clear();
        animationDataConfig = null;
        SequenceAniList = null;
        SequenceAniDic = null;
        animationDic = null;
    }


    #endregion

}



