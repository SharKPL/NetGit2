//==========================================================
//-------------|          INTERRA          |---------------
//==========================================================
//-------------|           4.2.1           |---------------
//==========================================================
//-------------| ©  INEFFABILIS ARCANUM    |---------------
//==========================================================

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine.Rendering;
#endif

#if USING_HDRP
	using UnityEngine.Rendering.HighDefinition;
#endif
#if USING_URP
	using UnityEngine.Rendering.Universal;
#endif

namespace InTerra
{
	public static class InTerra_Data
	{
		public const string ObjectShaderName = "InTerra/Built-in/Object into Terrain Integration";
		public const string DiffuseObjectShaderName = "InTerra/Built-in/Diffuse/Object into Terrain Integration (Diffuse)";
		public const string URPObjectShaderName = "InTerra/URP/Object into Terrain Integration";
		public const string HDRPObjectShaderName = "InTerra/HDRP/Object into Terrain Integration";
		public const string HDRPObjectTessellationShaderName = "InTerra/HDRP Tessellation/Object into Terrain Integration Tessellation";

		public const string TerrainShaderName = "InTerra/Built-in/Terrain (Standard With Features)";
		public const string DiffuseTerrainShaderName = "InTerra/Built-in/Diffuse/Terrain (Diffuse With Features)";
		public const string URPTerrainShaderName = "InTerra/URP/Terrain (Lit with Features)";
		public const string HDRPTerrainShaderName = "InTerra/HDRP/Terrain (Lit with Features)";
		public const string HDRPTerrainTessellationShaderName = "InTerra/HDRP Tessellation/Terrain (Lit with Features)";

		public const string MeshTerrainShaderName = "InTerra/Built-in/Mesh Terrain (Standard)";
		public const string DiffuseMeshTerrainShaderName = "InTerra/Built-in/Diffuse/Mesh Terrain (Diffuse)";
		public const string URPMeshTerrainShaderName = "InTerra/URP/Mesh Terrain";
		public const string HDRPMeshTerrainShaderName = "InTerra/HDRP/Mesh Terrain";
		public const string HDRPMeshTerrainTessellationShaderName = "InTerra/HDRP Tessellation/Mesh Terrain Tessellation";

		public const string TessellationShaderFolder = "InTerra/HDRP Tessellation";

		static readonly string[] TerrainToObjectsTextureProperties = {
			"_TerrainColorTintTexture",
			"_TerrainNormalTintTexture",
			"_TrackDetailTexture",
			"_TrackDetailNormalTexture"};

		static readonly string[] TerrainToObjectsFloats = {
			"_HeightTransition",
			"_Distance_HeightTransition",
			"_HT_distance_scale",
			"_HT_cover",
			"_TriplanarOneToAllSteep",
			"_TriplanarSharpness",
			"_TerrainColorTintStrenght",
			"_TerrainNormalTintStrenght",
			"_HeightmapBlending",
			"_Terrain_Parallax",
			"_Tracks",
			"_MipMapLevel",
			"_TrackAO",
			"_TrackDetailNormalStrenght",
			"_TrackNormalStrenght",
			"_TrackHeightOffset",
			"_TrackHeightTransition",
			"_ParallaxTrackAffineSteps",
			"_ParallaxTrackSteps",
			"_TrackEdgeNormals",
			"_TrackEdgeSharpness",
			"_Gamma",
			"_WorldMapping"};

		static readonly string[] TerrainToObjectsTessellationProperties = {   
			"_TessellationFactorMinDistance",
			"_TessellationFactorMaxDistance",
			"_Tessellation_HeightTransition",
			"_TessellationShadowQuality",
			"_TrackTessallationHeightTransition",
			"_TrackTessallationHeightOffset"};

		static readonly string[] TerrainToObjectsVectorProperties = {
			"_HT_distance",
			"_TerrainNormalTintDistance",
			"_MipMapFade"};

		static readonly string[] TerrainToObjectsKeywords = {"_TERRAIN_DISTANCEBLEND"};

		const string UpdaterName = "InTerra_UpdateAndCheck";
		static public GameObject Updater;
		static InTerra_UpdateAndCheck UpdateScript;
	
		static Camera TrackCamera;
		static Vector3 TrackCameraForwardVec;
		static Vector3 TrackCameraPositon;
		static float TracksUpdateTimeCount;
		static bool TracksCameraUpdate;
		public static bool initTrack;

		static MaterialPropertyBlock mtb = new();

		#if UNITY_EDITOR
			static string InTerraPath;		
			static bool GlobalKeywordsCheck;

			const int DEFINE_MASKMAP_LineNumber = 1;
			const int DEFINE_NORMAL_MASK_LineNumber = 2;
			const int DEFINE_HEIGTH_ONLY_LineNumber = 3;

			const int RESTRICT_NORMALMAP_LineNumber = 5;
			const int RESTRICT_HEIGHTBLEND_LineNumber = 6;	
			const int RESTRICT_TERR_PARALAX_LineNumber = 7;
			const int RESTRICT_TRACKS_LineNumber = 8;
			const int RESTRICT_OBJ_PARALLAX_LineNumber = 10;
			const int RESTRICT_PUDDLES_LineNumber = 12;
			#if USING_URP
				const int URP_VERSION_2022_2_LineNumber = 13;
			#endif		
			#if !(USING_URP || USING_HDRP)
				const int EIGHT_LAYERS_PASS_LineNumber = 13;
			#endif	
		#endif

