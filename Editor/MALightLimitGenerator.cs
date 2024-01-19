using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ribe.UnityAnimTool
{
    public class MALightLimitGenerator
    {
        [MenuItem("GameObject/리베툴/포이요미", false, 1)]
        static void Poiyomi(MenuCommand menuCommand)
        {
            var go = menuCommand.context as GameObject;
            if (go == null) return;

            var lightClips = new Dictionary<string, dynamic>
            {
                { "max", Common.POIYOMI_SHADOW_LIGHT },
                { "shadow", Common.POIYOMI_GRAY_LIGHT },
                { "gray", Common.POIYOMI_MIN_LIGHT },
                { "min", Common.POIYOMI_MAX_LIGHT }
            };

            Generate(go, lightClips);
        }

        [MenuItem("GameObject/리베툴/릴툰", false, 2)]
        static void Liltoon(MenuCommand menuCommand)
        {
            var go = menuCommand.context as GameObject;
            if (go == null) return;

            var lightClips = new Dictionary<string, dynamic>
            {
                { "min", Common.LILTOON_MIN_LIGHT },
                { "max", Common.LILTOON_MAX_LIGHT },
                { "mono", Common.LILTOON_MONO_LIGHT },
                { "unlit", Common.LILTOON_UNLIT_LIGHT }
            };

            Generate(go, lightClips);
        }

        private static void Generate(GameObject go, Dictionary<string, dynamic> clips)
        {
            Debug.Log($"애니메이션 클립 저장 시작!");
            var folder = GetFolder();
            var targetRenderers = GetRendererGameObjects(go);
            GenerateAnimation(targetRenderers, clips, folder);
            AnimatorControllerGenerator.Generate(folder, clips.Keys.ToArray());
            MAComponentsGenerator.Generate(go, clips.Keys.ToArray(), folder);
            Debug.Log("애니메이션 클립 저장 완료!");
        }

        private static string GetFolder()
        {
            var datetime = DateTime.Now.ToString("yyMMddHHmmss");
            var folder = Path.Combine("Assets", $"RIBE/MA_LIGHT_{datetime}");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            return folder;
        }

        private static List<GameObject> GetRendererGameObjects(GameObject go)
        {
            var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshRenderers = go.GetComponentsInChildren<MeshRenderer>(true);

            var targetRenderers = skinnedMeshRenderers
                .Select(x => x.gameObject)
                .Concat(meshRenderers.Select(x => x.gameObject))
                .Distinct()
                .ToList();

            return targetRenderers;
        }

        private static void GenerateAnimation(List<GameObject> targetRenderers, Dictionary<string, dynamic> lightClips,
            string folder)
        {
            foreach (var lightClip in lightClips)
            {
                AnimationClip clip;
                if (lightClip.Key == "max")
                {
                    clip = Common.CreateFlipLightAnimationClip(targetRenderers, lightClip.Value);
                }
                else
                {
                    clip = Common.CreateLightAnimationClip(targetRenderers, lightClip.Value);
                }

                Common.SaveAnimationClip(clip, $"{lightClip.Key}.anim", folder);
            }
        }
    }
}