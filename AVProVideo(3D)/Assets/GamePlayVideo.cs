using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GamePlayVideo : MonoBehaviour
{
    public VCRControl vCRControl;

    private int _VideoIndex = 0;
    public string[] _videoFiles = { "AVProVideoSamples/BigBuckBunny_720p30.mp4", "/AVProVideoSamples/SampleSphere.mp4", "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4" };
    // Start is called before the first frame update
    void Start()
    {
        //var path = Application.dataPath + "Assets/AVProVideoSamples/BigBuckBunny_720p30.mp4";
        var path = "Assets/AVProVideoSamples/BigBuckBunny_720p30.mp4";
        vCRControl.PlayVideo(path);

        vCRControl.AddVideoEvents(StartPlayVideo, null);
    }

    // Update is called once per frame
    void StartPlayVideo(MediaPlayer mediaPlayer, int eventType)
    {
        Debug.Log("StartPlayVideo=======================");
    }

    public void ChangeVideo()
    {
 
     
        _VideoIndex = (_VideoIndex + 1) % (_videoFiles.Length);
        var path = _videoFiles[_VideoIndex];
        if (! path.Contains("http://"))
        {
            path = Application.dataPath + path;
        }
        vCRControl.PlayVideo(path);
    }
}
