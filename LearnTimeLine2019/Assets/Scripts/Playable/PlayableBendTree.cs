using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableBendTree : MonoBehaviour
{
    private PlayableGraph playableGraph;
    private AnimationMixerPlayable mixerPlayable;
    public AnimationClip clip0;
    public AnimationClip clip1;
       
    [Range(0,1)]
    public float weight = 0.5f;
   
    void Start()
    {
        playableGraph = PlayableGraph.Create("PlayableBendTree");

        var playableOutPut = AnimationPlayableOutput.Create(playableGraph, "AniamtionOutput", GetComponent<Animator>());
        var clip0Playable = AnimationClipPlayable.Create(playableGraph, clip0);
        var clip1Playable = AnimationClipPlayable.Create(playableGraph, clip1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        playableGraph.Destroy();
    }
}
