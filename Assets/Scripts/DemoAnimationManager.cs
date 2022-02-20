// diff -u "D:/Unity Projects/visionlib-hololens/Assets/Scripts/Ex2AnimationManager.cs" "D:/Unity Projects/non-visionlib-holotest/Assets/Scripts/Ex2AnimationManager.cs"
using System;
using System.Collections.Generic;
using UnityEngine;
using Experiment2;
using Vuforia;

namespace Demo
{
    public class DemoAnimationManager : ViewManager
    {
        public GameObject fridgesEye;
        public GameObject fridgesMouse;
        public GameObject fridgesArm;
        public GameObject fridgesTear;
        public GameObject heartParameter;
        public GameObject fridgesBody;
        public GameObject cautionWindow;

        public GameObject SceneContents;
        public GameObject marker;

        private Animator anim;
        public Animator bodyAnim;
        public Material[] _EyeMaterial;
        public Material[] _BodyMaterial;

        public TextMesh kindBackground;
        public TextMesh valueBackground;

        public GameObject logOut;
        private LogOutput lo;

        private GameObject wssListener;
        private WebSocketServer wss;

        private GameObject faceTrackerListener;
        private DemoFaceTracking faceTracker;

        private GameObject hpmListener;
        private HeartParameterManager hpm;


        private System.Diagnostics.Stopwatch cautionStWa;
        private System.Diagnostics.Stopwatch animStWa; // アニメーションが変更されるまでの時間
        private System.Diagnostics.Stopwatch happyStWa; // アニメーションが変更されるまでの時間

        //public string message = " ";
        private string prevMessage = " ";

        private int prevAnimHash = 0;
        public int currentAnimId = 0;
        private int prevAnimId = 0;
        private int eyeTexture = 0;
        private int bodyTexture = 0;


        private bool currentConnVisibility = true;
        private bool badFridgeStatus = false;
        private Vector3 startPosition;
        private System.Random rand;
        public int intelligenceCondition = 0;

        private class ComponentNotFoundExeption : Exception
        {
            public ComponentNotFoundExeption(string message) : base(message) { }
        }

        // Start is called before the first frame update
        void Start()
        {
            lo = logOut.GetComponent<LogOutput>();

            wssListener = GameObject.Find("webSocket");
            wss = wssListener.GetComponent<WebSocketServer>();
            faceTrackerListener = GameObject.Find("face");
            faceTracker = faceTrackerListener.GetComponent<DemoFaceTracking>();
            hpmListener = GameObject.Find("heartParameterCanvas");
            hpm = hpmListener.GetComponent<HeartParameterManager>();

            cautionStWa = new System.Diagnostics.Stopwatch();
            animStWa = new System.Diagnostics.Stopwatch();
            happyStWa = new System.Diagnostics.Stopwatch();
            anim = GetComponent<Animator>();

            rand = new System.Random();

            changeStatus(1);
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyUp(KeyCode.Z) && !anim.IsInTransition(0)) changeStatus(1);
            else if (Input.GetKeyUp(KeyCode.X) && !anim.IsInTransition(0)) changeStatus(2);
            else if (Input.GetKeyUp(KeyCode.C) && !anim.IsInTransition(0)) changeStatus(1);
            else if (Input.GetKeyUp(KeyCode.V) && !anim.IsInTransition(0)) changeStatus(4);
            else if (Input.GetKeyUp(KeyCode.B) && !anim.IsInTransition(0)) changeStatus(5);
            else if (Input.GetKeyUp(KeyCode.N) && !anim.IsInTransition(0)) changeStatus(6);
            else if (Input.GetKeyUp(KeyCode.M) && !anim.IsInTransition(0)) changeStatus(7);
            else if (Input.GetKeyUp(KeyCode.Comma) && !anim.IsInTransition(0)) changeStatus(8);

