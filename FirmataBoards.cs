using System.Collections.Generic; // to use "List"

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
		if(PWMEnabled[0]!=-1) foreach(int i in PWMEnabled) this.PWMEnabled.Add(i);
	}
	public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut):this(totalDigitalIO,totalDigitalPWM,totalAnalogIn,totalAnalogOut, new int[]{-1}){
	}
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