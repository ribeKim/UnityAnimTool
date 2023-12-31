using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ribe.UnityAnimTool
{
    public class EditorHierarchy
    {
        [MenuItem("GameObject/GenerateOnOff", false, 10)]
        static void GenerateOnOff(MenuCommand menuCommand)
        { 
            var go = menuCommand.context as GameObject;
            if (go == null) return;
            
            Debug.Log("애니메이션 클립 저장 시작!");

            var folder = Path.Combine("Assets", "OnOffAnimationFolder");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var animationPath = Common.GetAnimationPath(go);

            var onAnimClip = new AnimationClip();
            var offAnimClip = new AnimationClip();

            var onCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f));
            var offCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f));

            onAnimClip.SetCurve(animationPath, typeof(GameObject), "m_IsActive", onCurve);
            offAnimClip.SetCurve(animationPath, typeof(GameObject), "m_IsActive", offCurve);

            Common.SaveAnimationClip(onAnimClip, $"{go.name.Replace(" ", "")}On.anim", folder);
            Common.SaveAnimationClip(offAnimClip, $"{go.name.Replace(" ", "")}Off.anim", folder);

            Debug.Log("애니메이션 클립 저장 완료!");
        }
    }
}