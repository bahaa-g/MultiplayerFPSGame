using UnityEngine;
using UnityEngine.EventSystems;


public class FixedButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [HideInInspector]
    public static bool Pressed;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
    }
    //public static FixedButton instance;

    //void Awake()
    //{
    //    instance = this;
    //}
}