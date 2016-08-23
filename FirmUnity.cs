using UnityEngine; // To be able to use waituntilframe and such
using System.IO.Ports; // Serial port communication
using System.Collections.Generic; // to use "Dictionary" class
using System.Threading; // sleep and "reading thread"

namespace FirmUnity{
	public class FirmataBridge {

		//Variables 

		private Dictionary<int,PinInfo> pins = new Dictionary<int,PinInfo> (); // pins infos. Digital pin are from 0 to ..., pin Analog n  corresponds to -(n+1)
		// so Analog 0 -> Pin -1, Analog 2 -> pin -2 and such
		private bool reportAll; // if true, report channels based on 
		private SerialPort serialPort;
		private bool isOpen;
		private Thread readingThread = new Thread (ReadInputs ());
		private Thread delayedOpenThread;
		private Board board; // Use "ArduinoBoards.Name" for Arduino boards, or create a new board specifying it's IO/PWM

		//Constructors

		public FirmataBridge (string portName, Board board, int baudRate, bool autoStart, int delay, bool reportAll, int readTimeout){
			int totalDigitalPins = board.totalDigitalIO + board.totalAnalogIn;
			int totalDigitalPorts = (totalDigitalPins - totalDigitalPins % 8) / 8 + 1;
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
			serialPort = new SerialPort (portName, baudRate);
			serialPort.ReadTimeout=readTimeout;
			isOpen = false;
			if (autoStart)
				Open (delay);
		}
		public FirmataBridge (string portName, Board board, int baudRate, int readTimeout):this(portName, ArduinoBoards.Uno, baudRate, false, 0, true, readTimeout){
		}
		public FirmataBridge (string portName, int baudRate, bool autoStart, int delay, bool reportAll, int readTimeout):this(portName, ArduinoBoards.Uno, baudRate, autoStart, delay, true, 10){
		}
		public FirmataBridge (string portName, int baudRate, bool reportAll, int readTimeout):this(portName, ArduinoBoards.Uno, baudRate, false, 0, reportAll, readTimeout){
		}
		public FirmataBridge (string portName, int baudRate, bool autoStart, int delay, int ReadTimeout):this(portName, ArduinoBoards.Uno, baudRate, autoStart, delay, true, ReadTimeout){
		}
		public FirmataBridge (string portName, int baudRate, bool autoStart):this(portName, ArduinoBoards.Uno, baudRate, autoStart, 500, true, 10){
		}
		public FirmataBridge (string portName, int baudRate, int readTimeout):this(portName, ArduinoBoards.Uno, baudRate, false, 0, true, readTimeout){
		}
		public FirmataBridge (string portName, bool autoStart, int delay):this(portName, ArduinoBoards.Uno, 57600, autoStart, delay, true, 10){
		}
		public FirmataBridge (string portName, bool autoStart):this(portName, ArduinoBoards.Uno, 57600, autoStart, 500, true, 10){
		}
		public FirmataBridge (string portName, int baudRate):this(portName, ArduinoBoards.Uno, baudRate, false, 0, true, 10){
		}
		public FirmataBridge (string portName):this(portName, ArduinoBoards.Uno, 57600, false, 0, true, 10){
		}
		public FirmataBridge (string portName, Board board, int baudRate, bool reportAll, int readTimeout):this(portName, board, baudRate, false, 0, reportAll, readTimeout){
		}
		public FirmataBridge (string portName, Board board, int baudRate, bool autoStart, int delay, int readTimeout):this(portName, board, baudRate, autoStart, delay, true, readTimeout){
		}
		public FirmataBridge (string portName, Board board, int baudRate, bool autoStart):this(portName, board, baudRate, autoStart, 500, true, 10){
		}
		public FirmataBridge (string portName, Board board, int baudRate, int readTimeout):this(portName, board, baudRate, false, 0, true, readTimeout){
		}
		public FirmataBridge (string portName, Board board, bool autoStart, int delay):this(portName, board, 57600, autoStart, delay, true, 10){
		}
		public FirmataBridge (string portName, Board board, bool autoStart):this(portName, board, 57600, autoStart, 500, true, 10){
		}
		public FirmataBridge (string portName, Board board, int baudRate):this(portName, board, baudRate, false, 0, true, 10){
		}
		public FirmataBridge (string portName, Board board):this(portName, board, 57600, false, 0, true, 10){
		}
			
