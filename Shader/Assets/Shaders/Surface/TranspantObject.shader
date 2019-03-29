Shader "CookBookCustom/TranspantObject" {
	Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
       
    }
    SubShader
    {
    	Tags { "Queue"="Transparent" "RenderType"="Transparent" }
    
        CGPROGRAM
        #pragma surface surf Lambert alpha exclude_path:prepass noforwardadd
		#pragma target 3.0

        sampler2D _MainTex;
            
	    struct Input 
		{
			half2 uv_MainTex;
		};
           
        
        void surf (Input IN, inout SurfaceOutput o)
        {                
            half4 texCol = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = texCol.rgb;
            o.Alpha = texCol.a;
        }
        
        ENDCG     
    }
    
    FallBack "Unlit/Transparent"
}
