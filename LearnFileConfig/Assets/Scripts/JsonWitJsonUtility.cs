using System.IO;
using UnityEngine;


namespace GameJsonUtility
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

    public class Npcs
    {
        public Npc[] npcs;
    }

    public class JsonWitJsonUtility : MonoBehaviour
    {
        void Start()
        {
            var jsonStr = Utils.ReadConfigFile("dataobject.json");

            var npcs = JsonUtility.FromJson<Npcs>(jsonStr);
            Debug.Log(npcs.npcs.Length);
            foreach (var npc in npcs.npcs)
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
