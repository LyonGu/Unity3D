
print("直接引用.lua文件===========main.lua")

require "test1"
require "test.test2"



-- lua调用c#创建Uinity对象
local GameObject = CS.UnityEngine.GameObject
local newGameObj = GameObject("HelloWorld")
local game = GameObject.Find("Game")
newGameObj.transform.parent = game.transform

