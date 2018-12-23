using UnityEngine;
using System.Collections;
using System.Linq;
using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{
    public class PlayerHead : CharacterHead
    {
        public string actorName = "Elroy";
        public PlayerMouth[] mouths;
        public SpriteRenderer[] eyebrows;
        public GameObject[] eyes;
        public Animator anPupils, anBlink;
        public CharacterBrain elroyBrain;
        public SpriteRenderer[] pupils;

        Vector3[] eyebrowPositionsOriginal;
        Vector3[] pupilPositionsOriginal;

        float lastEyebrowFactor = 0;
        float lastEyebrowHeightChange = 0;

        public override void Start()
        {
            base.Start();
        }

        void OnEnable()
        {
            if (elroyBrain)
            {
                SetEyeGesture(elroyBrain.eyeGesture);
                SetMood((int)elroyBrain.mood);
                if (elroyBrain.interruptFlag) StopTalking();
            }
        }

        public void Blink()
        {
            if (anBlink != null && anBlink.isActiveAndEnabled)
                anBlink.SetTrigger("blink");
        }

        void RecursivelySetRenderers(Transform tr, bool isEnabled)
        {
            if (tr.GetComponent<Renderer>() != null) tr.GetComponent<Renderer>().enabled = isEnabled;

            foreach (Transform child in tr)
            {
                if (child.GetComponent<Renderer>())
                    child.GetComponent<Renderer>().enabled = isEnabled;

                RecursivelySetRenderers(child, isEnabled);
            }
        }

        public override void Initialise()
        {
            if (eyebrows != null && eyebrows.Length > 0)
            {
                eyebrowPositionsOriginal = new Vector3[eyebrows.Length];

                for (int i = 0; i < eyebrows.Length; i++)
                {
                    if (eyebrows[i] == null)
                        eyebrowPositionsOriginal[i] = Vector3.zero;
                    else
                        eyebrowPositionsOriginal[i] = eyebrows[i].transform.localPosition;
                }
            }

            if (pupils != null && pupils.Length > 0)
            {
                pupilPositionsOriginal = new Vector3[pupils.Length];

                for (int i = 0; i < pupils.Length; i++)
                {
                    if (pupils[i] == null)
                        pupilPositionsOriginal[i] = Vector3.zero;
                    else
                        pupilPositionsOriginal[i] = pupils[i].transform.localPosition;
                }

            }

            // obsolete StartCoroutine(AnimatePupils());

            StartCoroutine(InitialiseMood());
        }

        IEnumerator InitialiseMood()
        {
            SetMood(0);
            yield return null;
        }

        public override void SetMood(int moodIndex)
        {
            if (moodIndex == currentMood) return;

            currentMood = moodIndex;

            PlayerMouth activeMouth = null;

            if (mouths != null && mouths.Length > currentMood) activeMouth = mouths[currentMood];

            for (int i = 0; i < moodCount; i++)
            {
                if (mouths != null && mouths.Length > i && mouths[i])
                    mouths[i].SetMouthEnabled(mouths[i] == activeMouth);

                if (eyebrows != null && eyebrows.Length > i && eyebrows[i])
                    eyebrows[i].enabled = currentMood == i;

                if (eyes != null && eyes.Length > i && eyes[i])
                {
                    if (currentMood == i)
                    {
                        RecursivelySetRenderers(eyes[i].transform, true);
                    }
                    else RecursivelySetRenderers(eyes[i].transform, false);
                }

            }
            if (currentMood != -1)
            {
                if (eyebrows != null && eyebrows.Length > currentMood && eyebrows[currentMood]) eyebrows[currentMood].enabled = true;
                if (eyes != null && eyes.Length > currentMood && eyes[currentMood]) RecursivelySetRenderers(eyes[currentMood].transform, true);
                if (mouths != null && mouths.Length > currentMood && mouths[currentMood]) mouths[currentMood].ShowPhoneme("MBP", true);
            }
            else
            {

            }

            //pupilTargetPosition = new Vector3( 0, 0, 0);

            ProcessPupilRelativePosition();
        }

        public override void ChangeMood(CharacterHead.Moods mood)
        {
            SetMood((int)mood);
        }

        public override void ChangeMode(string mode)
        {
        }

        public override void SetEnabled(bool b)
        {
        }

        void SetEyebrowRelativePosition(float diffy)
        {
            if (Mathf.Abs(lastEyebrowFactor - diffy) > 0.001f && (Time.time - lastEyebrowHeightChange) > 1.4f)
            {
                lastEyebrowHeightChange = Time.time;
                lastEyebrowFactor = diffy;
                try
                {
                    if (eyebrows != null && eyebrows.Length > 0 && eyebrows[currentMood] != null) eyebrows[currentMood].transform.localPosition = eyebrowPositionsOriginal[currentMood] + Vector3.up * diffy;
                }
                catch
                {
                    Debug.LogWarning("References missing: eyebrows");
                }
            }
        }

        void ProcessPupilRelativePosition()
        {
        }

        IEnumerator AnimatePupils()
        {
            while (true)
            {
                yield return new WaitForSeconds(1 + Random.value * 2.5f);

                CharacterBrain.EyeGestures eyeGesture = CharacterBrain.EyeGestures.idle;

                if (elroyBrain != null) eyeGesture = elroyBrain.GetCurrentEyeGesture();

                SetEyeGesture(eyeGesture);
            }
        }

        public void SetEyeGesture(CharacterBrain.EyeGestures eyeGesture)
        {
            if (anPupils != null && anPupils.isActiveAndEnabled)
            {
                anPupils.SetInteger("state", (int)eyeGesture);
            }

            if (elroyBrain != null && anPupils != null)
            {
                CharacterHead partner = elroyBrain.conversationPartner;
                if (partner != null)
                {
                    float dy = partner.transform.position.y - anPupils.transform.position.y;

                    if (partner is PlayerHead)
                    {
                        if ((partner as PlayerHead).anPupils != null)
                        {
                            dy = (partner as PlayerHead).anPupils.transform.position.y - anPupils.transform.position.y;
                        }
                    }

                    dy /= 1.5f;

                    if (dy > 1) dy = 1; if (dy < -1) dy = -1;
                }
            }
        }

        public override void ShowPhoneme(string phoneme)
        {
            if (currentMood != -1 && mouths != null && mouths.Length > currentMood)
            {
                var h = mouths[currentMood];
                if (h != null) h.ShowPhoneme(phoneme);
            }

            switch (phoneme.Trim())
            {
                case "AI":
                    SetEyebrowRelativePosition(0.01f);
                    break;

                case "E":
                    SetEyebrowRelativePosition(0.018f);
                    break;

                case "U":
                    SetEyebrowRelativePosition(-0.01f);
                    break;

                case "O":
                    SetEyebrowRelativePosition(-0.01f);
                    break;

                case "WQ":
                    SetEyebrowRelativePosition(0.00f);
                    break;

                case "etc":
                case "CDGKNRSThYZ":
                    SetEyebrowRelativePosition(0.00f);
                    break;

                case "Rest":
                case "rest":
                    SetEyebrowRelativePosition(0.00f);
                    break;

                case "MBP":
                    SetEyebrowRelativePosition(0.007f);
                    break;

                case "L":
                    SetEyebrowRelativePosition(0.01f);
                    break;

                case "FV":
                    SetEyebrowRelativePosition(0.01f);
                    break;

                default:
                    SetEyebrowRelativePosition(0.00f);
                    break;
            }
        }
    }
}