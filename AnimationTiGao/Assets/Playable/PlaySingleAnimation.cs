using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlaySingleAnimation : MonoBehaviour
{
    private Animator _animator;
    private PlayableGraph _graph;
    public AnimationClip aniClip;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        //Application.targetFrameRate = 1000;
    }
    void Start()
    {
        _graph = PlayableGraph.Create("PlaySingleAnimation");
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        //_graph.SetTimeUpdateMode(DirectorUpdateMode.Manual); //需要调用_graph.Evaluate触发
        AnimationPlayableOutput animationPlayableOutput = AnimationPlayableOutput.Create(_graph, "AnimationOutput", _animator);

        AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(_graph, aniClip);
        animationPlayableOutput.SetSourcePlayable(clipPlayable);

        _graph.Play();

        /*
          AnimationPlayableUtilities.PlayClip(_animator, aniClip, out PlayableGraph graph);
            graph.Play();
         */

    }

    //private void Update()
    //{
    //    if (_graph.IsValid())
    //    {
    //        float time = Time.deltaTime;
    //        int FPS = Application.targetFrameRate;
    //        float time1 = 1.0f / FPS;
    //        Debug.Log($"time====={time} , {time1} ,{FPS}");
    //        _graph.Evaluate(1.0f / FPS);
    //    }
            
    //}

    private void OnDestroy()
    {
        if(_graph.IsValid())
            _graph.Destroy();
     
    }
}
