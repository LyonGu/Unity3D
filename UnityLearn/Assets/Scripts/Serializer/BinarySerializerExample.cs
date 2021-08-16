using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinarySerializerExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //序列化到文件
        Person mike = new Person() { Age = 21, Name = "Mike" };
        mike.NameChanged += new EventHandler(mike_NameChanged);
        BinarySerializer.SerializeToFile(mike, @"c:\", "person.txt");
        Person p = BinarySerializer.DeserializeFromFile<Person>(@"c:\person.txt");
        Debug.Log($"序列化到文件 反序列化 1======{p.Age}  {p.Name}");
        p.Name = "Rose";
        Debug.Log($"序列化到文件 反序列化 2======{p.Age}  {p.Name}");

        //序列化到字符串
        //Person mike1 = new Person() { Age = 21, Name = "Mike" };
        //string str = BinarySerializer.Serialize<Person>(mike1);
        //Debug.Log($"序列化到字符串  1 ====== {str}");

        //Person mike2 = BinarySerializer.Deserialize<Person>(str); //反序列化接口报错
         //Console.WriteLine($"反序列化字符串  1 ====== {mike2.Age}  {mike2.Name}");
        
    }

    // Update is called once per frame


    static void mike_NameChanged(object sender, EventArgs e)
    {
        Debug.Log("Name Changed");
    }
}

[Serializable]
class Person
{
    private string name;
    public int Age { get; set; }
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            if (NameChanged != null)
            {
                NameChanged(this, null);
            }
            name = value;
        }
    }

    public event EventHandler NameChanged;
}
