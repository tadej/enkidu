using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

using Motiviti.Enkidu;

namespace Motiviti.Enkidu
{

    public class Conversation : MonoBehaviour
    {
        public string noConversationString = "Nothing to talk about.";

        Player player;

        public PlayerHead playerCustomHead;

        public int conversationLoaded;

        PlayerHead[] heads;

        public int startingEntry = 0;

        public GameObject sendMessage;

        public string customFinishMessage = "finish";

        public bool phoneConversation = false;

        bool loaded = false;

        int maxStringLength = 60;

        XElement con;

        public string[] actorNames;
        public PlayerHead[] actorHeads;

        void Awake()
        {
            heads = GameObject.FindObjectsOfType<PlayerHead>();
        }

        void Start()
        {
            player = PersistentEngine.player;

            HandleChatmapperXML();
        }

        void DebugExpression(string ex)
        {
            Debug.Log("Expression " + ex + " evaluated as " + AreWorldConditionsMet(ex));
        }

        void HandleChatmapperXML()
        {   
            TextAsset xml = (TextAsset)Resources.Load("game-dialogs");

            XDocument doc = XDocument.Parse(xml.text);

            var actors = doc.Descendants("Actors").Elements();

            actorNames = new string[actors.Count()];
            actorHeads = new PlayerHead[actors.Count()];

            int i = 0;
            foreach (XElement element in actors)
            {
                var el = element.Descendants("Fields").Elements();
                var n = el.ElementAt(0);
                var actorName = actorNames[i] = n.Elements().ElementAt(1).Value;

                var go = GameObject.Find(actorName);
                if (go)
                {
                    var eh = go.GetComponent<PlayerHead>();

                    if (eh != null)
                    {
                        actorHeads[i] = eh;
                    }
                }

                i++;
            }

            var conversations = doc.Descendants("Conversations");

            var conversation = conversations.Elements();

            con = new XElement("test");
            foreach (XElement element in conversation)
            {
                if ((int)element.Attribute("ID") == conversationLoaded)
                    con = element;
            }
            loaded = true;
        }

        public void StartConversation()
        {
            StartCoroutine(StartConversationCor());
        }

        IEnumerator StartConversationCor()
        {
            while (!loaded)
                yield return new WaitForSeconds(0.05f);

            yield return null;

            Debug.Log("Starting entry id: " + startingEntry);
            XElement entrie = GetElementWithId(startingEntry);						
            Debug.Log("Starting entry: " + entrie);
            if (entrie != null)
                StartCoroutine(HandleDialogEntry(entrie, startingEntry));
            else
            {
                yield return StartCoroutine(player.SpeakProcedure(noConversationString));
                if (sendMessage) sendMessage.SendMessage(customFinishMessage, SendMessageOptions.DontRequireReceiver);
            }
        }

        public XElement GetElementWithId(int id)
        {
            var GlobalDialogEntries = con.Descendants("DialogEntries").Elements();
            foreach (XElement element in GlobalDialogEntries)
            {
                if ((int)element.Attribute("ID") == id)
                    return element;
            }
            return null;
        }

        string FixTooLongText(string text)
        {
            text = text.Substring(0, maxStringLength) + " ...";
            return text;
        }

        float ProcessAction(string actionValue)
        {
            if (actionValue.Length > 0)
            {
                int start = actionValue.IndexOf('.') + 1;
                int end = actionValue.IndexOf('(');
                string function = actionValue.Substring(start, end - start);
                if (sendMessage) sendMessage.SendMessage(function, SendMessageOptions.DontRequireReceiver);

                start = actionValue.IndexOf('(') + 1;
                end = actionValue.IndexOf(')');
                if (end > start)
                {
                    string delay = actionValue.Substring(start, end - start);
                    float delayf = 0;
                    try
                    {
                        delayf = float.Parse(delay);
                    }
                    catch { }
                    return delayf;
                }
            }
            return 0;
        }

        string[] ExtractContentsOfHTMLTag(string input, string tag)
        {
            string pattern = "<" + tag + ".*?>(.*?)<\\/" + tag + ">";

            MatchCollection matches = Regex.Matches(input, pattern);

            if (matches.Count < 1) return null;

            string[] ret = new string[matches.Count];
            int i = 0;
            foreach (Match m in matches)
                ret[i++] = m.Groups[1].ToString();
            return ret;
        }

