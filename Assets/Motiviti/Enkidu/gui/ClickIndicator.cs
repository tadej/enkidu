using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class ClickIndicator : MonoBehaviour
    {

        Animator animator;
        State state = State.Idle;

        int clickNumber = 0;

        int animHashUse, animHashLook, animHashWalk, animHashTalk, animHashExit;

        Transform myTransform;

        AdvCamera advCamera;

        float originalCameraSize = 5;

        float lastCameraSize = 0;

        public bool isFlipped = false;

        bool lastIsFlipped = false;

        public bool showClickIndicator = false;

        public enum State
        {
            Idle = 0,
            Walk = 1,
            Look = 2,
            Use = 3,
            Talk = 4,
            Exit = 5
        }

        public void ChangeState(State newState)
        {
            if (showClickIndicator)
            {
                clickNumber++;
                if (animator.GetComponent<Animation>()) animator.GetComponent<Animation>().Rewind();
                StartCoroutine(ChangeStateProcess(newState, clickNumber));
            }
        }

        IEnumerator ChangeStateProcess(State newState, int clicknum)
        {
            state = newState;

            switch (state)
            {
                case State.Look:
                    if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animHashLook) animator.SetTrigger("reset");
                    else
                        animator.SetInteger("state", (int)state);
                    yield return new WaitForSeconds(1.8f);

                    break;

                case State.Use:
                    if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animHashUse) animator.SetTrigger("reset");
                    else
                        animator.SetInteger("state", (int)state);
                    yield return new WaitForSeconds(1.8f);

                    break;

                case State.Walk:
                    if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animHashWalk) animator.SetTrigger("reset");
                    else
                        animator.SetInteger("state", (int)state);
                    yield return new WaitForSeconds(1.8f);

                    break;

                case State.Talk:
                    if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animHashTalk) animator.SetTrigger("reset");
                    else
                        animator.SetInteger("state", (int)state);
                    yield return new WaitForSeconds(1.8f);

                    break;

                case State.Exit:
                    if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animHashExit) animator.SetTrigger("reset");
                    else
                        animator.SetInteger("state", (int)state);
                    yield return new WaitForSeconds(1.8f);

                    break;
            }


            if (clickNumber == clicknum)
            {
                state = State.Idle;
                //animator.SetInteger ("state", (int)state);
            }
        }

        // Use this for initialization
        void Start()
        {
            animator = GetComponent<Animator>();

            animHashWalk = Animator.StringToHash("Base Layer.gui-steps");
            animHashLook = Animator.StringToHash("Base Layer.gui-look");
            animHashUse = Animator.StringToHash("Base Layer.gui-use");
            animHashTalk = Animator.StringToHash("Base Layer.gui-talk");
            animHashExit = Animator.StringToHash("Base Layer.gui-exit");

            myTransform = transform;
            advCamera = PersistentEngine.advCamera;
        }

        // Update is called once per frame
        void Update()
        {
            if (advCamera)
            {
                if (lastCameraSize != advCamera.cameraSize)
                {
                    if (PersistentEngine.IsMobileScreen())
                        myTransform.localScale = Vector3.one * 0.6f * (advCamera.cameraSize / originalCameraSize);
                    else
                        myTransform.localScale = Vector3.one * 0.5f * (advCamera.cameraSize / originalCameraSize);// + Vector3.one * 0.1f;
                    lastCameraSize = advCamera.cameraSize;
                }
                if (isFlipped && lastIsFlipped != isFlipped)
                {
                    myTransform.localScale = new Vector3(-myTransform.localScale.x, myTransform.localScale.y, myTransform.localScale.z);
                    lastIsFlipped = isFlipped;
                }
                else if (lastIsFlipped != isFlipped)
                {
                    myTransform.localScale = new Vector3(-myTransform.localScale.x, myTransform.localScale.y, myTransform.localScale.z);
                    lastIsFlipped = isFlipped;
                }
            }
        }
    }
}