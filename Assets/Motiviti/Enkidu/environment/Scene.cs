﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

namespace Motiviti.Enkidu
{
    public class Scene : StatefulItem
    {
        public float scaleMax = 2;
        public float scaleMin = 1;
        public float scaleMaxPosY = -1.8f;
        public float scaleMinPosY = 5.5f;
        public bool supportInventory = true;
        public bool startWithStaticCharacter = false;
        public bool startInCutscene = false;
        public bool flipYCoordinate = false;
        public string playableCharacter = "player";
        public bool stayInCutsceneAfterInitialState = false;
        public bool sceneAnimationOnlyFirstTime = true; 
        public float speedHorizontal = 1, speedVertical = 0.5f;
        public string[] voicesWontWork;
        public Animator startAnimator;
        public Vector3 StartCirclePosition = Vector3.zero;
        public Animator StartCam;
        public static Transform[] CharacterSizePoints;
        public bool initialStateOneTime = false;
        public string sentenceAfterInitialState = "";
        public delegate void ScenePreInit(object sender, EventArgs e);
        public event ScenePreInit OnScenePreInit;

        [SaveState]
        public Vector3 playerLocation = Vector3.zero;

        [SaveState]
        public Player.State initialState = Player.State.IdleDiagonalFront;

        [SaveState]
        public bool initialStateFinished = false;

        [SaveState]
        public bool enableSceneAnimation = true;

        [SaveState]
        public int arrivalCave = -1;

        Animator animator;
        InteractiveItem[] interactiveItems;
        Transform[] characterSizePoints;
        ArrayList characterSizePointsX;
        ArrayList characterSizePointsY;
        MeshFilter meshFilter;

        public int xsize = 50;
        public int ysize = 50;
        public int randomPoints = 1000;
        public int trianglesInChunk = 20000;
        float boundsTop = float.MinValue, boundsBottom = float.MaxValue, boundsLeft = float.MaxValue, boundsRight = float.MinValue;


        [SaveState]
        int shouldUseExitDoor = 0;

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawIcon(playerLocation, "gizmo-playerpos.psd", true);
        }

        public void useExitDoor()
        {
            shouldUseExitDoor = 1;
            SaveState();
        }

        public void DontUseExitDoor()
        {
            shouldUseExitDoor = 0;
            SaveState();
        }

        public void SetArivalCave(int num)
        {
            arrivalCave = num;
            SaveState();
        }

        public void SetTrigger(string name)
        {
            if (animator) animator.SetTrigger(name);
        }

        public void SetNewScales(float max, float min, float maxposy, float minposy)
        {
            this.scaleMax = max;
            this.scaleMin = min;
            this.scaleMaxPosY = maxposy;
            this.scaleMinPosY = minposy;
        }

        void FixBounds(Transform tr)
        {
            SpriteRenderer s = tr.GetComponent<SpriteRenderer>();

            if (s != null)
            {
                if (s.bounds.min.x < boundsLeft) boundsLeft = s.bounds.min.x;
                if (s.bounds.min.y < boundsBottom) boundsBottom = s.bounds.min.x;
                if (s.bounds.max.x > boundsRight) boundsRight = s.bounds.max.x;
                if (s.bounds.max.y > boundsTop) boundsTop = s.bounds.max.y;
            }

            foreach (Transform child in tr) FixBounds(child);
        }

        public Vector3 GetPoint3D(int index)
        {
            for (int j = 0; j < characterSizePoints.Length; j++)
            {
                Vector3 pos = characterSizePoints[j].position;
            }
            return Vector3.back;
        }

        public float GetCharacterScale()
        {
            Vector3 charPos = PersistentEngine.player.transform.position;

            if (characterSizePoints != null && characterSizePoints.Length >= 3)
            {
                RaycastHit hit;
                charPos = new Vector3(charPos.x, charPos.y, -1);
                Ray ray = new Ray(new Vector3(charPos.x, charPos.y, -1), Vector3.forward);

                if (Physics.Raycast(ray, out hit, 1 << LayerMask.NameToLayer("CharacterScale")))
                {
                    return hit.point.z;
                }
                else return 1f;
            }
            else
            {
                float scaleSlope = 1;
                float scale = 1;
                scaleSlope = (scaleMax - scaleMin) / (scaleMaxPosY - scaleMinPosY);

                scale = scaleMin + (charPos.y - scaleMinPosY) * scaleSlope;
                return scale;
            }
        }

