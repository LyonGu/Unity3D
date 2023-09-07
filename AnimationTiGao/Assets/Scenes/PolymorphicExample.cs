using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;

//IPolymorphic 继承这个是为了可以在Unity面板上用item标识不同类型  只要声明一个基类就行
[Serializable]
public abstract class Item: IPolymorphic
{
    [SerializeField] private string _Name;
    [SerializeField] private string _Description;
    [SerializeField] private float _Weight;
}

[Serializable]
public class Armour : Item
{
    [SerializeField] private int _Defence;
}

[Serializable]
public class Potion : Item
{
    [SerializeField] private int _HealingAmount;
}

[Serializable]
public class Weapon : Item
{
    [SerializeField] private int _Damage;
    [SerializeField] private float _AttackSpeed;
}
public class PolymorphicExample : MonoBehaviour
{
    /*
     *    _ItemSerializeField 根本不显示。常规序列化字段不支持继承，因此该字段只能序列化为 Item
     *   但Item类是抽象的，因此item对象不可能存在，并且 Unity 根本不会尝试序列化它
     *
     */
    // [SerializeField] private Item _ItemSerializeField;
    
    /*
     *
     *  * public abstract class Item : IPolymorphic on the top class or
     *   [SerializeReference] [Polymorphic] private Item itemSerializeReference; on the bottom field.
     * 
     */
    [SerializeReference] private Item[] _ItemSerializeField;
    
    //_PotionSerializeField 显示正确，但始终是Potion类型，不能是任何其他类型的物品
    // [SerializeField] private Potion _PotionSerializeField;
    
    //Unity 的序列化系统通常不支持继承。使用 [SerializeReference] 属性可以做到这一点
    //_ItemSerializeReference 显示出来，但默认情况下它为 null，并且 Unity 不会绘制任何内容来让您在检查器中为其分配值
    // [SerializeReference] private Item _ItemSerializeReference;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