		// Utility Methods

		public bool IsOpen (){
			return isOpen;
		}

		// Open Methods

		public void Open(int delay){
			delayedOpenThread = DelayedOpen (delay);
			delayedOpenThread.Start ();
		}
		public void Open (){
			if (!isOpen) {
				try {
					serialPort.Open ();
				}catch{
					Debug.Log ("errors while opening the serial port!");
				}
				readingThread.Start ();
				isOpen = true;
			}
		}
		public void DelayedOpen(int delay){
			Thread.Sleep (delay);
			Open ();
		}
		// Reading Thread

		private void ReadBridge (){
			string protocol_Version="";
			int totalDigitalPins = board.totalDigitalIO + board.totalAnalogIn;
			int totalDigitalPorts = (totalDigitalPins - totalDigitalPins % 8) / 8 + 1;
			byte[] message = new byte[128]; // most instructions use only the first 3 
			int readed=0;
			//  if reportAll = true, enable report for all pins by default, else, pin are to be enabled when "pinMode" is used on them
			message [1] = 1;
			if (reportAll) {
				for (int i = 0; i < totalDigitalPorts; i++) {
					message [0] = (byte)Message.REPORT_DIGITAL_PORT | i;
					serialPort.Write (message, 0, 2);
				}
				for (int i = 0; i < board.totalAnalogIn+board.totalAnalogOut; i++) {
					message [0] = (byte)Message.REPORT_ANALOG_PIN | i;
					serialPort.Write (message, 0, 2);
				}
			}
			do{
				if((serialPort.BytesToRead!=-1)&&(serialPort.IsOpen())){
					readed=serialPort.ReadByte;
					if((readed&Message.ANALOG_MESSAGE==Message.ANALOG_MESSAGE)){
						// 0xE1 & 0xE0 -> 0xE0
						int pin = readed - Message.ANALOG_MESSAGE;
						int value = serialPort.ReadByte();
						value+=serialPort.ReadByte()*Mathf.Pow(2,7);
						ChangePinState(pin,value);
					}
					if((readed&Message.DIGITAL_MESSAGE==Message.DIGITAL_MESSAGE)){
						// 0x91 & 0x90 -> 0xE0
						int port = readed - Message.DIGITAL_MESSAGE;
						int max = Mathf.Min(port*8-1, totalDigitalPins);
						readed=serialPort.ReadByte();
						for (int i=port*8 ; i<max; i++){
							int value = readed&PinHelper.getPinMask(i%8);
							if(value!=0) value=1;
							ChangePinState(i,value);
						}
						readed=serialPort.ReadByte();
						if((port+1)*8< totalDigitalPins){
							int value = readed&PinHelper.getPinMask(7);
							if(value!=0) value=1;
							ChangePinState(port*8+8, value);

						}

					}
					if(readed==Message.START_SYSEX){
						// SYSEX Management on hold
						while(serialPort.BytesToRead=!Message.SYSEX_END) serialPort.ReadByte();
					}
					if(readed==Message.PROTOCOL_VERSION){
						protocol_Version="";
						protocol_Version+=serialPort.ReadByte()+"."+serialPort.ReadByte();
						Debug.Log("Protocol Version "+protocol_Version);
					}
					// SET_DIGITAL_PIN_MODE, SET_DIGITAL_PIN_VALUE, REPORT_ANALOG_PIN, REPORT_DIGITAL_PORT are only sent to and never received from the board, SYSEX_END is only at the end of SYSEXs
				}

				WaitForEndOfFrame();
			}
			while(true);
			
		}

