// Component Script

using UnityEngine; // To be able to use waituntilframe and such
using System.IO.Ports; // Serial port communication
using System.Collections.Generic; // to use "Dictionary"
using System.Collections; // to use IEnumerator / CoRoutines

public class FirmataBridge : MonoBehaviour {
	// Public Variables for initialisation
	[HideInInspector] public string portType;
	public uint portNumber = 3;
	public Board board = ArduinoBoards.Uno;
	public StandardBaudRate baudRate = StandardBaudRate.baud57600;
	public bool autoStart = false;
	public int autoStartDelay = 0;
	public bool reportAll = true;
	public int readTimeout = 1;
	public static FirmataBridge instance; // in future, this may be a List of instances instead, so one can have multiple connections with differents devices all at once
	// Private Variables
	private Dictionary<int,PinInfo> pins = new Dictionary<int,PinInfo> (); // pins infos. Digital pin are from 0 to ..., pin Analog n  corresponds to -(n+1)
	// so Analog 0 -> Pin -1, Analog 2 -> pin -2 and such
	private SerialPort serialPort;
	private int totalDigitalPins, totalDigitalPorts;
	private bool isOpen;
	//Variables 
	//Constructors
	void Start(){
		string portName;
		totalDigitalPins = board.totalDigitalIO + board.totalAnalogIn;
		totalDigitalPorts = (totalDigitalPins - totalDigitalPins % 8) / 8 + 1;
		for (int i = 0; i < totalDigitalPins; i++) {
			if (board.PWMEnabled.Contains(i))
				pins.Add (i, new PinInfo (PinType.PWM));
			else
				pins.Add (i, new PinInfo (PinType.DIGITAL));
		}
		for (int i = 0; i < board.totalAnalogIn+board.totalAnalogOut; i++) {
			if (i < board.totalAnalogIn)
				pins.Add (-(i + 1), new PinInfo (PinType.ANALOG));
			else pins.Add(-(i + 1), new PinInfo (PinType.DAC));
		}
		Debug.Log (pins.Count);
		switch (portType) {
		case PortSelection.AUTO:
			portName = SerialPort.GetPortNames () [0];
			break;
		case PortSelection.COM:
			portName = "COM" + portNumber;
			break;
		default:
			portName = "COM3";
			break;
		}
		switch (portType) {
		case PortSelection.AUTO:
		case PortSelection.COM:
			serialPort = new SerialPort (portName, (int)baudRate);
			serialPort.ReadTimeout = readTimeout;
			break;
		// Might extend the library to support other forms of 
		default:
			serialPort = new SerialPort (portName, (int)baudRate);
			serialPort.ReadTimeout=readTimeout;
			break;
		}
		if (instance == null)
			instance = this;
		this.enabled = false;
		if (autoStart) {
			StartCoroutine(WaitOpen (autoStartDelay));	
		}
		Debug.Log (serialPort.PortName);
	}		

	private IEnumerator WaitOpen(int milliseconds){
		yield return new WaitForSeconds (milliseconds / 1000); // it will stay about 5 more ms
		Open ();
	}

	// Open Method

	public void Open (){
		if (!isOpen) {
			try {
				serialPort.Open ();
				this.enabled = true;
				isOpen = true;
			}catch{
				Debug.Log ("errors while opening the serial port!");
			}
		}
		if (isOpen) {
			byte[] message = new byte[2]; // enlarge it when strings are to be supported
			message [1] = 1;
			//  if reportAll = true, enable report for all pins by default, else, pin are to be enabled when "pinMode" is used on them
			if (instance.reportAll) {
				for (int i = 0; i < totalDigitalPorts; i++) {
					message [0] = (byte)((int)Message.REPORT_DIGITAL_PORT | i);
					instance.serialPort.Write (message, 0, 2);
				}
				/*for (int i = 0; i < instance.board.totalAnalogIn+instance.board.totalAnalogOut; i++) {
					message [0] = (byte)((int)Message.REPORT_ANALOG_PIN | i);
					instance.serialPort.Write (message, 0, 2);
				}*/
			}
		}
	}

	// Utility Methods

	public bool IsOpen (){
		return isOpen;
	}

	// Update method. Start when this.enabled

