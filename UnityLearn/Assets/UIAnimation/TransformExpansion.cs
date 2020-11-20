using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Universal.UIAnimation
{
public static  class TransformExpansion
{
        public static UIAnimation GetUIAnimation(this Transform tagert)
        {
            UIAnimation uiAnimation = tagert.GetComponent<UIAnimation>();
            if (uiAnimation == null)
            {
                tagert.gameObject.AddComponent<UIAnimation>().InitAnimation(tagert);
            }
            return tagert.GetComponent<UIAnimation>();
        }

        public static Transform HideUIAnimation(this Transform tagert)
        {
            GetUIAnimation(tagert).Hide();
            return tagert;
        }

        public static Transform PlayUIAnimation(this Transform tagert, UIAnimationEnum playState)
        {
            GetUIAnimation(tagert).PlayAnimation(playState);
            return tagert;
        }


        public static Transform PlayUIAnimation(this Transform tagert, UIAnimationEnum playState, UIAnimationDirEnum derType)
        {
            GetUIAnimation(tagert).PlayAnimation(playState, derType);
                return tagert;
        }

        public static Transform PlayUIAnimation(this Transform tagert, UIAnimationEnum playState,UIAnimationDirEnum der , float animationTime = 0.42f,bool hasMask = true)
        {
            GetUIAnimation(tagert).PlayAnimation(playState, animationTime, der, hasMask);
            return tagert;
        }
        #region 常用属性
        public static Transform SetUIDelay(this Transform tagert, float time)
        {
            GetUIAnimation(tagert).SetDelay(time);
            return tagert;
        }

        public static Transform OnUIStart(this Transform tagert, Action endCallback = null)
        {
            GetUIAnimation(tagert).OnStart(endCallback);
            return tagert;
        }

        public static Transform OnUIUpdate(this Transform tagert, Action<float> updateCallback = null)
        {
            GetUIAnimation(tagert).OnUpdate(updateCallback);
            return tagert;
        }

        public static Transform OnUIComplete(this Transform tagert, Action startCallback = null)
        {
            GetUIAnimation(tagert).OnComplete(startCallback);
            return tagert;
        }

        public static Transform SetUIEase(this Transform tagert, DG.Tweening.Ease ease)
        {
            GetUIAnimation(tagert).SetEase(ease);
            return tagert;
        }

        public static Transform SetUIEase(this Transform tagert, AnimationCurve curve)
        {
            GetUIAnimation(tagert).SetCurve(curve);
            return tagert;
        }

        public static Transform SetAnimationAttribute(this Transform tagert, ChangeEnum type, float startValue, float overValue, float animationTime = 0.42f)
        {
            GetUIAnimation(tagert).SetAnimationAttribute(type, startValue, overValue, animationTime);
            return tagert;
        }
        #endregion
        #region 遮罩相关
        public static Transform SetMaskEnable(this Transform tagert, bool hasMask = true)
        {
            GetUIAnimation(tagert).SetMaskEnable(hasMask);
            return tagert;
        }

         public static Transform SetMaskSprite(this Transform tagert, Sprite sprite)
        {
            GetUIAnimation(tagert).SetMaskSprite(sprite);
            return tagert;
        }

        public static Transform SetMaskColor(this Transform tagert, Color color)
        {
            GetUIAnimation(tagert).SetMaskColor(color);
            return tagert;
        }

        public static Transform SetMaskClickCallBack(this Transform tagert, Action callBack)
        {
            GetUIAnimation(tagert).SetMaskCallBack(callBack);
            return tagert;
        }

        public static Transform SetMaskAlphaIntervalValue(this Transform tagert, float startValue, float endValue)
        {
            GetUIAnimation(tagert).SetMaskAlphaIntervalValue(startValue, endValue);
            return tagert;
        }

        public static Transform SetMaskAnimationTime(this Transform tagert, float animationTime)
        {
            GetUIAnimation(tagert).SetMaskAnimationTime(animationTime);
            return tagert;
        }
        #endregion
        #region  缩放属性设置

        public static Transform SetScaleIntervalValue(this Transform tagert, float startValue, float overValue)
        {
            GetUIAnimation(tagert).SetScaleIntervalValue(startValue, overValue);
            return tagert;
        }

        public static Transform SetScaleAnimtionTime(this Transform tagert, float animationTime)
        {
            GetUIAnimation(tagert).SetScaleAnimtionTime(animationTime);
            return tagert;
        }
        #endregion

        #region 旋转属性设置
        public static Transform SetRotationIntervalValue(this Transform tagert, float startValue, float overValue)
        {
            GetUIAnimation(tagert).SetRotationIntervalValue(startValue, overValue);
            return tagert;
        }

        public static Transform SetRotationAnimtionTime(this Transform tagert, float animationTime)
        {
            GetUIAnimation(tagert).SetRotationAnimtionTime(animationTime);
            return tagert;
        }
        #endregion

        #region 透明度属性设置
        public static Transform SetAlphaIntervalValue(this Transform tagert, float startValue, float overValue)
        {
            GetUIAnimation(tagert).SetAlphaIntervalValue(startValue, overValue);
            return tagert;
        }

        public static Transform SetAlphaAnimtionTime(this Transform tagert, float animationTime)
        {
            GetUIAnimation(tagert).SetAlphaAnimtionTime(animationTime);
            return tagert;
        }
        #endregion
    }
}