        bool EvalExpression(string expression)
        {
            int i = -1;

            int i1 = expression.IndexOf("=");
            int i2 = expression.IndexOf("<");
            int i3 = expression.IndexOf(">");

            if (i1 != -1) i = i1; if (i2 != -1) i = i2; if (i3 != -1) i = i3;

            int? worldval = PersistentEngine.GetState(expression.Substring(0, i));

            if (worldval == null) return false;

            string cmpvalstr = expression.Substring(i + 1);
            int cmpval = -1;
            try
            {
                cmpval = Int32.Parse(cmpvalstr);
            }
            catch
            {
                return false;
            }

            int worldvalnotnull = (int)worldval;

            if (i1 != -1) // operator =
            {
                return (cmpval == worldvalnotnull);
            }
            else
            if (i2 != -1) // operator =
            {
                return (worldvalnotnull < cmpval);
            }
            else
            if (i3 != -1) // operator =
            {
                return (worldvalnotnull > cmpval);
            }

            return false;
        }
       
         bool AreWorldConditionsMet(string actionValue)
        {
            string[] ifhas = ExtractContentsOfHTMLTag(actionValue, "ifhas");

            if (ifhas != null)
                foreach (string l in ifhas)
                {
                    if (!PersistentEngine.inventory.HasItem(l)) return false;
                }

            string[] ifis = ExtractContentsOfHTMLTag(actionValue, "ifis");

            if (ifis != null)
                foreach (string l in ifis)
                {
                    if (!EvalExpression(l)) return false;
                }

            return true;
        }

