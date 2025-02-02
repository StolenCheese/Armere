﻿Shader "Universal Render Pipeline/Custom/Cell Lit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
       [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _MetallicGlossMap("Metallic Map", 2D) = "white" {}

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0

        [Gamma]_Metallic("Metallic", Range(0.0, 1.0)) = 0.5

		_SmoothnessTextureChannel("Smoothness texture channel", Float) = 0
        
		//Custom Lighting varialbes
        _ShadowCutoff("Shadow Cutoff", Range(0.0, 1.0)) = 0.5
        _FresnelCutoff("Fresnel Cutoff", Range(0.0, 1.0)) = 0.5
        _FresnelMultiplier("Fresnel Multiplier",Float) = 0.5

        // BlendMode
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("Src", Float) = 1.0
        [HideInInspector] _DstBlend("Dst", Float) = 0.0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
        [Enum(Off,0,Front,1,Back,2)]_Cull("__cull", Float) = 2.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }
    SubShader
    {
        Tags { 
			"RenderType" = "Opaque" 
			"IgnoreProjector" = "True" 
			"RenderPipeline" = "UniversalPipeline"
			 "UniversalMaterialType" = "Lit" 
			  "ShaderModel"="4.5"
			 }
        LOD 300



        Pass
        {
            Name "Cell Lit Forward"
            Tags{"LightMode"= "UniversalForward"}

			Blend [_SrcBlend][_DstBlend]
			ZWrite [_ZWrite]
			Cull [_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 4.5


            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _ALPHAPREMULTIPLY_ON
            //#pragma shader_feature _EMISSION
            #pragma shader_feature_local _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //#pragma shader_feature _OCCLUSIONMAP

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #include "RampLighting.hlsl"
            #include "CellLitInput.hlsl"



            struct Attributes
            {
                    float4 positionOS   : POSITION;
                    float3 normalOS     : NORMAL;
                    float4 tangentOS    : TANGENT;
                    float2 texcoord     : TEXCOORD0;
                    float2 lightmapUV   : TEXCOORD1;
                    
            UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

#ifdef _ADDITIONAL_LIGHTS
                float3 positionWS               : TEXCOORD2;
#endif

#ifdef _NORMALMAP
                float4 normalWS                 : TEXCOORD3;    // xyz: normal, w: viewDir.x
                float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: viewDir.y
                float4 bitangentWS              : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
                float3 normalWS                 : TEXCOORD3;
                float3 viewDirWS                : TEXCOORD4;
#endif

                half4 fogFactorAndVertexLight   : TEXCOORD6;
#ifdef MAIN_LIGHT_CALCULATE_SHADOWS
                float4 shadowCoord              : TEXCOORD7;
#endif
                float4 pos                      : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.pos = vertexInput.positionCS;
                half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
                half3 vertexLight = RampedVertexLighting(vertexInput.positionWS, normalInput.normalWS,_ShadowCutoff);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);


            #ifdef _ADDITIONAL_LIGHTS
                output.positionWS = vertexInput.positionWS; //only used for additional lights
            #endif

            #ifdef _NORMALMAP
                output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
                output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
                output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
            #else
                output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
                output.viewDirWS = viewDirWS;
            #endif
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

            #ifdef MAIN_LIGHT_CALCULATE_SHADOWS
                output.shadowCoord = GetShadowCoord(vertexInput);
            #endif
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);



                return output;
            }


            half3 GetAdditionalLightEffect(uint lightIndex,half smoothness,half3 diffuse,half3 specularTint,half3 positionWS,half3 normalWS,half3 viewDirWS){
                    Light light = GetAdditionalRampedLight(lightIndex, positionWS);
                    //attenuate light color
                    light.color = diffuse * light.color * smoothstep(_ShadowCutoff,_ShadowCutoff + 0.01,  dot(light.direction,normalWS));
                    
                    //apply light specular
                    half3 halfVector =  normalize(light.direction + viewDirWS);
                    half specularIntensity = DistributionGGX(normalWS,halfVector,1-smoothness);
                    specularIntensity = smoothstep(1-smoothness,1.01-smoothness,specularIntensity); //toon ramp

                    return light.color * specularTint * specularIntensity +light.color * light.distanceAttenuation* light.shadowAttenuation ;
            }


            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            #ifdef MAIN_LIGHT_CALCULATE_SHADOWS
                Light light = GetMainLight(input.shadowCoord);
				//return input.shadowCoord;
            #else
                Light light = GetMainLight();
            #endif
                half3 normalWS;
                #ifdef _NORMALMAP
                    half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
                    normalWS = TransformTangentToWorld(SampleNormal(input.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale),
                        half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
                #else
                    half3 viewDirWS = input.viewDirWS;
                    normalWS = input.normalWS;
                #endif

                half3 lightColor = light.color;

                half ndotl = dot(normalWS,light.direction);
                half lightIntensity = smoothstep(_ShadowCutoff,_ShadowCutoff+0.005, ndotl * light.distanceAttenuation * light.shadowAttenuation);

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                half3 albedo = texColor.rgb * _BaseColor.rgb;                       //albedo - raw color of object

                half3 specularTint;
                half oneMinusReflectivity;


				float4 metallicSpecGloss =  SampleMetallicSpecGloss(input.uv,texColor.a);

				half smoothness = metallicSpecGloss.a;
				half metallic = metallicSpecGloss.r;

                    //diffuse - color keeping convervation with specular
                half3 diffuse = DiffuseAndSpecularFromMetallic(albedo.rgb,metallic,specularTint,oneMinusReflectivity);

                half3 halfVector = normalize(light.direction+ viewDirWS);





                half specularIntensity = DistributionGGX(normalWS,halfVector,1-smoothness);
                specularIntensity = smoothstep(1-smoothness,1.01-smoothness,specularIntensity); //toon ramp

                half3 specular =   specularTint* lightIntensity * light.color * specularIntensity;

                //calculate reflection amount
                half3 fresnel = 1- saturate( dot(SafeNormalize(viewDirWS),normalWS));

                fresnel = smoothstep(_FresnelCutoff,_FresnelCutoff + 0.01,fresnel)*_FresnelMultiplier; //toon ramp

				half3 rimLight = fresnel * lightIntensity * lightColor;

                half3 ambient = albedo * GlossyEnvironmentReflection(reflect(-viewDirWS, normalWS), 1-smoothness + 0.8, 1);//   half3(0.1,0.1,0.1);

                half3 color = specular + diffuse * lightColor * lightIntensity + ambient + rimLight;


                //add additional light colors
            #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();

                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    color += GetAdditionalLightEffect(lightIndex,smoothness,diffuse,specularTint,input.positionWS,normalWS,viewDirWS);
                }
            #endif


                color = MixFog(color, input.fogFactorAndVertexLight.x);
                
             #ifdef _ADDITIONAL_LIGHTS_VERTEX //TODO - make vertex lights actually work
                color += input.fogFactorAndVertexLight.yzw * diffuse;
            #endif

                return  half4(color,1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "CellLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Pass
        // {
		// 	Name "DepthOnly"
		// 	Tags
		// 	{
		// 		"LightMode" = "DepthOnly"
		// 	}

		// 	// Render State
		// 	Cull Back
		// 	Blend One Zero
		// 	ZTest LEqual
		// 	ZWrite On
		// 	ColorMask 0


        //     HLSLPROGRAM
        //     // Required to compile gles 2.0 with standard srp library
        //     #pragma prefer_hlslcc gles
        //     #pragma exclude_renderers d3d11_9x
        //     #pragma target 2.0

        //     #pragma vertex DepthOnlyVertex
        //     #pragma fragment DepthOnlyFragment

        //     // -------------------------------------
        //     // Material Keywords
        //     #pragma shader_feature _ALPHATEST_ON
        //     #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

        //     //--------------------------------------
        //     // GPU Instancing
        //     #pragma multi_compile_instancing


        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
        //     ENDHLSL
        // }


        Pass // - Copied from lit shader
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "CellLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }



		        // This pass is used when drawing to a _CameraNormalsTexture texture - Copied from lit shader
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "CellLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }


    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
	//CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
    CustomEditor "CellLitShader"
}
