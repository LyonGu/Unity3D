
--[==[
*******其实就是把图片和文字都放到一张图集里 动态创建mesh 不使用gameObject 使用Commandbuff去drawMesh

HudSetting ==》 全局设置信息配置都设置在一个prefab上，启动的是会加载读取配置

HUDVertex ==> 链表结构
{
	HUDMesh    //HUD Mesh数据类
	HUDGif     //HUD 顶点数据类
}


HUDTitleBase
{
	
}

HUDRender ==》 最后的绘制类  使用commandBuffer
{
	HUDMesh ==》 HUDVertex
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
	HUDTitleBatcher  头顶批处理
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



	CAtlasMng instance==》AtlasMng.InitAltasCfg ==》 加载配置信息 assets_all.text 可以直接修改配置信息来调试看结果

	Player.RefreshTitle 有很多例子
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


--[==[

------------------------------------文本解析控制码--------------------------------

那么，如何用文本来表示颜色、图片和超链接等信息呢？
我们需要定义一些控制码，绝大多数控制码都以“[”开头，并以"]”结尾。具体方案如下：
	(1)颜色
	有三种表示方法：
		a)使用明码：格式是2#RRRGGGBBB],比如用[2#255000000]表示红色。这种方法简单明了，便于编辑与书写。
		b)使用十六进制：格式是[rggbb],比如用[f0000]表示红色、用[0000]表示绿色。这种方法字符串更短，但编辑不太方便。
		c)使用带透明度的明码可以输入颜色的透明度，格式是[1#AAARRRGGGBBB],比如用1#128255000000]表示0.5透明度的红色。
	(2)图片
		[4#iconlD,w,h]:如[4#120,50,50]，这表示D为120的图元，显示大小为50X50像素：
		[5#iconname]]:如[5#icon01],这个表示图元的名字是icon_01,显示大小是原始大小。

	(3)表情动画
		格式是[6#ani id,w;h],比如：用[6#1；40：40]表示动画ID为1的动画，显示大小为40X40像素。这里w和h默认为零，如果没有写，就表示使用表情图片自身的大小。
		这样，[6#1]就表示动画ID为1的动画，显示大小取表情自身的大小。
		表情动画专门有一个ML配置文件，用来配置它的信息，本质上，它就是一个帧动画。 Assets/xml/sprite_gif.xml


	(4)超链接
		其实头顶并不需要这个，但NPC头顶可能需要，所以我就把这个功能也给实现了。格式如下：
		[7#链接字符：自定义字符串]表示默认的链接，无下划线显示
		[7#链接字符u:自定义字符串]添加u:,表示有下划线
		[7#链接字符：：自定义字符串]添加：，表示有下划线，并闪烁提示
		
		自定义的字符串，是并不显示的，是U层逻辑用的。
		如：[7#百花园：Scne1000,160,180],这个自定义字符串，可以由项目上层自己定义与实现，U底层只提供射线拾取的方法就可以了，由于这里并不需要，这里只提供了解释渲染的代码，为了简化，其它的代码我就删除了。
	
	(5)关于占位符
		有时我想控制文本空格的大小，用敲空格的方法实在是太笨，也很难对齐，于是有了占位符，这个其实并不需要渲染，只是在文本排版时空出一个指
		定大小的位置而已。格式是[8#www-hhh],比如：[8#40-40]表示指定一个40X40大小的、不可见的空格。
	
	(6)其它控制符
		如果文本中需要显示“[”，就需要使用"[”，连续两个“[”表示一个有效的"[”。类似转义符的原理。
		同样：]】表示一个有效的]\表示一个有效的n表示手动换行

]==]

--[==[


5.1描边
大家对于NGU或UGU中的描边与阴影文字效果，一定不会陌生。我们既然是自己绘制文字，当然也得实现这个功能。
先说描边，描边一般有三种实现方案：
	(1) 在生成文字位图后，将位置用边缘扫描算法直接在原位图上勾边，再写入到字体纹理。这个方法的好处是与普通文字可以采用同一个材质效果，也不会产生额外的三角形。
		由于我们使用Unity中的font,无法自己控制位图的生成，所以这个方案在这里是无法实现的。

	(2) 使用Shaders效果实现，在Shader中用合适的算法描绘出边缘。常见算法有Sobel、Laplacian和Canny filter,这里并不具体介绍，有兴趣的同学请自行百度或谷歌。
		这个方法的好处是不需要产生额外的三角形，但需要区分Shader,所以会打断HUD的合批渲染。

	(3) 第三个方案采用的是NGU中的方案，就是先分别在文字的左上方、左下方、右上方和右下方，指定偏移量，指定阴影色，反复渲染四次，产生个向外模糊的阴影，最后再叠加正常的文字，形成一个扩散型的描边效果。
		这里偏移量一般取1或2个像素，参数可调。这个方案的好处是实现简单，不需要区分Shader,也就不会打断HUD的合批渲染，但坏处是会增加三角形的数量。但对于HUD,总的三角形数量在可接受的范围。


5.2阴影效果==》【在原位置向右下偏移一到二个像素】

	阴影效果就简单多了，就是在原位置向右下偏移一到二个像素，当然在这个系统里，偏移量和颜色都是可以自己指定的。
	可能出现的问题：
	如果文字是基于3D摄像机透视变换的，阴影的位移与描边在远处透视变换之后，位移量可能会变得很小，导致描边与位移效果不是很明显。要解决这
	个问题，可以参考NGUI的UI的渲染方法，先将摄像机变换到合适的分辨率；或者在Shader中自己控制顶点变换生成屏幕坐标；或者完全使用2D的方式，不做3D变换。


5.3渐变效果 ==》 【【一个文字是由一个四边形绘制而成，那么它就有四个顶点，我们完全可以在每个顶点上设置不同的颜色，利用GPU的自动插值功能，生成一个混合颜色】】

	有了描边与阴影，你是不是还想有点其它的效果啊？没错，就是渐变效果。有了这个效果，对于伤害数字这些，甚至完全可以不用美术字。
	我们知道,一个文字是由一个四边形绘制而成，那么它就有四个顶点，我们完全可以在每个顶点上设置不同的颜色，利用GPU的自动插值功能，生成
	一个混合颜色。如果我们不需要有渐变，将四个颜色设置成同一个就可以了，是不是很简单？这种做法无需产生额外的Shader,因而不会打断合批渲染。
	当然，如果你想要更多更酷炫的效果，只要脑洞够大，就一定会有收获。比如，我们不使用顶点颜色的方案，额外增加一个美术制作的纹理就可
	以实现更多神奇的效果
]==]