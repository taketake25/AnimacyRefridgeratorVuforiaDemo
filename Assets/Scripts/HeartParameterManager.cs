using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Experiment2
{

    public class HeartParameterManager : MonoBehaviour
    {
        private GameObject innerListener;
        private Material waveMaterial;
        //private GameObject cameraListener;

        public int currentHeartParameter = 0;
        public int heartParameter = 0;
        private bool grow = true;
        private float f1value = 0.1f;

        // Start is called before the first frame update
        void Start()
        {
            innerListener = GameObject.Find("innerHeart");
            waveMaterial = innerListener.GetComponent<Image>().material;

        }

        // Update is called once per frame
        void Update()
        {

            /*
            if (grow == true)
            {
                heartParameter = 1000;
                if (currentHeartParameter == heartParameter)
                {
                    heartParameter = 0;
                    grow = false;
                }
            }
            if (grow == false)
            {
                heartParameter = 0;
                if (currentHeartParameter == heartParameter)
                {
                    heartParameter = 1000;
                    grow = true;
                }
            }*/

            if (heartParameter - currentHeartParameter > 0) currentHeartParameter += 1;
            else currentHeartParameter -= 1;


            //Debug.Log(currentHeartParameter);

            waveMaterial.SetFloat("_FillAmount", (float)(currentHeartParameter / 1000.0f));
        }
    }
}
