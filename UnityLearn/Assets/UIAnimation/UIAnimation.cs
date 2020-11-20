using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Universal.UIAnimation
{
    public enum UIAnimationDirEnum
    {
        Center,
        Up,
        Down,
        Left,
        Right,
        PlayerSet
    }

    public enum UIAnimationEnum
    {
        Appear,
        Hide,
    }

    public enum UIAnimationType
    {
        TweeningEase,
        AnimationCurve,
    }

    public class UIAnimation : MonoBehaviour
    {
        /// <summary>
        /// 配置类
        /// </summary>
        static UIAnimationDefine UDE;
        public static void InitUDE()
        {
            if (UDE == null)
            {
                UDE = Resources.Load<UIAnimationDefine>("UIAnimationDefine");
            }
        }


        public float INIT_MASK_MAX_ALPHA = UDE.INIT_MASK_MAX_ALPHA;
        public float INIT_ANIMATION_TIME = UDE.INIT_ANIMATION_TIME;

        private event Action Event_MaskDown;
        private event Action Event_Start;
        private event Action Event_Over;
        private event Action<float> Event_Update;

        /// <summary>
        /// UI动画作用的物体
        /// </summary>
        private GameObject targetObject;
        #region 遮罩相关
        /// <summary>
        /// UI动画作用物体的遮罩
        /// </summary>
        private GameObject maskObject;
        /// <summary>
        /// 是否存在遮罩
        /// </summary>
        private bool hasMask = UDE.hasMask;
        /// <summary>
        /// 遮罩的深度
        /// </summary>
        private float maskMaxAlpha = UDE.maskMaxAlpha;
        /// <summary>
        /// 遮罩的颜色
        /// </summary>
        private Color maskColor = UDE.maskColor;
        /// <summary>
        /// 遮罩的图片
        /// </summary>
        private Sprite maskSprite = UDE.maskSprite;
        #endregion
        #region 动画曲线
        /// <summary>
        /// Dotween曲线
        /// </summary>
        private UIAnimationType animationType = UIAnimationType.TweeningEase;
        /// <summary>
        /// 使用的曲线
        /// </summary>
        private AnimationCurve useCurve= UDE.useCurve;
        /// <summary>
        /// 使用的动画类型
        /// </summary>
        private Ease useEase = UDE.useEase;
        #endregion
        /// <summary>
        /// 动画的时间
        /// </summary>
        private float animationTime;
        #region 动画播放控制
        /// <summary>
        /// 等待的时间
        /// </summary>
        private float delayTime;
        /// <summary>
        /// 动画计时
        /// </summary>
        private float timer;
        /// <summary>
        /// 播放控制
        /// </summary>
        private bool startPlayAnimation = false;
        #endregion
        /// <summary>
        /// 当前的执行的判断行为的Yweener
        /// </summary
        private Tweener currentTweener;
        /// <summary>
        /// 当前的执行的UITween
        /// </summary>
        private Tweener currentScaleTweener;
        /// <summary>
        /// 当前的遮罩执行的tween
        /// </summary>
        private Tweener currentMaksTweener;
        /// <summary>
        /// 当前Group
        /// </summary>
        private Tweener groupTweener;
        /// <summary>
        /// 当前的执行的移动动画
        /// </summary>
        private Tweener currentMoveTweener;
        /// <summary>
        /// 当前的执行的UITween
        /// </summary>
        private Tweener currentRotationTweener;
        /// <summary>
        /// 当前动画处于的状态
        /// </summary>
        private UIAnimationEnum currentState;
        /// <summary>
        /// 当前动画的方向
        /// </summary>
        private UIAnimationDirEnum derType;
        private Dictionary<ChangeEnum, bool> animationSwtichDic = new Dictionary<ChangeEnum, bool>();
        private Dictionary<ChangeEnum, AnimationData> animationDic = new Dictionary<ChangeEnum, AnimationData>();
        private List<AnimationData> animationList = new List<AnimationData>();
        public bool HasMask
        {
            get
            {
                return hasMask;
            }

            set
            {
                hasMask = value;
                if (hasMask && maskObject == null)
                {
                    #region 生成遮罩
                    int index = targetObject.transform.GetSiblingIndex();
                    GameObject _mask = new GameObject();
                    // 定义对象名为 Mask  
                    _mask.name = "Mask";
                    // 添加Image  
                    _mask.AddComponent<UnityEngine.UI.Image>();
                    _mask.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2000, 2000);
                    _mask.transform.SetParent(targetObject.transform.parent);
                    _mask.transform.localPosition = Vector3.zero;
                    _mask.transform.localScale = Vector3.one;
                    _mask.transform.SetSiblingIndex(index);
                    _mask.transform.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
                    maskObject = _mask;
                    EventTriggerListener sendPicMask = EventTriggerListener.Get(maskObject.gameObject);
                    sendPicMask.onDown = MaskDown;
                    maskObject.gameObject.SetActive(false);
                    #endregion
                }
            }
        }

        void Update()
        {
            if (startPlayAnimation)
            {
                timer += Time.deltaTime;
                if (timer >= delayTime)
                {
                    startPlayAnimation = false;
                    Play(currentState);
                    timer = 0;
                }
            }
            if (currentTweener != null)
            {
                if (Event_Update != null)
                {
                    if (currentTweener.Elapsed(false) / currentTweener.Duration(false) < 0.99f)
                    {
                        Event_Update(currentTweener.Elapsed(false) / currentTweener.Duration(false));
                    }
                }
            }
        }

        public void InitAnimation(Transform targetObject)
        {
            this.targetObject = targetObject.gameObject;
            HasMask = UDE.hasMask;
            animationTime = INIT_ANIMATION_TIME;
            maskColor = new Color(0, 0, 0, 0);
            maskMaxAlpha = INIT_MASK_MAX_ALPHA;
            animationSwtichDic.Add(ChangeEnum.Alpha, true);
            animationSwtichDic.Add(ChangeEnum.MaskAlpha, true);
            animationSwtichDic.Add(ChangeEnum.Rotation, false);
            animationSwtichDic.Add(ChangeEnum.Scale, true);

            animationList.Add(new AnimationData(animationTime, 0, 1));
            animationList.Add(new AnimationData(animationTime, 0, 1));
            animationList.Add(new AnimationData(animationTime, 0, 360));
            animationList.Add(new AnimationData(animationTime, 0, 1));
        }


        public void Hide()
        {
            currentState = UIAnimationEnum.Hide;
            gameObject.SetActive(false);
            if (maskObject != null)
            {
                maskObject.gameObject.SetActive(false);
            }
        }

        public void PlayAnimation(UIAnimationEnum playState)
        {
            InitBeforePlay(playState, UIAnimationDirEnum.Center ,animationTime, HasMask);

        }

        public void PlayAnimation(UIAnimationEnum playState, UIAnimationDirEnum der)
        {
            InitBeforePlay(playState, der, animationTime, HasMask);
        }

        public void PlayAnimation(UIAnimationEnum playState, float animationTime , UIAnimationDirEnum der = UIAnimationDirEnum.Center, bool hasMask = true)
        {
            if (animationTime != 0)
            {
                InitBeforePlay(playState, der, animationTime, hasMask);
                if (maskObject != null)
                {
                    maskObject.gameObject.SetActive(false);
                }
            }
            else
            {
                gameObject.SetActive(false);
                if (maskObject != null)
                {
                    maskObject.gameObject.SetActive(false);
                }
            }
        }

        void InitBeforePlay(UIAnimationEnum playState, UIAnimationDirEnum der,
        float animationTime , bool hasMask )
        {
            HasMask = hasMask;
            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }
            this.animationTime = animationTime;
            if (der != UIAnimationDirEnum.Center)
            {
                transform.localScale = Vector3.one;
            }
            derType = der;
            if (currentState == playState && playState == UIAnimationEnum.Hide)
            {
                return;
            }
            currentState = playState;
            startPlayAnimation = true;
            delayTime = 0.01f;
            timer = 0;
            if (currentState == UIAnimationEnum.Appear)
            {
                //默认动画配置设置
                animationType = UIAnimationType.AnimationCurve;
                useCurve = UDE.useCurve;
                gameObject.SetActive(true);
                transform.localScale = Vector3.one * 0.1f;
                GetComponent<CanvasGroup>().alpha = 0;
                maskColor = new Color(25 / 255f, 32 / 255f, 43 / 255f, 0);
                SetMaskAlphaIntervalValue(0, 0.5f);
            }
            else
            {
                HasMask = false;
            }
            InitTweener();
            ClearAction();
        }

        #region 动画逻辑
        public void Play(UIAnimationEnum playState)
        {
            InitTweener();
            Init();
            if (derType == UIAnimationDirEnum.Center || derType ==UIAnimationDirEnum.PlayerSet)
            {
                //缩放动画
                UItargetScaleAnimation(playState);
                currentTweener = currentScaleTweener;
            }
            else
            {
                #region 定制缩放
                if (animationSwtichDic[ChangeEnum.Scale])
                {
                    UItargetScaleAnimation(playState);
                    animationSwtichDic[ChangeEnum.Scale] = false;
                }
                #endregion
                PlayTweener(playState);
                currentTweener = currentMoveTweener;
            }
            TweenerAction(currentTweener, playState);
            if (animationSwtichDic[ChangeEnum.Rotation])
            {
                UITargetRotationAnimation(playState);
                animationSwtichDic[ChangeEnum.Rotation] = false;
            }
            //透明度动画
            if (animationSwtichDic[ChangeEnum.Alpha])
            {
                UIGroupAnimation(playState);
            }
            #region 遮罩动画
            if (animationSwtichDic[ChangeEnum.MaskAlpha])
            {
                UIMaskAnimation(playState);
            }
            #endregion
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        /// <param name="targetGame"></param>
        /// <param name="State"></param>
        public void UItargetScaleAnimation(UIAnimationEnum state)
        {

            #region 加入玩家定制动画
            float animationTime = this.animationTime;
            float targetValue = 0;
            float startValue = 0;
            if (state == UIAnimationEnum.Hide)
            {
                targetValue = 0;
                startValue = 0;
            }
            else if (state == UIAnimationEnum.Appear)
            {
                targetValue = 1;
                startValue = 0.4f;
            }

            if (animationDic.ContainsKey(ChangeEnum.Scale))
            {
                animationTime = animationDic[ChangeEnum.Scale].animationTime;
                targetValue = animationDic[ChangeEnum.Scale].overValue;
                startValue = animationDic[ChangeEnum.Scale].startValue;
                animationDic.Remove(ChangeEnum.Scale);
            }
            #endregion

            if (state == UIAnimationEnum.Hide)
            {
                if (startValue != 1)
                {
                    #region UI缩放动画
                    currentScaleTweener = transform.DOScale(Vector3.one * 0.2f, animationTime).OnKill(() =>
                    {
                        currentScaleTweener = null;
                    });
                    #endregion
                }
                else
                {
                    #region UI缩放动画
                    currentScaleTweener = transform.DOScale(startValue, animationTime).OnKill(() =>
                    {
                        currentScaleTweener = null;
                    });
                    #endregion
                }
            }
            else if (state == UIAnimationEnum.Appear)
            {
                gameObject.SetActive(true);
                transform.localScale = Vector3.one * startValue;
                currentScaleTweener = transform.DOScale(Vector3.one * targetValue, animationTime).OnKill(() =>
                 {
                     currentScaleTweener = null;
                 });
            }
        }

        /// <summary>
        /// 遮罩动画
        /// </summary>
        /// <param name="targetGame"></param>
        /// <param name="State"></param>
        public void UIMaskAnimation(UIAnimationEnum state)
        {
            #region 加入玩家定制动画
            float animationTime = this.animationTime;
            float targetValue = 0;
            // float startValue = 0;
            if (state == UIAnimationEnum.Hide)
            {
                targetValue = 0;
                // startValue = 0;

            }
            else if (state == UIAnimationEnum.Appear)
            {
                targetValue = maskMaxAlpha;
                // startValue = 0;
            }

            if (animationDic.ContainsKey(ChangeEnum.MaskAlpha))
            {
                animationTime = animationDic[ChangeEnum.MaskAlpha].animationTime;
                targetValue = animationDic[ChangeEnum.MaskAlpha].overValue;
                // startValue = animationDic[ChangeEnum.MaskAlpha].startValue;
                animationDic.Remove(ChangeEnum.MaskAlpha);
            }
            #endregion

            if (state == UIAnimationEnum.Hide)
            {
                if (HasMask)
                {
                    maskObject.gameObject.SetActive(true);
                    maskObject.GetComponent<UnityEngine.UI.Image>().sprite = maskSprite;
                    currentMaksTweener = maskObject.GetComponent<UnityEngine.UI.Image>().DOColor(new Color(maskColor.r, maskColor.g, maskColor.b, targetValue), animationTime).OnComplete(() =>
                    {
                        maskObject.gameObject.SetActive(false);
                    });
                }
                else
                {
                    maskObject.gameObject.SetActive(false);
                }
            }
            else if (state == UIAnimationEnum.Appear)
            {
                if (HasMask)
                {
                    maskObject.gameObject.SetActive(true);
                    maskObject.GetComponent<UnityEngine.UI.Image>().sprite = maskSprite;
                    currentMaksTweener = maskObject.GetComponent<UnityEngine.UI.Image>().DOColor(new Color(maskColor.r, maskColor.g, maskColor.b, targetValue), animationTime);
                }
                else
                {
                    maskObject.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 透明度动画
        /// </summary>
        /// <param name="targetGame"></param>
        /// <param name="State"></param>
        /// <returns></returns>
        public Tweener UIGroupAnimation(UIAnimationEnum state)
        {
            groupTweener = null;
            #region 加入玩家定制动画
            float animationTime = this.animationTime;
            float targetValue = 0;
            float startValue = 0;
            if (state == UIAnimationEnum.Hide)
            {
                targetValue = 0;
                startValue = 0;
            }
            else if (state == UIAnimationEnum.Appear)
            {
                targetValue = 1;
                startValue = 0;
            }

            if (animationDic.ContainsKey(ChangeEnum.Alpha))
            {
                animationTime = animationDic[ChangeEnum.Alpha].animationTime;
                targetValue = animationDic[ChangeEnum.Alpha].overValue;
                startValue = animationDic[ChangeEnum.Alpha].startValue;
                animationDic.Remove(ChangeEnum.Alpha);
            }
            #endregion

            if (state == UIAnimationEnum.Hide)
            {
                groupTweener = DOTween.To(x => GetComponent<CanvasGroup>().alpha = x, GetComponent<CanvasGroup>().alpha, targetValue, 0.15f);
                return groupTweener;
            }
            else if (state == UIAnimationEnum.Appear)
            {
                GetComponent<CanvasGroup>().alpha = 0;
                groupTweener = DOTween.To(x => GetComponent<CanvasGroup>().alpha = x, startValue, targetValue, animationTime);
                return groupTweener;
            }
            return groupTweener;
        }


        /// <summary>
        /// 缩放动画
        /// </summary>
        /// <param name="targetGame"></param>
        /// <param name="State"></param>
        public void UITargetRotationAnimation(UIAnimationEnum state)
        {

            #region 加入玩家定制动画
            float animationTime = this.animationTime;
            float targetValue = 0;
            float startValue = 0;
            if (state == UIAnimationEnum.Hide)
            {
                targetValue = 0;
                startValue = 0;
            }
            else if (state == UIAnimationEnum.Appear)
            {
                targetValue = maskMaxAlpha;
                startValue = 0.4f;
            }

            if (animationDic.ContainsKey(ChangeEnum.Rotation))
            {
                animationTime = animationDic[ChangeEnum.Rotation].animationTime;
                targetValue = animationDic[ChangeEnum.Rotation].overValue;
                startValue = animationDic[ChangeEnum.Rotation].startValue;
                animationDic.Remove(ChangeEnum.Rotation);
                #endregion

                if (state == UIAnimationEnum.Hide)
                {
                    Debug.Log(startValue + " + " + targetValue);
                    #region UI缩放动画
                    currentRotationTweener = transform.DOLocalRotate(new Vector3(0, 0, targetValue), animationTime).OnKill(() =>
                     {
                         currentRotationTweener = null;
                     });
                    #endregion
                }
                else if (state == UIAnimationEnum.Appear)
                {
                    gameObject.SetActive(true);
                    transform.localRotation = Quaternion.Euler(new Vector3(0, 0, startValue));
                    currentRotationTweener = transform.DOLocalRotate(new Vector3(0, 0, targetValue), animationTime).OnKill(() =>
                     {
                         currentRotationTweener = null;
                     });
                }
            }
        }

        /// <summary>
        /// 清除所有Tweener操作
        /// </summary>
        void InitTweener()
        {
            if (currentTweener != null)
            {
                currentTweener.Kill();
                currentTweener = null;
            }
            if (currentMoveTweener != null)
            {
                currentMoveTweener.Kill();
                currentMoveTweener = null;
            }
            if (currentScaleTweener != null)
            {
                currentScaleTweener.Kill(false);
                currentScaleTweener = null;
            }
            if (currentMaksTweener != null)
            {
                currentMaksTweener.Kill();
                currentMaksTweener = null;
            }
            if (groupTweener != null)
            {
                groupTweener.Kill();
                groupTweener = null;
            }
            if (currentRotationTweener != null)
            {
                currentRotationTweener.Kill();
                currentRotationTweener = null;
            }
        }

        /// <summary>
        /// 初始化位置
        /// </summary>
        void Init()
        {
            if (currentState == UIAnimationEnum.Appear)
            {
                transform.GetComponent<RectTransform>().localRotation = Quaternion.Euler(Vector3.zero);
                Vector2 position = transform.GetComponent<RectTransform>().anchoredPosition;
                switch (derType)
                {
                    case UIAnimationDirEnum.Center:
                        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                        break;
                    case UIAnimationDirEnum.Down:
                        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -Screen.height + 100);
                        break;
                    case UIAnimationDirEnum.Up:
                        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, Screen.height + 100);
                        break;
                    case UIAnimationDirEnum.Left:
                        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.width + 100, 0);
                        break;
                    case UIAnimationDirEnum.Right:
                        transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width + 100, 0);
                        break;
                }
            }
        }

        public void PlayTweener(UIAnimationEnum playState)
        {
            transform.localScale = Vector3.one;
            switch (derType)
            {
                case UIAnimationDirEnum.Down:
                    {
                        if (playState == UIAnimationEnum.Appear)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosY(0, animationTime);
                        }
                        else if (playState == UIAnimationEnum.Hide)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosY(-Screen.height - 100, animationTime);
                        }
                    }
                    break;
                case UIAnimationDirEnum.Up:
                    {
                        if (playState == UIAnimationEnum.Appear)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosY(0, animationTime);
                        }
                        else if (playState == UIAnimationEnum.Hide)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosY(Screen.height + 100, animationTime);
                        }
                    }
                    break;
                case UIAnimationDirEnum.Left:
                    {
                        if (playState == UIAnimationEnum.Appear)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosX(0, animationTime);
                        }
                        else if (playState == UIAnimationEnum.Hide)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosX(-Screen.width - 100, animationTime);
                        }
                    }
                    break;
                case UIAnimationDirEnum.Right:
                    {
                        if (playState == UIAnimationEnum.Appear)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosX(0, animationTime);
                        }
                        else if (playState == UIAnimationEnum.Hide)
                        {
                            currentMoveTweener = transform.GetComponent<RectTransform>().DOAnchorPosX(Screen.width + 100, animationTime);
                        }
                    }
                    break;
            }
            currentMoveTweener.OnKill(() =>
            {
                currentMoveTweener = null;
            });
        }

        /// <summary>
        /// 动画的行为反馈
        /// </summary>
        /// <param name="tweener"></param>
        /// <param name="state"></param>
        void TweenerAction(Tweener tweener, UIAnimationEnum state)
        {
            if (animationType == UIAnimationType.AnimationCurve)
            {
                tweener.SetEase(useCurve);
            }
            else if (animationType == UIAnimationType.TweeningEase)
            {
                tweener.SetEase(useEase);
            }
            tweener.OnComplete(() =>
            {
                if (Event_Over != null)
                {
                    Event_Over();
                }
                if (state == UIAnimationEnum.Hide)
                {
                    gameObject.SetActive(false);
                }
                else if (state == UIAnimationEnum.Appear)
                {
                    transform.localScale = Vector3.one;
                    GetComponent<CanvasGroup>().alpha = 1;
                }
                tweener = null;
            }).OnStart(() =>
            {
                if (Event_Start != null)
                {
                    Event_Start();
                }
            }).OnKill(()=>
            {
                Debug.Log("Dea");
            });
        }
        /// <summary>
        /// 清除所有事件
        /// </summary>
        void ClearAction()
        {
            if (Event_Start != null)
            {
                Delegate[] ar = Event_Start.GetInvocationList();
                for (int i = 0; i < ar.Length; i++)
                {
                    Event_Start -= ar[i] as Action;
                }
            }
            if (Event_Over != null)
            {
                Delegate[] ar = Event_Over.GetInvocationList();
                for (int i = 0; i < ar.Length; i++)
                {
                    Event_Over -= ar[i] as Action;
                }
            }
            if (Event_Update != null)
            {
                Delegate[] ar = Event_Update.GetInvocationList();
                for (int i = 0; i < ar.Length; i++)
                {
                    Event_Update -= ar[i] as Action<float>;
                }
            }
        }
        #endregion

        #region 行为
        public void OnComplete(Action over)
        {
            Event_Over = over;
        }

        public void OnStart(Action start)
        {
            Event_Start = start;
        }

        public void OnUpdate(Action<float> update)
        {
            Event_Update = update;
        }
        #endregion

        #region 属性设置

        public void SetEase(Ease ease)
        {
            animationType = UIAnimationType.TweeningEase;
            useEase = ease;
        }

        public void SetCurve(AnimationCurve ease)
        {
            animationType = UIAnimationType.AnimationCurve;
            useCurve = ease;
        }

        public void SetDelay(float time = 0.01f)
        {
            if (time < 0.01f)
            {
                time = 0.01f;
            }
            delayTime = time;
            timer = 0;
        }

        /// <summary>
        /// 设置通用时间
        /// </summary>
        /// <param name="animationTime"></param>
        public void SetCurrencyTime(float animationTime = 0.45f)
        {
            this.animationTime = animationTime;
        }

        #region 遮罩属性设置

        private void MaskDown(GameObject go)
        {
            if (Event_MaskDown != null)
            {
                Event_MaskDown();
            }
        }
        /// <summary>
        /// 遮罩开关
        /// </summary>
        /// <param name="hasMask"></param>
        public void SetMaskCallBack(Action callBack)
        {
            HasMask = true;
            Event_MaskDown=callBack;
        }


        /// <summary>
        /// 遮罩开关
        /// </summary>
        /// <param name="hasMask"></param>
        public void SetMaskEnable(bool hasMask = true)
        {
            animationSwtichDic[ChangeEnum.MaskAlpha] = hasMask;
            HasMask = hasMask;
            if (maskObject != null && HasMask == false)
            {
                maskObject.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 设置遮罩的图片
        /// </summary>
        /// <param name="sprite"></param>
        public void SetMaskSprite(Sprite sprite)
        {
            HasMask = true;
            maskSprite = sprite;
        }

        /// <summary>
        /// 设置遮罩的颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetMaskColor(Color color)
        {
            HasMask = true;
            maskColor = color;
        }

        /// <summary>
        /// 设置遮罩透明度的结束值
        /// 如果你在Hide时StartValue是无意义的，它会适应当前的属性继续变化
        /// </summary>
        /// <param name="endValue"></param>
        public void SetMaskAlphaIntervalValue(float startValue, float overValue)
        {
            HasMask = true;
            AnimationData data = GetData(ChangeEnum.MaskAlpha);
            data.startValue = startValue;
            data.overValue = overValue;
        }

        /// <summary>
        /// 设置遮罩的动画时间
        /// </summary>
        /// <param name="animationTime"></param>
        public void SetMaskAnimationTime(float animationTime)
        {
            HasMask = true;
            AnimationData data = GetData(ChangeEnum.MaskAlpha);
            data.animationTime = animationTime;
        }
        #endregion

        #region 缩放属性设置
        /// <summary>
        /// 设置缩放的开始值
        /// 如果你在Hide时StartValue是无意义的，它会适应当前的属性继续变化
        /// </summary>
        /// <param name="hasScale"></param>
        public void SetScaleIntervalValue(float startValue, float overValue)
        {
            animationSwtichDic[ChangeEnum.Scale] = true;
            AnimationData data = GetData(ChangeEnum.Scale);
            data.startValue = startValue;
            data.overValue = overValue;
        }

        /// <summary>
        /// 设置缩放的结束值
        /// </summary>
        /// <param name="hasScale"></param>
        public void SetScaleAnimtionTime(float animationTime)
        {
            animationSwtichDic[ChangeEnum.Scale] = true;
            AnimationData data = GetData(ChangeEnum.Scale);
            data.animationTime = animationTime;
        }

        #endregion

        #region 旋转属性设置

        /// <summary>
        /// 设置旋转的开始值
        /// 如果你在Hide时StartValue是无意义的，它会适应当前的属性继续变化
        /// </summary>
        /// <param name="hasScale"></param>
        public void SetRotationIntervalValue(float startValue, float overValue)
        {
            animationSwtichDic[ChangeEnum.Rotation] = true;
            AnimationData data = GetData(ChangeEnum.Rotation);
            data.startValue = startValue;
            data.overValue = overValue;
        }

        /// <summary>
        /// 设置旋转的时间
        /// </summary>
        /// <param name="hasScale"></param>
        public void SetRotationAnimtionTime(float animationTime)
        {
            animationSwtichDic[ChangeEnum.Rotation] = true;
            AnimationData data = GetData(ChangeEnum.Rotation);
            data.animationTime = animationTime;
        }
        #endregion

        #region 透明度属性设置
        /// <summary>
        /// 设置透明度的开始值
        /// 如果你在Hide时StartValue是无意义的，它会适应当前的属性继续变化
        /// </summary>
        /// <param name="hasScale"></param>
        public void SetAlphaIntervalValue(float startValue, float overValue)
        {
            animationSwtichDic[ChangeEnum.Alpha] = true;
            AnimationData data = GetData(ChangeEnum.Alpha);
            data.startValue = startValue;
            data.overValue = overValue;
        }

        /// <summary>
        /// 设置透明度的结束值
        /// </summary>
        /// <param name="hasScale"></param>
        public void SetAlphaAnimtionTime(float animationTime)
        {
            animationSwtichDic[ChangeEnum.Alpha] = true;
            AnimationData data = GetData(ChangeEnum.Alpha);
            data.animationTime = animationTime;
        }
        #endregion

        /// <summary>
        /// 定制属性
        /// </summary>
        /// <param name="startValue">如果你在Hide时StartValue是无意义的，它会适应当前的属性继续变化</param>
        /// <param name="endValue"></param>
        /// <param name="animationTime"></param>
        public void SetAnimationAttribute(ChangeEnum type, float startValue, float overValue, float animationTime = 0.42f)
        {
            animationSwtichDic[type] = true;
            AnimationData data = GetData(type);
            data.startValue = startValue;
            data.overValue = overValue;
            data.animationTime = animationTime;
        }

        private AnimationData GetData(ChangeEnum type)
        {
            if (animationDic.ContainsKey(type) == false)
            {
                animationDic.Add(type, animationList[(int)type]);
            }
            return animationDic[type];
        }
        #endregion
    }

    public enum ChangeEnum
    {
        MaskAlpha,
        Scale,
        Rotation,
        Alpha,
    }

    public class AnimationData
    {
        public bool enable;
        public float animationTime;
        public float startValue;
        public float overValue;
        public AnimationData(float animationTime, float startValue, float overValue)
        {
            this.animationTime = animationTime;
            this.startValue = startValue;
            this.overValue = overValue;
        }
    }
}
