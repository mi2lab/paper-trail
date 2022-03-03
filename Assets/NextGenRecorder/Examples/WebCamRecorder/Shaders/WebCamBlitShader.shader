Shader "Next Gen Recorder/WebCam Blit Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Size ("Size", Vector) = (1, 1, 0, 0)
        _Angle ("Angle", float) = 0
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

            float4 _Size;
            float _Angle;

			v2f vert (appdata v)
			{
				v2f o;
				float4 vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = vertex;
                o.vertex.x = cos(_Angle) * vertex.x -  sin(_Angle) * vertex.y;
                o.vertex.y = cos(_Angle) * vertex.y + sin(_Angle) * vertex.x;
                o.vertex.xy = sign(o.vertex.xy) * _Size.xy;
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
