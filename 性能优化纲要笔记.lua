
--[==[
性能标准
{
	耗时推荐值：FPS 渲染 逻辑代码 UI模块 物理模块 GPU耗时
	内存推荐值：
	{
		Reserved Total: 可以理解为分配总内存
		避免游戏闪退的重点在于控制PSS内存峰值。而PSS内存的大头又在于Reserved Total中的【资源内存和Mono堆内存】。对于使用Lua的项目来说，还应关注Lua内存。

		根据UWA的经验，只有当PSS内存峰值控制在硬件总内存的0.5-0.6倍以下的时候，闪退风险才较低
		举例而言，对于2G的设备而言，PSS内存应控制在1G以下为最佳，3G的设备则应控制在1.5G以下
		而对于大多数项目而言，PSS内存大约高于Reserved Total 200MB-300MB左右，故2G设备的Reserved Total应控制在700MB以下、3G设备则控制在1G以下。

		内存类型
		{
			资源内存
			{
				Texture
				Mesh
				Shader
				Animation Clip
				Audio Clip
				Render Texture
				Font
				ParticleSystem
			}
			Mono堆内存
			Lua内存
		}
	}
}
]==]

--[==[
性能排查工具
{
	Unity Profiler
	Unity FrameDebugger
	Mali Offline Compiler:该工具主要用来计算Shader的复杂度，结合分档的高中低数值来判断Shader是否过于复杂。
	XCode FrameDebugger: 特别强大
	GOT Online
}

]==]

--[==[
策略导致的内存问题
{
	资源冗余: 资源冗余往往是很多项目中最为常见的内存问题之一。而其中往往【AssetBundle打包策略】和【资源加载缓存策略】又是导致冗余的最主要的两种原因
	{
		bundle资源冗余：同样的资源被打了两份造成内存浪费 也会加载多次造成加载时间边长
	}
	代码生成的资源: 运行时通过代码接口生成某些暂时性资源是非常常见的做法，但使用不当也很容易产生性能问题。
	加载和缓存策略:
}


]==]

--------------------------------------Gfx内存---------------------------------------------------------
--[==[

纹理资源
{
	纹理资源是最常用、也是很多项目中占内存大头的一种资源。它还和项目的渲染模块CPU、GPU性能有很大关联。本节主要从内存角度探讨纹理的格式、分辨率、Read/Write Enabled、Mipmap

	压缩格式
	{
	 	平常存在磁盘上的都是jpg或者png的压缩格式，这些格式只能用于减少图片的磁盘占用空间，无法被GPU识别和读取，
	 	所以无论这些图片在电脑种以什么样的格式存储，在导入Unity的时候都会经过import这个过程转换成纹理的格式（ETC或者 ASTC 能直接被Mobile GPU直接读取）

	 	纹理压缩的目的
	 	{
			节省内存
			减少带宽
			降低加载纹理消耗
	 	}

	 	设备平台不支持的格式Unity会自动回退到RGBA格式

	 	ETC2_8bit 与 ATSC4X4 压缩后大小相同
	 	ETC2_4bit 与 ATSC6X6 压缩后大小相同

	}

	MipMap
	{
		优化远处物体的表现效果
		减少Cache Miss, 减少带宽

		缺点是内存增加1/3

		建议
		{
			2d UI，摄像机距离固定，没有远近变化，应关闭
			3d UI以及其他作用3d渲染的纹理，建议开启
		}
	}

	Texture Quality
	{
		可以显著改变纹理的内存占用==》通过减少加载进内存的Mipmap层数来减少纹理的占用，只针对开启Mipmap的纹理生效
		Editor——Project Settings —— Quality —— Textures
		{
			Full Res :加载所有的Mipmap层级
			Half Res ：排除Mipmap0，其他层级都加载
			Quarter Res: 排除Mipmap0 Mipmap1，其他层级都加载
		}

		可以用来画质分级


	}
	Texture Streaming
	{
	
	}
}

网格资源
{
	
}


Shader资源
{
	
}

]==]
