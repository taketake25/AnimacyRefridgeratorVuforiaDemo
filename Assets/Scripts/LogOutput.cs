using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if WINDOWS_UWP
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
using System.Threading.Tasks;
#endif


public class LogOutput : MonoBehaviour
    {


#if WINDOWS_UWP
    public Windows.Storage.StorageFolder folder;
    public Windows.Storage.StorageFile gazeLogFile;
    public Windows.Storage.StorageFile eventLogFile;
    public Windows.Storage.StorageFile imuLogFile;
    public Windows.Storage.StorageFile meshLogFile;
    public Windows.Storage.StorageFile posLogFile;
#endif
    public string buf = "";
    private int gazeIndex = 1;
    private int eventIndex = 1;
    private int imuIndex = 1;
    private int meshIndex = 1;
    private int posIndex = 1;
    public int participantNumber = 0;
    public bool enableOutput = false;

    public int experimentStatus = 0;
    public int trackingStatus = 0;
    public int currentFridgeStatus = 0;
    public int doorStatus = 0;
    public int leaveFromChair = 0;

    GameObject actionListener;
    private WebSocketServer wss;

    private void Start()
    {
        actionListener = GameObject.Find("webSocket");
        wss = actionListener.GetComponent<WebSocketServer>();
        string wsip = "null";
        int tlen = 0;

#if WINDOWS_UWP
        Task.Run(async ()=>
        {
            string dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            this.folder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("ExprimentLog", 
                                                                CreationCollisionOption.OpenIfExists);

            //dt = "0";
            this.eventLogFile = await folder.CreateFileAsync("eventLog-" + participantNumber.ToString() + "-" + dt + ".csv", CreationCollisionOption.ReplaceExisting);
            this.gazeLogFile = await folder.CreateFileAsync("gazeLog-" + participantNumber.ToString() + "-" + dt + ".csv", CreationCollisionOption.ReplaceExisting);
            this.imuLogFile= await folder.CreateFileAsync("imuLog-" + participantNumber.ToString() + "-" + dt + ".csv", CreationCollisionOption.ReplaceExisting);
            this.meshLogFile= await folder.CreateFileAsync("meshLog-" + dt + ".csv", CreationCollisionOption.ReplaceExisting);
            this.posLogFile= await folder.CreateFileAsync("positionLog-" + participantNumber.ToString() + "-" + dt + ".csv", CreationCollisionOption.ReplaceExisting);
        
            using (var stream = await gazeLogFile.OpenStreamForWriteAsync())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(@"index,time,event,x,y");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            using (var stream = await eventLogFile.OpenStreamForWriteAsync())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(@"index,time,experiment,tracking,fridge,door,event,value1");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            using (var stream = await imuLogFile.OpenStreamForWriteAsync())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(@"index,time,accX,accY,accZ,gyrX,gyrY,gyrZ,magX,magY,magZ");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            using (var stream = await meshLogFile.OpenStreamForWriteAsync())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(@"index,1x,1y,1z,2x,2y,2z,3x,3y,3z");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            using (var stream = await posLogFile.OpenStreamForWriteAsync())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(@"index,time,X,Y,Z");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }


            logWriter(gazeLogFile, "\n", 0);
            logWriter(eventLogFile, "\n", 0);
            logWriter(imuLogFile, "\n", 0);
            logWriter(meshLogFile, "\n", 0);
            logWriter(posLogFile, "\n", 0);
        });

        // Websocket先のIPアドレスが書かれたファイルを参照
        Task.Run(async ()=>
        {
            Windows.Storage.StorageFolder configFolder =
                await KnownFolders.DocumentsLibrary.CreateFolderAsync("config", CreationCollisionOption.OpenIfExists);
            Windows.Storage.StorageFile wsipFile =
                await configFolder.GetFileAsync("wsip.txt");

            wsip = await Windows.Storage.FileIO.ReadTextAsync(wsipFile);
            tlen = wsip.Length;
            wss.IPaddrB = wsip;
        });
#endif
    }


    public void gazeLogWrite(string text)
    {
        if (!enableOutput) return;

        var t = DateTime.Now;
        string tinfo = t.ToString("yyyyMMddHHmmss") + t.Millisecond.ToString();
#if WINDOWS_UWP
        //NotificationHelper.SendInfo("gaze:" + text);
        try{
            logWriter(gazeLogFile, gazeIndex.ToString() + "," + tinfo + "," + text + "\n", 1);
        }catch (System.IO.IOException ex){
            Debug.LogError(">> IOException");
        }
#endif
    }

    public void eventLogWrite(string text)
    {

        var t = DateTime.Now;
        string tinfo = t.ToString("yyyyMMddHHmmss") + t.Millisecond.ToString();
        tinfo = tinfo + "," + experimentStatus.ToString() + "," + trackingStatus.ToString() + "," + currentFridgeStatus.ToString() + "," + doorStatus.ToString();
        wss.message = text;
#if WINDOWS_UWP
        try{
            logWriter(eventLogFile, eventIndex.ToString() + "," + tinfo + "," + text + "\n", 2);
            // eventIndex++;
        }catch (System.IO.IOException ex ){
            Debug.LogError(">> IOException");
        }
#endif
    }
    public void imuLogWrite(string text)
    {

        var t = DateTime.Now;
        string tinfo = t.ToString("yyyyMMddHHmmss") + t.Millisecond.ToString();
        tinfo = tinfo + "," + experimentStatus.ToString() + "," + trackingStatus.ToString() + "," + currentFridgeStatus.ToString() + "," + doorStatus.ToString();
#if WINDOWS_UWP
        try{
            logWriter(imuLogFile, imuIndex.ToString() + "," + tinfo + "," + text + "\n", 3);
        }catch (System.IO.IOException ex ){
            Debug.LogError(">> IOException");
        }
#endif
    }
    public void meshLogWrite(string text)
    {
#if WINDOWS_UWP
        try{
            logWriter(meshLogFile, meshIndex.ToString() + "," + text + "\n", 5);
        }catch (System.IO.IOException ex ){
            Debug.LogError(">> IOException");
        }
#endif
    }
    public void posLogWrite(string text)
    {

        var t = DateTime.Now;
        string tinfo = t.ToString("yyyyMMddHHmmss") + t.Millisecond.ToString();
        tinfo = tinfo + "," + experimentStatus.ToString() + "," + trackingStatus.ToString() + "," + currentFridgeStatus.ToString() + "," + doorStatus.ToString();
#if WINDOWS_UWP
        try{
            logWriter(posLogFile, posIndex.ToString() + "," + tinfo + "," + text + "\n", 4);
        }catch (System.IO.IOException ex ){
            Debug.LogError(">> IOException");
        }
#endif
    }


#if WINDOWS_UWP
    public void logWriter(Windows.Storage.StorageFile file, string text, int kind)

    {

        Task.Run(async ()=>
        {
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

            using (var outputStream = stream.GetOutputStreamAt(stream.Size))
            {
                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    dataWriter.WriteString(text);
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();

                    if(kind==1){
                        gazeIndex++;
                    }else if(kind==2){
                        eventIndex++;
                    }else if(kind==3){
                        imuIndex++;
                    }else if(kind==4){
                        posIndex++;
                    }else if(kind==5){
                        meshIndex++;
                    }
                }
            }
            stream.Dispose();

        });


    }
#endif

}
