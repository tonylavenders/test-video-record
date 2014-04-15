using UnityEngine;

namespace Helpers
{	
	public static class InputHelp
	{
		private static Vector2 mMousePositionYDown = Vector2.zero;
		private static Vector2 mMouseInitPosYDown = Vector2.zero;
		private static Vector2 mMouseOffsetPosYDown = Vector2.zero;
		private static Vector2 mDeltaMousePositionYDown = Vector2.zero;
		private static Vector2 mMousePositionYUp = Vector2.zero;
		private static bool mMultiTouch;
		private static int mFingerId0 = -1;
		private static int mFingerId1 = -1;
		
		/// <summary>
		/// Posición real del mouse para entorno 3D, sin filtros.
		/// (0, 0) (Izquierda, Abajo).
		/// </summary>
		public static Vector2 mousePositionYDown {
			get { return mMousePositionYDown; }
		}
		
		/// <summary>
		/// Posición del mouse con los primeros píxeles de movimiento filtrados en entorno iOS.
		/// (0, 0) (Izquierda, Arriba).
		/// </summary>
		public static Vector2 mousePosition {
			get { return mMousePositionYUp; }
		}
		
		/// <summary>
		/// Posición real del mouse, sin filtros.
		/// (0, 0) (Izquierda, Arriba).
		/// </summary>
		public static Vector2 realMousePosition {
			get {
				Vector2 res = mMousePositionYDown;
				res.y = Screen.height - res.y;
				return res;
			}
		}
		
		/// <summary>
		/// Delta del mouse para entorno 3D, sin filtros.
		/// (0, 0) (Izquierda, Abajo).
		/// </summary>
		public static Vector2 mouseDeltaPositionYDown {
			get { return mDeltaMousePositionYDown; }
		}

		/// <summary>
		/// Delta del mouse, sin filtros.
		/// (0, 0) (Izquierda, Abajo).
		/// </summary>
		public static Vector2 mouseDeltaPosition {
			get { return new Vector2(mDeltaMousePositionYDown.x, mDeltaMousePositionYDown.y * -1); }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool fingerChange {
			get { return mFingerId0 != mFingerId1; }
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool GetMouseButtonDown(int button) {
			return Input.GetMouseButtonDown(button);
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool GetMouseButton(int button) {
			return Input.GetMouseButton(button);
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool GetMouseButtonUp(int button) {
			return Input.GetMouseButtonUp(button);
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool GetAnyMouseButton {
			get { return Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2); }
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool GetAnyMouseButtonDown {
			get { return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2); }
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool GetKey(KeyCode k) {
			return Input.GetKey(k);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool GetKeyDown(KeyCode k) {
			return Input.GetKeyDown(k);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Update() {
			Vector2 pos = GetMousePos();
			mDeltaMousePositionYDown = pos - mMousePositionYDown;
			mMousePositionYDown = pos;

			if(Application.platform == RuntimePlatform.IPhonePlayer) {
				if(Mathf.Abs(mMouseOffsetPosYDown.x) < 8) {
					mMousePositionYUp.x = mMouseInitPosYDown.x;
					mMouseOffsetPosYDown.x = mMouseInitPosYDown.x - mMousePositionYDown.x;
				} else {
					mMousePositionYUp.x = mMousePositionYDown.x + mMouseOffsetPosYDown.x;
				}
				if(Mathf.Abs(mMouseOffsetPosYDown.y) < 8) {
					mMousePositionYUp.y = mMouseInitPosYDown.y;
					mMouseOffsetPosYDown.y = mMouseInitPosYDown.y - mMousePositionYDown.y;
				} else {
					mMousePositionYUp.y = mMousePositionYDown.y + mMouseOffsetPosYDown.y;
				}
			} else
				mMousePositionYUp = pos;

			mMousePositionYUp.y = Screen.height - mMousePositionYUp.y;
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		private static Vector2 GetMousePos() {
			Vector2 mousePos = Vector2.zero;
			
			if(Application.platform == RuntimePlatform.IPhonePlayer) {
				if(Input.touchCount == 1 || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)) {
					if(!mMultiTouch) {
						if(Input.GetMouseButtonDown(0)) {
							mousePos = Input.touches[0].position;
							mMouseInitPosYDown = Input.touches[0].position;
							mMousePositionYDown = mousePos;
							mMouseOffsetPosYDown = Vector2.zero;
						} else {
							mousePos = mMousePositionYDown + Input.touches[0].deltaPosition;
						}
						
						mFingerId0 = Input.touches[0].fingerId;
						mFingerId1 = Input.touches[0].fingerId;
					} else {
						mousePos = mMousePositionYDown + Input.touches[0].deltaPosition;
						mFingerId1 = Input.touches[0].fingerId;
					}
				} else if(Input.touchCount > 1) {
					mousePos = mMousePositionYDown + Input.touches[0].deltaPosition;
					mMultiTouch = true;
				} else {
					mMultiTouch = false;
					mousePos = mMousePositionYDown;
					/*mMousePosition = Vector2.zero;
					mMouseInitPos = Vector2.zero;
					mMouseOffsetPos = Vector2.zero;
					mDeltaMousePosition = Vector2.zero;
					mMousePositionInvertedY = Vector2.zero;*/
				}
			} else {
				mousePos = Input.mousePosition;
			}
			
			return mousePos;
		}
	}
}