using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.LakeWalk.MadGuns
{
    public class CustomizeButton : MonoBehaviour, IDragHandler
    {
        //public ButtonEdit buttonEdit;// Remove the button edit script from editor
        [HideInInspector]
        public RectTransform editButton;
        public RectTransform inGameButton;

        void Awake()
        {
            editButton = GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            editButton.anchoredPosition += eventData.delta / CanvasManager.instance.canvas.scaleFactor;
        }

        public void ApplyChanges()
        {
            inGameButton.anchoredPosition = editButton.anchoredPosition;
            inGameButton.localScale = editButton.localScale;
        }

        public void loadCurrentPos()
        {
            editButton.anchoredPosition = inGameButton.anchoredPosition;
            editButton.localScale = inGameButton.localScale;
        }

        public void ChangeScale(float scale)
        {
            editButton.localScale = new Vector3(scale, scale, 1f);
        }

    }
}
