
--[==[


HUDVertex ==> 链表结构
{
	HUDMesh
	HUDGif
}


HUDTitleBase
{
	
}

HUDRender ==》 最后的绘制类  使用commandBuffer
{
	HUDMesh
}


//头顶信息类
HUDTitleInfo
{
	HUDTilteLine
	HUDTitleBatcher // 头顶批处理
	{
		HUDRender
	}
}


HUDTitleRender
{
	HUDTitleBatcher
}


HUDTitleInfo：PushSprite
{
	HUDVertex:InitSprite  ==> 设置顶点数组 uv数组 顶点颜色 ==》 为赋给mesh做准备

	HUDMesh：UpdateMesh
	{
		FillVertex ==》 HUDVertex里的数据构建mesh需要的数据


		构建Mesh 赋值，最后HUDRender里commandBuffer去绘制
	}


}


问题
{
	assets_all.txt 这个文件是怎样生成的 --》TODO

	{
		1 HUD/新材质编辑器
		2 把需要打进图集的图片放入到非Altas文件夹中，选中要打进图集的texture，
		3 工具窗口会出现"添加或者更新按钮", 操作即可
		4 最后点击修改

		//代码里的使用的配置都在HUDSetting里

	}
}




]==]