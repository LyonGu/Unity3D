using CsvHelper;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace GameWebRequest
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

    public class NpcCsv
    {
        public int id { get; set; }
        public string name { get; set; }
        public float hp { get; set; }
        public float attack { get; set; }
        public float def { get; set; }
    }

    public class GetFileByWebRequest : MonoBehaviour
    {

        //IEnumerator Start()
        //{
        //    var path = Utils.GetConfigFilePath("data.json");
        //    var uri = new System.Uri(path);
        //    var request = UnityWebRequest.Get(uri.AbsoluteUri);
        //    yield return request.SendWebRequest();

        //    if (request.isHttpError || request.isNetworkError)
        //    {
        //        Debug.Log(request.error);
        //    }
        //    else
        //    {
        //        Debug.Log(request.downloadHandler.text);
        //    }
        //}

        void Start()
        {
            //读取json
            var path = Utils.GetConfigFilePath("data.json");
            var uri = new System.Uri(path);
            var request = UnityWebRequest.Get(uri.AbsoluteUri);
            request.SendWebRequest();
            while (!request.isDone) { };
            //Debug.Log(request.downloadHandler.text);
            var jsonStr = request.downloadHandler.text;
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

            //读取csv
            path = Utils.GetConfigFilePath("Npcs.csv");
            uri = new System.Uri(path);
            request = UnityWebRequest.Get(uri.AbsoluteUri);
            request.SendWebRequest();
            while (!request.isDone) { };
            //Debug.Log(request.downloadHandler.text);
            var csvStr = request.downloadHandler.text;
            using (var reader = new StringReader(csvStr))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 需要using System.Linq;才能使用ToList()
                var records = csv.GetRecords<NpcCsv>().ToList();
                Debug.Log(records.Count);

                foreach (var npc in records)
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

}


