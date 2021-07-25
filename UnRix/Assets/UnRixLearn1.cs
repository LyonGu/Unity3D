using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UniRx;
using UniRx.Diagnostics;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Logger = UnityEngine.Logger;
using Object = UnityEngine.Object;



public class UnRixLearn1 : MonoBehaviour
{

    public Button UButtnon;

    public Toggle Utoggle;

    public Slider USlider;

    public InputField UInputField;

    public Text UText;
    public Button resurrectButton;		
		
    Player player;	

    void Start()
    {
       
        //ReactiveProperty,ReactiveCollection
        //游戏数据通常需要通知，我们应该使用属性和事件回调吗？这样的话，简直太麻烦了，还好UniRx为我们提供了ReactiveProperty,轻量级的属性代理人
        TestReactiveProperty();
        
        //ReactiveCommand,AsyncReactiveCommand
        //ReactiveCommand作为可交互按钮命令的抽象。
        //AsyncReactiveCommand 是ReactiveCommand的异步形式，将CanExecute(大多数情况下绑定到按钮的interactable)更改为false，直到异步操作执行完成。
        TestReactiveCommand();
    }

    public class Enemy
    {
        public IReactiveProperty<long> CurrentHp { get; private set; }
        public IReadOnlyReactiveProperty<bool> IsDead { get; private set; }
        public Enemy(int initialHp)
        {
            // Declarative Property
            CurrentHp = new ReactiveProperty<long>(initialHp);
            IsDead = CurrentHp.Select(x=> x<=0).ToReactiveProperty();
        }
    }

    private void TestReactiveProperty()
    {
        Enemy enemy = new Enemy(1000);
        UButtnon.onClick.AsObservable().Subscribe(_ => enemy.CurrentHp.Value-=100);

        enemy.CurrentHp.SubscribeToText(UText);
        enemy.IsDead.Where(isDead => isDead == true)
            .Subscribe(_ =>
            {
                UButtnon.interactable = false;
                Utoggle.isOn = false;
            });
    }

    public class Player
    {		
        public ReactiveProperty<int> Hp;		
        public ReactiveCommand Resurrect;		
		
        public Player()
        {		
            Hp = new ReactiveProperty<int>(1000);		
        		
            // If dead, can not execute.		
            Resurrect = Hp.Select(x => x > 0).ToReactiveCommand();		
            // Execute when clicked		
            Resurrect.Subscribe(_ =>		
            {		
                Debug.Log("dddddddddddddd");
                Hp.Value = 1000;		
            }); 		
        }		
    }
    private void TestReactiveCommand()
    {
        player = new Player();		
		
        // If Hp <= 0, can't press button.		
        player.Resurrect.BindTo(resurrectButton);	
    }

}