		private void ChangePinState(int pin, int value){
			if(pins[pin].value==0){
				pins[pin].value=value;
				if (value == 1)
					pins [pin].keyDown = true;
				else
					pins [pin].keyUp = false;
			}
			else{
				pins[i].value=value;
				if (value == 0)
					pins [pin].keyUp = true;
				else
					pins [pin].keyDown = false;
			}
		}

		// Digital I/O
		public void pinMode(int pin, PinMode mode){
			byte message = new byte[3];
			pins [pin].pinMode = mode;
			message [0] = (byte)Message.SET_DIGITAL_PIN_MODE;
			message [1] = (byte)pin;
			message [2] = (byte)mode; 
			serialPort.Write (message, 0, 3);
		}
		public void digitalWrite(int pin, Value state){
			byte message = new byte[3];
			pins [pin].value = state;
			message [0] = (byte)Message.SET_DIGITAL_PIN_VALUE;
			message [1] = (byte)pin;
			message [2] = (byte)state; 
			serialPort.Write (message, 0, 3);
		}
		public int digitalRead(int pin){
			return pins [pin].value;
		}
	}
		
	public class PinInfo{
		PinType pinType;
		public PinMode pinMode;
		public int value; // represent current value of the pin
		public bool keyUp; // is true during the frame next to the release
		public bool keyDown; // is true during the frame where it is pressed
		public PinInfo(PinType type){
			pinType = type;
			pinMode = PinMode.INPUT;
		}
	}
	public class Board{
		public int totalDigitalIO;
		public int totalDigitalPWM;
		public int totalAnalogIn;
		public int totalAnalogOut;
		public List<int> PWMEnabled=new List<int>();
		public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut, int[] PWMEnabled){
			this.totalDigitalIO = totalDigitalIO; // total digital IO that can provide digital input/output
			this.totalDigitalPWM = totalDigitalPWM; // first n digital IO that can provide PWM output
			this.totalAnalogIn = totalAnalogIn; // read analog input, can be used as digital IO
			this.totalAnalogOut = totalAnalogOut; // provide analog DAC output. Currently unsupported via firmata
			foreach(int i in PWMEnabled) this.PWMEnabled.Add(i);
		}
		public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut):this(totalDigitalIO,totalDigitalPWM,totalAnalogIn,totalAnalogOut, null){
		}
	}
	public enum PinMode{
		INPUT = 0,  // Normal digital/analog input behaviour
		OUTPUT = 1, // Normal digital/analog output behaviour
		ANALOG = 2,
		PWM = 3,
		SERVO = 4,
		I2C = 6,
		ONEWIRE = 7,
		STEPPER = 8,
		ENCODER = 9,
		SERIAL = 10,
		INPUT_PULLUP = 11 // Inverted Input Behaviour
	}
	public enum PinType{
		DIGITAL = 0, // Digital IO
		PWM = 1, // Digital PWM + Digital IO. PWM Pins can send PWM
		ANALOG = 2, // Analog In + Digital PWM + Digital IO
		DAC = 3 // Anolog Out(DAC) + Digital IO
	}
	public enum Value{
		LOW = 0,
		HIGH =1
	}
	public enum Message{
		ANALOG_MESSAGE=0xE0, // Send/Receive an Analog Message to/from Analog PIN#. OR with PIN#. first byte = LSB, second byte = MSB
		DIGITAL_MESSAGE=0x90, // Send/Receive a Digital Message to/from Digital PORT#. OR with PORT#. first byte represent Pins 8#(assigned to bit 0) to 8#+6(assigned to bit 6). Second byte bit 0 is bit 8#+7 state
		REPORT_ANALOG_PIN=0xC0, // Enable/Disable report from PIN #. OR with PIN#
		REPORT_DIGITAL_PORT=0xD0, // Enable/Disable report from PORT #. 
		START_SYSEX=0xF0,
		SET_DIGITAL_PIN_MODE=0xF4,
		SET_DIGITAL_PIN_VALUE=0xF5,
		SYSEX_END=0xF7,
		PROTOCOL_VERSION=0xF9
	}
	public static class PinHelper{
		public static int getPinMask(int pin){
			if (pin == 0)
				return 0x01;
			if (pin == 1)
				return 0x02;
			if (pin == 2)
				return 0x04;
			if (pin == 3)
				return 0x08;
			if (pin == 4)
				return 0x10;
			if (pin == 5)
				return 0x20;
			if (pin == 6)
				return 0x40;
			if (pin == 7)
				return 0x01; // pin 7 is on the second byte!
			return -1;
		} 
	}

	// Sysex Queries are to be implemented later

	public enum SysexQuery{
		SERIAL_MESSAGE = 0x60, // communicate with serial devices, including other boards
		ENCODER_DATA = 0x61, // reply with encoders current positions
		ANALOG_MAPPING_QUERY = 0x69, // ask for mapping of analog to pin numbers
		ANALOG_MAPPING_RESPONSE = 0x6A, // reply with mapping info
		CAPABILITY_QUERY = 0x6B, // ask for supported modes and resolution of all pins
		CAPABILITY_RESPONSE = 0x6C, // reply with supported modes and resolution
		PIN_STATE_QUERY = 0x6D, // ask for a pin's current mode and state (different than value)
		PIN_STATE_RESPONSE = 0x6E, // reply with a pin's current mode and state (different than value)
		EXTENDED_ANALOG = 0x6F, // analog write (PWM, Servo, etc) to any pin
		SERVO_CONFIG = 0x70, // pin number and min and max pulse
		STRING_DATA = 0x71, // a string message with 14-bits per char
		STEPPER_DATA = 0x72, // control a stepper motor
		ONEWIRE_DATA = 0x73, // send an OneWire read/write/reset/select/skip/search request
		SHIFT_DATA = 0x75, // shiftOut config/data message (reserved - not yet implemented)
		I2C_REQUEST = 0x76, // I2C request messages from a host to an I/O board
		I2C_REPLY = 0x77, // I2C reply messages from an I/O board to a host
		I2C_CONFIG = 0x78, // Enable I2C and provide any configuration settings
		REPORT_FIRMWARE = 0x79, // report name and version of the firmware
		SAMPLEING_INTERVAL = 0x7A, // the interval at which analog input is sampled (default = 19ms)
		SCHEDULER_DATA = 0x7B, // send a createtask/deletetask/addtotask/schedule/querytasks/querytask request to the scheduler
		SYSEX_NON_REALTIME = 0x7E, // MIDI Reserved for non-realtime messages
		SYSEX_REALTIME = 0x7F // MIDI Reserved for realtime messages

	}
	public class ArduinoBoards{
		public static Board _101 = new Board(14,4,6,0);
		public static Board Gemma = new Board(3,2,1,0);
		public static Board LilyPad = new Board(14,6,6,0);
		public static Board LilyPad_SimpleSnap = new Board(9,4,4,0);
		public static Board LilyPad_USB = new Board(9,4,4,0);
		public static Board Mega_2560 = new Board(54,15,16,0);
		public static Board Micro = new Board(20,7,12,0);
		public static Board MKR1000 = new Board(8,4,7,1);
		public static Board Pro= new Board(14,6,6,0);
		public static Board ProMini= new Board(14,6,6,0);
		public static Board Uno = new Board(14,6,6,0, new int[] {3,5,6,9,10,11,14,15,16,17,18,19});
		public static Board Zero = new Board(14,10,6,1);
		public static Board Due = new Board(54,12,12,1);
		public static Board BT = new Board(14,6,6,0);
		public static Board Ethernet = new Board(14,4,6,0);
		public static Board Fio =new Board(14,6,8,0);
		public static Board Leonardo = new Board(20,7,12,0);
		public static Board Mega_ADK = new Board(54,15,16,0);
		public static Board Nano = new Board(14,6,8,0);
		public static Board Mini = new Board(14,6,8,0);
		public static Board Yùn = new Board(20,7,12,0);
	} 
}