            else if (Input.GetKeyUp(KeyCode.Alpha1) && !anim.IsInTransition(0)) changeStatus(11);
            else if (Input.GetKeyUp(KeyCode.Alpha2) && !anim.IsInTransition(0)) changeStatus(12);
            else if (Input.GetKeyUp(KeyCode.Alpha3) && !anim.IsInTransition(0)) changeStatus(13);
            else if (Input.GetKeyUp(KeyCode.Alpha4) && !anim.IsInTransition(0)) changeStatus(14);
            else if (Input.GetKeyUp(KeyCode.Alpha5) && !anim.IsInTransition(0)) changeStatus(15);
            else if (Input.GetKeyUp(KeyCode.Alpha6) && !anim.IsInTransition(0)) changeStatus(16);
            else if (Input.GetKeyUp(KeyCode.Alpha7) && !anim.IsInTransition(0)) changeStatus(1);
            else if (Input.GetKeyUp(KeyCode.Alpha0) && !anim.IsInTransition(0)) changeStatus(0);
            // else anim.SetInteger("animationID", 0);


            if (Input.GetKeyDown(KeyCode.P)) { pauseConnTracking(); }
            if (Input.GetKeyDown(KeyCode.R)) { resumeConnTracking(); }


            if (Input.GetKeyDown(KeyCode.H))
            { // hide face and body
                openDoor();
            }
            if (Input.GetKeyDown(KeyCode.J))
            {// show face and body
                closeDoor();
            }

            if (Input.GetKeyDown(KeyCode.S)) startExperiment();
            if (Input.GetKeyDown(KeyCode.F)) finishExperiment();
            if (Input.GetKeyDown(KeyCode.I)) changeConnVisibility();
            if (Input.GetKeyDown(KeyCode.L)) changeIntelligence();


            if (!String.Equals(this.prevMessage, this.message))
            {
                parseWSMessage(this.message);
                this.prevMessage = String.Copy(this.message);
            }


            float dx = fridgesBody.transform.position.x - Camera.main.transform.position.x;
            float dz = fridgesBody.transform.position.z - Camera.main.transform.position.z;
            double distance = Math.Sqrt(dx * dx + dz * dz);
            if (distance < 0.7f) // 1m以内に接近したら
            {
                openDoor();
            }
            else if (lo.experimentStatus == 1)
            {
                //NotificationHelper.SendInfo(distance.ToString());
                closeDoor();
            }

            dx = startPosition.x - Camera.main.transform.position.x;
            dz = startPosition.z - Camera.main.transform.position.z;
            distance = Math.Sqrt(dx * dx + dz * dz); // 初期位置から1メートルずれたとき


            // `animationが終了して変更されたタイミングでテクスチャを変更 --> ただのアニメ変更時
            int animState = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
            //if (animState != prevAnimHash) {

            

            // 一定時間（20秒）で喜び動作を終了
            if ((float)happyStWa.Elapsed.TotalSeconds > 20.0f)
            {
                happyStWa.Stop();
                happyStWa.Reset();
                changeStatus(0);
            }

        }


        public void changeIntelligence() // 見え方を実験条件に合わせて変更する
        {
            intelligenceCondition++;
            intelligenceCondition = intelligenceCondition % 4;

            // 面倒くさいから，一度全ての表示を消す
            fridgesTear.SetActive(false);
            heartParameter.SetActive(false);
            faceTracker.randomTracking = false;

            switch (intelligenceCondition)
            {
                case 0: //テキスト条件
                    fridgesBody.SetActive(false);
                    fridgesEye.SetActive(false);
                    fridgesMouse.SetActive(false);
                    fridgesArm.SetActive(false);
                    cautionWindow.SetActive(true);
                    
                    changeStatus(1);
                    break;

                case 1: //低知能条件
                    cautionWindow.SetActive(false);
                    fridgesBody.SetActive(true);
                    fridgesEye.SetActive(true);
                    fridgesMouse.SetActive(true);
                    fridgesArm.SetActive(true);

                    fridgesTear.SetActive(true);
                    changeStatus(0);
                    break;

                case 2: //アニマシー条件
                    changeStatus(0);
                    break;

                case 3: //知的条件
                    heartParameter.SetActive(true);
                    faceTracker.randomTracking = true;
                    hpm.heartParameter = 500;
                    hpm.currentHeartParameter = 500;
                    changeStatus(0);
                    break;
            }

        }


