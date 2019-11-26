using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

// A behaviour that is attached to a playable
public class TestPlayableBehaviour : PlayableBehaviour
{
    public Text talkText1;

    public string talkStr1;
    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable) {
        talkText1.gameObject.SetActive(true);
        talkText1.text = talkStr1;
    }

	// Called when the owning graph stops playing
	public override void OnGraphStop(Playable playable) {
        if (talkText1 != null)
            talkText1.gameObject.SetActive(false);

    }

	// Called when the state of the playable is set to Play
	public override void OnBehaviourPlay(Playable playable, FrameData info) {
        talkText1.gameObject.SetActive(true);
    }

	// Called when the state of the playable is set to Paused
	public override void OnBehaviourPause(Playable playable, FrameData info) {
        if (talkText1!=null)
            talkText1.gameObject.SetActive(false);
    }

	// Called each frame while the state is set to Play
	public override void PrepareFrame(Playable playable, FrameData info) {
        talkText1.text = talkStr1;
    }
}
