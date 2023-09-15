1 RichAI 脚本默认的中心点在对象底部 这个脚本专门为导航网格图上的移动而编写

移动组件
{

	AIPath
	{
		良好的全能运动脚本，适用于所有图形类型。
		平稳地遵循路径并对物理做出反应。
		与局部回避效果很好。
		支持 3D 游戏和 2D 游戏中的移动。
	}
	
	RichAI
	{
		专为导航网格/重铸图形而设计，不适用于任何其他图形类型。
		在基于导航网格的图形上跟踪路径方面，它比 AIPath 脚本更好，它可以更好地处理被推离其路径的情况，并且通常更顺利地跟踪路径。
		与 AIPath 相比，对离网链接有更好的支持。
		与局部回避效果很好。
		支持 3D 游戏中的移动（XZ 平面中的移动），但不支持 2D。
	}
	
	AILerp
	{
		使用线性插值沿着路径移动（这就是名称中包含“lerp”（代表线性插值）的原因）。不以任何方式使用物理。
		完全按照路线走，没有任何偏差。
		由于上述几点，使用局部回避是没有意义的，因此不支持它。
		迄今为止最快的运动脚本，因为运动本身要简单得多，但请记住，如果您在游戏中需要任何类型的物理现实主义，通常应该使用其他运动脚本之一。
		支持 3D 游戏和 2D 游戏中的移动
	}

	简而言之，如果您使用基于导航网格的图形：请使用 RichAI 脚本，否则根据您的游戏需要哪种运动风格，使用 AIPath 或 AILerp
	

}

2 格子地图
{
	优点：构建速度快
	
	缺点：内存使用大，常长距离寻路较慢，无法扩展到大地图
}

2 网格地图
{
	优点
	{
		可以同时表现小细节和大区域
		由于节点数少，寻路速度很快。
		支持大世界。
		使用导航网格切割进行相当快的更新（仅限于在导航网格中切出大部分孔）
		内存使用量相对较低。
	}


	
	缺点：
	{
		构建速度可能会很慢，尤其是当您需要非常高的细节或非常大的世界时。

		更新速度比网格图慢（但如果可以使用导航网格切割速度会更快）。

		大型起伏的空白区域（例如没有任何树木或障碍物的小山）可能很难让重铸图很好地表示。
	}
}

3 寻路流程 ==》当您设置移动脚本的目的地时，这会触发一系列事件
{

	1 移动脚本（AIPath/RichAI）很快就会意识到其目的地已更改，并将安排自动路径重新计算。很快就会意识到其目的地已更改，并将安排自动路径重新计算。它创建一个路径对象（例如ABPath），并将其发送给Seeker进行计算。

	2 Seeker 将应用一些设置，然后将路径发送到主AstarPath组件。(重写了组件名 Pathfinder) 然后，该路径被放入队列中，工作线程将尽快拾取它。可能有很多工作线程同时进行寻路工作。

	3 工作线程使用A*算法计算路径。如果您有兴趣，可以在这里阅读更多相关信息：https://www.redblobgames.com/pathfinding/a-star/introduction.html。

	4 工作线程完成路径计算并将计算出的路径放入返回队列中。

	5 在下一个 Update 循环中，AstarPath组件向 Seeker 组件发送一条消息，告知路径已计算。

	6 搜索者运行修饰符来平滑路径，或以其他方式对其进行后处理。请参阅使用修饰符
	
	7 最后，搜索者在移动脚本中调用回调，以指示路径现已计算完毕并准备好遵循。

}

4 寻路组件Seeker：返回路径
{
	Seeker.StartPath() ==》仅仅是把路径请求放到一个队列，避免同时多个请求创建路径计算
	Seeker一次只会进行一次寻路调用，如果您在前一个路径完成之前请求新路径，则先前的路径请求将被取消。
	
	
	Path p = seeker.StartPath (transform.position, transform.position + Vector3.forward * 10);
	p.BlockUntilCalculated(); ==》路径立即被计算
	
	
	需要同时计算多条路径的时候使用  AstarPath.StartPath
}

5 路径修改器==》移除不必要的路径点，让路径更加平滑
{
	https://arongranberg.com/astar/documentation/stable/modifiers2.html

	SimpleSmoothModifier
	RaycastModifier ==》 grid graphs
	FunnelModifier  ==》 The funnel modifier is a modifier for simplifying paths on navmeshes or grid graphs is a fast and exact way.
	RadiusModifier  ==》  grid graphs  point graph
	StartEndModifier
	AlternativePath
	Modifiers
}