        // websocketからのメッセージを分析
        public void parseWSMessage(string message)
        {
            // NotificationHelper.SendInfo(message);
            if (message[0] == 'a')
            {
                switch (message[1])
                {
                    case 'z': changeStatus(1); break;
                    case 'x': changeStatus(2); break;
                    case 'c': changeStatus(1); break;
                    case 'v': changeStatus(4); break;
                    case 'b': changeStatus(5); break;
                    case 'n': changeStatus(6); break;
                    case 'm': changeStatus(7); break;
                    case '1': changeStatus(11); break;
                    case '2': changeStatus(12); break;
                    case '3': changeStatus(13); break;
                    case '4': changeStatus(14); break;
                    case '5': changeStatus(15); break;
                    case '6': changeStatus(16); break;
                    case '7': changeStatus(1); break;
                    case '0': changeStatus(0); break;
                    default: break;
                }
            }
            else
            {
                switch (message[0])
                {
                    case 'p': // pause
                        pauseConnTracking();
                        break;
                    case 'r': // resume
                        resumeConnTracking();
                        break;

                    case 'h': // hide face and body = ドアが空いているとき
                        openDoor();
                        break;

                    case 'j': // J(s)how face and body
                        closeDoor();
                        break;

                    case 's': // start experiment
                        startExperiment();
                        break;

                    case 'f': // finish experiment
                        finishExperiment();
                        break;

                    case 'i': // show / hide the connector model
                        changeConnVisibility();
                        break;

                    case 'l': // intelligence controll
                        changeIntelligence();
                        break;
                }
            }
        }


        private void resumeConnTracking()
        {
            SceneContents.SetActive(false);

            marker.SetActive(true);
            VuforiaBehaviour.Instance.enabled = true;
        }
        private void pauseConnTracking()
        {
            SceneContents.SetActive(true);
            
            SceneContents.transform.rotation = marker.transform.rotation;
            SceneContents.transform.position = marker.transform.position;

            VuforiaBehaviour.Instance.enabled = false;
            marker.SetActive(false);

            currentConnVisibility = false;
#if WINDOWS_UWP
                wss.OnConnectA();
#endif
            lo.trackingStatus = 1;

            //var parent = this.transform;
            //Vector3 offset = new Vector3((float)0.1, (float)0.1, (float)0.1);

        }


        private void openDoor()
        {
            cautionWindow.SetActive(false);
            fridgesEye.SetActive(false);
            fridgesMouse.SetActive(false);
            fridgesArm.SetActive(false);
            fridgesBody.SetActive(false);

            fridgesTear.SetActive(false);
            heartParameter.SetActive(false);

            if (lo.doorStatus == 0)
            {
                lo.doorStatus = 1;

                string outMessage = "5,1";
                lo.eventLogWrite(outMessage);
            }
        }
        private void closeDoor()
        {
            if(intelligenceCondition == 0)
                cautionWindow.SetActive(true);
            else
            {
                fridgesEye.SetActive(true);
                fridgesMouse.SetActive(true);
                fridgesArm.SetActive(true);
                fridgesBody.SetActive(true);
            }

            if (intelligenceCondition == 1 && badFridgeStatus) fridgesTear.SetActive(true);
            else if (intelligenceCondition == 3) heartParameter.SetActive(true);

            lo.doorStatus = 0;
        }


        private void startExperiment()
        {
            string outMessage = "0,1";
            lo.experimentStatus = 1;
            closeDoor();

            animStWa.Reset();
            animStWa.Start();

            //lo.eventLogWrite(outMessage);
            //lo.enableOutput = true;

            startPosition = Camera.main.transform.position;
            intelligenceCondition=-1;
            changeIntelligence(); // 見え方を実験条件に合わせて変更する

        }
        private void finishExperiment()
        {
            string outMessage = "0,2";
            lo.experimentStatus = 2;
            //lo.eventLogWrite(outMessage);
            lo.enableOutput = false;

            wss.message = "finish experiment!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!";

        }

