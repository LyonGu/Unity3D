
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

/*
	新增加了方法，最好先删除一遍，再重新导出
*/
namespace Hxp
{
	[LuaCallCSharp]
    public class CustomHelper  {

		public static int Add(int num1, int num2)
		{
			return num1 + num2;
		}

		public CustomHelper() {}
		
		public string HString(string str)
		{
			return str + "_HXP";
		}
		


    }
}
