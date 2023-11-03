using BepInEx;
using System;
using UnityEngine;
using Utilla;
using GorillaTagScripts;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.InputSystem;
using PlayFab.ClientModels;
using System.Runtime.CompilerServices;

namespace CastingManager
{
    
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        private GameObject city = null;
        private GameObject forest = null;
        private GameObject canyons = null;
        private GameObject mountains = null;
        private GameObject beach = null;
        private GameObject clouds = null;
        private GameObject betterDayNight;
        private Transform smallTreesParent;
        private GameObject treeLeaf;
        private GameObject treeLeaf1;
        private GameObject treeLeaf2;
        private GameObject treeLeaf3;
        private GameObject smallTreesParentObject;

        private bool isMenuOpen = true;
        private int currentMenu = 0;
        private int maxPage = 1;
        private bool isDragging = false;
        private Vector2 offset = Vector2.zero;
        private Rect windowRect = new Rect(10, 10, 250, 250); // Define the initial position and size of the window
        private GameObject[] smallTrees;
        private float sliderValue = 10000.0f;
        private readonly float minValue = 1000.0f;
        private readonly float maxValue = 10000.0f;

        private bool optionCityOn = false;
        private bool optionForestOn = true;
        private bool optionCanyonsOn = false;
        private bool optionMountainsOn = false;
        private bool optionBeachOn = false;
        private bool optionCloudsOn = false;
        private bool optionLeavesOn = false;


        void Start()
        {
            city = GameObject.Find("Environment Objects/LocalObjects_Prefab/City/");
            forest = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/");
            canyons = GameObject.Find("Environment Objects/LocalObjects_Prefab/Canyon/");
            mountains = GameObject.Find("Environment Objects/LocalObjects_Prefab/Mountain/");
            beach = GameObject.Find("Environment Objects/LocalObjects_Prefab/Beach/");
            clouds = GameObject.Find("Environment Objects/LocalObjects_Prefab/skyjungle/");
            betterDayNight = GameObject.Find("Gameplay Scripts/BetterDayNight/");


            Debug.Log("Mod starting...\nAll maps loaded!");

            treeLeaf = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/SmallTrees/PrefabSmallTree/SpringToFall/smallleaves (1)/");
            treeLeaf1 = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/SmallTrees/PrefabSmallTree/SpringToFall/smallleaves.001/");
            treeLeaf2 = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/SmallTrees/PrefabSmallTree/SpringToFall/smallleaves.002/");
            treeLeaf3 = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/SmallTrees/PrefabSmallTree/SpringToFall/smallleaves.003/");
            smallTreesParentObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/SmallTrees/");

            if (smallTreesParentObject != null)
            {
                smallTreesParent = smallTreesParentObject.transform;
                Debug.Log("Small Trees Parent found.");
            }
            else
            {
                Debug.LogError("Error: Small Trees Parent not found.");
            }

            if (treeLeaf != null && treeLeaf1 != null && treeLeaf2 != null && treeLeaf3 != null && smallTreesParent != null)
            {
                Debug.Log("All leaves loaded and found.");
            }
            else
            {
                Debug.LogError("Error loading leaves or smallTreesParent is null.");
            }

            if (betterDayNight == null)
            {
                Debug.LogError("Better day night game object not found.");
                return;
            }
        }

        private void Update()
        {

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                Debug.Log("T Recognised");

                isMenuOpen = !isMenuOpen;
            }

            if (betterDayNight == null)
            {
                Debug.LogError("Better day night game object not found.");
                return;
            }

            BetterDayNightManager betterDayNightManager = betterDayNight.GetComponent<BetterDayNightManager>();

            var baseSeconds = typeof(BetterDayNightManager).GetField("baseSeconds", BindingFlags.NonPublic | BindingFlags.Instance);

