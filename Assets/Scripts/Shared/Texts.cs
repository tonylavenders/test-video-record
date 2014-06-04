namespace TVR
{	
	public static class Texts {
		//Tabla Unicode http://www.utf8-chartable.de/
		//Menus
		public const string WARNING = "AVISO";
		
		public const string EPISODES_NEW_EPISODE 			= "NUEVO EPISODIO";
		public const string EPISODES_UNDO_CREATE 			= "Eliminar\u00E1 el \u00FAltimo episodio creado.\n\u00BFEst\u00E1 seguro?";
		public const string EPISODE_INFORMATION_LINE1 		= "Duraci\u00F3n: {0}m {1}s";
		public const string EPISODE_INFORMATION_LINE2 		= "N\u00FAmero de secuencia: {0}";
		public const string EPISODE_BUTTON_PLAY 			= "REPRODUCIR EPISODIO";
		public const string EPISODE_BUTTON_EDIT 			= "EDITAR EPISODIO";
		public const string EPISODE_BUTTON_DELETE 			= "ELIMINAR EPISODIO";
		public const string EPISODE_BUTTON_PROPERTIES		= "PROPIEDADES EPISODIO";
		public const string ACCEPT 							= "Aceptar";
		public const string YES 							= "S\u00ED";
		public const string NO 								= "No";
		public const string EPISODE_MESSAGE_DELETE 			= "\u00BFDesea eliminar este episodio?";
		public const string SCENES_NEW_SCENE 				= "Nueva secuencia";
		public const string SCENES_UNDO_CREATE 				= "Eliminar\u00E1 la \u00FAltima secuencia creada.\n\u00BFEst\u00E1 seguro?";
		public const string SCENES_SCENE 					= "SECUENCIA";
		public const string SCENES_DELETE 					= "\u00BFDesea eliminar la secuencia seleccionada?";
		public const string SCENES_DELETE_LAST 				= "No puede eliminar la \u00FAltima secuencia.";
		public const string SCENE_NEW_STAGE 				= "Nueva escena";
		//public const string SCENE_NEW_STAGE2				= "Nueva";
		public const string SCENE_SCENE 					= "//SECUENCIA ";
		public const string SCENE_UNDO_CREATE 				= "Eliminar\u00E1 la \u00FAltima escena creada.\n\u00BFEst\u00E1 seguro?";
		//public const string SCENE_BACK 						= "Fondo: ";
		public const string SCENE_DELETE 					= "\u00BFDesea eliminar la escena seleccionada?";
		public const string SCENE_DELETE_LAST 				= "No puede eliminar la \u00FAltima escena.";

		public const string SAVE 							= "Guardar";
		public const string CLOSE							= "Cerrar";
		public const string PROPERTIES_TITLE				= "T\u00CDTULO:";
		public const string PROPERTIES_TITLE_2				= "Pulse aqu\u00ED para introducir el t\u00EDtulo";
		public const string PROPERTIES_INFO					= "INFO:";
		public const string PROPERTIES_INFO_2				= "Pulse aqu\u00ED para introducir la informaci\u00F3n";
		public const string PROPERTIES_DURATION				= "DURACI\u00D3N: {0} Frames // {1}m {2}s";

		public const string EPISODE_PROPERTIES	 			= "PROPIEDADES DEL EPISODIO";
		public const string SCENE_PROPERTIES	 			= "PROPIEDADES DE LA SECUENCIA";
		public const string STAGE_PROPERTIES	 			= "PROPIEDADES DE LA ESCENA";

		public const string UNDO_MENU						= "Se ha deshecho #ACTION# de#OBJECT# #NUMBER#";
		public const string UNDO_ACTION_DELETE				= "la acci\u00F3n de eliminar";
		public const string UNDO_ACTION_CREATE				= "la acci\u00F3n de crear";
		public const string UNDO_ACTION_INFO				= "un cambio en las propiedades";
		public const string UNDO_ACTION_RENUM				= "la acci\u00F3n de reordenar (de posici\u00F3nn #NUMBER# a #OLDNUMBER#)";
		public const string UNDO_EPISODE		 			= "l episodio";
		public const string UNDO_SCENE			 			= " la secuencia";
		public const string UNDO_STAGE			 			= " la escena";

		//NewScene
		public const string STAGE_DURATION		 			= "DURACI\u00D3N DE LA ESCENA";
		public const string NEWSTAGE_FRAMES		 			= "(Frames)";
		public static string MAX_DURATION		 			= "DURACI\u00D3N M\u00C1XIMA " + Globals.MAXFRAMES + " FRAMES";

		//Scene
		public const string SCENE_BIG_BLOCK 				= "Bloque demasiado grande.\n\u00BFDesea que se reescale?";
		
		public const string STAGE_BLOCK_VISIBILITY 			= "Visible";
		
		public const string STAGE_UNDO_CREATE_SCREENSHOT 	= "Eliminar\u00E1 la \u00FAltima captura de pantalla.\n\u00BFEst\u00E1 seguro?";
		public const string STAGE_UNDO_CREATE_OBJECT 		= "Eliminar\u00E1 el \u00FAltimo objeto creado.\n\u00BFEst\u00E1 seguro?";
		
		public const string STAGE_NO_BLOCKS_DUPLICATE		= "No se ha podido duplicar, no hay espacios libres.";
		public const string INVALID_ZONE 					= "AVISO: La posici\u00F3n del elemento est\u00E1 fuera del escenario.";
		public const string NO_BLOCK_OVER_BLOCK 			= "No puede colocar un bloque encima de otro.";

		public const string UNDO_CREATE_SOUND 				= "Eliminar\u00E1 LA \u00FAltima voz grabada.\n\u00BFEst\u00E1 seguro?";
		public const string UNDO_CREATE_SOUND_ASSOCIATED 	= "No se puede eliminar la \u00FAltima voz grabada.\nEst\u00E1 asociada a un bloque.";
		public const string ERROR_DELETE_SOUND			 	= "No se puede eliminar esta voz. Est\u00E1 asociada a un bloque.";
		public const string VOLUME_1					 	= "VOLUMEN DEL BLOQUE";
		public const string VOLUME_2					 	= "Seleccione el nivel de volumen para el clip de audio.";
		public const string FXGUI_1					 		= "EFECTOS DE LA IMAGEN";
		public const string FXGUI_2					 		= "Seleccione el efecto para el bloque de la imagen.";

		public const string DUPLICATE_1					 	= "Duplicando";
		public const string DUPLICATE_2					 	= "Espere unos segundos mientras se realiza la copia.";

		public const string PLANE_INIT_SCENE	 			= "APLICAR AL INICIO DE LA ESCENA";
		public const string PLANE_END_SCENE		 			= "APLICAR AL FINAL DE LA ESCENA";
		public const string NO_TIMELINE_CHANGE	 			= "En estos momentos no se puede cambiar de l\u00EDnea de tiempo.";

		//SoundRecorder
		public const string NEW_AUDIO_TEXT					= "Escriba el nombre";
		public const string NEW_AUDIO_DATA 					= "NUEVO AUDIO";
		public const string NO_MICROPHONE 					= "El dispositivo que est\u00E1 usando no tiene micr\u00F3fono.";
		public const string MAX_AUDIO_RECORD_TIME 			= "El tiempo m\u00E1ximo de grabaci\u00F3n por cada clip es de 30 segundos.";
		public const string SAVING_AUDIO_CLIP 				= "Guardando clip de audio.\nEste proceso puede durar un rato.";
		public const string EMPTY_AUDIO_CLIP_NAME 			= "El nombre del clip de audio no puede estar vac\u00EDo.";
		public const string AUDIO_NOT_SAVED 				= "No se ha guardado la \u00FAltima grabaci\u00F3n.\n\u00BFEst\u00E1 seguro que desea continuar?";
		public const string APPLYING_EFFECTS 				= "Aplicando efectos de audio. \nEspere por favor.";
		public const string DELETE_AUDIO_FILE 				= "Se va a borrar el clip de audio.\n\u00BFEst\u00E1 seguro?";
		
		public const string EMPTY_FOLDER 					= "La carpeta est\u00E1 vac\u00EDa.";
		public const string DELETE_ROT_FRAMES 				= "Se borrar\u00E1n todos los frames de rotaci\u00F3n. \u00BFEst\u00E1 seguro que desea continuar?";

		//Player
		public const string NOTHING_TO_REPRODUCE 			= "No hay nada para reproducir.";

		//Export
		public const string CANCEL_ENCODING					= "Se va a cancelar la creaci\u00F3n del video. \u00BFEst\u00E1 seguro que desea continuar?";
		public const string CANCEL_ENCODING1				= "Espere mientras eliminamos los archivos temporales generados.";
		public const string ERROR_EXPORT					= "Se ha producido un error mientras se exportaba.";
		public const string RESIZE_TIMELINE 				= "Operaci\u00F3n no permitida.\nVac\u00EDe el \u00E1rea del timeline que desea recortar.";
		
		//Tabs
		public static string[] TAB_NAMES = new string[]{"VISIBILIDAD","POSICION","ROTACION","ESCALA","SONIDOS","MUSICA","VOCES","EXPRESIONES","ANIMACIONES","PROP1","PROP2","PROP3","GUI1","ZOOM","GUI2"};
		
		//Libs
		public static string[] LIB_NAMES = new string[]{"Sonidos","M\u00FAsica","Voces","Personajes","Props","Fx","GUI","Expresiones","Animaciones","Vacio","Props"};

		//Se ha deshecho un cambio en las propiedeades de la secuencia XX
		//Se ha deshecho la acción de eliminar de la secuencia XX
		//Se ha deshecho la acción de crear de la secuencia XX
		//Se ha deshecho la acción de reordenar (de 4 a 5) de la secuencia XX
	}
}