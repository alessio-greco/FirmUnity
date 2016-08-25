using System.Collections.Generic; // to use "List"
using UnityEngine;

public class Board{
	public int totalDigitalIO;
	public int totalDigitalPWM;
	public int totalAnalogIn;
	public int totalAnalogOut;
	public List<int> PWMEnabled=new List<int>();
	public string name;
	private static List<Board> boardList=new List<Board>();
	public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut, int[] PWMEnabled, string name){
		this.totalDigitalIO = totalDigitalIO; // total digital IO that can provide digital input/output
		this.totalDigitalPWM = totalDigitalPWM; // first n digital IO that can provide PWM output
		this.totalAnalogIn = totalAnalogIn; // read analog input, can be used as digital IO
		this.totalAnalogOut = totalAnalogOut; // provide analog DAC output. Currently unsupported via firmata
		if(PWMEnabled[0]!=-1) foreach(int i in PWMEnabled) this.PWMEnabled.Add(i);
		this.name = name;
		boardList.Add (this);
	}
	public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut):this(totalDigitalIO,totalDigitalPWM,totalAnalogIn,totalAnalogOut, new int[]{-1}, "No name was specified"){
	}
	public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut, int[] PWMEnabled):this(totalDigitalIO,totalDigitalPWM,totalAnalogIn,totalAnalogOut, PWMEnabled, "No name was specified"){
	}
	public Board(int totalDigitalIO, int totalDigitalPWM, int totalAnalogIn, int totalAnalogOut, string name):this(totalDigitalIO,totalDigitalPWM,totalAnalogIn,totalAnalogOut, new int[]{-1}, name){
	}
	public static string[] returnNameArray(){
		string[] names=new string[Board.boardList.Count];
		for (int i = 0; i < Board.boardList.Count; i++)
			names [i] = boardList [i].name;
		return names;
	}
	public static List<Board> returnList(){
		List<Board> list = new List<Board> (Board.boardList);
		return list;
	}

	public static Board getBoard(int index){
		return Board.boardList [index];
	}
}


public class ArduinoBoards{
	public static Board _101 = new Board(14,4,6,0,"Arduino 101");
	public static Board Gemma = new Board(3,2,1,0,"Arduino Gemma");
	public static Board LilyPad = new Board(14,6,6,0,"Arduino LilyPad");
	public static Board LilyPad_SimpleSnap = new Board(9,4,4,0,"Arduino LilyPad SimpleSnap");
	public static Board LilyPad_USB = new Board(9,4,4,0,"Arduino LilyPad USB");
	public static Board Mega_2560 = new Board(54,15,16,0,"Arduino Mega 2560");
	public static Board Micro = new Board(20,7,12,0,"Arduino Micro");
	public static Board MKR1000 = new Board(8,4,7,1,"Arduino MKR1000");
	public static Board Pro= new Board(14,6,6,0,"Arduino Pro");
	public static Board ProMini= new Board(14,6,6,0,"Arduino ProMini");
	public static Board Uno = new Board(14,6,6,0, new int[] {3,5,6,9,10,11,14,15,16,17,18,19},"Arduino Uno");
	public static Board Zero = new Board(14,10,6,1,"Arduino Zero");
	public static Board Due = new Board(54,12,12,1,"Arduino Due");
	public static Board BT = new Board(14,6,6,0,"Arduino BT");
	public static Board Ethernet = new Board(14,4,6,0,"Arduino Ethernet");
	public static Board Fio =new Board(14,6,8,0,"Arduino Fio");
	public static Board Leonardo = new Board(20,7,12,0,"Arduino Leonardo");
	public static Board Mega_ADK = new Board(54,15,16,0,"Arduino Mega ADK");
	public static Board Nano = new Board(14,6,8,0,"Arduino Nano");
	public static Board Mini = new Board(14,6,8,0,"Arduino Mini");
	public static Board Yùn = new Board(20,7,12,0,"Arduino Yùn");
}