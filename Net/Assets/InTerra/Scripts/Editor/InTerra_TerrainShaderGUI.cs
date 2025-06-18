using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEditor;


namespace InTerra
{
	public class InTerra_TerrainShaderGUI : ShaderGUI
	{
		bool setMinMax;
		bool setNormScale;
		bool moveLayer;
		bool pomSetting;
		bool tessSetting;
		bool tessDistances;
		bool layersScales;
		bool colorTintLayers;
		bool colorTintTexture;
		bool normalTintTexture;		
		bool mipMinMax;
		bool normDistMinMax;
		bool trackLayersSetting;
		bool trackDetailSetting;
		bool trackParallaxSetting;
		bool globalWetness;
		bool raindrops;
		bool shaderSetting;
		bool mtList;
		bool applyRestictionsButton;

		static public bool restictiInit;
		bool normalmapsDisabled;
		bool heightBlendingDisabled;
		bool terrainParallaxDisabled;
		bool tracksDisabled;
		bool puddlessDisabled;
		bool objectParallaxDisabled;

		int layerToFirst = 0;
		string shaderName = " ";

		const int PRECISION = 1024;

		static TerrainLayer[] terrainLayers;
		static MaterialProperty[] terrainProperties;
		static MaterialEditor terrainEditor;
		static Material targetMat;

		List<MeshRenderer> sharedMatMeshTerrainsList = new List<MeshRenderer>();

		string[] maskMapLabels = new string[] { "None", "Metallic, AO, Height, Smoothness", "Normal map, AO, Height", "Heightmap Only" };
		string[] heightBaseLabels = new string[] { "Y Position", "Mesh Lowest Point", "Custom Y Position" };
		int numberOfLayers = 1;

		public enum TessellationMode
		{
			[Description("None")] None,
			[Description("Phong")] Phong
		}

		enum RenderTextureSize
		{
			_512 = 512,
			_1024 = 1024,
			_2048 = 2048,
			_4096 = 4096,
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			targetMat = materialEditor.target as Material;
			terrainProperties = properties;
			terrainEditor = materialEditor;

			bool disableUpdates = InTerra_Setting.DisableAllAutoUpdates;
			bool updateDict = InTerra_Setting.DictionaryUpdate;
			InTerra_UpdateAndCheck updaterScript = InTerra_Data.GetUpdaterScript();
			InTerra_GlobalData globalData = InTerra_Data.GetGlobalData();
			List<MeshRenderer> meshTerrainsList = updaterScript.MeshTerrainsList;

			//----------------------------- FONT STYLES ----------------------------------
			var styleButtonBold = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
			var styleBold = new GUIStyle(EditorStyles.boldLabel);
			var styleBigBold = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 };
			

			//----------------------------- TERRAIN ------------------------------------
			Terrain terrain = null;
			MeshRenderer meshTerrain = null;
			GameObject meshTerrainObject = null;
			InTerra_MeshTerrainData meshTerrainData = null;

			if (Selection.activeGameObject != null)
			{
				terrain = Selection.activeGameObject.GetComponent<Terrain>();
				meshTerrain = Selection.activeGameObject.GetComponent<MeshRenderer>();
				meshTerrainObject = Selection.activeGameObject;
			}

			if (!InTerra_Data.CheckMeshTerrainShader(targetMat))
			{
				if (terrain == null)
				{
					if (Terrain.activeTerrain != null)
					{
						terrain = Terrain.activeTerrain;
						EditorGUILayout.HelpBox("No Terrain is selected, setings for Terrain Layers are loaded from active terrain!", MessageType.Info);
					}
					else
					{
						EditorGUILayout.HelpBox("No Terrain is selected, some settings may not be available!", MessageType.Warning);
					}
					if(meshTerrain != null)
                    {
						if (InTerra_Data.CheckTerrainShader(targetMat))
						{
							CheckAndReplaceShader(InTerra_Data.TerrainShaderName, InTerra_Data.MeshTerrainShaderName);
							CheckAndReplaceShader(InTerra_Data.DiffuseTerrainShaderName, InTerra_Data.DiffuseMeshTerrainShaderName);
							CheckAndReplaceShader(InTerra_Data.URPTerrainShaderName, InTerra_Data.URPMeshTerrainShaderName);
							CheckAndReplaceShader(InTerra_Data.HDRPTerrainShaderName, InTerra_Data.HDRPMeshTerrainShaderName);
							CheckAndReplaceShader(InTerra_Data.HDRPTerrainTessellationShaderName, InTerra_Data.HDRPMeshTerrainTessellationShaderName);
						}	
					}

				}
				if (terrain != null)
				{
					terrainLayers = terrain.terrainData.terrainLayers;
				}
			}
			else
            {
				EditorGUILayout.HelpBox("Mesh Terrain shaders are currently in a Preview stage!", MessageType.Warning);
				if (terrain != null)
				{
					if (InTerra_Data.CheckMeshTerrainShader(targetMat))
					{
						CheckAndReplaceShader(InTerra_Data.MeshTerrainShaderName, InTerra_Data.TerrainShaderName);
						CheckAndReplaceShader(InTerra_Data.DiffuseMeshTerrainShaderName, InTerra_Data.DiffuseTerrainShaderName);
						CheckAndReplaceShader(InTerra_Data.URPMeshTerrainShaderName, InTerra_Data.URPTerrainShaderName);
						CheckAndReplaceShader(InTerra_Data.HDRPMeshTerrainShaderName, InTerra_Data.HDRPTerrainShaderName);
						CheckAndReplaceShader(InTerra_Data.HDRPMeshTerrainTessellationShaderName, InTerra_Data.HDRPTerrainTessellationShaderName);
					}
				}
				else
                {
					if (meshTerrainObject)
					{
						meshTerrainObject.TryGetComponent<InTerra_MeshTerrainData>(out meshTerrainData);
						if (meshTerrainData == null)
						{
							meshTerrainData = meshTerrainObject.AddComponent<InTerra_MeshTerrainData>();
						}
					}
					else
                    {
						if(updaterScript.MeshTerrainsList != null )
                        {
							foreach(var mt in updaterScript.MeshTerrainsList)
							{
								if(mt && mt.sharedMaterial == targetMat)
                                {
									mt.TryGetComponent<InTerra_MeshTerrainData>(out meshTerrainData);
								}								
							}
						}
					}
				}			
			}

			//------- Update when Material shader is changed -------
			if (targetMat.shader.name != shaderName)
			{
				if (!disableUpdates) InTerra_Data.UpdateTerrainData(updateDict);			
				shaderName = targetMat.shader.name;
			}

			//---------------- MASK MAP MODE ----------------
			if (targetMat.shader.name != InTerra_Data.DiffuseTerrainShaderName && targetMat.shader.name != InTerra_Data.DiffuseMeshTerrainShaderName)
			{
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					GUI.backgroundColor = GlobalSettingColor();
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						MaskMapMode();						
					}

