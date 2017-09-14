using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Uploader))]
public class CustomGBXTInspector : Editor {

	public override void OnInspectorGUI(){
		// DrawDefaultInspector();
		Uploader s = target as Uploader;
		
		GUILayout.Button("Capture Screenshot", GUILayout.Width(300), GUILayout.Height(30));
		GUI.enabled = false;

		if(GUILayout.Button("Capture Screenshot")){
			
		}

	}
}
