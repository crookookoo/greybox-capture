using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Net;
using System.IO;
using UnityEditor;
using FTPNetwork;

public class Uploader : MonoBehaviour
{
    private string FTPHost = "ftp://ftp.ekrivoruchko.com/";
    private string FTPUserName = "vrsuploader@eugene.works";
    private string FTPPassword = "1uploadvrscreenshot";
    public string FilePath;
    
    private string validToken = "oZNZh1S41yghfzuv6PnVdakGlij1";
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

    }

    IEnumerator UploadPut(){

        WWWForm form = new WWWForm();
        
        form.AddField( "name", "value" );
        Dictionary<string, object> headers  = StateManager.HashtableToDictionary<string, object>(form.headers);
        byte[] rawData = form.data;
        string url = "www.myurl.com";

        // Add a custom header to the request.
        // In this case a basic authentication to access a password protected resource.
        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes("username:password"));

        // Post a request to an URL with our custom headers
        WWW www = new WWW(url, rawData, headers);
        yield return www;



        //-----------------



        // byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        byte[] myData = File.ReadAllBytes(new FileInfo(FilePath).Name);
        
        UnityWebRequest www = UnityWebRequest.Put(PUTurl, myData);
        yield return www.Send();
 
        if(www.isError) {
            Debug.Log(www.error);
        }
        else {
            Debug.Log("Upload complete!");
        }

    }

    public byte[] imageToByteArray(System.Drawing.Image imageIn)
    {
        MemoryStream ms = new MemoryStream();
        imageIn.Save(ms,System.Drawing.Imaging.ImageFormat.Gif);
        return  ms.ToArray();
    }

    public Image byteArrayToImage(byte[] byteArrayIn)
    {
        MemoryStream ms = new MemoryStream(byteArrayIn);
        Image returnImage = Image.FromStream(ms);
        return returnImage;
    }

    void Start()
    {
        // UploadFile();
    }
}
