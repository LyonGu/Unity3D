
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RenderHeads.Media.AVProVideo;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

public class VCRControl : MonoBehaviour
{
    public MediaPlayer _mediaPlayer;
    public MediaPlayer _mediaPlayerB;
    public DisplayUGUI _mediaDisplay;
    public Slider _videoSeekSlider;  //拖动进度条

    public bool isShowSlider = false;

    private float _setVideoSeekSliderValue; //当前视频的进度
    private bool _wasPlayingOnScrub;

    //public MediaPlayer.FileLocation _location = MediaPlayer.FileLocation.AbsolutePathOrURL;


    //[CSharpCallLua]
    public delegate void VideoEvent(MediaPlayer mediaPlayer, int eventType);

    private VideoEvent _startedCallBack; //开始播放回调
    private VideoEvent _finishedPlayingCallBack; //结束播放回调，仅仅对非循环播放生效

    private MediaPlayer _loadingPlayer;

    public MediaPlayer LoadingPlayer
    {
        get
        {
            return _loadingPlayer;
        }
    }

    public MediaPlayer PlayingPlayer
    {
        get
        {
            if (LoadingPlayer == _mediaPlayer)
            {
                return _mediaPlayerB;
            }
            return _mediaPlayer;
        }
    }

    private void SwapPlayers()
    {
        // Pause the previously playing video
        PlayingPlayer.Control.Pause();

        //Swap the videos
        if (LoadingPlayer == _mediaPlayer)
        {
            _loadingPlayer = _mediaPlayerB;
        }
        else
        {
            _loadingPlayer = _mediaPlayer;
        }

        // Change the displaying video
        _mediaDisplay.CurrentMediaPlayer = PlayingPlayer;
    }

    private void Awake()
    {
        _loadingPlayer = _mediaPlayerB;
    }

    void Start()
    {
        if (PlayingPlayer)
        {
           
            PlayingPlayer.Events.AddListener(OnVideoEvent);

            if (LoadingPlayer)
            {
                LoadingPlayer.Events.AddListener(OnVideoEvent);
            }
        }

        if (_videoSeekSlider!=null)
        {
            _videoSeekSlider.gameObject.SetActive(isShowSlider);
        }
    }

    private void SetVideoEventCall(VideoEvent startEventCall, VideoEvent finishedPlayingEventCall)
    {
        _startedCallBack = startEventCall;
        _finishedPlayingCallBack = finishedPlayingEventCall;
    }


