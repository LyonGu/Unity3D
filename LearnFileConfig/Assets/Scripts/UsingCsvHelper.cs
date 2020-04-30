using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using CsvHelper;
using System.Linq;
using System.Globalization;

namespace GameCsvHelper
{
    //这里面在定义数据类的时候，必须使用属性，使用成员变量的方式将无法识别
    public class Npc
    {
        public int id { get; set; }
        public string name { get; set; }
        public float hp { get; set; }
        public float attack { get; set; }
        public float def { get; set; }
    }
    public class UsingCsvHelper : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var path = Utils.GetConfigFilePath("Npcs.csv");

            var csvStr = File.ReadAllText(path);

            using (var reader = new StringReader(csvStr))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 需要using System.Linq;才能使用ToList()
                var records = csv.GetRecords<Npc>().ToList();
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


