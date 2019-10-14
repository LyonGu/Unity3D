using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public enum DOFADETYPE
{
    None = 0,
    CanvasGroup = 1,
    Graphic = 2,
    Image = 3,
    Outline = 4,
    Text = 5,

};
public class DoTweenUtils
{
    private static DoTweenUtils _instance;

    public static DoTweenUtils  GetInstance()
    {
        if(_instance == null)
        {
            _instance = new DoTweenUtils();
        }
        return _instance;
    }
    private DoTweenUtils()
    { 

    }

    ~DoTweenUtils()
    { 
    
    }

    /****************************************/



    public Tween DOMove(GameObject obj, Vector3 pos, TweenCallback callBack = null, float time = 1.0f, Ease curvre = Ease.OutCubic, bool isPause = false)
    {
        Tween twe = obj.transform.DOMove(pos, time);
        twe.SetEase(curvre);
        if (isPause)
        {
            twe.Pause();
        }
        if (callBack!=null)
        {
            twe.OnComplete(callBack);
        }
        return twe;
    }

    public Tween DOLocalMove(GameObject obj, Vector3 pos, TweenCallback callBack = null, float time = 1.0f, Ease curvre = Ease.OutCubic, bool isPause = false)
    {
        Tween twe = obj.transform.DOLocalMove(pos, time);
        twe.SetEase(curvre);
        if (isPause)
        {
            twe.Pause();
        }
        if (callBack != null)
        {
            twe.OnComplete(callBack);
        }
        return twe;
    }

    public Tween DOColor(GameObject obj, Color color, TweenCallback callBack = null, float time = 1.0f, Ease curvre = Ease.OutCubic, bool isPause = false)
    {
        Material material = obj.GetComponent<MeshRenderer>().material;
        Tween twe = material.DOColor(color, time);
        twe.SetEase(curvre);
        if (isPause)
        {
            twe.Pause();
        }
        if (callBack != null)
        {
            twe.OnComplete(callBack);
        }
        return twe;
    }

    public Tween DOFade(Component comp, float alpha, int type = 0, TweenCallback callBack = null, float time = 1.0f, Ease curvre = Ease.OutCubic, bool isPause = false)
    {
        if (type == 0) return null;
        
        switch (type)
       {
           case 1:
               CanvasGroup co1 = comp as CanvasGroup;
               if (co1 != null)
               {
                   Tween twe = co1.DOFade(alpha, time);
                   twe.SetEase(curvre);
                   if (isPause)
                   {
                       twe.Pause();
                   }
                   if (callBack != null)
                   {
                       twe.OnComplete(callBack);
                   }
                   return twe;
               }
               break;
           case 2:
               Graphic co2 = comp as Graphic;
               if (co2 != null)
               {
                   Tween twe = co2.DOFade(alpha, time);
                   twe.SetEase(curvre);
                   if (isPause)
                   {
                       twe.Pause();
                   }
                   if (callBack != null)
                   {
                       twe.OnComplete(callBack);
                   }
                   return twe;
               }
               break;
           case 3:
               Image co3 = comp as Image;
               if (co3 != null)
               {
                   Tween twe = co3.DOFade(alpha, time);
                   twe.SetEase(curvre);
                   if (isPause)
                   {
                       twe.Pause();
                   }
                   if (callBack != null)
                   {
                       twe.OnComplete(callBack);
                   }
                   return twe;
               }
               break;
           case 4:
               Outline co4 = comp as Outline;
               if (co4 != null)
               {
                   Tween twe = co4.DOFade(alpha, time);
                   twe.SetEase(curvre);
                   if (isPause)
                   {
                       twe.Pause();
                   }
                   if (callBack != null)
                   {
                       twe.OnComplete(callBack);
                   }
                   return twe;
               }
               break;
           case 5:
               Text co5 = comp as Text;
               if (co5 != null)
               {
                   Tween twe = co5.DOFade(alpha, time);
                   twe.SetEase(curvre);
                   if (isPause)
                   {
                       twe.Pause();
                   }
                   if (callBack != null)
                   {
                       twe.OnComplete(callBack);
                   }
                   return twe;
               }
               break;

       }

        return null;
    }

