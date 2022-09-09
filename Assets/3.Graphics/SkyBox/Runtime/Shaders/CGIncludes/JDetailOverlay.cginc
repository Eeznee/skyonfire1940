#ifndef DETAIL_OVERLAY_INCLUDED
#define DETAIL_OVERLAY_INCLUDED

void CalculateDetailOverlayColor(
	float4 localPos,
	samplerCUBE cubemap,
	fixed4 tint,
	out fixed4 color)
{
	color = texCUBE(cubemap, localPos.xyz);
	color *= tint;
}

void CalculateDetailOverlayColor(
	float4 localPos,
	samplerCUBE cubemap,
	fixed4 tint,
	float rotationSpeed,
	out fixed4 color)
{
	float angle = rotationSpeed*_Time.y;
	float sinY = sin(radians(angle));
	float cosY = cos(radians(angle));
	float3x3 ry = float3x3(cosY,  0,  sinY,
							  0,  1,     0,
						  -sinY,  0,  cosY);
	localPos.xyz = mul(ry, localPos.xyz);

	color = texCUBE(cubemap, localPos.xyz);
	color *= tint;
}

#endif