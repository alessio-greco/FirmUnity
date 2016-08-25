using UnityEngine;
// Various type of Constants based on firmata protocols

// Messages

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

// numbers used in setPinMode

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

// possible values for digital pins

public enum Value{
	LOW = 0,
	HIGH =1
}


// standards for ports

public enum StandardBaudRate{
	baud110 = 110,
	baud300 = 300,
	baud600 = 600,
	baud1200 = 1200,
	baud2400 = 2400,
	baud4800 = 4800,
	baud9600 = 9600,
	baud14400 = 14400,
	baud19200 = 19200,
	baud28800 = 28800,
	baud38400 = 38400,
	baud57600 = 57600,
	baud115200 = 115200,
	baud230400 = 230400
}

public class PortSelection{
	public const string AUTO = "Automatic Selection of Serial Port, Select the first Serial port";
	public const string COM = "Serial port, Sketch to upload is 'StandardFirmata'";
	public const string BLE = "Bluetooth Low Energy, unimplemented, use 'StandardFirmataBLE'";
	public const string WIFI = "WiFi, unimplemented, use 'StandardFirmataWiFi'";
	public const string ETHERNET = "Ethernet, unimplemented<, use 'StandardFirmataEthernet'";
	public const string PLUS = "Serial Port but with additional features for more powerful boards, unimplemented, use 'StandardFirmataPlus'";
	public const string CHIPKIT = "Serial Port but using ChipKITs, unimplemented, use 'StandardFirmataChipKIT'";
}

// pin informations, helper and such

public enum PinType{
	DIGITAL = 0, // Digital IO
	PWM = 1, // Digital PWM + Digital IO. PWM Pins can send PWM
	ANALOG = 2, // Analog In + Digital PWM + Digital IO
	DAC = 3 // Anolog Out(DAC) + Digital IO
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