using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public class DemoFaceTracking : MonoBehaviour
    {
        private GameObject faceListener;
        private GameObject bodyListener;
        private GameObject cameraListener;
        Vector3 direc;
        double currentYaw = 180;
        double currentPitch = 0;
        const float neckSpeed = 1.0f;
        private float f1value = 0.1f;
        private int count = 0;

        //public LineRenderer line1;
        //public LineRenderer line2;
        //public GameObject targetposListener;

        GameObject actionListener;
        DemoAnimationManager ma;
        private Animator anim;


        public bool randomTracking = false;

        // Start is called before the first frame update
        void Start()
        {
            faceListener = GameObject.Find("face");
            bodyListener = GameObject.Find("body");
            //cameraListener = GameObject.Find("Camera");

            actionListener = GameObject.Find("deforming_refrigerator_cat_limbs");
            ma = actionListener.GetComponent<DemoAnimationManager>();
            anim = actionListener.GetComponent<Animator>();

        }



        void Update()
        {
            if (randomTracking) // intelligence=2のときだけランダムに首振り
            {
                Transform ft = faceListener.transform;
                Transform ct = Camera.main.transform;
                direc.Set(ct.position.x - ft.position.x, ct.position.y - ft.position.y, ct.position.z - ft.position.z);
                count++;
                int id = ma.currentAnimId;

                // 間欠カオス法による1/fゆらぎの算出
                if (count == 100)
                {
                    count = 0;

                    if (f1value < 0.5) f1value = f1value + 2 * f1value * f1value;
                    else f1value = f1value - 2 * (1.0f - f1value) * (1.0f - f1value);

                    if (f1value < 0.1) f1value = UnityEngine.Random.Range(0.2f, 0.1f);
                    if (f1value > 0.9) f1value = UnityEngine.Random.Range(0.8f, 0.9f);
                    //Debug.Log(f1value);
                }


                Quaternion targetRotataion;
                // 首振りのタイミング
                if ((Input.GetKey(KeyCode.K) || f1value > 0.5) && id!=0)
                {
                    if (!(id == 1 || id == 3 || id == 17 || id == 5))
                    { //喜び, 太り動作のときは動作を変えない
                        anim.SetInteger("animationID", 0);
                    }

                    var lookAtRotation = Quaternion.LookRotation(direc, Vector3.up);
                    // 回転補正
                    var offsetRotation = Quaternion.FromToRotation(new Vector3(0, -1, 0), Vector3.forward);
                    // ゆっくり回転
                    targetRotataion = lookAtRotation * offsetRotation;
                }
                else
                {
                    anim.SetInteger("animationID", ma.currentAnimId);
                    targetRotataion = bodyListener.transform.rotation;
                }
                ft.rotation = Quaternion.Slerp(ft.rotation, targetRotataion, 0.1f);
            }
            else // intelligence=2以外の時は正面を向く
            {
                Transform ft = faceListener.transform;
                Vector3 front = bodyListener.transform.localEulerAngles;
                ft.rotation = bodyListener.transform.rotation;
            }
        }
    }
}