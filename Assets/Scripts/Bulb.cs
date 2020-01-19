using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;



public class Bulb : MonoBehaviour
{
    Bulb bulb;
    UdpClient discovery_client = new UdpClient();
    void Start()
{
   

}
    public string displayIPAddress;

    Socket _socket;
    int DefaultPort = 5577;


    public string IpAddress
    {
        get;
        set;
    }
    public string MacAddress
    {
        get;
        set;
    }
    public string TypeID
    {
        get;
        set;
    }

    BulbColor bulbColor;

    public bool IsOn
    {
        get;
        set;
    }

    public bool IsRGBWW
    {
        get;
        set;
    }

    public bool IsPersistant
    {
        get;
        set;
    }

    public enum BulbType
    {
        RGBWWStrip,
        RGBWWBulb,
        RGBWBulb
    }

     byte[] powerOn = {
   0x71,
   0x23,
   0x0f
  };

    byte[] powerOff = {
   0x71,
   0x24,
   0x0f
  };

    BulbType bulbType;

    public void InitializeBulb(string ipAddress, string macAddress, string bulbID, BulbType typeOfBulb)
    {
                IPEndPoint _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), DefaultPort);
        _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        _socket.ReceiveTimeout = 1;
        _socket.SendTimeout = 1;
        _socket.Connect(_endPoint);

        IpAddress = ipAddress;
        MacAddress = macAddress;
        TypeID = bulbID;
        bulbType = typeOfBulb;
        displayIPAddress = IpAddress;
        GetState();
    }

    public async Task GetState()
{
    try
    {
      
        byte[] getStatusMessage = new byte[] {
     0x81,
     0x8A,
     0x8B,
     0x96
    };

        byte[] response = Transmit.SendMessage(IpAddress, getStatusMessage, false, true);


        byte persistance = response[0];
        byte bulbType = response[1];
        byte powerState = response[2];
        byte mode = response[3];
        byte presetDelay = response[5];
        byte red = response[6];
        byte green = response[7];
        byte blue = response[8];
        byte warmWhite = response[9];
        byte versionNumber = response[10];
        byte coldWhite = response[11];

        bulbColor= new BulbColor(red, green, blue, warmWhite, coldWhite);
        bool isOn = (powerState == 0x24 ? true : false);
        bool isRGBWW = (bulbType == 0x35 ? true : false);
        bool isPersistant = (persistance == 0x31 ? true : false);

        IsOn = isOn;
        IsRGBWW = isRGBWW;
        IsPersistant = isPersistant;
    }
    catch
    {
      
    }
}

public void TurnOn()
{
    IsOn = true;
    UpdateStatePower();

}

public void TurnOff()
{
    IsOn = false;
    UpdateStatePower();
}

public void SetColorLevel(byte r = 0, byte g = 0, byte b = 0)
{
        bulbColor.Red = r;
        bulbColor.Green = g;
        bulbColor.Blue = b;
    UpdateStateColor();
}

public virtual void SetWarmWhiteLevel(byte w)
{
    bulbColor.WarmWhite = w;
    UpdateStateWhite();
}

public virtual void SetColdWhiteLevel(byte c)
{
        bulbColor.ColdWhite = c;
    if(bulbType == BulbType.RGBWBulb)
            bulbColor.WarmWhite = c;
    UpdateStateWhite();
}

public virtual void SetBothWhiteLevel(byte w, byte c)
{
        bulbColor.WarmWhite = w;
        bulbColor.ColdWhite = c;
    UpdateStateWhite();
}

public virtual void SetColorAndWhiteLevel(byte r, byte g, byte b, byte w, byte c)
{
        bulbColor.Red = r;
        bulbColor.Green = g;
        bulbColor.Blue = b;
        bulbColor.WarmWhite = w;
        bulbColor.ColdWhite = c;
        if (bulbType == BulbType.RGBWBulb || bulbType == BulbType.RGBWWBulb)
        {
            if ((r > 0 || g > 0 || b > 0))
                UpdateStateColor();
            else
                UpdateStateWhite();
        }
        UpdateStateColorAndWhite();
}

private void UpdateStatePower()
{
    CreateBasicMessage((IsOn == true ? powerOn : powerOff));
}

