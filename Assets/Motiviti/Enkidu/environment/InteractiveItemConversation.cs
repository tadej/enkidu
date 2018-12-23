﻿using UnityEngine;
using System.Collections;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class InteractiveItemConversation : InteractiveItemAction
    {

        public string comment = "";

        InteractiveItemSwitch connectedSwitch;
        public InteractiveItemSwitch.State enableOnlyForSwitchState = InteractiveItemSwitch.State.ANY;

        public bool onlyOnce = false;

        [SaveState]
        bool usedFlag = false;

        public enum ConversationState
        {
            Idle = 0,
            AlreadyTalked = 1,
            GiveItem = 2,
            NothingToTalk = 3
        }

        public ConversationState conversationState = ConversationState.Idle;

        protected DialogControl dialogControl;

        public int[] conversationEntries;

        public void Start()
        {
            base.Initialise();
            connectedSwitch = gameObject.GetComponent<InteractiveItemSwitch>();
            dialogControl = GameObject.Find("DialogControl") ? GameObject.Find("DialogControl").GetComponent<DialogControl>() : null;
        }

        new void Update()
        {
            base.Update();
        }

        public override IEnumerator ProcessArrivedAt()
        {
            if (connectedSwitch != null && !connectedSwitch.ValidateSwitchState(enableOnlyForSwitchState))
            {
                yield return null;
            }
            else
            {
                if (onlyOnce && usedFlag)
                {

                }
                else
                {
                    Global.player.StopWalking();
                    Global.player.StopTalking();
                    Global.player.ChangeState(Player.State.IdleDiagonalFront);
                    Global.player.TurnTowards(interactiveItem);
                    Global.player.conversationPartner = GetComponent<CharacterHead>();
                    usedFlag = true;
                    SaveState();
                    yield return StartCoroutine(RunConversation());
                }
            }
        }

        public virtual IEnumerator RunConversation()
        {
            Global.player.SetInCutScene(true, CutsceneTools.Type.Puzzle);
            if (conversationEntries != null && conversationEntries.Length > (int)conversationState) { }
            else
                //OBSOLETE-TOMI conversationProcess.startingEntry = conversationEntries[0];
                //OBSOLETE-TOMI conversationProcess.StartConversation();
                yield return null;
        }

        public virtual void finish()
        {
            Global.player.SetTargetItem(null, true);

            Global.player.SetInCutScene(false);
        }

        public virtual void JumpToConversationEntry(string entry)
        {
            if (entry.Length > 0)
            {
                int e = 1;
                int.TryParse(entry, out e);
                //OBSOLETE-TOMIconversationProcess.startingEntry = e;
                //OBSOLETE-TOMIconversationProcess.StartConversation();
            }
        }
    }
}