		public static void UpdateTerrainData(bool UpdateDictionary)
		{
			Terrain[] terrains = Terrain.activeTerrains;
			List<MeshRenderer> meshTerrainsList = GetUpdaterScript().MeshTerrainsList;

			if (terrains.Length > 0 || (meshTerrainsList != null && meshTerrainsList.Count > 0))
			{
				DictionaryMaterialTerrain materialTerrain = GetUpdaterScript().MaterialTerrain;
				DictionaryMaterialMeshTerrain materialMeshTerrain = GetUpdaterScript().MaterialMeshTerrain;

				meshTerrainsList.Clear();

				//===== DICTIONARY OF MATERIALS WITH INTERRA SHADERS AND SUM POSITIONS OF RENDERERS WITH THAT MATERIAL ======
				if (UpdateDictionary)
				{
					Dictionary<Material, Vector3> matPos = new Dictionary<Material, Vector3>();
					#if UNITY_2023_1_OR_NEWER
						MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
					#else
						MeshRenderer[] renderers = Object.FindObjectsOfType<MeshRenderer>();
					#endif
					foreach (MeshRenderer rend in renderers)
					{
						if (rend != null && rend.bounds != null)
						{
							foreach (Material mat in rend.sharedMaterials)
							{
								if (CheckObjectShader(mat))
								{
									if (!matPos.ContainsKey(mat))
									{
										matPos.Add(mat, new Vector3(rend.bounds.center.x, rend.bounds.center.z, 1));
									}
									else
									{
										Vector3 sumPos = matPos[mat];
										sumPos.x += rend.bounds.center.x;
										sumPos.y += rend.bounds.center.z;
										sumPos.z += 1;
										matPos[mat] = sumPos;
									}
								}

								if (CheckMeshTerrainShader(mat))
								{
									if (!meshTerrainsList.Contains(rend))
									{
										meshTerrainsList.Add(rend);

									}
								}
							}
						}
					}

					//===================== DICTIONARY OF MATERIALS AND TERRAINS WHERE ARE PLACED =========================
					Dictionary<Material, Terrain> tempMatTerDict = CopyMaterialTerrainDictionary(materialTerrain);
					Dictionary<Material, MeshRenderer> tempMatMeshTerDict = CopyMaterialMeshDictionary(materialMeshTerrain);

					materialTerrain.Clear();
					materialMeshTerrain.Clear();

					foreach (Material mat in matPos.Keys)
					{
						Vector2 averagePos = matPos[mat] / matPos[mat].z;
						foreach (Terrain terrain in terrains)
						{
							if (!materialTerrain.ContainsKey(mat))
							{
								if (mat.GetFloat("_CustomTerrainSelection") > 0 && tempMatTerDict.ContainsKey(mat))
								{
									materialTerrain.Add(mat, tempMatTerDict[mat]);
								}
								else
								{
									if (CheckPosition(terrain, averagePos))
									{
										materialTerrain.Add(mat, terrain);
									}
								}
							}
							if (CheckTerrainShaderContains(terrain, "InTerra/HDRP"))
							{
								terrain.materialTemplate.renderQueue = 2225;
							}
						}

						//===================== MESH TERRAINS =======================
						foreach (MeshRenderer meshTerrain in meshTerrainsList)
						{
							if (!materialMeshTerrain.ContainsKey(mat))
							{
								if (mat.GetFloat("_CustomTerrainSelection") == 1 && mat.HasProperty("_MeshTerrain") && mat.GetFloat("_MeshTerrain") == 1 && tempMatMeshTerDict.ContainsKey(mat))
								{
									materialMeshTerrain.Add(mat, tempMatMeshTerDict[mat]);
								}
								else if (mat.GetFloat("_CustomTerrainSelection") == 0)
								{
									if (CheckMeshTerrainPosition(meshTerrain, averagePos))
									{
										materialMeshTerrain.Add(mat, meshTerrain);
									}
								}
							}

							InTerra_MeshTerrainData meshTerrainData = meshTerrain.GetComponent<InTerra_MeshTerrainData>();
							if (meshTerrainData != null)
							{
								SetMeshTerrainPositionAndSize(meshTerrain.sharedMaterial, meshTerrain);
								for (int i = 0; i < 8; i++)
								{
									TerrainLaeyrDataToMaterial(meshTerrainData.TerrainLayers[i], i, meshTerrain.sharedMaterial);
								}
							}
						}
					} 
				}

				//================================================================================
				//--------------------|    SET TERRAINS DATA TO MATERIALS    |--------------------
				//================================================================================
				foreach (Material mat in materialTerrain.Keys)
				{
					Terrain terrain = materialTerrain[mat];
					if (terrain != null && terrain.materialTemplate != null && CheckObjectShader(mat))
					{
						#if USING_HDRP
							if (terrain.terrainData.alphamapTextureCount > 1 && !(mat.IsKeywordEnabled("_LAYERS_ONE") && mat.IsKeywordEnabled("_LAYERS_TWO"))) mat.EnableKeyword("_LAYERS_EIGHT"); else mat.DisableKeyword("_LAYERS_EIGHT");
						#elif USING_URP
							if (terrain.materialTemplate.IsKeywordEnabled("_LAYERS_EIGHT") && !(mat.IsKeywordEnabled("_LAYERS_ONE") && mat.IsKeywordEnabled("_LAYERS_TWO"))) mat.EnableKeyword("_LAYERS_EIGHT"); else mat.DisableKeyword("_LAYERS_EIGHT");
						#endif

						Vector4 heightScale = new Vector4(terrain.terrainData.heightmapScale.x, terrain.terrainData.heightmapScale.y / (32766.0f / 65535.0f), terrain.terrainData.heightmapScale.z, terrain.terrainData.heightmapScale.y);

						SetTerrainDataToMaterials(terrain.terrainData.size, terrain.transform.position, heightScale, terrain.terrainData.heightmapTexture, terrain.terrainData.alphamapTextures, terrain.materialTemplate, terrain.terrainData.terrainLayers, mat);
					}
				}
				foreach (Material mat in materialMeshTerrain.Keys)
				{
					InTerra_MeshTerrainData meshTerrainData = materialMeshTerrain[mat].GetComponent<InTerra_MeshTerrainData>();
					if (meshTerrainData != null)
					{
						Material meshTerrainMaterial = materialMeshTerrain[mat].sharedMaterial;
						Texture2D[] controlmaps = new Texture2D[] { meshTerrainData.ControlMap, meshTerrainData.ControlMap1 };

						#if (USING_URP || USING_HDRP)							
							if (meshTerrainMaterial.IsKeywordEnabled("_LAYERS_EIGHT") && !(mat.IsKeywordEnabled("_LAYERS_ONE") && mat.IsKeywordEnabled("_LAYERS_TWO"))) mat.EnableKeyword("_LAYERS_EIGHT"); else mat.DisableKeyword("_LAYERS_EIGHT");
						#endif

						SetTerrainDataToMaterials(meshTerrainMaterial.GetVector("_TerrainSize"), meshTerrainMaterial.GetVector("_TerrainPosition"), meshTerrainMaterial.GetVector("_TerrainHeightmapScale"), meshTerrainMaterial.GetTexture("_TerrainHeightmapTexture"), controlmaps, meshTerrainMaterial, meshTerrainData.TerrainLayers, mat);
					}
				}
			}
			#if UNITY_EDITOR
				if (!CheckDefinedKeywords()) WriteDefinedKeywords();
				#if USING_URP
					URPShadersVersionAdjust();
				#endif
			#endif
			TerrainMaterialUpdate();
		}

		//============================================================================
		//-------------------------|		FUNCTIONS		|-------------------------
		//============================================================================
		public static bool CheckPosition(Terrain terrain, Vector2 position)
		{
			return terrain != null && terrain.terrainData != null
			&& terrain.GetPosition().x <= position.x && (terrain.GetPosition().x + terrain.terrainData.size.x) > position.x
			&& terrain.GetPosition().z <= position.y && (terrain.GetPosition().z + terrain.terrainData.size.z) > position.y;
		}

		public static bool CheckMeshTerrainPosition(MeshRenderer terrain, Vector2 position)
		{
			return terrain != null
			&& terrain.bounds.min.x <= position.x && (terrain.bounds.min.x + terrain.bounds.size.x) > position.x
			&& terrain.bounds.min.z <= position.y && (terrain.bounds.min.z + terrain.bounds.size.z) > position.y;
		}

