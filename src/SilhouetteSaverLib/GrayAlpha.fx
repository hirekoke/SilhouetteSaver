//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- GrayAlphaEffect
//
//--------------------------------------------------------------------------------------

float GrayishValue : register(C0);
float Inverse : register(C1);
float MediumValue : register(C2);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
   float4 color = tex2D(implicitInputSampler, uv);
   
	float3x3 RgbToYuvMx = {
		0.299,  0.587,  0.114,
		-0.169, -0.331,  0.500,
		0.500, -0.419, -0.081
	};

	float3x3 YuvToRgbMx = {
		1.000,  0.000,  1.402,
		1.000, -0.344, -0.714,
		1.000,  1.772,  0.000
	};

   float3 yuv = mul(RgbToYuvMx, color.rgb);

   if(Inverse > 0.51) {
     yuv.x = 1.0f - yuv.x;
   }
   
   if(yuv.x > MediumValue) {
     yuv.x = lerp(0.5, 1.0, (yuv.x - MediumValue) * 2);
   } else {
     yuv.x = lerp(0.5, 0.0, (MediumValue - yuv.x) * 2);
   }
   color.rgb = mul(YuvToRgbMx, yuv.xyz);

   float black = 0.0f;
   color.rgb = lerp(color.rgb, black.xxx, GrayishValue.x);

   color.a = 1.0f - yuv.x;
   return color;
}