    public Tween DORotate(GameObject obj, Vector3 endValue, TweenCallback callBack = null, float time = 1.0f, Ease curvre = Ease.OutCubic, bool isPause = false)
    {
        Tween twe = obj.transform.DORotate(endValue, time);
        twe.SetEase(curvre);
        if (isPause)
        {
            twe.Pause();
        }
        if (callBack != null)
        {
            twe.OnComplete(callBack);
        }
        return twe;
    }

    public Tween DORotateQuaternion(GameObject obj, Quaternion endValue, TweenCallback callBack = null, float time = 1.0f, Ease curvre = Ease.OutCubic, bool isPause = false)
    {
        Tween twe = obj.transform.DORotateQuaternion(endValue, time);
        twe.SetEase(curvre);
        if (isPause)
        {
            twe.Pause();
        }
        if (callBack != null)
        {
            twe.OnComplete(callBack);
        }
        return twe;
    }



    public Tween DOScale(GameObject obj, Vector3 endValue, TweenCallback callBack = null, float time = 1.0f, Ease curvre = Ease.OutCubic, bool isPause = false)
    {
        Tween twe = obj.transform.DOScale(endValue, time);
        twe.SetEase(curvre);
        if (isPause)
        {
            twe.Pause();
        }
        if (callBack != null)
        {
            twe.OnComplete(callBack);
        }
        return twe;
    }

    public Tween DODelay(GameObject obj, float time = 1.0f, TweenCallback callBack = null, bool isPause = false)
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(time);
        if (isPause)
        {
            seq.Pause();
        }
        if (callBack != null)
        {
            seq.AppendCallback(callBack);
        }
        return seq;
    }

    //顺序播放
    public void DoSequence(List<Tween> tweList, TweenCallback callBack = null)
    {
        int size = tweList.Count;
        if (size == 0) return;
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < size; i++)
        {
            seq.Append(tweList[i]);
        }
        if (callBack != null)
        {
            seq.AppendCallback(callBack);
        }
        seq.Play();
    }

    //同时播放
    public void DoSpawn(List<Tween> tweList)
    {
        int size = tweList.Count;
        if (size == 0) return;
        for (int i = 0; i < size; i++)
        {
            tweList[i].Play();
        }
    }

}

public class DotweenLearn : MonoBehaviour {

    public Text txt;
	// Use this for initialization
	void Start () {
        Vector3 pos = new Vector3(4, 3, 0);
        float time = 3.0f;
        DoTweenUtils instance = DoTweenUtils.GetInstance();
        //instance.DOMove(gameObject, pos, MoveOver, time);
        //instance.DOColor(gameObject, Color.red, ChangeColorOver , time);

        Tween twe1 = instance.DOMove(gameObject, pos, MoveOver, time, Ease.OutCubic, true);
        Tween twe2 = instance.DOColor(gameObject, Color.red, ChangeColorOver, time, Ease.OutCubic, true);
        List<Tween> tweList = new List<Tween> { twe1, twe2 };
        instance.DoSequence(tweList, SequenceOver);
        //instance.DoSpawn(tweList);

        //instance.DOFade(txt, 1.0f, 5, FadeOver, 3.0f);
        //instance.DORotate(gameObject, new Vector3(0, 90, 0), RotateOver, 2.0f);
        //instance.DOScale(gameObject, new Vector3(2, 2, 2), ScaleOver, 2.0f);
 

	}

    public void MoveOver()
    {
        print("MoveOver===========");
    }

    public void ChangeColorOver()
    {
        print("ChangeColorOver===========");
    }

    public void FadeOver()
    {
        print("FadeOver===========");
    }

    public void RotateOver()
    {
        print("RotateOver===========");
    }

    public void ScaleOver()
    {
        print("ScaleOver===========");
    }

    public void SequenceOver()
    {
        print("SequenceOver===========");
    }
	
	
	// Update is called once per frame
	void Update () {
		
	}
}
