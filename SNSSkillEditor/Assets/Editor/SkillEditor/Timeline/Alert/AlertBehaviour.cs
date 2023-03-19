using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor.Timeline
{
    public class AlertBehaviour : BaseBehaviour
    {
        private string prefabPath = "Assets/Arts/Effects/BattleSkill/alert.prefab";

        private GameObject go; 
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            // if(go != null)
            //     Object.DestroyImmediate(go);
            //
            // var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            // go = Object.Instantiate(prefab);
            //
            // var alert = go.GetComponent<K.Alert>();
            // var data = (Alert) GetData();
            // switch (data.shape)
            // {
            //     case EnumConfig.shape.round:
            //         alert.SetShape(K.Alert.Shape.Round, data.length, 0, 360);
            //         break;
            //     case EnumConfig.shape.sector:
            //         alert.SetShape(K.Alert.Shape.Sector, data.length, 0, data.angle);
            //         break;
            //     case EnumConfig.shape.rectangle:
            //         alert.SetShape(K.Alert.Shape.Rectangle, data.length, data.width);
            //         break;
            // }
            //
            // var rootTrans = GetSelfModelRoot();
            // go.transform.forward = rootTrans.forward;
            // go.transform.position = rootTrans.position + rootTrans.forward.normalized * data.shiftDistance +
            //                         new UnityEngine.Vector3(0, 0.1f, 0);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
                go = null;
            }
        }
    }
}