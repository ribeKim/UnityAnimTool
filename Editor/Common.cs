using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ribe.UnityAnimTool
{

    public static class Common
    {
        public static string POIYOMI_MAX_LIGHT = "material._LightingCap";
        public static string POIYOMI_MIN_LIGHT = "material._LightingMinLightBrightness";
        public static string[] POIYOMI_GRAY_LIGHT =
        {
            "material._LightingMonochromatic",
            "material._LightingAdditiveMonochromatic"
        };
        public static string POIYOMI_SHADOW_LIGHT = "material._ShadowStrength";
        
        public static string LILTOON_MAX_LIGHT = "material._LightMaxLimit";
        public static string LILTOON_MIN_LIGHT = "material._LightMinLimit";
        public static string LILTOON_MONO_LIGHT = "material._MonochromeLighting";
        public static string LILTOON_UNLIT_LIGHT = "material._AsUnlit";
        
        public static void SaveAnimationClip(AnimationClip animationClip, string clipName, string folder)
        {
            var savePath = Path.Combine(folder, clipName).Replace("\\", "/");
            if (File.Exists(savePath)) File.Delete(savePath);

            AssetDatabase.CreateAsset(animationClip, savePath);
            AssetDatabase.Refresh();
        }

        public static string GetAnimationPath(GameObject gameObject)
        {
            var parent = gameObject.transform.parent;
            var path = gameObject.name;

            while (parent != null)
            {
                if (parent.parent != null)
                {
                    path = parent.name + "/" + path;
                }

                parent = parent.parent;
            }

            return path;
        }
    }
}