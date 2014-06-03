using UnityEngine;
using System.Collections.Generic;
using TVR.Helpers;

namespace TVR
{
	public static class ResourcesLibrary
	{
		public static Folder Backgrounds;
		public static Folder Characters;
		public static Folder Animations;
		public static Folder Expressions;
		public static Folder Cameras;
		public static Folder Music;

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static void Init()
		{
			initCameras();
			initBackgrounds();
			initCharacters();
			initMusic();
			initAnimations();
			initExpressions();
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static CameraParams getCamera(int ID) {
			return Cameras.getItem(ID) as CameraParams;
		}
		
		public static Resource getBackground(int ID) {
			return Backgrounds.getItem(ID);
		}

		public static Resource getCharacter(int ID) {
			return Characters.getItem(ID);
		}
		
		public static Resource getAnimation(int ID){
			return Animations.getItem(ID);
		}
		
		public static Resource getExpression(int ID){
			return Expressions.getItem(ID);
		}
		
		public static Resource getMusic(int ID) {
			return Music.getItem(ID) as Resource;
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//BACKGROUNDS
		private static void initBackgrounds()
		{
			Backgrounds = new Folder(null);
			
			Backgrounds.addResource(1, "Solar", "Backgrounds/Prefabs/Solar");
			Backgrounds.addResource(2, "Room", "Backgrounds/Prefabs/QRoom");
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//CAMERAS
		private static void initCameras()
		{
			Cameras = new Folder(null);

			Cameras.addCameraParams(1, "Front", new Vector3(0.0f,0.0f,0.0f), new Vector3(15,0,0), 50);
			Cameras.addCameraParams(2, "Side", new Vector3(-1.4f,-0.2f,0.9f), new Vector3(12,34,-2), 50);
			Cameras.addCameraParams(3, "Full", new Vector3(0.0f,1.0f,-0.8f), new Vector3(27,0,0), 50);
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//CHARACTERS
		private static void initCharacters()
		{
			Characters = new Folder(null);
			
			Characters.addResource(1, "q_main", "Characters/Prefabs/q_main");
			Characters.addResource(2, "liz_cory", "Characters/Prefabs/liz_cory");
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//ANIMATIONS
		private static void initAnimations()
		{
			Animations = new Folder(null);
			
			Animations.addResource(1, "idle", "idle");
			Animations.addResource(2, "wave", "wave");
			Animations.addResource(3, "talk", "talk");
			Animations.addResource(4, "walk", "walk");
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//EXPRESSIONS
		private static void initExpressions()
		{
			Expressions = new Folder(null);

			Expressions.addResource(1, "base", "base");
			Expressions.addResource(2, "bostezo", "bostezo");
			Expressions.addResource(3, "indignado_c", "indignado_c");
			Expressions.addResource(4, "ouch", "ouch");
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//MUSIC
		private static void initMusic()
		{
			Music = new Folder(null);
			
			Music.addResource(1, "dance", "Music/dance_loop");
			Music.addResource(2, "lisa", "Music/lisa_mirror_loop");
			Music.addResource(3, "love", "Music/love_song_loop");
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//RESOURCE
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public class Resource : System.IComparable<Resource>
		{
			public int ID;					//Identificador único
			public string Name;				//Nombre
			public string ResourceName;		//Modelo
			public int Number;				//Número para poder ordenar la lista
			
			////////////////////////////////////////////////////////////////////////////////////////
			
			public GameObject getInstance(string Scene)
			{
				return MonoBehaviour.Instantiate(ResourcesManager.LoadModel(ResourceName, Scene)) as GameObject;
			}

			////////////////////////////////////////////////////////////////////////////////////////

			public int CompareTo(Resource other) {		
				if ( this.Number < other.Number ) return -1;
				else if ( this.Number > other.Number ) return 1;
				else return 0;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public class CameraParams : Resource
		{
			public Vector3 Position;
			public Vector3 EulerAngles;
			public int DoF;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//FOLDER
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public class Folder : Resource
		{
			public List<Resource> Content;
			private Dictionary<int,Resource> dicContent;
			
			////////////////////////////////////////////////////////////////////////////////////////
			
			public Folder(Resource parent) : base()
			{
				Content = new List<Resource>();
				dicContent = new Dictionary<int, Resource>();
			}
			
			////////////////////////////////////////////////////////////////////////////////////////
			
			public Resource getItem(int ID)
			{
				return dicContent[ID];
			}
			
			////////////////////////////////////////////////////////////////////////////////////////
			
			public void addResource(int ID, string sName, string sResourceName, int number=0)
			{
				Resource r = new Resource();
				r.ID = ID;
				r.Name = sName;
				r.ResourceName = sResourceName;
				r.Number = number;
				Content.Add(r);
				dicContent.Add(r.ID, r);
			}
			
			
			////////////////////////////////////////////////////////////////////////////////////////
			
			public void addCameraParams(int ID, string sName, Vector3 vPos, Vector3 vRot, int iDof, int number=0)
			{
				CameraParams c = new CameraParams();
				c.ID = ID;
				c.Name = sName;
				c.Position = vPos;
				c.EulerAngles = vRot;
				c.DoF = iDof;
				c.Number = number;
				Content.Add(c);
				dicContent.Add(c.ID, c);
			}
		}
	}
}



