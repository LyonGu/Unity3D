/***
 *
 *   Title: "XLua热更新"项目
 *   
 *          C#---> lua
 *          通过interface ，来映射lua中的表。
 *
 *   Description:
 *          注意：
 *              通过interface映射lua中的表，是一种引用拷贝。
 *          
 *
 *   Author: Liuguozhu
 *
 *   Date: 2018
 *
 *   Modify：  
 *
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;

namespace XluaPro
{
	public class CallLuaTableByInterface:MonoBehaviour
	{
        //lua环境(官方建议全局唯一)
        LuaEnv env = null;

        private void Start()
        {
            env = new LuaEnv();
            env.AddLoader(CustomMyLoader);

            env.DoString("require 'GameMain'");
            //得到lua中的表信息
            IGameLanguage gameLan = env.Global.Get<IGameLanguage>("gameLanguage");
            //输出显示
            Debug.Log("[使用接口]gameLan.str1="+ gameLan.str1);
            Debug.Log("[使用接口]gameLan.str2=" + gameLan.str2);
            Debug.Log("[使用接口]gameLan.str3=" + gameLan.str3);
            Debug.Log("[使用接口]gameLan.str4=" + gameLan.str4);
            //演示接口的引用拷贝原理
            gameLan.str1 = "我是修改后的内容";
            env.DoString("print('修改后gameLanguage.str1='..gameLanguage.str1)");

            //使用接口调用复杂表
            IgameUser gameUser = env.Global.Get<IgameUser>("gameUser");
            Debug.Log("[使用接口]gameUser.name=" + gameUser.name);
            Debug.Log("[使用接口]gameUser.age=" + gameUser.age);
            Debug.Log("[使用接口]gameUser.ID=" + gameUser.ID);

            gameUser.Speak();
            gameUser.Walking();

            int result = gameUser.Calulation(20,1,1);
            Debug.Log("[使用接口]result=" + result);

            int result1 = 0;
            int result2 = 0;
            int result3 = 0;
            gameUser.getDatas(20, 1, 1, out result1, out result2, out result3);
            Debug.Log(string.Format("[使用接口]多返回值= res1={0},res2={1},res3={2}", result1, result2, result3));


        }



        private byte[] CustomMyLoader(ref string fileName)
        {

            fileName = fileName.Replace(".", "/");
            byte[] byArrayReturn = null; //返回数据
            //定义lua路径
            string luaPath = Application.dataPath + "/Resources/scripts/LuaScripts/" + fileName + ".lua";
            //读取lua路径中指定lua文件内容
            string strLuaContent = File.ReadAllText(luaPath);
            //数据类型转换
            byArrayReturn = System.Text.Encoding.UTF8.GetBytes(strLuaContent);

            return byArrayReturn;
        }

        private void OnDestroy()
        {
            //释放luaenv
            env.Dispose();
        }

        //定义接口
        [CSharpCallLua]
        public interface IGameLanguage
        {
            string str1 { get; set; }
            string str2 { get; set; }
            string str3 { get; set; }
            string str4 { get; set; }
        }


        [CSharpCallLua]
        public interface IgameUser
        {
            string name { get; set; }
            int age { get; set; }
            string ID { get; set; }

            void Speak();
            void Walking();
            int Calulation(int age, int num1, int num2);

            void getDatas(int age, int num1, int num2, out int result1, out int result2, out int result3);
       
        }


    }
}


