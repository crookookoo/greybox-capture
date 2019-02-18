using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Runtime.InteropServices;
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
		private RenderTexture outputTex;  // equirect or cubemap ends up here
		private RenderTexture externalTex;
		private RenderTexture equirect;	
		private string screenshotFullPath;
	
		[DllImport("FBCapture", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
		private static extern bool saveScreenShot(IntPtr texture, string path, bool needFlipping);                
	
	
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
	
//		public void TakeScreenShot(){
//			Debug.Log("Starting screenshot capture...");
//			TakeScreenshot(4096, 2048, ScreenShotName(4096, 2048));
//			// StartCoroutine(CaptureScreenshot(4096,2048));
//			// 	urls.Add("http://greybox.it/lsdfe4");
//			// 	canTakeScreenshot = false;
//		}
	
		public void TakeScreenShotNative(){
			Debug.Log("Starting native screenshot capture...");
			
			camera.stereoSeparation = 0.064f; // Eye separation (IPD) of 64mm.
			camera.RenderToCubemap(cubemapTex, 63, Camera.MonoOrStereoscopicEye.Right);
			cubemapTex.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
			
			FilePath = ScreenShotName(4096, 2048);
			StartCoroutine(SaveRenderTexture(equirect, FilePath));
//			yield return null;
			
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
			StartCoroutine(UploadPost());
			
		}
	
//		public void TakeScreenshot(int width, int height, string screenshotPathName = "")
//		{
//			if (!string.IsNullOrEmpty(screenshotPathName)) {
//				screenshotFullPath = screenshotPathName;
//			}
//	
//			camera.RenderToCubemap(cubemapTex);  // render cubemap
//	
//			StartCoroutine(CaptureScreenshot(width, height));            
//		}
	
	
		public void ClearURLs(){
			urls.Clear();
		}
			
//		IEnumerator CaptureScreenshot(int width, int height) {
//			SetOutputSize(width, height);
//			// yield a frame to re-render into the rendertexture
//			yield return new WaitForEndOfFrame();
//	
//			Debug.LogFormat("[SurroundCapture] Saved {0} x {1} screenshot: {2}", width, height, screenshotFullPath);
//	
//			if(!saveScreenShot(externalTex.GetNativeTexturePtr(), screenshotFullPath, false)) {
//				Debug.Log("Failed on taking screenshot. Please check FBCaptureSDK.log file");
//			} else {
//				Debug.Log("Screenshot captured, starting upload...");
//				FilePath = screenshotFullPath;
//				StartCoroutine(UploadPost());
//	
//				// uploadScreenShot(screenshotFullPath);
//			}
//		}
	
		IEnumerator UploadPost(){

			Debug.Log("Starting upload...");
			
			WWWForm form = new WWWForm();
			Debug.Log("Reading from: " + FilePath);
			form.AddBinaryData("file", File.ReadAllBytes(@FilePath), "screenShot.jpg", "image/jpeg");
			
			Dictionary<string, string> headers = form.headers;
			
			headers["x-token"] = token;
			//headers["debug"] = "lol";
			headers["content-type"] = "application/json; charset=utf-8";
			
//			UnityWebRequest www = UnityWebRequest.Post(POSTurl, form);
//			www.SetRequestHeader("x-token", token);
//			www.SetRequestHeader("content-type", "application/json; charset=utf-8");
//
//			yield return www.SendWebRequest();
//			
//			if(www.isNetworkError || www.isHttpError) {
//				Debug.Log(www.error);
//			}
//			else {
//				Debug.Log("Form upload complete!");
//			}

			WWW w = new WWW(POSTurl, form.data, headers);
			// UnityWebRequest www = UnityWebRequest.Put(PUTurl, rawData);
				
			yield return w;
			print(w.text);

//			if (!string.IsNullOrEmpty(w.error)) {
//				print(w.text);
//			}
//			else {
//				print(w.text);
//			}
		}
	
//		void SetOutputSize(int width, int height) {
//	
//			if (width == lastWidth && height == lastHeight) {
//				return;
//			}
//			else {
//				lastWidth = width;
//				lastHeight = height;
//			}
//	
//			if (outputTex != null) {
//				Destroy(outputTex);
//			}
//	
//			outputTex = new RenderTexture(width, height, 0);
//			outputTex.hideFlags = HideFlags.HideAndDontSave;
//	
//			if (externalTex != null) {
//				Destroy(externalTex);
//			}
//	
//			externalTex = new RenderTexture(width, height, 0);
//			externalTex.hideFlags = HideFlags.HideAndDontSave;
//		}
	
		string ScreenShotName(int width, int height){            
	
			return string.Format("{0}/{1}.jpg",
					galleryFolder,
					DateTime.Now.ToString("ddMMM-hh-mm-ss"));
	
		}
	
	
	}
}
