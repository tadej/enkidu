using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InventoryButtonBehavior : MonoBehaviour
    {

        UnityEngine.UI.Button button;

        Color normalColor, highlightedColor;

        Sprite originalSprite;

        CanvasGroup canvasGroup;
        // Use this for initialization
        void Awake()
        {
            button = GetComponent<UnityEngine.UI.Button>();
            if (button)
            {
                normalColor = button.colors.normalColor;
                highlightedColor = button.colors.highlightedColor;
                originalSprite = button.image.sprite;
            }

            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        bool IsVisible()
        {
            if (canvasGroup != null)
            {
                return canvasGroup.interactable;
            }
            return true;
        }

        public void SetEnabled(bool b)
        {
            button.interactable = b;

            if (b)
            {
                button.image.sprite = originalSprite;
            }
            else
            {
                button.image.sprite = button.spriteState.disabledSprite;
            }
        }

        public void OnPointerEnter()
        {
            //Debug.Log ("PointerEnter " + gameObject.name + " " + Time.time);
            if (button && IsVisible())
            {
                if (button.transition == Selectable.Transition.ColorTint)
                {
                    var cols = button.colors;

                    cols.normalColor = cols.highlightedColor = highlightedColor;

                    button.colors = cols;

                    button.image.color = cols.normalColor;
                }
                else
                if (button.transition == Selectable.Transition.SpriteSwap)
                {
                    button.image.sprite = button.spriteState.highlightedSprite;
                }

                Canvas.ForceUpdateCanvases();
            }
        }

        public void ResetButton()
        {
            OnPointerExit();
        }

        public void OnPointerClick()
        {
            OnPointerExit();
        }

        public void OnPointerExit()
        {
            if (button && IsVisible())
            {
                if (button.transition == Selectable.Transition.ColorTint)
                {
                    var cols = button.colors;

                    cols.normalColor = cols.highlightedColor = normalColor;

                    button.colors = cols;

                    button.image.color = cols.normalColor;
                }
                else
                if (button.transition == Selectable.Transition.SpriteSwap)
                {
                    button.image.sprite = originalSprite;
                }


            }

        }
    }
}