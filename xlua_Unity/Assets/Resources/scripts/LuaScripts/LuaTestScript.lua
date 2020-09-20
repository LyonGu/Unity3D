local LuaCSharpArr = require("LuaCSharpArr")

if jit then
    print("using luajit")
    jit.off()
    jit.flush()
else
    print("using lua")
end

local math = require("math")


local luaTable = nil
local tableLen = 10000000

-- 对应的C#代码请参见LuaTestBehaviour.cs中的LuaTestBasicExample
local function BasicExample()
    local testCase = CS.LuaTestBasicExample()

    local testArr = LuaCSharpArr.New(123)
    testArr[1] = 222.5
    testArr[123] = 9527

    local CSharpAccess = testArr:GetCSharpAccess()
    testCase:CSharpExample(CSharpAccess)
end


-- 验证lua和C#是否能够正确处理int/double的读写
local function TestIntDoubleUsage()
	local testCase = CS.LuaTestIntDoubleUsage()
    local testTbl = LuaCSharpArr.New(123)
    local CSharpAccess = testTbl:GetCSharpAccess()

    testTbl[1] = 111
    testTbl[123] = 321.5
    testCase:PinTable(CSharpAccess)

    assert(math.abs(testTbl[1] - 99999.5) < 0.00001)
    assert(testTbl[123] == 123)

    testCase:Step1()
    assert(testTbl[124] == 31222)
    assert(#testTbl == 124)
    print("testTbl[124]====",testTbl[124])
    --testCase:Step2()
end

-- 验证CSharpAccess是否准确处理array变长的情况
local function TestArrayExpand()
	local testCase = CS.LuaTestArrayExpand()
    local testTbl = LuaCSharpArr.New(123)
    local CSharpAccess = testTbl:GetCSharpAccess()
    testCase:PinTable(CSharpAccess)

    for i = 124, 512 do
    	testTbl[i] = i
    end

    testCase:Check()
end

-- 验证在array被gc后，C#是否能移除指针
local function TestGCSafety()
	local testCase = CS.LuaTestGCSafety()
    local testTbl = LuaCSharpArr.New(123)
    local CSharpAccess = testTbl:GetCSharpAccess()
    collectgarbage()

    -- CSharpAccess:IsValid() should be true
    testCase:PinTable(CSharpAccess)

    -- clear ref and GC
    testTbl = nil
    CSharpAccess = nil
    collectgarbage()

    -- CSharpAccess:IsValid() should be false now
    testCase:Check()
end



function start()
    -- luaTable = LuaCSharpArr.New(123)                 -- 申请一个长123的数组，长度只是预分配，不影响动态扩充
    
    -- luaTable[1] = 123456                            -- 正常读写访问
    -- luaTable[13] = 12.5

    -- local CSharpAccess = luaTable:GetCSharpAccess() -- 获取C#访问器
    -- self:PinTable(CSharpAccess)                     -- 通过自定义函数将C#访问器传递到C#
    BasicExample()

    TestIntDoubleUsage()
    TestArrayExpand()
    TestGCSafety()
end

function update()
end

function ondestroy()
    print("lua destroy")
end



