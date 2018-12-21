using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class HeadTalkReceiver : MonoBehaviour 
    {
        public PlayerHead head;
        public PlayerMouth mouth;
        public string[] possibleSprites = new string[50];
        public int possibleSpritesint = 0;

        public int[] possibleSpritesId = new int[50];
        public int possibleSpritesIdint = 0;

        public void ShowPhoneme(int id)
        {
            //Debug.Log("showing phoneme: " + id);
            /*int pos = Array.IndexOf(possibleSpritesId, id);
            if (pos > -1)
            {
                // the array contains the string and the pos variable
                // will have its position in the array
            }
            else
            {
                possibleSpritesId[possibleSpritesIdint] = id;
                possibleSpritesIdint++;
            }*/
            //head.mouths[id].enabled = true;
        }

        public void ShowPhoneme(string phoneme)
        {
            switch (phoneme)
            {
                case "mouth_A": head.ShowPhoneme("AI"); break;
                case "mouth_E": head.ShowPhoneme("E"); break;
                case "mouth_O": head.ShowPhoneme("O"); break;
                case "mouth_U": head.ShowPhoneme("U"); break;
                case "mouth_rest": head.ShowPhoneme("rest"); break;
                case "mouth_W": head.ShowPhoneme("WQ"); break;
                case "mouth_M": head.ShowPhoneme("MBP"); break;
                case "mouth_FV": head.ShowPhoneme("FV"); break;
                case "mouth_L": head.ShowPhoneme("L"); break;
                case "mouth_etc": head.ShowPhoneme("etc"); break;
                case "sad": head.ChangeMood(CharacterHead.Moods.Sad); break;
                case "scared": head.ChangeMood(CharacterHead.Moods.Scared); break;
                case "happy": head.ChangeMood(CharacterHead.Moods.Happy); break;
                case "neutral": head.ChangeMood(CharacterHead.Moods.Neutral); break;
                case "determined": head.ChangeMood(CharacterHead.Moods.Determined); break;
                case "angry": head.ChangeMood(CharacterHead.Moods.Angry); break;
            }
            Debug.Log("showing phoneme name: " + phoneme);
            int pos = Array.IndexOf(possibleSprites, phoneme);
            if (pos > -1)
            {
                // the array contains the string and the pos variable
                // will have its position in the array
            }
            else
            {
                possibleSprites[possibleSpritesint] = phoneme;
                possibleSpritesint++;
            }
        }

        public void ShowGesture(string gesture)
        {
            Debug.Log("showing gesture name: " + gesture);
        }

        public void InitialAnimationFinished()
        {

        }
    }
}