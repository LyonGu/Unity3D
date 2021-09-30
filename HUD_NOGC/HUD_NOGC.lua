
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



UISpriteInfo.SerializeToTxt ==> 读取对应的字段

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

	AtlasMng_Editor中 SaveAltasCfg配置信息保存

	UISpriteInfo.SerializeToTxt==> 序列化sprite信息


	SlicedFill==》 这个方法需要理解下
}




怎样渲染
{
	HUDTitleRender.UpdateLogic==》BaseUpdateLogic-> 
	{
		1 m_StaticBatcher.UpdateLogic(bCameraDirty, vCameraPos); ==>
		{
			a HUDTitleInfo.ApplyMove
			b HUDTitleBatcher.InitTitleHUDMesh(title)
			{
				 // 遍历title.m_aSprite 拿到每一个HUDTitle信息，并且构建对应hudMesh 以及材质 （HUDMesh。SetAtlasID） 
				 // HUDRender.m_MeshList.Add(pHudMesh);
        		 // HUDRender.m_ValidList.Add(pHudMesh);
			}
			c HUDRender.FillMesh
			{
				mesh.UpdateLogic()-> mesh.UpdateMesh
				{
					FillVertex() ////拿HUDVertex里的数据构建mesh需要的数据 （顶点位置，偏移，UV, 顶点颜色）
					AdjustIndexs // 构建三角形索引数组
				}
			}

		};

		2 m_StaticBatcher.FillMeshRender -->  m_StaticBatcher.m_MeshRender.RenderTo(m_cmdBuffer);
		{
			遍历m_ValidList ，里面存的都是hudMesh
			{
				cmdBuffer.DrawMesh(mesh.m_Mesh, matWorld, mesh.m_mat); //使用commandbuffer绘制
			}
		}

	}
	材质使用的是 m_mat = new Material(Shader.Find("Unlit/HUDSprite"));


	m_StaticBatcher.
}


]==]