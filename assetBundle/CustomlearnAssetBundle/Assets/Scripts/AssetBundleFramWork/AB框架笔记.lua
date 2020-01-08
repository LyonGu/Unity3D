

--[====[


	1 单包管理，主要看SingleABLoader和AssetLoader
	{
		SingleABLoader --> new的时候把ab包名以及回调传递进去，LoadAssetBundle这个方法是用来创建加载器的
		{
			LoadAssetBundle
			{
				AssetBundle abObj = www.assetBundle;
				_AssetLoader = new AssetLoader(abObj);
			}

			使用的时候调用SingleABLoader的LoadAsset方法 --》UnityEngine.Object tmpObj = _LoadObj.LoadAsset(_AssetName1, false);
			{
				_AssetLoader.LoadAsset(assetName,isCache) //调用到AssetLoader的loadAsset方法

				// 真正加载ab包里资源的方法
				AssetLoader:LoadAsset
				{
					//_currentAssetBundle就是AssetBundle的实例
					T tmpTResource = _currentAssetBundle.LoadAsset<T>(assetName); // 最后都要调用c#的方法
				}
			}

		}
	}



	2 多包管理  AssetBundleMgr  MultiABMgr ABManifestLoader

	{
		1 加载Manifest清单文件，在AssetBundleMgr的Awake方法里被调用, 涉及到ABManifestLoader类
		  主要是为了之后获得ab包的依赖项
		{
			StartCoroutine(ABManifestLoader.GetInstance().LoadMainifestFile());

			ABManifestLoader:LoadMainifestFile
			{
				AssetBundle abObj = www.assetBundle;
				_ABReadManifest = abObj;  //ab实例
                //读取清单文件资源。（读取到系统类的实例中。）
                _ManifestObj = _ABReadManifest.LoadAsset(ABDefine.ASSETBUNDLE_MANIFEST) as AssetBundleManifest;
                //本次加载与读取清单文件完毕。
                _IsLoadFinish = true;

			}
		}

		2 Test_framwork的Start方法里调用AssetBundleMgr的LoadAssetBundlePack方法，加载某个场景下的ab包(依赖包也会加载)以及设置回调
		{
			StartCoroutine(AssetBundleMgr.GetInstance().LoadAssetBundlePack(_sceneName, _assetBundleName, LoadAllABComplete));

			LoadAssetBundlePack
			{
				A: 等待Manifest清单文件加载完成
				B: 把当前场景加入集合中，_DicAllScenes以场景名为key, MultiABMgr为value
				{
					if (!_DicAllScenes.ContainsKey(scenesName))
			        {
			            MultiABMgr multiMgrObj = new MultiABMgr(scenesName,abName, loadAllCompleteHandle);
			            _DicAllScenes.Add(scenesName, multiMgrObj);
			        }
				}
				C: 调用“多包管理类”的加载指定AB包。
				{
					tmpMultiMgrObj.LoadAssetBundeler(abName)

					MultiABMgr:LoadAssetBundeler
					{
						a: AB包关系的建立, 用了一个字典，存储ab包对应的关系对象_DicABRelation<abName,ABRelation>
						b: 得到指定AB包所有的依赖关系（查询Manifest清单文件）, 这里也是直接调用c#的内置方法
						{
							//_ManifestObj是 AssetBundleManifest的一个实例
							return _ManifestObj.GetAllDependencies(abName);  // C#自身方法
						}
						c: 把这个ab包的依赖关系都加入到自定义的ABRelation里，以及引用关系
						{
							 ab1包里依赖ab2包，ab2包被ab1包引用，这里使用了递归，一定是先加载依赖包

						}
						d: 真正加载AB包, 创建SingleABLoader加载单个ab包
					}

				}

			}

			总结：multiMgrObj缓存，abRelationObj缓存，SingleABLoader缓存， 真正加载完成的ab资源对象缓存
		}

		3 调用方法 Object tObj = (Object)AssetBundleMgr.GetInstance().LoadAsset(_sceneName,_assetBundleName,_assetName);
		{
			AssetBundleMgr:LoadAsset
			{
				a: 根据scenesName找到对应的 MultiABMgr对象
				b: 调用MultiABMgr:LoadAsset方法
				{
					从_DicSingleABLoaderCache里根据ab包名，找到对应的SingleABLoader对象，然后调用SingleABLoader:LoadAsset
				}
				c: SingleABLoader:LoadAsset里会使用AssetLoader对象的LoadAsset方法，真正返回对应的资源对象

			}

			总结下：AssetBundleMgr--> MultiABMgr --> SingleABLoader --> AssetLoader --> 返回真正的资源对象

		}


	}


	AssetBundleMgr.GetInstance().DisposeAllAssets(_sceneName);
	{
		1 调用所有MultiABMgr对象的DisposeAllAsset方法
		2 MultiABMgr:DisposeAllAsset --> 调用所有的SingleABLoader：的DisposeALL方法
		3 SingleABLoader:DisposeALL  --> 调用AssetLoader的DisposeALL方法

	}


	优化
	{
		1 抽出打标签的逻辑，不规定死以功能文件夹作为一级目录 ==>都打成单个ab包，方便热更
		2 把所有的ab包和ab包里的asset做一个映射表，要求同一个ab包里的资源不重名
		3 使用ab包下载资源时，只要用asset的名字即可，不需要完整路径
		4 AssetBundle ab = AssetBundle.LoadFromFile(abName); 加载ab包不需要 "file://"   https://blog.csdn.net/chinacyr/article/details/45028529
	}



]====]
