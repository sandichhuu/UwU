using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UwU.IFS
{
    public static class DIExtension
    {
        /// <summary>
        /// Solve injection from scenes
        /// </summary>
        /// <param name="instance"></param>
        public static void SolveSceneComponent(this object instance)
        {
            if (instance is MonoBehaviour)
            {
                SolveUnityComponents(instance as MonoBehaviour);
            }
        }

        private static void SolveUnityComponents(MonoBehaviour obj)
        {
            var getComponentFields = GetFieldsWithAttribute<GetComponent>(obj);
            var getComponentsFields = GetFieldsWithAttribute<GetComponents>(obj);
            var getComponentInChildFields = GetFieldsWithAttribute<GetComponentInChildren>(obj);
            var getComponentsInChildFields = GetFieldsWithAttribute<GetComponentsInChildren>(obj);
            var getComponentInParentFields = GetFieldsWithAttribute<GetComponentInParent>(obj);
            var getComponentsInParentFields = GetFieldsWithAttribute<GetComponentsInParent>(obj);
            var findObjectOfTypeFields = GetFieldsWithAttribute<FindObjectOfType>(obj);

            foreach (var field in getComponentFields)
            {
                var type = field.FieldType;
                var component = obj.GetComponent(type);
                if (component == null)
                {
                    Debug.LogError("[" + obj.gameObject.name + "]->[" + obj.name + "]: GetComponent<" + type.Name + "> failed !");
                    continue;
                }
                field.SetValue(obj, component);
            }

            foreach (var field in getComponentsFields)
            {
                var type = field.FieldType;
                var components = obj.GetComponents(type);
                if (components == null)
                {
                    Debug.LogError("[" + obj.gameObject.name + "]->[" + obj.name + "]: GetComponents<" + type.Name + "> failed !");
                    continue;
                }
                field.SetValue(obj, components);
            }

            foreach (var field in getComponentInChildFields)
            {
                var type = field.FieldType;
                var includeInactive = field.GetCustomAttribute<GetComponentInChildren>().includeInactive;
                var childCount = obj.transform.childCount;

                if (childCount <= 0)
                {
                    Debug.Break();
                    Debug.DebugBreak();
                }

                if (childCount <= 0)
                {
                    Debug.LogError("[" + obj.gameObject.name + "]->[" + obj.name + "]: Object has no child !");
                }

                Component result = null;

                for (var i = 0; i < childCount; i++)
                {
                    var child = obj.transform.GetChild(i);
                    var component = child.GetComponent(type);

                    if (component == null)
                    {
                        component = child.GetComponentInChildren(type, includeInactive);
                    }

                    if (component != null)
                    {
                        result = component;
                        break;
                    }
                }

                if (result == null)
                {
                    result = obj.GetComponent(type);
                }

                if (result == null)
                {
                    Debug.LogError("[" + obj.gameObject.name + "]->[" + obj.name + "]: GetComponentInChildren<" + type.Name + "> failed !");
                    continue;
                }

                field.SetValue(obj, result);
            }

            foreach (var field in getComponentsInChildFields)
            {
                var fieldType = field.FieldType;
                var includeInactive = field.GetCustomAttribute<GetComponentsInChildren>().includeInactive;
                var type = fieldType.GetElementType();
                var components = obj.GetComponentsInChildren(type, includeInactive);
                var length = components.Length;

                if (components == null)
                {
                    Debug.LogError(string.Format("[{0}]->[{1}]: GetComponentsInChildren<{2}> failed !", obj.gameObject.name, obj.name, type.Name));
                    continue;
                }

                var selfComponent = obj.GetComponent(type);
                var isContainParent = selfComponent && components[0].GetInstanceID() == selfComponent.GetInstanceID();
                var startCopyIndex = isContainParent ? 1 : 0;

                var castedComponents = Array.CreateInstance(type, length - startCopyIndex);
                Array.Copy(components.ToArray(), startCopyIndex, castedComponents, 0, length - startCopyIndex);

                field.SetValue(obj, castedComponents);
            }

            foreach (var field in getComponentInParentFields)
            {
                var type = field.FieldType;
                var component = obj.GetComponentInParent(type);
                if (component == null)
                {
                    Debug.LogError("[" + obj.gameObject.name + "]->[" + obj.name + "]: GetComponentInParent<" + type.Name + "> failed !");
                    continue;
                }
                field.SetValue(obj, component);
            }

            foreach (var field in getComponentsInParentFields)
            {
                var type = field.FieldType.GetElementType();
                var components = obj.GetComponentsInParent(type);
                var length = components.Length;

                if (components == null)
                {
                    Debug.LogError("[" + obj.gameObject.name + "]->[" + obj.name + "]: GetComponentsInParent<" + type.Name + "> failed !");
                    continue;
                }

                var castedComponents = Array.CreateInstance(type, length);
                Array.Copy(components, castedComponents, length);

                field.SetValue(obj, castedComponents);
            }

            foreach (var field in findObjectOfTypeFields)
            {
                var type = field.FieldType;
                var component = UnityEngine.Object.FindObjectOfType(type);
                if (component == null)
                {
                    Debug.LogError("[" + obj.gameObject.name + "]->[" + obj.name + "]: FindObjectOfType<" + type.Name + "> failed !");
                    continue;
                }
                field.SetValue(obj, component);
            }
        }

        //private static T CastComponent<T>(Component component) where T : Component
        //{
        //    return component as T;
        //}

        //private static T[] CastComponents<T>(Component[] components) where T : Component
        //{
        //    return components as T[];
        //}

        private static IEnumerable<FieldInfo> GetFieldsWithAttribute<Attribute>(MonoBehaviour obj)
             where Attribute : System.Attribute
        {
            return obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttributes(typeof(Attribute), true).Length > 0);
        }
    }
}