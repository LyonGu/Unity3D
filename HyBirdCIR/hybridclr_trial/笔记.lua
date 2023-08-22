
打包的时候，如果全是原始文件会报错，必须要有一个资源文件

配置好资源服务器后，下载目录为 工程目录/yoo/DefaultPackage

"C:\Personer\GitRespositories\Unity3D\HyBirdCIR\hybridclr_trial\yoo\DefaultPackage"


AOT程序集 不能直接引用热更新代码，可以使用其他方法 https://hybridclr.doc.code-philosophy.com/docs/basic/runhotupdatecodes


打包运行
运行菜单 HybridCLR/Generate/All 进行必要的生成操作。这一步不可遗漏!!!
将{proj}/HybridCLRData/HotUpdateDlls/StandaloneWindows64(MacOS下为StandaloneMacXxx)目录下的HotUpdate.dll复制到Assets/StreamingAssets/HotUpdate.dll.bytes，注意，要加.bytes后缀！！！
打开Build Settings对话框，点击Build And Run，打包并且运行热更新示例工程。
如果打包成功，并且屏幕上显示 'Hello,HybridCLR'，表示热更新代码被顺利执行！

测试热更新
修改Assets/HotUpdate/Hello.cs的Run函数中Debug.Log("Hello, HybridCLR");代码，改成Debug.Log("Hello, World");。
运行菜单命令HybridCLR/CompileDll/ActiveBulidTarget重新编译热更新代码。
将{proj}/HybridCLRData/HotUpdateDlls/StandaloneWindows64(MacOS下为StandaloneMacXxx)目录下的HotUpdate.dll复制为刚才的打包输出目录的 XXX_Data/StreamingAssets/HotUpdate.dll.bytes。
重新运行程序，会发现屏幕中显示Hello, World，表示热更新代码生效了！

打包流程
1  运行菜单 HybridCLR/Generate/All 进行必要的生成操作。这一步不可遗漏!!!
2  HybridCLR/Build/BuildAssetsAndCopyToStreamingAssets ==》 这一步是为了再编辑器上使用，如果只在真机上不用这部
3  