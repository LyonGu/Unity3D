
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


--[==[

如何消除GC?
{
	这里的GC包括HUD对象的GC、Mesh的GC以及控制字符解析所产生的GC。传统方法是使用在GameObject.上挂接U川来做HUD渲染，
	这个太重度了，创建与释放都有不小的开销。

	要消除这些GC,首先就要改变实现方式。我使用了无U的方式，通过自己解析字符串，直接生成渲染对象(Mesh)，并挂接到摄像机上来渲染。
	再配合对象池，可以实现伤害数字的显示与隐藏无任何GC。

	说到对象池，很多人会想到List,将不用对象push到List中，用时再pop出来。



	那么要高效渲染HUD,就得解决掉这些问题。具体步骤如下：
	(1)取消GameObject,以不使用Ul的方式显示HUD,自己解析HUD内容，这样可以消除GameObject创建、销毁、显示或隐藏的开销。
	(2)全程使用对象池，将字符串解析过程中生成的数据全部使用对象池，减少GC。使用自定义的LS或使用自链表消除GC。尽可能使用D,如：图片名字或动画名字，减少字符串的拷贝与内存的开销，当然这需要使用一个管理器来管理这些名字与D的关系。
	(3)将HUD分离成相对运动和相对静止的两个列表，减/少逻辑更新与Msh频率。动静状态由角色移动或摄像机移动做相对转换，采用惰性更新机制，只在这个队列中有对象位置变化或摄像机变化时才更新，并设置一个延时转换时间。
	(4)使用unsafe方式修改List的长度，来解决Mesh数据更新产生的GC。
}


]==]

--[==[
如何解决HUD的前后遮挡问题
有同学会问，如果合并了HUD的DrawCall,那HUD的前后遮挡问题怎么解决？
由于HUD是半透明渲染的，所以并不能通过开启ztest.与Zwit两个开关来解决。要通过GPU硬件的遮挡算法，解决HUD的前后顺序问题。
这里我给出几个方案：
1.将HUD对象相对摄像机的位置做远近排序，将离摄像机近的排在前面，并按这个先后顺序，离相机近的后写入到Msh,然后通过CommandBuffer渲染，并适当拆分一些rawCall。
或增加一些额外规则，主角的单独使用一个DrawCall,总是最后渲染。或同层级的图片与文字，图片总是在文字后面渲染。
或按摄像机的距离来分层，同一层的合并到同一个DrawCall。.
当然这个并不能完全解决HUD前后遮挡问题，但是经过这些处理后，显示上并不会有太多奇怪的问题，总体还是可以接受的。

2.使用多纹理技术，目前支特OpenGL ES2.0版本的GPU,一个Drawcā，最多支特4张纹理。
利用这个规则，我们可以修改一下HUD的渲染Shdr,同时传入文字的纹理与三张图片，并在顶点中写入当前顶点所对应的纹理D。
从个人的项目经验来看，像组队标记、PK标记和头顶图片称号，这些都单独打图集，一般情况下，是不会超过三个图集的。
这样处理后，HUD就有望将文字与图片合并到一个DrawCāll中，再配合摄像机的排序，就解决了前后遮挡的问题。

3.动态图集合批技术，写一个C++的DLL,绕过Uty的文字生成，自己加载字体并生成字符纹理，再与头顶显示的图片动态合成一个图集。大部
分情况下，一张1024X1024的图片，是可以放下全部HUD与头顶图片信息的。即使不行，也可以按前后的层级关系分别生成，这样不使用多纹
理方案，也能完美解决。

]==]