					if (globalData.maskMapMode == 1)
					{
						if (GUILayout.Button(LabelAndTooltip("Mask Map Creator", "Open window for creating Mask Map"), styleButtonBold))
						{
							InTerra_MaskCreator.OpenWindow(false);
						}
					}
					else if (globalData.maskMapMode == 2)
					{
						if (GUILayout.Button(LabelAndTooltip("Normal-Mask Map Creator", "Open window for creating Mask Map including Normal map."), styleButtonBold))
						{
							InTerra_MaskCreator.OpenWindow(true);
						}

						EditorGUI.indentLevel = 1;
						setNormScale = EditorGUILayout.Foldout(setNormScale, "Normal Scales", true);

						if (setNormScale && terrainLayers != null)
						{
							for (int i = 0; i < terrainLayers.Length; i++)
							{
								TerrainLayer tl = terrainLayers[i];
								if (tl)
								{
									float nScale = tl.normalScale;
									EditorGUI.BeginChangeCheck();
									nScale = EditorGUILayout.FloatField((i + 1).ToString() + ". " + tl.name + " :", nScale);
									if (EditorGUI.EndChangeCheck())
									{
										Undo.RecordObject(terrainLayers[i], "InTerra TerrainLayer Normal Scale");
										tl.normalScale = nScale;

										if (meshTerrain != null)
										{
											Undo.RecordObject(targetMat, "InTerra TerrainLayer Normal Scale");
											InTerra_Data.TerrainLaeyrDataToMaterial(tl, i, targetMat);
										}
									}
								}
							}
						}
						EditorGUI.indentLevel = 0;
					}
				}
			}

			//---------------- TESSELLATION ----------------
			if (targetMat.shader.name.Contains("Tessellation"))
			{
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					InTerra_GUI.Tessellation(materialEditor, targetMat, terrainLayers, ref mipMinMax, ref tessDistances, ref tessSetting);
				}
			}
				
			//------------- HEIGHTMAP BLENDING --------------
			bool heightBlending;
			if (globalData.disableHeightmapBlending || (TerrainLayersMaskDisabled() && (targetMat.shader.name != InTerra_Data.DiffuseTerrainShaderName)))
			{
				GUI.enabled = false;
				heightBlending = false;
			}
			else
			{
				heightBlending = targetMat.GetFloat("_HeightmapBlending") > 0;
			}
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				InTerra_GUI.HeightmapBlending(heightBlending, materialEditor, targetMat, "Heightmap Blending", "Heightmap based texture transition.");

				#if !USING_HDRP
					if (heightBlending && terrain != null && terrain.terrainData.alphamapLayers > 4 && numberOfLayers == 1)			
					{
						EditorGUILayout.HelpBox("Heightmap Blending is applied only to one shader pass, which is now four Layers, but you can select \"Eight Layers - Single Pass\" in \"Terrain Layers\" enum.", MessageType.Info);
					}
				#endif
				GUI.enabled = true;
			}

			//---------------- PARALLAX ----------------
			if (targetMat.shader.name != InTerra_Data.DiffuseTerrainShaderName && targetMat.shader.name != InTerra_Data.DiffuseMeshTerrainShaderName)
			{
				if (!targetMat.shader.name.Contains("Tessellation"))
				{
					bool parallax;
					if (globalData.disableTerrainParallax || TerrainLayersMaskDisabled())
					{
						parallax = false;
						GUI.enabled = false;
					}
					else
					{
						parallax = targetMat.GetFloat("_Terrain_Parallax") == 1;
					}
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						InTerra_GUI.ParallaxOcclusionMapping(parallax, materialEditor, targetMat, terrainLayers, meshTerrain != null, ref pomSetting, ref mipMinMax);
					}
					GUI.enabled = true;
				}
			}
			
			//========================= HIDE TILING (DISTANCE BLENDING) ========================
			bool distanceBlending = targetMat.IsKeywordEnabled("_TERRAIN_DISTANCEBLEND");
			Vector4 distance = targetMat.GetVector("_HT_distance");
			
			using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
			{ 
				EditorGUI.BeginChangeCheck();
				EditorStyles.label.fontStyle = FontStyle.Bold;
				distanceBlending = EditorGUILayout.ToggleLeft(LabelAndTooltip("Hide Tiling", "Hides tiling by covering the texture by its scaled up version in the given distance from the camera."), distanceBlending);
				EditorStyles.label.fontStyle = FontStyle.Normal;

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra HideTiling Keyword");
					SetKeyword("_TERRAIN_DISTANCEBLEND", distanceBlending);
					if (!disableUpdates) InTerra_Data.UpdateTerrainData(updateDict);
				}

				EditorGUI.BeginChangeCheck();
				if (distanceBlending)
				{
					using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
					{					
						PropertyLine("_HT_distance_scale", "Scale", "This value is multiplying the scale of the Texture of a distant area.");
						EditorGUI.indentLevel = 1;
						layersScales = EditorGUILayout.Foldout(layersScales, "Adjust Layers Scales", true);
						if (layersScales && terrainLayers != null)
						{
							for (int i = 0; i < terrainLayers.Length; i++)
							{
								TerrainLayer tl = terrainLayers[i];
								if (tl)
								{
									Vector4 scale = tl.diffuseRemapMin;
									EditorGUI.BeginChangeCheck();
									scale.x = EditorGUILayout.Slider((i + 1).ToString() + ". " + tl.name + " :", scale.x, -1, 1);
									if (EditorGUI.EndChangeCheck())
									{
										Undo.RecordObject(terrainLayers[i], "InTerra Terrain Layers Hide Tiling Scales");
										tl.diffuseRemapMin = scale;
										if(meshTerrain != null)
                                        {
											Undo.RecordObject(targetMat, "InTerra Terrain Layers Hide Tiling Scales");
											InTerra_Data.TerrainLaeyrDataToMaterial(tl, i, targetMat);
										}
									}
								}
							}
						}
						EditorGUI.indentLevel = 0;

						PropertyLine("_HT_cover", "Cover strength", "Strength of covering the Terrain textures in the distant area.");
						distance = InTerra_GUI.MinMaxValues(distance, true, true, ref setMinMax);
					}

					//========================= WORLD MAPPING ===========================
					bool worldMapping = targetMat.GetFloat("_WorldMapping") == 1;

					EditorGUI.BeginChangeCheck();

					EditorStyles.label.fontSize = 10;
					worldMapping = EditorGUILayout.ToggleLeft(LabelAndTooltip("World Mapping of Terrain Layers", "This option is useful if you have multiple Terrains connected to prevent possible seams at Terrain edges."), worldMapping);
					EditorStyles.label.fontSize = 12;
					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra World Mapping");
						targetMat.SetFloat("_WorldMapping", worldMapping ? 1.0f : 0.0f);
					}
				}

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra HideTiling Value");
					targetMat.SetVector("_HT_distance", distance);
				}			
			}

			//============================= TRIPLANAR ============================= 
			bool triplanar = targetMat.IsKeywordEnabled("_TERRAIN_TRIPLANAR") || targetMat.IsKeywordEnabled("_TERRAIN_TRIPLANAR_ALL") || targetMat.IsKeywordEnabled("_TERRAIN_TRIPLANAR_ONE");
			bool triplanarOneLayer = targetMat.IsKeywordEnabled("_TERRAIN_TRIPLANAR_ONE");
			bool applyFirstLayer = targetMat.GetFloat("_TriplanarOneToAllSteep") == 1;

			using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
			{
				EditorGUI.BeginChangeCheck();

				EditorStyles.label.fontStyle = FontStyle.Bold;
				triplanar = EditorGUILayout.ToggleLeft(LabelAndTooltip("Triplanar Mapping", "The Texture on steep slopes of Terrain will not be stretched."), triplanar);
				EditorStyles.label.fontStyle = FontStyle.Normal;
				if (triplanar)				
				{				
					if (terrain)
                    {
						targetMat.SetVector("_TerrainSize", terrain.terrainData.size); //needed for triplanar UV
					}
					if(meshTerrain)
                    {
						targetMat.SetVector("_TerrainSize", meshTerrain.bounds.size);
					}										
				}

				if (triplanar)
				{					
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						PropertyLine("_TriplanarSharpness", "Sharpness", "Sharpness of the textures transitions between planar projections.");
						triplanarOneLayer = EditorGUILayout.ToggleLeft(LabelAndTooltip("First Layer Only", "Only the first Terrain Layer will be triplanared - this option is for performance reasons."), triplanarOneLayer, GUILayout.MaxWidth(115));

						if (triplanarOneLayer)
						{							
							EditorGUI.indentLevel = 1;
							EditorStyles.label.fontSize = 11;
							applyFirstLayer = EditorGUILayout.ToggleLeft(LabelAndTooltip("Apply first Layer to all steep slopes", "The first Terrain Layer will be automaticly applied to all steep slopes."), applyFirstLayer);
							EditorStyles.label.fontSize = 12;

							if (!targetMat.shader.name.Contains("Mesh"))
							{
								if (terrain && terrain.terrainData.alphamapLayers > 1)
								{
									MoveTerrainLayerToFIrst();
								}
							}							
						}
					}
				}
				EditorGUI.indentLevel = 0;

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Triplanar Terrain");
					SetKeyword("_TERRAIN_TRIPLANAR_ONE", triplanar && triplanarOneLayer);
					SetKeyword("_TERRAIN_TRIPLANAR", triplanar && !triplanarOneLayer);
					SetKeyword("_TERRAIN_TRIPLANAR_ALL", triplanar && !triplanarOneLayer);
					if (applyFirstLayer && triplanar && triplanarOneLayer) targetMat.SetFloat("_TriplanarOneToAllSteep", 1); else targetMat.SetFloat("_TriplanarOneToAllSteep", 0);
					InTerra_Data.TerrainMaterialUpdate();
				}
			}

			//========================= TRACKS ===========================
			bool track = targetMat.GetFloat("_Tracks") == 1;

			if (globalData.disableTracks)
			{
				track = false;
				targetMat.SetFloat("_Tracks", 0.0f);
				GUI.enabled = false;
			}

			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUI.BeginChangeCheck();

				EditorStyles.label.fontStyle = FontStyle.Bold;
				track = EditorGUILayout.ToggleLeft(LabelAndTooltip("Tracks", "Enable Tracks on this Terrain."), track);

				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Tracks Enable");
					targetMat.SetFloat("_Tracks", track ? 1.0f : 0.0f);

					InTerra_Data.TerrainMaterialUpdate();

					if (!disableUpdates) InTerra_Data.UpdateTerrainData(updateDict);
				}

				EditorStyles.label.fontStyle = FontStyle.Normal;

				if (track)
				{
					EditorGUILayout.HelpBox("Objects that are supposed to create tracks needs to have the InTerra Tracks script attached!", MessageType.Info);

					GUI.backgroundColor = GlobalSettingColor();

					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GlobalSettitngLabel();
						EditorGUILayout.Space();

						float trackArea = globalData.trackArea;
						int textureSize = globalData.trackTextureSize;
						float trackTime = globalData.trackUpdateTime;

						LayerMask trackLayer = globalData.trackLayer;

						EditorGUI.BeginChangeCheck();
						trackArea = EditorGUILayout.FloatField(LabelAndTooltip("Area Size", "Size of area around camera where Tracks will be visible."), trackArea);
						trackArea = Mathf.Clamp(trackArea, 30, 100);
						trackTime = EditorGUILayout.FloatField(LabelAndTooltip("Update Time", "Time interval in seconds for updating the Tracks."), trackTime);
						trackTime = Mathf.Clamp(trackTime, 0, 10);

						if (EditorGUI.EndChangeCheck())
						{
							Undo.RecordObject(globalData, "InTerra Tracks Values");
							Undo.RecordObject(updaterScript.GetComponent<Camera>(), "InTerra Tracks Values");						
							globalData.trackArea = trackArea;
							globalData.trackUpdateTime = trackTime;
							EditorUtility.SetDirty(globalData);
						}

						EditorGUI.BeginChangeCheck();
						RenderTextureSize ts = (RenderTextureSize)textureSize;
						ts = (RenderTextureSize)EditorGUILayout.EnumPopup(LabelAndTooltip("Render Texture Size", "Size of the Render Texture for capturing the Tracks."), ts);

						textureSize = (int)ts;
						trackLayer = EditorGUILayout.LayerField(LabelAndTooltip("Track Layer", "Layer for rendering tracks, it is needed for this Layer to be exclusively used just for this feature."), trackLayer);


						if (EditorGUI.EndChangeCheck())
						{
							Undo.RecordObject(globalData, "InTerra Tracks Values");
							InTerra_Data.GetGlobalData().trackTextureSize = textureSize;
							globalData.trackTextureSize = textureSize;
							if (updaterScript.TrackTexture)
							{
								InTerra_Data.CreateTrackRenderTexture();
							}

							globalData.trackLayer = trackLayer;
							EditorUtility.SetDirty(globalData);
							updaterScript.GetComponent<Camera>().cullingMask = 1 << InTerra_Data.GetGlobalData().trackLayer;
						}

						if (trackLayer.value == 0)
						{
							EditorGUILayout.HelpBox("Please create and select a Layer that will be used for Tracks only!", MessageType.Warning);
						}	
						GUI.backgroundColor = Color.white;			
					}

					EditorGUILayout.Space();

					float fadingTime = updaterScript.TracksFadingTime;
					bool fading = InTerra_Data.TracksFadingEnabled();

					EditorGUI.BeginChangeCheck();
					
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						fading = EditorGUILayout.ToggleLeft(LabelAndTooltip("Fade in Time", "Enable Tracks to disappear in a given time, this setting is applied to the whole scene."), fading);

						EditorGUI.indentLevel = 1;
						if (fading)
						{
							fadingTime = EditorGUILayout.FloatField(LabelAndTooltip("Fading Time", "Time in seconds for tracks to completely disappear, this setting is applied to the whole scene."), fadingTime);
							fadingTime = Mathf.Max(fadingTime, 1.0F);
						}
						EditorGUI.indentLevel = 0;
					}
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(updaterScript, "InTerra Tracks Fading");
						InTerra_Data.SetTracksFading(fading);
						InTerra_Data.SetTracksFadingTime(fadingTime);
					}

					EditorGUILayout.Space();


					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GUILayout.Label("Normals:", styleBold);
						PropertyLine("_TrackNormalStrenght", "Normals Strength", "Strength of normals calculated from tracks heightmap.");

						PropertyLine("_TrackEdgeSharpness", "Edge Sharpness ", "Sharpness of the edge of the tracks.");
						PropertyLine("_TrackEdgeNormals", "Additional Edge ", "Strength of normals for additional edge around tracks.");
						EditorGUILayout.Space();
					}

					if (targetMat.shader.name != InTerra_Data.DiffuseTerrainShaderName)
					{
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							PropertyLine("_TrackAO", "Ambient Occlusion", "Ambient Occlusion for tracks.");
						}
					}

					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						GUILayout.Label("Heightmap Blending:", styleBold);

						PropertyLine("_TrackHeightTransition", "Blending Sharpness", "Sharpness of heightmap blending transition.");


						if (targetMat.GetFloat("_TrackHeightTransition") == 0)
						{
							GUI.enabled = false;
						}
						PropertyLine("_TrackHeightOffset", "Heightmap Offset", "Offset for tracks heightmap.");
						GUI.enabled = true;

						if (targetMat.HasProperty("_TrackTessallationHeightTransition"))
						{
							using (new GUILayout.VerticalScope(EditorStyles.helpBox))
							{
								PropertyLine("_TrackTessallationHeightTransition", "Tessellation Sharpness", "Sharpness of heightmap blending for tessellation of track.");
							}
						}
					}

					//------------------------- TRACK PARALLAX -------------------------
					bool parallax = targetMat.GetFloat("_Terrain_Parallax") == 1;
					if (parallax)
					{
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							EditorGUI.indentLevel = 1;
							trackParallaxSetting = EditorGUILayout.Foldout(trackParallaxSetting, "Parallax Setting", true);
							EditorGUI.indentLevel = 0;
							if (trackParallaxSetting)
							{
								float affineSteps = targetMat.GetFloat("_ParallaxTrackAffineSteps");
								float parallaxSteps = targetMat.GetFloat("_ParallaxTrackSteps");

								EditorGUI.BeginChangeCheck();

								affineSteps = EditorGUILayout.IntField(LabelAndTooltip("Affine Steps: ", "The higher number the smoother transition between steps, but also the higher number will increase performance heaviness."), (int)affineSteps);

								parallaxSteps = EditorGUILayout.IntField(LabelAndTooltip("Parallax Steps:", "Each step is creating a new layer for offsetting. The more steps, the more precise the parallax effect will be, but also the higher number will increase performance heaviness."), (int)parallaxSteps);
								affineSteps = Mathf.Clamp(affineSteps, 1, 10);


								if (EditorGUI.EndChangeCheck())
								{
									materialEditor.RegisterPropertyChangeUndo("InTerra Track Parallax Values");
									targetMat.SetFloat("_ParallaxTrackAffineSteps", affineSteps);
									targetMat.SetFloat("_ParallaxTrackSteps", parallaxSteps);
								}
							}
						}
					}

					//------------------------- TRACK DETAIL ------------------------													
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						EditorGUI.indentLevel = 1;
						trackDetailSetting = EditorGUILayout.Foldout(trackDetailSetting, "Detail Map Textures", true);
						EditorGUI.indentLevel = 0;
						if (trackDetailSetting)
						{
							materialEditor.TexturePropertySingleLine(new GUIContent("Detail Albedo"), FindProperty("_TrackDetailTexture", properties));
							TextureSingleLine("_TrackDetailNormalTexture", "_TrackDetailNormalStrenght", "Normal Map", "Detail Normal Map");
							using (new GUILayout.VerticalScope(EditorStyles.helpBox))
							{
								materialEditor.TextureScaleOffsetProperty(FindProperty("_TrackDetailTexture", properties));
							}
						}
					}

					//------------------------- TRACK LAYERS -------------------------			
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						EditorGUI.indentLevel = 1;
						trackLayersSetting = EditorGUILayout.Foldout(trackLayersSetting, "Layers Setting", true);
						EditorGUI.indentLevel = 0;
						if (trackLayersSetting && terrainLayers != null)
						{

							for (int i = 0; i < terrainLayers.Length && i < 8; i++)
							{
								TerrainLayer tl = terrainLayers[i];
								if (tl)
								{
									Vector4 values = tl.specular;
									Vector2 tintRG = UnpackValues(tl.specular.g);
									Vector2 tintBA = UnpackValues(tl.specular.b);
									Color tint = new Color(tintRG.x, tintRG.y, tintBA.x);

									Vector4 additional = UnpackValues(tl.specular.r);
									Vector4 additional2 = tl.diffuseRemapMin;

									float colorOpacity = tintBA.y;
									float trackSmoothness = additional.x;
									float normalOpacity = additional.y;
									float depth = (additional2.w * 10.0f) % 1;
									float applyDetail = Mathf.Floor((additional2.w % 1.0f) * 10.0f);
									bool detail = applyDetail > 0;

									EditorGUI.BeginChangeCheck();

									using (new GUILayout.VerticalScope(EditorStyles.helpBox))
									{
										using (new GUILayout.HorizontalScope())
										{
											EditorGUILayout.LabelField((i + 1).ToString() + ". " + tl.name, styleBold, GUILayout.MinWidth(100));
										}

										using (new GUILayout.HorizontalScope())
										{
											if (tl && AssetPreview.GetAssetPreview(tl.diffuseTexture))
											{
												GUI.Box(EditorGUILayout.GetControlRect(GUILayout.Width(40), GUILayout.Height(40)), AssetPreview.GetAssetPreview(tl.diffuseTexture));
											}
											using (new GUILayout.VerticalScope())
											{
												tint = EditorGUILayout.ColorField(LabelAndTooltip("Color", "Color tint for the tracks on " + tl.name + " Terrain Layer."), tint, true, false, false);
												colorOpacity = EditorGUILayout.Slider(LabelAndTooltip("Color Opacity", "Opacity strength for the track color on " + tl.name + " Terrain Layer."), colorOpacity, 0, 1);
											}
										}

										normalOpacity = EditorGUILayout.Slider(LabelAndTooltip("Normal Opacity", "Normals Opacity strength for the tracks on " + tl.name + " Terrain Layer."), normalOpacity, 0, 1);

										if (targetMat.shader.name != InTerra_Data.DiffuseTerrainShaderName)
										{
											trackSmoothness = EditorGUILayout.Slider(LabelAndTooltip("Smoothness", "Smoothness strength for the tracks on " + tl.name + " Terrain Layer."), trackSmoothness, 0, 1);
										}

										detail = EditorGUILayout.Toggle(LabelAndTooltip("Apply Detail Maps", "If checked detail maps will be aplied for tracks on " + tl.name + " Terrain Layer."), detail);

										if (parallax || targetMat.shader.name.Contains(InTerra_Data.HDRPTerrainTessellationShaderName))
										{
											depth = depth < 0.005f ? 0.0f : depth;
											depth = depth > 0.995f ? 1.0f : depth;

											depth = EditorGUILayout.Slider(LabelAndTooltip("Depth", "Parallax or tessellation depth for the tracks on " + tl.name + " Terrain Layer."), depth, 0, 1);
										}

										depth = Mathf.Clamp(depth, 0.0001f, 0.999f);
										values.x = PackValues(new Vector2(trackSmoothness, normalOpacity));
										values.y = PackValues(new Vector2(Mathf.Max(tint.r, 0.001f), Mathf.Max(tint.g, 0.001f)));
										values.z = PackValues(new Vector2(Mathf.Max(tint.b, 0.001f), Mathf.Max(colorOpacity, 0.001f)));
									}

									if (EditorGUI.EndChangeCheck())
									{
										Undo.RecordObject(terrainLayers[i], "InTerra Tracks Values");
										materialEditor.RegisterPropertyChangeUndo("InTerra Tracks Values");
										tl.specular = values;
										additional2.w = Mathf.Floor(additional2.w) + (detail ? 0.1f : 0.0f) + (depth * 0.1f);
										tl.diffuseRemapMin = additional2;
										if (meshTerrain != null)
										{
											Undo.RecordObject(targetMat, "InTerra Tracks Values");
											InTerra_Data.TerrainLaeyrDataToMaterial(tl, i, targetMat);
										}
									}
								}
							}
						}
					}
				}
			}
			GUI.enabled = true;

			//========================= COLOR TINT ===========================
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("Color Tint", styleBigBold);
				EditorGUI.indentLevel = 1;
				colorTintTexture = EditorGUILayout.Foldout(colorTintTexture, "Color Tint Texture", true);
				EditorGUI.indentLevel = 0;

				if (colorTintTexture)
				{

					Texture ColorTintTexture = targetMat.GetTexture("_TerrainColorTintTexture");
					float tintStrenght = targetMat.GetFloat("_TerrainColorTintStrenght");

					EditorGUI.BeginChangeCheck();
					using (new GUILayout.HorizontalScope())
					{
						ColorTintTexture = (Texture2D)EditorGUILayout.ObjectField(ColorTintTexture, typeof(Texture2D), false, GUILayout.Height(65), GUILayout.Width(65));
						using (new GUILayout.VerticalScope())
						{ 
							using (new GUILayout.VerticalScope(EditorStyles.helpBox))
							{
								EditorGUILayout.LabelField("Tint Strength:", GUILayout.MinWidth(35));
								tintStrenght = EditorGUILayout.Slider(tintStrenght, 0, 1, GUILayout.MinWidth(35));
								EditorGUILayout.LabelField(" ", GUILayout.MinWidth(35));
							}						
						}
					}
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						materialEditor.TextureScaleOffsetProperty(FindProperty("_TerrainColorTintTexture", properties));
					}	

					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra Terrain Color Tint Texture");
						targetMat.SetTexture("_TerrainColorTintTexture", ColorTintTexture);

						targetMat.SetFloat("_TerrainColorTintStrenght", tintStrenght);
					}
				}

				EditorGUI.indentLevel = 1;
				colorTintLayers = EditorGUILayout.Foldout(colorTintLayers, "Layers Color Tint", true);

				if (colorTintLayers && terrainLayers != null)
				{
					for (int i = 0; i < terrainLayers.Length; i++)
					{
						TerrainLayer tl = terrainLayers[i];
						if (tl)
						{
							Vector4 color = tl.diffuseRemapMax;
							EditorGUI.BeginChangeCheck();
							color = EditorGUILayout.ColorField(new GUIContent() { text = (i + 1).ToString() + ". " + tl.name} , color, true, false, false);

							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(terrainLayers[i], "InTerra Terrain Layer Color Tint");
								tl.diffuseRemapMax = color;
								if (meshTerrain != null)
								{
									Undo.RecordObject(targetMat, "InTerra Terrain Layer Color Tint");
									InTerra_Data.TerrainLaeyrDataToMaterial(tl, i, targetMat);
								}
							}
						}
					}
				}
				EditorGUI.indentLevel = 0;
			}

			//========================= ADDITIONAL NORMAL ===========================
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("Additional Normal", styleBigBold);
				EditorGUI.indentLevel = 1;
				normalTintTexture = EditorGUILayout.Foldout(normalTintTexture, "Additional Normal Texture", true);
				EditorGUI.indentLevel = 0;

				if (normalTintTexture)
				{
					Texture normalTintTexture = targetMat.GetTexture("_TerrainNormalTintTexture");
					float tintStrenght = targetMat.GetFloat("_TerrainNormalTintStrenght");
					Vector4 normalDistance = targetMat.GetVector("_TerrainNormalTintDistance");


					EditorGUI.BeginChangeCheck();
					using (new GUILayout.HorizontalScope())
					{
						normalTintTexture = (Texture2D)EditorGUILayout.ObjectField(normalTintTexture, typeof(Texture2D), false, GUILayout.Height(65), GUILayout.Width(65));
						using (new GUILayout.VerticalScope())
						{
							using (new GUILayout.VerticalScope(EditorStyles.helpBox))
							{
								EditorGUILayout.LabelField("Normal Strenght:", GUILayout.MinWidth(35));
								tintStrenght = EditorGUILayout.Slider(tintStrenght, 0, 1, GUILayout.MinWidth(35));
								EditorGUILayout.LabelField(" ", GUILayout.MinWidth(35));
							}							
						}
					}
					materialEditor.TextureCompatibilityWarning(FindProperty("_TerrainNormalTintTexture", properties));

					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						materialEditor.TextureScaleOffsetProperty(FindProperty("_TerrainNormalTintTexture", properties));	
					}
					using (new GUILayout.VerticalScope(EditorStyles.helpBox))
					{
						EditorGUILayout.LabelField("Starting Distance");
						normalDistance = InTerra_GUI.MinMaxValues(normalDistance, true, false, ref normDistMinMax);
					}

					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra Additional Normal");
						targetMat.SetTexture("_TerrainNormalTintTexture", normalTintTexture);
						targetMat.SetFloat("_TerrainNormalTintStrenght", tintStrenght);
						targetMat.SetVector("_TerrainNormalTintDistance", normalDistance);
					}
				}
			}

			//========================= GLOBAL WETNESS ===========================
			if (targetMat.shader.name != InTerra_Data.DiffuseTerrainShaderName)
            {
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					EditorGUI.indentLevel = 1;
					globalWetness = EditorGUILayout.Foldout(globalWetness, "Global Wetness and Puddles", true);
					EditorGUI.indentLevel = 0;

					if (globalWetness)
					{
						float Wetness = updaterScript.GlobalWetness;
						Vector2 puddles = new Vector2(updaterScript.GlobalPuddles.x, updaterScript.GlobalPuddles.y);
						Vector3 raindropRipplesVector = updaterScript.GlobalRaindropRipples;

						int raindropRipples = (int)updaterScript.GlobalRaindropRipples.x;						
						float riplesStrength = updaterScript.GlobalRaindropRipples.y;
						float riplesSize = updaterScript.GlobalRaindropRipples.z;

						bool setAtSceneLoad = updaterScript.SetSceneWetnessValues;

						

						EditorGUI.BeginChangeCheck();
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							Wetness = EditorGUILayout.Slider(LabelAndTooltip("Wetness:", "Strenght of global wetness."), Wetness, 0.0f, 1.0f);
						}

						GUI.enabled = !puddlessDisabled;
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							EditorGUILayout.LabelField("Puddles Height:");

							using (new GUILayout.HorizontalScope())
							{
								EditorGUILayout.LabelField(puddles.x.ToString("0.0"), GUILayout.Width(33));
								EditorGUILayout.MinMaxSlider(ref puddles.x, ref puddles.y, -0.05f, 1.05f);
								EditorGUILayout.LabelField(puddles.y.ToString("0.0"), GUILayout.Width(33));
								puddles.y = puddles.x + (float)0.001 >= puddles.y ? puddles.y + (float)0.001 : puddles.y;
							}
						
							using (new GUILayout.VerticalScope(EditorStyles.helpBox))
							{
								EditorGUI.indentLevel = 1;
								raindrops = EditorGUILayout.Foldout(raindrops, "Raindrops", true);
								EditorGUI.indentLevel = 0;
								if (raindrops)
								{								
									raindropRipples = EditorGUILayout.IntSlider("Density:", raindropRipples, 0, 5);
									riplesStrength = EditorGUILayout.Slider("Strength:", riplesStrength, 0.1f, 5.0f);
									riplesSize = EditorGUILayout.Slider("Size:", riplesSize, 0.2f, 2.0f);	
								}
								raindropRipplesVector = new Vector3(raindropRipples, riplesStrength, riplesSize);
							}
						}
						GUI.enabled = true;
						setAtSceneLoad = EditorGUILayout.ToggleLeft(LabelAndTooltip("Set Wetness Values at Scene Load", "Check this option if you want the Wetness and Puddles values to be set at scene load."), setAtSceneLoad);

						if (EditorGUI.EndChangeCheck())
						{
							Undo.RecordObject(updaterScript, "InTerra Global Smoothness");
							Shader.SetGlobalFloat("_InTerra_GlobalWetness", updaterScript.GlobalWetness);
							Shader.SetGlobalVector("_InTerra_GlobalPuddles", updaterScript.GlobalPuddles);
							Shader.SetGlobalVector("_InTerra_GlobalRaindropRipples", raindropRipplesVector);
							updaterScript.GlobalRaindropRipples = raindropRipplesVector;
							updaterScript.GlobalPuddles = puddles;
							updaterScript.SetSceneWetnessValues = setAtSceneLoad;
							SceneView.RepaintAll();
							updaterScript.GlobalWetness = Wetness;
						}
					}	
				}
				
			}

			//========================= TERRAIN LAYERS ===========================
			using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
			{
				string[] layersNumberLabels;
				if (terrain != null)
                {
				#if (USING_HDRP)
					layersNumberLabels = new string[] { "Two Layers Only", "Eight Layers - Single Pass" };
				#else
					layersNumberLabels = new string[] { "Two Layers Only", "Four Layers - Multiple Passes", "Eight Layers - Single Pass" };
				#endif
				}
				else
                {
					layersNumberLabels = new string[] { "Two Layers Only", "Four Layers - Single Pass", "Eight Layers - Single Pass" };
				}

				int numberOfLayers = 1;	
								
				#if !(USING_URP || USING_HDRP)
				if (globalData.eightLayersPass)
				#else
				if (targetMat.IsKeywordEnabled("_LAYERS_EIGHT"))
				#endif
				{
					numberOfLayers = 2;

					if(terrain != null)
					{ 
						MaterialPropertyBlock mtb = new();
						terrain.GetSplatMaterialPropertyBlock(mtb);
						mtb.SetFloat("_NumLayersCount", terrain.terrainData.alphamapLayers);

						#if (USING_URP || USING_HDRP)
						if (terrain.materialTemplate.IsKeywordEnabled("_LAYERS_EIGHT"))
						#endif
						{
							for (int i = 0; i < terrain.terrainData.alphamapLayers && i < 8; i++)
							{
								InTerra_Data.TerrainLaeyrDataToTerrain(terrain.terrainData.size, terrain.terrainData.terrainLayers[i], i, mtb);
							}

							terrain.SetSplatMaterialPropertyBlock(mtb);
							InTerra_Data.GetUpdaterScript().checkTerrain = terrain;
							terrain.terrainData.SetBaseMapDirty();
						}
					}
				}

				
				#if !(USING_URP || USING_HDRP)
					else if (targetMat.GetFloat("_TwoLayersOnly") > 0)
				#else
					else if (targetMat.IsKeywordEnabled("_LAYERS_TWO"))
				#endif
				{
					numberOfLayers = 0;
				}

				EditorGUI.BeginChangeCheck();

				numberOfLayers = EditorGUILayout.Popup(LabelAndTooltip("Terrain Layers: ", "Option for Terrain Layers number."), numberOfLayers, layersNumberLabels);
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Terrain Layers Pass");
					Undo.RecordObject(globalData, "InTerra Terrain Layers Pass");

					#if !(USING_URP || USING_HDRP)
					if(globalData.eightLayersPass && numberOfLayers != 2 && (EditorUtility.DisplayDialog("Terrain Layers", "Note: Swithcing from Eight Layers Pass require reiporting the shaders which can take a few minutes.", "Continue", "Cancel")))
                    {
						globalData.eightLayersPass = false;
						EditorUtility.SetDirty(globalData);
						targetMat.SetFloat("_TwoLayersOnly", numberOfLayers == 0 ? 1.0f : 0.0f);
					}
					else
                    {
						if(numberOfLayers == 0)
						{
							targetMat.SetFloat("_TwoLayersOnly", 1.0f);

						}
						else
						{
							targetMat.SetFloat("_TwoLayersOnly", 0.0f);
							if(numberOfLayers == 2 && EditorUtility.DisplayDialog("Eight Layers Pass", "Note: Eight Layers Pass will be applied globaly and require reiporting the shaders which can take a few minutes.", "Continue", "Cancel"))
							{
								globalData.eightLayersPass = true;
								EditorUtility.SetDirty(globalData);
							}
							else
							{
								globalData.eightLayersPass = false;
								EditorUtility.SetDirty(globalData);
							}
						}						
					}
					#else
						SetKeyword("_LAYERS_TWO", numberOfLayers == 0);
						SetKeyword("_LAYERS_EIGHT", numberOfLayers == 2);
					#endif
					

					if (!disableUpdates) InTerra_Data.UpdateTerrainData(updateDict);

				}
			}


			//========================= NORMAL MAPS IN DEPTH PASS (URP) ===========================
			#if USING_URP
			if(targetMat.shader.name == InTerra_Data.URPTerrainShaderName)
			{ 
				bool depthMaps = targetMat.IsKeywordEnabled("_DEPTH_NORMALS_MAPS");

				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					EditorGUI.BeginChangeCheck();

					depthMaps = EditorGUILayout.ToggleLeft(LabelAndTooltip("Normal Maps in Depth Pass (SSAO)", "Allow sampling normal maps in depth pass, you can see the effect if the \"Screen Space Ambient Occlusion\" is enabled."), depthMaps);

					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra Normal Maps in Depth Pass");
						SetKeyword("_DEPTH_NORMALS_MAPS", depthMaps);
					}
				}
			}
			#endif
			EditorGUILayout.Space();


			//========================= MESH TERRAIN SETTING ===========================
			if (InTerra_Data.CheckMeshTerrainShader(targetMat))
			{
				EditorGUILayout.HelpBox("The Mesh Terrain should be placed the same way as Unity Terrain, always aligned with the world axis. Oriented according to Control map and Heightmap sampling which always begins from bottom left corner at min X and Z position to top right corner at max X and Z position!", MessageType.Info);
								
				if (!meshTerrainsList.Contains(meshTerrain))
				{
					meshTerrainsList.Add(meshTerrain);
				}
				else if(!mtList)
                {
					foreach (MeshRenderer mr in meshTerrainsList)
					{
						if (mr && mr.sharedMaterial && targetMat == mr.sharedMaterial)
						{
							sharedMatMeshTerrainsList.Add(mr);

							if (mr != meshTerrain)
							{
								InTerra_MeshTerrainData mtd = mr.GetComponent<InTerra_MeshTerrainData>();
								if (meshTerrainData && meshTerrainData.ControlMap == null && mtd.ControlMap != null)
								{
									meshTerrainData.ControlMap = mtd.ControlMap;
								}
								if (meshTerrainData && meshTerrainData.ControlMap1 == null && mtd.ControlMap1 != null)
								{
									meshTerrainData.ControlMap1 = mtd.ControlMap1;
								}
								if (meshTerrainData && meshTerrainData.HeightMap == null && mtd.HeightMap != null)
								{
									meshTerrainData.HeightMap = mtd.HeightMap;
								}

								for (int i = 0; i < 4; i++)
								{									
									if (meshTerrainData && meshTerrainData.TerrainLayers[i] == null && mtd && mtd.TerrainLayers[i] != null)
									{
										meshTerrainData.TerrainLayers[i] = mtd.TerrainLayers[i];
										InTerra_Data.TerrainLaeyrDataToMaterial(mtd.TerrainLayers[i], i, targetMat);
									}
								}
							}
						}
					}
					mtList = true;
				}

				if (meshTerrainData != null)
				{
					Texture2D controlMap = meshTerrainData.ControlMap;
					Texture2D controlMap1 = meshTerrainData.ControlMap1;
					terrainLayers = meshTerrainData.TerrainLayers;
					MeshRenderer[] mts = sharedMatMeshTerrainsList.ToArray();

					#if !(USING_URP || USING_HDRP)
						if (targetMat.GetFloat("_TwoLayersOnly") > 0)
					#else
						if (targetMat.IsKeywordEnabled("_LAYERS_TWO"))
					#endif
					{
						InTerra_GUI.MeshTerrainSplats(materialEditor, targetMat, terrainLayers, meshTerrain, controlMap, "_Control", new Vector2Int(0, 2), mts);
					}
					#if !(USING_URP || USING_HDRP)
						else if (globalData.eightLayersPass)
					#else
						else if(targetMat.IsKeywordEnabled("_LAYERS_EIGHT"))
					#endif					
					{
						InTerra_GUI.MeshTerrainSplats(materialEditor, targetMat, terrainLayers, meshTerrain, controlMap, "_Control", new Vector2Int(0, 4), mts);
						InTerra_GUI.MeshTerrainSplats(materialEditor, targetMat, terrainLayers, meshTerrain, controlMap1, "_Control1", new Vector2Int(4, 8), mts);
					}
					else
					{
						InTerra_GUI.MeshTerrainSplats(materialEditor, targetMat, terrainLayers, meshTerrain, controlMap, "_Control", new Vector2Int(0, 4), mts);
					}
				
					CheckTextureClampWrapMode(controlMap, "Control Map");
					CheckTextureClampWrapMode(controlMap1, "Control Map 2");

					Texture2D heightmap = (Texture2D)targetMat.GetTexture("_TerrainHeightmapTexture");
					Vector4 heightScale = targetMat.GetVector("_TerrainHeightmapScale");
					int heightBase = (int)targetMat.GetFloat("_HeightmapBase");
					float heightMapBaseCustom = targetMat.GetFloat("_HeightmapBaseCustom");

					using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
					{
						EditorGUI.BeginChangeCheck();

						using (new GUILayout.VerticalScope())
						{
							EditorGUILayout.LabelField("Heightmap", styleBold, GUILayout.MinWidth(35));
							heightmap = (Texture2D)EditorGUILayout.ObjectField(heightmap, typeof(Texture2D), false, GUILayout.Height(100), GUILayout.Width(100));
						}

						using (new GUILayout.VerticalScope())
						{
							EditorGUILayout.LabelField("Heightmap Scale", GUILayout.MinWidth(35));
							heightScale.y = EditorGUILayout.FloatField (heightScale.y);

							EditorGUILayout.Space();

							EditorGUILayout.LabelField("Heightmap Base", GUILayout.MinWidth(35));
							heightBase = EditorGUILayout.Popup(heightBase, heightBaseLabels);
							if(heightBase == 2)
                            {
								heightMapBaseCustom = EditorGUILayout.FloatField(heightMapBaseCustom);
							}
						}

						if (EditorGUI.EndChangeCheck())
						{
							if (heightmap)
							{
								heightScale.x = meshTerrain.bounds.size.x / heightmap.width;
								heightScale.z = meshTerrain.bounds.size.z / heightmap.height;
							}
							else
							{
								heightScale.x = 1;
								heightScale.z = 1;
							}
							heightScale.w = heightScale.y * (32766.0f / 65535.0f);

							materialEditor.RegisterPropertyChangeUndo("InTerra Terrain Heightmap");

							targetMat.SetTexture("_TerrainHeightmapTexture", heightmap);
							targetMat.SetVector("_TerrainHeightmapScale", heightScale);
							targetMat.SetFloat("_HeightmapBase", heightBase);
							targetMat.SetFloat("_HeightmapBaseCustom", heightMapBaseCustom);

							foreach (var mr in mts)
							{
								if (mr.TryGetComponent<InTerra_MeshTerrainData>(out var m))
								{
									var mtd = mr.GetComponent<InTerra_MeshTerrainData>();
									Undo.RecordObject(mtd, "InTerra Terrain Heightmap");

									mtd.HeightMap = heightmap;
								}
							}
						}						
					}
					CheckTextureClampWrapMode(heightmap, "Heightmap");

					if (heightmap && !(heightmap.format == TextureFormat.R16 || heightmap.format == TextureFormat.R8))
					{
						using (new GUILayout.VerticalScope(EditorStyles.helpBox))
						{
							EditorGUILayout.HelpBox("Heightmap texture format should be R8 or R16. \nFormat can be changed in Texture Import setting!", MessageType.Warning);
						}
					}
				}
			}

			EditorGUILayout.Space();

			using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
			{
				if (GUILayout.Button(LabelAndTooltip("Update Terrain Data", "Send updated data from Terrain to Objects integrated to Terrain."), styleButtonBold))
				{
					InTerra_Data.UpdateTerrainData(true);

					//--------- Updating the Materials outside of active Scene ---------
					string[] matGUIDS = AssetDatabase.FindAssets("t:Material", null);

					foreach (string guid in matGUIDS)
					{
						Material mat = (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Material));
						if (mat && mat.shader && mat.shader.name != null && !InTerra_Data.GetUpdaterScript().MaterialTerrain.ContainsKey(mat) && InTerra_Data.CheckObjectShader(mat))
						{
							if (mat.IsKeywordEnabled("_LAYERS_ONE"))
							{
								InTerra_Data.TerrainLaeyrDataToMaterial(InTerra_Data.TerrainLayerFromGUID(mat, "TerrainLayerGUID_1"), 0, mat);
							}
							if (mat.IsKeywordEnabled("_LAYERS_TWO"))
							{
								InTerra_Data.TerrainLaeyrDataToMaterial(InTerra_Data.TerrainLayerFromGUID(mat, "TerrainLayerGUID_1"), 0, mat);
								InTerra_Data.TerrainLaeyrDataToMaterial(InTerra_Data.TerrainLayerFromGUID(mat, "TerrainLayerGUID_2"), 1, mat);
							}
						}
					}
				}
			}
			EditorGUILayout.Space();


			//========================= GLOBAL SHADER RESTRICTIONS ===========================
			GUI.backgroundColor = GlobalSettingColor();
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{		
				EditorGUI.indentLevel = 1;
				EditorStyles.label.fontSize = 10;
				shaderSetting = EditorGUILayout.Foldout(shaderSetting, LabelAndTooltip("Global Shader Restrictions", "Option for globaly disabling features that does not \"shader_feature\" Keywords defined (to avoid too many shader variant) and only rely on shader properties, so if you are not using the feature at all disabling the feature will prevent the branching inside the shader.") , true); 
				EditorStyles.label.fontSize = 12;
				
				if (shaderSetting)
				{
					EditorGUILayout.Space();

					if(!restictiInit)
                    {
						normalmapsDisabled = globalData.disableNormalmap;
						heightBlendingDisabled = globalData.disableHeightmapBlending;
						terrainParallaxDisabled = globalData.disableTerrainParallax;
						tracksDisabled = globalData.disableTracks;
						objectParallaxDisabled = globalData.disableObjectParallax;
						puddlessDisabled = globalData.disablePuddles;
						restictiInit = true;						 
					}

					EditorGUI.BeginChangeCheck();

					using (new GUILayout.VerticalScope())
					{
						GUI.backgroundColor = Color.white;
						normalmapsDisabled = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Normal maps", "Disable Normals Maps use"), normalmapsDisabled);
						heightBlendingDisabled = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Heightmap Blending", "Disable Heightmap Blending"), heightBlendingDisabled);
						terrainParallaxDisabled = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Parallax for Terrain", "Disable Parallax for Terrain Only"), terrainParallaxDisabled);
						objectParallaxDisabled = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Parallax for Objects", "Disable Parallax for Objects Only"), objectParallaxDisabled);
						tracksDisabled = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Tracks", "Disable Tracks"), tracksDisabled);
						puddlessDisabled = EditorGUILayout.ToggleLeft(LabelAndTooltip("Disable Puddles", "Disable Puddles"), puddlessDisabled);
					}

					GUI.backgroundColor = GlobalSettingColor();

					if (EditorGUI.EndChangeCheck())
					{
						applyRestictionsButton = true;
					}

					GUI.enabled = applyRestictionsButton;
					if (GUILayout.Button(LabelAndTooltip("Apply Restrictions", "Restrictions will be writed to shaders and the shaders will be reimported."), styleButtonBold))
					{
						#if !(USING_URP || USING_HDRP)
						if (EditorUtility.DisplayDialog("Restrictions", "Note: Applying restrictions can take a few minutes.", "Continue", "Cancel"))
						#endif
						{
							Undo.RecordObject(globalData, "InTerra Shader Restrictions");
							globalData.disableNormalmap = normalmapsDisabled;
							globalData.disableHeightmapBlending = heightBlendingDisabled;
							globalData.disableTerrainParallax = terrainParallaxDisabled;
							globalData.disableObjectParallax = objectParallaxDisabled;
							globalData.disableTracks = tracksDisabled;
							globalData.disablePuddles = puddlessDisabled;
							EditorUtility.SetDirty(globalData);
							InTerra_Data.WriteDefinedKeywords();
							applyRestictionsButton = false;
						}
						#if !(USING_URP || USING_HDRP)
						else
                        {
							restictiInit = false;
						}
						#endif
					}
					GUI.enabled = true; 

					EditorGUILayout.Space();
				}
				EditorGUI.indentLevel = 0;				
			}
			GUI.backgroundColor = Color.white;
			EditorGUILayout.Space();


			if (targetMat.shader.name.Contains(("InTerra/HDRP")))			
			{								
				using (new GUILayout.VerticalScope())
				{
					targetMat.renderQueue = 2225;
					//========================= ENABLE DECALS ===========================
					bool decals = !targetMat.IsKeywordEnabled("_DISABLE_DECALS");
					EditorGUI.BeginChangeCheck();
					decals = EditorGUILayout.Toggle(LabelAndTooltip("Receive Decals", "Enable to allow Materials to receive decals."), decals);
					if (EditorGUI.EndChangeCheck())
					{
						materialEditor.RegisterPropertyChangeUndo("InTerra Enable Receive Decals");
						SetKeyword("_DISABLE_DECALS", !decals);
					}			
				}
				//========================= PER PIXEL NORMAL ===========================
				bool perPixelNormal = targetMat.IsKeywordEnabled("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
				EditorGUI.BeginChangeCheck();
				perPixelNormal = EditorGUILayout.Toggle(LabelAndTooltip("Enable Per-pixel Normal", "Enable per-pixel normal when the terrain uses instanced rendering."), perPixelNormal);
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Enable Per-pixel Normal");
					SetKeyword("_TERRAIN_INSTANCED_PERPIXEL_NORMAL", perPixelNormal);
				}
			}
			else
			{
				bool perPixelNormal = targetMat.IsKeywordEnabled("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
				EditorGUI.BeginChangeCheck();
				perPixelNormal = EditorGUILayout.Toggle(LabelAndTooltip("Enable Per-pixel Normal", "Enable per-pixel normal when the terrain uses instanced rendering."), perPixelNormal);
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("InTerra Enable Per-pixel Normal");
					SetKeyword("_TERRAIN_INSTANCED_PERPIXEL_NORMAL", perPixelNormal);
				}				
			}

			materialEditor.EnableInstancingField();

			void MaskMapMode()
            {
				int maskMapMode = globalData.maskMapMode;
				
				EditorGUI.BeginChangeCheck();
				EditorStyles.label.fontStyle = FontStyle.Bold;
				maskMapMode = EditorGUILayout.Popup(LabelAndTooltip("Mask Map Mode: ", "Global setting for Terrain Layer Mask Maps."), maskMapMode, maskMapLabels);
				EditorStyles.label.fontStyle = FontStyle.Normal;
				if (EditorGUI.EndChangeCheck())
				{
					#if !(USING_URP || USING_HDRP)
					if (EditorUtility.DisplayDialog("Mask Map Mode Change", "Note: Switching Mask Map Mode can take a few minutes.", "Continue", "Cancel"))
					#endif
					{
						Undo.RecordObject(globalData, "InTerra Mask Map Mode");
						globalData.maskMapMode = maskMapMode;
						InTerra_Data.WriteDefinedKeywords();
						EditorUtility.SetDirty(globalData);
						if (!disableUpdates) InTerra_Data.UpdateTerrainData(updateDict);
					}					
				}				
				GUI.backgroundColor = Color.white;
			}
					

			void MoveTerrainLayerToFIrst()
			{
				moveLayer = EditorGUILayout.Foldout(moveLayer, "Move Layer To First Position", true);
				EditorGUI.indentLevel = 0;
				if (moveLayer)
				{
					List<string> tl = new List<string>();
					for (int i = 1; i < terrain.terrainData.alphamapLayers; i++)
					{
						tl.Add((i + 1).ToString() + ". " + terrain.terrainData.terrainLayers[i].name.ToString());
					}
					if ((layerToFirst + 1) >= terrain.terrainData.alphamapLayers) layerToFirst = 0;
					TerrainLayer terainLayer = terrain.terrainData.terrainLayers[layerToFirst + 1];

					using (new GUILayout.HorizontalScope())
					{
						if (terainLayer && AssetPreview.GetAssetPreview(terainLayer.diffuseTexture))
						{
							GUI.Box(EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50)), AssetPreview.GetAssetPreview(terainLayer.diffuseTexture));
						}
						else
						{
							EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
						}
						using (new GUILayout.VerticalScope())
						{
							layerToFirst = EditorGUILayout.Popup("", layerToFirst, tl.ToArray(), GUILayout.MinWidth(170));
							if (GUILayout.Button("Move Layer to First Position", GUILayout.MinWidth(170), GUILayout.Height(27)))
							{
								MoveLayerToFirstPosition(terrain, layerToFirst + 1);
								if (!disableUpdates) InTerra_Data.UpdateTerrainData(updateDict);
							}
						}
						EditorGUILayout.GetControlRect(GUILayout.MinWidth(10));
					}
				}
			}

			void TextureSingleLine(string property1, string property2, string label, string tooltip = null)
			{
				materialEditor.TexturePropertySingleLine(new GUIContent() { text = label, tooltip = tooltip }, FindProperty(property1, properties), FindProperty(property2, properties));
			}			

			bool TerrainLayersMaskDisabled()
			{
				return globalData.maskMapMode == 0;
			}

			Color GlobalSettingColor()
			{
				return new Color(0.85f, 0.9f, 1.0f, 1.0f);
			}

			void GlobalSettitngLabel()
			{
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					GUILayout.Label("Global setting", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft });
				}
			}

			void CheckAndReplaceShader(string check, string replace)
			{
				if (targetMat.shader.name == check) targetMat.shader = Shader.Find(replace);
			}
		}
		GUIContent LabelAndTooltip(string label, string tooltip)
		{
			return new GUIContent() { text = label, tooltip = tooltip };
		}

		static public void PropertyLine(string property, string label, string tooltip = null)
		{
			terrainEditor.ShaderProperty(FindProperty(property, terrainProperties), new GUIContent() { text = label, tooltip = tooltip });
		}

		static public void SetKeyword(string name, bool set)
		{
			if (set) targetMat.EnableKeyword(name); else targetMat.DisableKeyword(name);
		}


		Vector2 UnpackValues(float value)
		{
			Vector2 color = new Vector4(0, 0, 0, 0);

			color.y = value % PRECISION;
			value = Mathf.Floor(value / PRECISION);

			color.x = value;

			color /= (PRECISION - 1);

			color.x = color.x > 0.995f ? 1.0f : color.x;
			color.x = color.x < 0.005f ? 0.0f : color.x;

			color.y = color.y > 0.995f ? 1.0f : color.y;
			color.y = color.y < 0.005f ? 0.0f : color.y;

			return color;
		}

		float PackValues(Vector2 color)
		{
			float output = 0;

			color.x = Mathf.Clamp(color.x, 0.001f, 0.999f);
			color.y = Mathf.Clamp(color.y, 0.001f, 0.999f);

			output += (Mathf.Floor(color.x * (PRECISION - 1))) * PRECISION;
			output += (Mathf.Floor(color.y * (PRECISION - 1)));

			return output;
		}


		private void CheckTextureClampWrapMode(Texture2D texture, string textureName)
		{
			if (texture && texture.wrapMode != TextureWrapMode.Clamp)
			{
				using (new GUILayout.VerticalScope(EditorStyles.helpBox))
				{
					EditorGUILayout.HelpBox(textureName + " texture Wrap mode should be set as Clamp!", MessageType.Warning);
					using (new GUILayout.VerticalScope())
					{
						if (GUILayout.Button("Set Wrap Mode As Clamp"))
						{
							TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
							importer.wrapMode = TextureWrapMode.Clamp;

							importer.SaveAndReimport();
							AssetDatabase.Refresh();
						}
					}
				}
			}
		}

		static void MoveLayerToFirstPosition(Terrain terrain, int indexToFirst)
		{
			float[,,] alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

			for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
			{
				for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
				{
					float a0 = alphaMaps[x, y, 0];
					float a1 = alphaMaps[x, y, indexToFirst];

					alphaMaps[x, y, 0] = a1;
					alphaMaps[x, y, indexToFirst] = a0;

				}
			}
			TerrainLayer[] origLayers = terrain.terrainData.terrainLayers;
			TerrainLayer[] movedLayers = terrain.terrainData.terrainLayers;

			TerrainLayer firstLayer = terrain.terrainData.terrainLayers[0];
			TerrainLayer movingLayer = terrain.terrainData.terrainLayers[indexToFirst];

			movedLayers[0] = movingLayer;
			movedLayers[indexToFirst] = firstLayer;

			terrain.terrainData.SetTerrainLayersRegisterUndo(origLayers, "InTerra Move Terrain Layer");			
			terrain.terrainData.terrainLayers = movedLayers;
			
			Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, "InTerra Move Terrain Layer");
			terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
		}
	}
}
