using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
[System.Serializable]


/*
 //https://blog.csdn.net/ILYPL/article/details/78062995
 https://blog.csdn.net/qq_30180107/article/details/97188831
 https://gameinstitute.qq.com/community/detail/127401


    在timeline按下播放时，会调用全部behaviour的pause方法，
    之后再调用排在最前面的asset对应behaviour的play方法。
    当出现两条轨道的asset存在相连的连续播放时，前一条asset播放完后，会调用其对应behaviour的pause（没错，调用的就是pause），
    再调用后一条的pause和play。


 */
public class TestPlayableAsset : PlayableAsset
{
    //通过ExposedReference来将一些unity控件，显示在inspector上，并获取它
    public ExposedReference<Text> talkText;

    public string talkStr;
    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {

        /*
         * 通过在PlayableAsset中实例化对应PlayableBehaviour【即TestPlayableBehaviour】，并将，PlayableAsset的值赋予TestPlayableBehaviour，
         * 
            同时，要使控件在轨道上可以被playableTrack控制，我们需要在PlayableAsset的CreatePlayable中，实例化对应playableTrack【即PlayableTrackTalk】，并将PlayableAsset中的对应值赋予playableTrack，
            赋值时要使用Resolve(graph.GetResolver())来为unity的控件赋值
            talkPlayable.talkText1 = talkText.Resolve(graph.GetResolver());

         */
        TestPlayableBehaviour talkPlayable = new TestPlayableBehaviour();
        talkPlayable.talkText1 = talkText.Resolve(graph.GetResolver());

        talkPlayable.talkStr1 = talkStr;

        return ScriptPlayable<TestPlayableBehaviour>.Create(graph, talkPlayable);
    }
}
