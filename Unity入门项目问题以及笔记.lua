

--[====[

	拉方块

	1 手臂伸长 使用 localScale， 锚点设置在哪，不是两端同时scale


]====]




--笔记
--[==============[


	1 旋转方向
	{
		方法1：四元素插值, 推荐这个
		{
			Quaternion dir = Quaternion.LookRotation(player.position - transform.position);
        	transform.rotation = Quaternion.Lerp(transform.rotation, dir, Time.deltaTime);
		}

		方法2：lookat
		{
			//body就是一个transform组件
			Vector3 lookTarget = new Vector3(hit.point.x, body.position.y, hit.point.z);
            body.LookAt(lookTarget);  //用四元数代替lookat接口  
		}

		方法3
		{
			//body就是一个transform组件
			box.rotation = Quaternion.Lerp(box.rotation, body.rotation, Time.deltaTime * 5); //四元数旋转
		}

	}
	

	
	2 btn的事件点击
	{
		方法1：直接注册
			xxx.GetComponent<Button>().onClick.AddListener(fhGame); //fhGame为函数名


		方法2：delegate 可扩展参数
		Button[] btns = gameObject.GetComponentsInChildren<Button>();
        foreach (Button btn in btns)
        {
            btn.onClick.AddListener(delegate()
            {
                this.onClick(btn);
            });
        }

        方法3： EventTrigger
        {
			EventTrigger trigger = GetComponent<EventTrigger>();   //获取自身的EventTrigger组件
	        EventTrigger.Entry entry1 = new EventTrigger.Entry();

	        //鼠标点击事件
	        entry1.eventID = EventTriggerType.PointerClick;
	        //鼠标进入事件 
	        //entry2.eventID = EventTriggerType.PointerEnter;
	        //鼠标滑出事件 
	        //entry3.eventID = EventTriggerType.PointerExit;

	        entry1.callback = new EventTrigger.TriggerEvent();   //委托添加要执行的方法(回调函数)
	        entry1.callback.AddListener(OnClick);      //执行
	        trigger.triggers.Add(entry1);
        }

	}

	3 层级关系
	当摄像机和sortingLayer都一致的时候，可以通过 order in layer 属性去调节层级，就算父子节点，子节点也能在父节点下面

	4 射线发射 https://www.jianshu.com/p/d6d3d7bf5151
	{

		创建一条射线Ray需要指明射线的起点（origin）和射线的方向（direction）。这两个参数也是Ray的成员变量。
		注意，射线的方向在设置时如果未单位化，Unity 3D会自动进行单位归一化处理。射线Ray的构造函数为 ：
		public Ray(Vector3 origin, Vector3 direction);

		RaycastHit类用于存储发射射线后产生的碰撞信息。常用的成员变量如下：collider与射线发生碰撞的碰撞器
		distance 从射线起点到射线与碰撞器的交点的距离
		normal 射线射入平面的法向量
		point 射线与碰撞器交点的坐标（Vector3对象）

		方法一：Physics.Raycast静态函数用于在场景中发射一条可以和碰撞器碰撞的射线
		{
			// 以摄像机所在位置为起点，创建一条向下发射的射线  
           Ray ray = new Ray(transform.position, -transform.up);  
           RaycastHit hit;  
           if(Physics.Raycast(ray, out hit, Mathf.Infinity))  
           {  
               // 如果射线与平面碰撞，打印碰撞物体信息  
               Debug.Log("碰撞对象: " + hit.collider.name);  
                // 在场景视图中绘制射线  
               Debug.DrawLine(ray.origin, hit.point, Color.red); 
           }  
		}

		方法二，从摄像机的位置，往某一点发射射线 ScreenPointToRay / ViewportPointToRay
		{
			定向发射射线的实现
				当我们要使用鼠标拾取物体或判断子弹是否击中物体时，我们往往是沿着特定的方向发射射线，这个方向可能是朝向屏幕上的一个点，
				或者是世界坐标系中的一个矢量方向，沿世界坐标系中的矢量方向发射射线我们已经在上面演示过如何实现。
				针对向屏幕上的某一点发射射线，Unity 3D为我们提供了两个API函数以供使用，分别是ScreenPointToRay和ViewportPointToRay。

			public Ray ScreenPointToRay(Vector3 position); ###position 屏幕坐标
			参数说明：position是屏幕上的一个参考点坐标。
			返回值说明：返回射向position参考点的射线。当发射的射线未碰撞到物体时，碰撞点hit.point的值为(0,0,0)。

			ScreenPointToRay方法从摄像机的近视口nearClip向屏幕上的一点position发射射线。
			Position用实际像素值表示射线到屏幕上的位置。当参考点position的x分量或y分量从0增长到最大值时，
			射线将从屏幕的一边移动到另一边。由于position在屏幕上，因此z分量始终为0。

			 Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	        RaycastHit hit;
	        if (Physics.Raycast(ray, out hit)){
	            //获取射线碰撞到碰撞体的方位
	            _VecRayPosion = hit.point;
	        }

		}


		方法三:Physics2D.RaycastAll(transform.position, Vector3.left, 200);     //向左发射一条200的射线(会射到自己)


	}

	5 移动
	{
		方法一：使用角色控制器
		{
			//移动的步伐
	        Vector3 step = Vector3.ClampMagnitude(VecGoalPosition - this.transform.position, 0.1f);
	        //角色控制器的移动
	        CC.Move(step);
		}

		方法二: 每帧使用Vector3.MoveTowards
		{
			transform.position = Vector3.MoveTowards(transform.position, stars.transform.position, speed * Time.deltaTime);
		}

		方法三:transform.Translate 
		{
			//效果不好
	        Vector3 dis = targetPos - transform.position;
	        transform.Translate(dis * speed * Time.deltaTime, Space.World);
		}
	    
	}

	


]==============]