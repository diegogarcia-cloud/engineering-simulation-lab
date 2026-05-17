using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public static class PostFxRig
    {
        private const string VolumeName = "Gravity Simulator Global Bloom Volume";

        public static void EnsureBloom(Transform parent)
        {
            GameObject existing = GameObject.Find(VolumeName);
            if (existing == null)
            {
                existing = new GameObject(VolumeName);
                existing.transform.SetParent(parent, false);
                Volume volume = existing.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 10f;
                volume.weight = 1f;
                volume.sharedProfile = CreateBloomProfile();
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                UniversalAdditionalCameraData data = mainCamera.GetUniversalAdditionalCameraData();
                if (data != null)
                {
                    data.renderPostProcessing = true;
                    data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                }
                mainCamera.allowHDR = true;
            }
        }

        private static VolumeProfile CreateBloomProfile()
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Runtime Bloom Profile";

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.1f);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.72f);
            bloom.tint.Override(new Color(1f, 0.92f, 0.78f));
            bloom.clamp.Override(65000f);

            return profile;
        }
    }
}
