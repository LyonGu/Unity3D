using UnityEngine;
using UnityEngine.Playables;
public class TestLightControlBehaviour : PlayableBehaviour
{
    public Light light = null;
    public Color color = Color.white;
    public float intensity = 1f;

    /// <summary>
    /// 在playable attrack上运行时 每一帧调用
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    /// <param name="playerData"></param>
    public override void ProcessFrame(Playable playable, FrameData info, object
    playerData)
    {
        if (light != null)
        {
            light.color = color;
            light.intensity = intensity;
        }
    }
}