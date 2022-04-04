using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//using Photon.Pun;

namespace Com.LakeWalk.MadGuns
{
    public class ADS : MonoBehaviour, IPointerDownHandler
    {
        //[HideInInspector]
        public static bool aim = false;
        public static bool reload;
        //public float swayReduction;
        //public GameObject ADSHolder;
        //public static ADS instance;

        //   void Awake()
        //{
        //       instance = this;
        //}

        //public void AimDownSight()
        //{
        //    aim = !aim;
        //    FireAndRecoil.scope = aim;
        //    RigidbodyFirstPersonController.aiming = aim;
        //    //ADSHolder.SetActive(aim);
        //}
        
        public void OnPointerDown(PointerEventData eventData)
        {
            aim = !aim;
            FireAndRecoil.scope = aim;
            RigidbodyFirstPersonController.aiming = aim;
            if (aim)
            {
                RigidbodyFirstPersonController.swayReduction = 0.1f;
                Debug.Log("Aim");
            }
            else
                RigidbodyFirstPersonController.swayReduction = 1;

        }

        //public void AimDownSight(GameObject scope)
        //{
        //    aim = !aim;
        //    scope.SetActive(aim);
        //}
        public void ReloadButton()
        {
            FireAndRecoil.reload = true;

        }

    }
}