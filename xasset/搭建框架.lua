

1 资源管理和热更新模块使用XAssets自带的，很好用

2 基础整体框架设计
{
	启动流程=》 ，资源GameRoot 和startupScene， 脚本 GameRoot GameLaunch GameMain
	lua框架支持==》 参考xlua-framework-unity2018框架，基础类封装使用
	{
		UI框架（参考现有项目），场景管理（参考现有项目）， MVC框架
	}

	配置表 ==》 参考xlua-framework-unity2018框架

	C#层还要实现一套框架

}

3 扩展功能支持
{
	下载器
	UI特效 UIPaticles： 使用第三方就行
	存储机制
	本地化 (Localization) 
	网络 (Network)
	动作框架：使用第三方 https://kybernetik.com.au/animancer/

	TODO:

}