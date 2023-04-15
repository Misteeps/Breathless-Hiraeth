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
        public static CinemachineTransposer Transposer { get; } = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        public static Bloom Bloom { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is Bloom) as Bloom;
        public static Vignette Vignette { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is Vignette) as Vignette;
        public static ChromaticAberration ChromaticAberration { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is ChromaticAberration) as ChromaticAberration;
        public static ShadowsMidtonesHighlights ShadowsMidtonesHighlights { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is ShadowsMidtonesHighlights) as ShadowsMidtonesHighlights;

        public static float FOV { get => VirtualCamera.m_Lens.FieldOfView; set => VirtualCamera.m_Lens.FieldOfView = value; }

        public static float shake;
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
                Zoom(currentZoom, true);
            }
        }

        private static float currentZoom = 0;
        private static bool hijacked;


        public static void Zoom(float zoom) => Zoom(zoom, !hijacked);
        private static void Zoom(float zoom, bool force)
        {
            if (hijacked && !force) return;

            new Transition(() => currentZoom, Set, currentZoom, zoom, "Camera Zoom").Curve(Function.Cubic, Direction.Out, 420).Start();
            static void Set(float value)
            {
                currentZoom = value;
                value = (30 - value + ZoomOffset);

                Transposer.m_FollowOffset = new Vector3(0, value * 0.2f, value * -0.12f);
                Monolith.CameraObject.transform.eulerAngles = new Vector3(Mathf.Log10(3 * value - 30) * 19 + 17.5f, 0, 0);
            }
        }

        public static void HijackZoom(float zoom) => Zoom(zoom, true);
        public static void HijackRelease() => hijacked = false;
    }
}