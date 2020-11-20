using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimationDefine : ScriptableObject
{
    public float INIT_MASK_MAX_ALPHA = 0.25F;
    public float INIT_ANIMATION_TIME = 0.25F;
    public bool hasMask = false;
    public float maskMaxAlpha;
    public Color maskColor;
    public Sprite maskSprite;
    public AnimationCurve useCurve;
    public Ease useEase = Ease.Linear;
}
