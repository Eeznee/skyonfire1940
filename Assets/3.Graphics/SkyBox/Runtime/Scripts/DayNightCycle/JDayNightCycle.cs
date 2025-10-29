using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;

namespace Pinwheel.Jupiter
{
    [ExecuteInEditMode]
    public class JDayNightCycle : MonoBehaviour
    {
        [SerializeField]
        private JDayNightCycleProfile profile;
        public JDayNightCycleProfile Profile
        {
            get
            {
                return profile;
            }
            set
            {
                profile = value;
            }
        }

        [SerializeField]
        private JSky sky;
        public JSky Sky
        {
            get
            {
                return sky;
            }
            set
            {
                sky = value;
            }
        }

        [SerializeField]
        private bool useSunPivot;
        public bool UseSunPivot
        {
            get
            {
                return useSunPivot;
            }
            set
            {
                useSunPivot = value;
            }
        }

        [SerializeField]
        private Transform sunOrbitPivot;
        public Transform SunOrbitPivot
        {
            get
            {
                return sunOrbitPivot;
            }
            set
            {
                sunOrbitPivot = value;
            }
        }

        [SerializeField]
        private bool useMoonPivot;
        public bool UseMoonPivot
        {
            get
            {
                return useMoonPivot;
            }
            set
            {
                useMoonPivot = value;
            }
        }

        [SerializeField]
        private Transform moonOrbitPivot;
        public Transform MoonOrbitPivot
        {
            get
            {
                return moonOrbitPivot;
            }
            set
            {
                moonOrbitPivot = value;
            }
        }

