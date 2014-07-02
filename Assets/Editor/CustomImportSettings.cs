using UnityEngine;
using UnityEditor;
using System;

public class CustomImportSettings : AssetPostprocessor 
{
	//TEXTURES
	void OnPreprocessTexture()
	{
		TextureImporter textureImporter = assetImporter as TextureImporter;

		if(textureImporter.assetPath.Contains("Interface") || textureImporter.assetPath.Contains("Icons"))
		{
			textureImporter.textureType = TextureImporterType.GUI;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
			textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		}else{
			textureImporter.textureType = TextureImporterType.Image;
			textureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
			textureImporter.maxTextureSize = 512;
		}
	}
	/*
	//MODELS
	void OnPreprocessModel()
	{
		ModelImporter modelImporter = assetImporter as ModelImporter;
		modelImporter.generateSecondaryUV=true;
	}
	*/
}














