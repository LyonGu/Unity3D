using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

/*
    1、将Xml的结构定义为类：
    如果Xml节点中的名字和节点一致，那就不需要[XmlType("npc")]这个设置，如果不一致需要设置（区分大小写） 
*/

[XmlType("npc")]
public class Npc
{
    public int id;
    public string name;
    public float hp;
    public float attack;
    public float def;
}

// 对于同样的数组类型可以这么写
[XmlType("npcs")]
public class Npcs : List<Npc>
{

}
public class XmlDecode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 使用Path.Combine减少路径拼接的错误
        var path = Utils.GetConfigFilePath("npcs.xml");
        var xmlStr = File.ReadAllText(path);

        // 使用using，在离开作用范围后会自动释放rdr
        using (var rdr = new StringReader(xmlStr))
        {
            //声明序列化对象实例serializer
            XmlSerializer serializer = new XmlSerializer(typeof(Npcs));
            //反序列化，并将反序列化结果值赋给变量i
            var npcs = (Npcs)serializer.Deserialize(rdr);
            Debug.Log(npcs.Count);

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
