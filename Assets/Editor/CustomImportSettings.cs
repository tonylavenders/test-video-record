using UnityEngine;
using UnityEditor;
using System;

public class CustomImportSettings : AssetPostprocessor 
{
	//TEXTURES
	void OnPreprocessTexture()
	{
		TextureImporter textureImporter = assetImporter as TextureImporter;

		if(textureImporter.assetPath.Contains("Docs") || textureImporter.assetPath.Contains("InterfaceTextures"))
		{
			textureImporter.textureType = TextureImporterType.GUI;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
			textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		}
	}
}














