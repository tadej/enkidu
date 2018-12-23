using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class AnimationNarrate : MonoBehaviour
    {

        Player narrator;
        public string[] lines;

        public void SayLine(int l)
        {
            if (narrator != null)
                narrator.Speak(lines[l]);
        }
        // Use this for initialization
        void Start()
        {
            narrator = Global.narrator;
        }
    }
}