using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class CubeScript : MonoBehaviour {
	Rigidbody myBody;
	FirmataBridge arduinoUno;
	// Use this for initialization
	void Start () {
		myBody=GetComponent<Rigidbody>();
		arduinoUno = FirmataBridge.instance;
		arduinoUno.Open ();
		StartCoroutine (SetPins ());
	}
	IEnumerator SetPins(){
		do {
			yield return new WaitForEndOfFrame();
		} while(!arduinoUno.IsOpen ());
		arduinoUno.pinMode (13, PinMode.INPUT);
		arduinoUno.pinMode (2, PinMode.OUTPUT);

	}
	IEnumerator Wait(int milliseconds){
		yield return new WaitForSeconds (milliseconds/1000);
	}
	// Update is called once per frame
	void Update () {
		if (arduinoUno.IsOpen ()) {
			int direction = (int)(arduinoUno.analogRead (0)*1.5*Mathf.PI);
			float x = 50*Mathf.Cos ((float)(direction)/ 1024);
			float z = 50*Mathf.Sin ((float)(direction)/ 1024);
			if (arduinoUno.getKeyDown (13)) {
				Debug.Log("Jumped to: "+x+","+z+" with read"+direction);
				myBody.AddForce (new Vector3 (x, 50, z));
			}
			
		}
	}
	void OnCollisionEnter(Collision other){
		if(arduinoUno.IsOpen()) arduinoUno.digitalWrite (2, Value.HIGH);
	}
	void OnCollisionExit(Collision other){
		if(arduinoUno.IsOpen()) arduinoUno.digitalWrite (2, Value.LOW);
	}
}