        private void changeConnVisibility()
        {
            if (currentConnVisibility == true)
            {
                fridgesBody.SetActive(false);
                fridgesBody.GetComponent<Renderer>().material = _BodyMaterial[bodyTexture];
                currentConnVisibility = false;
            }
            else
            {
                fridgesBody.SetActive(true);
                fridgesBody.GetComponent<Renderer>().material = _BodyMaterial[0];
                currentConnVisibility = true;
            }
        }

        private void changeStatus(int id)
        {
            if (intelligenceCondition == 0)
            {
                changeCautions(id);
            }
            else changeAnimation(id);
        }

        private void changeAnimation(int id)
        {
            happyStWa.Stop();
            happyStWa.Reset();

            if (id == 1)
            {
                int preasureAnim = rand.Next(3); //0~2
                switch (preasureAnim)
                {
                    case 0: id = 1; break;
                    case 1: id = 3; break;
                    case 2: id = 17; break;
                }

                happyStWa.Reset();
                happyStWa.Start();
            }
            if (id == 5) { // 太り動作も20秒にする
                happyStWa.Reset();
                happyStWa.Start();
            }

            // 実質的な冷蔵庫の状態はここで決定する
            currentAnimId = id;
            badFridgeStatus = !(id == 0 || id == 1 || id == 3 || id == 5 || id == 17);

            // idを18に変更する前にテクスチャの変更
            if (currentAnimId != prevAnimId)
            {
                Transform tra = fridgesMouse.transform;
                Vector3 pos = tra.localPosition;
                eyeTexture = 0;
                bodyTexture = 0;


                switch (currentAnimId)
                {
                    case 0: eyeTexture = 0; bodyTexture = 0; break;
                    case 1: eyeTexture = 1; bodyTexture = 0; break;
                    case 2: eyeTexture = 3; bodyTexture = 1; break;
                    case 3: eyeTexture = 1; bodyTexture = 0; break;
                    case 4: eyeTexture = 2; bodyTexture = 2; break;
                    case 5: eyeTexture = 0; bodyTexture = 0; break;
                    case 6: eyeTexture = 3; bodyTexture = 0; break;
                    case 7: eyeTexture = 3; bodyTexture = 3; break;
                    case 8: eyeTexture = 4; bodyTexture = 0; break;
                    case 11: eyeTexture = 4; bodyTexture = 4; break;
                    case 12: eyeTexture = 4; bodyTexture = 4; break;
                    case 13: eyeTexture = 4; bodyTexture = 4; break;
                    case 14: eyeTexture = 4; bodyTexture = 5; break;
                    case 15: eyeTexture = 4; bodyTexture = 4; break;
                    case 16: eyeTexture = 4; bodyTexture = 4; break;
                    case 17: eyeTexture = 1; bodyTexture = 0; break; // happy3
                    default: eyeTexture = 0; bodyTexture = 0; break;
                }

                // 快不快条件の時の目は固定表現
                if (intelligenceCondition == 1 && badFridgeStatus)
                {
                    eyeTexture = 3;
                }

                //if (currentAnimId == 5) pos.z = 0.08f;
                //else pos.z = 0.05f;

                fridgesBody.GetComponent<Renderer>().material = _BodyMaterial[bodyTexture];
                fridgesEye.GetComponent<Renderer>().material = _EyeMaterial[eyeTexture];
                // prevAnimHash = animState;
                prevAnimId = currentAnimId;
            }




            if (id == 5) bodyAnim.SetInteger("bodyID", 5);
            else if (id == 6) bodyAnim.SetInteger("bodyID", 6);
            else bodyAnim.SetInteger("bodyID", 0);


            // 快不快条件のときに涙のパーティクルを可視化
            if (intelligenceCondition == 1 && badFridgeStatus)
            {
                fridgesTear.SetActive(true);
                id = 18;
            }
            else
            {
                fridgesTear.SetActive(false);
            }

            // 知性条件において友好度の可視化を適当に
            if (intelligenceCondition == 3)
            {
                if (!badFridgeStatus && id!=0) hpm.heartParameter += 100;
                else hpm.heartParameter -= 25;
            }

            // 実際に冷蔵庫のアニメーションを適用
            anim.SetInteger("animationID", id);


            if (lo.currentFridgeStatus != currentAnimId)
            {
                animStWa.Stop(); // 時間計測

                lo.currentFridgeStatus = currentAnimId;
                string outMessage = "1," + animStWa.Elapsed.ToString();
                //lo.eventLogWrite(outMessage);

                animStWa.Reset();
                animStWa.Start();
            }

            wss.message = currentAnimId.ToString();
            //showNotify("" + id);
        }