            if (baseSeconds != null)
            {
                // Set the baseSeconds value based on the slider value
                baseSeconds.SetValue(betterDayNightManager, sliderValue);
            }
        }

        private void OnGUI()
        {
            if (isMenuOpen)
            {
                // Handle dragging
                if (isDragging)
                {
                    windowRect.position = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y) - offset;
                }

                GUI.BeginGroup(windowRect);

                GUIStyle customStyle = new GUIStyle(GUI.skin.box);
                customStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.24f));
                customStyle.normal.textColor = Color.white;
                customStyle.fontSize = 14;
                customStyle.border = new RectOffset(10, 10, 10, 10); // Add padding for rounded corners

                GUI.Box(new Rect(0, 0, windowRect.width, windowRect.height), "", customStyle); // Apply custom style

                GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.normal.textColor = new Color(255f, 255f, 255f);
                headerStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.24f));
                headerStyle.fontSize = 14;
                headerStyle.alignment = TextAnchor.MiddleCenter;

                GUI.Box(new Rect(0, 0, windowRect.width, 20), "PUGG's Casting Manager", headerStyle); // Draw header

                windowRect = GUI.Window(0, windowRect, DrawWindow, ""); // Empty string for title

                GUIStyle centerText= new GUIStyle(GUI.skin.label);
                centerText.fontSize = 16;
                centerText.fontStyle = FontStyle.Bold;
                centerText.alignment = TextAnchor.MiddleCenter;

                if (currentMenu == 0)
                {
                    GUI.Label(new Rect(105, 33, 50, 20), "Maps", centerText);
                } else if (currentMenu == 1)
                {
                    GUI.Label(new Rect(105, 33, 50, 20), "Other", centerText); 
                }

                // Cycle Button
                if (GUI.Button(new Rect(15, 30, 50, 20), "<"))
                {
                    if (currentMenu != 0)
                    {
                        currentMenu--;
                    }
                }
                if (GUI.Button(new Rect(185, 30, 50, 20), ">"))
                {
                    if (currentMenu != maxPage)
                    {
                        currentMenu++;
                    }
                }

                // Menu Buttons
                if (currentMenu == 0)
                {
                    if (GUI.Toggle(new Rect(10, 60, 85, 25), optionForestOn, "Forest Map"))
                    {
                        optionForestOn = true;
                        forest.SetActive(true);
                    }
                    else
                    {
                        optionForestOn = false;
                        forest.SetActive(false);
                    }

                    if (GUI.Toggle(new Rect(10, 85, 80, 25), optionCityOn, "City Map"))
                    {
                        optionCityOn = true;
                        city.SetActive(true);
                    }
                    else
                    {
                        optionCityOn = false;
                        city.SetActive(false);
                    }

                    if (GUI.Toggle(new Rect(10, 110, 90, 25), optionCanyonsOn, "Canyon Map"))
                    {
                        optionCanyonsOn = true;
                        canyons.SetActive(true);
                    }
                    else
                    {
                        optionCanyonsOn = false;
                        canyons.SetActive(false);
                    }

                    if (GUI.Toggle(new Rect(10, 135, 110, 25), optionMountainsOn, "Mountains Map"))
                    {
                        optionMountainsOn = true;
                        mountains.SetActive(true);
                    }
                    else
                    {
                        optionMountainsOn = false;
                        mountains.SetActive(false);
                    }

                    if (GUI.Toggle(new Rect(10, 160, 85, 25), optionBeachOn, "Beach Map"))
                    {
                        optionBeachOn = true;
                        beach.SetActive(true);
                    }
                    else
                    {
                        optionBeachOn = false;
                        beach.SetActive(false);
                    }

                    if (GUI.Toggle(new Rect(10, 185, 85, 25), optionCloudsOn, "Clouds Map"))
                    {
                        optionCloudsOn = true;
                        clouds.SetActive(true);
                    }
                    else
                    {
                        optionCloudsOn = false;
                        clouds.SetActive(false);
                    }
                }
                else if (currentMenu == 1)
                {
                    GUI.Label(new Rect(10, 70, 100, 30), "Time of Day");

                    sliderValue = GUI.HorizontalSlider(new Rect(10, 95, 100, 30), sliderValue, minValue, maxValue);

                    if (GUI.Toggle(new Rect(10, 120, 100, 30), optionLeavesOn, "Toggle Leaves"))
                    {
                        optionLeavesOn = true;

                        if (smallTreesParent != null)
                        {
                            Transform[] smallTrees = smallTreesParent.GetComponentsInChildren<Transform>();

                            foreach (Transform smallTreeTransform in smallTrees)
                            {
                                if (smallTreeTransform.name == "PrefabSmallTree")
                                {
                                    GameObject treeLeaf = smallTreeTransform.Find("SpringToFall/smallleaves (1)").gameObject;
                                    GameObject treeLeaf1 = smallTreeTransform.Find("SpringToFall/smallleaves.001").gameObject;
                                    GameObject treeLeaf2 = smallTreeTransform.Find("SpringToFall/smallleaves.002").gameObject;
                                    GameObject treeLeaf3 = smallTreeTransform.Find("SpringToFall/smallleaves.003").gameObject;

                                    if (treeLeaf != null && treeLeaf1 != null && treeLeaf2 != null && treeLeaf3 != null)
                                    {                                   
                                        treeLeaf.SetActive(false);
                                        treeLeaf1.SetActive(false);
                                        treeLeaf2.SetActive(false);
                                        treeLeaf3.SetActive(false);
                                    }
                                    else
                                    {
                                        Debug.LogError("Error loading leaves for PrefabSmallTree: " + smallTreeTransform.name);
                                    }
                                }
                            }                
                        }
                        else
                        {
                            Debug.LogError("smallTreesParent is null.");
                        }
                    }
                    else
                    {
                        optionLeavesOn = false;

                        if (smallTreesParent != null)
                        {
                            Transform[] smallTrees = smallTreesParent.GetComponentsInChildren<Transform>();

                            foreach (Transform smallTreeTransform in smallTrees)
                            {
                                if (smallTreeTransform.name == "PrefabSmallTree")
                                {
                                    GameObject treeLeaf = smallTreeTransform.Find("SpringToFall/smallleaves (1)").gameObject;
                                    GameObject treeLeaf1 = smallTreeTransform.Find("SpringToFall/smallleaves.001").gameObject;
                                    GameObject treeLeaf2 = smallTreeTransform.Find("SpringToFall/smallleaves.002").gameObject;
                                    GameObject treeLeaf3 = smallTreeTransform.Find("SpringToFall/smallleaves.003").gameObject;

                                    if (treeLeaf != null && treeLeaf1 != null && treeLeaf2 != null && treeLeaf3 != null)
                                    {
                                        treeLeaf.SetActive(true);
                                        treeLeaf1.SetActive(true);
                                        treeLeaf2.SetActive(true);
                                        treeLeaf3.SetActive(true);
                                    }
                                    else
                                    {
                                        Debug.LogError("Error loading leaves for PrefabSmallTree: " + smallTreeTransform.name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("smallTreesParent is null.");
                        }
                    }
                }
                GUI.EndGroup();
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }


        private void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
        }

        public void DragWindow(Rect rect)
        {

            if (GUI.RepeatButton(new Rect(0, 0, rect.width, 20), ""))
            {
                if (!isDragging)
                {
                    isDragging = true;
                    offset = new Vector2(Input.mousePosition.x - rect.x, Input.mousePosition.y - rect.y);
                }

                rect.x = Input.mousePosition.x - offset.x;
                rect.y = Input.mousePosition.y - offset.y;
            }
            else
            {
                isDragging = false;
            }
        }
    }

}
