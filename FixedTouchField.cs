using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FixedTouchField : MonoBehaviour//, IPointerDownHandler, IPointerUpHandler
{
    //public static FixedTouchField instance;
    public static Vector2 TouchDist;
    [HideInInspector]
    public Vector2 PointerOld;
    [HideInInspector]
    public int PointerId;
    public static int buttonPointerId;
    public static bool Pressed = false;
    public static bool firePressed = false;
    private int touchIndex;
    Touch currentTouch;
    public static float XSensitivity = 1f;
    public static float YSensitivity = 1f;
    private bool searchForTouch = true;
    //public TextMeshProUGUI startTouch;
    //public TextMeshProUGUI pressedText;
    //public TextMeshProUGUI touchPosition;
    //public TextMeshProUGUI numberOfTouchesText;
    //public TextMeshProUGUI touchcount;
    //public TextMeshProUGUI touchDistance;
    //public TextMeshProUGUI fingerId;
    public static float scopeSensitivity;// if there is no change in scope sens then it is equal to 1
    private static float sensitivityReduction = 1;
    //public float noScopeSense = 1f;// when the scope isn't active
    //private bool found = false;
    private int indexOfTouch = 0;

    private int touchCount;

    void Start()
    {
        searchForTouch = true;
        XSensitivity = PlayerPrefs.GetFloat("SensitivityX", 0.8f);
        YSensitivity = PlayerPrefs.GetFloat("SensitivityY", 0.5f);
        scopeSensitivity = PlayerPrefs.GetFloat("ScopeSensitivity", 1f);
        touchCount = 0;
    }
    void Update()
    {
        if (Input.touches.Length < touchCount)
        {
            if (PointerId > 0)
            {
                PointerId--;
            }
        }
        touchCount = Input.touches.Length;
        for (int i = 0; i < Input.touches.Length; i++)
        {
            //Touch touch = Input.touches[i];
            Touch touch = Input.touches[i];
            if (touch.phase == TouchPhase.Began)
            {
                //if (touch.position.x > 664f && touch.position.y < 700f)
                //{
                    if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        //PointerId = touch.fingerId;
                        PointerId = i;
                        Pressed = true;
                    }
                    else
                    {
                        if (IgnoreButton(touch.fingerId))
                        {
                            buttonPointerId = touch.fingerId;
                            firePressed = true;
                        }
                    }
                //}
            }
        }

        //if (EventSystem.current.IsPointerOverGameObject())
        //{
        //    Debug.Log("Touched the UI");
        //}

        if (Pressed)
        {
            if (PointerId >= 0 && PointerId < Input.touches.Length)
            {
                TouchDist = Input.touches[PointerId].deltaPosition;
                //Input.touches.
                TouchDist.x *= XSensitivity * sensitivityReduction;
                TouchDist.y *= YSensitivity * sensitivityReduction;
            }
            if (Input.touches[PointerId].phase == TouchPhase.Ended)
            {
                Pressed = false;
                TouchDist = new Vector2();
            }
        }
        if (firePressed && !Pressed)
        {
            if (buttonPointerId >= 0 && buttonPointerId < Input.touches.Length)
            {
                TouchDist = Input.touches[buttonPointerId].deltaPosition;
                TouchDist.x *= XSensitivity * sensitivityReduction;
                TouchDist.y *= YSensitivity * sensitivityReduction;
                //PointerOld = Input.touches[PointerId].position;
            }
            if (Input.touches[buttonPointerId].phase == TouchPhase.Ended)
            {
                firePressed = false;
                if (!Pressed)
                    TouchDist = new Vector2();
            }
        }
    }

    public bool IgnoreButton(int fingerId)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.touches[fingerId].position;//Input.mousePosition;
                                                                      //pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            //if (raycastResultList[i].gameObject.GetComponent<DenyUI>() != null)
            //{
            //    Debug.Log("Over unwanted button");
            //}
            if (raycastResultList[i].gameObject.GetComponent<DenyUI>() != null)
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }
        return raycastResultList.Count > 0;
    }
    //if (Pressed)
    //{
    //    TouchDist = mainTouch.deltaPosition;
    //    TouchDist.x *= XSensitivity * sensitivityReduction;
    //    TouchDist.y *= YSensitivity * sensitivityReduction;

    //    if (mainTouch.phase == TouchPhase.Ended)
    //    {
    //        Pressed = false;
    //        TouchDist = new Vector2();
    //    }
    //}

    //if (firePressed && !Pressed)
    //{
    //    TouchDist = buttonTouch.deltaPosition;
    //    TouchDist.x *= XSensitivity * sensitivityReduction;
    //    TouchDist.y *= YSensitivity * sensitivityReduction;

    //    if (buttonTouch.phase == TouchPhase.Ended)
    //    {
    //        firePressed = false;
    //        if (!Pressed)
    //            TouchDist = new Vector2();
    //    }
    //}



    //public void OnPointerDown(PointerEventData eventData)
    //{

    //    PointerId = eventData.pointerId;
    //    PointerOld = eventData.position;
    //    Pressed = true;
    //    //indexOfTouch = Input.touches.Length - 1;
    //    //PointerId = Input.touches[indexOfTouch].fingerId;
    //}


    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    Pressed = false;
    //}

    public static void scoped(bool scoped)
    {
        if (scoped)
            sensitivityReduction = scopeSensitivity;
        else
            sensitivityReduction = 1f;
    }
}