	void Update(){
		
		if (isOpen) {
			string protocol_Version = "";
			int readed = instance.ReadByte ();
			if (((readed & (int)Message.ANALOG_MESSAGE) == (int)Message.ANALOG_MESSAGE)) {
				// 0xE1 & 0xE0 -> 0xE0
				int pin = readed - (int)Message.ANALOG_MESSAGE;
				int value = instance.ReadByte ();
				value += instance.ReadByte () * (int)Mathf.Pow (2, 7);
				instance.ChangePinState (pin, value);
			}
			if (((readed & (int)Message.DIGITAL_MESSAGE) == (int)Message.DIGITAL_MESSAGE)) {
				// 0x91 & 0x90 -> 0x90
				int port = readed - (int)Message.DIGITAL_MESSAGE;
				int max = Mathf.Min ((port + 1) * 8 - 1, totalDigitalPins);
				readed = instance.ReadByte ();
				Debug.Log ("for port " + port + " byte is " + readed);
				for (int i = port * 8; i < max; i++) {
					int value = readed & PinHelper.getPinMask (i % 8);
					if (value != 0)
						value = 1;
					instance.ChangePinState (i, value);
				}
				readed = instance.ReadByte ();
				if ((port + 1) * 8 < totalDigitalPins) {
					int value = readed & PinHelper.getPinMask (7);
					if (value != 0)
						value = 1;
					instance.ChangePinState (port * 8 + 8, value);
				}
			}
			if (readed == (int)Message.START_SYSEX) {
				// SYSEX Management on hold
				while (instance.serialPort.BytesToRead != (int)Message.SYSEX_END)
					instance.ReadByte ();
			}
			if (readed == (int)Message.PROTOCOL_VERSION) {
				protocol_Version = "";
				protocol_Version += instance.ReadByte () + "." + instance.ReadByte ();
				Debug.Log ("Protocol Version " + protocol_Version);
			}
			// SET_DIGITAL_PIN_MODE, SET_DIGITAL_PIN_VALUE, REPORT_ANALOG_PIN, REPORT_DIGITAL_PORT are only sent to and never received from the board, SYSEX_END is only at the end of SYSEXs
		}

	}

	// Read byte but don't care about the "no bytes to read" exception as serialPort.BytesToRead throw an exception himself so one can't check if there are bytes to read

	private int ReadByte(){
		int readed=0;
		try{
			readed=instance.serialPort.ReadByte();
		}catch{
		}
		return readed;
	}

	// change pinState, controlling keyUp and keyDown states

	private void ChangePinState(int pin, int value){
		if ((pins [pin].value == 0)&&(value == 1)) {
				pins [pin].keyDown = true;
				StartCoroutine (StopKeyDown (pin));
		}
		if ((pins [pin].value == 1)&&(value == 0)) {
			pins [pin].keyUp = true;
			StartCoroutine (StopKeyUp (pin));
		}
		pins [pin].value = value;
		if (pin == 13)
			Debug.Log ("value is " + pins [pin].value + " keydown: " + pins [pin].keyDown + " keyup: " + pins [pin].keyUp);
	}

	private IEnumerator StopKeyUp(int pin){
		yield return new WaitForEndOfFrame (); // skip current frame
		yield return new WaitForEndOfFrame (); // skip frame where it will be readed
		pins [pin].keyUp = false;
	}
	private IEnumerator StopKeyDown(int pin){
		yield return new WaitForEndOfFrame (); // skip current frame
		yield return new WaitForEndOfFrame (); // skip frame where it will be readed
		pins [pin].keyDown = false;
	}

	// Digital I/O Arduino functions 

	public void pinMode(int pin, PinMode mode){
		byte[] message = new byte[3];
		pins [pin].pinMode = mode;
		message [0] = (byte)Message.SET_DIGITAL_PIN_MODE;
		message [1] = (byte)pin;
		message [2] = (byte)mode; 
		serialPort.Write (message, 0, 3);
	}

	public void digitalWrite(int pin, Value state){
		byte[] message = new byte[3];
		pins [pin].value = (int)state;
		message [0] = (byte)Message.SET_DIGITAL_PIN_VALUE; // Single-pin Digital Write is much, much easier with "SetDigitalPinValue"
		message [1] = (byte)pin;
		message [2] = (byte)state; 
		serialPort.Write (message, 0, 3);
	}
	public int digitalRead(int pin){
		return pins [pin].value;
	}

	// Additional Digital I/O functions based on Unity Input
	public bool getKeyUp(int pin){
		return pins [pin].keyUp;
	}
	public bool getKeyDown(int pin){
		
		return pins [pin].keyDown;
	}
	public bool getKey(int pin){
		if (pins [pin].value == 1)
			return true;
		else return false;
	}

	//

}		
// Sysex Queries are to be implemented later




