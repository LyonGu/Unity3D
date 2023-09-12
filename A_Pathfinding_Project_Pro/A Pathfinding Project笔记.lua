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
	RaycastModifier
	FunnelModifier  ==》 The funnel modifier is a modifier for simplifying paths on navmeshes or grid graphs is a fast and exact way.
	RadiusModifier  ==》  grid graphs  point graph
	StartEndModifier
	AlternativePath
	Modifiers
}