print("This is a script from a utf8 file")
print("tolua: 你好! こんにちは! 안녕하세요!")

c = 0
function CoFunc()
 --    while true do
	-- 	coroutine.wait(1) 
	-- 	print("Count: "..c)
	-- 	c = c + 1
	-- end


	-- while true do
	-- 	print("current frameCount: "..Time.frameCount)
	-- 	coroutine.step()
	-- 	print("yield frameCount: "..Time.frameCount)
	-- end
end


function startCor( ... )
	print("startCor==============")
	coroutine.start(CoFunc)
end


function testCall(x, obj)
	print("testCall:",x,obj.transform.position.x,obj.name)
end

function testCall1(x)
	return x+1
end

function testCall2(x)
	return x+1,"hexinping"
end


var2read = 42
varTable = {1,2,3,4,5}
varTable.default = 1
varTable.map = {}
varTable.map.name = 'map'


