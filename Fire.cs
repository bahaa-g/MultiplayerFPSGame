using UnityEngine;
using UnityEngine.EventSystems;

namespace Com.LakeWalk.MadGuns
{
    public class Fire : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        //public static Fire instance;

        //[HideInInspector]
        //public static bool Pressed = false;
        public static bool notFixed;

        void Awake()
        {
            //instance = this;
            notFixed = (PlayerPrefs.GetInt("FireFixed", 1) == 1) ? true : false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //Pressed = true;
            FireAndRecoil.startFire = true;

			//if (notFixed)
   //             FixedTouchField.AimWithButton(eventData.pointerId);
            
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //Pressed = false;
            FireAndRecoil.startFire = false;
            //if (notFixed)
            //    FixedTouchField.firePressed = false;

        }
        
    }
}