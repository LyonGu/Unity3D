using UnityEngine.UI;
using UnityEngine;
using Universal.UIAnimation;
using System.Collections.Generic;
using System;

public class UIAnimationTest : MonoBehaviour {
    public GameObject tagertPanel;
    public AnimationCurve curve;
    public Button appearButton;
    public Button hideButton;
    public InputField delayInputField;
    public InputField animationTimeInputField;
    public Image maskColorAndAlpha;
    public Dropdown easeDropdown;
    public Dropdown easeDerDropdown;
    public Toggle easeToggle;
    public Toggle curveToggle;
    public Sprite maskSprite;
    List<string> enumList;
    List<string> derList;

    void Start () {

        UIAnimation.InitUDE();
        enumList = new List<string>();
        derList = new List<string>();
        foreach (DG.Tweening.Ease item in Enum.GetValues(typeof(DG.Tweening.Ease)))
        {
            enumList.Add(item.ToString());
        }
        foreach (UIAnimationDirEnum item in Enum.GetValues(typeof(UIAnimationDirEnum)))
        {
            derList.Add(item.ToString());
        }
        easeDropdown.ClearOptions();
        easeDropdown.AddOptions(enumList);
        easeDerDropdown.ClearOptions();
        easeDerDropdown.AddOptions(derList);
        appearButton.onClick.AddListener(OnAppearButtonClick);
        hideButton.onClick.AddListener(OnHideButtonClick);
    }

    void Update()
    {

    }

    public void OnAppearButtonClick()
    {
        float animationTime = 0f;
        float delayResult=0f;
        Color MaskColor = maskColorAndAlpha.color;
        if (float.TryParse(delayInputField.text, out delayResult) ==false)
        {
            delayResult = 0;
        }

        if (float.TryParse(animationTimeInputField.text, out animationTime) == false)
        {
            animationTime = 0;
        }

        UIAnimationDirEnum derType = (UIAnimationDirEnum)Enum.Parse(typeof(UIAnimationDirEnum), derList[easeDerDropdown.value]);

        //调用动画
        tagertPanel.transform.PlayUIAnimation(UIAnimationEnum.Appear, derType).OnUIUpdate((o) =>
        {
          Debug.Log("Update:" + o);
        }).SetUIDelay(delayResult).SetMaskColor(MaskColor).SetMaskAlphaIntervalValue(0, MaskColor.a);

        if (easeToggle.isOn)
        {
            DG.Tweening.Ease tagert = (DG.Tweening.Ease)Enum.Parse(typeof(DG.Tweening.Ease), enumList[easeDropdown.value]);
            tagertPanel.transform.SetUIEase(tagert);
        }
        if (curveToggle.isOn)
        {
            tagertPanel.transform.SetUIEase(curve);
        }
        //tagertPanel.transform.SetMaskSprite(maskSprite);
        //tagertPanel.transform.SetAnimationAttribute(ChangeEnum.Rotation, 180, 360, 0.42f);
        //tagertPanel.transform.SetAnimationAttribute(ChangeEnum.Scale, 0.5f, 1.1f, 0.42f);
    }

    public void OnHideButtonClick()
    {
        UIAnimationDirEnum derType = (UIAnimationDirEnum)Enum.Parse(typeof(UIAnimationDirEnum), derList[easeDerDropdown.value]);
        //调用动画
        tagertPanel.transform.PlayUIAnimation(UIAnimationEnum.Hide, derType);
        tagertPanel.transform.OnUIStart(() =>
        {
            Debug.Log("ToStart");
        });
        tagertPanel.transform.OnUIComplete(() =>
        {
            Debug.Log("OnComplete");
        });
    }
}