		public static bool CheckObjectShader(Material mat)
		{
			return mat && mat.shader && mat.shader.name != null
			&& (mat.shader.name == ObjectShaderName
			 || mat.shader.name == DiffuseObjectShaderName
			 || mat.shader.name == URPObjectShaderName
			 || mat.shader.name == HDRPObjectShaderName
			 || mat.shader.name == HDRPObjectTessellationShaderName);
		}

		public static bool CheckTerrainShader(Material mat)
		{
			return mat && mat.shader && mat.shader.name != null
			   && (mat.shader.name == TerrainShaderName
				|| mat.shader.name == DiffuseTerrainShaderName
				|| mat.shader.name == URPTerrainShaderName
				|| mat.shader.name.Contains(HDRPTerrainShaderName)
				|| mat.shader.name.Contains(HDRPTerrainTessellationShaderName));
		}

		public static bool CheckTerrainShaderContains(Terrain terrain, string name)
		{
			return terrain
				&& terrain.materialTemplate
				&& terrain.materialTemplate.shader
				&& terrain.materialTemplate.shader.name != null
				&& terrain.materialTemplate.shader.name.Contains(name);
		}

		public static bool CheckMeshTerrainShader(Material mat)
		{
			return mat && mat.shader && mat.shader.name != null
			&& (mat.shader.name == MeshTerrainShaderName
			|| mat.shader.name == DiffuseMeshTerrainShaderName
			|| mat.shader.name == URPMeshTerrainShaderName
			|| mat.shader.name.Contains(HDRPMeshTerrainShaderName)
			|| mat.shader.name.Contains(HDRPMeshTerrainTessellationShaderName));
		}

		public static void SetTerrainDataToMaterials( Vector3 terrainSize, Vector3 terrainPosition, Vector4 heightmapScale, Texture heightmap, Texture2D[] controlMaps, Material terrainMaterial, TerrainLayer[] tl, Material objectMaterial)
		{
			objectMaterial.SetVector("_TerrainSize", terrainSize);
			objectMaterial.SetVector("_TerrainPosition", terrainPosition);
			objectMaterial.SetVector("_TerrainHeightmapScale", heightmapScale);
			objectMaterial.SetTexture("_TerrainHeightmapTexture", heightmap);

			if (CheckTerrainShader(terrainMaterial) || CheckMeshTerrainShader(terrainMaterial))
			{
				TerrainKeywordsToMaterial(terrainMaterial, objectMaterial, TerrainToObjectsKeywords);
				SetTerrainFloatsToMaterial(terrainMaterial, objectMaterial, TerrainToObjectsFloats);
				SetTerrainTextureToMaterial(terrainMaterial, objectMaterial, TerrainToObjectsTextureProperties);
				SetTerrainVectorsToMaterial(terrainMaterial, objectMaterial, TerrainToObjectsVectorProperties);

				if (objectMaterial.shader.name == HDRPObjectTessellationShaderName && terrainMaterial.shader.name.Contains(TessellationShaderFolder))
				{
					float terrainMaxDisplacement = terrainMaterial.GetFloat("_TessellationMaxDisplacement");
					float objectMaxDisplacement = objectMaterial.GetFloat("_TessellationObjMaxDisplacement");

					objectMaterial.SetFloat("_TessellationMaxDisplacement", terrainMaxDisplacement > objectMaxDisplacement ? terrainMaxDisplacement : objectMaxDisplacement);
					SetTerrainFloatsToMaterial(terrainMaterial, objectMaterial, TerrainToObjectsTessellationProperties);
				}
			}
			else
			{
				#if (USING_HDRP || USING_URP)
					DisableKeywords(objectMaterial, TerrainToObjectsKeywords);
					if (terrainMaterial.IsKeywordEnabled("_TERRAIN_BLEND_HEIGHT")) objectMaterial.SetFloat("_HeightmapBlending", 1); else objectMaterial.SetFloat("_HeightmapBlending", 0);
					objectMaterial.SetFloat("_HeightTransition", 60 - 60 * terrainMaterial.GetFloat("_HeightTransition"));
				#else
					DisableKeywords(objectMaterial, TerrainToObjectsKeywords);							
				#endif
			}

			//----------- ONE PASS ------------
			#if (USING_HDRP || USING_URP)
				if(!objectMaterial.IsKeywordEnabled("_LAYERS_TWO") && !objectMaterial.IsKeywordEnabled("_LAYERS_ONE") && !objectMaterial.IsKeywordEnabled("_LAYERS_EIGHT"))
			#else
				if(!objectMaterial.IsKeywordEnabled("_LAYERS_TWO") && !objectMaterial.IsKeywordEnabled("_LAYERS_ONE") && !GetGlobalData().eightLayersPass)					
			#endif
			{
				int passNumber = (int)objectMaterial.GetFloat("_PassNumber");

				for (int i = 0; (i + (passNumber * 4)) < tl.Length && i < 4; i++)
				{
					TerrainLaeyrDataToMaterial(tl[i + (passNumber * 4)], i, objectMaterial);
				}

				if (controlMaps.Length > passNumber) objectMaterial.SetTexture("_Control", controlMaps[passNumber]);
				if (passNumber > 0) objectMaterial.SetFloat("_HeightmapBlending", 0);
			}
			else
			{
				int passNumber = (int)objectMaterial.GetFloat("_PassNumber");

				for (int i = 0; (i + (passNumber * 4)) < tl.Length && i < 8; i++)
				{
					TerrainLaeyrDataToMaterial(tl[i + (passNumber * 4)], i, objectMaterial);
				}

				if (controlMaps.Length > 0) objectMaterial.SetTexture("_Control", controlMaps[0]);
				if (controlMaps.Length > 1) objectMaterial.SetTexture("_Control1", controlMaps[1]);
			}

			//----------- ONE LAYER ------------
			if (objectMaterial.IsKeywordEnabled("_LAYERS_ONE"))
			{
				#if UNITY_EDITOR //The TerrainLayers in Editor are referenced by GUID, in Build by TerrainLayers array index
					TerrainLayer terainLayer = TerrainLayerFromGUID(objectMaterial, "TerrainLayerGUID_1");
					TerrainLaeyrDataToMaterial(terainLayer, 0, objectMaterial);
				#else
					int layerIndex1 = (int)objectMaterial.GetFloat("_LayerIndex1");
					CheckLayerIndex(tl, 0, objectMaterial, ref layerIndex1);
					TerrainLaeyrDataToMaterial(tl[layerIndex1], 0, objectMaterial);	
				#endif
			}

			//----------- TWO LAYERS ------------
			if (objectMaterial.IsKeywordEnabled("_LAYERS_TWO"))
			{
				#if UNITY_EDITOR
					TerrainLayer terainLayer1 = TerrainLayerFromGUID(objectMaterial, "TerrainLayerGUID_1");
					TerrainLayer terainLayer2 = TerrainLayerFromGUID(objectMaterial, "TerrainLayerGUID_2");
					TerrainLaeyrDataToMaterial(terainLayer1, 0, objectMaterial);
					TerrainLaeyrDataToMaterial(terainLayer2, 1, objectMaterial);
					int layerIndex1 = tl.ToList().IndexOf(terainLayer1);
					int layerIndex2 = tl.ToList().IndexOf(terainLayer2);
				#else
					int layerIndex1 = (int)objectMaterial.GetFloat("_LayerIndex1"); 
					int layerIndex2 = (int)objectMaterial.GetFloat("_LayerIndex2");
					CheckLayerIndex(tl, 0, objectMaterial, ref layerIndex1);
					CheckLayerIndex(tl, 1, objectMaterial, ref layerIndex2);
					TerrainLaeyrDataToMaterial(tl[layerIndex1], 0, objectMaterial);
					TerrainLaeyrDataToMaterial(tl[layerIndex2], 1, objectMaterial);	
				#endif

				objectMaterial.SetFloat("_ControlNumber", layerIndex1 % 4);

				if (controlMaps.Length > layerIndex1 / 4) objectMaterial.SetTexture("_Control", controlMaps[layerIndex1 / 4]);
				if (layerIndex1 > 3 || layerIndex2 > 3) objectMaterial.SetFloat("_HeightmapBlending", 0);
			}

			if ((objectMaterial.shader.name != DiffuseObjectShaderName) && objectMaterial.GetFloat("_DisableTerrainParallax") == 1)
			{
				objectMaterial.SetFloat("_Terrain_Parallax", 0.0f);							
			}

			if (objectMaterial.GetFloat("_DisableDistanceBlending") == 1)
			{
				objectMaterial.DisableKeyword("_TERRAIN_DISTANCEBLEND");
			}

			if (objectMaterial.shader.name == DiffuseObjectShaderName)
			{
				if (objectMaterial.GetTexture("_BumpMap")) { objectMaterial.EnableKeyword("_OBJECT_NORMALMAP"); } else { objectMaterial.DisableKeyword("_OBJECT_NORMALMAP"); }
			}					
		}

