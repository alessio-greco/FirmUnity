using UnityEngine;
using System.Collections.Generic;

// numbers used in setPinMode

public enum PinMode{
	INPUT = 0,  // Normal digital/analog input behaviour
	OUTPUT = 1, // Normal digital/analog output behaviour
	ANALOG = 2, // Analog input
	PWM = 3, // PWM output
	SERVO = 4, // Servo control
	I2C = 6, // I2C
	ONEWIRE = 7,
	STEPPER = 8,
	ENCODER = 9,
	SERIAL = 10,
	INPUT_PULLUP = 11 // Inverted Input Behaviour
}


// pin informations, helper and such

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
	public List<PinMode> pinModesCompatible= new List<PinMode>();
	public PinMode currentMode;
	public int value; // represent current value of the pin
	public bool keyUp; // is true during the frame next to the release
	public bool keyDown; // is true during the frame where it is pressed
	public bool reporting; // is it reporting?
	public bool analogReporting;
	public int analogBits, servoBits, PWMBits, i2cBits;
	public int minPulse, maxPulse;
	public PinInfo(){
		currentMode = PinMode.INPUT;
	}
	public bool isPinModeSupported(PinMode mode){
		return pinModesCompatible.Contains (mode);
	}
	public void setCompatibilities(List<PinMode> pinModesCompatible, int analogBits, int PWMBits, int servoBits, int i2cBits){
		this.pinModesCompatible=pinModesCompatible;
		this.analogBits = analogBits;
		this.servoBits = servoBits;
		this.PWMBits = PWMBits;
		this.i2cBits = i2cBits;
		minPulse = 0;
		maxPulse = (int)Mathf.Pow (2, 7) - 1;
	}

}