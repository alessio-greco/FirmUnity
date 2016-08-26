using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class CubeScript : MonoBehaviour {
	Rigidbody myBody;
	Transform myTransform;
	FirmataBridge arduinoUno;
	// Use this for initialization
	void Start () {
		myBody=GetComponent<Rigidbody>();
		myTransform = GetComponent<Transform> ();
		arduinoUno = FirmataBridge.instance;
		arduinoUno.Open ();
		StartCoroutine (SetPins ());
	}
	IEnumerator SetPins(){
		do {
			yield return new WaitForEndOfFrame();
		} while(!arduinoUno.IsReady ());
		arduinoUno.pinMode (13, PinMode.INPUT);
		arduinoUno.pinMode (2, PinMode.OUTPUT);
		arduinoUno.pinMode (5, PinMode.PWM);
		arduinoUno.servoConfig (8, 0, 1024);
		arduinoUno.pinMode (arduinoUno.A (0), PinMode.ANALOG);
	}
	IEnumerator Wait(int milliseconds){
		yield return new WaitForSeconds (milliseconds/1000);
	}
	// Update is called once per frame
	void Update () {
		if (arduinoUno.IsReady ()) {
			int counter = 0;
			int potentiometer = arduinoUno.analogRead (0);
			Debug.Log ("potententiometer: "+potentiometer);
			int direction = (int)(potentiometer*1.5*Mathf.PI);
			arduinoUno.analogWrite (8, 1024-potentiometer);
			float x = 50*Mathf.Cos ((float)(direction)/ 1024);
			float z = 50*Mathf.Sin ((float)(direction)/ 1024);
			if (arduinoUno.getKeyDown (13)) {
				Debug.Log("Jumped to: "+x+","+z+" with read"+direction);
				myBody.AddForce (new Vector3 (x, 50, z));
			}
			if ((counter = arduinoUno.pulseln (13)) != 0)
				Debug.Log ("pulse length: " + counter + " ms");
			arduinoUno.analogWrite (5,Mathf.FloorToInt(Vector3.Distance (myTransform.position, new Vector3 (-2.336f, 1.6f, -9.998f))));
		}
	}
	void OnCollisionEnter(Collision other){
		if(arduinoUno.IsOpen()) arduinoUno.digitalWrite (2, Value.HIGH);
	}
	void OnCollisionExit(Collision other){
		if(arduinoUno.IsOpen()) arduinoUno.digitalWrite (2, Value.LOW);
	}
}
