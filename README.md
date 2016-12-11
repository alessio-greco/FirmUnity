# FirmUnity

FirmUnity is a library project used to permit the interaction between Firmata-compatible peripheals, like Arduino and Spark, and Unity.
Since it use the information contained on the [protocol](https://github.com/firmata/protocol) it should pretty much be compatible with most, if not all, of the peripheals able to comunicate with Firmata.

## Usage

You need to upload one the StandardFirmata sketches, or implement a custom sketch to your Firmata-Compatible board, like "StandardFirmata" for the ArduinoUno, depending on your needs.
Firmata/Arduino has these sketches.
* StandardFirmata 	
  * Uses the Serial Port to comunicate.  
*StandardFirmataBLE
  * Uses the BLE to comunicate.   	
*StandardFirmataChipKIT 
  * Uses the Serial Port to comunicate, for use with ChipKIT expansion kits.  
*StandardFirmataEthernet 	
  * Uses the Ethernet to comunicate.  
*StandardFirmataPlus 
  * Uses the Serial Port to comunicate, has more 
*StandardFirmataWiFi
  * Uses the WiFi to comunicate.
As of now, the library ony works with the Serial Port.

On Unity-side, you need to copy this folder to your library, and add one "FirmataBridge" component to one of the gameObjects.
Then, you can assign the firmata bridge instance (in future it may change, permitting multiple instances using a dictionary of '<string,FirmataBridge>', with the string set from the editor)

'public static FirmataBridge firmata;
firmata = FirmataBridge.instance;' 

You can choose the name you want, obviously. After that open the bridge

'firmata.Open ();'

Then set the pin modes, waiting for the bridge to be opened with

'do {
	yield return new WaitForEndOfFrame();
} while(!firmata.IsReady ());''

And set the pins with the usual manner

'firmata.pinMode (movementPin, PinMode.INPUT);

After that, you may use your board freely

## In construction