using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


//使用分层动画可以根据身体的不同部位实现复杂的动画组合
public class PlayAnimationLayerMixer : MonoBehaviour
{

    public AnimationClip runClip;
    public AnimationClip eyeCloseClip;
    public AvatarMask faceAvatarMask;
    // Start is called before the first frame update
    void Start()
    {
        PlayableGraph graph = PlayableGraph.Create("PlayAnimationLayerMixer");
        var animationOutputPlayable = AnimationPlayableOutput.Create(graph, "AnimationOutput", GetComponent<Animator>());
        var layerMixerPlayable = AnimationLayerMixerPlayable.Create(graph, 2);
        var runClipPlayable = AnimationClipPlayable.Create(graph, runClip);
        var eyeCloseClipPlayable = AnimationClipPlayable.Create(graph, eyeCloseClip);
        graph.Connect(runClipPlayable, 0, layerMixerPlayable, 0);//第一层Layer
        graph.Connect(eyeCloseClipPlayable, 0, layerMixerPlayable, 1);//第二层Layer
        animationOutputPlayable.SetSourcePlayable(layerMixerPlayable);
        layerMixerPlayable.SetLayerMaskFromAvatarMask(1, faceAvatarMask);
        layerMixerPlayable.SetInputWeight(0, 1);
        layerMixerPlayable.SetInputWeight(1, 0.5f);
        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
