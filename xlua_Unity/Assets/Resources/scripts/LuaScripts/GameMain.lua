
 require("LuaDebug")("localhost", 7003)

print("直接引用.lua文件===========GameMain.lua")

require "CSharpCallxLua.CallLuaByGlableVar"  -- 这里面的require也是使用自定义的加载函数
require "CSharpCallxLua.CallLuaFunctionByLuaFun"
require "CSharpCallxLua.CallLuaTable"
require "xluaCallCSharp.xluaCallCSharp"


