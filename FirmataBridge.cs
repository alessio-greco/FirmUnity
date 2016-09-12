// Component Script

using UnityEngine; // To be able to use waituntilframe and such
using System.IO.Ports; // Serial port communication
using System.Collections.Generic; // to use "Dictionary"
using System.Collections; // to use IEnumerator / CoRoutines 

public class FirmataBridge : MonoBehaviour {
	// Public Variables for initialisation
	[HideInInspector] public string portType;
	public uint portNumber = 3;
	public StandardBaudRate baudRate = StandardBaudRate.baud57600;
	public bool autoStart = false;
	public int autoStartDelay = 0;
	public bool reportAllDigitalPins = true; // Digital pins
	public int readTimeout = 1;
	public static FirmataBridge instance; // in future, this may be a List of instances instead, so one can have multiple connections with differents devices all at once
	public int samplingInterval = 33; // 2^14 is the maximum samplingInterval supportable.
	// Private Variables
	private Dictionary<int,PinInfo> pins = new Dictionary<int,PinInfo> (); // pins infos. Digital pin are from 0 to ..., pin Analog n  corresponds to -(n+1)
	// so Analog 0 -> Pin -1, Analog 2 -> pin -2 and such
	private Dictionary<int,int> analogPins = new Dictionary<int, int>();
	private SerialPort serialPort;
	private bool isOpen;
	private bool isReady = false;
	private Dictionary<int, int> pulseCounter = new Dictionary<int, int>();
	//Variables 
	//Constructors
	void Start(){
		string portName;
		switch (portType) {
		case PortSelection.AUTO:
			portName = SerialPort.GetPortNames () [0]; // implement a better AUTO selection...
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
	}		

	private IEnumerator WaitOpen(int milliseconds){
		yield return new WaitForSeconds (milliseconds / 1000); // it will stay about 5 more ms
		Open ();
	}

	// Open Method

	public void Open (int delay){
		StartCoroutine (WaitOpen (delay));
	}

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
			byte[] message = new byte[5]; // enlarge it when strings are to be supported
			// Send capability Query
			message [0] = (byte)Message.START_SYSEX;
			message [1] = (byte)Sysex.CAPABILITY_QUERY;
			message [2] = (byte)Message.SYSEX_END;
			serialPort.Write (message, 0, 3);

			// Ask for analogPins Mapping
			message[1] = (byte)Sysex.ANALOG_MAPPING_QUERY;
			serialPort.Write (message, 0, 3);

			if (samplingInterval >= (int)Mathf.Pow (2, 14))
				samplingInterval = (int)Mathf.Pow (2, 14) - 1;

			// send a request to modify the sampling rate

			message [0] = (byte)Message.START_SYSEX;
			message [1] = (byte)Sysex.SAMPLING_INTERVAL;
			message [2] = (byte)(samplingInterval%Mathf.Pow (2, 7)); // if sampling = 1200 -> message[2] = 0x30 (  seven lsb ) 
			message [3] = (byte)Mathf.Floor((samplingInterval-samplingInterval%Mathf.Pow (2, 7))/Mathf.Pow (2, 7)); // message[3]= 0x09 ( seven msb 0x09 
			message [4] = (byte)Message.SYSEX_END;
			serialPort.Write (message, 0, 5);

		}
	}

	// Utility Methods

	public bool IsOpen (){
		return isOpen;
	}

	public bool IsReady(){
		return isReady;
	}

	public int A(int pin){
		return analogPins [pin];
	}

	// Update method. Start when this.enabled

	void Update(){
		
		if (isOpen) {
			string protocol_Version = "";
			int readed = ReadByte ();
			if (readed == (int)Message.START_SYSEX) {
				// SYSEX Management on hold
				Debug.Log("Sysex");
				readed = ReadByte ();
				switch (readed) {
				case (int)Sysex.CAPABILITY_RESPONSE:
					ReadCapabilities ();
					if (reportAllDigitalPins)
						ReportAllDigitalPorts ();
					//  if reportAllDigitalPins = true, enable report for all pins by default, else, pin are to be enabled when "pinMode" is used on them
					// no automatic reporting for analog Pins as they need frequent readings
					byte[] message = new byte[2];
					message [1] = 1;
					break;
				case (int)Sysex.ANALOG_MAPPING_RESPONSE:
					mapAnalogs ();
					isReady = true;
					break;
				default:
					while (readed!=(int)Message.SYSEX_END) {
						Debug.Log (readed);
					}
					break;
				}

			}
			if (readed == (int)Message.PROTOCOL_VERSION) {
				protocol_Version = "";
				protocol_Version += ReadByte () + "." + ReadByte ();
				Debug.Log ("Protocol Version " + protocol_Version);
				readed = ReadByte ();
			}
			while ((readed < 0xF0 ) && (readed > 0xDF )) { 
				int pin = readed - (int)Message.ANALOG_MESSAGE;
				int value = ReadByte ();
				value += ReadByte () * (int)Mathf.Pow (2, 7);
				ChangePinState (analogPins[pin], value);
				readed = ReadByte (); // continue
			}
			while ((readed < 0xA0 ) && (readed > 0x8F )) {
				// 0x91 & 0x90 -> 0x90
				int port = readed - (int)Message.DIGITAL_MESSAGE;
				int max = Mathf.Min ((port + 1) * 8 - 1, pins.Count);
				readed = ReadByte ();
				for (int i = port * 8; i < max; i++) {
					int value = readed & PinHelper.getPinMask (i % 8);
					if(pins[i].currentMode!=PinMode.ANALOG) ChangePinState (i, value);
				}
				readed = ReadByte ();
				if ((port + 1) * 8 < pins.Count) {
					int value = readed & PinHelper.getPinMask (7);
					if (value != 0)
						value = 1;
					ChangePinState (port * 8 + 8, value);
				}
				readed= ReadByte (); // 
			}

		}

	}

	private void ReadCapabilities(){
		int readed0, readed1;
		int i;
		for (i = 0;; i++) {
			List<PinMode> supported = new List<PinMode> ();
			readed0 = ReadByte ();
			int analogBits = 0;
			int servoBits = 0;
			int PWMBits = 0;
			int i2cBits = 0;
			Debug.Log ("pin " + i);
			while ((readed0!= 0x7F)&&(readed0!=(int)Message.SYSEX_END)) {
				readed1 = ReadByte ();
				switch (readed0) {
				case (int)PinMode.ANALOG:
					Debug.Log ("is Analog Compatible");
					supported.Add (PinMode.ANALOG);
					analogBits=readed1;
					break;
				case (int)PinMode.ENCODER:
					supported.Add (PinMode.ENCODER);
					break;
				case (int)PinMode.I2C:
					Debug.Log ("is i2c Compatible");
					i2cBits=readed1;
					supported.Add (PinMode.I2C);
					break;
				case (int)PinMode.INPUT:
					supported.Add (PinMode.INPUT);
					break;
				case (int)PinMode.INPUT_PULLUP:
					supported.Add (PinMode.INPUT_PULLUP);
					break;
				case (int)PinMode.ONEWIRE:
					supported.Add (PinMode.ONEWIRE);
					break;
				case (int)PinMode.OUTPUT:
					supported.Add (PinMode.OUTPUT);
					break;
				case (int)PinMode.PWM:
					Debug.Log ("is PWM Compatible");
					PWMBits=readed1;
					supported.Add (PinMode.PWM);
					break;
				case (int)PinMode.SERIAL:
					supported.Add (PinMode.SERIAL);
					Debug.Log ("is Serial Compatible bits:"+readed1);
					break;
				case (int)PinMode.SERVO:
					Debug.Log ("is Servo Compatible");
					servoBits=readed1;
					supported.Add (PinMode.SERVO);
					break;
				case (int)PinMode.STEPPER:
					supported.Add (PinMode.STEPPER);
					break;
				default:
					break;
				}

				readed0 = ReadByte ();
			}
			if (readed0 == (int)Message.SYSEX_END) {
				i--;
				break;
			} else {
				if (!pins.ContainsKey (i)) {
					pins.Add (i, new PinInfo ());
					pins [i].setCompatibilities (supported, analogBits,PWMBits,servoBits,i2cBits);
				}
			}
		}
		Debug.Log ("Capabilities response End");
		if (i != pins.Count) {
			Debug.Log ("Board has " + i + " pins!");
		}
	}

	private void mapAnalogs(){
		for (int i = 0;; i++) {
			int readed = ReadByte ();
			if (readed == (int)Message.SYSEX_END)
				break;
			if (readed != 127) {
				analogPins.Add (readed, i);
				Debug.Log("A"+readed+" corresponds to "+i);
			}
		}
					
	}


	// Read byte but don't care about the "no bytes to read" exception as serialPort.BytesToRead throw an exception himself so one can't check if there are bytes to read

	private int ReadByte(){
		int readed=0;
		try{
			readed=serialPort.ReadByte();
		}catch{
		}
		return readed;
	}

	// change pinState, controlling keyUp and keyDown states

	private void ChangePinState(int pin, int value){
		if ((pins [pin].value == 0)&&(value != 0)) {
				pins [pin].keyDown = true;
				StartCoroutine (StopKeyDown (pin));
		}
		if ((pins [pin].value != 0)&&(value == 0)) {
			pins [pin].keyUp = true;
			StartCoroutine (StopKeyUp (pin));
		}
		pins [pin].value = value;
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

	private void ReportAllDigitalPorts(){
		for (int i = 0; i < (pins.Count) / 8 + 1; i++)
			reportPort (i);
	}

	private void reportPort(int port){
		for (int i = port; i < 8 * port; i++) {
			pins [i].reporting = true;
		}
		byte[] message = new byte[2]; // enlarge it when strings are to be supported
		message [0] = (byte)((int)Message.REPORT_DIGITAL_PORT | port);
		message [1] = 1;
		serialPort.Write (message, 0, 2);
	}

	private void reportAnalogPin(int analogPin){
		pins [analogPins[analogPin]].analogReporting = true;
		pins [analogPins [analogPin]].reporting = false;
		byte[] message = new byte[2]; // enlarge it when strings are to be supported
		message [0] = (byte)((int)Message.REPORT_ANALOG_PIN|analogPin);
		message [1] = 1;
		serialPort.Write (message, 0, 2);
	}
		
	private void extendedAnalog(int pin, int value){
		byte[] message = new byte [20];
		message[0] = (byte)Message.START_SYSEX;
		message[1] = (byte)Sysex.EXTENDED_ANALOG;
		message[2] = (byte)pin;
		switch (pins [pin].currentMode) {
		case PinMode.PWM:
			if (value > 255)
				value = 255;
			if (value < 0)
				value = 0;
			break;
		case PinMode.SERVO:
			if (value < 0)
				value = 0;
			if (value > 180)
				value = 180;
			Debug.Log ("result angle: "+value);
			break;
		default:
			break;
		}
		message[3] = (byte)(value%128);
		message[4] = (byte)(Mathf.FloorToInt(value/128));
		message[5] = (byte)Message.SYSEX_END;
		serialPort.Write(message,0,6);
	}

	private IEnumerator CheckPulse(int pin, bool startingValue){
		while (getKey (pin) == startingValue) {
			yield return new WaitForEndOfFrame ();
		}
		int counter = 0;
		while (getKey (pin) != startingValue){
			counter += (int)Mathf.FloorToInt(Time.deltaTime*1000);
			yield return new WaitForEndOfFrame ();
		}
		pulseCounter [pin] = counter;
	}

	// Digital I/O Arduino functions 

	public void pinMode(int pin, PinMode mode){
		if ((pin > pins.Count) || (pin < 0)) {
			Debug.Log ("Pin doesn't exist!");
			return;
		}
		if (!pins [pin].isPinModeSupported(mode)){
			Debug.Log ("Pin mode not supported!");
			return;
		}
		byte[] message = new byte[3];
		pins [pin].currentMode = mode;
		if (!pins [pin].reporting)
			reportPort ((pin - pin % 8) / 8);
		message [0] = (byte)Message.SET_DIGITAL_PIN_MODE;
		message [1] = (byte)pin;
		message [2] = (byte)mode; 
		serialPort.Write (message, 0, 3);
	}

	public void digitalWrite(int pin, Value state){
		if ((pin > pins.Count) || (pin < 0) ) {
			Debug.Log ("Pin doesn't exist!");
			return;
		}
		if (pins [pin].currentMode != PinMode.OUTPUT) {
			Debug.Log ("Pin mode isn't OUTPUT!");
			return;
		}
		byte[] message = new byte[3];
		pins [pin].value = (int)state;
		message [0] = (byte)Message.SET_DIGITAL_PIN_VALUE; // Single-pin Digital Write is much, much easier with "SetDigitalPinValue"
		message [1] = (byte)pin;
		message [2] = (byte)state; 
		serialPort.Write (message, 0, 3);
	}
	public int digitalRead(int pin){
		if ((pin > pins.Count) || (pin < 0)){
			Debug.Log ("Pin doesn't exist!");
			return 0;
		}
		return pins [pin].value;
	}



	// Additional Digital I/O functions based on Unity Input
	public bool getKeyUp(int pin){
		if ((pin > pins.Count) || (pin < 0))
			return false;
		return pins [pin].keyUp;
	}
	public bool getKeyDown(int pin){
		if ((pin > pins.Count) || (pin < 0))
			return false;
		return pins [pin].keyDown;
	}
	public bool getKey(int pin){
		if ((pin > pins.Count) || (pin < 0))
			return false;
		if (pins [pin].value !=0)
			return true;
		else return false;
	}

	// non-pwm analog function

	public int analogRead(int analogPin){
		if (pins [analogPins [analogPin]].currentMode != PinMode.ANALOG)
			pinMode (analogPins [analogPin], PinMode.ANALOG);
		if (pins [analogPins[analogPin]].analogReporting==false)
			reportAnalogPin (analogPin); // start pin reporting
		return pins [analogPins[analogPin]].value; 
	}

	// analog pwm function

	public void analogWrite(int pin, int value){
		if ((pin < 0) || (pin > pins.Count)) {
			Debug.Log ("Pin doesn't exist!");
			return;
		}
		extendedAnalog (pin, value);
	}

	// Advanced I/O

	public int pulseln(int pin){ // Non blocking implementation of pulseln, 
		//that is used to issue a counter for a pulse in a certain pin and in case the counter started, return 0 if the counter didn't stop and pulseln if the counter stopped
		if (pulseCounter.ContainsKey (pin)) { // if there's a pulse counter for that 
			int counter=pulseCounter[pin];
			if (counter == 0)
				return 0;//counter is still counting or there wasn't any pulse
			pulseCounter.Remove(pin); // remove counter 
			return counter; // return the pulse duration
		} else {
			pulseCounter.Add (pin, 0);
			StartCoroutine(CheckPulse(pin,getKey(pin)));
			return 0;
		}

	}

	// Servo 

	public void servoConfig(int pin, int minPulse, int maxPulse){
		if ((pin < 0) || (pin > pins.Count)) {
			Debug.Log ("Pin doesn't exist!");
			return;
		}
		if (!pins [pin].isPinModeSupported (PinMode.SERVO)) {
			Debug.Log ("Servo not supported for " + pin);
			return;
		}
		byte[] message = new byte[8];
		message [0] = (byte)Message.START_SYSEX;
		message [1] = (byte)Sysex.SERVO_CONFIG;
		message [2] = (byte)pin;
		message [3] = (byte)(minPulse % 128);
		message [4] = (byte)Mathf.FloorToInt (minPulse / 128);
		message [5] = (byte)(maxPulse % 128);
		message [6] = (byte)Mathf.FloorToInt (maxPulse / 128);
		pins [pin].minPulse = minPulse;
		pins [pin].maxPulse = maxPulse;
		message [7] = (byte)Message.SYSEX_END;
		serialPort.Write (message, 0, 8);
		if (pins [pin].currentMode != PinMode.SERVO)
			pinMode (pin, PinMode.SERVO);
	}
}		
// Sysex Queries are to be implemented later




