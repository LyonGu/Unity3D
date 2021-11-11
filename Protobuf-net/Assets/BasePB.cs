using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BasePB : MonoBehaviour
{

    [ProtoContract]
    [Serializable]
    public class Person
    {
        [ProtoMember(1)]
        public int id;
        [ProtoMember(2)]
        public string name;

        [ProtoMember(3)]
        public Address Address { get; set; }

        [ProtoMember(4)]
        public List<int> Friends { get; set; }

        [ProtoMember(5)]
        public Dictionary<string, Friend> FriendsDic { get; set; }

    }

    [ProtoContract]
    [Serializable]
    public class Address
    {
        [ProtoMember(1)]
        public string City { get; set; }
        [ProtoMember(2)]
        public string Street { get; set; }
    }

    [ProtoContract]
    [Serializable]
    public class Friend
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public int Age { get; set; }
    }

    public Person person;

 

    // Start is called before the first frame update
    void Start()
    {
        person = new Person
        {
            id = 11,
            name = "sdfdsf",
            Address = new Address
             {
                 City = "ShenZhen",
                 Street = "ShenNanDaDao"
             },
            Friends = new List<int>() { 1, 2, 3 },
            FriendsDic = new Dictionary<string, Friend>() {
                    {"hxp1", new Friend(){ Name = "hxp1",Age =223} },
                    {"hxp2", new Friend(){ Name = "hxp2",Age =223} },
                }
        };

        //序列化
        byte[] bytes = null;
        using (MemoryStream ms = new MemoryStream())
        {
            Serializer.Serialize(ms, person);
            bytes = new byte[ms.Length];
            Buffer.BlockCopy(ms.GetBuffer(), 0, bytes, 0, (int)ms.Length);
        }

        //反序列化
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            Person p = Serializer.Deserialize<Person>(ms);

            Debug.Log($"{p.id} {p.name}");
            Debug.Log($"{p.Address.City} {p.Address.Street}");
            Debug.Log($"{p.Friends[0]} {p.Friends[1]}");
            Debug.Log($"{p.FriendsDic["hxp1"].Age} {p.FriendsDic["hxp1"].Name}");
        }

        //序列化到文件
        string path = Application.dataPath + "/person.bytes";
        using (FileStream file = File.Create(path))
        {
            Serializer.Serialize(file, person);
        }

        //从文件里反序列化
        using (FileStream file = File.OpenRead(path))
        {
            Person p = Serializer.Deserialize<Person>(file);
            Debug.Log($"File: {p.id} {p.name}");
            Debug.Log($"File: {p.Address.City} {p.Address.Street}");
            Debug.Log($"File: {p.Friends[0]} {p.Friends[1]}");
            Debug.Log($"File: {p.FriendsDic["hxp1"].Age} {p.FriendsDic["hxp1"].Name}");
        }
    }
}
