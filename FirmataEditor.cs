// Editor modifications
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FirmataBridge))]
public class FirmataEditor : Editor{
	string[] portChoices = new string[] { PortSelection.AUTO, PortSelection.COM, PortSelection.PLUS, PortSelection.CHIPKIT, PortSelection.ETHERNET, PortSelection.BLE, PortSelection.WIFI};
	string[] boardChoices;
	int portIndex = 1;
	int boardIndex = 0;

	void OnEnable (){
		boardChoices = Board.returnNameArray();
		for (int i = 0; i < boardChoices.Length; i++) {
			if (boardChoices [i] == "Arduino Uno") {
				boardIndex = i;
				break;
			}
		}
	}

	public override void OnInspectorGUI (){
		serializedObject.Update ();
		var firmataBridge = target as FirmataBridge;
		portIndex = EditorGUILayout.Popup ("Port Type: ", portIndex, portChoices);
		DrawPropertiesExcluding (serializedObject, "m_Script");
		boardIndex = EditorGUILayout.Popup ("Board: ", boardIndex, boardChoices);
		firmataBridge.portType = portChoices [portIndex];
		firmataBridge.board = Board.getBoard (boardIndex);
		serializedObject.ApplyModifiedProperties ();
		EditorUtility.SetDirty (target);
	}		
}