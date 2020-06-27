using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimeLineControlMixerBehaviour : PlayableBehaviour
{

    public Dictionary<string, double> markerDic = new Dictionary<string, double>();
    // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //playerData 就是轨道上绑定的对象


        int inputCount = playable.GetInputCount ();

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<TimeLineControlBehaviour> inputPlayable = (ScriptPlayable<TimeLineControlBehaviour>)playable.GetInput(i);
            TimeLineControlBehaviour input = inputPlayable.GetBehaviour ();

            // Use the above variables to process each frame of this playable.
            if (inputWeight > 0)
            {
                //inputWeight>0 表示当前运行的clip
                var markerType = input.markType;
                switch (markerType)
                {
                    case MarkerType.JumpToMark:
                        if (!input.CheckCondition())
                        {
                            var t = markerDic[input.markerName];
                            var director = playable.GetGraph().GetResolver() as PlayableDirector;
                            if (director != null)
                            {
                                director.time = t;
                            }
                        }
                        break;
                }
            }

        }
    }
}
