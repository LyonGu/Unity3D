
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
}


]==]