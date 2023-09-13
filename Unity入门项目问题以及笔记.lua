

--[====[

	拉方块

	1 手臂伸长 使用 localScale， 锚点设置在哪，不是两端同时scale
  
	2  [SerializeField] 把私有字段和保护字段也显示在属性面板上
]====]





--笔记
--[==============[


	1 旋转方向
	{
		方法1：四元素插值, 推荐这个
		{
			Quaternion dir = Quaternion.LookRotation(player.position - transform.position);
        	transform.rotation = Quaternion.SLerp(transform.rotation, dir, Time.deltaTime);

			// Calculate the rotation 另一种旋转方式，超向目标
            float3 displacement = targetPosition - transform.Position;
            float3 upReference = new float3(0, 1, 0);
            quaternion lookRotation =
                quaternion.LookRotationSafe(displacement, upReference);

            orientation.Value =
                math.slerp(orientation.Value, lookRotation, deltaTime);

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

		方法4
		{
			Vector3 v1 = invader.transform.position - transform.position;
	        v1.y = 0;
	        // 结合叉积和Rotate函数进行旋转，很简洁很好用，建议掌握
	        // 使用Mathf.Min(turnSpeed, Mathf.Abs(angle))是为了严谨，避免旋转过度导致的抖动
	        Vector3 cross = Vector3.Cross(transform.forward, v1);
	        float angle = Vector3.Angle(transform.forward, v1);
	        transform.Rotate(cross, Mathf.Min(turnSpeed, Mathf.Abs(angle)));
		}

		transform.rotation --> 返回的是四元数 this.transform.rotation = Quaternion.Euler(0f, 60.0f, 0f); //用四元素效率更高点
		transform.eulerAngles = new Vector3(0,100,0);  //面板上roration属性为欧拉角

		transform.rotation = 
		
		方法5
		{
			e.Transform.rotation *= Quaternion.AngleAxis(e.Rotator.Speed * deltaTime, Vector3.up);
		}

		方法6
		Quaternion.FromToRotation 创建一个从 fromDirection 旋转到 toDirection 的旋转。
			将物体从 fromDirection 旋转到 toDirection

		通常情况下，您使用该方法对变换进行旋转，使其的一个轴（例如 Y 轴）跟随世界空间中的目标方向 /toDirection/。
		transform.rotation = Quaternion.FromToRotation(Vector3.up, transform.forward);


		向量绕Y轴旋转45度
		Vector3 dir = new Vector3(x,x,x)
		Quaternion.Euler(0,45,0) * dir


		//鼠标点旋转，只旋转y轴
		if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit; 
            if (Physics.Raycast(ray, out hit, 100))
            {
                var targetPoint = new  Vector3(hit.point.x, transform.position.y, hit.point.z);
                Debug.DrawLine(transform.position, targetPoint, Color.red, 10);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPoint - transform.position), 1.0f);
             
            }
        }
		
		Vector3.ProjectOnPlane(movement, hit.normal); ==》 求出向量movement在被法线hit.normal垂直的平面上的投影

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

		方法四：利用Rigidbody组件
		{
			// 获得输入的H轴和V轴，也就是横轴和纵轴，也就是W、S键或是A、D键的状态。
	        float input_h = Input.GetAxisRaw("Horizontal");
	        float input_v = Input.GetAxisRaw("Vertical");

	        // 输入是一个-1~+1之间的浮点数，把它转化成方向向量
	        Vector3 vec = new Vector3(input_h, 0, input_v);

	        // 当W键和D键同时按下时，vec会比单按W键要长一些，你可以想想为什么。
	        // 所以这里要把输入归一化，无论怎么按键，vec长度都要一致。
	        vec = vec.normalized;

	        // 乘以moveSpeed可以让调整vec的长度
	        vec = vec * moveSpeed;

	        // 把vec赋值给刚体的速度，就可以让刚体运动起来了
	        myRigidbody.velocity = vec;
		}

		方法五：
		{
			float3 targetDir = math.normalize(targetInfo.pos - translation.Value);
            float moveSpeed = 5f;
            translation.Value += targetDir * moveSpeed * DeltaTime;
		}


	    
	}

	6 Aimatior中state和blendtree的区别：state只能使用给一个动作，blendtree其实也是一个状态，只不过可以使用多个动作进行融合
	{
		https://www.cnblogs.com/hammerc/p/4832642.html
	}
	
	Input.GetKey(KeyCode.A) --> 持续按键的判断
	Input.GetKeyDown(KeyCode.A) --> 按下的动作，一次按下只触发一次

	Debug.DrawLine函数显示的射线只会出现在编辑窗口里，而不出现在Game窗口

	Vector3.Angle(transform.forward, v1) --> 两个向量之间的夹角

	{
		Application.dataPath
		Application.persistentDataPath
	}

	{
		判断射线碰到什么
		1 if (hitt.transform != null && hitt.transform.name == "Ground")

		2 Physics.Raycast(ray, out hitt, 100, LayerMask.GetMask("Ground"));  //只和"Ground"层做碰撞检测
	}
	

	7 结构体类型定义 自定义数据结构序列化，就可以使用多个自定义结构了
	{
		[System.Serializable] //可序列化标识
		public struct AnimationClipSet  // public class AnimationClipSet
		{
		    //动画剪辑
		    public AnimationClip AnimaClip;
		    //动画剪辑播放速度
		    public float ClipPlaySpeed;
		}

		在某个脚本里使用
		public AnimationClipSet Runing
	}

	8 一种动作管理的思想
	{
		1 animatonMgr中定义一个委托，在update里实时调用，所有的动作方法都写在animatonMgr里
		2 在其他脚本里，根据状态控制animatonMgr中对应的委托函数
	}

	9 委托的重载
	{
		1 父类有一个委托，在OnTriggerEnter里调用
		2 子类重载这个委托，这样就实现了，所有的子类只要单纯实现逻辑即可
	}

	10 光照贴图，烘焙 --》烘焙完成后就可以删掉或者禁用掉对应的光源了
	{
		1 烘焙 --》只针对静态物体  光源为bake模式
		2 反射探头 --》为了增强烘焙后的反射效果
		3 光照探头 --》针对动态物体的光照
	}
	
	11 角色移动，摄像机移动
	{

		不使用插件
		{
			利用Rigidbody组件，键盘上
			{
				// 获得输入的H轴和V轴，也就是横轴和纵轴，也就是W、S键或是A、D键的状态。
		        float input_h = Input.GetAxisRaw("Horizontal");
		        float input_v = Input.GetAxisRaw("Vertical");

		        // 输入是一个-1~+1之间的浮点数，把它转化成方向向量
		        Vector3 vec = new Vector3(input_h, 0, input_v);

		        // 当W键和D键同时按下时，vec会比单按W键要长一些，你可以想想为什么。
		        // 所以这里要把输入归一化，无论怎么按键，vec长度都要一致。
		        vec = vec.normalized;

		        // 乘以moveSpeed可以让调整vec的长度
		        vec = vec * moveSpeed;

		        // 把vec赋值给刚体的速度，就可以让刚体运动起来了
		        myRigidbody.velocity = vec;
			}


			使用CharacterController组件，键盘上
			{
				float h = Input.GetAxis(GlobalParams.Horizontal);
		        float v = Input.GetAxis(GlobalParams.Vertical);
		        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1)
		        {
		            Vector3 targetDir = new Vector3(h, 0, v);
		            transform.LookAt(-targetDir + transform.position);

		            Vector3 movement = transform.forward * Time.deltaTime * _playerEnitity.moveSpeed;

		           

		            //添加模拟重力
		            movement.y -= _gravity;
		            _CC.Move(movement);
		            
		        }
			}

			设备上 todo
			{
	
			}

		}
		

		使用插件 easyTouch
	
	}

	12 canvas 对于UI来说，后面的比前面的更后渲染，显示在上层

	13 string tag = prefab.GetInstanceID().ToString()  //GameObject的唯一标识

	14
	{
		var list = ListPool<Canvas>.Get();
		gameObject.GetComponentsInParent(false, list); --> 获取当前gameobject父对象上的所有canvas组件
		if (list.Count == 0)
                return;
	}

	15 世界坐标 屏幕坐标转换成UGUI坐标 : RectTransformUtility.ScreenPointToLocalPointInRectangle 和 anchoredPosition 属性
	{
		void worldToScreenInUICamera()
	    {

	        //世界坐标
	        Vector3 wPos = cubeTransform.position;

	        //转换成屏幕坐标
	        Vector3 screenPos = Camera.main.WorldToScreenPoint(wPos);


	        Vector2 localPos;
	        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos, UICamera, out localPos);
	        textRectTransform.anchoredPosition = localPos;

	    
	    }

	     if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPos = Input.mousePosition;
            Vector2 outVec;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, UICamera, out outVec);

            Debug.Log("Setting anchored positiont to: " + outVec);
            //textRectTransform.position = outVec;
            textRectTransform.anchoredPosition = outVec; //anchoredPosition 才是UGUI坐标系的坐标位置（属性面板里position属性）
            //textRectTransform.position = outVec;  //position是代表世界空间的坐标
        }

		
		//unity检测鼠标是否点在了某个UI上
        if (Input.GetMouseButtonUp(1))//右键
        {
            RectTransform rctTr = _TreeView.gameObject.GetComponent<RectTransform>();
            //如果Canvas为Overlay不需要传Camera参数，否则需要传Camera
            //Canvas canvas = GetComponent<Canvas>();
            //Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
            bool isContain = RectTransformUtility.RectangleContainsScreenPoint(rctTr, Input.mousePosition, null);
            if(isContain)
            {
                Debug.Log("点上了");
            }
            else
            {
                Debug.Log("没点上");
            }            
        }


        // 代码设置Canvas的camera
        {
			//设置UI摄像机
	        _uiCamera = GameObject.FindGameObjectWithTag("UICamera");
	        canvas.renderMode = RenderMode.ScreenSpaceCamera;
	        canvas.worldCamera = _uiCamera.GetComponent<Camera>();
	        canvas.sortingOrder = GlobalParams.DamageLabelOrder;
        }
 
	}


	16 动画状态机判断正处于某个状态
	{

		//将名称转换为哈希值可以提高索引的速度
      	private int moveSpeed = Animator.StringToHash("moveSpeed");
    	private int jump = Animator.StringToHash("Jump");
    	private int runState = Animator.StringToHash("Base Layer.Run");

		//获取动画的当前状态
		AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
		if(info.fullPathHash == runState)
		{
			xxxx
		}




		AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        if(state.shortNameHash == Animator.StringToHash("Run"))
        {
            //
        }
	}

	17 计算向量夹角
	{
		http://www.skcircle.com/?id=1360

		Vector3.Angle()方法只能返回0到180度的向量夹角，比如返回一个30度，你无法知道是30度还是330度，需要用叉乘判断


		float SignedAngle(Vector2 from, Vector2 to) 带上正负方向的角度

		{
			float angle = Vector3.Angle(v1, v2);
			angle *= Mathf.Sign(Vector3.Cross(v1, v2).y);
		}

		{
			在游戏中经常用到的，已知两向量A, B ，求出A->B的旋转角。但U3D给出的Vector3.Angle()方法只能返回0到180度的向量夹角。如果想得到0到360度的旋转角，需要自己做一些逻辑运算。方法如下：

			参数 a：起始向量; b：目标向量; n：旋转方向

			复制代码
			public static float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
			    {
			        float angle = Vector3.Angle(a,b);
			        float sign = Mathf.Sign(Vector3.Dot(n,Vector3.Cross(a,b)));
			        float signed_angle = angle * sign;
			        return (signed_angle <= 0) ? 360 + signed_angle : signed_angle;
			    }
			复制代码
			例子：已知向量a=(1, 0, 1), b=(-1, 0, 1) ，求a顺时针转到b的旋转角，则n应该为(0, 1, 0)，结果为270度。若n=(0, -1, 0)，则逆时针，结果为90度。
		}

	}


	18 DefaultExecutionOrder 是动态调整代码执行顺序的属性，越小的越先执行

	19 物理检测相关
	{
		https://blog.csdn.net/qq_39162826/article/details/120198438

		private Vector3 checkCenter = new Vector3(0, -0.45f, 0);
    	private Vector3 checkSize = new Vector3(1, 0.1f, 1);

		第三个参数用transform.rotation就行
		Physics.OverlapBox(transform.position, transform.GetComponent<BoxCollider>().size, transform.rotation)

		两种方法检测 FixedUpdate里
		{
	
			 void FixedUpdate()
		    {
		        
		        // Collider[] _colliders = Physics.OverlapBox(transform.position + checkCenter, checkSize*0.5f, transform.rotation, 1<<LayerMask.NameToLayer("Ground"));
		        // if (_colliders.Length > 0)
		        // {
		        //     Debug.Log($"{Time.frameCount} ==== 碰到了物体个数  {_colliders.Length}");
		        //     for (int i = 0; i < _colliders.Length; i++)
		        //     {
		        //         Debug.Log($"{Time.frameCount} ==== 碰到了物体名字 {_colliders[i].transform.name}");
		        //     }
		        // }
		        
		        int count = Physics.OverlapBoxNonAlloc(transform.position + checkCenter, checkSize*0.5f, _colliders, transform.rotation, 1<<LayerMask.NameToLayer("Ground"));
		        if (count > 0)
		        {
		            Debug.Log($"{Time.frameCount} ==== 碰到了物体个数  {count}");
		            for (int i = 0; i < count; i++)
		            {
		                Debug.Log($"{Time.frameCount} ==== 碰到了物体名字 {_colliders[i].transform.name}");
		            }
		        }
		        
		    }


		    /*
			     *    var matrix = Gizmos.matrix;
			            Color color = Gizmos.color;   
			            Gizmos.color = Color.yellow;  
			            Gizmos.matrix = transform.localToWorldMatrix;
			            //这里的m_localCenter是本地坐标系下的中心坐标
			            Gizmos.DrawWireCube(m_localCenter, m_size);
			            Gizmos.color = color;
			            Gizmos.matrix = matrix;
			                 * 
			     */
			    private void OnDrawGizmos()
			    {
			        var matrix = Gizmos.matrix;
			        Gizmos.matrix = transform.localToWorldMatrix; //如果立方体发生旋转也支持
			        Gizmos.color = Color.red;
			        // Gizmos.DrawWireCube(Vector3.zero, transform.GetComponent<BoxCollider>().size);
			         //Scene窗口里绘制考虑了scale影响,但Physics.OverlapBox接口transform.GetComponent<BoxCollider>().size跟scale不同步的，checkSize也要缩小一半
        			// Gizmos.DrawWireCube(Vector3.zero, transform.GetComponent<BoxCollider>().size);
			        Gizmos.DrawWireCube(checkCenter, checkSize); //**** Gizmos.DrawWireCube接口绘制是Physics.OverlapBox里size的2倍
			        Gizmos.matrix = matrix;
			    }
		}
	}
	
	
	20 Rigidbody.AddForce的ForceMode
	{
		https://blog.csdn.net/quan2008happy/article/details/39299991
		https://blog.csdn.net/littlepandas/article/details/56008898
		
		动量定理 计算公式  f•t=m•v ==》 最后算出速度v
		
		有4个枚举成员，在以下举例中均设刚体质量为m=2.0f，力向量为f=(10.0f,0.0f,0.0f)。
		
		ForceMode.Force：给物体添加一个持续的力并使用其质量。
		{
			默认方式，【使用刚体的质量计算】，以每帧间隔时间为单位计算动量。设FixedUpdate()的执行频率采用系统默认值（即0.02s）
			则由动量定理f•t=m•v
			可得：10*0.02=2*v1，从而可得v1=0.1，即【每帧刚体在X轴上值增加0.1米】，从而可计算得刚体的每秒移动速度为v2=(1/0.02)*v1=5m/s。
		}
		
		ForceMode.Acceleration:：给物体添加一个持续的加速度，但是忽略其质量。即无论设置的质量为多少，都采用默认质量1
		{
			在此种作用方式下会【忽略刚体的实际质量而采用默认值1.0f】，时间间隔以系统帧频间隔计算（默认值为0.02s）
			即f•t=1.0•v, 即可得v1= f•t=10*0.02=0.2，即【刚体每帧增加0.2米】，从而可得刚体的每秒移动速度为v2=(1/0.02)*v1=10m/s。
		}
		
		ForceMode.Impulse;：给物体添加一个瞬间的力并使用其质量
		{
			此种方式采用瞬间力作用方式，即把【t的值默认为1，不再采用系统的帧频间隔】
			即f•1.0=m•v 即可得v1=f/m=10.0/2.0=5.0，即【刚体每帧增加5.0米】，从而可得刚体每秒的速度为v2=(1/0.02)*5.0=250m/s。
			
		}
		
		ForceMode.VelocityChange;：给物体添加一个瞬间的加速度，但是忽略其质量
		{
			此种作用方式下将【忽略刚体的实际质量,采用默认质量1.0】，同时也【忽略系统的实际帧频间隔，采用默认间隔1.0】
			即f•1.0=1.0•v 即可得v1=f=10.0，即【刚体每帧沿X轴移动距离为10米】，从而可得刚体每秒的速度为v2=(1/0.02)*v1=500m/s。
		}
		
		速度大小依次是 VelocityChange > Impulse > Acceleration > Force
		
	}


	
            
            

//


//https://blog.csdn.net/qq_39162826/article/details/120198438

Physics.OverlapBox 方法使用
{
	
}

]==============]