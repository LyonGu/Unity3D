using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(Animator))]
public class PlayableAnimation : MonoBehaviour
{
    public AnimationClip clip;
    private PlayableGraph _graph;
    void Start()
    {
        /*
            PlayableGraph  : 图
            Playable ： 图里的资产
            PlayableOutPut : 图最后的输出显示


            PlayableGraph 包含Playable和PlayableOutPut

         */
        _graph = PlayableGraph.Create("PlayableAnimation");

        AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(graph, clip);
        AnimationPlayableOutput outPut = AnimationPlayableOutput.Create(graph, "AnimationOutPutFirst", GetComponent<Animator>());
        outPut.SetSourcePlayable(clipPlayable);  //给output设置输出资产
        graph.Play();



       // AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), clip, out graph);
    }

    private void OnDestroy()
    {
        _graph.Destroy();
    }


}
