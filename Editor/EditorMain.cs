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
        private bool _showFoldout = true;
        private bool _genPoiyomi;
        private bool _genLiltoon = true;
        private string _savePath = "OnOffAnimationFolder";

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
            AvatarSection();
            OptionSection();

            if (GUILayout.Button("LightLimit 생성") && _objectStates.Count > 0)
            {
                SaveObjectListToLightAnimationClip();
            }

            DrawList();
        }

        private void AvatarSection()
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
        }

        private void OptionSection()
        {
            _showFoldout = EditorGUILayout.Foldout(_showFoldout, "설정 옵션");
            if (!_showFoldout) return;
            _savePath = EditorGUILayout.TextField("저장 경로", _savePath);
            _genPoiyomi = EditorGUILayout.Toggle("포이요미", _genPoiyomi);
            _genLiltoon = EditorGUILayout.Toggle("릴툰", _genLiltoon);
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

                _reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    Rect rectButtonSelectAll = new Rect(rect.x + 15f, rect.y, 75f, rect.height);
                    if (GUI.Button(rectButtonSelectAll, "전체 선택"))
                    {
                        for (int i = 0; i < _objectStates.Count; i++)
                        {
                            _objectStates[i] = true;
                        }
                    }

                    Rect rectButtonDeselectAll = new Rect(rect.x + 90f, rect.y, 75f, rect.height);
                    if (GUI.Button(rectButtonDeselectAll, "전체 해제"))
                    {
                        for (int i = 0; i < _objectStates.Count; i++)
                        {
                            _objectStates[i] = false;
                        }
                    }

                    EditorGUI.LabelField(
                        new Rect(rect.x + rectButtonDeselectAll.xMax + 10, rect.y, rect.width - 75f * 2, rect.height),
                        "대상 오브젝트 목록");
                };
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

        private void SaveObjectListToLightAnimationClip()
        {
            Debug.Log("애니메이션 클립 저장 시작!");

            var folder = Path.Combine("Assets", _savePath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var activeList = _objects
                .Where((obj, index) => _objectStates[index])
                .ToList();
            
            if (_genPoiyomi)
            {
                var shadowClipForPoiyomi = CreateLightAnimationClip(activeList, "material._ShadowStrength");
                var worldLightClipForPoiyomi = CreateLightAnimationClip(activeList, new[]
                {
                    "material._LightingMonochromatic",
                    "material._LightingAdditiveMonochromatic"
                });
                var minLightClipForPoiyomi = CreateLightAnimationClip(
                    activeList, "material._LightingMinLightBrightness");
                var maxLightClipForPoiyomi = CreateLightAnimationClip(activeList, "material._LightingCap");

                Common.SaveAnimationClip(shadowClipForPoiyomi, "1.ShadowForPoiyomi.anim", folder);
                Common.SaveAnimationClip(worldLightClipForPoiyomi, "1.WorldLightForPoiyomi.anim", folder);
                Common.SaveAnimationClip(minLightClipForPoiyomi, "1.MinLightForPoiyomi.anim", folder);
                Common.SaveAnimationClip(maxLightClipForPoiyomi, "1.MaxLightForPoiyomi.anim", folder);
            }

            if (_genLiltoon)
            {
                var minLightClipForLilToon = CreateLightAnimationClip(activeList, "material._LightMinLimit");
                var maxLightClipForLilToon = CreateLightAnimationClip(activeList, "material._LightMaxLimit");
                var monoLightClipForLilToon = CreateLightAnimationClip(
                    activeList, "material._MonochromeLighting");
                var asUnlitClipForLilToon = CreateLightAnimationClip(
                    activeList, "material._AsUnlit");

                Common.SaveAnimationClip(minLightClipForLilToon, "2.MinLightClipForLilToon.anim", folder);
                Common.SaveAnimationClip(maxLightClipForLilToon, "2.MaxLightClipForLilToon.anim", folder);
                Common.SaveAnimationClip(monoLightClipForLilToon, "2.MonoLightClipForLilToon.anim", folder);
                Common.SaveAnimationClip(asUnlitClipForLilToon, "2.AsUnlitClipForLilToon.anim", folder);
            }

            Debug.Log("애니메이션 클립 저장 완료!");
        }

        private AnimationClip CreateLightAnimationClip(List<GameObject> objects, string propertyName)
        {
            var animationClip = new AnimationClip();
            var curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.01f, 1.0f));

            foreach (var animationPath in objects.Select(Common.GetAnimationPath))
            {
                animationClip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), propertyName, curve);
            }

            return animationClip;
        }

        private AnimationClip CreateLightAnimationClip(List<GameObject> objects, string[] propertyName)
        {
            var animationClip = new AnimationClip();
            var curve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.01f, 1.0f));

            foreach (var animationPath in objects.Select(Common.GetAnimationPath))
            {
                foreach (var s in propertyName)
                {
                    animationClip.SetCurve(animationPath, typeof(SkinnedMeshRenderer), s, curve);
                }
            }

            return animationClip;
        }
    }
}