
Shader "Dissove/DissoveSpreadTop"
{
	Properties{
		_Diffuse("Diffuse", Color) = (1,1,1,1)
		_DissolveColorA("Dissolve Color A", Color) = (0,0,0,0)
		_DissolveColorB("Dissolve Color B", Color) = (1,1,1,1)
		_MainTex("Base 2D", 2D) = "white"{}
		_DissolveMap("DissolveMap", 2D) = "white"{}
		_DissolveThreshold("DissolveThreshold", Range(0,1)) = 0
		_ColorFactorA("ColorFactorA", Range(0,1)) = 0.7
		_ColorFactorB("ColorFactorB", Range(0,1)) = 0.8

		_FlyThreshold("FlyThreshold", Range(0,1)) = 0
		_FlyFactor("FlyFactor", Range(0,1)) = 0
	}
	
	CGINCLUDE
	#include "Lighting.cginc"
	uniform fixed4 _Diffuse;
	uniform fixed4 _DissolveColorA;
	uniform fixed4 _DissolveColorB;
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_ST;
	uniform sampler2D _DissolveMap;
	uniform float _DissolveThreshold;
	uniform float _ColorFactorA;
	uniform float _ColorFactorB;

	uniform float _FlyThreshold;
	uniform float _FlyFactor;
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float3 worldNormal : TEXCOORD0;
		float2 uv : TEXCOORD1;
	};
	

	//要让模型向上运动，分为几种情况。第一种是模型空间的向上，这种最简单，
	//直接在appdata_base传入的vertex的y轴坐标修改一下就可以了，不过这种情况下模型只会沿着所谓模型的Y轴进行偏移，
	//如果运气好，Y轴正好在头顶，那么效果是正常的，但是如果Y轴不是头顶，
	//我们所谓的向上就不一定向哪里了；第二种是在屏幕空间的向上，比如我们上面实验的角度上，
	//我们可以直接在计算完MVP之后得到的坐标的y方向进行偏移，相对也比较简单，但是这种情况也会有一定的问题，
	//比如我们相机换了个角度看的话，模型的偏移方向仍然是在屏幕上向上移动，这种效果也是不对的；第三种是在世界坐标向上，
	//世界空间只有一个，而且所有的模型都共享这个世界空间，我们在这个空间上进行偏移操作的话，所有的模型不管自身坐标系是怎样的，
	//也不管相机观察角度是怎样的，都会在世界空间的y方向进行偏移，这种最为符合我们现实世界，正所谓世界就在那，如果观察的角度不同，
	//那么得到的结果也不同（恩恩，写个shader竟然还上升到哲学层面了=。=）。那就看一下在世界空间上进行模型偏移的操作。
	//这个就没有直接在模型空间或者屏幕空间进行操作方便了，因为这个操作是穿插在MVP变换中间的，
	//我们需要先.........（等等，我好像想到了点什么），让模型从世界空间上向一个方向运动的话，不就是改Transform的Y轴方向吗（我是不是写shader写傻了=。=）！！！
	//如果我在shader里面进行这步操作的话，模型有N个顶点，那么这个是个O（N）复杂度的运算，而如果我直接在脚本层面让模型朝Y方向位移一段的话，
	//这个时间复杂度就是O（1）的（类似变换矩阵之类的都是在每帧针对每个物体计算好再传递给shader中进行运算，也就是shader中的uniform类型的变量）。
	//所以，个人感觉实现这个的最好方法就是直接挂个脚本，按照时间大于某个值直接把模型在Y方向上向上平移一下就好了。

	//总之，shader虽然强大，可以做出很多很有意思的效果，但是，并不是所有的效果都适合在shader中做。毕竟如果我们在脚本里面计算的话，
	//对于一个模型来说，只需要计算一次。但是如果放到vertexshader中就需要模型顶点数次运算，
	//放到fragment shader的话就需要光栅化后可见像素数次运算，性能还是不一样的。

	v2f vert(appdata_base v)
	{
		v2f o;
		//在世界空间向上移动 (向上移动在模型空间和视觉空间都不合适) --> 优化手段: 放到脚本里向上移动，这样效率会提高很多
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; 
		float offset = saturate(_DissolveThreshold - _FlyThreshold) * _FlyFactor;
		worldPos.y += offset;
		o.pos = UnityWorldToClipPos(worldPos);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
		return o;
	}
	
	fixed4 frag(v2f i) : SV_Target
	{
		//采样Dissolve Map
		fixed4 dissolveValue = tex2D(_DissolveMap, i.uv);
		//小于阈值的部分直接discard
		if (dissolveValue.r < _DissolveThreshold)
		{
			discard;
		}
		//Diffuse + Ambient光照计算
		fixed3 worldNormal = normalize(i.worldNormal);
		fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
		//fixed3 lambert = saturate(dot(worldNormal, worldLightDir));
		fixed3 lambert = dot(worldNormal, worldLightDir) * 0.5 + 0.5;
		fixed3 albedo = lambert * _Diffuse.xyz * _LightColor0.xyz + UNITY_LIGHTMODEL_AMBIENT.xyz;
		fixed3 color = tex2D(_MainTex, i.uv).rgb * albedo;
		//这里为了比较方便，直接用color和最终的边缘lerp了
		float lerpValue = _DissolveThreshold / dissolveValue.r;
		if (lerpValue > _ColorFactorA)
		{
			if (lerpValue > _ColorFactorB)
				return _DissolveColorB;
			return _DissolveColorA;
		}
		return fixed4(color, 1);
	}
	ENDCG
	
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag	
			ENDCG
		}
	}
	FallBack "Diffuse"
}