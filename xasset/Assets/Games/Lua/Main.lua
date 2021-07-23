print("加载Main.lua 文件===")
require("Login")

local GameObject = CS.UnityEngine.GameObject
local TextT = CS.UnityEngine.UI.Text
local MainLuaGameObject = GameObject("MainLua")
local LuaTxt = GameObject.Find("LuaTxt")
local textCom = LuaTxt:GetComponent(typeof(TextT))
textCom.text = "被Lua修改了"

local rapidjson = require('rapidjson')
local t = rapidjson.decode('{"a":123}')
print(t.a)
t.a = 456
local s = rapidjson.encode(t)
print('rapidjson  ====', s)

local cjson = require "cjson"
local ct = cjson.decode('{"a":456}')
print('cjson  ====', ct.a)