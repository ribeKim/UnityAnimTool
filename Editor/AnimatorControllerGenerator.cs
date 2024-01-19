using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Ribe.UnityAnimTool
{
    public class AnimatorControllerGenerator : MonoBehaviour
    {
        public static void Generate(string path, string[] names)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{path}/MA_FX.controller");
            foreach (var clip in names)
            {
                controller.AddParameter($"RIBE/{clip}", AnimatorControllerParameterType.Float);
            }

            var blendTree = new BlendTree
            {
                blendType = BlendTreeType.Direct,
                name = "빛 조절 블렌드 트리"
            };

            foreach (var s in names)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{path}/{s}.anim");
                var childMotions = blendTree.children;
                var childMotion = new ChildMotion
                {
                    timeScale = 1,
                    motion = clip,
                    position = Vector2.zero,
                    threshold = 0,
                    directBlendParameter = $"RIBE/{s}"
                };
                ArrayUtility.Add(ref childMotions, childMotion);
                blendTree.children = childMotions;
            }

            controller.AddMotion(blendTree);
        }
    }
}