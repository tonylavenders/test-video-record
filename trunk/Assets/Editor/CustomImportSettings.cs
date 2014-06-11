using UnityEngine;
using UnityEditor;
using System;

public class CustomImportSettings : AssetPostprocessor 
{
	//TEXTURES
	void OnPreprocessTexture()
	{
		TextureImporter textureImporter = assetImporter as TextureImporter;

		if(textureImporter.assetPath.Contains("Interface"))
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
}














