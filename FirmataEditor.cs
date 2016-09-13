// Editor modifications
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FirmataBridge))]
public class FirmataEditor : Editor{
	string[] portChoices = new string[] { PortSelection.AUTO, PortSelection.COM, PortSelection.PLUS, PortSelection.CHIPKIT, PortSelection.ETHERNET, PortSelection.BLE, PortSelection.WIFI};
	int portIndex=0;

	void OnEnable (){
		var firmataBridge = target as FirmataBridge;
		for(int i=0;i<portChoices.Length;i++){
			if (portChoices [i] == firmataBridge.portType)
				portIndex = i;
		}
	}

	public override void OnInspectorGUI (){
		serializedObject.Update ();
		var firmataBridge = target as FirmataBridge;
		portIndex = EditorGUILayout.Popup ("Port Type: ", portIndex, portChoices);
		DrawPropertiesExcluding (serializedObject, "m_Script");
		firmataBridge.portType = portChoices [portIndex];
		serializedObject.ApplyModifiedProperties ();
		EditorUtility.SetDirty (target);
	}		
}
