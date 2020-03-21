Shader "Common/Wave" {
	Properties {
		_MainTex ("Albedo (RGBA)", 2D) = "white" {}
        _AlphaTest("AlphaTest", Range(0, 1)) = 0

		_AlphaTex ("Albedo (A)", 2D) = "white" {}

        _MetallicTex("Metallic", 2D) = "white"{}  //金属图
        _Metallic ("Intensity", Range(0,1)) = 0.0
        _SmoothnessTex("Smoothness", 2D) = "white"{} //平滑度
        _Smoothness ("Intensity", Range(0,1)) = 0

        _NormalTex("Normal", 2D) = "bump"
        _Normal("Intensity", float) = 1

        _Speed("Speed", float) = 1
        _Frequency("Frequency", float) = 1
        _Amplitude("Amplitude", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue" = "AlphaTest"}
		LOD 200

		CULL Off

		//渲染类型为不透明
		//渲染顺序为透明度测试
		//CULL off：关闭模型面的剔除，这样模型的背面也会被渲染
		//使用默认的Standard光照模型
		//我们使用自定义的顶点函，命名为vert
		//修改了顶点函数之后，投影会出现跟模型不一致的现象，所以使用addshadow指令重新计算正确投影
		//透明类型为alphatest，然后把一开始定义的_AlphaTest属性传递过来，小于这个数值的像素会被裁切，大于这个数值的像素会被保留

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow alphatest:_AlphaTest
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MetallicTex;
        sampler2D _SmoothnessTex;
        sampler2D _NormalTex;
		sampler2D _AlphaTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Smoothness;
        half _Metallic;
        float _Normal;

        half _Speed;
        half _Frequency;
        half _Amplitude;

		//这里使用了CG里提供的时间变量Time的y分量乘以_Speed属性，得到新的时间变量，以后我们就可以在材质的Inspector通过调节Speed属性控制飞毯的速度了
		//顶点z轴的坐标与_Frequency相乘，相当于对z轴进行了一个范围的缩放。然后跟time变量相加，是为了把顶点Z轴方向与时间结合在一起变为动态变量。经过sin处理之后，可以得出一个随时间推迟而循环浮动的偏移值。最后乘以_Amplitude属性可以控制偏移的大小
		//顶点的y方向加上这个偏移值，实现垂直方向上的上下浮动效果

		void vert(inout appdata_full v)
		{
			float time = _Time.y * _Speed;
			float offset = (sin(time + v.vertex.z * _Frequency)) * _Amplitude;
			v.vertex.y += offset;
		}

		//金属工作流
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = tex2D(_AlphaTex, IN.uv_MainTex).r;
			o.Metallic = tex2D(_MetallicTex,IN.uv_MainTex) * _Metallic;
			o.Smoothness = tex2D(_SmoothnessTex, IN.uv_MainTex) *  _Smoothness;

			fixed3 n = UnpackNormal(tex2D(_NormalTex, IN.uv_MainTex)).rgb;
            n.x *= _Normal;
            n.y *= _Normal;
			n.z = sqrt(1.0 - saturate(dot(n.xy, n.xy)));
            o.Normal = n;

		}
		ENDCG
	}
	FallBack "Diffuse"
}
