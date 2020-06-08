using UnityEngine;
using UnityEngine.Playables;
public class TestLightControlBehaviour : PlayableBehaviour
{
    
    public Color color = Color.white;
    public float intensity = 1f;

    /// <summary>
    /// 在playable attrack上运行时 每一帧调用
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    /// <param name="playerData">绑定的组件</param>
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Light light = playerData as Light; 
        if (light != null)
        {
            light.color = color;
            light.intensity = intensity;
        }
    }
}