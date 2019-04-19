



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





