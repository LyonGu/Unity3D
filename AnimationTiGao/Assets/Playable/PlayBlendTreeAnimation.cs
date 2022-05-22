using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayBlendTreeAnimation : MonoBehaviour
{

    private Animator _animator;
    private PlayableGraph _graph;
    private AnimationMixerPlayable _animationMixerPlayable;
    private AnimationClipPlayable _animationClipPlayable1;
    private AnimationClipPlayable _animationClipPlayable2;


    public AnimationClip aniClip1;
    public AnimationClip aniClip2;

    [Range(0, 1)] 
    public float speed;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Start()
    {
        _graph = PlayableGraph.Create("PlayBlendTreeAnimation");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        AnimationPlayableOutput animationPlayableOutput = AnimationPlayableOutput.Create(_graph, "AnimationOutput", _animator);

        //AnimationMixerPlayable.Create方法中第二个参数，代表我们要将几个动画进行混合
        _animationMixerPlayable = AnimationMixerPlayable.Create(_graph, 2); //参数为2代表需要2个动画进行融合
        var clipPlayable1 = AnimationClipPlayable.Create(_graph, aniClip1);
        var clipPlayable2 = AnimationClipPlayable.Create(_graph, aniClip2);
        _animationClipPlayable1 = clipPlayable1;
        _animationClipPlayable2 = clipPlayable2;
        //要利用PlayableGraph.Connect方法将AnimationClipPlayable与AnimationMixerPlayable关联起来
        _graph.Connect(clipPlayable1, 0, _animationMixerPlayable, 0);
        _graph.Connect(clipPlayable2, 0, _animationMixerPlayable, 1);


        /*同时设置连接以及权重*/
        //_animationMixerPlayable.AddInput(clipPlayable1,0, 1.0f - speed);
        //_animationMixerPlayable.AddInput(clipPlayable2, 0, speed);

        animationPlayableOutput.SetSourcePlayable(_animationMixerPlayable);
        _graph.Play();

      

    }

    void Update()
    {
        //动画混合必须设置权重
        _animationMixerPlayable.SetInputWeight(0, 1.0f - speed);
        _animationMixerPlayable.SetInputWeight(1, speed);

        //https://www.zhihu.com/question/464825919  动画长度不一样导致混合异常

        float curLength = aniClip1.length;
        float otherLength = aniClip2.length;

        float mixLength = Mathf.Lerp(otherLength, curLength, 1.0f-speed);
        _animationClipPlayable1.SetSpeed(curLength / mixLength);
        _animationClipPlayable2.SetSpeed(otherLength / mixLength);
    }
    private void OnDestroy()
    {
        if (_graph.IsValid())
            _graph.Destroy();

    }
}
