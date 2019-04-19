

1 c#调用lua
{
	1 只能调用全局方法或者全局变量
	2 映射lua中的表的时，复杂加用接口映射（需要打标签），简单表用dict或者list映射，不推荐使用luaTable方式
	3 映射lua中的方法时，可以用委托来映射（需要打标签），不推荐用LuaFunction方式映射

}

2 lua调用c#
{

	1 只能调用c#中的public属性或者方法
	2 
	{
			1 lua中创建Unity中的对象类，需要加上命命空间CS.UnityEngine或者CS.UnityEngine.xxx
			  如果不知道可以先到unity创建，在照搬过来
			2 给gameobject添加组件需要添加typeof
			3 c#脚本组件的添加只要加上CS.xxx
			4 属性调用用. 方法调用用:
	}

	3 lua中调用继承MonoBehaviour的脚本，其实就是先获取到这个脚本组件然后调用
	4 lua中调用不继承MonoBehaviour的脚本（自定义脚本）
	{
		1 lua中的表作为参数，可以在c#中用结构体映射
		2 lua中的表作为参数，可以在c#中用接口映射，推荐使用，还可以调用lua的方法 （需要打标签）
		3 lua中的方法作为参数，可以在c#中用委托映射 （需要打标签）
	}
}

总结:
{
	1 c#映射lua,用接口映射表，用委托映射方法
	2 lua映射c#，用接口映射表，用委托映射方法
}
