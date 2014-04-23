using UnityEngine;

namespace TVR {
	public static class Globals {
		public const int FRAMESPERSECOND = 25;
		public const float MILISPERFRAME = 1f / (float)FRAMESPERSECOND;
		public const float TIMEPRESSED = 0.5f; //Tiempo que esperamos antes de cambiar el botón de estado (por ejemplo: botón de movimiento de cámara)
		public static float RATIOX = Screen.width / 1024f;
		public static float RATIOY = Screen.height / 768f;
		public const int MINFRAMES = 25;
		public const int MAXFRAMES = 5000;

		public const int OUTPUTRATEPERSECOND = 44100;
		public const int OUTPUTRATEPERFRAME = OUTPUTRATEPERSECOND / FRAMESPERSECOND;
		public const short NUMCHANNELS = 2;
		public const int WAVHEADERSIZE = 44; //default for uncompressed wav

		//Menus
		public const float GREY = 0.15f;
		public const float ANIMATIONDURATION=0.4f;
		public const int MESSAGEPOSY = 190;
		public const int MESSAGEBUTONSPOSY = MESSAGEPOSY + 90;
		public const int MESSAGEBUTONSSEPARATION = 75;
		public const int MESSAGEBUTONSWIDTH = 110;
		public const int MESSAGEBUTONSHEIGHT = 51;
		public const int MESSAGETITLEFONTSIZE = 35;
		public const int MESSAGEBUTTONFONTSIZE = 25;
		public const float BRAKEDURATION = 1.5f;
		public const float MINIMUMSPEED = 1.5f;
		public const float MAXIMUMSPEED = 150;
		public const int TITLEFONTSIZE = 35;
		public const int SPEEDS = 5; //Número de velocidades que guardamos para hacer la media de velocidad en frenada.
		
		//NewScene
		
		//Scene
		public static Color colorInv = new Color(1,0.5f,0.5f);
		public static Color colorOut = Color.gray;
		
		//Data
		public const int SCREENSHOT_WIDTH = 81;
		public const int SCREENSHOT_HEIGHT = 81;
		public static string BasePath = UnityEngine.Application.persistentDataPath;
#if UNITY_ANDROID
		public static string DataBase = System.IO.Path.Combine(BasePath, "TVR.db").Replace("files","databases");
#else
		public static string DataBase = System.IO.Path.Combine(BasePath, "TVR.db");
#endif
		public static string ScreenshotsPath = System.IO.Path.Combine(BasePath, "Screenshots");
		public static string RecordedSoundsPath = System.IO.Path.Combine(BasePath, "RecordedSounds");
		public static string RendersPath = System.IO.Path.Combine(BasePath, "Renders");
		
		//Debug
		public static bool CLEAR_DATA = false;
		public static bool INIT_IN_EDITOR = false;
		public static int DEFAULT_BACKGROUND = -1; //-1 - Cajita / 1 - Solar / 2 - Clase / 3 - Habitacion.
	}
}