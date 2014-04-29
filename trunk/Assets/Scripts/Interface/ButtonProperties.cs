using UnityEngine;
using System.Collections;

public class ButtonProperties
{
	//Button
	public const float buttonZDepth = 10;
	const float buttonMarginRatio = 0.0125f;
	const float buttonRatio = 0.088f;
	public static float buttonSize;
	public static float buttonMargin;

	//ButtonBar
	public const float buttonBarZDepth = 20;
	const float buttonBarRatio = 0.1f;
	public static float buttonBarScaleX;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void Init()
	{
		float totalHeight = 5*buttonRatio*Screen.width+6*buttonMarginRatio*Screen.width;
		
		if(totalHeight > Screen.height){
			//The relation between buttons and margins is 7-1
			buttonMargin = Screen.height/41;
			buttonSize = 7*buttonMargin;
			buttonBarScaleX = 1.13f*buttonSize;
		}
		else{
			buttonMargin = Screen.width*buttonMarginRatio;
			buttonSize = Screen.width*buttonRatio;
			buttonBarScaleX = Screen.width*buttonBarRatio;
		}
	}
}
