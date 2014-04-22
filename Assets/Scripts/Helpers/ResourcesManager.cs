using UnityEngine;
using System.Collections.Generic;

namespace TVR.Helpers
{	
	public static class ResourcesManager {
		private static Dictionary<string, AdvResource> mResources;
		private static Dictionary<string, List<AdvResource>> mScenesResources;
		
		private static void Instanced() {
			if(mResources==null) {
				mResources=new Dictionary<string, AdvResource>();
				mScenesResources=new Dictionary<string, List<AdvResource>>();
			}
		}
		
		public static Object LoadResource(string Name, string Scene) {
			Instanced();
			if (Name=="") {
				throw new System.ArgumentException("Parameter cannot be null", "Name");
			}
			if (Scene=="") {
				throw new System.ArgumentException("Parameter cannot be null", "Scene");
			}
			AdvResource res;
			if(mResources.ContainsKey (Name)) {
				res = mResources[Name];
				res.addScene (Scene);
			} else {
				res = new AdvResource(Name,Scene);
				mResources.Add(Name,res);
			}
			if(mScenesResources.ContainsKey (Scene)) {
				if(!mScenesResources[Scene].Contains(res))
					mScenesResources[Scene].Add (res);
			} else {
				List<AdvResource> list = new List<AdvResource>();
				mScenesResources.Add(Scene,list);
				list.Add (res);
			}
			
			return res.Resource;
		}

		public static Object LoadModel(string Name, string Scene) {
			Instanced();
			if (Name=="") {
				throw new System.ArgumentException("Parameter cannot be null", "Name");
			}
			if (Scene=="") {
				throw new System.ArgumentException("Parameter cannot be null", "Scene");
			}
			AdvResource res;
			if(mResources.ContainsKey (Name)) {
				res = mResources[Name];
				res.addScene (Scene);
			} else {
				res = new AdvResource(Name,Scene,typeof(GameObject));
				mResources.Add(Name,res);
			}
			if(mScenesResources.ContainsKey (Scene)) {
				if(!mScenesResources[Scene].Contains(res))
					mScenesResources[Scene].Add (res);
			} else {
				List<AdvResource> list = new List<AdvResource>();
				mScenesResources.Add(Scene,list);
				list.Add (res);
			}
			
			return res.Resource;
		}

		public static bool UnloadResource(string Name, string Scene, bool Force = false) {
			Instanced();
			if(mResources.ContainsKey (Name)) {
				AdvResource res = mResources[Name];
				if(!Force) {
					if(res.removeScene (Scene)==0) {
						List<AdvResource> list = mScenesResources[Scene];
						list.Remove (res);
						mResources.Remove(Name);
						if(list.Count==0)
							mScenesResources.Remove (Scene);
					} else
						return false;
				} else {
					List<string> scenes = res.Scenes;
					foreach(string scene in scenes) {
						List<AdvResource> list = mScenesResources[scene];
						list.Remove (res);
						if(list.Count==0)
							mScenesResources.Remove (Scene);
					}
				}
				res.Unload();
				res=null;
				return true;
			} else
				return false;
		}
	
		public static bool UnloadScene(string Scene, bool Force = false) {
			Instanced();
			if(mScenesResources.ContainsKey (Scene)) {
				List<AdvResource> ress = mScenesResources[Scene];
				List<AdvResource> ress2 = new List<AdvResource>();
				foreach(AdvResource res in ress) {
					ress2.Add (res);
				}
				foreach(AdvResource res in ress2) {
					UnloadResource (res.Name,Scene,Force);
				}
				return true;
			}
			return false;
		}
	
		private class AdvResource {
			private List<string> mScenes;
			private string mName;
			private Object mResource;
			
			public Object Resource {
				get {
					return mResource;
				}
			}
			public List<string> Scenes {
				get {
					return mScenes;
				}
			}
			public string Name {
				get {
					return mName;
				}
			}
			
			public AdvResource(string Name, string Scene) {
				mScenes=new List<string>();
				mName=Name;
				mScenes.Add (Scene);
				mResource = Resources.Load(Name);
				
				if(mResource==null){
					Debug.Log ("El recurso:" + Name + " no se ha cargado correctamente.");
				}
			}
	
			public AdvResource(string Name, string Scene, System.Type type) {
				mScenes=new List<string>();
				mName=Name;
				mScenes.Add (Scene);
				mResource = Resources.Load(Name, type);
				
				if(mResource==null){
					Debug.Log ("El recurso:" + Name + " no se ha cargado correctamente.");
				}
			}
	
			public bool addScene(string Scene) {
				if(mScenes==null)
					return false;
				if (mScenes.Contains (Scene)) 
					return false;
				else
					mScenes.Add (Scene);
				return true;
			}
			
			public int countScenes() {
				return mScenes.Count;
			}
			
			public int removeScene(string Scene) {
			if (!mScenes.Contains (Scene)) 
					return -1;
				else
					mScenes.Remove(Scene);
				return countScenes();
			}
			
			public bool containsScene(string Scene) {
				return mScenes.Contains (Scene);
			}
			
			public void Unload() {
				//try {
				mScenes.Clear ();
				mScenes = null;
				mName = null;
				if(!(mResource is GameObject))
					Resources.UnloadAsset(mResource);
				mResource=null;
				//} catch {
				//	int a = 0;
				//	a+=1;
				//}
			}
		}
	}
}