Shader "FX/StencilMask" {
	Properties{

		_ID("Mask ID", Int) = 1
	}
		SubShader{
			Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+1" }
			ColorMask 0
			ZWrite off			
			Stencil{
				Ref[_ID]
				Comp always  //默认always
				Pass replace //默认Keep
				//Fail Keep  模板测试失败 默认保持不变
				//ZFail Keep 深度测试失败 默认保持不变
			}
		Pass{
			CGINCLUDE
		struct appdata {
			float4 vertex : POSITION;
		};
		struct v2f {
			float4 pos : SV_POSITION;
		};

	
		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
		}
		half4 frag(v2f i) : SV_Target{

			return half4(1,1,1,1);
		}
			ENDCG

	}
}
}