Shader "Shaders/Chapter12/WaterWaveEffect"
{	
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		//距离系数
		_distanceFactor("Distance Factor", Range(1,100)) = 60

		 //时间系数
		_timeFactor ("Time Factor", Range(-50,50)) = -30

		//sin函数结果系数
		_totalFactor("Total Factor", Range(0,1)) = 1

		//波纹宽度
		_waveWidth("WaveWidth", Range(0,1)) = 1

		//波纹移动的距离
		_curWaveDis("CurWaveDis", Float) = 0
	}
	SubShader
	{

		/*
			引用https://blog.csdn.net/puppet_master/article/details/52975666
		*/
		CGINCLUDE
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _distanceFactor;
			float _timeFactor;
			float _totalFactor;
			float _waveWidth;
			float _curWaveDis;
			float4 _startPos;
			int   _isLoop;

			struct a2v{
				float4 vertex: POSITION;
				float2 texcoord: TEXCOORD0;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(a2v i)
			{
				v2f o;
				o.pos =  UnityObjectToClipPos(i.vertex);
				o.uv = i.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{	
				//DX下纹理坐标反向问题
				#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
						_startPos.y = 1 - _startPos.y;
				#endif

				//计算uv到中间点的向量(向外扩，反过来就是向里缩)
				fixed2 dv = _startPos.xy - i.uv;

				//按照屏幕长度进行缩放
				dv = dv * float2(_ScreenParams.x / _ScreenParams.y, 1);

				//计算像素点距中点的距离
				float dis = sqrt(dv.x * dv.x + dv.y * dv.y);

				/*
					用sin函数计算出波形的偏移值factor
					dis在这里都是小于1的，所以我们需要乘以一个比较大的数，比如60，这样就有多个波峰波谷sin函数是（-1，1）的值域，我们希望偏移值很小，所以这里我们缩小100倍，据说乘法比较快,so...
				*/

				float sinFactor = sin(dis * _distanceFactor + _Time.y * _timeFactor) * _totalFactor * 0.01;

				/*
					距离当前波纹运动点的距离，如果小于waveWidth才予以保留，否则已经出了波纹范围，factor通过clamp设置为0
				*/

				float discardFactor = clamp(_waveWidth - abs(_curWaveDis - dis),0,1);

				//归一化
				float2 dv1 = normalize(dv);

				//计算每个像素uv的偏移值
				float2 offset = dv1 * sinFactor;
				if(!_isLoop)
				{
					offset = offset * discardFactor;
				}

				//像素采样时偏移offset
				float2 uv = offset + i.uv;

				return tex2D(_MainTex, uv);

			}

		ENDCG

		Pass
		{
			//标配
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			
			ENDCG

		}
	}
	Fallback Off
}
