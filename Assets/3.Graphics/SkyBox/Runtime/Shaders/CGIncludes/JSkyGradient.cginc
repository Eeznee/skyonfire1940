#ifndef SKY_GRADIENT_INCLUDED
#define SKY_GRADIENT_INCLUDED

#include "JCommon.cginc"

void CalculateSkyGradientColor(
	float4 localPos,
	fixed4 skyColor, fixed4 horizonColor, fixed4 groundColor, 
	fixed horizonThickness, fixed horizonExponent, fixed horizonStep,
	out fixed4 skyBlendColor, out fixed4 horizonBlendColor)
{
	skyBlendColor = lerp(groundColor, skyColor, localPos.y > 0);
	horizonThickness *= lerp(0.25, 1, localPos.y > 0);
	
	#if ALLOW_STEP_EFFECT
	localPos.y = StepValue(localPos.y, horizonStep);
	#endif

	fixed horizonBlendFactor = saturate(1 - InverseLerpUnclamped(0, horizonThickness, abs(localPos.y)));
	horizonBlendFactor = pow(horizonBlendFactor, horizonExponent);
	//horizon = lerp(color, horizonColor, horizonBlendFactor);
	horizonBlendColor = fixed4(horizonColor.xyz, horizonBlendFactor*horizonColor.a);
}

#endif