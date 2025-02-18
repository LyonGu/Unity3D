﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "UnityEffects/ShadowProjector" {
	Properties {
		_ShadowTex ("ShadowTex", 2D) = "gray" {}
		_bulerWidth ("BulerWidth", float) = 1
		_shadowfactor ("Shadowfactor", Range(0,1)) = 0.5
		_ShadowMask ("ShadowMask",2D) = "white"{}
	}
	SubShader {
		Tags { "Queue"="AlphaTest+1" }
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos:POSITION;
				float4 sproj:TEXCOORD0;
			};

			float4x4 unity_Projector;  //Unity默认矩阵
			sampler2D _ShadowTex;
			sampler2D _ShadowMask;
			uniform half4 _ShadowTex_TexelSize;
			float _bulerWidth;
			float _shadowfactor;

			v2f vert(float4 vertex:POSITION){
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.sproj = mul(unity_Projector, vertex);
				//贴图的采样uv是由unity_Projector 乘 v.vertex实现的，采样的时候用tex2Dproj(_MainTex, i.texc);采样，
				//后者等价于tex2D(_MainTex, i.texc.xy/i.texc.z)。
				return o;
			}

			float4 frag(v2f i):COLOR{
				
				//使用tex2Dproj和UNITY_PROJ_COORD进行采样
				//如果i.sproj为float3 等价与tex2D(_ShadowTex, i.sproj.xy/i.sproj.z)
				//如果i.sproj为float4 等价与tex2D(_ShadowTex, i.sproj.xy/i.sproj.w)
				half4 shadowCol = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.sproj));
				half maskCol = tex2Dproj(_ShadowMask, UNITY_PROJ_COORD(i.sproj)).r;
				half a = (shadowCol * maskCol).a;

				float4 uv4= UNITY_PROJ_COORD(i.sproj);
				float2 uv = uv4.xy / uv4.w ;

				//blur来柔化边缘
				a += tex2D(_ShadowTex, uv + _ShadowTex_TexelSize.xy * _bulerWidth * float2(1,0)).a;
				a += tex2D(_ShadowTex, uv + _ShadowTex_TexelSize.xy * _bulerWidth * float2(0,1)).a;
				a += tex2D(_ShadowTex, uv + _ShadowTex_TexelSize.xy * _bulerWidth * float2(-1,0)).a;
				a += tex2D(_ShadowTex, uv + _ShadowTex_TexelSize.xy * _bulerWidth * float2(0,-1)).a;

				a = a/5;
				if(a > 0)
				{
					return  float4(1,1,1,1) * (1 - _shadowfactor * a);
				}
				else
				{
					return float4(1,1,1,1) ;
				}
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
