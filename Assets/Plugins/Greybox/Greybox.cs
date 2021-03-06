﻿// Copyright (c) 2019 Eugene Krivoruchko
// Learn more at http://greybox.it

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
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
		[SerializeField] private string localImageFolder = "Assets/Plugins/Greybox/Greybox Gallery";

		[Tooltip("The hotkey alternative to the capture button below")]
		[SerializeField] private KeyCode screenshotHotkey = KeyCode.F1;

		[HideInInspector]
		public string state = "Ready";
	
		[HideInInspector]
		public List<string> urls = new List<string>();
	
		[HideInInspector]
		public bool canTakeScreenshot = true;
	   
		private string FilePath;
		private string galleryFolder;
	
		private string POSTurl = "https://us-central1-graybox-219f6.cloudfunctions.net/upload";
	
		private int lastWidth = 0, lastHeight = 0;
				
		private RenderTexture cubemapTex;
		private RenderTexture equirect;	
		private string screenshotFullPath, lastScreenshotName;	
		
		public struct SimpleJsonData
		{
			public string url;
		}
		
		public struct ErrorJsonData
		{
			public string error;
		}
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
			
				if (!Directory.Exists(galleryFolder)) {
					Directory.CreateDirectory(galleryFolder);
				}
			}
		}
		
		void Update () {
			if(Input.GetKeyDown(screenshotHotkey)){
				TakeScreenShot();
			}
		}
	
		public void TakeScreenShot(){	
			camera.stereoSeparation = 0.064f; // Eye separation (IPD) of 64mm.
			camera.RenderToCubemap(cubemapTex, 63, Camera.MonoOrStereoscopicEye.Mono);
			cubemapTex.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
			
			FilePath = ScreenShotName();
			StartCoroutine(SaveRenderTexture(equirect, FilePath));
		}
	
		IEnumerator SaveRenderTexture(RenderTexture rt, string jpgOutPath)
		{
			yield return new WaitForEndOfFrame();
			
			var oldRT = RenderTexture.active;
	
			var tex = new Texture2D(rt.width, rt.height);

			float yAngle = camera.transform.eulerAngles.y;

			RenderTexture.active = rt;
			tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0,false);
			RotateEquirect(tex, yAngle);
			tex.Apply();

			byte[] bytes = tex.EncodeToJPG();

			File.WriteAllBytes(jpgOutPath, bytes);
			RenderTexture.active = oldRT;
			
			Debug.Log("Greybox saved JPG locally: " + lastScreenshotName);
			
			FilePath = jpgOutPath;
			StartCoroutine(UploadPost(bytes));
			
		}

		private void RotateEquirect(Texture2D tex, float yAngle)
		{
			int shift = Mathf.FloorToInt(yAngle * tex.width / 360);

			Color32[] result = new Color32[tex.width * tex.height];
			Color32[] original = tex.GetPixels32();
			
			for (int i = 0; i < tex.height; i++)
			{
				for (int j = 0; j < tex.width; j++)
				{
					int delta = j + shift;
					
					if (delta < 0) delta += tex.width;
					if (delta >= tex.width) delta -= tex.width;
					
					result[i * tex.width + j] = original[i * tex.width + delta];
				}
			}
			
			tex.SetPixels32(result);
		}
		
		public void ClearURLs(){
			urls.Clear();
		}
				
		IEnumerator UploadPost(byte[] bytes){

			WWWForm form = new WWWForm();
			
			form.AddBinaryData("file", bytes, "screenShot.jpg", "image/jpeg");
						
			using (var w = UnityWebRequest.Post(POSTurl, form))
			{
				
				w.SetRequestHeader("x-token", token);
				
				yield return w.SendWebRequest();
				
				if (w.isNetworkError || w.isHttpError) {
					ErrorJsonData response = JsonUtility.FromJson<ErrorJsonData>(w.downloadHandler.text);
					Debug.Log("<color=maroon>Greybox upload error: " + response.error + "</color>");
				}
				else {
					SimpleJsonData response = JsonUtility.FromJson<SimpleJsonData>(w.downloadHandler.text);
					Debug.Log("<color=navy>Greybox upload complete: " + response.url + " (copied to clipboard)" + "</color>");
					urls.Add(response.url);
					CopyToClipboard(response.url);
				}
			}
		}
	
		string ScreenShotName(){        
			lastScreenshotName = string.Format("{0}/{1}.jpg",
				localImageFolder, SceneManager.GetActiveScene().name + " " +
				DateTime.Now.ToString("ddMMM-hh-mm-ss"));
			return lastScreenshotName;
		}
		
		private void CopyToClipboard(string str){
			TextEditor te = new TextEditor();
			te.text = str;
			te.SelectAll();
			te.Copy();
		
			state = "URL is copied to clipboard!";
			
			if(urls.Count > 1) 
				state = "Last URL is copied to clipboard!";
		}
	}
}
