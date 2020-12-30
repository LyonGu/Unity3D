using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class UIAnimationFramLearn : MonoBehaviour
{
    // Start is called before the first frame update

    public AnimationCurve animationCurve;
    public Transform cubeTransform;
    public Image image;
    public Text text;
    public Gradient gradient;

    private float EndTime;
    private float StartTime;

    private Tween moveTween;
    Sequence quence;

    private Vector3 curPos;

    public List<Vector3> CubePathList = new List<Vector3>();
    public List<Vector3> ImagePathList = new List<Vector3>();

    public Transform lookAtTargetTran;

    public GameObject particleSystemObj;

    void Start()
    {
        //transform.DOLocalMoveX(10, 10).SetEase(animationCurve).SetRelative();

        //TweenParams t;

        int tLength = animationCurve.length;
        Debug.Log($"tLength============{tLength}"); //返回的是key的长度

        Keyframe[] keys = animationCurve.keys;
        foreach (var item in keys)
        {
            Debug.Log($"time:{item.time}, value:{item.value}");
        }
        StartTime = animationCurve.keys[0].value;
        EndTime = animationCurve.keys[tLength - 1].value;


        //image.DOColor(Color.red, 5).SetEase(animationCurve);
        //image.DOFade
        //image.DOGradientColor(gradient, 5);


        //AnimationCurve 横轴是时间, 不过不是具体的时间，而是时间比例
        //AnimationCurve 纵轴是倍数
        //假设纵轴的值为v，传入DOMove的第一个参数endValue是e，起始点坐标是s
        //此物体最后动画结束时的实际坐标即为 v * （e - s）+s

        //transform.DOLocalMoveX(0, 5).SetEase(animationCurve); //会影响最后位置 good
        //transform.DOLocalMoveX(0, 5).SetEase(Ease.Linear); //不会影响最后位置 good

        //moveTween = transform.DOMove(Vector3.one *2, 5).SetEase(Ease.Linear); //所有坐标值（3个）的都会变化
        //moveTween.SetAutoKill(false);
        //transform.DOMove(Vector3.one * 2, 5).SetEase(animationCurve);
        //transform.DOScale(Vector3.one * 2, 2).SetEase(animationCurve);

        //gameObject destory的时候killmoveTween？？？？？
        //moveTween.OnKill(() =>
        //{
        //    Debug.Log("OnKill==============");
        //});

        //quence = DOTween.Sequence();
        //quence.Append(transform.DOMove(Vector3.one * 2, 2).SetEase(Ease.Linear).OnComplete(() =>
        //{
        //    Debug.Log("Move Action Over");
        //}));
        //quence.Join(transform.DOScale(Vector3.one * 5, 5).SetEase(Ease.Linear).OnComplete(() =>
        //{
        //    Debug.Log("Scale Action Over");
        //}));
        //quence.Join(transform.DORotate(Vector3.up * 360, 5).SetEase(Ease.Linear).OnComplete(() =>
        //{
        //    Debug.Log("Rotate Action Over");
        //}));
        //quence.Play();

        //transform.DORotate(new Vector3(0, -360, 0), 5).SetEase(Ease.Linear).SetRelative(true);  //正数表示逆时针

        //curPos = transform.position;
        //DOTween.To(() => transform.position, x => transform.position = x, Vector3.one * 2, 2);

        //curPos = transform.position;
        //DOTween.To(() => curPos, x => curPos = x, Vector3.one * 2, 2);

        //moveTween = transform.DOMove(Vector3.one * 2, 5).SetEase(Ease.Linear);
        //moveTween.SetId<Tween>(moveTween);


        //DoPath
        //cubeTransform.DOPath(CubePathList.ToArray(), 5, PathType.CatmullRom, PathMode.Full3D, 10, Color.clear).SetEase(Ease.InSine);
        cubeTransform.DOLocalPath(CubePathList.ToArray(), 5, PathType.CatmullRom, PathMode.Full3D, 10, Color.clear).SetEase(Ease.InSine).OnComplete(()=> {
            Debug.Log("Path Action is Done==========");
        
        });

        //this.gameObject.transform.DOPath(CubePathList.ToArray(), 5, PathType.CatmullRom, PathMode.Full3D, 10, Color.clear).SetEase(Ease.InSine);
        //var tweenPath = image.transform.DOLocalPath(ImagePathList.ToArray(), 5, PathType.CatmullRom).SetEase(Ease.InSine); // Ok
        //tweenPath.SetLookAt(lookAtTargetTran);

        //tweenPath.SetOptions(false, AxisConstraint.Z, AxisConstraint.Y);
        //tweenPath.SetLookAt(1);

        Renderer renderer = cubeTransform.gameObject.GetComponent<Renderer>();
        var material = new Material(renderer.material);
        material.name = "Test";
        renderer.material = material;
        //MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        //renderer.SetPropertyBlock(propertyBlock);

        //不存在的属性不会产生回调
        material.DOColor(Color.blue, "_Color33333333", 5).OnComplete(()=> {
            Debug.Log("material DOColor Action is Done==========");
        });

        //propertyBlock.SetColor()
        //DOVector(this Material target, Vector4 endValue, string property, float duration);
        //DOFloat(this Material target, float endValue, string property, float duration);
        //DOColor(this Material target, Color endValue, float duration);
        //DOColor(this Material target, Color endValue, string property, float duration);


        //material.DOVector();
        //material.DOFloat();
        //material.DOColor()

    }

    public void TestPart()
    {
        particleSystemObj.SetActive(false);
        particleSystemObj.SetActive(true);
    }

    public void Stop()
    {
        
        moveTween.Kill();
    }

    public void Pause()
    {
        moveTween.Pause();
    }

    public void Resume()
    {
        bool isPlaying = moveTween.IsPlaying();
        Debug.Log($"isPlaying==========={isPlaying}");
        moveTween.Play();
    }


    public void StopSequence()
    {

        quence.Kill();
    }

#if UNITY_EDITOR
    [ContextMenu("ActionTest")]
    public void MoveAction()
    {
        Debug.Log("MoveAction================");
        //DOTween.To(() => transform.position, x => transform.position = x, Vector3.one * 2, 2);
        //测试
        EditorApplication.update -= ExcuteMoveUpdate;
        //EditorApplication.update += ExcuteMoveUpdate;

        curPos = transform.position;

        //压根就不执行
        DOTween.To(() => curPos, x => curPos = x, Vector3.one * 2, 2).OnComplete(()=> {
            Debug.Log("MoveAction over====");
            EditorApplication.update -= ExcuteMoveUpdate;
        });
    }

    void ExcuteMoveUpdate()
    {
        Debug.Log($"x y z ==========={curPos.x} {curPos.y} {curPos.z}");
        if(curPos!= null)
            transform.position = curPos;
    }
#endif


    [ContextMenu("DoTweenAlpha")]
    void DoTweenAlpha()
    {
        Debug.Log("DoTweenAlpha");

        //UIRect uiRect = m_uiRectAni;
        //if (uiRect != null)
        //{
        //    DOTween.To(x => uiRect.alpha = x, 1.0f, 0.0f, 5.0f).SetId("Tween");
        //}
        text.DOText("下面是有奖竞猜:", 2);
    }


    public void MoveActionBack()
    {
        moveTween.PlayBackwards();
    }

    private void OnDestroy()
    {
        if (moveTween!= null)
        {
            moveTween.Kill();
            moveTween = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Time.time < StartTime)
        //{
        //    //用length其实不准，应该用第一帧时间和最后一帧时间
        //    //Debug.Log("动画播放未开始=========");
        //    return;
        //}
        //if (Time.time > EndTime)
        //{
        //    //用length其实不准，应该用第一帧时间和最后一帧时间
        //    //Debug.Log("动画播放已结束=========");
        //    return;
        //}

        //Debug.Log("动画播放=========");
        //float r = animationCurve.Evaluate(Time.time);
        //Debug.Log($"r====={r}  {Time.time}");
        //transform.eulerAngles = new Vector3(0, r * 100, 0);
        //image.color = new Color(r, r, r, 1);

        //if (curPos != null)
        //    Debug.Log($"x ==========={curPos.x}");
    }
}
