using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;

using System.Net;
using System.IO;
using UnityEditor;
using FTPNetwork;

public class Uploader : MonoBehaviour
{   

    public bool dropdownbutton;
    public string userName;
    public int r;

    private string FTPHost = "ftp://ftp.ekrivoruchko.com/";
    private string FTPUserName = "vrsuploader@eugene.works";
    private string FTPPassword = "1uploadvrscreenshot";
    [HideInInspector]
    public string FilePath;
    
    public string Token = "oZNZh1S41yghfzuv6PnVdakGlij1";  
    public KeyCode screenShotKey = KeyCode.F1;
    public bool keepImagesLocally = false;

    private string invalidToken = "HUMajetpFtWJcgaB8FcNAqwi0rG3";
    private string PUTurl = "https://us-central1-graybox-219f6.cloudfunctions.net/upload";
  

    public void UploadFile()
    {
        // FilePath = Application.dataPath + "/StreamingAssets/data.xml";
        // Debug.Log("Path: " + FilePath);


        WebClient client = new System.Net.WebClient();
        Uri uri = new Uri((FTPHost) + new FileInfo(FilePath).Name);

        client.UploadProgressChanged += new UploadProgressChangedEventHandler(OnFileUploadProgressChanged);
        client.UploadFileCompleted += new UploadFileCompletedEventHandler(OnFileUploadCompleted);
        client.Credentials = new System.Net.NetworkCredential(FTPUserName, FTPPassword);
        client.UploadFileAsync(uri, "STOR", FilePath);
    }

	public void UploadFileAlt(){
		/* Create Object Instance */
		ftp ftpClient = new ftp(@"ftp://ftp.ekrivoruchko.com/", "vrsuploader@eugene.works", "1uploadvrscreenshot");

		/* Upload a File */
		ftpClient.upload(new FileInfo(FilePath).Name, @FilePath);
		ftpClient = null;
		
		string url = "eugene.works/vrs/?i=" + Path.GetFileNameWithoutExtension(new FileInfo(FilePath).Name);
        Debug.Log("Screenshot URL"+"\n"+url);

	}

    void OnFileUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
    {
        Debug.Log("Uploading Progreess: " + e.ProgressPercentage);
    }

    void OnFileUploadCompleted(object sender, UploadFileCompletedEventArgs e)
    {	
		string url = "eugene.works/vrs/?i=" + Path.GetFileNameWithoutExtension(new FileInfo(FilePath).Name);

        // Debug.Log("Screenshot URL (also copied to clipboard)"+"\n"+"<color=blue><b>"+url+"</b></color>");
        Debug.Log("Screenshot URL"+"\n"+url);

		        // EditorGUIUtility.systemCopyBuffer = url;


		// te.text = url;
		// te.SelectAll();
		// te.Copy();


    }

    public void UploadToGBXT(){
        Debug.Log("poehali");
        StartCoroutine(UploadPost());
    }
    IEnumerator UploadPost(){
        
        WWWForm form = new WWWForm();
                
        form.AddBinaryData("file", File.ReadAllBytes(@FilePath), "screenShot.jpg", "image/jpeg");
        
        Dictionary<string, string> headers = form.headers;
        
        headers["x-token"] = Token;
        headers["debug"] = "lol";
        headers["content-type"] = "application/json; charset=utf-8";
        // headers["X-HTTP-Method-Override"] = "PUT";

        WWW w = new WWW(PUTurl, form.data, headers);
        // UnityWebRequest www = UnityWebRequest.Put(PUTurl, rawData);

        yield return w;
        if (!string.IsNullOrEmpty(w.error)) {
            print(w.text);
        }
        else {
            print(w.text);
        }
    }

    void Start()
    {
        // UploadFile();
    }
}
