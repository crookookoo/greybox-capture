using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using System.IO;
namespace FBCapture
{
[RequireComponent(typeof(Camera))]
public class Greybox : MonoBehaviour {
	public string token;
	
	[SearchableEnum]
	public KeyCode screenshotHotkey = KeyCode.F1;
	public bool keepLocalImageCopies = false;

	[HideInInspector]
	public string state = "Ready";

	[HideInInspector]
	public List<string> urls = new List<string>();

	[HideInInspector]
	public bool canTakeScreenshot = true;
   
    [HideInInspector]
    public string FilePath;
    
	[HideInInspector]    
	public string outputPath;

	private string POSTurl = "https://us-central1-graybox-219f6.cloudfunctions.net/upload";

	private int lastWidth = 0, lastHeight = 0;
			
	private RenderTexture cubemapTex;
	private RenderTexture outputTex;  // equirect or cubemap ends up here
	private RenderTexture externalTex;
	private RenderTexture equirect;	
    private Camera camera;
	private string screenshotFullPath;

	[DllImport("FBCapture", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
	private static extern bool saveScreenShot(IntPtr texture, string path, bool needFlipping);                


	// Use this for initialization
	void Start () {
		camera = GetComponent<Camera>();
    	cubemapTex = new RenderTexture(2048, 2048, 0);
        cubemapTex.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        cubemapTex.hideFlags = HideFlags.HideAndDontSave;

		if (string.IsNullOrEmpty(outputPath)) {
			outputPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Gallery");
			// create the directory
			if (!Directory.Exists(outputPath)) {
				Directory.CreateDirectory(outputPath);
			}
		}

	}
	
	void Update () {
		if(Input.GetKeyDown(screenshotHotkey)){
			TakeScreenShot();
		}
	}

	public void TakeScreenShot(){
		Debug.Log("Starting screenshot capture...");
		TakeScreenshot(4096, 2048, ScreenShotName(4096, 2048));
		// StartCoroutine(CaptureScreenshot(4096,2048));
		// 	urls.Add("http://greybox.it/lsdfe4");
		// 	canTakeScreenshot = false;
	}

	void TakeScreenShotNative(){
		camera.stereoSeparation = 0.064f; // Eye separation (IPD) of 64mm.
		camera.RenderToCubemap(cubemapTex, 63, Camera.MonoOrStereoscopicEye.Left);
		cubemapTex.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
		
		FilePath = ScreenShotName(4096, 2048);
		DumpRenderTexture(cubemapTex, FilePath);

	}

    void DumpRenderTexture(RenderTexture rt, string jpgOutPath)
    {
        var oldRT = RenderTexture.active;

        var tex = new Texture2D(rt.width, rt.height);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        File.WriteAllBytes(jpgOutPath, tex.EncodeToJPG());
        RenderTexture.active = oldRT;
    }

	public void TakeScreenshot(int width, int height, string screenshotPathName = "")
	{
		if (!string.IsNullOrEmpty(screenshotPathName)) {
			screenshotFullPath = screenshotPathName;
		}

		camera.RenderToCubemap(cubemapTex);  // render cubemap

		StartCoroutine(CaptureScreenshot(width, height));            
	}


	public void ClearURLs(){
		urls.Clear();
	}

	IEnumerator CaptureScreenshot(int width, int height) {
		SetOutputSize(width, height);
		// yield a frame to re-render into the rendertexture
		yield return new WaitForEndOfFrame();

		Debug.LogFormat("[SurroundCapture] Saved {0} x {1} screenshot: {2}", width, height, screenshotFullPath);

		if(!saveScreenShot(externalTex.GetNativeTexturePtr(), screenshotFullPath, false)) {
			Debug.Log("Failed on taking screenshot. Please check FBCaptureSDK.log file");
		} else {
			Debug.Log("Screenshot captured, starting upload...");
			FilePath = screenshotFullPath;
			StartCoroutine(UploadPost());

			// uploadScreenShot(screenshotFullPath);
		}
	}

	IEnumerator UploadPost(){
	
		WWWForm form = new WWWForm();
				
		form.AddBinaryData("file", File.ReadAllBytes(@FilePath), "screenShot.jpg", "image/jpeg");
		
		Dictionary<string, string> headers = form.headers;
		
		headers["x-token"] = token;
		// headers["debug"] = "lol";
		headers["content-type"] = "application/json; charset=utf-8";

		WWW w = new WWW(POSTurl, form.data, headers);
		// UnityWebRequest www = UnityWebRequest.Put(PUTurl, rawData);

		yield return w;
		if (!string.IsNullOrEmpty(w.error)) {
			print(w.text);
		}
		else {
			print(w.text);
		}
	}

	void SetOutputSize(int width, int height) {

		if (width == lastWidth && height == lastHeight) {
			return;
		}
		else {
			lastWidth = width;
			lastHeight = height;
		}

		if (outputTex != null) {
			Destroy(outputTex);
		}

		outputTex = new RenderTexture(width, height, 0);
		outputTex.hideFlags = HideFlags.HideAndDontSave;

		if (externalTex != null) {
			Destroy(externalTex);
		}

		externalTex = new RenderTexture(width, height, 0);
		externalTex.hideFlags = HideFlags.HideAndDontSave;
	}

	string ScreenShotName(int width, int height){            
		//("{0}/screenshot_{1}x{2}_{3}.jpg"
		// return string.Format("{0}/screenshot_{1}x{2}_{3}.jpg",
		//                     outputPath,
		//                     width, height,
		//                     DateTime.Now.ToString("ddMMM-hh-mm-ss"));

		return string.Format("{0}/{1}.jpg",
				outputPath,
				DateTime.Now.ToString("ddMMM-hh-mm-ss"));

	}


}
}
