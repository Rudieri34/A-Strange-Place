// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vefects/SH_Vefects_VFX_BIRP_Goo_Arm_01"
{
	Properties
	{
		_Color01("Color 01", Color) = (0.3960784,0.4352941,0.09019608,1)
		_Color02("Color 02", Color) = (0.3098039,0.372549,0.1294118,1)
		_Color03("Color 03", Color) = (0.9882354,0.9843138,0.2705882,1)
		_ColorFresnel("Color Fresnel", Color) = (0.08235294,0.8901961,0.8313726,1)
		_HueShift("Hue Shift", Float) = 0
		_Emission("Emission", Float) = 0.1
		_Specular("Specular", Float) = 0.01
		_SmoothnessMin("Smoothness Min", Float) = 0.5
		_SmoothnessMax("Smoothness Max", Float) = 0.96
		[Space(33)][Header(Noise)][Space(13)]_NoiseTexture("Noise Texture", 2D) = "white" {}
		_NoiseTextureScale("Noise Texture Scale", Vector) = (1,1,0,0)
		_NoiseTexturePan("Noise Texture Pan", Vector) = (0.03,-0.1,0,0)
		[Space(33)][Header(Normal)][Space(13)]_NoiseTextureNormal("Noise Texture Normal", 2D) = "white" {}
		_NormalIntensity("Normal Intensity", Float) = 1
		[Space(33)][Header(Distortion)][Space(13)]_NoiseTextureDistortion("Noise Texture Distortion", 2D) = "white" {}
		_NoiseTextureDistortionScale("Noise Texture Distortion Scale", Vector) = (0.5,0.5,0,0)
		_NoiseTextureDistortionPan("Noise Texture Distortion Pan", Vector) = (-0.05,-0.1,0,0)
		_NoiseDistortionLerp("Noise Distortion Lerp", Float) = 0.03
		[Space(33)][Header(Fresnel)][Space(13)]_FresnelScale("Fresnel Scale", Float) = 1
		_FresnelPower("Fresnel Power", Float) = 3
		_FresnelBias("Fresnel Bias", Float) = 0
		_WPOIntensity1("WPO Intensity", Float) = 0.1
		_WPOSineTime("WPO Sine Time", Float) = 1
		_WPOSineIntensity("WPO Sine Intensity", Float) = 0.333
		_WPOMin("WPO Min", Float) = -0.3
		_WPOMax("WPO Max", Float) = 0.3
		_WPOBandErosion("WPO Band Erosion", Float) = 0
		_WPOBandErosionSmoothness("WPO Band Erosion Smoothness", Float) = 1
		_WPOCollapseWidth("WPO Collapse Width", Float) = -1
		_WPOCollapseLerp("WPO Collapse Lerp", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_VERSION 19701
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			INTERNAL_DATA
			float3 worldNormal;
		};

		uniform float _WPOCollapseWidth;
		uniform float _WPOMin;
		uniform float _WPOMax;
		uniform sampler2D _NoiseTexture;
		uniform float2 _NoiseTexturePan;
		uniform float2 _NoiseTextureScale;
		uniform sampler2D _NoiseTextureDistortion;
		uniform float2 _NoiseTextureDistortionPan;
		uniform float2 _NoiseTextureDistortionScale;
		uniform float _NoiseDistortionLerp;
		uniform float _WPOIntensity1;
		uniform float _WPOSineTime;
		uniform float _WPOSineIntensity;
		uniform float _WPOBandErosion;
		uniform float _WPOBandErosionSmoothness;
		uniform float _WPOCollapseLerp;
		uniform sampler2D _NoiseTextureNormal;
		uniform float _NormalIntensity;
		uniform float4 _Color02;
		uniform float4 _Color01;
		uniform float4 _Color03;
		uniform float _HueShift;
		uniform float4 _ColorFresnel;
		uniform float _FresnelBias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform float _Emission;
		uniform float _Specular;
		uniform float _SmoothnessMin;
		uniform float _SmoothnessMax;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 ase_vertexNormal = v.normal.xyz;
			float2 panner68 = ( 1.0 * _Time.y * _NoiseTexturePan + ( v.texcoord.xy * _NoiseTextureScale ));
			float2 panner72 = ( 1.0 * _Time.y * _NoiseTextureDistortionPan + ( v.texcoord.xy * _NoiseTextureDistortionScale ));
			float lerpResult61 = lerp( -1.0 , 1.0 , tex2Dlod( _NoiseTextureDistortion, float4( panner72, 0, 0.0) ).g);
			float2 temp_cast_0 = (lerpResult61).xx;
			float2 lerpResult63 = lerp( float2( 0,0 ) , temp_cast_0 , _NoiseDistortionLerp);
			float2 temp_output_14_0 = ( panner68 + lerpResult63 );
			float4 tex2DNode11 = tex2Dlod( _NoiseTexture, float4( temp_output_14_0, 0, 0.0) );
			float lerpResult79 = lerp( _WPOMin , _WPOMax , tex2DNode11.g);
			float temp_output_156_0 = ( lerpResult79 * _WPOIntensity1 );
			float3 break86 = ( ase_vertexNormal * temp_output_156_0 );
			float saferPower99 = abs( ( 1.0 - v.texcoord.xy.y ) );
			float saferPower97 = abs( v.texcoord.xy.y );
			float smoothstepResult157 = smoothstep( _WPOBandErosion , ( _WPOBandErosion + _WPOBandErosionSmoothness ) , saturate( ( saturate( ( saturate( pow( saferPower99 , 1.0 ) ) * saturate( pow( saferPower97 , 1.0 ) ) ) ) * 3.0 ) ));
			float temp_output_158_0 = saturate( smoothstepResult157 );
			float3 appendResult87 = (float3(break86.x , ( break86.y + ( ( (-1.0 + (sin( ( _Time.y * _WPOSineTime ) ) - -1.0) * (-0.1 - -1.0) / (1.0 - -1.0)) * _WPOSineIntensity ) * temp_output_158_0 ) ) , break86.z));
			float3 lerpResult165 = lerp( ( ase_vertex3Pos * _WPOCollapseWidth ) , appendResult87 , saturate( ( ( 1.0 - v.texcoord.xy.y ) + (-1.0 + (_WPOCollapseLerp - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) ));
			float3 WPO77 = lerpResult165;
			v.vertex.xyz += WPO77;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 panner68 = ( 1.0 * _Time.y * _NoiseTexturePan + ( i.uv_texcoord * _NoiseTextureScale ));
			float2 panner72 = ( 1.0 * _Time.y * _NoiseTextureDistortionPan + ( i.uv_texcoord * _NoiseTextureDistortionScale ));
			float lerpResult61 = lerp( -1.0 , 1.0 , tex2D( _NoiseTextureDistortion, panner72 ).g);
			float2 temp_cast_0 = (lerpResult61).xx;
			float2 lerpResult63 = lerp( float2( 0,0 ) , temp_cast_0 , _NoiseDistortionLerp);
			float2 temp_output_14_0 = ( panner68 + lerpResult63 );
			float3 lerpResult15 = lerp( float3(0,0,1) , tex2D( _NoiseTextureNormal, temp_output_14_0 ).rgb , _NormalIntensity);
			float3 normal18 = lerpResult15;
			o.Normal = normal18;
			float4 tex2DNode11 = tex2D( _NoiseTexture, temp_output_14_0 );
			float3 lerpResult34 = lerp( _Color02.rgb , _Color01.rgb , tex2DNode11.g);
			float3 lerpResult30 = lerp( lerpResult34 , _Color03.rgb , saturate( ( ( tex2DNode11.g - 0.9 ) / 2.0 ) ));
			float temp_output_40_0 = saturate( ( ( pow( tex2DNode11.g , 3.0 ) - 0.5 ) * 2.0 ) );
			float3 lerpResult36 = lerp( ( lerpResult30 * 0.7 ) , lerpResult30 , temp_output_40_0);
			float3 hsvTorgb44 = RGBToHSV( lerpResult36 );
			float3 hsvTorgb43 = HSVToRGB( float3(( hsvTorgb44.x + _HueShift ),hsvTorgb44.y,hsvTorgb44.z) );
			float3 color20 = hsvTorgb43;
			o.Albedo = color20;
			float3 temp_cast_1 = (0.0).xxx;
			float3 ase_worldPos = i.worldPos;
			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float fresnelNdotV53 = dot( mul(ase_tangentToWorldFast,normal18), ase_viewDirWS );
			float fresnelNode53 = ( _FresnelBias + _FresnelScale * pow( max( 1.0 - fresnelNdotV53 , 0.0001 ), _FresnelPower ) );
			float3 lerpResult54 = lerp( temp_cast_1 , _ColorFresnel.rgb , saturate( fresnelNode53 ));
			float3 emission50 = ( lerpResult54 * _Emission );
			o.Emission = emission50;
			float3 temp_cast_2 = (_Specular).xxx;
			o.Specular = temp_cast_2;
			float lerpResult48 = lerp( _SmoothnessMin , _SmoothnessMax , temp_output_40_0);
			o.Smoothness = lerpResult48;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