private void UpdateStateColor()
{
    CreateColorMessage(0xF0); // sets only color channels to change
}
private void UpdateStateWhite()
{
    CreateColorMessage(0x0F); // sets only white channels to change
}
private void UpdateStateColorAndWhite()
{
    CreateColorMessage(0xFF);
}

private void CreateColorMessage(byte mask)
{
    byte[] sendMessageByte;
        if (bulbType == BulbType.RGBWBulb)
        {
            sendMessageByte = new byte[] {
    0x31,
    bulbColor.Red,
    bulbColor.Green,
    bulbColor.Blue, //4 Blue byte
    bulbColor.WarmWhite, //5 WarmWhite byte
    mask, //7
    0x0F //8 terminator (I'll be back)
   };
        } else
        {
            sendMessageByte = new byte[] {
    0x31,
    bulbColor.Red,
    bulbColor.Green,
    bulbColor.Blue, //4 Blue byte
    bulbColor.WarmWhite, //5 WarmWhite byte
    bulbColor.ColdWhite, //6 ColdWhite byte
    mask, //7
    0x0F //8 terminator (I'll be back)
   };
        }
    CreateBasicMessage(sendMessageByte);
}


    public void SendColorMessage(byte red, byte green, byte blue, byte warmWhite, byte coldWhite)
    {
        byte mask;
        if (red > 0 || green > 0 || blue > 0)
            mask = 0xF0;
        else
            mask = 0x0F;

        byte[] sendMessageByte;
        if (bulbType == BulbType.RGBWBulb)
        {
            sendMessageByte = new byte[] {
    0x31,
    bulbColor.Red,
    bulbColor.Green,
    bulbColor.Blue, //4 Blue byte
    bulbColor.WarmWhite, //5 WarmWhite byte
    mask, //7
    0x0F //8 terminator (I'll be back)
   };
        }
        else
        {
            sendMessageByte = new byte[] {
    0x31,
    bulbColor.Red,
    bulbColor.Green,
    bulbColor.Blue, //4 Blue byte
    bulbColor.WarmWhite, //5 WarmWhite byte
    bulbColor.ColdWhite, //6 ColdWhite byte
    mask, //7
    0x0F //8 terminator (I'll be back)
   };
        }
        CreateBasicMessage(sendMessageByte);
    }

    public void CreateBasicMessage(byte[] message)
{

    try
    {
        SendMessage(IpAddress, message, true);
    }
    catch (Exception e)
    {
        Console.WriteLine(e + " exception found for ip address: " + IpAddress);
    }

}

public byte CalculateChecksum(byte[] bytes)
{
    byte checksum = 0;

    foreach (var b in bytes)
    {
        unchecked
        {
            checksum += b;
        }
    }

    return checksum;
}

public void SendMessage(string ipAddress, byte[] bytes, bool sendChecksum)
{


    if (sendChecksum)
    {
        var checksum = CalculateChecksum(bytes);
        Array.Resize(ref bytes, bytes.Length + 1);
        bytes[bytes.Length - 1] = checksum;
    }

    _socket.Send(bytes);


}
    public BulbColor GetBulbColor()
    {
        BulbColor bulbColors = this.bulbColor;
        return bulbColors;
    }
    public struct BulbColor
    {

        public BulbColor(byte red = 0, byte green = 0, byte blue = 0, byte warmWhite = 0, byte coldWhite = 0)
        {
            Red = red;
            Green = green;
            Blue = blue;
            WarmWhite = warmWhite;
            ColdWhite = coldWhite;

        }

        public BulbColor(BulbColor bulbColor)
        {
            Red = bulbColor.Red;
            Green = bulbColor.Green;
            Blue = bulbColor.Blue;
            WarmWhite = bulbColor.WarmWhite;
            ColdWhite = bulbColor.ColdWhite;
        }


        public byte Red
        {
            get;
            set;
        }
        public byte Green
        {
            get;
            set;
        }
        public byte Blue
        {
            get;
            set;
        }
        public byte WarmWhite
        {
            get;
            set;
        }
        public byte ColdWhite
        {
            get;
            set;
        }

    }

}
