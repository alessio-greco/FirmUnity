using System.Collections.Generic; // to use "List"
using UnityEngine;

public class Board{
	public int totalPins;
	public string name;
	private static List<Board> boardList=new List<Board>();
	public Board(int totalPins, string name){
		this.totalPins = totalPins;
		this.name = name;
		boardList.Add (this);
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
	public static Board _101 = new Board(20,"Arduino 101");
	public static Board Gemma = new Board(4,"Arduino Gemma");
	public static Board LilyPad = new Board(20,"Arduino LilyPad");
	public static Board LilyPad_SimpleSnap = new Board(13,"Arduino LilyPad SimpleSnap");
	public static Board LilyPad_USB = new Board(13,"Arduino LilyPad USB");
	public static Board Mega_2560 = new Board(70,"Arduino Mega 2560");
	public static Board Micro = new Board(32,"Arduino Micro");
	public static Board MKR1000 = new Board(16,"Arduino MKR1000");
	public static Board Pro= new Board(20,"Arduino Pro");
	public static Board ProMini= new Board(20,"Arduino ProMini");
	public static Board Uno = new Board(20,"Arduino Uno");
	public static Board Zero = new Board(21,"Arduino Zero");
	public static Board Due = new Board(67,"Arduino Due");
	public static Board BT = new Board(20,"Arduino BT");
	public static Board Ethernet = new Board(20,"Arduino Ethernet");
	public static Board Fio =new Board(22,"Arduino Fio");
	public static Board Leonardo = new Board(32,"Arduino Leonardo");
	public static Board Mega_ADK = new Board(70,"Arduino Mega ADK");
	public static Board Nano = new Board(22,"Arduino Nano");
	public static Board Mini = new Board(22,"Arduino Mini");
	public static Board Yùn = new Board(32,"Arduino Yùn");
}