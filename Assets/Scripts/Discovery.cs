using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;


    public class Discovery : MonoBehaviour
    {

    public static Dictionary<string, GameObject> bulbMap = new Dictionary<string, GameObject>();


        const int DISCOVERY_PORT = 48899; //Port that we listen on
        byte[] MAGIC_UDP_PACKET = Encoding.ASCII.GetBytes("HF-A11ASSISTHREAD"); //Encode magic UDP packet


      
        private CancellationTokenSource m_cancelScanSource; //cancelation token to cancel our scan method

        public async Task Scan(int millisecondsTimeout = 4000, int maxRetries = 5)
        {

             UdpClient discovery_client = new UdpClient(); //Create UDP Client for discovery broadcast
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);



            int scanRetryCount = 0;

            //repeat the scan if we didn't hear back from any bulbs, but only if scanRetryCount doesn't exceed 'maxRetries'
            while (scanRetryCount <= maxRetries)
            {

                m_cancelScanSource = new CancellationTokenSource(millisecondsTimeout); //creae the cancellation token         

                Console.WriteLine("sending magic packet");
                discovery_client.Send(MAGIC_UDP_PACKET, MAGIC_UDP_PACKET.Length, ip); //Send magic packet to get controllers to announce themselves

                //Listen for their return packets
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, DISCOVERY_PORT);

               
                    //Hack in a way to allow a CancellationToken for ReceiveAsync
                    //Based heavily on https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
                    var receive_task = discovery_client.ReceiveAsync();
                    var tcs = new TaskCompletionSource<bool>();

                    using (m_cancelScanSource.Token.Register(s => tcs.TrySetResult(true), null))
                    {
                        if (await Task.WhenAny(receive_task, tcs.Task) == receive_task) //if the cancellation token isn't true continue, else break the loop
                        {
                            Console.WriteLine("receiving message");
                            string message = Encoding.ASCII.GetString(receive_task.Result.Buffer); //ReceiveAsync was successful, encode the reply into ASCII and parse
                            Console.WriteLine(message);
                            string[] bulb_data = message.Split(',');

                            string ipAddress = bulb_data[0];
                            string macAddress = bulb_data[1];
                            string typeID = bulb_data[2];


                        CreateBulbInstance(ipAddress, macAddress, typeID);
                
                        Debug.Log(ipAddress);
                 
                    }
                        else //Cancelled (or timed out), close out socket
                        {
                       
                   

                            discovery_client.Close();
                            m_cancelScanSource.Dispose();
                            m_cancelScanSource = null;
                        scanRetryCount++;

                    }

                    
                }
            
        }
        
        }

    public static void CreateBulbInstance(string ipAddress, string macAddress, string typeID)
    {
        Vector3 vector3;
        Quaternion q = new Quaternion(0f, 0f, 0f, 0f);

        Bulb bulb;
        GameObject gameObject;

        if (macAddress.Contains("DC4F22E1") || macAddress.Contains("5CCF7FE18"))
        {
            vector3 = new Vector3(5f, -2f);
            gameObject = GameObject.Instantiate(Resources.Load("RGBWWBulb"), vector3, q) as GameObject;
            bulb = gameObject.GetComponent<Bulb>();
            bulb.InitializeBulb(ipAddress, macAddress, typeID, Bulb.BulbType.RGBWWBulb);

        }
        else if (macAddress.Contains("6001940ED"))
        {
            vector3 = new Vector3(-10f, -2f);
             gameObject = GameObject.Instantiate(Resources.Load("RGBWWStrip"), vector3, q) as GameObject;
            bulb = gameObject.GetComponent<Bulb>();
            bulb.InitializeBulb(ipAddress, macAddress, typeID, Bulb.BulbType.RGBWWStrip);
        }
        else if (macAddress.Contains("DC4F22") || macAddress.Contains("600194"))
        {
            vector3 = new Vector3(3f, -2f);
             gameObject = GameObject.Instantiate(Resources.Load("RGBWBulb"), vector3, q) as GameObject;
            bulb = gameObject.GetComponent<Bulb>();
            bulb.InitializeBulb(ipAddress, macAddress, typeID, Bulb.BulbType.RGBWBulb);
        }
        else if (typeID.Contains("ZJ2101"))
        {
            vector3 = new Vector3(10f, -2f, 0);

             gameObject = GameObject.Instantiate(Resources.Load("RGBWWBulb"), vector3, q) as GameObject;
            bulb = gameObject.GetComponent<Bulb>();
            bulb.InitializeBulb(ipAddress, macAddress, typeID, Bulb.BulbType.RGBWWBulb);
        }
        else
        {
            gameObject = null;
        }

       
        bulbMap.Add(macAddress, gameObject);


        }

}