6 地图数据更新
{
	Recast graph ==> 就是以开源RecastNavgation项目弄的，跟Unity的导航系统一样
	NavMeshGraph只能全地图更新
	
	对于除 NavMeshGraph 之外的所有图形，都可以按照与开始时计算相同的方式重新计算图形的一小部分，因为通常只有完全重新计算它才有意义
	使用脚本和 GraphUpdateScene 组件执行此操作的方法是将名为“updatePhysics”的字段设置为 true
	
	
	重铸图(Recast graph)一次只能重新计算整个图块。因此，当请求更新它时，边界触及的所有图块都将完全重新计算。
	因此，最好使用较小的图块尺寸来避免大量的重新计算时间。然而，从那时起它就不再太小了，它本质上退化为网格图。
	如果您使用多线程，则大部分图块重新计算将被卸载到单独的线程，以避免对 FPS 产生太大影响。
	
	重铸图也可以使用navmesh Cutting进行伪更新。导航网格切割可以为导航网格中的障碍物切出孔，但不能添加更多导航网格表面
	
	
	https://arongranberg.com/astar/documentation/stable/navmeshcutting.html
	导航网格切割(navmesh Cutting)用于在由重铸或导航网格图生成的现有导航网格中切孔。
	重铸/导航网格图通常只允许更改现有节点上的参数（例如，使整个三角形不可行走），这不是很灵活，或者重新计算整个图块，这非常慢。
	通过导航网格切割，您可以移除（剪切）导航网格中被障碍物（例如 RTS 游戏中的新建筑物）阻挡的部分，但是您无法向导航网格添加任何新内容或更改节点的位置。这比在重铸图中从头开始重新计算整个图块要快得多。
	
	在场景视图中，导航网格切割看起来像一个挤压的 2D 形状，因为导航网格切割也有高度。它只会切割它接触到的导航网格部分。
	出于性能原因，它仅检查导航网格中三角形的边界框，因此即使三角形不与挤出形状相交，它也可能会剪切与其相交的边界框的三角形。然而，在大多数情况下，这不会产生很大的差异
	
	默认情况下，导航网格切割不考虑旋转或缩放。如果您想这样做，可以将useRotationAndScale字段设置为 true。这有点慢，但差别不是很大。
	在 3.x 中，导航网格切割只能与重铸图一起使用，但在 4.x 中，它们可以与重铸图和导航网格图一起使用。
	
	
	#######根据一个物体的collider来更新局部地图 需要确保物体能被图识别 对象的图层包含在碰撞测试蒙版或高度测试蒙版中
	{
		方法一：
		Bounds bounds = GetComponent<Collider>().bounds;
		AstarPath.active.UpdateGraphs(bounds);
		
		方法二：
		// As an example, use the bounding box from the attached collider
		Bounds bounds = GetComponent<Collider>().bounds;
		var guo = new GraphUpdateObject(bounds);

		// Set some settings
		guo.updatePhysics = true;
		AstarPath.active.UpdateGraphs(guo);
	}
	
	
	您在 Unity 编辑器中处理已知图形，则使用 GraphUpdateScene 组件通常是最简单的。例如，您可以轻松更改特定区域的标签，而无需任何代码
}


7 移动脚本
{
	感觉不需要CharacterControl脚本呀，为啥所有的例子里都用了这个
	
	
}

8 *******如果要跟其他物体交互，需要添加Collider组件以及Ridgbody组件 并且开启运动学（跟Unity的导航系统一样）


9 局部避让
{
	Unity 的 NavMesh Agent 内置了局部回避功能。
	在此包中，局部回避由名为RVOController的单独组件处理。
	如果你将它附加到一个已经有移动脚本的对象上，那么它会被自动拾取，并且局部回避将像使用 Unity 的包一样工作。
	您还需要RVOSimulator组件的单个全局实例，它处理所有计算并具有全局模拟设置。
}

10 计算路径
{
    Seeker.StartPath ==> 当前仅仅只能计算一条，后面会取消前面正在计算中的路径请求
	AstarPath.StartPath。如果您想同时计算很多路径，这非常有用
}


TODO
{
   学习目标
   {
		基础操作：构建图数据(所有类型)，设置标签 成本  网格外链接
		动态更新地图数据：navmesh Cutting
		局部避让:RVOController RVOSimulator
		
		AIPath、RichAI和AILerp。 ==》源码看下
		
		代码操作
		
		自己扩展移动脚本
   }
   
   #####最好的方式应该是只得到路径点，移动自己控制不用他内置的脚本，但局部避让ROV的自己实现？？？？
   
   自定义局部避让 https://arongranberg.com/astar/documentation/stable/localavoidanceintegration.html
}