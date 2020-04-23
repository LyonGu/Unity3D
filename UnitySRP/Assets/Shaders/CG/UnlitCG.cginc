#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED

    float4 _Color;
    struct VertexInput {
        float4 pos : POSITION;
    };

    struct VertexOutput {
        float4 clipPos : SV_POSITION;
    };

    VertexOutput UnlitPassVertex (VertexInput input) {
        VertexOutput output;
	    output.clipPos = UnityObjectToClipPos(input.pos);
        return output;
    }

    float4 UnlitPassFragment (VertexOutput input) : SV_TARGET {
        return _Color;
    }

#endif // MYRP_UNLIT_INCLUDED
