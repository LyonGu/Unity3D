using UnityEngine;
using UnityEngine.Playables;
public class TestLightControlBehaviour : PlayableBehaviour
{
    public Light light = null;
    public Color color = Color.white;
    public float intensity = 1f;

    /// <summary>
    /// ��playable attrack������ʱ ÿһ֡����
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