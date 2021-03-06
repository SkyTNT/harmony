// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/AQUAS/Underwater/Fog Simple"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_DistortionLens("DistortionLens", 2D) = "white" {}
		_Distortion("Distortion", Range( 0 , 0.05)) = 0.05
		_DropletNormals("Droplet Normals", 2D) = "white" {}
		_DropletCutout("DropletCutout", 2D) = "white" {}
		[Toggle]_WetLensDistortionSwitch("WetLensDistortionSwitch", Float) = 1
		_Wetness("Wetness", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			
			CGPROGRAM

#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
		//only defining to not throw compilation error over Unity 5.5
		#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform sampler2D _MainTex;
			uniform sampler2D _DropletCutout;
			uniform float4 _DropletCutout_ST;
			uniform sampler2D _DropletNormals;
			uniform float4 _DropletNormals_ST;
			uniform float _Wetness;
			uniform sampler2D _DistortionLens;
			uniform float4 _DistortionLens_ST;
			uniform float _Distortion;
			uniform float _WetLensDistortionSwitch;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord1 = screenPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				float3 vertexValue =  float3(0,0,0) ;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				float2 uv_DropletCutout = i.ase_texcoord.xy * _DropletCutout_ST.xy + _DropletCutout_ST.zw;
				float4 tex2DNode155 = tex2D( _DropletCutout, uv_DropletCutout );
				float2 uv_DropletNormals = i.ase_texcoord.xy * _DropletNormals_ST.xy + _DropletNormals_ST.zw;
				float2 uv0153 = i.ase_texcoord.xy * float2( 1,1 ) + ( (( saturate( ( tex2DNode155.r + tex2DNode155.g + tex2DNode155.b ) ) * tex2D( _DropletNormals, uv_DropletNormals ) )).rga * _Wetness ).xy;
				float2 uv0127 = i.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv0_DistortionLens = i.ase_texcoord.xy * _DistortionLens_ST.xy + _DistortionLens_ST.zw;
				float cos95 = cos( 5.0 * _Time.y );
				float sin95 = sin( 5.0 * _Time.y );
				float2 rotator95 = mul( uv0_DistortionLens - float2( 0.5,0.5 ) , float2x2( cos95 , -sin95 , sin95 , cos95 )) + float2( 0.5,0.5 );
				float cos136 = cos( 1.5708 );
				float sin136 = sin( 1.5708 );
				float2 rotator136 = mul( uv0_DistortionLens - float2( 0.5,0.5 ) , float2x2( cos136 , -sin136 , sin136 , cos136 )) + float2( 0.5,0.5 );
				float cos139 = cos( 5.0 * _Time.y );
				float sin139 = sin( 5.0 * _Time.y );
				float2 rotator139 = mul( rotator136 - float2( 0.5,0.5 ) , float2x2( cos139 , -sin139 , sin139 , cos139 )) + float2( 0.5,0.5 );
				float temp_output_140_0 = ( tex2D( _DistortionLens, rotator95 ).r - tex2D( _DistortionLens, rotator139 ).r );
				float2 appendResult132 = (float2(temp_output_140_0 , temp_output_140_0));
				float4 screenPos = i.ase_texcoord1;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 break116 = abs( (float2( -1,-1 ) + ((ase_screenPosNorm).xy - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) );
				float4 lerpResult163 = lerp( tex2D( _MainTex, uv0153 ) , tex2D( _MainTex, ( uv0127 + ( appendResult132 * ( 1.0 - pow( max( break116.x , break116.y ) , 3.0 ) ) * _Distortion ) ) ) , lerp(0.0,1.0,_WetLensDistortionSwitch));
				
				
				finalColor = lerpResult163;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=16800
183;30;2370;1364;3297.379;1521.637;1;True;True
Node;AmplifyShaderEditor.ScreenPosInputsNode;121;-5902.254,-435.7553;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;122;-5678.254,-445.7553;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;123;-5428.636,-449.9041;Float;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;96;-6453.326,-961.8932;Float;False;0;85;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;138;-6510.9,-666.1945;Float;False;Constant;_Float1;Float 1;8;0;Create;True;0;0;False;0;1.5708;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;114;-5161.902,-463.0383;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;136;-6161.9,-740.1945;Float;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;155;-5190.954,-1393.473;Float;True;Property;_DropletCutout;DropletCutout;4;0;Create;True;0;0;False;0;22bdafb3e875133449518d7c0d57b8e9;22bdafb3e875133449518d7c0d57b8e9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;116;-4986.903,-460.0383;Float;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RotatorNode;139;-5836.9,-744.1945;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;95;-5835.326,-918.8932;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;154;-4874.954,-1352.473;Float;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;135;-5277.9,-729.1945;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;85;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;115;-4701.903,-463.0383;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;85;-5275.66,-933.1594;Float;True;Property;_DistortionLens;DistortionLens;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;146;-4898.368,-1114.25;Float;True;Property;_DropletNormals;Droplet Normals;3;0;Create;True;0;0;False;0;957a873581e6f4c41b5ab0876dfb33e7;957a873581e6f4c41b5ab0876dfb33e7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;159;-4651.954,-1273.473;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;117;-4477.903,-463.0383;Float;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;140;-4904.9,-806.1945;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-4360.954,-1223.473;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;132;-4640.186,-809.4619;Float;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;120;-4196.615,-494.2629;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-4321.386,-100.4599;Float;False;Property;_Distortion;Distortion;2;0;Create;True;0;0;False;0;0.05;0.0173;0;0.05;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;147;-4064.096,-1286.402;Float;True;True;True;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;174;-4016.379,-913.6375;Float;False;Property;_Wetness;Wetness;6;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-3933.398,-538.5189;Float;True;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;127;-3937.543,-700.1891;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;173;-3763.379,-1002.637;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;153;-3493.25,-1073.773;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-3433.633,-669.178;Float;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;171;-2961.725,-219.233;Float;False;Property;_WetLensDistortionSwitch;WetLensDistortionSwitch;5;0;Create;True;0;0;False;0;1;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-3009.514,-868.1307;Float;True;Property;_MainTex;Main Tex;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;141;-3014.02,-608.0185;Float;True;Property;_TextureSample2;Texture Sample 2;0;0;Create;True;0;0;False;0;84508b93f15f2b64386ec07486afc7a3;84508b93f15f2b64386ec07486afc7a3;True;0;False;white;Auto;False;Instance;6;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;163;-2466.879,-681.4346;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;-2034.571,-623.7874;Float;False;True;2;Float;ASEMaterialInspector;0;1;Hidden/AQUAS/Underwater/Fog Simple;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;122;0;121;0
WireConnection;123;0;122;0
WireConnection;114;0;123;0
WireConnection;136;0;96;0
WireConnection;136;2;138;0
WireConnection;116;0;114;0
WireConnection;139;0;136;0
WireConnection;95;0;96;0
WireConnection;154;0;155;1
WireConnection;154;1;155;2
WireConnection;154;2;155;3
WireConnection;135;1;139;0
WireConnection;115;0;116;0
WireConnection;115;1;116;1
WireConnection;85;1;95;0
WireConnection;159;0;154;0
WireConnection;117;0;115;0
WireConnection;140;0;85;1
WireConnection;140;1;135;1
WireConnection;156;0;159;0
WireConnection;156;1;146;0
WireConnection;132;0;140;0
WireConnection;132;1;140;0
WireConnection;120;0;117;0
WireConnection;147;0;156;0
WireConnection;124;0;132;0
WireConnection;124;1;120;0
WireConnection;124;2;128;0
WireConnection;173;0;147;0
WireConnection;173;1;174;0
WireConnection;153;1;173;0
WireConnection;126;0;127;0
WireConnection;126;1;124;0
WireConnection;6;1;153;0
WireConnection;141;1;126;0
WireConnection;163;0;6;0
WireConnection;163;1;141;0
WireConnection;163;2;171;0
WireConnection;0;0;163;0
ASEEND*/
//CHKSM=91F396137461AF81F362C1B13E8717B9AE6F945F