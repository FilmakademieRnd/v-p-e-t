﻿/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace vpet
{
	public class ConfigEvent : UnityEvent<ConfigWidget> { }
	public class AmbientIntensityChangedEvent : UnityEvent<float> { }
	public class VisibilityChangeEvent: UnityEvent<bool> { }
	public class SceneScaleChangedEvent : UnityEvent<float> { }

#if USE_TANGO || USE_ARKIT
    public class TrackingScaleIntensityChangedEvent : UnityEvent<float> { }
	public class ToggleARSwitchEvent : UnityEvent<bool> { }
#endif

    public class ConfigWidget: MonoBehaviour
	{
	    public ConfigEvent SubmitEvent = new ConfigEvent();
		public AmbientIntensityChangedEvent AmbientChangedEvent = new AmbientIntensityChangedEvent();
        public SceneScaleChangedEvent OnSceneScaleChanged = new SceneScaleChangedEvent();
#if USE_TANGO || USE_ARKIT
		public TrackingScaleIntensityChangedEvent TrackingScaleChangedEvent = new TrackingScaleIntensityChangedEvent();
		public ToggleARSwitchEvent ToggleAREvent = new ToggleARSwitchEvent();
#endif

        //!
        //! IP Adress of server
        //!
        [HideInInspector]
	    public string serverIP = "172.17.21.129";
	    //!
	    //! Do we load scene from dump file
	    //!
	    [HideInInspector]
	    public bool doLoadFromResource = true;
	    //!
	    //! Dump scene file name
	    //!
	    [HideInInspector]
	    public string sceneFileName = "";
	    //!
	    //! Shall we load Textures ?
	    //!
	    [HideInInspector]
	    public bool doLoadTextures = true;
		//!
		//! 
		//!
		private float ambientLight = 0.1f;

#if USE_TANGO || USE_ARKIT
        [HideInInspector]
        public float trackingScale = 1.0f;
        private Slider trackingScaleSlider;
#endif


        private InputField serverIPField;

		private Button submitButton;

        private Toggle loadCacheToggle;

		private Toggle loadTexturesToggle;

		private Toggle debugToggle;

        private Toggle gridToggle;

        private Toggle arToggle;

        private Toggle cmToggle;

        private Slider ambientIntensitySlider;

	    void Awake()
		{
            // Submit button
            Transform childWidget = this.transform.Find("Start_button");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: Start_button.", this.GetType()));
            else
            {
				submitButton = childWidget.GetComponent<Button>();
				if (submitButton == null) Debug.LogError(string.Format("{0}: Cant Component: Button.", this.GetType()));
                else
                {
					submitButton.onClick.RemoveAllListeners();
					submitButton.onClick.AddListener(() => OnSubmit());
                }
            }

			// Textfield Server IP
			childWidget = this.transform.Find("ServerIP_field");
			if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: ServerIP_field.", this.GetType()));
			else
			{
				serverIPField = childWidget.GetComponent<InputField>();
				if (serverIPField == null) Debug.LogError(string.Format("{0}: Cant Component: InputField.", this.GetType()));
			}


			// debug toggle
            childWidget = this.transform.Find("Debug_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: Debug_toggle.", this.GetType()));
            else
            {
				debugToggle = childWidget.GetComponent<Toggle>();
				if (debugToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    debugToggle.onValueChanged.AddListener(this.OnToggleDebug);
                }
            }

            // grid toggle
            childWidget = this.transform.Find("Grid_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: Grid_toggle.", this.GetType()));
            else
            {
                gridToggle = childWidget.GetComponent<Toggle>();
                if (gridToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    gridToggle.onValueChanged.AddListener(this.OnToggleGrid);
                }
            }

            // Units toggle
            childWidget = this.transform.Find("CM_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: CM_toggle.", this.GetType()));
            else
            {
                cmToggle = childWidget.GetComponent<Toggle>();
                if (cmToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    cmToggle.onValueChanged.AddListener(this.OnToggleUnitsCm);
                }
            }

            // ar toggle
            childWidget = this.transform.Find("AR_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: AR_toggle.", this.GetType()));
            else
            {

#if USE_TANGO || USE_ARKIT
                arToggle = childWidget.GetComponent<Toggle>();
                if (arToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
                else
                {
                    arToggle.onValueChanged.AddListener(this.OnToggleAr);
                }
#else
                childWidget.gameObject.SetActive(false);
#endif
            }

            // Cache Load Local
            childWidget = this.transform.Find("LoadCache_toggle");
            if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: LoadCache_toggle.", this.GetType()));
            else
            {
                loadCacheToggle = childWidget.GetComponent<Toggle>();
                if (loadCacheToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
            }



			// Checkbox Load Texture
			childWidget = this.transform.Find("LoadTextures_toggle");
			if (childWidget == null) Debug.LogError(string.Format("{0}: Cant Find: LoadTextures_toggle.", this.GetType()));
			else
			{
				loadTexturesToggle = childWidget.GetComponent<Toggle>();
				if (loadTexturesToggle == null) Debug.LogError(string.Format("{0}: Cant Component: Toggle.", this.GetType()));
			}


			// ambient intensity
			childWidget = this.transform.Find("AI_Slider").Find("Ambient_slider");
			if (childWidget == null) Debug.LogWarning(string.Format("{0}: Cant Find: Ambient_slider.", this.GetType()));
			else
			{
				ambientIntensitySlider = childWidget.GetComponent<Slider>();
				if (ambientIntensitySlider == null) Debug.LogWarning(string.Format("{0}: Cant Component: Slider.", this.GetType()));
				else
				{
					ambientIntensitySlider.value = ambientLight;
					ambientIntensitySlider.onValueChanged.AddListener( this.OnAmbientChanged );
				}
			}

#if USE_TANGO || USE_ARKIT
            // tracking scale
            childWidget = this.transform.Find("TS_Slider").Find("TrackingScale_slider");
            if (childWidget == null) Debug.LogWarning(string.Format("{0}: Cant Find: Tracking_Scale.", this.GetType()));
            else
            {
                trackingScaleSlider = childWidget.GetComponent<Slider>();
                if (trackingScaleSlider == null) Debug.LogWarning(string.Format("{0}: Cant Component: Slider.", this.GetType()));
                else
                {
                    trackingScaleSlider.value = trackingScale;
                    trackingScaleSlider.onValueChanged.AddListener(this.OnTrackingScaleChanged);
                }
            }
#endif

        }


        void Start()
		{
			initUIValues();
        }


        public void initConfigWidget()
	    {
	        initUIValues();
            if (VPETSettings.Instance.sceneDumpFolderEmpty)
            {
                loadCacheToggle.isOn = false;
                loadCacheToggle.interactable = false;
                doLoadFromResource = false;
            }
        }


        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
			readUIValues();
            gameObject.SetActive(false);
        }
	
	    void initUIValues()
	    {
	        // Textfield Server IP
			if ( serverIPField )
			{
				serverIPField.text = serverIP;
			}
				
            // Checkbox Load Local
            if (loadCacheToggle )
            {
                loadCacheToggle.isOn = doLoadFromResource;
            }

			// Checkbox Load Texture
			if ( loadTexturesToggle )
			{
				loadTexturesToggle.isOn = doLoadTextures;
			}

			// Ambient intensity
			if ( ambientIntensitySlider )
			{
				ambientIntensitySlider.value = ambientLight;
                Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/AI_Slider/AI_SliderValue").GetComponent<Text>();
                sliderValueText.text = ambientLight.ToString("f1");
                ambientIntensitySlider.onValueChanged.Invoke(ambientLight);
            }

#if USE_TANGO || USE_ARKIT
            // Tracking Scale
            if (trackingScaleSlider)
            {
                trackingScaleSlider.value = trackingScale;
                Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/TS_Slider/TrackingScale_Value").GetComponent<Text>();            
                sliderValueText.text = trackingScale.ToString("f1");
            }
#endif

        }


        void readUIValues()
	    {

			// Textfield Server IP
			if ( serverIPField )
			{
				serverIP = serverIPField.text;
			}

			// Checkbox Load Local
			if (loadCacheToggle )
			{
				doLoadFromResource = loadCacheToggle.isOn;
			}
				
			// Checkbox Load Texture
			if ( loadTexturesToggle )
			{
				doLoadTextures = loadTexturesToggle.isOn;
			}

			// Ambient intensity
			if ( ambientIntensitySlider )
			{
				ambientLight = ambientIntensitySlider.value;
            }

#if USE_TANGO || USE_ARKIT
            // tracking Scale
            if (trackingScaleSlider) {
                trackingScale = trackingScaleSlider.value;
            }
#endif

            // Checkbox Load Local
            if (loadCacheToggle != null)
            {
                doLoadFromResource = loadCacheToggle.isOn;
            }
	
	    }
	
	    public void OnSubmit()
	    {
	        readUIValues();
	        SubmitEvent.Invoke(this);
	    }


        private void OnToggleDebug( bool isOn)
        {
            VPETSettings.Instance.debugMsg = isOn;
        }


        private void OnToggleGrid(bool isOn)
        {
            drawGrid drawGrid = Camera.main.GetComponent<drawGrid>();
            if (drawGrid)
                drawGrid.enabled = isOn;
        }


        private void OnToggleUnitsCm(bool isOn)
        {
            if ( isOn )
            {
                VPETSettings.Instance.sceneScale = 0.01f;
            }
            else
            {
                VPETSettings.Instance.sceneScale = 1f;
            }
            OnSceneScaleChanged.Invoke(VPETSettings.Instance.sceneScale);
        }

#if USE_TANGO || USE_ARKIT
        private void OnToggleAr( bool isOn )
        {
            if (arToggle)
            {
                //MainController mainCtrl = GameObject.Find("MainController").GetComponent<MainController>();
                //mainCtrl.ToggleArMode(isOn);
				ToggleAREvent.Invoke (isOn);

            }
        }
#endif
        private void OnAmbientChanged( float v )
		{
			ambientLight = v;
            Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/AI_Slider/AI_SliderValue").GetComponent<Text>();
            sliderValueText.text = v.ToString("n1");
            AmbientChangedEvent.Invoke( v );
		}

#if USE_TANGO || USE_ARKIT
        private void OnTrackingScaleChanged( float v )
        {
            trackingScale = v;            
            Text sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/TS_Slider/TrackingScale_Value").GetComponent<Text>();            
            sliderValueText.text = v.ToString("n1");
            TrackingScaleChangedEvent.Invoke(v);
        }
#endif

    }

}
