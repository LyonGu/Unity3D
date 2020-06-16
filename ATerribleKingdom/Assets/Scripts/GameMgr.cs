using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameMgr : MonoBehaviour
{

    public static GameMgr Instance { get; private set; }

    private PlayableDirector _director;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResumeTimeLine();
        }
    }

    public void PauseTimeLine(PlayableDirector director)
    {
        if (director.state == PlayState.Playing)
        {
            director.Pause();
            _director = director;
        }
    
    }

    private void ResumeTimeLine()
    {
        if (_director != null)
        {
            if (_director.state == PlayState.Paused)
            {
                _director.Play();
                _director = null;
                UIMgr.Instance.HideDialog();
            }     
        }
    }
}
