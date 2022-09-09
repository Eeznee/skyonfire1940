#ifndef SUN_MOON_INCLUDED
#define SUN_MOON_INCLUDED

#include "JCommon.cginc"

void CalculateSunMoonColorTextured(
	float4 localPos,
	sampler2D tex, float4x4 posToUvMatrix,
	fixed4 tintColor,
	float size, fixed softEdge, fixed glow,
	float4 direction,
	out fixed4 color)
{
	fixed3 sunPos = -direction;
	fixed3 rayDir = localPos.xyz;
	fixed3 sunPlaneOrigin = sunPos;
	fixed3 sunPlaneNormal = sunPos;

	fixed rayLength = dot(sunPlaneOrigin, sunPlaneNormal)/(dot(rayDir, sunPlaneNormal)+0.000001);
	fixed3 intersectionPoint = rayDir*rayLength;
	fixed sqrDistanceToSun = SqrDistance(intersectionPoint, sunPos);
	fixed fSize = 1 - InverseLerpUnclamped(0, size*size*0.25, sqrDistanceToSun);
	fixed fGlow = 1 - InverseLerpUnclamped(0, size*size*400*glow*glow, sqrDistanceToSun);
	fGlow = saturate(fGlow);

	fixed4 clear = fixed4(0,0,0,0);
	fixed4 white = fixed4(1,1,1,1);

	fixed4 glowColor = fixed4(tintColor.xyz, fGlow*fGlow*fGlow*fGlow*fGlow*fGlow*glow);
	float4 uvSpacePos = mul(posToUvMatrix, localPos);
	fixed4 texColor = tex2D(tex, uvSpacePos.xy - float2(0.5, 0.5));
	texColor.a = lerp(0, texColor.a, SqrMagnitude(uvSpacePos.xy) <= 0.25);
	float texAlpha = texColor.a;

	float fSoftEdge = saturate(lerp(0, 1/(softEdge+0.0000001), fSize));
	texColor = lerp(glowColor, texColor, fSoftEdge*texAlpha);
	texColor.a = texAlpha*fSoftEdge;

	color = texColor + glowColor*glowColor.a;
	color *= tintColor;
	color.a = saturate(color.a);

	float dotProduct = dot(localPos.xyz, sunPos);
	color = lerp(clear, color, dotProduct >= 0);
}

void CalculateSunMoonColor(
	float4 localPos,
	fixed4 tintColor,
	float size, fixed softEdge, fixed glow,
	float4 direction,
	out fixed4 color)
{
	fixed3 sunPos = -direction;
	fixed3 rayDir = localPos.xyz;
	fixed3 sunPlaneOrigin = sunPos;
	fixed3 sunPlaneNormal = sunPos;

	fixed rayLength = dot(sunPlaneOrigin, sunPlaneNormal)/(dot(rayDir, sunPlaneNormal)+0.000001);
	fixed3 intersectionPoint = rayDir*rayLength;
	fixed sqrDistanceToSun = SqrDistance(intersectionPoint, sunPos);
	fixed fSize = 1 - InverseLerpUnclamped(0, size*size*0.25, sqrDistanceToSun);
	fixed fGlow = 1 - InverseLerpUnclamped(0, size*size*400*glow*glow, sqrDistanceToSun);
	fGlow = saturate(fGlow);

	fixed4 clear = fixed4(0,0,0,0);
	fixed4 white = fixed4(1,1,1,1);

	fixed4 texColor = white;
	fixed4 glowColor = fixed4(tintColor.xyz, fGlow*fGlow*fGlow*fGlow*fGlow*fGlow*glow);
	fixed fSoftEdge = saturate(lerp(0, 1/(softEdge+0.0000001), fSize));
	texColor = lerp(glowColor, texColor, fSoftEdge);

	color = texColor + glowColor*glowColor.a*glowColor.a;
	color *= tintColor;
	color.a = saturate(color.a);

	fixed dotProduct = dot(localPos.xyz, sunPos);
	color = lerp(clear, color, dotProduct >= 0);
}

void CalculateSunMoonColorBaked(
	float4 localPos, float4x4 rotationMatrix,
	samplerCUBE sunCubemap,
	out fixed4 color)
{
	localPos = mul(rotationMatrix, float4(localPos.xyz,0));
	color = texCUBE(sunCubemap, localPos.xyz);
}
#endif