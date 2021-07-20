

print("lua 启动文件====")
require("Test")

local GameObject = CS.UnityEngine.GameObject
local TextT = CS.UnityEngine.UI.Text
local MainLuaGameObject = GameObject("MainLua")
local LuaTxt = GameObject.Find("LuaTxt")
local textCom = LuaTxt:GetComponent(typeof(TextT))
textCom.text = "被Lua修改了"