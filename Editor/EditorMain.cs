using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Ribe.UnityAnimTool
{
    public class EditorMain : EditorWindow
    {
        [SerializeField] List<GameObject> _objects = new List<GameObject>();
        [SerializeField] List<bool> _objectStates = new List<bool>();

        GameObject _baseAvatar;
        AnimationClip _animationClip;
        SerializedObject _serializedObject;
        ReorderableList _reorderableList;
        Vector2 _scrollPosition = Vector2.zero;

        [MenuItem("RIBE/Light Limit Generator")]
        public static void OpenWindow()
        {
            GetWindow<EditorMain>("Light Limit Generator");
        }

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            GUILayout.Label("Avatar", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _baseAvatar = EditorGUILayout.ObjectField(
                "VRC Avatar", _baseAvatar, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck() && _baseAvatar != default)
            {
                _objects = _baseAvatar.GetComponentsInChildren<Renderer>(true)
                    .Select(x => x.gameObject)
                    .ToList();
                _objectStates = Enumerable.Repeat(true, _objects.Count).ToList();
            }

            if (_baseAvatar == default)
            {
                ClearObjects();
                EditorGUILayout.HelpBox("아바타 오브젝트를 넣어주세요", MessageType.Error, true);
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("On/Off 생성"))
            {
                SaveObjectListToAnimationClip();
            }

            if (GUILayout.Button("LightLimit 생성"))
            {
                SaveObjectListToLightAnimationClip();
            }

            EditorGUILayout.EndHorizontal();
            DrawList();
        }

        private void DrawList()
        {
            if (_reorderableList == default)
            {
                _reorderableList = new ReorderableList(
                    serializedObject: _serializedObject,
                    elements: _serializedObject.FindProperty("_objects"),
                    draggable: false,
                    displayHeader: true,
                    displayAddButton: false,
                    displayRemoveButton: false);
                _reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "대상 오브젝트 목록");
                _reorderableList.drawElementCallback += DrawElementCallback;
                _reorderableList.onChangedCallback = list => Repaint();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            _serializedObject.Update();
            _reorderableList.DoLayoutList();

            EditorGUILayout.EndScrollView();
        }

        private void ClearObjects()
        {
            _objects.Clear();
            _objectStates.Clear();
            if (_reorderableList == null) return;
            _reorderableList.drawElementCallback -= DrawElementCallback;
            _reorderableList = null;
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var elementProperty = _serializedObject.FindProperty("_objects").GetArrayElementAtIndex(index);
            var element = elementProperty.objectReferenceValue as GameObject;

            var toggleRect = new Rect(rect.x, rect.y, 16f, EditorGUIUtility.singleLineHeight);
            var objectRect = new Rect(rect.x + 20f, rect.y, rect.width - 20f, EditorGUIUtility.singleLineHeight);
            _objectStates[index] = EditorGUI.Toggle(toggleRect, _objectStates[index]);
            EditorGUI.LabelField(objectRect, element.name);
        }

        private void SaveObjectListToAnimationClip()
        {
            Debug.Log("애니메이션 클립 저장 시작!");

            var folder = Path.Combine("Assets", "OnOffAnimationFolder");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var activeList = _objects
                .Where((obj, index) => _objectStates[index])
                .ToList();

            foreach (var o in activeList)
            {
                var animationPath = GetAnimationPath(o);

                var onAnimClip = new AnimationClip();
                var offAnimClip = new AnimationClip();

                var onCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f));
                var offCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f));

                onAnimClip.SetCurve(animationPath, typeof(GameObject), "m_IsActive", onCurve);
                offAnimClip.SetCurve(animationPath, typeof(GameObject), "m_IsActive", offCurve);

                SaveAnimationClip(onAnimClip, $"{o.name.Replace(" ", "")}On.anim", folder);
                SaveAnimationClip(offAnimClip, $"{o.name.Replace(" ", "")}Off.anim", folder);
            }

            Debug.Log("애니메이션 클립 저장 완료!");
        }

        private void SaveObjectListToLightAnimationClip()
        {
            Debug.Log("애니메이션 클립 저장 시작!");

            var folder = Path.Combine("Assets", "OnOffAnimationFolder");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var activeList = _objects
                .Where((obj, index) => _objectStates[index])
                .ToList();
            
            var shadowClipForPoiyomi = CreateLightAnimationClip(activeList, "material._ShadowStrength");
            var worldLightClipForPoiyomi = CreateLightAnimationClip(activeList, new[]
            {
                "material._LightingMonochromatic",
                "material._LightingAdditiveMonochromatic"
            });
            var minLightClipForPoiyomi = CreateLightAnimationClip(
                activeList, "material._LightingMinLightBrightness");
            var maxLightClipForPoiyomi = CreateLightAnimationClip(activeList, "material._LightingCap");

            var minLightClipForLilToon = CreateLightAnimationClip(activeList, "material._LightMinLimit");
            var maxLightClipForLilToon = CreateLightAnimationClip(activeList, "material._LightMaxLimit");
            var monoLightClipForLilToon = CreateLightAnimationClip(
                activeList, "material._MonochromeLighting");
            var asUnlitClipForLilToon = CreateLightAnimationClip(
                activeList, "material._AsUnlit");
            
            SaveAnimationClip(shadowClipForPoiyomi, "1.ShadowForPoiyomi.anim", folder);
            SaveAnimationClip(worldLightClipForPoiyomi, "1.WorldLightForPoiyomi.anim", folder);
            SaveAnimationClip(minLightClipForPoiyomi, "1.MinLightForPoiyomi.anim", folder);
            SaveAnimationClip(maxLightClipForPoiyomi, "1.MaxLightForPoiyomi.anim", folder);
            
            SaveAnimationClip(minLightClipForLilToon, "2.MinLightClipForLilToon.anim", folder);
            SaveAnimationClip(maxLightClipForLilToon, "2.MaxLightClipForLilToon.anim", folder);
            SaveAnimationClip(monoLightClipForLilToon, "2.MonoLightClipForLilToon.anim", folder);
            SaveAnimationClip(asUnlitClipForLilToon, "2.AsUnlitClipForLilToon.anim", folder);

            Debug.Log("애니메이션 클립 저장 완료!");
        }

        private AnimationClip CreateLightAnimationClip(List<GameObject> objects, string propertyName)
        {
            var animationClip = new AnimationClip();
            var curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

            foreach (var animationPath in objects.Select(GetAnimationPath))
            {
                animationClip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), propertyName, curve);
            }

            return animationClip;
        }

        private AnimationClip CreateLightAnimationClip(List<GameObject> objects, string[] propertyName)
        {
            var animationClip = new AnimationClip();
            var curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

            foreach (var animationPath in objects.Select(GetAnimationPath))
            {
                foreach (var s in propertyName)
                {
                    animationClip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), s, curve);
                }
            }

            return animationClip;
        }

        private void SaveAnimationClip(AnimationClip animationClip, string clipName, string folder)
        {
            var savePath = Path.Combine(folder, clipName).Replace("\\", "/");
            if (File.Exists(savePath)) File.Delete(savePath);

            AssetDatabase.CreateAsset(animationClip, savePath);
            AssetDatabase.Refresh();
        }

        private string GetAnimationPath(GameObject gameObject)
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