using UnityEngine;
using TMPro;

namespace Motiviti.Enkidu
{

    public class CharacterDialogText : MonoBehaviour
    {
        public TextMeshProUGUI text, shadow1, shadow2, shadow3;

        public void SetTextColor(Color main, Color outline)
        {
            text.color = main;
            shadow1.color = shadow2.color = shadow3.color = outline;
        }

        public void SetText(string str)
        {
            text.text = shadow1.text = shadow2.text = shadow3.text = str;
        }

        // Use this for initialization
        void Start()
        {
            SetText("");
        }
    }
}