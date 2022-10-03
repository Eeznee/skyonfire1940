
Shader "Sof/Reflector"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TexScale("Texture Scale", Range(0.01, 0.5)) = 0.1
		_Emission("Emission", Range(0.5,10)) = 1.8
		_Loss("Loss", Range(0,10)) = 1
		_Color("Tint Color",Color) = (1,1,1,1)
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
			Blend SrcAlpha OneMinusSrcAlpha
			LOD 100
			ZWrite On

			Pass
			{
				CGPROGRAM
				// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members uv)
				//#pragma exclude_renderers d3d11
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float3 tangent : TANGENT;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float3 pos : TEXCOORD1;
					float3 normal : NORMAL;
					float3 tangent : TANGENT;
				};

				sampler2D _MainTex;
				float _TexScale;
				float _Emission;
				float _Loss;
				float4 _Color;
				float4 _MainTex_ST;

				v2f vert(appdata v) {
					v2f o;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.pos = UnityObjectToViewPos(v.vertex);         //transform vertex into eye space
					o.normal = mul(UNITY_MATRIX_IT_MV, v.normal);   //transform normal into eye space
					o.tangent = mul(UNITY_MATRIX_IT_MV, v.tangent); //transform tangent into eye space
					return o;
				}

				fixed4 frag(v2f i) : SV_Target {
					float3 normal = normalize(i.normal);    //get normal of fragment
					float3 tangent = normalize(i.tangent);  //get tangent
					float3 cameraDir = normalize(i.pos);    //get direction from camera to fragment, normalize(i.pos - float3(0, 0, 0))

					float3 offset = cameraDir + normal;     //calculate offset from two points on unit sphere, cameraDir - -normal

					float3x3 mat = float3x3(tangent, cross(normal, tangent), normal);
					offset = mul(mat, offset);  //transform offset into tangent space

					float2 uv = offset.xy / _TexScale + float2(0.5, 0.5);
					float dis = length(uv - i.uv);
					float emi = _Emission * _Emission * (1-dis*_Loss);

					return (tex2D(_MainTex, uv) * _Color) * emi;  //shift sample to center of texture
				}
				ENDCG
			}
		}
}