		static Dictionary<Material, Terrain> CopyMaterialTerrainDictionary(Dictionary<Material, Terrain> matTerDict)
		{
			Dictionary<Material, Terrain> tempMatTerDict = new();
			foreach (Material mt in matTerDict.Keys)
			{
				if (!tempMatTerDict.ContainsKey(mt))
				{
					tempMatTerDict.Add(mt, matTerDict[mt]);
				}
			}
			return tempMatTerDict;
		}

		static Dictionary<Material, MeshRenderer> CopyMaterialMeshDictionary(Dictionary<Material, MeshRenderer> matMeshDict)
		{
			Dictionary<Material, MeshRenderer> tempMatMeshDict = new();

			foreach (Material mt in matMeshDict.Keys)
			{
				if (!tempMatMeshDict.ContainsKey(mt))
				{
					tempMatMeshDict.Add(mt, matMeshDict[mt]);
				}
			}
			return tempMatMeshDict;
		}

		#if UNITY_EDITOR
			public static TerrainLayer TerrainLayerFromGUID(Material mat, string tag)
			{
				return (TerrainLayer)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(mat.GetTag(tag, false)), typeof(TerrainLayer));
			}
			public static TerrainData TerrainDataFromGUID(Material mat, string tag)
			{
				return (TerrainData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(mat.GetTag(tag, false)), typeof(TerrainData));
			}
		#endif

