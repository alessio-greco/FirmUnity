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
		private Board board = ArduinoBoards.Uno; // Use "ArduinoBoards.Name" for Arduino boards, or create a new board specifying it's IO/PWM

		//Constructors

		public FirmataBridge (string portName, Board board, int baudRate, bool autoStart, int delay, bool reportAll, int readTimeout){
			int totalDigitalPins = board.totalDigitalIO + board.totalAnalogIn;
			int totalDigitalPorts = (totalDigitalPins - totalDigitalPins % 8) / 8 + 1;
			for (int i = 0; i < totalDigitalPins; i++) {
				if (i < board.totalDigitalPWM)
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
			if (!isOpen) {
				Thread.Sleep (delay);
				Open();
			}
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

		// Reading Thread

		private void ReadBridge (){
			
		}
		private void SendI (){

		}
	}

	public class PinInfo{
		PinType pinType;
		PinMode pinMode;
		int value; // represent current value of the pin
		bool keyUp; // is true during the frame next to the release
		bool keyDown; // is true during the frame where it is pressed
		public PinInfo(PinType type){
			pinType = type;
			pinMode = PinMode.OFF;
		}
	}
	public class Board{
		public int totalDigitalIO;
		public int totalDigitalPWM;
		public int totalAnalogIn;
		public int totalAnalogOut;
		public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut){
			this.totalDigitalIO = totalDigitalIO; // total digital IO that can provide digital input/output
			this.totalDigitalPWM = totalDigitalPWM; // first n digital IO that can provide PWM output
			this.totalAnalogIn = totalAnalogIn; // read analog input, can be used as digital IO
			this.totalAnalogOut = totalAnalogOut; // provide analog DAC output. Currently unsupported via firmata
		}
	}
	public enum PinMode{
		OFF = -1,
		INPUT = 0,  // Normal digital input behaviour
		OUTPUT = 1, // Normal digital output behaviour
		ANALOG = 2,
		PWM = 3,
		SERVO = 4,
		I2C = 6,
		ONEWIRE = 7,
		STEPPER = 8,
		ENCODER = 9,
		SERIAL = 10,
		PULLUP = 11 // Inverted Input Behaviour
	}
	public enum PinType{
		DIGITAL = 0, // Digital IO
		PWM = 1, // Digital PWM + Digital IO
		ANALOG = 2, // Analog In + Digital IO
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
	public enum Pin{
		PIN_0 = 0x01,
		PIN_1 = 0x02,
		PIN_2 = 0x04,
		PIN_3 = 0x08,
		PIN_4 = 0x10,
		PIN_5 = 0x20,
		PIN_6 = 0x40,
	}
	public enum Port{
		PIN_0_TO_7 = 0x00,
		PIN_8_TO_15 = 0x01,
		PIN_16_TO_23 = 0x02,
		PIN_24_TO_31 = 0x03,
		PIN_32_TO_39 = 0x04,
		PIN_40_TO_47 = 0x05,
		PIN_48_TO_55 = 0x06,
		PIN_56_TO_63 = 0x07,
		PIN_64_TO_71 = 0x08,
		PIN_72_TO_79 = 0x09,
		PIN_80_TO_71 = 0x0A,
		PIN_88_TO_79 = 0x0B,
		PIN_96_TO_103 = 0x0C,
		PIN_104_TO_111 = 0x0D,
		PIN_112_TO_119 = 0x0E,
		PIN_120_TO_127 = 0x0F,
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
		public static Board Uno = new Board(14,6,6,0);
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