using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Com.LakeWalk.MadGuns
{
    public class EditSettings : MonoBehaviour
    {
        public RectTransform crossH1;
        public RectTransform crossH2;
        public RectTransform crossV1;
        public RectTransform crossV2;
        public float crossPos = 12f;
        private Rect cross;
        public GameObject dot;
        private float positionBeforeMoved;// The position of the button before you start adjusting its position

        public TextMeshProUGUI sensX;
        public TextMeshProUGUI sensY;
        public TextMeshProUGUI scopeSens;
        //public RectTransform[] buttonsInEditor;
        public RectTransform[] buttons;

        private void Start()
        {
            UpdateButtons(buttons);
        }

        public void changeValueX(float senseitivityX)
        {
            FixedTouchField.XSensitivity = senseitivityX;
            sensX.text = senseitivityX.ToString("0.00");
        }

        public void changeValueY(float senseitivityY)
        {
            FixedTouchField.YSensitivity = senseitivityY;
            sensY.text = senseitivityY.ToString("0.00");
        }

        public void changeScopeSensitivity(float sensitivity)// 0% to 200%
        {
            FixedTouchField.scopeSensitivity = sensitivity;
            scopeSens.text = sensitivity.ToString("0.00");
        }

        public void FixedFireButton(bool notFixed)
        {
            Fire.notFixed = notFixed;
        }

        public void saveValues()
        {
            PlayerPrefs.SetFloat("SensitivityX", FixedTouchField.XSensitivity);
            PlayerPrefs.SetFloat("SensitivityY", FixedTouchField.YSensitivity);
            PlayerPrefs.SetInt("FireFixed", Fire.notFixed ? 1 : 0);
            PlayerPrefs.SetFloat("ScopeSensitivity", FixedTouchField.scopeSensitivity);
        }
        public void CrossHairWidth(float width)
        {
            crossH1.localScale = new Vector3(width, crossH1.localScale.y, 0f);
            crossH1.anchoredPosition = new Vector3(crossPos + width / 2 - 16 / 2, 0f, 0f);

            crossH2.localScale = crossH1.localScale;
            crossH2.anchoredPosition = -crossH1.anchoredPosition;

            crossV1.localScale = crossH1.localScale;
            crossV1.anchoredPosition = new Vector3(0f, crossH1.anchoredPosition.x, 0f);

            crossV2.localScale = crossH1.localScale;
            crossV2.anchoredPosition = new Vector3(0f, -crossH1.anchoredPosition.x, 0f);
        }
        public void CrossHairPosition(float position)// minimum 12
        {
            crossPos = position;
            crossH1.anchoredPosition = new Vector3(crossH1.localScale.x / 2 + position - 8, 0f, 0f);
            crossH2.anchoredPosition = -crossH1.anchoredPosition;
            crossV1.anchoredPosition = new Vector3(0f, crossH1.anchoredPosition.x, 0f);
            crossV2.anchoredPosition = new Vector3(0f, -crossH1.anchoredPosition.x, 0f);
        }

        public void CrossHairHeight(float height)
        {
            crossH1.localScale = new Vector3(crossH1.localScale.x, height, 0f);
            crossH2.localScale = crossH1.localScale;
            crossV1.localScale = crossH1.localScale;
            crossV2.localScale = crossH1.localScale;
        }

        public void saveButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                string btn = "button";
                btn += i;
                string buttonPosX = btn + "posX";
                string buttonPosY = btn + "posY";
                string buttonScale = btn + "scale";
                var custBtn = buttons[i].GetComponent<CustomizeButton>();
                custBtn.ApplyChanges();
                PlayerPrefs.SetFloat(buttonPosX, custBtn.editButton.anchoredPosition.x);
                PlayerPrefs.SetFloat(buttonPosY, custBtn.editButton.anchoredPosition.y);
                PlayerPrefs.SetFloat(buttonScale, custBtn.editButton.localScale.x);
                //buttons[i].localPosition = buttonsInEditor[i].localPosition;
            }
        }

        public void SaveCrossHair()
		{
            PlayerPrefs.SetFloat("CrossPosX", crossH1.anchoredPosition.x);
            PlayerPrefs.SetFloat("CrossScaleX", crossH1.localScale.x);
            PlayerPrefs.SetFloat("CrossScaleY", crossH1.localScale.y);
        }

        public void EnableDot(bool enable)
		{
            dot.SetActive(enable);
		}

        public void UpdateCrossHair()
		{
            crossH1.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CrossPosX"), 0f, 0f);
            crossH1.localScale = new Vector3(PlayerPrefs.GetFloat("CrossScaleX"),
            PlayerPrefs.GetFloat("CrossScaleY"), 0f);

            crossH2.localScale = crossV1.localScale = crossV2.localScale = crossH1.localScale;

            crossH2.anchoredPosition = -crossH1.anchoredPosition;

            crossV1.anchoredPosition = new Vector3(0f, crossH1.anchoredPosition.x, 0f);

            crossV2.anchoredPosition = new Vector3(0f, -crossH1.anchoredPosition.x, 0f);
            
        }

        public void UpdateButtons(RectTransform[] buttons)
		{
            if (PlayerPrefs.GetFloat("button0posX") != 0 && PlayerPrefs.GetFloat("button0posX") != 0)
            {
                for (int i = 0; i < buttons.Length; i++)// Checks if the buttons have been edited before if not it will just load the default values
                {
                    string btn = "button";
                    btn += i;
                    string buttonPosX = btn + "posX";
                    string buttonPosY = btn + "posY";
                    string buttonScale = btn + "scale";
                    Vector2 weaponPos = new Vector3(0f, 0f, 0f);
                    weaponPos.x = PlayerPrefs.GetFloat(buttonPosX);
                    weaponPos.y = PlayerPrefs.GetFloat(buttonPosY);
                    buttons[i].anchoredPosition = weaponPos;
                    buttons[i].localScale = new Vector3(PlayerPrefs.GetFloat(buttonScale), PlayerPrefs.GetFloat(buttonScale), 1f);
                    //var custBtn = buttons[i].GetComponent<CustomizeButton>();
                    //custBtn.ApplyChanges();
                }
            }
        }

       
    }
}