using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GBXT
{
	public class Greybox : MonoBehaviour {
		[Tooltip("Camera to be used in 360 capture. If empty, will assign the one from the component.")]
		public Camera camera;
		
		[Space(10)]
		
		[Tooltip("Your unique Greybox token. Get one at http://greybox.it")]
		public string token;
		public string localImageFolder = "Assets/Plugins/Greybox/Greybox Gallery";

		[Tooltip("The hotkey alternative to the capture button below")]
		public KeyCode screenshotHotkey = KeyCode.F1;

		[HideInInspector]
		public string state = "Ready";
	
		[HideInInspector]
		public List<string> urls = new List<string>();
	
		[HideInInspector]
		public bool canTakeScreenshot = true;
	   
		[HideInInspector]
		public string FilePath;
		
		[HideInInspector]
		public string galleryFolder;
	
		private string POSTurl = "https://us-central1-graybox-219f6.cloudfunctions.net/upload";
	
		private int lastWidth = 0, lastHeight = 0;
				
		private RenderTexture cubemapTex;
		private RenderTexture equirect;	
		private string screenshotFullPath;	
		
		public struct SimpleJsonData
		{
			public string url;
		}

		// Use this for initialization
		void Start () {
			if (camera == null && GetComponent<Camera>() != null)
			{
				camera = GetComponent<Camera>();				
			}
			else
			{
				if(camera == null) Debug.LogError("Greybox needs a camera to be set.");
			}
			
			cubemapTex = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGB32);
			cubemapTex.dimension = UnityEngine.Rendering.TextureDimension.Cube;
			cubemapTex.hideFlags = HideFlags.HideAndDontSave;
	
			equirect = new RenderTexture(4096, 2048, 0,  RenderTextureFormat.ARGB32);
			
			if (string.IsNullOrEmpty(galleryFolder)) {
				galleryFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), localImageFolder);
			
				// create the directory
				if (!Directory.Exists(galleryFolder)) {
					Directory.CreateDirectory(galleryFolder);
				}
			}
	
		}
		
		void Update () {
			if(Input.GetKeyDown(screenshotHotkey)){
				TakeScreenShotNative();
			}
		}
	
	
		public void TakeScreenShotNative(){
			Debug.Log("Starting native screenshot capture...");
			
			camera.stereoSeparation = 0.064f; // Eye separation (IPD) of 64mm.
			camera.RenderToCubemap(cubemapTex, 63, Camera.MonoOrStereoscopicEye.Right);
			cubemapTex.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
			
			FilePath = ScreenShotName(4096, 2048);
			StartCoroutine(SaveRenderTexture(equirect, FilePath));
			
		}
	
		IEnumerator SaveRenderTexture(RenderTexture rt, string jpgOutPath)
		{
			yield return new WaitForEndOfFrame();
			
			var oldRT = RenderTexture.active;
	
			var tex = new Texture2D(rt.width, rt.height);
			RenderTexture.active = rt;
			tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			tex.Apply();
				
			File.WriteAllBytes(jpgOutPath, tex.EncodeToJPG());
			RenderTexture.active = oldRT;

			Debug.Log("Saved JPG to a file");
			
			FilePath = jpgOutPath;
			StartCoroutine(UploadPost(tex));
			
		}
	
		public void ClearURLs(){
			urls.Clear();
		}
				
		IEnumerator UploadPost(Texture2D tex){

			WWWForm form = new WWWForm();

			byte[] bytes = tex.EncodeToJPG();
			
			form.AddBinaryData("file", bytes, "screenShot.jpg", "image/jpeg");
						
			using (var w = UnityWebRequest.Post(POSTurl, form))
			{
				
				w.SetRequestHeader("x-token", token);
				
				yield return w.SendWebRequest();

					SimpleJsonData response = JsonUtility.FromJson<SimpleJsonData>(w.downloadHandler.text);
					print(w.downloadHandler.text);
					print(response.url);
					urls.Add(response.url);

					//					print(w.downloadHandler.data);

			}
		}
	
		
		
		string ScreenShotName(int width, int height){            
	
			return string.Format("{0}/{1}.jpg",
					galleryFolder,
					DateTime.Now.ToString("ddMMM-hh-mm-ss"));
	
		}
	}
}