        Vector3 GetScaleAtClosestPoint(Vector3 p)
        {
            float dist = float.MaxValue;
            int index = 0;
            for (int i = 0; i < characterSizePoints.Length; i++)
            {
                Transform tr = characterSizePoints[i];
                float d = Vector3.Distance(tr.position, p);
                if (d < dist)
                {
                    dist = d;
                    index = i;
                }
            }

            return characterSizePoints[index].localScale;
        }
        void InitialiseCharSizePoints()
        {
            if (characterSizePoints.Length <= 0)
                return;

            FixBounds(transform);

            Transform[] newCharacterSizePoints = new Transform[characterSizePoints.Length + 4];
            newCharacterSizePoints[0] = Instantiate(characterSizePoints[0], new Vector3(boundsLeft, boundsBottom, 0), Quaternion.identity).transform;
            newCharacterSizePoints[0].localScale = GetScaleAtClosestPoint(newCharacterSizePoints[0].position);
            newCharacterSizePoints[1] = Instantiate(characterSizePoints[1], new Vector3(boundsLeft, boundsTop, 0), Quaternion.identity).transform;
            newCharacterSizePoints[1].localScale = GetScaleAtClosestPoint(newCharacterSizePoints[1].position);
            newCharacterSizePoints[2] = Instantiate(characterSizePoints[2], new Vector3(boundsRight, boundsBottom, 0), Quaternion.identity).transform;
            newCharacterSizePoints[2].localScale = GetScaleAtClosestPoint(newCharacterSizePoints[2].position);
            newCharacterSizePoints[3] = Instantiate(characterSizePoints[3], new Vector3(boundsRight, boundsTop, 0), Quaternion.identity).transform;
            newCharacterSizePoints[3].localScale = GetScaleAtClosestPoint(newCharacterSizePoints[3].position);

            for (int i = 0; i < characterSizePoints.Length; i++)
            {
                newCharacterSizePoints[i + 4] = characterSizePoints[i];
            }

            characterSizePoints = newCharacterSizePoints;

            Scene.CharacterSizePoints = characterSizePoints;

            characterSizePointsX = new ArrayList();
            characterSizePointsY = new ArrayList();

            for (int i = 0; i < characterSizePoints.Length; i++)
            {
                characterSizePointsX.Add(i);
                characterSizePointsY.Add(i);
            }

            characterSizePointsX.Sort(new CharSizePointComparerX());
            characterSizePointsY.Sort(new CharSizePointComparerY());

            foreach (Transform tr in characterSizePoints)
            {
                var child = tr.GetChild(0);
                SpriteRenderer r = child.GetComponent<SpriteRenderer>();
                r.enabled = false;
            }

        }

        void UpdateScalingRectangle()
        {
            if (characterSizePoints.Length <= 0)
                return;

            int len = characterSizePoints.Length;

            Vector3 pos = PersistentEngine.player.transform.position;

        }

        void Awake()
        {
            base.Initialise();

            if (OnScenePreInit != null) OnScenePreInit(this, null);

            GameObject player = null;

            GameObject[] gos = GameObject.FindGameObjectsWithTag("CharacterSizePoint");

            characterSizePoints = new Transform[gos.Length];

            for (int i = 0; i < gos.Length; i++)
            {
                characterSizePoints[i] = gos[i].transform;
            }

            var pf = Resources.Load("prefabs/Character-" + playableCharacter.ToString()) as GameObject;

            player = Instantiate(pf);

            player.transform.position = playerLocation;

            player.transform.parent = transform;

            PersistentEngine.player = player.GetComponent<Player>();
            PersistentEngine.player.staticCharacter = startWithStaticCharacter;


            var inventory = GameObject.Find("UI-Inventory") ? GameObject.Find("UI-Inventory").GetComponent<Inventory>() : null;

            if (supportInventory && inventory == null)
            {

                var minv = Resources.Load("prefabs/UI-Inventory") as GameObject;

                var inv = Instantiate(minv);
                PersistentEngine.inventory = inv.GetComponent<Inventory>();
            }
            else
            {
                PersistentEngine.inventory = inventory;
            }

            InitialiseCharSizePoints();

            Debug.Log(shouldUseExitDoor);
        }

        void Start()
        {
            animator = GetComponent<Animator>();

            var anim = GetComponent<Animation>();

            if (initialStateFinished && sceneAnimationOnlyFirstTime) enableSceneAnimation = false;

            if (anim)
            {
                anim.enabled = enableSceneAnimation;
            }

            if (startAnimator)
            {
                startAnimator.enabled = enableSceneAnimation;
            }

            interactiveItems = GameObject.FindObjectsOfType<InteractiveItem>();

            SaveState();

            TextMeshPro[] hinges = GetComponentsInChildren<TextMeshPro>(true);

            foreach (TextMeshPro hinge in hinges)
            {
                if (hinge.gameObject.GetComponent<TextContainer>() == null)
                    Debug.LogWarning("name text: " + hinge.name);
            }

            if (startInCutscene)
            {
                PersistentEngine.player.SetInCutScene(true, CutsceneTools.Type.None);
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Highlight all usable items in the scene
            bool highlight = false;
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2) || Input.GetKey(KeyCode.Space))
            {
                highlight = true;
            }

            foreach (InteractiveItem item in interactiveItems)
            {
                item.Highlight(highlight);
            }

            if (Input.GetKeyUp(KeyCode.C))
            {
                PersistentEngine.player.SetInCutScene(false);
            }
        }

        public class CharSizePointComparerX : IComparer
        {
            public int Compare(object x, object y)
            {

                Transform i1 = (Transform)Scene.CharacterSizePoints[(int)x];
                Transform i2 = (Transform)Scene.CharacterSizePoints[(int)y];

                if ((i1.position.x) < (i2.position.x)) return -1;

                if ((i1.position.x) == (i2.position.x)) return 0;

                return 1;
            }
        }

        public class CharSizePointComparerY : IComparer
        {
            public int Compare(object x, object y)
            {

                Transform i1 = (Transform)Scene.CharacterSizePoints[(int)x];
                Transform i2 = (Transform)Scene.CharacterSizePoints[(int)y];

                if ((i1.position.x) < (i2.position.x)) return -1;

                if ((i1.position.x) == (i2.position.x)) return 0;

                return 1;
            }
        }
    }
}