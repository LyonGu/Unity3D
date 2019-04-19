

-- 定义全局变量，c#中只能使用全局变量

--定义一个简单表
gameLanguage={str1="C#语言",str2="lua语言",str3="C++语言",str4="C语言"}


--定义一个综合表（lua中的OOP思想）
gameUser={
	name="崔永元",
	age=40,
	ID="18011112222",

	Speak=function()
		print("lua玩家在讨论中")
	end,

	Walking=function()
		print("lua玩家在健身中")
	end,

	Calulation=function(age,num1,num2)--说明：age 这里命名可以任意，表示当前对象（即：gameUser）
		return age.age+num1+num2
	end
}


--定义更简单的表
programLanguage={"C#","lua","C++","C"}
