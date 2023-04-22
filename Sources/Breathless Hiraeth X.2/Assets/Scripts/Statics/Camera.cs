using System;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

using Cinemachine;
using Simplex;


namespace Game
{
    public static class Camera
    {
        public static CinemachineVirtualCamera VirtualCamera { get; } = Monolith.Camera.GetComponent<CinemachineVirtualCamera>();
        public static CinemachineBasicMultiChannelPerlin Noise { get; } = VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        public static CinemachineOrbitalTransposer Transposer { get; } = VirtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();

        public static Bloom Bloom { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is Bloom) as Bloom;
        public static Vignette Vignette { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is Vignette) as Vignette;
        public static ChromaticAberration ChromaticAberration { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is ChromaticAberration) as ChromaticAberration;
        public static ShadowsMidtonesHighlights ShadowsMidtonesHighlights { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is ShadowsMidtonesHighlights) as ShadowsMidtonesHighlights;
        public static ColorAdjustments ColorAdjustments { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is ColorAdjustments) as ColorAdjustments;

        public static float FOV { get => VirtualCamera.m_Lens.FieldOfView; set => VirtualCamera.m_Lens.FieldOfView = value; }

        public static float Rotation
        {
            get => Transposer.m_XAxis.Value;
            set
            {
                Transposer.m_XAxis.Value = value;
                Monolith.CameraObject.transform.eulerAngles = new Vector3(Monolith.CameraObject.transform.eulerAngles.x, value, 0);
            }
        }

        private static float shake;
        public static float Shake
        {
            get => shake;
            set
            {
                Noise.m_AmplitudeGain = value;
                Noise.m_FrequencyGain = Mathf.Clamp(value, 0, 1);
            }
        }

        private static int zoomOffset;
        public static int ZoomOffset
        {
            get => zoomOffset;
            set
            {
                zoomOffset = value;
                Zoom(Settings.zoom, 1, false);
            }
        }

        private static float currentZoom = -1;
        private static bool hijacked;


        public static void Zoom(float zoom, float speed = 1, bool hijack = false)
        {
            if (hijack) hijacked = true;
            else if (hijacked) return;

            new Transition(() => currentZoom, Set, currentZoom, Mathf.Clamp(zoom + ZoomOffset, -15, 15), "Camera Zoom").Curve(Function.Cubic, Direction.Out, 420).Modify(speed, true).Start();
            static void Set(float value)
            {
                currentZoom = value;
                value = (30 - value);

                Transposer.m_FollowOffset = new Vector3(0, value * 0.2f, value * -0.12f);
                Monolith.CameraObject.transform.eulerAngles = new Vector3(Mathf.Log10(3 * value - 30) * 19 + 17.5f, Rotation, 0);
            }
        }

        public static void ReleaseHijack()
        {
            hijacked = false;
            Zoom(Settings.zoom);
        }
    }
}