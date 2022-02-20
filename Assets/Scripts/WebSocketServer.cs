//using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.HoloLens;
using Experiment2;

#if WINDOWS_UWP
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.Storage.Streams;
#endif



public class WebSocketServer : MonoBehaviour
{

#if WINDOWS_UWP
    private MessageWebSocket messageWebSocket;
#endif

    public GameObject actionListener;
    ViewManager ma;

    public string message = " ";
    private string prevMessage = " ";

    public string IPaddrA = "";
    public string IPaddrB = "";


    void Start()
    {
        DateTime dt = DateTime.Now;
        ma = actionListener.GetComponent<ViewManager>();

        // ma.message = "az";

        /*#if WINDOWS_UWP
            Task.Run(async ()=>
            {
                Windows.Storage.StorageFolder configFolder =
                    await KnownFolders.DocumentsLibrary.CreateFolderAsync("config", CreationCollisionOption.OpenIfExists);
                Windows.Storage.StorageFile wsipFile =
                    await configFolder.GetFileAsync("wsip.txt");

                string wsip = await Windows.Storage.FileIO.ReadTextAsync(wsipFile);
                IPaddrB = wsip;
                int tlen = wsip.Length;
                // NotificationHelper.SendInfo("wsip: " + wsip);
                // NotificationHelper.SendInfo("wsip.Length: " +tlen.ToString());
             });
        #endif*/

    }

#if WINDOWS_UWP
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            try
            {
                Task.Run(async () => {
                    await WebSock_SendMessage(messageWebSocket, "Hi!!");
                });
            }
            catch (Exception ex)
            {
                Debug.Log("error : " + ex.ToString());
            }
            NotificationHelper.SendInfo("send Hi! through websocket");
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            // 研究用PCにWebSocket接続開始
            OnConnectA();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            // ノートPCにWebSocket接続開始
            OnConnectB();
        }

        if (!String.Equals(this.prevMessage,this.message))
        {
            WebSock_SendMessage(messageWebSocket, this.message);
            this.prevMessage = String.Copy(this.message);
        }
    }

  

    public void OnConnectA(){
        //OnConnect("ws://163.221.174.210:1234");
        OnConnect(IPaddrA);
    }
    public void OnConnectB(){
        OnConnect(IPaddrB);
        NotificationHelper.SendInfo("IP: " + IPaddrB);

    }

    public void OnConnect(string ipAddr)
    {
  
        Debug.Log("OnConnect");
          
        messageWebSocket = new MessageWebSocket();
 
        messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
        messageWebSocket.MessageReceived += WebSocket_MessageReceived;
        messageWebSocket.Closed += WebSock_Closed;
  
        Uri serverUri = new Uri(ipAddr); // 別PCのWebSocket serverにつながる
        NotificationHelper.SendInfo(ipAddr);
  
        try
        {
            Task.Run(async () =>
            {
                await messageWebSocket.ConnectAsync(serverUri);
 
                Debug.Log("Connect to the server...." + serverUri.ToString());
                Debug.Log("ConnectAsync OK");
  
                await WebSock_SendMessage(messageWebSocket, "Connect Start");
                NotificationHelper.SendInfo("success onConnect");
            });
        }
        catch (Exception ex) // For debugging
            {
                // Error happened during connect operation.
                messageWebSocket.Dispose();
                messageWebSocket = null;

                NotificationHelper.SendInfo("error : " + ex.ToString());
                Debug.Log("error : " + ex.ToString());

                return;
            }

    }
      
    private async Task WebSock_SendMessage(MessageWebSocket webSock, string message)
    {
        DataWriter messageWriter = new DataWriter(webSock.OutputStream);
        messageWriter.WriteString(message);
        await messageWriter.StoreAsync();
        messageWriter.DetachStream(); // ストリームの破棄。今回加えたら何度も送られるようになった。加えないと一度だけしか送られない。 
    }
  



    private void WebSocket_MessageReceived(Windows.Networking.Sockets.MessageWebSocket sender, Windows.Networking.Sockets.MessageWebSocketMessageReceivedEventArgs args)
    {

        try
        {
            using (DataReader dataReader = args.GetDataReader())
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                string message = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                // Debug.Log("Message received from MessageWebSocket: " + message);
                // this.messageWebSocket.Dispose();
                ma.message = message;
            }
        }
        catch (Exception ex)
        {
            Windows.Web.WebErrorStatus webErrorStatus = Windows.Networking.Sockets.WebSocketError.GetStatus(ex.GetBaseException().HResult);
            // Debug.Log("Error: " + ex.ToString());
            // Add additional code here to handle exceptions.
        }
    }


    private void WebSock_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
    {
        // WebSock_Closed
    }
  
#endif

}