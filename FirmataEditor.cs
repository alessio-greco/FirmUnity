#if UNITY_EDITOR
// Editor modifications
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FirmataBridge))]
public class FirmataEditor : Editor{
	string[] portChoices = new string[] { PortSelection.AUTO, PortSelection.COM, PortSelection.PLUS, PortSelection.CHIPKIT, PortSelection.ETHERNET, PortSelection.BLE, PortSelection.WIFI};
	int portIndex = 1;

	void OnEnable (){
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
#endif