        private void changeCautions(int id)
        {
            bool changeFlag = true;

            happyStWa.Stop();
            happyStWa.Reset();

            if (id == 1)
            {
                int preasureAnim = rand.Next(3); //0~2
                switch (preasureAnim)
                {
                    case 0: id = 1; break;
                    case 1: id = 3; break;
                    case 2: id = 17; break;
                }

                happyStWa.Reset();
                happyStWa.Start();
            }

            if (id == 5)
            { // 容量が十分な場合も20秒表示
                happyStWa.Reset();
                happyStWa.Start();
            }


            // なにもないときは非表示
            if (id != 0)
            {
                cautionWindow.SetActive(true);
            }



            switch (id)
            {
                case 0:
                    kindBackground.text = ""; valueBackground.text = "";
                    cautionWindow.SetActive(false);
                    break;
                case 1:
                    kindBackground.text = "状態が良くなりました"; valueBackground.text = "";
                    break;
                case 2:
                    kindBackground.text = "庫内が汚れています"; valueBackground.text = "";
                    break;
                case 3:
                    // kindBackground.text = "庫内が綺麗に\nなりました"; valueBackground.text = "";
                    kindBackground.text = "解決しました"; valueBackground.text = "";
                    break;
                case 4:
                    kindBackground.text = "扉が開きっぱなしに\nなっています"; valueBackground.text = "";
                    break;
                case 5:
                    kindBackground.text = "食材が十分に\nあります"; valueBackground.text = "";
                    break;
                case 6:
                    kindBackground.text = "食材が少ないです"; valueBackground.text = "";
                    break;
                case 7:
                    kindBackground.text = "庫内の温度が\n高すぎます"; valueBackground.text = "";
                    break;
                case 8:
                    kindBackground.text = "庫内の温度が\n低すぎます"; valueBackground.text = "";
                    break;
                case 11:
                    kindBackground.text = "下から1段目にある\n食材が期限切れです"; valueBackground.text = "";
                    break;
                case 12:
                    kindBackground.text = "下から2段目にある\n食材が期限切れです"; valueBackground.text = "";
                    break;
                case 13:
                    kindBackground.text = "下から3段目にある\n食材が期限切れです"; valueBackground.text = "";
                    break;
                case 14:
                    kindBackground.text = "下段にある食材が\n賞味期限切れです"; valueBackground.text = "";
                    //kindBackground.text = "下から4段目にある\n食材が期限切れです"; valueBackground.text = "";
                    break;
                case 15:
                    kindBackground.text = "上から2段目にある\n食材が期限切れです"; valueBackground.text = "";
                    break;
                case 16:
                    kindBackground.text = "上から1段目にある\n食材が期限切れです"; valueBackground.text = "";
                    break;
                case 17:
                    kindBackground.text = "ありがとうございます"; valueBackground.text = "";
                    break;
                default:
                    changeFlag = false;
                    break;
            }

            if (changeFlag == true)
            {
                wss.message = id.ToString();

                if (lo.currentFridgeStatus != id)
                {
                    cautionStWa.Stop();

                    lo.currentFridgeStatus = id;
                    string outMessage = "1," + cautionStWa.Elapsed.ToString();
                    //lo.eventLogWrite(outMessage);

                    cautionStWa.Reset();
                    cautionStWa.Start();
                }

            }

        }


        private T FindComponent<T>() where T : Component
        {
            T component = FindObjectOfType<T>();

            if (component == null)
            {
                throw new ComponentNotFoundExeption(typeof(T).ToString());
            }
            return component;
        }
    }
}
