#ifndef HORIZON_CLOUD_INCLUDED
#define HORIZON_CLOUD_INCLUDED

#include "JCommon.cginc"

void CalculateHorizonCloudColor(
	float4 localPos,
	fixed4 cloudColor,
	fixed cloudStart, fixed cloudEnd,
	fixed cloudSize, fixed cloudStep,
	fixed animationSpeed,
	out fixed4 color)
{
	fixed fadeBottom = InverseLerpUnclamped(cloudStart, 0, localPos.y);
	fixed fadeTop = InverseLerpUnclamped(cloudEnd, 0, localPos.y);
	fixed fade = saturate(lerp(fadeBottom, fadeTop, localPos.y >= 0));
	fade = fade*fade;
	
	fixed loop;
	#if SHADER_API_MOBILE
		loop = 1;
	#else
		loop = 2;
	#endif

	fixed noise = 0;
	fixed sample0 = 0;
	fixed sample1 = 0;
	fixed noiseSize = cloudSize + 0.0001;
	fixed noiseAmp = 1;
	fixed sign = -1;
	fixed2 span = animationSpeed*_Time.y*0.0001;
	for (fixed i=0; i<loop; ++i)
	{
		sample0 = CloudTexLod0((localPos.xy)/noiseSize + sign*span)*noiseAmp;
		sample1 = CloudTexLod0((localPos.yz)/noiseSize + sign*span)*noiseAmp;
		noise += (sample0 + sample1)*0.5;
		noiseSize *= 0.5;
		noiseAmp *= 0.5;
		sign *= -1;
	}

	noise = noise*0.5 + 0.5;

	#if ALLOW_STEP_EFFECT
		noise = StepValue(noise, cloudStep);
	#endif

	color = fixed4(1, 1, 1, fade*noise*2)*cloudColor;
}

#endif