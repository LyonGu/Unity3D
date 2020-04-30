using System.IO;
using UnityEngine;
using LitJson;


namespace GameLitJson
{
    [System.Serializable]
    public class Npc
    {
        public int id;
        public string name;
        public float hp;
        public float attack;
        public float def;
    }


    public class JsonWitLitJson : MonoBehaviour
    {
        void Start()
        {
            var jsonStr = Utils.ReadConfigFile("data.json");


            var npcs = JsonMapper.ToObject<Npc[]>(jsonStr);
            Debug.Log(npcs.Length);
            foreach (var npc in npcs)
            {
                int id = npc.id;
                string name = npc.name;
                float hp = npc.hp;
                float attack = npc.attack;
                float def = npc.def;
                Debug.Log("id: " + id + " name: " + name + " hp: " + hp + " attack: " + attack + " def: " + def);
            }
        }
    }
}
