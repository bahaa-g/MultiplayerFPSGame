using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Com.LakeWalk.MadGuns
{
    public class WeaponSelect : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        //public UnityEvent onHold;
        //public UnityEvent onMouseUp;
        //public static WeaponSelect instance;
        public GameObject weaponList;

        public static int weaponIndex = 0;
        //public static bool switchWeapon;// when this is true the FireAndRecoil Script will switch to the weapnIndedx
        private int PointerId;

        //   private void Awake()
        //{
        //       instance = this;
        //}

        public void OnPointerDown(PointerEventData eventData)
        {
            //onHold.Invoke();
            weaponList.SetActive(true);
            PointerId = eventData.pointerId;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            IsMouseOverUIWithIgnores();

            weaponList.SetActive(false);
        }

        public bool IsMouseOverUIWithIgnores()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.touches[PointerId].position;//Input.mousePosition;
            //pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResultList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
            for (int i = 0; i < raycastResultList.Count; i++)
            {
                if (raycastResultList[i].gameObject.GetComponent<WeaponButton>() != null)
                {
                    WeaponButton weaponButton = raycastResultList[i].gameObject.GetComponent<WeaponButton>();
                    weaponIndex = weaponButton.weaponIndex;
                    FireAndRecoil.switchWeapon = true;
                }
            }
            return raycastResultList.Count > 0;
        }

        private bool IsMouseOverUI()// Not used at the moment
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        public void SwitchIndex()
        {

        }
    }

}