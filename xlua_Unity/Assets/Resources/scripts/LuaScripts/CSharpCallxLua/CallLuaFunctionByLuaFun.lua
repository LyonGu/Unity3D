

-- 定义全局方法

--定义单独的lua函数
function ProcMyFunc1()
	print("procMyFunc1 无参函数")
end

function ProcMyFunc2(num1,num2)
	print("procMyFunc1 两个函数 num1+num2="..num1+num2)
end

function ProcMyFunc3(num1,num2)
	print("procMyFunc1 具备返回数值的函数")
	return num1+num2
end

function ProcMyFunc4(num1,num2,num3)
	print("procMyFunc4 两个函数 num1+num2+num3="..num1+num2+num3)
end

--定义具有多个返回数值的函数
function ProcMyFunc5(num1,num2)
	local result=num1+num2
	print("ProcMyFunc5 函数，具备返回多个数值")

	return num1,num2,result
end
