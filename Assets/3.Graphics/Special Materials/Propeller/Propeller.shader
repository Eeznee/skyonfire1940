Shader "SOF/Propeller"
{
	Properties
	{
		_LowRpmTex("Low Rpm Texture", 2D) = "white" {}		// = 400/3
		_MidRpmTex("Mid Rpm Texture", 2D) = "white" {}		// = 800/3
		_HighRpmTex("High Rpm Texture", 2D) = "white" {}	// = 1200/3
		_Color("Color", Color) = (0,0,0,1)
		_TipColor("Tip Color",Color) = (1,1,0,1)
		_Transparency("Transparency", Range(0,1)) = 0.2
		_TipLimit("Tip Limit", Range(0,1)) = 0.9
		[PerRendererData]_Rpm("Rpm",float) = 3000
		[PerRendererData]_CameraAngle("Angle with Camera",float) = 0

	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}

			LOD 100
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

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
					float3 normal : NORMAL;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				sampler2D _LowRpmTex;
				sampler2D _MidRpmTex;
				sampler2D _HighRpmTex;

				float4 _LowRpmTex_ST;
				float4 _Color;
				float4 _TipColor;
				float _Transparency;
				float _TipLimit;
				float _Rpm;
				float _CameraAngle;

				fixed3 viewDir;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _LowRpmTex);

					return o;
				}
				fixed4 frag(v2f i) : SV_Target
				{
					float alpha = _Color.a * 0.5 + tex2D(_HighRpmTex, i.uv).a * 0.5;
					if(_Rpm < 200){
						alpha = 0;
					}
					else if (_Rpm < 400) {
						alpha = lerp(0, tex2D(_LowRpmTex, i.uv).a, _Rpm / 200 - 1);
					}
					else if (_Rpm < 800) {
						alpha = lerp(tex2D(_LowRpmTex, i.uv).a, tex2D(_MidRpmTex, i.uv).a,_Rpm / 400 - 1);
					}
					else if (_Rpm < 1200) {
						alpha = lerp(tex2D(_MidRpmTex, i.uv).a, tex2D(_HighRpmTex, i.uv).a,_Rpm / 400 - 2);
					}
					else if (_Rpm < 1600) {
						alpha = lerp(tex2D(_HighRpmTex, i.uv).a, alpha, _Rpm / 400 - 3);
					}
					fixed4 col = (length(i.uv - float2(0.5,0.5)) * 2 > _TipLimit) ? _TipColor : _Color;
					col.w = alpha * _Transparency;
					col.w *= lerp(1, 0.8 / _Transparency, _CameraAngle);

					return col;
				}
				ENDCG
			}
		}
}