        IEnumerator HandleDialogEntry(XElement dialogEntrie, int dialogEntryId)
        {
            yield return new WaitForSeconds(0.2f);
            var OutgoingLinks = dialogEntrie.Descendants("OutgoingLinks").Elements();    //dobim outgoinglinks za dialog entry

            int outgoingLinksCount = OutgoingLinks.Count();     //ševilo možnosti
            bool isGroup = (bool)dialogEntrie.Attribute("IsGroup");

            Debug.Log("dialog entrie: " + dialogEntrie.Attribute("ID") + " is group: " + isGroup + " OutgoingLinks: " + outgoingLinksCount);

            if (isGroup)
            {   //če je grupa, pomeni da ima več tekstov
                string[] lines = new string[5];
                int[] ids = new int[5];
                string[] speaker = new string[5];
                int j = 0;
                for (int i = 0; i < outgoingLinksCount; i++)
                {
                    XElement link = OutgoingLinks.ElementAt(i);
                    int option = (int)link.Attribute("DestinationDialogID");   //id možnosti
                    XElement possibleOption = GetElementWithId(option); // element na opciji
                    var Fields = possibleOption.Descendants("Fields").Elements();  //fields od  possibleOption
                    XElement text = (XElement)Fields.ElementAt(6).LastNode;  //polje z možnim tekstom
                    string talkedText = text.Value;
                    XElement actor = (XElement)Fields.ElementAt(3).LastNode;    //polje ki pove kdo govori možni tekst
                    string actorNumber = actor.Value;
                    XElement action = (XElement)Fields.ElementAt(7).LastNode;   //polje ki pove kdo govori možni tekst

                    bool x = AreWorldConditionsMet(action.Value);

                    if (x)
                    {
                        lines[j] = talkedText;
                        ids[j] = option;
                        speaker[j] = actorNumber;
                        j++;
                    }
                }

                if (outgoingLinksCount == 1)
                {  
                    StartCoroutine(HandleDialogEntry(GetElementWithId(ids[0]), 0));
                }
                else
                {
                    Debug.Log("starting dialog control");
                    Vector3 data = new Vector3(0, 0, 0);
                    if (sendMessage) sendMessage.SendMessage("ActorIsTalking", data, SendMessageOptions.DontRequireReceiver);
                    yield return null;
                    yield return StartCoroutine(DialogControlUI.instance.ShowDialogProc(lines[0], lines[1], lines[2], lines[3], lines[4]));
                    Debug.Log("finished dialog control");
                    int selectedLine = DialogControlUI.instance.selectedLine;
                    StartCoroutine(HandleDialogEntry(GetElementWithId(ids[selectedLine]), selectedLine));  //rekurzija  
                }
            }
            else
            {
                if (outgoingLinksCount == 1)
                {   //če je le ena možnost
                    var Fields = dialogEntrie.Descendants("Fields").Elements();  //fields od  dialogEntrie
                    XElement text = (XElement)Fields.ElementAt(6).LastNode;  //polje s govorjenim textom
                    string talkedText = text.Value;
                    XElement actor = (XElement)Fields.ElementAt(3).LastNode;    //polje ki pove kdo govori
                    string actorValue = actor.Value;
                    if (talkedText.Length > 0)
                    {   // če je text ga pove
                        yield return StartCoroutine(Talk(talkedText, actorValue, dialogEntryId));
                    }

                    XElement action = (XElement)Fields.ElementAt(7).LastNode;	//polje ki pove akcijo

                    var xactions = ExtractContentsOfHTMLTag(action.Value, "action");
                    if (xactions != null)
                        foreach (var a in xactions)
                        {
                            float delay = ProcessAction(a);
                            yield return new WaitForSeconds(delay);
                        }

                    XElement link = OutgoingLinks.ElementAt(0);   //vzamemo link k naslednjemu
                    int newId = (int)link.Attribute("DestinationDialogID");   //id od naslednjega

                    StartCoroutine(HandleDialogEntry(GetElementWithId(newId), newId));  //rekurzija 
                }
                else if (outgoingLinksCount > 1)
                {  // več možnosti      
                    var Fields = dialogEntrie.Descendants("Fields").Elements();  //fields od  dialogEntrie
                    XElement text = (XElement)Fields.ElementAt(6).LastNode;  //polje s govorjenim textom
                    string talkedText = text.Value;
                    //	Debug.Log(talkedText);

                    XElement actor = (XElement)Fields.ElementAt(3).LastNode;    //polje ki pove kdo govori
                    string actorValue = actor.Value;
                    //	Debug.Log(actor.Value);

                    if (talkedText.Length > 0)
                    {   // če je text ga pove
                        yield return StartCoroutine(Talk(talkedText, actorValue, dialogEntryId));
                    }

                    XElement action = (XElement)Fields.ElementAt(7).LastNode;   //polje ki pove akcijo

                    var xactions = ExtractContentsOfHTMLTag(action.Value, "action");
                    if (xactions != null)
                        foreach (var a in xactions)
                        {
                            float delay = ProcessAction(a);
                            yield return new WaitForSeconds(delay);
                        }
                }
                else if (outgoingLinksCount == 0)
                {
                    var Fields = dialogEntrie.Descendants("Fields").Elements();  //fields od  dialogEntrie
                    XElement text = (XElement)Fields.ElementAt(6).LastNode;  //polje s govorjenim textom
                    string talkedText = text.Value;

                    XElement actor = (XElement)Fields.ElementAt(3).LastNode;    //polje ki pove kdo govori
                    string actorValue = actor.Value;

                    if (talkedText.Length > 0)
                    {   // če je text ga pove
                        yield return StartCoroutine(Talk(talkedText, actorValue, dialogEntryId));
                    }
                    XElement action = (XElement)Fields.ElementAt(7).LastNode;   //polje z akcijo

                    var xactions = ExtractContentsOfHTMLTag(action.Value, "action");
                    if (xactions != null)
                        foreach (var a in xactions)
                        {
                            float delay = ProcessAction(a);
                            yield return new WaitForSeconds(delay);
                        }

                    if (sendMessage) sendMessage.SendMessage(customFinishMessage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        IEnumerator Talk(string text, string actor, int entryId)
        {
            text = text.Replace("–", "-");
     
            Vector3 data = new Vector3(0, 0, 0);

            if (sendMessage) sendMessage.SendMessage("ActorIsTalking", data, SendMessageOptions.DontRequireReceiver);
            text = StripText(text);

            if (actor == "Player")
            {
                if (player && !player.staticCharacter)
                {
                    if (phoneConversation)
                        yield return StartCoroutine(player.SpeakProcedure(text, Player.TalkMode.OnPhone, true));
                    else
                        yield return StartCoroutine(player.SpeakProcedure(text));
                }
                else if (playerCustomHead)
                {
                    yield return StartCoroutine(playerCustomHead.Talk(text));
                }

            }
            else
            {

                foreach (var head in heads)
                {
                    if (head.actorName == actor)
                    {
                        yield return StartCoroutine(head.Talk(text));
                    }
                }
            }

            yield return null;
        }

        string StripText(string strIn)
        {
            // Replace invalid characters with empty strings. 
            try
            {
                return Regex.Replace(strIn, @"[^\w[0-9]\., '!?-$%;#]", "", RegexOptions.None);
            }
            // If we timeout when replacing invalid characters,  
            // we should return Empty. 
            catch
            {
                return "";
            }
        }
    }
}