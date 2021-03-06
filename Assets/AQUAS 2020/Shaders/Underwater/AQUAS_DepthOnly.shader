// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/AQUAS/Utils/Depth Only"
{
	Properties
	{
		_ObjectScale("Object Scale", Vector) = (0,0,0,0)
		_waterLevel("waterLevel", Float) = 0
		_RangeVector("Range Vector", Vector) = (0,0,0,0)
		_RandomMask("Random Mask", 2D) = "white" {}
		[Toggle]_ProjectGrid("Project Grid", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPos;
		};

		uniform float _ProjectGrid;
		uniform float4 _RangeVector;
		uniform float2 _ObjectScale;
		uniform float _waterLevel;
		uniform sampler2D _RandomMask;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float2 appendResult16 = (float2(max( -_RangeVector.x , 0.0 ) , max( -_RangeVector.y , 0.0 )));
			float2 RangeX18 = appendResult16;
			float2 appendResult17 = (float2(min( -_RangeVector.z , 0.0 ) , min( -_RangeVector.w , 0.0 )));
			float2 RangeZ20 = appendResult17;
			float2 ObjectScale21 = _ObjectScale;
			float2 break86 = ( (RangeX18 + (v.texcoord.xy - float2( -0.5,-0.5 )) * (RangeZ20 - RangeX18) / (float2( 0.5,0.5 ) - float2( -0.5,-0.5 ))) * ObjectScale21 );
			float3 appendResult88 = (float3(break86.x , 1.0 , break86.y));
			float Displacement23 = -10.0;
			float2 break39 = ( (RangeX18 + (v.texcoord.xy - float2( -0.5,-0.5 )) * (RangeZ20 - RangeX18) / (float2( 0.5,0.5 ) - float2( -0.5,-0.5 ))) * ObjectScale21 );
			float3 appendResult40 = (float3(break39.x , 1.0 , break39.y));
			float3 temp_output_45_0 = ( ( appendResult40 * Displacement23 ) + ase_vertex3Pos );
			float3 objToWorld47 = mul( unity_ObjectToWorld, float4( temp_output_45_0, 1 ) ).xyz;
			float3 TargetPoint49 = objToWorld47;
			float3 objToWorld46 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 StartPointGlobal48 = objToWorld46;
			float3 temp_output_52_0 = ( TargetPoint49 - StartPointGlobal48 );
			float3 x56 = ( temp_output_52_0 / length( temp_output_52_0 ) );
			float3 _Vector11 = float3(0,1,0);
			float3 PlaneNormal28 = _Vector11;
			float3 appendResult25 = (float3(0.0 , _waterLevel , 0.0));
			float3 PlaneOrigin27 = appendResult25;
			float dotResult61 = dot( PlaneNormal28 , ( TargetPoint49 - PlaneOrigin27 ) );
			float distV264 = dotResult61;
			float dotResult62 = dot( PlaneNormal28 , x56 );
			float cosPhi63 = dotResult62;
			float3 IntersectionPoint72 = ( TargetPoint49 - ( x56 * ( distV264 / cosPhi63 ) ) );
			float3 Projection92 = ( ( appendResult88 * ( Displacement23 * ( distance( IntersectionPoint72 , StartPointGlobal48 ) / distance( StartPointGlobal48 , TargetPoint49 ) ) ) ) + ase_vertex3Pos );
			float2 appendResult1_g579 = (float2(0.15 , 0.0));
			float2 temp_output_149_0 = ( (IntersectionPoint72).xz * 0.5 );
			float2 panner3_g579 = ( _Time.y * appendResult1_g579 + temp_output_149_0);
			float2 appendResult1_g578 = (float2(0.3 , 0.0));
			float cos150 = cos( radians( 180.0 ) );
			float sin150 = sin( radians( 180.0 ) );
			float2 rotator150 = mul( temp_output_149_0 - float2( 0.5,0.5 ) , float2x2( cos150 , -sin150 , sin150 , cos150 )) + float2( 0.5,0.5 );
			float2 panner3_g578 = ( _Time.y * appendResult1_g578 + rotator150);
			float4 _Vector12 = float4(0,2,-0.15,0.15);
			float4 temp_cast_2 = (_Vector12.x).xxxx;
			float4 temp_cast_3 = (_Vector12.y).xxxx;
			float4 temp_cast_4 = (_Vector12.z).xxxx;
			float4 temp_cast_5 = (_Vector12.w).xxxx;
			float4 lerpResult174 = lerp( (temp_cast_4 + (( tex2Dlod( _RandomMask, float4( panner3_g579, 0, 0.0) ) + tex2Dlod( _RandomMask, float4( panner3_g578, 0, 0.0) ) ) - temp_cast_2) * (temp_cast_5 - temp_cast_4) / (temp_cast_3 - temp_cast_2)) , float4(0,0,0,0) , saturate( pow( ( distance( IntersectionPoint72 , _WorldSpaceCameraPos ) / 1.0 ) , 0.5 ) ));
			float4 PhysicalWaves175 = lerpResult174;
			float3 worldToObjDir29 = mul( unity_WorldToObject, float4( _Vector11, 0 ) ).xyz;
			float3 VertexNormal30 = worldToObjDir29;
			v.vertex.xyz = ( _ProjectGrid )?( ( float4( Projection92 , 0.0 ) + ( PhysicalWaves175 * float4( VertexNormal30 , 0.0 ) ) ) ):( float4( ase_vertex3Pos , 0.0 ) ).xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float3 temp_cast_0 = (eyeDepth1).xxx;
			o.Emission = temp_cast_0;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17200
7;29;2546;1364;1686.062;450.8934;1;True;False
Node;AmplifyShaderEditor.Vector4Node;7;-5136,146.3372;Float;False;Property;_RangeVector;Range Vector;2;0;Create;True;0;0;False;0;0,0,0,0;-0.5464204,-0.4748375,0.5464242,0.4464068;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NegateNode;10;-4832,288;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;9;-4816,192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;8;-4832,384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;11;-4832,96;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;14;-4624,384;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;12;-4624,80;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;15;-4624,288;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;13;-4608,176;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;16;-4432,112;Inherit;False;FLOAT2;4;0;FLOAT;-0.5;False;1;FLOAT;-0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;-4432,320;Inherit;False;FLOAT2;4;0;FLOAT;0.5;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-4256,112;Inherit;False;RangeX;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-4256,304;Inherit;False;RangeZ;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;19;-4768,-800;Inherit;False;Property;_ObjectScale;Object Scale;0;0;Create;True;0;0;False;0;0,0;1.009911,1.154699;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;2;-3920,-768;Inherit;False;2545.219;517.7468;Calculate Target Point in World Space;15;93;49;47;45;43;42;41;40;39;38;37;36;35;34;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-3808,-464;Inherit;False;18;RangeX;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;34;-3840,-656;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;33;-3792,-368;Inherit;False;20;RangeZ;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;-4560,-768;Inherit;False;ObjectScale;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;36;-3392,-640;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;-0.5,-0.5;False;2;FLOAT2;0.5,0.5;False;3;FLOAT2;0,0;False;4;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;37;-3312,-448;Inherit;False;21;ObjectScale;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-4785,-592;Inherit;False;Constant;_Displacement;Displacement;2;0;Create;True;0;0;False;0;-10;-240.11;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-3088,-656;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-4592,-592;Inherit;False;Displacement;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;39;-2896,-640;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;40;-2576,-656;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-2768,-512;Inherit;False;23;Displacement;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;3;-3920,-1063.21;Inherit;False;775.7944;254.3346;Calculate Start Point in World Space;3;48;46;44;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-2384,-656;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;42;-2416,-512;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-2192,-592;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;44;-3888,-1015.21;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformPositionNode;46;-3652,-991.2099;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;47;-1952,-592;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-1632,-592;Inherit;False;TargetPoint;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-3418.337,-973.3148;Inherit;False;StartPointGlobal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;4;-3904,-192;Inherit;False;1309.425;1225.474;Compute Line-Plane-Intersection;23;72;71;70;69;68;67;66;65;64;63;62;61;60;59;58;57;56;55;54;53;52;51;50;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-3840,288;Inherit;False;48;StartPointGlobal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-3808,-16;Inherit;False;49;TargetPoint;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-4864,-432;Inherit;False;Property;_waterLevel;waterLevel;1;0;Create;True;0;0;False;0;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;52;-3568,224;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LengthOpNode;53;-3376,304;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;25;-4640,-448;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-4480,-448;Inherit;False;PlaneOrigin;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;-3216,224;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;26;-4848,-256;Inherit;False;Constant;_Vector11;Vector 11;4;0;Create;True;0;0;False;0;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;55;-3824,128;Inherit;False;27;PlaneOrigin;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-3056,224;Inherit;False;x;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;28;-4592,-256;Inherit;False;PlaneNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;59;-3536,64;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-3808,-144;Inherit;False;28;PlaneNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;57;-3792,528;Inherit;False;56;x;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-3808,416;Inherit;False;28;PlaneNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;61;-3312,-80;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;62;-3568,464;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;-3168,-80;Inherit;False;distV2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;-3408,464;Inherit;False;cosPhi;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-3792,640;Inherit;False;64;distV2;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-3792,736;Inherit;False;63;cosPhi;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-3568,624;Inherit;False;56;x;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;68;-3552,720;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-3344,704;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-3328,608;Inherit;False;49;TargetPoint;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;71;-3088,672;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-2912,672;Inherit;False;IntersectionPoint;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;143;-5656.943,1942.267;Inherit;False;3824.451;1087.266;Small Waves Displacement and Normals;20;175;174;173;171;172;169;168;170;167;166;153;152;178;150;148;149;147;145;146;144;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;144;-5546.842,2139.971;Inherit;False;72;IntersectionPoint;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;147;-5132.683,2309.99;Inherit;False;Constant;_Float3;Float 3;5;0;Create;True;0;0;False;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;146;-5246.353,2155.192;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-4709.363,2329.089;Float;False;Constant;_Float11;Float 11;8;0;Create;True;0;0;False;0;180;180;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;6;-3888,1088;Inherit;False;2498.382;702.0112;Compute vertex projection;20;92;91;90;89;88;87;86;85;84;83;82;81;80;79;78;77;76;75;74;73;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RadiansOpNode;148;-4472.244,2335.597;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;149;-4831.243,2155.429;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0.05;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;150;-4205.323,2292.302;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;74;-3824,1152;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;75;-3776,1376;Inherit;False;20;RangeZ;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-3792,1296;Inherit;False;18;RangeX;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;153;-3769.281,2174.958;Inherit;False;Wave Movement;-1;;579;a21caff016826fd409ae039d4063927b;0;2;6;FLOAT2;0,0;False;7;FLOAT;0.15;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;76;-3536,1216;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;-0.5,-0.5;False;2;FLOAT2;0.5,0.5;False;3;FLOAT2;0,0;False;4;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-2928,1440;Inherit;False;72;IntersectionPoint;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-2928,1552;Inherit;False;48;StartPointGlobal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-2912,1648;Inherit;False;49;TargetPoint;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;178;-3781.492,1975.432;Inherit;True;Property;_RandomMask;Random Mask;3;0;Create;True;0;0;False;0;15f5278be96c0184fa0b0fa59e90a43a;15f5278be96c0184fa0b0fa59e90a43a;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-3040,1312;Inherit;False;21;ObjectScale;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;152;-3773.791,2293.081;Inherit;False;Wave Movement;-1;;578;a21caff016826fd409ae039d4063927b;0;2;6;FLOAT2;0,0;False;7;FLOAT;0.3;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DistanceOpNode;83;-2672,1488;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;82;-2768,1200;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;166;-3448.342,2096.362;Inherit;True;Property;_TextureSample11;Texture Sample 11;65;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;81;-2672,1600;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;167;-3447.271,2299.621;Inherit;True;Property;_TextureSample12;Texture Sample 12;65;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;86;-2608,1200;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.Vector4Node;170;-3357.742,2549.878;Inherit;False;Constant;_Vector12;Vector 12;64;0;Create;True;0;0;False;0;0,2,-0.15,0.15;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;84;-2592,1392;Inherit;False;23;Displacement;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;168;-3181.252,2775.709;Inherit;False;72;IntersectionPoint;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;169;-3070.271,2203.621;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;85;-2512,1552;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;172;-2886.682,2211.519;Inherit;False;Constant;_Vector13;Vector 13;65;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;173;-2822.682,2688.518;Inherit;False;Distance Blend;-1;;595;a467635a8f465a3408a87831c1dc83fd;0;3;13;FLOAT3;0,0,0;False;11;FLOAT;1;False;12;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-2320,1376;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;171;-2890.742,2399.879;Inherit;False;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;88;-2304,1200;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-2064,1248;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;90;-2128,1488;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;29;-4608,-144;Inherit;False;World;Object;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;174;-2485.681,2352.519;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;175;-2240.271,2272.621;Inherit;False;PhysicalWaves;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;91;-1856,1328;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-4352,-144;Inherit;False;VertexNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;92;-1696,1328;Inherit;False;Projection;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;179;-1207.874,590.1559;Inherit;False;175;PhysicalWaves;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;180;-1144.388,829.5873;Inherit;False;30;VertexNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;181;-911.8705,447.4938;Inherit;False;92;Projection;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;182;-906.0552,658.4462;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PosVertexDataNode;185;-920.1575,263.4958;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;183;-690.5952,479.1991;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenDepthNode;1;-442.2115,-3.266235;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;-1968,-384;Inherit;False;TempDisp;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;184;-453.1575,369.4958;Inherit;False;Property;_ProjectGrid;Project Grid;4;0;Create;True;0;0;False;0;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;ASEMaterialInspector;0;0;Standard;Hidden/AQUAS/Utils/Depth Only;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;7;3
WireConnection;9;0;7;2
WireConnection;8;0;7;4
WireConnection;11;0;7;1
WireConnection;14;0;8;0
WireConnection;12;0;11;0
WireConnection;15;0;10;0
WireConnection;13;0;9;0
WireConnection;16;0;12;0
WireConnection;16;1;13;0
WireConnection;17;0;15;0
WireConnection;17;1;14;0
WireConnection;18;0;16;0
WireConnection;20;0;17;0
WireConnection;21;0;19;0
WireConnection;36;0;34;0
WireConnection;36;3;35;0
WireConnection;36;4;33;0
WireConnection;38;0;36;0
WireConnection;38;1;37;0
WireConnection;23;0;22;0
WireConnection;39;0;38;0
WireConnection;40;0;39;0
WireConnection;40;2;39;1
WireConnection;43;0;40;0
WireConnection;43;1;41;0
WireConnection;45;0;43;0
WireConnection;45;1;42;0
WireConnection;46;0;44;0
WireConnection;47;0;45;0
WireConnection;49;0;47;0
WireConnection;48;0;46;0
WireConnection;52;0;50;0
WireConnection;52;1;51;0
WireConnection;53;0;52;0
WireConnection;25;1;24;0
WireConnection;27;0;25;0
WireConnection;54;0;52;0
WireConnection;54;1;53;0
WireConnection;56;0;54;0
WireConnection;28;0;26;0
WireConnection;59;0;50;0
WireConnection;59;1;55;0
WireConnection;61;0;58;0
WireConnection;61;1;59;0
WireConnection;62;0;60;0
WireConnection;62;1;57;0
WireConnection;64;0;61;0
WireConnection;63;0;62;0
WireConnection;68;0;65;0
WireConnection;68;1;66;0
WireConnection;69;0;67;0
WireConnection;69;1;68;0
WireConnection;71;0;70;0
WireConnection;71;1;69;0
WireConnection;72;0;71;0
WireConnection;146;0;144;0
WireConnection;148;0;145;0
WireConnection;149;0;146;0
WireConnection;149;1;147;0
WireConnection;150;0;149;0
WireConnection;150;2;148;0
WireConnection;153;6;149;0
WireConnection;76;0;74;0
WireConnection;76;3;73;0
WireConnection;76;4;75;0
WireConnection;152;6;150;0
WireConnection;83;0;77;0
WireConnection;83;1;78;0
WireConnection;82;0;76;0
WireConnection;82;1;79;0
WireConnection;166;0;178;0
WireConnection;166;1;153;0
WireConnection;81;0;78;0
WireConnection;81;1;80;0
WireConnection;167;0;178;0
WireConnection;167;1;152;0
WireConnection;86;0;82;0
WireConnection;169;0;166;0
WireConnection;169;1;167;0
WireConnection;85;0;83;0
WireConnection;85;1;81;0
WireConnection;173;13;168;0
WireConnection;87;0;84;0
WireConnection;87;1;85;0
WireConnection;171;0;169;0
WireConnection;171;1;170;1
WireConnection;171;2;170;2
WireConnection;171;3;170;3
WireConnection;171;4;170;4
WireConnection;88;0;86;0
WireConnection;88;2;86;1
WireConnection;89;0;88;0
WireConnection;89;1;87;0
WireConnection;29;0;26;0
WireConnection;174;0;171;0
WireConnection;174;1;172;0
WireConnection;174;2;173;0
WireConnection;175;0;174;0
WireConnection;91;0;89;0
WireConnection;91;1;90;0
WireConnection;30;0;29;0
WireConnection;92;0;91;0
WireConnection;182;0;179;0
WireConnection;182;1;180;0
WireConnection;183;0;181;0
WireConnection;183;1;182;0
WireConnection;93;0;45;0
WireConnection;184;0;185;0
WireConnection;184;1;183;0
WireConnection;0;2;1;0
WireConnection;0;11;184;0
ASEEND*/
//CHKSM=C8781A5B95502BFC37C96E2340157EEE36F16D55