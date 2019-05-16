Shader "Common/BaseTranspantObjectTest" {
	Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
       
    }
    SubShader
    {
    	Tags { "Queue"="Transparent" "RenderType"="Transparent" }
    
        CGPROGRAM
        //使用alpha混合
        #pragma surface surf Lambert alpha exclude_path:prepass noforwardadd
		#pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
            
	    struct Input 
		{
			half2 uv_MainTex;
		};
           
        
        void surf (Input IN, inout SurfaceOutput o)
        {                
            fixed4 texCol = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = texCol.rgb;
            o.Alpha = texCol.a;
        }
        
        ENDCG     
    }
    
    FallBack "Unlit/Transparent"
}
