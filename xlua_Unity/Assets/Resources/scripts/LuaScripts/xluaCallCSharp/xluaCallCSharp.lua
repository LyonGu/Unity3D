



--[[  一： 学习 lua  调用 Unity 系统 API   ]]--

--1： lua 中实例化一个 Unity 的对象 

--[===========[
	
	1 lua中创建Unity中的对象类，需要加上命命空间CS.UnityEngine或者CS.UnityEngine.xxx
	  如果不知道可以先到unity创建，在照搬过来
	2 给gameobject添加组件需要添加typeof
	3 c#脚本组件的添加只要加上CS.xxx
	4 属性调用用. 方法调用用:


]===========]
local CS = CS
local UnityEngine  = CS.UnityEngine
local GameObject   = UnityEngine.GameObject
local MeshRenderer = UnityEngine.MeshRenderer
local Resources    = UnityEngine.Resources


local newGo= GameObject()
newGo.name="lua创建的GameObject"

--给这个对象加一个组件
newGo:AddComponent(typeof(MeshRenderer))

--给这个对象加一个cs脚本组件
-- newGo:AddComponent(typeof(CS.RunLuaByFile))

--获取一个游戏对象以及获取上面的组件
local Cube = GameObject.Find("Cube")
Cube:AddComponent(typeof(CS.Rotating))
local meshRender = Cube:GetComponent(typeof(MeshRenderer))
--meshRender.enabled = false
local m = Resources.Load("logo")  --替换shader
meshRender.material = m


--[[  二： 学习 lua  调用继承MonoBehaviour C#脚本   ]]--
local gameObject = GameObject.Find("Game")
local XluaCallCSharpMonoBehaviour = gameObject:GetComponent(typeof(CS.XluaCallCSharpMonoBehaviour))
XluaCallCSharpMonoBehaviour:xluaCallCSharp()



--[[  三： 学习 lua  调用自定义C#脚本 不继承MonoBehaviour  ]]--

local XluaCallCSharpClass=CS.XluaCallCSharpClass
local xluaCallCSharp = XluaCallCSharpClass:getInstance()
xluaCallCSharp:xluaCallCSharp1()  --调用共有方法 成功
-- xluaCallCSharp:xluaCallCSharp2()　--调用私有方法 不成功

local num = xluaCallCSharp:xluaCallCSharp3(2)
print("num=======",num)

--调用属性字段
local childAge1 = xluaCallCSharp.childAge1
print("childAge1=======",childAge1)
-- local name = xluaCallCSharp.name　--name为私有属性，返回为nil或者报错
-- print("name=======",name)

--调用父类的方法和属性
local parentAge = xluaCallCSharp.parentAge
print("parentAge=======",parentAge)

xluaCallCSharp:xluaCallCSharp1Parent()
xluaCallCSharp:xluaCallCSharp3Parent(20)

--测试调用C#方法重载
local result1 = xluaCallCSharp:xluaCallCSharp3(20)
local result2 = xluaCallCSharp:xluaCallCSharp3("hexinpng")
print("result1=======",result1)
print("result2=======",result2)


local testFunc = function ()
	print("传了一个方法进去========")
end

--测试C#中带有params 关键字的方法 传入方法没成功
-- xluaCallCSharp:xluaCallCSharp4(20,"hexinpng",68, testFunc)


--测试lua调用C#中带有结构体参数的方法
--定义一个表
local myStructTable={
	x="C#语言",
	y="lua语言",

	-- 传入方法没法调用，因为struct
	test = function ()
		print("传了一个方法进去========")
	end
}
xluaCallCSharp:xluaCallCSharp5(myStructTable)


--测试lua调用C#中带有接口参数的方法
--定义一个表
local myInterfaceTable=
{
	x=1000,
	y=300,

	Speak=function()
		print("lua中 Speak 方法被调用!")
	end,

	SpeakSelf=function(self)
		print("lua中 SpeakSelf 方法被调用!")
	end
}

xluaCallCSharp:xluaCallCSharp6(myInterfaceTable)


--定义lua调用C#中带有委托参数的方法
--定义函数
myDelegate=function(num)
	print("lua 中对应委托方法。参数num="..num)
end
xluaCallCSharp:xluaCallCSharp7(myDelegate)
xluaCallCSharp:xluaCallCSharp8(myInterfaceTable, myDelegate)
--[====[
	
	总结下
	1 使用接口或者而委托可以传入lua函数作为参数回调
	2 使用接口或者而委托可以使用表来作为参数，特别方便


]====]


--接收C#多返回数值
local num1=10
local num2=20
local res1,res2,res3=xluaCallCSharp:xluaCallCSharp9(num1,num2)
print("res1="..res1)  --输出结果： 110
print("res2="..res2)  --输出结果： 3000
print("res3="..res3)  --输出结果： 999
