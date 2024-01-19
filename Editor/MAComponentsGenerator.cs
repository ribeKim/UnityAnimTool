using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.modular_avatar.core.menu;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Ribe.UnityAnimTool
{
    public class MAComponentsGenerator
    {
        public static void Generate(GameObject go, string[] names, string path)
        {
            var lightLimitChanger = FindByObjectName(go, "LightLimitChanger");
            if (!lightLimitChanger)
            {
                lightLimitChanger = new GameObject("LightLimitChanger");
                lightLimitChanger.transform.SetParent(go.transform);
            }

            if (!lightLimitChanger) return;

            var mergeAnimator = lightLimitChanger.GetComponent<ModularAvatarMergeAnimator>();
            if (!mergeAnimator)
            {
                lightLimitChanger.AddComponent<ModularAvatarMenuInstaller>();
                mergeAnimator = lightLimitChanger.AddComponent<ModularAvatarMergeAnimator>();
            }

            mergeAnimator.animator = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{path}/MA_FX.controller");
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;
            mergeAnimator.deleteAttachedAnimator = true;

            var menuItem = lightLimitChanger.GetComponent<ModularAvatarMenuItem>();
            if (!menuItem)
            {
                menuItem = lightLimitChanger.AddComponent<ModularAvatarMenuItem>();
            }

            menuItem.Control = menuItem.Control ?? new VRCExpressionsMenu.Control();
            menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            menuItem.MenuSource = SubmenuSource.Children;

            var avatarParameters = lightLimitChanger.GetComponent<ModularAvatarParameters>();
            if (!avatarParameters)
            {
                avatarParameters = lightLimitChanger.AddComponent<ModularAvatarParameters>();
            }

            foreach (var s in names)
            {
                AddMenuItem(lightLimitChanger, s);
                AddParameter(avatarParameters, s);
            }
        }

        private static GameObject FindByObjectName(GameObject go, string name)
        {
            GameObject targetGameObject = null;
            for (var i = 0; i < go.transform.childCount; i++)
            {
                var childGameObject = go.transform.GetChild(i).gameObject;
                if (childGameObject.name != name) continue;
                targetGameObject = childGameObject;
                break;
            }

            return targetGameObject;
        }

        private static GameObject AddMenuItem(GameObject go, string name)
        {
            var hasGameObject = FindByObjectName(go, name);

            ModularAvatarMenuItem menuItem;
            if (!hasGameObject)
            {
                var newGameObject = new GameObject(name);
                menuItem = newGameObject.AddComponent<ModularAvatarMenuItem>();
                newGameObject.transform.SetParent(go.transform);
            }
            else
            {
                menuItem = hasGameObject.GetComponent<ModularAvatarMenuItem>();
            }

            menuItem.Control = menuItem.Control ?? new VRCExpressionsMenu.Control();
            menuItem.Control.name = name;
            menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.RadialPuppet;
            menuItem.Control.subParameters = new[]
            {
                new VRCExpressionsMenu.Control.Parameter
                {
                    name = $"RIBE/{name}"
                }
            };

            return hasGameObject;
        }

        private static void AddParameter(ModularAvatarParameters parameters, string parameterName)
        {
            var parameterExists = parameters.parameters.Any(p => p.nameOrPrefix == $"RIBE/{parameterName}");

            if (parameterExists) return;
            var newParameter = new ParameterConfig
            {
                nameOrPrefix = $"RIBE/{parameterName}",
                syncType = ParameterSyncType.Float,
                defaultValue = 0,
                saved = true
            };
            parameters.parameters.Add(newParameter);
        }
    }
}