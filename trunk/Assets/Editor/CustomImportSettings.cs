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
		}
	}
}














