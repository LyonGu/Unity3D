using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CustomAnimationControllor : MonoBehaviour
{

    public AnimationClip[] clipsToPlay;
    PlayableGraph m_Graph;
    // Start is called before the first frame update
    void Start()
    {
        m_Graph = PlayableGraph.Create("CustomAnimationControllor1");

        //一个playable里包含一个PlayableBehaviour
        var custPlayable = ScriptPlayable<CustomAnimationControllerPlayable>.Create(m_Graph);

        var playQueue = custPlayable.GetBehaviour();

        playQueue.Initialize(clipsToPlay, custPlayable, m_Graph);

        var playableOutput = AnimationPlayableOutput.Create(m_Graph, "CustomAnimationOutput", GetComponent<Animator>());

        //设置SourcePlayable为自定义的custPlayable
        playableOutput.SetSourcePlayable(custPlayable);
        playableOutput.SetSourceInputPort(0);

        m_Graph.Play();
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        if (m_Graph.IsValid())
            m_Graph.Destroy();
    }
}
