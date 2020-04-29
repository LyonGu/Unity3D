using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class XmlDoc : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 使用Path.Combine减少路径拼接的错误
        string path = Utils.GetConfigFilePath("npcs.xml");
        // 创建XmlDocument对象
        XmlDocument doc = new XmlDocument();
        // 加载文件
        doc.Load(path);

        //选择npcs这个节点
        var npcs = doc.SelectSingleNode("/npcs");

        //通过循环读取每个npc节点的name子节点，用InnerText获取数据
        foreach (XmlNode npc in npcs.ChildNodes)
        {
            string id = npc.SelectSingleNode("id").InnerText;
            string name = npc.SelectSingleNode("name").InnerText;
            string hp = npc.SelectSingleNode("hp").InnerText;
            string attack = npc.SelectSingleNode("attack").InnerText;
            string def = npc.SelectSingleNode("def").InnerText;
            Debug.Log("id: "+ id + " name: "+ name + " hp: " + hp + " attack: " + attack + " def: " + def);
        }
    }


}
