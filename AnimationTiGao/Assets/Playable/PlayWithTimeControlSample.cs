using UnityEngine;

using UnityEngine.Playables;

using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]

public class PlayWithTimeControlSample : MonoBehaviour

{

    public AnimationClip clip;

    public float time;

    PlayableGraph playableGraph;

    AnimationClipPlayable playableClip;

    void Start()

    {

        playableGraph = PlayableGraph.Create("PlayWithTimeControlSample");

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        // Wrap the clip in a playable

        playableClip = AnimationClipPlayable.Create(playableGraph, clip);

        // Connect the Playable to an output

        playableOutput.SetSourcePlayable(playableClip);

        // Plays the Graph.

        playableGraph.Play();

        // Stops time from progressing automatically.

        playableClip.SetPlayState(PlayState.Paused);

    }

    void Update()

    {

        // Control the time manually 设置动画卡在哪一个时间
        time += Time.deltaTime;
        playableClip.SetTime(time);

    }



    void OnDisable()

    {

        // Destroys all Playables and Outputs created by the graph.

        playableGraph.Destroy();

    }

}