    #region 基础操作函数
    //播放视频
    public void PlayVideo(string videoPath, bool isAutoStart = true)
    {
        if (LoadingPlayer != null)
        {
            LoadingPlayer.m_VideoPath = videoPath;
            LoadingPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, LoadingPlayer.m_VideoPath, isAutoStart);

        }
    }

    //倒着播放
    public void BackPlayVideo()
    {
        SetPlaybackRate(-1.0f);
    }

    //暂停播放
    public void PauseVideo()
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Pause();
        }
    }

    //继续播放，会在停止的位置继续播
    public void ResumPlayVideo()
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Pause();
            PlayingPlayer.Play();
        }
    }

    //停止播放
    public void StopPlayVideo()
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Stop();
        }
    }

    //回到视频最开始
    public void RewindVideo(bool isPause = true)
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Rewind(isPause);
        }
    }

    public void SetLoop(bool isLoop)
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Control.SetLooping(isLoop);
        }
    }

    public bool IsLoop()
    {
        bool isLoop = false;
        if (PlayingPlayer != null)
        {
            isLoop = PlayingPlayer.Control.IsLooping();
        }
        return isLoop;
    }

    public bool IsPlaying()
    {
        bool isPlaying = false;
        if (PlayingPlayer != null)
        {
            isPlaying = PlayingPlayer.Control.IsPlaying();
        }
        return isPlaying;
    }

    public bool IsPaused()
    {
        bool isPaused = false;
        if (PlayingPlayer != null)
        {
            isPaused = PlayingPlayer.Control.IsPaused();
        }
        return isPaused;
    }

    //设置静音
    public void SetMute(bool isMute)
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Control.MuteAudio(isMute);
        }
    }


    public bool IsMute()
    {
        bool isMute = false;
        if (PlayingPlayer != null)
        {
            isMute = PlayingPlayer.Control.IsMuted();
        }
        return isMute;
    }

    //设置播放速率，负值为倒着播放
    //倍率播放 0.25, 0.5, 1.0, 1.25, 1.5, 1.75, 2.0，---》 倒着播 -0.25, -0.5, -1.0 
    public void SetPlaybackRate(float rate = 1.0f)
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Control.SetPlaybackRate(rate);
        }
    }

    public float GetPlaybackRate()
    {
        float rate = 0.0f;
        if (PlayingPlayer != null)
        {
            rate = PlayingPlayer.Control.GetPlaybackRate();
        }
        return rate;
    }

    public void SetVolume(float volume)
    {
        if (PlayingPlayer != null)
        {
            PlayingPlayer.Control.SetVolume(volume);
        }
    }

    public float GetVolume()
    {
        float volume = 0.0f;
        if (PlayingPlayer != null)
        {
            volume = PlayingPlayer.Control.GetVolume();
        }
        return volume;
    }

    //获取视频播放进度
    public float GetVideoPlayProgress()
    {
        float pro = 0.0f;
        if (PlayingPlayer != null)
        {
            float time = PlayingPlayer.Control.GetCurrentTimeMs();
            float duration = PlayingPlayer.Info.GetDurationMs();
            pro = Mathf.Clamp(time / duration, 0.0f, 1.0f);
        }
        return pro;
    }

    //设置视频播放进度 0~1
    public void SetVideoPlayProgress(float value)
    {
        value = Mathf.Clamp(value, 0.0f, 1.0f);
        if (PlayingPlayer != null)
        {
            float duration = PlayingPlayer.Info.GetDurationMs();
            PlayingPlayer.Control.Seek(value * duration);
        }
    }

    public void AddVideoEvents(VideoEvent startEventCall, VideoEvent finishedPlayingEventCall)
    {
        RemoveVideoEvents();
        if (PlayingPlayer != null)
        {

            //PlayingPlayer.Events.AddListener(OnVideoEvent);
            SetVideoEventCall(startEventCall, finishedPlayingEventCall);
        }
    }

    public void RemoveVideoEvents()
    {
        if (PlayingPlayer != null)
        {
            //PlayingPlayer.Events.RemoveListener(OnVideoEvent);
            _startedCallBack = null;
            _finishedPlayingCallBack = null;
        }
    }
    #endregion


    public void OnVideoSeekSlider()
    {
        if (PlayingPlayer && _videoSeekSlider && _videoSeekSlider.value != _setVideoSeekSliderValue)
        {
            PlayingPlayer.Control.Seek(_videoSeekSlider.value * PlayingPlayer.Info.GetDurationMs());
        }
    }

    public void OnVideoSliderDown()
    {
        if (PlayingPlayer != null)
        {
            _wasPlayingOnScrub = PlayingPlayer.Control.IsPlaying();
            if (_wasPlayingOnScrub)
            {
                PlayingPlayer.Control.Pause();
                //				
            }
            OnVideoSeekSlider();
        }
    }
    public void OnVideoSliderUp()
    {
        if (PlayingPlayer && _wasPlayingOnScrub)
        {
            PlayingPlayer.Control.Play();
            _wasPlayingOnScrub = false;

        }
    }

    private void OnDestroy()
    {
        if (PlayingPlayer)
        {
            _startedCallBack = null;
            _finishedPlayingCallBack = null;
            if (LoadingPlayer)
            {
                LoadingPlayer.Events.RemoveListener(OnVideoEvent);
            }
            if (PlayingPlayer)
            {
                PlayingPlayer.Events.RemoveListener(OnVideoEvent);
            }
        }
    }

    void Update()
    {
        if (PlayingPlayer && PlayingPlayer.Info != null && PlayingPlayer.Info.GetDurationMs() > 0f)
        {
            float time = PlayingPlayer.Control.GetCurrentTimeMs();
            float duration = PlayingPlayer.Info.GetDurationMs();
            float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);

            // Debug.Log(string.Format("time: {0}, duration: {1}, d: {2}", time, duration, d));

            _setVideoSeekSliderValue = d;
            if (_videoSeekSlider != null)
            {
                _videoSeekSlider.value = d;
            }
        }
    }

    // Callback function to handle events
    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {

            case MediaPlayerEvent.EventType.Started:
                if (_startedCallBack != null)
                {
                    _startedCallBack(mp, (int)et);
                }
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady: // Triggered when the first frame has been rendered
                SwapPlayers();
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                if (_finishedPlayingCallBack != null)
                {
                    _finishedPlayingCallBack(mp, (int)et);
                }
                break;
        }

        Debug.Log("Event: " + et.ToString());
    }
}