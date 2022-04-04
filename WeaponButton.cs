using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponButton : MonoBehaviour, IPointerUpHandler
{
    public int weaponIndex = 0;
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnBUTTON!!");
    }
}
