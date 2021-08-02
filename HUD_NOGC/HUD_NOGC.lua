
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

]==]