using UnityEngine;
using UnityEngine.EventSystems;

public class RotateWeaponView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    [HideInInspector]
    public Vector2 TouchDist;
    [HideInInspector]
    public Vector2 PointerOld;
    [HideInInspector]
    public int PointerId;
    [HideInInspector]
    public bool Pressed = true;
    private int touchIndex;
    Touch currentTouch;
    public float XSensitivity = 1f;
    public float YSensitivity = 1f;

    public void Start()
    {
        Pressed = false;
    }

    void Update()
    {
        if (Pressed)
        {
            if (PointerId >= 0 && PointerId < Input.touches.Length)
            {
                TouchDist = Input.touches[PointerId].position - PointerOld;
                TouchDist.x *= XSensitivity;
                TouchDist.y *= YSensitivity;
                PointerOld = Input.touches[PointerId].position;
            }
            else
            {
                TouchDist = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - PointerOld;
                TouchDist.x *= XSensitivity;
                TouchDist.y *= YSensitivity;
                PointerOld = Input.mousePosition;
            }
        }
        else
        {
            TouchDist = new Vector2();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        PointerId = eventData.pointerId;
        PointerOld = eventData.position;
        Pressed = true;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
    }
}

