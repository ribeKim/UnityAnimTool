using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ribe.UnityAnimTool
{

    public static class Common
    {
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