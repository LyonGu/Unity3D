
 require("LuaDebug")("localhost", 7003)

print("直接引用.lua文件===========GameMain.lua")

require "CSharpCallxLua.CallLuaByGlableVar"  -- 这里面的require也是使用自定义的加载函数
require "CSharpCallxLua.CallLuaFunctionByLuaFun"
require "CSharpCallxLua.CallLuaTable"
require "xluaCallCSharp.xluaCallCSharp"


local CustomHelper = CS.Hxp.CustomHelper

-- 静态方法 可以参考导出的类结构一般使用.
local sum = CustomHelper.Add(2,3)
print("CustomHelper sum======",sum)

-- 对象方法
local customH = CustomHelper()
local str = customH:HString("age")
print("CustomHelper str======", str)

