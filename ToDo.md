# Unity3D

1 光照模型添加带有纹理得到完整例子 shaders/commmon中的BumpedDiffuse和BumpedSpecular 只是不带高光贴图的
以下几种情况贴图
{
	1 只带漫反射贴图
	2 漫反射 + 法线  ==》ok
	3 漫反射 + 法线 +  高光


	if 支持高光 and 支持漫反射 then
		{
			1 只带漫反射贴图
			2 漫反射 + 法线  ==》ok
			3 漫反射 + 法线 +  高光
		}
	elseif 不支持高光 and 支持漫反射  then
	   {
	   		1 只带漫反射贴图
	   		2  漫反射 + 法线  ==》 ok
	   }
	elseif 不支持高光 and 不支持漫反射 then
	      1 只带漫反射贴图
	end 


	阴影 todo
}

2 opengl里一些2d的常见shader用Unity实现一遍

3 后期处理的一些shader，也可以放到单独的物体上，但是必须打开深入写入 自己弄一遍

4 用程序动态给gameobject加材质然后用程序修改shader参数

5 渲染透明图片特别麻烦，==》 再pass里设置一个透明度丢弃 统一改下

6 书里的的反射公式有点问题？？折射公式正确

7 传一个颜色矩阵联系下