		#if UNITY_EDITOR
			public static void TextureAlphaSmoothness(TerrainLayer tl)
			{
				if (tl)
				{
					TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tl.diffuseTexture)) as TextureImporter;
					if (importer && importer.DoesSourceTextureHaveAlpha())
					{
						tl.smoothness = 1;
					}
				}
			}
		#endif

		public static void TerrainLaeyrDataToMaterial(TerrainLayer tl, int n, Material mat)
		{
			#if UNITY_EDITOR
				TextureAlphaSmoothness(tl);
			#endif

			mat.SetTexture("_Splat" + n.ToString(), tl ? tl.diffuseTexture : null);
			mat.SetTexture("_Normal" + n.ToString(), tl ? tl.normalMapTexture : null);
			mat.SetTexture("_Mask" + n.ToString(), tl ? tl.maskMapTexture : null);
			mat.SetVector("_SplatUV" + n.ToString(), tl ? new Vector4(tl.tileSize.x, tl.tileSize.y, tl.tileOffset.x, tl.tileOffset.y) : new Vector4(1, 1, 0, 0));
			mat.SetVector("_MaskMapRemapScale" + n.ToString(), tl ? tl.maskMapRemapMax - tl.maskMapRemapMin : new Vector4(1, 1, 1, 1));
			mat.SetVector("_MaskMapRemapOffset" + n.ToString(), tl ? tl.maskMapRemapMin : new Vector4(0, 0, 0, 0));
			mat.SetVector("_DiffuseRemapScale" + n.ToString(), tl ? tl.diffuseRemapMax : new Vector4(1, 1, 1, 1));
			mat.SetVector("_DiffuseRemapOffset" + n.ToString(), tl ? tl.diffuseRemapMin : new Vector4(0, 0, 0, 0));
			mat.SetFloat("_Smoothness" + n.ToString(), tl ? tl.smoothness : 0.0f);
			mat.SetFloat("_Metallic" + n.ToString(), tl ? tl.metallic : 0.0f);
			mat.SetFloat("_NormalScale" + n.ToString(), tl ? tl.normalScale : 0.0f);
			mat.SetColor("_Specular" + n.ToString(), tl ? tl.specular : new Color(0, 0, 0, 0)); 
			mat.SetFloat("_LayerHasMask" + n.ToString(), tl ? (float)(tl.maskMapTexture ? 1.0 : 0.0) : (float)0.0);			
		}

		public static void TerrainLaeyrDataToTerrain(Vector3 size, TerrainLayer tl, int n, MaterialPropertyBlock mtb)
		{
			#if UNITY_EDITOR
				TextureAlphaSmoothness(tl);
			#endif
			
			mtb.SetTexture("_Splat" + n.ToString(), tl && tl.diffuseTexture ? tl.diffuseTexture : Texture2D.whiteTexture);
			mtb.SetTexture("_Normal" + n.ToString(), tl && tl.normalMapTexture ? tl.normalMapTexture : Texture2D.normalTexture);
			mtb.SetTexture("_Mask" + n.ToString(), tl && tl.maskMapTexture ? tl.maskMapTexture : Texture2D.linearGrayTexture);
			mtb.SetVector("_Splat" + n.ToString() + "_ST", tl ? new Vector4(size.x / tl.tileSize.x, size.z / tl.tileSize.y, tl.tileOffset.x, tl.tileOffset.y) : new Vector4(1, 1, 0, 0));
			mtb.SetVector("_MaskMapRemapScale" + n.ToString(), tl ? tl.maskMapRemapMax - tl.maskMapRemapMin : new Vector4(1, 1, 1, 1));
			mtb.SetVector("_MaskMapRemapOffset" + n.ToString(), tl ? tl.maskMapRemapMin : new Vector4(0, 0, 0, 0));
			mtb.SetVector("_DiffuseRemapScale" + n.ToString(), tl ? tl.diffuseRemapMax - tl.diffuseRemapMin : new Vector4(1, 1, 1, 1));
			mtb.SetVector("_DiffuseRemapOffset" + n.ToString(), tl ? tl.diffuseRemapMin : new Vector4(0, 0, 0, 0));
			mtb.SetFloat("_NormalScale" + n.ToString(), tl ? tl.normalScale : 1);
			mtb.SetFloat("_Smoothness" + n.ToString(), tl ? tl.smoothness : 0);
			mtb.SetFloat("_Metallic" + n.ToString(), tl ? tl.metallic : 0);
			mtb.SetColor("_Specular" + n.ToString(), tl ? tl.specular : new Color(0, 0, 0, 0));
			mtb.SetFloat("_LayerHasMask" + n.ToString(), tl ? (float)(tl.maskMapTexture ? 1.0 : 0.0) : (float)0.0);
			mtb.SetVector("_TerrainSizeXZPosY", new Vector3(size.x, size.z, 0.0f));
		}

		public static void CheckLayerIndex(TerrainLayer[] layers, int n, Material mat, ref int layerIndex)
		{
			bool diffuse = mat.shader.name == DiffuseObjectShaderName;
			foreach (TerrainLayer tl in layers)
			{
				bool equal = tl && mat.GetTexture("_Splat" + n.ToString()) == tl.diffuseTexture
				&& mat.GetTexture("_Normal" + n.ToString()) == tl.normalMapTexture
				&& mat.GetVector("_TerrainNormalScale")[n] == tl.normalScale
				&& mat.GetTexture("_Mask" + n.ToString()) == tl.maskMapTexture
				&& mat.GetVector("_SplatUV" + n.ToString()) == new Vector4(tl.tileSize.x, tl.tileSize.y, tl.tileOffset.x, tl.tileOffset.y)
				&& mat.GetVector("_MaskMapRemapScale" + n.ToString()) == tl.maskMapRemapMax - tl.maskMapRemapMin
				&& mat.GetVector("_MaskMapRemapOffset" + n.ToString()) == tl.maskMapRemapMin
				&& mat.GetVector("_DiffuseRemapScale" + n.ToString()) == tl.diffuseRemapMax
				&& mat.GetVector("_DiffuseRemapOffset" + n.ToString()) == tl.diffuseRemapMin;

				bool equalMetallicSmooth = diffuse || tl && mat.GetVector("_TerrainMetallic")[n] == tl.metallic
				&& mat.GetVector("_TerrainSmoothness")[n] == tl.smoothness;

				if (equal && equalMetallicSmooth)
				{
					layerIndex = layers.ToList().IndexOf(tl);
					mat.SetFloat("_LayerIndex" + (n + 1).ToString(), layerIndex);
				}
			}
		}

		static void SetTerrainFloatsToMaterial(Material terrainMaterial, Material objectMaterial, string[] properties)
		{
			foreach (string prop in properties)
			{
				objectMaterial.SetFloat(prop, terrainMaterial.GetFloat(prop));
			}
		}

		static void SetTerrainVectorsToMaterial(Material terrainMaterial, Material objectMaterial, string[] vectors)
		{
			foreach (string vec in vectors)
			{
				objectMaterial.SetVector(vec, terrainMaterial.GetVector(vec));
			}
		}

		static void SetTerrainTextureToMaterial(Material terrainMaterial, Material objectMaterial, string[] textures)
		{
			foreach (string texture in textures)
			{
				objectMaterial.SetTexture(texture, terrainMaterial.GetTexture(texture));
				objectMaterial.SetTextureScale(texture, terrainMaterial.GetTextureScale(texture));
				objectMaterial.SetTextureOffset(texture, terrainMaterial.GetTextureOffset(texture));
			}
		}

		static void TerrainKeywordsToMaterial(Material terrainMaterial, Material objectMaterial, string[] keywords)
		{
			foreach (string keyword in keywords)
			{
				if (terrainMaterial.IsKeywordEnabled(keyword))
				{
					objectMaterial.EnableKeyword(keyword);
				}
				else
				{
					objectMaterial.DisableKeyword(keyword);
				}
			}
		}

		static void DisableKeywords(Material mat, string[] keywords)
		{
			foreach (string keyword in keywords)
			{
				mat.DisableKeyword(keyword);
			}
		}
		
		public static InTerra_UpdateAndCheck GetUpdaterScript()
		{
			if (UpdateScript == null)
			{
				if (!Updater)
				{
					if (!GameObject.Find(UpdaterName))
					{
						Updater = new GameObject(UpdaterName);
						Updater.AddComponent<InTerra_UpdateAndCheck>();

						Updater.hideFlags = HideFlags.HideInInspector;
						Updater.hideFlags = HideFlags.HideInHierarchy;
					}
					else
					{
						Updater = GameObject.Find(UpdaterName);
					}
				}

				UpdateScript = Updater.GetComponent<InTerra_UpdateAndCheck>();
			}
			return (UpdateScript);
		}

		public static InTerra_GlobalData GetGlobalData()
		{
			if (GetUpdaterScript().GlobalData == null)
			{
				#if UNITY_EDITOR
				if (Resources.Load<InTerra_GlobalData>("InTerra_GlobalData") == null)
				{
					if (!Directory.Exists(Path.Combine(GetInTerraPath(), "Resources")))
					{
						AssetDatabase.CreateFolder(GetInTerraPath(), "Resources");
					}

					InTerra_GlobalData globalData = ScriptableObject.CreateInstance<InTerra_GlobalData>();
					AssetDatabase.CreateAsset(globalData, GetInTerraPath() + "/Resources/InTerra_GlobalData.asset");
					AssetDatabase.SaveAssets();
				}
				#endif				 

				GetUpdaterScript().GlobalData = Resources.Load<InTerra_GlobalData>("InTerra_GlobalData");
			}
			return GetUpdaterScript().GlobalData;
		}

		public static void TracksUpdate()
		{
			if (Updater != null)
			{
				if (TrackCamera == null)
				{
					if (!Updater.TryGetComponent<Camera>(out Camera c))
                    {
						TrackCamera = Updater.AddComponent<Camera>();
						InTerra_TracksCameraSettings.SetTrackCamera(Updater.GetComponent<Camera>());
					}
					else
                    {
						TrackCamera = Updater.GetComponent<Camera>();
					}					
				}
				else
				{
					TracksUpdateTimeCount += Time.deltaTime;

					if (!TracksCameraUpdate)
					{
						EnableTracksCamera(false);
					}
					else
                    {
						TracksCameraUpdate = false;
						TracksUpdateTimeCount = 0;
					}
					
					if ((TracksUpdateTimeCount >= GetGlobalData().trackUpdateTime) || !initTrack)
					{
						#if USING_HDRP || USING_URP
							TrackCamera.enabled = true;
						#endif

						initTrack = true;
						TracksCameraUpdate = true;
						EnableTracksCamera(true);						

						#if UNITY_EDITOR
							if (GetUpdaterScript().TrackTexture.height != GetGlobalData().trackTextureSize) CreateTrackRenderTexture();
							if (TrackCamera.cullingMask != 1 << GetGlobalData().trackLayer) TrackCamera.cullingMask = 1 << GetGlobalData().trackLayer;

							var view = SceneView.lastActiveSceneView;
					 		if (view != null && EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.ToString() == " (UnityEditor.SceneView)" && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
							{
								TrackCameraPositon = view.camera.transform.position;
								TrackCameraForwardVec = view.camera.transform.forward;
							}
							else if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.ToString() == " (UnityEditor.GameView)" )
							{
								TrackCameraForwardVec = Camera.main.transform.forward;
								TrackCameraPositon = Camera.main.transform.position;
							}
						#else
							TrackCameraForwardVec = Camera.main.transform.forward;
							TrackCameraPositon = Camera.main.transform.position;
						#endif

						float trackArea = GetGlobalData().trackArea;
						if (GetUpdaterScript().TrackTexture == null) CreateTrackRenderTexture();

						Vector3 tcPos = TrackCameraPositon + TrackCameraForwardVec * Mathf.Round((trackArea * 0.5f));
						float roundIndex = trackArea / (GetGlobalData().trackTextureSize * 0.2f);
						tcPos.x = Mathf.Round(tcPos.x / roundIndex) * roundIndex;
						tcPos.y = Mathf.Round(tcPos.y / roundIndex) * roundIndex;
						tcPos.z = Mathf.Round(tcPos.z / roundIndex) * roundIndex;

						Updater.transform.position = new Vector3(tcPos.x, TracksStampYPosition() - 100.0f, tcPos.z);
						
						Shader.SetGlobalVector("_InTerra_TrackPosition", tcPos);

						Shader.SetGlobalFloat("_InTerra_TrackArea", trackArea);
						Shader.SetGlobalTexture("_InTerra_TrackTexture", GetUpdaterScript().TrackTexture);

						TrackCamera.targetTexture = GetUpdaterScript().TrackTexture;
						TrackCamera.orthographicSize = trackArea * 0.5f;
					}
				}
			}
		}

		public static float TracksStampYPosition()
		{
			float positionY = -100.0f;

			if (Terrain.activeTerrain != null)
			{
				positionY = Terrain.activeTerrain.GetPosition().y - 10.0f;
			}
			else if (GetUpdaterScript().MeshTerrainsList.Count > 0)
			{
				positionY = GetUpdaterScript().MeshTerrainsList[0].bounds.min.y - 10.0f;
			}

			return positionY - GetUpdaterScript().TrackDepthIndex;
		}

		private static void EnableTracksCamera(bool enable)
        {
			#if USING_HDRP
			if (TrackCamera.TryGetComponent<HDAdditionalCameraData>(out var cam))
			{
				cam.fullscreenPassthrough = !enable;
			}
			#elif USING_URP
			if (TrackCamera.TryGetComponent<UniversalAdditionalCameraData>(out var cam))
			{
				cam.renderType = enable ? CameraRenderType.Base : CameraRenderType.Overlay;
			}
			#else
				TrackCamera.enabled = enable;
			#endif
        }

		public static void CheckAndUpdate()
		{
			Terrain[] terrains = Terrain.activeTerrains;
			if (GetUpdaterScript() == null) return;
			var meshTerrainList = GetUpdaterScript().MeshTerrainsList;

			if (terrains.Length > 0 || (meshTerrainList != null && meshTerrainList.Count > 0))
			{
				UpdateScript = GetUpdaterScript();
				DictionaryMaterialTerrain materialTerrain = UpdateScript.MaterialTerrain;

				if (materialTerrain != null && materialTerrain.Count > 0)
				{
					Material mat = materialTerrain.Keys.First();

					if (mat && materialTerrain[mat] && !mat.GetTexture("_TerrainHeightmapTexture") && materialTerrain[mat] && materialTerrain[mat].terrainData.heightmapTexture.IsCreated())
					{
						UpdateTerrainData(InTerra_Setting.DictionaryUpdate);
					}
				}
				else if (!UpdateScript.FirstInit)
				{
					if (!InTerra_Setting.DisableAllAutoUpdates) UpdateTerrainData(true);
					UpdateScript.FirstInit = true;
				}


				#if UNITY_EDITOR
					if (!GlobalKeywordsCheck)
					{
						if (!CheckDefinedKeywords()) WriteDefinedKeywords();						
						GlobalKeywordsCheck = true;					
						#if USING_URP
							URPShadersVersionAdjust();
						#endif
					}
				#endif

			#if !(USING_URP || USING_HDRP)
				if(GetGlobalData().eightLayersPass)
			#endif	
				{
					Terrain checkTerrainMPB = GetUpdaterScript().checkTerrain;
					if (checkTerrainMPB != null)
					{
						checkTerrainMPB.GetSplatMaterialPropertyBlock(mtb);
							#if !(USING_URP || USING_HDRP)
								if(!mtb.GetTexture("_Control1") || !mtb.GetTexture("_Splat4"))
							#else
								if(CheckTerrainShader(checkTerrainMPB.materialTemplate) && checkTerrainMPB.materialTemplate.IsKeywordEnabled("_LAYERS_EIGHT") && (!mtb.GetTexture("_Control1") || !mtb.GetTexture("_Splat4")))
							#endif
							{
							TerrainMaterialUpdate();
							}
					}						
				}
			}
			if (GetUpdaterScript().TracksEnabled)
			{
				TracksUpdate();
			}
			else
			{
				if (Updater.TryGetComponent<Camera>(out Camera cam) && cam.enabled)
				{
					cam.enabled = false;
				}
			}
		}

		static public void CreateTrackRenderTexture()
		{
			if(GetUpdaterScript().TrackTexture != null)
            {
				GetUpdaterScript().TrackTexture.Release();
			}

			int tracksTexSize = GetGlobalData().trackTextureSize;
			GetUpdaterScript().TrackTexture = new RenderTexture(tracksTexSize, tracksTexSize, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) { name = "TrackTexture", enableRandomWrite = true};

			GetUpdaterScript().TrackTexture.Create();
		}

		public static bool TracksFadingEnabled()
		{
			return GetUpdaterScript().TracksFading;
		}

		public static void SetTracksFading(bool enable)
		{
			GetUpdaterScript().TracksFading = enable;
		}

		public static void SetTracksFadingTime(float time)
		{
			GetUpdaterScript().TracksFadingTime = time;
		}


	#if UNITY_EDITOR
		static public void WriteDefinedKeywords()
		{
			if (File.Exists(GetKeywordsPath()))
			{ 
				string[] lines = File.ReadAllLines(GetKeywordsPath());
				InTerra_GlobalData data = GetGlobalData();

				lines[DEFINE_MASKMAP_LineNumber] = DefineKeywordLine(data.maskMapMode == 1, "_TERRAIN_MASK_MAPS");
				lines[DEFINE_NORMAL_MASK_LineNumber] = DefineKeywordLine(data.maskMapMode == 2, "_TERRAIN_NORMAL_IN_MASK");
				lines[DEFINE_HEIGTH_ONLY_LineNumber] = DefineKeywordLine(data.maskMapMode == 3, "_TERRAIN_MASK_HEIGHTMAP_ONLY"); 

				lines[RESTRICT_NORMALMAP_LineNumber] = DefineKeywordLine(!data.disableNormalmap, "_NORMALMAPS");
				lines[RESTRICT_HEIGHTBLEND_LineNumber] = DefineKeywordLine(!data.disableHeightmapBlending, "_TERRAIN_BLEND_HEIGHT");
				lines[RESTRICT_TERR_PARALAX_LineNumber] = DefineKeywordLine(!data.disableTerrainParallax, "_TERRAIN_PARALLAX");
				lines[RESTRICT_TRACKS_LineNumber] = DefineKeywordLine(!data.disableTracks, "_TRACKS");
				lines[RESTRICT_OBJ_PARALLAX_LineNumber] = DefineKeywordLine(!data.disableObjectParallax, "_OBJECT_PARALLAX");
				lines[RESTRICT_PUDDLES_LineNumber] = DefineKeywordLine(!data.disablePuddles, "_PUDDLES");
				#if !(USING_URP || USING_HDRP)
					lines[EIGHT_LAYERS_PASS_LineNumber] = DefineKeywordLine(data.eightLayersPass, "_LAYERS_EIGHT");
				#endif
				File.WriteAllLines(GetKeywordsPath(), lines);

				EditorApplication.delayCall += () =>
				{
					File.WriteAllLines(GetKeywordsPath(), lines);
					EditorApplication.delayCall += () =>
					{
						AssetImporter ai = AssetImporter.GetAtPath(GetKeywordsPath());
						ai.SaveAndReimport();
					};
				};
			}
		}

		static public bool CheckDefinedKeywords()
		{
			InTerra_GlobalData data = GetGlobalData();

			return
				#if !(USING_URP || USING_HDRP)
					DefinedKeyword(EIGHT_LAYERS_PASS_LineNumber) == (data.eightLayersPass) &&
				#endif
				DefinedKeyword(DEFINE_MASKMAP_LineNumber) == (data.maskMapMode == 1) &&
				DefinedKeyword(DEFINE_NORMAL_MASK_LineNumber) == (data.maskMapMode == 2) &&
				DefinedKeyword(DEFINE_HEIGTH_ONLY_LineNumber) == (data.maskMapMode == 3) &&
				DefinedKeyword(RESTRICT_NORMALMAP_LineNumber) != data.disableNormalmap &&
				DefinedKeyword(RESTRICT_HEIGHTBLEND_LineNumber) != data.disableHeightmapBlending &&
				DefinedKeyword(RESTRICT_TERR_PARALAX_LineNumber) != data.disableTerrainParallax &&
				DefinedKeyword(RESTRICT_TRACKS_LineNumber) != data.disableTracks &&
				DefinedKeyword(RESTRICT_OBJ_PARALLAX_LineNumber) != data.disableObjectParallax &&
				DefinedKeyword(RESTRICT_PUDDLES_LineNumber) != data.disablePuddles;
		}

		#if USING_URP
			static public void URPShadersVersionAdjust()
			{
				if (File.Exists(GetKeywordsPath()))
				{
					string[] globalLines = File.ReadAllLines(GetKeywordsPath());
					#if UNITY_2022_2_OR_NEWER
						globalLines[URP_VERSION_2022_2_LineNumber] = "#define UNITY_2022_2_OR_NEWER";
					#else
						globalLines[URP_VERSION_2022_2_LineNumber] = " ";
					#endif

					if(File.ReadAllLines(GetKeywordsPath())[URP_VERSION_2022_2_LineNumber] != globalLines[URP_VERSION_2022_2_LineNumber])
					{
						EditorApplication.delayCall += () =>
						{
							File.WriteAllLines(GetKeywordsPath(), globalLines);
							EditorApplication.delayCall += () =>
							{
								AssetImporter ai = AssetImporter.GetAtPath(GetKeywordsPath());
								ai.SaveAndReimport();
							};
						};
					}
				}
			}
		#endif

		public static string GetInTerraPath()
		{			
			if(string.IsNullOrEmpty(InTerraPath) || !Directory.Exists(InTerraPath))
            {
				string[] guids = AssetDatabase.FindAssets($"t:script InTerra_Data");
				if (guids == null || guids.Length == 0) return null;
				string relativePath = AssetDatabase.GUIDToAssetPath(guids[0]);
				InTerraPath = Path.GetDirectoryName(Path.GetDirectoryName(relativePath));
			}
			return InTerraPath;
		}

		public static string GetKeywordsPath()
		{
			#if USING_HDRP
				string hdrpGlobal = Path.Combine(GetInTerraPath(), "HDRP", "Shaders", "InTerra_HDRP_DefinedGlobalKeywords.hlsl");
				if (File.Exists(hdrpGlobal))
				{ return hdrpGlobal; }
				else
				{	//There was a typo in folder name in some versions, but updating package will not update the old name of folder, therfore there is a folowing alternative path.
					return Path.Combine(GetInTerraPath(), "HRDP", "Shaders", "InTerra_HDRP_DefinedGlobalKeywords.hlsl"); 				
				}							
			#elif USING_URP
				return Path.Combine(GetInTerraPath(), "URP", "Shaders", "InTerra_URP_DefinedGlobalKeywords.hlsl");
			#else
				return Path.Combine(GetInTerraPath(), "Built-in", "Shaders", "InTerra_DefinedGlobalKeywords.cginc");
			#endif
		}


		static string DefineKeywordLine(bool set, string keyword)
		{			
			return (set ? "#define " : "#undef ") + keyword;
		}

		public static bool DefinedKeyword(int line)
		{
			if (!File.Exists(GetKeywordsPath())) return false;
			string[] definedKeywordsLines = File.ReadAllLines(GetKeywordsPath());
			bool defined;

			if (definedKeywordsLines[line].Contains("define"))
			{
				defined = true;
			}
			else
			{
				defined = false;
			}
			return defined;
		}
	#endif

		public static void TerrainMaterialUpdate()
		{
			Terrain[] terrains = Terrain.activeTerrains;
			bool tracksEnabled = false;
 
			foreach (Terrain terrain in terrains)
			{
				if (terrain && terrain.terrainData && terrain.materialTemplate && CheckTerrainShader(terrain.materialTemplate))
				{
					terrain.materialTemplate.SetVector("_TerrainSizeXZPosY", new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.z, terrain.transform.position.y));

					if(terrain.materialTemplate.GetFloat("_Tracks") == 1)
                    {
						tracksEnabled = true;
						#if UNITY_EDITOR
							terrain.materialTemplate.SetFloat("_Gamma", (PlayerSettings.colorSpace == ColorSpace.Gamma ? 1.0f : 0.0f));
						#endif
                    }						
				}
				mtb.Clear();
				mtb.SetFloat("_NumLayersCount", terrain.terrainData.alphamapLayers);

				#if !(USING_URP || USING_HDRP)
					if(GetGlobalData().eightLayersPass)
				#else
					if(terrain.materialTemplate.IsKeywordEnabled("_LAYERS_EIGHT"))
				#endif			
				{
					for (int i = 0; i < terrain.terrainData.alphamapLayers && i < 8; i++)
					{
						TerrainLaeyrDataToTerrain(terrain.terrainData.size, terrain.terrainData.terrainLayers[i], i, mtb);
					}

					mtb.SetTexture("_Control", terrain.terrainData.alphamapTextureCount > 0 ? terrain.terrainData.alphamapTextures[0] : Texture2D.redTexture);
					mtb.SetTexture("_Control1", terrain.terrainData.alphamapTextureCount > 1 ? terrain.terrainData.alphamapTextures[1] : Texture2D.blackTexture);

					terrain.SetSplatMaterialPropertyBlock(mtb);
					terrain.terrainData.SetBaseMapDirty();
					GetUpdaterScript().checkTerrain = terrain;
					#if UNITY_EDITOR
						EditorUtility.SetDirty(GetUpdaterScript());
					#endif
				}
				else
				{
					mtb.Clear();
					mtb.SetFloat("_NumLayersCount", terrain.terrainData.alphamapLayers);
					terrain.SetSplatMaterialPropertyBlock(mtb);
				}
				
			}

			foreach (var meshTerrain in GetUpdaterScript().MeshTerrainsList)
			{
				if (meshTerrain != null && CheckMeshTerrainShader(meshTerrain.sharedMaterial) && meshTerrain.bounds != null && meshTerrain.transform)
				{
					meshTerrain.sharedMaterial.SetVector("_TerrainSizeXZPosY", new Vector3(meshTerrain.bounds.size.x, meshTerrain.bounds.size.z, meshTerrain.transform.position.y));

					if(meshTerrain.sharedMaterial.GetFloat("_Tracks") == 1)
					{
						tracksEnabled = true;
					}
				}
			}
		

			if(GetUpdaterScript().TracksEnabled != tracksEnabled)
            {
				GetUpdaterScript().TracksEnabled = tracksEnabled;
				#if UNITY_EDITOR
					EditorUtility.SetDirty(GetUpdaterScript());
				#endif
			}

			if (GetUpdaterScript().SetSceneWetnessValues)
			{
				Shader.SetGlobalFloat("_InTerra_GlobalWetness", GetUpdaterScript().GlobalWetness);
				Shader.SetGlobalVector("_InTerra_GlobalPuddles", GetUpdaterScript().GlobalPuddles);
				Shader.SetGlobalVector("_InTerra_GlobalRaindropRipples", GetUpdaterScript().GlobalRaindropRipples);
			}
		}

		public static void SetMeshTerrainPositionAndSize(Material mat, MeshRenderer meshTerrain)
		{
			List<MeshRenderer> meshTerrainsList = GetUpdaterScript().MeshTerrainsList;
			meshTerrain.TryGetComponent<InTerra_MeshTerrainData>(out var mtd);

			if (meshTerrainsList.Count > 1)
			{
				Vector3 positonMin = meshTerrain.bounds.min;
				Vector3 positonMax = meshTerrain.bounds.max;

				if (mtd && mat.GetFloat("_HeightmapBase") == 0)
                {
					positonMin.y = meshTerrain.transform.position.y;
				}

				foreach (var mt in meshTerrainsList)
				{
					if(mt && mt.sharedMaterial == mat)
					{ 
						positonMin.x = mt.bounds.min.x < positonMin.x ? mt.bounds.min.x : positonMin.x;						
						positonMin.z = mt.bounds.min.z < positonMin.z ? mt.bounds.min.z : positonMin.z;

						switch (mat.GetFloat("_HeightmapBase"))
						{
							case 0:
								positonMin.y = mt.transform.position.y < positonMin.y ? mt.transform.position.y : positonMin.y;
								break;
							case 1:
								positonMin.y = mt.bounds.min.y < positonMin.y ? mt.bounds.min.y : positonMin.y;
								break;
							case 2:
								positonMin.y = mat.GetFloat("_HeightmapBaseCustom");
								break;
						}
						 
						positonMax.x = mt.bounds.max.x > positonMax.x ? mt.bounds.max.x : positonMax.x;
						positonMax.y = mt.bounds.max.y > positonMax.y ? mt.bounds.max.y : positonMax.y;
						positonMax.z = mt.bounds.max.z > positonMax.z ? mt.bounds.max.z : positonMax.z;					
					}
				}

				mat.SetVector("_TerrainSize", new Vector3(positonMax.x - positonMin.x, positonMax.y - positonMin.y, positonMax.z - positonMin.z));
				mat.SetVector("_TerrainPosition", positonMin);
			}
			else
			{
				mat.SetVector("_TerrainSize", meshTerrain.bounds.size);

				Vector3 positonMin = meshTerrain.bounds.min;

				switch (mat.GetFloat("_HeightmapBase"))
				{
					case 0:
						positonMin.y = meshTerrain.transform.position.y;
						break;
					case 1:
						positonMin.y = meshTerrain.bounds.min.y;
						break;
					case 2:
						positonMin.y = mat.GetFloat("_HeightmapBaseCustom");
						break;
				}
				
				mat.SetVector("_TerrainPosition", positonMin);
			}
		}


		#if UNITY_EDITOR
			public static void CenterOnMainWin(this UnityEditor.EditorWindow aWin)
			{
				var main = EditorGUIUtility.GetMainWindowPosition();
				var pos = aWin.position;
				float w = (main.width - pos.width) * 0.5f;
				float h = (main.height - pos.height) * 0.5f;
				pos.x = main.x + w;
				pos.y = main.y + h;
				aWin.position = pos;
			}
		#endif
	}

	//The Serialized Dictionary is based on christophfranke123 code from this page https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
	[System.Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TKey> keys = new List<TKey>();

		[SerializeField]
		private List<TValue> values = new List<TValue>();

		// save the dictionary to lists
		public void OnBeforeSerialize()
		{
			keys.Clear();
			values.Clear();
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				keys.Add(pair.Key);
				values.Add(pair.Value);
			}
		}

		// load dictionary from lists
		public void OnAfterDeserialize()
		{
			if (this != null)
            {
				this.Clear();
				if (keys.Count != values.Count)
					throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

				for (int i = 0; i < keys.Count; i++)
					this.Add(keys[i], values[i]);
			}	
		}

	}
}