        [SerializeField]
        private float startTime;
        public float StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = Mathf.Clamp(value, 0f, 24f);
            }
        }

        [SerializeField]
        private float timeIncrement;
        public float TimeIncrement
        {
            get
            {
                return timeIncrement;
            }
            set
            {
                timeIncrement = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private bool autoTimeIncrement;
        public bool AutoTimeIncrement
        {
            get
            {
                return autoTimeIncrement;
            }
            set
            {
                autoTimeIncrement = value;
            }
        }

        private float time;
        public float Time
        {
            get
            {
                return time % 24f;
            }
            set
            {
                time = value % 24f;
            }
        }

        [SerializeField]
        private bool shouldUpdateEnvironmentReflection;
        public bool ShouldUpdateEnvironmentReflection
        {
            get
            {
                return shouldUpdateEnvironmentReflection;
            }
            set
            {
                shouldUpdateEnvironmentReflection = value;
            }
        }

        [SerializeField]
        private int environmentReflectionResolution;
        public int EnvironmentReflectionResolution
        {
            get
            {
                return environmentReflectionResolution;
            }
            set
            {
                int oldValue = environmentReflectionResolution;
                int newValue = Mathf.Clamp(value, 16, 2048);
                environmentReflectionResolution = newValue;
                if (oldValue != newValue)
                {
                    if (environmentReflection != null)
                    {
                        JUtilities.DestroyObject(environmentReflection);
                    }
                    if (environmentProbe != null)
                    {
                        JUtilities.DestroyGameobject(environmentProbe.gameObject);
                    }
                }
            }
        }

        [SerializeField]
        private ReflectionProbeTimeSlicingMode environmentReflectionTimeSlicingMode;
        public ReflectionProbeTimeSlicingMode EnvironmentReflectionTimeSlicingMode
        {
            get
            {
                return environmentReflectionTimeSlicingMode;
            }
            set
            {
                environmentReflectionTimeSlicingMode = value;
            }
        }

        [SerializeField]
        private ReflectionProbe environmentProbe;
        private ReflectionProbe EnvironmentProbe
        {
            get
            {
                if (environmentProbe == null)
                {
                    GameObject probeGO = new GameObject("~EnvironmentReflectionRenderer");
                    probeGO.transform.parent = transform;
                    probeGO.transform.position = new Vector3(0, -1000, 0);
                    probeGO.transform.rotation = Quaternion.identity;
                    probeGO.transform.localScale = Vector3.one;

                    environmentProbe = probeGO.AddComponent<ReflectionProbe>();
                    environmentProbe.resolution = EnvironmentReflectionResolution;
                    environmentProbe.size = new Vector3(1, 1, 1);
                    environmentProbe.cullingMask = 0;
                }
                environmentProbe.clearFlags = ReflectionProbeClearFlags.Skybox;
                environmentProbe.mode = ReflectionProbeMode.Realtime;
                environmentProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                environmentProbe.timeSlicingMode = EnvironmentReflectionTimeSlicingMode;
                environmentProbe.hdr = false;
                return environmentProbe;
            }
        }

        private Cubemap environmentReflection;
        private Cubemap EnvironmentReflection
        {
            get
            {
                if (environmentReflection == null)
                {
                    environmentReflection = new Cubemap(EnvironmentProbe.resolution, TextureFormat.RGBA32, true);
                }
                return environmentReflection;
            }
        }

        private int probeRenderId = -1;

        private void Reset()
        {
            Sky = GetComponent<JSky>();
            StartTime = 0;
            TimeIncrement = 1;
            AutoTimeIncrement = true;
            Time = 0;
        }

        private void OnEnable()
        {
            time = StartTime;
            time = PlayerPrefs.GetFloat("Hour", 10f);
            Camera.onPreCull += OnCameraPreCull;
            RenderPipelineManager.beginContextRendering += OnBeginFrameRenderingSRP;
        }

        private void OnDisable()
        {
            Camera.onPreCull -= OnCameraPreCull;
            RenderPipelineManager.beginContextRendering -= OnBeginFrameRenderingSRP;

            CleanUp();
        }

        private void CleanUp()
        {
            if (environmentProbe != null)
            {
                JUtilities.DestroyGameobject(environmentProbe.gameObject);
            }
            if (environmentReflection != null)
            {
                JUtilities.DestroyObject(environmentReflection);
            }
        }

        private void OnCameraPreCull(Camera cam)
        {
            //AnimateSky();
        }

        private void OnBeginFrameRenderingSRP(ScriptableRenderContext context, List<Camera> list)
        {
            //AnimateSky();
        }

        private void Start() //Update()
        {
            AnimateSky();
            if (ShouldUpdateEnvironmentReflection)
            {
                UpdateEnvironmentReflection();
            }
            else
            {
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            }
        }

        private void AnimateSky()
        {
            if (Profile == null)
                return;
            if (Sky == null)
                return;
            if (Sky.Profile == null)
                return;
            if (AutoTimeIncrement)
            {
                Time += TimeIncrement * UnityEngine.Time.deltaTime;
            }
            float evalTime = Mathf.InverseLerp(0f, 24f, Time);
            Profile.Animate(Sky, evalTime);

            if (Sky.Profile.EnableSun && Sky.SunLightSource != null)
            {
                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.up);

                Transform pivot = (UseSunPivot && SunOrbitPivot != null) ? SunOrbitPivot : transform;
                Matrix4x4 localToWorld = pivot.localToWorldMatrix;
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);
                Sky.SunLightSource.transform.forward = worldDirection;
            }

            if (Sky.Profile.EnableMoon && Sky.MoonLightSource != null)
            {
                float angle = evalTime * 360f;
                Matrix4x4 localRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
                Vector3 localDirection = localRotationMatrix.MultiplyVector(Vector3.down);

                Transform pivot = (UseMoonPivot && MoonOrbitPivot != null) ? MoonOrbitPivot : transform;
                Matrix4x4 localToWorld = pivot.localToWorldMatrix;
                Vector3 worldDirection = localToWorld.MultiplyVector(localDirection);
                Sky.MoonLightSource.transform.forward = worldDirection;
            }
        }

        private void UpdateEnvironmentReflection()
        {
            if ((SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) != 0)
            {
                if (EnvironmentProbe.texture == null)
                {
                    probeRenderId = EnvironmentProbe.RenderProbe();
                }
                else if (EnvironmentProbe.texture != null || EnvironmentProbe.IsFinishedRendering(probeRenderId))
                {
                    Graphics.CopyTexture(EnvironmentProbe.texture, EnvironmentReflection as Texture);
                    RenderSettings.customReflectionTexture = EnvironmentReflection;
                    RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                    probeRenderId = EnvironmentProbe.RenderProbe();
                }
            }
        }
    }
}
