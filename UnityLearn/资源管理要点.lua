
1 统一的接口实现
{
	1 支持同步和异步
	2 支持不同目录： Resources/AssetBundle/PersistentDataPath/StreamingAssets，甚至是网络资源
}

2 需要考虑的问题
{
	让我们把 资源引用计数 和 异步加载 结合起来看，异步加载 分三个阶段：未开始、进行中、已完成。异步加载一个资源，发现资源一共可能是三种状态：

	未开始： 开始加载资源，状态切换到进行中，设置资源引用计数，设置加载完成后回调。
	进行中： 设置资源引用计数，设置加载完成后回调。
	已完成： 设置资源引用计数，然后直接返回该资源。
	资源管理还涉及到卸载，这里我们使问题简单化，卸载操作是“同步”的：首先让一个资源和对象解除关联，如果一个资源所有的引用都解除了，那么就可以卸载这个资源了。卸载时也会遇到资源状态问题：

	未开始： 报错，不能卸载未加载的资源。
	进行中： 解除对象对资源的引用，加载完成逻辑如果发现资源已经没对象引用了，需要卸载。
	已完成： 解除对象对资源的引用，如果资源没有对象引用，卸载。
}

3 AssetBundle加载支持不传入ab名，只需要传入资源名即可==》资源不能同名

--[==[
    1 看下reskit那个开源库
    {
        ResLoader 继承DisposableObject IResLoader
        {
            DisposableObject：标准的DisposableObject接口
            IResLoader：基类封装接口 <--IPoolable,IPoolType
            {
                可以放入对象池中管理
            }

            ResLoader中有List<IRes>
        }

        ResSearchKeys <--IPoolable,IPoolType  
        {
			其实就是一个搜索匹配规则的类
        }

        IRes <-- IRefCounter, IPoolType, IEnumeratorTask
        {
            IRefCounter: 引用计数基类接口
            IPoolType： 对象回收接口
            IEnumeratorTask： 异步接口 使用协程实现==》 协程有最大限制，
        }

        SimpleRC <--IRefCounter：引用计数实现类
        Res <-- SimpleRC, IRes, IPoolable
        {
            Res对象可以放入对象池中
        }

        ResourcesRes <-- Res
        AssetBundleRes <-- Res
        AssetRes <-- Res
        NetImageRes <-- Res


        ResMgr
        {
	        [SerializeField] private int                         mCurrentCoroutineCount;
	        private                  int                         mMaxCoroutineCount    = 8; //最快协成大概在6到8之间

	        //双向列表
	        {
				数组和List、ArrayList集合都有一个重大的缺陷，就是从数组的中间位置删除或插入一个元素需要付出很大的代价，其原因是数组中处于被删除元素之后的所有元素都要向数组的前端移动。
				LinkedList（底层是由链表实现的）基于链表的数据结构，很好的解决了数组删除插入效率低的问题，且不用动态的扩充数组的长度。
				LinkedList的优点：插入、删除元素效率比较高；缺点：访问效率比较低。

				LinkedList< T>是一个双向链表 – 每个节点都知道它的前一个条目和下一个条目。这对于在特定节点(或头/尾)之后/之后插入是快速的，但是通过索引访问缓慢。

				LinkedList< T>通常会比List< T>占用更多的内存因为它需要所有下一个/先前引用的空间 – 并且数据可能具有较少的引用位置，因为每个节点是单独的对象。另一方面，列表< T>可以有一个比当前需要大得多的后备阵列。

	        }
	        private                  LinkedList<IEnumeratorTask> mIEnumeratorTaskStack = new LinkedList<IEnumeratorTask>();

			ResTable

        }


        ResFactory
        {
			IResCreator
			
			ResourcesResCreator <--IResCreator
			{
				里面会调用ResourcesRes.Allocate
			}
			AssetBundleResCreator <--IResCreator
			AssetResCreator <--IResCreator
			AssetBundleSceneResCreator <--IResCreator
			NetImageResCreator <--IResCreator
			LocalImageResCreator <--IResCreator
        }

    }
    }

]==]