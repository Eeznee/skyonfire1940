#ifndef	STARS_INCLUDED
#define STARS_INCLUDED

#include "JCommon.cginc"

void CalculateStarsColor(
	float4 localPos,
	float starsStartPosition, float starsEndPosition,
	fixed4 starsColor, fixed starsOpacity,
	float starsDensity,
	float starsSize,
	fixed starsGlow,
	fixed starsTwinkle,
	out fixed4 color)
{
	float cellCount = starsDensity*50;
	float cellSize = 1/cellCount;

	float3 cellCenter = floor(localPos.xyz/cellSize)*cellSize + 0.5*cellSize*float3(1,1,1);
	float rand = RandomValue(cellCenter.xyz) - 0.5;
	float3 starPos = cellCenter + float3(1,1,1)*cellSize*rand;
	float sqrDistance = SqrDistance(localPos.xyz, starPos);
	float sqrDistanceThreshold = 0.00001*starsSize*starsSize;

	fixed4 clear = fixed4(0,0,0,0);
	fixed4 white = fixed4(1,1,1,1);

	color = lerp(clear, white, sqrDistance <= sqrDistanceThreshold);
	color *= starsColor;
	color.a *= 2*(1 + starsGlow)*(rand + 0.5);
	float wave = TriangleWave(rand*_Time.y*starsTwinkle)*0.5 + 0.5;
	color.a *= lerp(0.5, 1, (1-wave));
	color.a *= starsOpacity;

	color = lerp(clear, color, cellCenter.y >= starsStartPosition);
	color = lerp(clear, color, cellCenter.y <= starsEndPosition);
	color = lerp(clear, color, rand <= starsDensity);
}

void CalculateStarsColorBaked(
	float4 localPos,
	samplerCUBE starsCubemap,
	sampler2D starsTwinkleMap,
	fixed starsOpacity,
	out fixed4 color)
{
	float2 span = float2(1,1)*_Time.y*0.1;
	fixed twinkle = tex2D(starsTwinkleMap, localPos.xy + span).r;

	color = texCUBE(starsCubemap, localPos.xyz);
	color.a *= twinkle;
	color.a *= starsOpacity;
}

#endif