using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.SceneManagement;


namespace Simplex.Editor
{
    #region Window Types
    public static class WindowTypes
    {
        public static Type Inspector => Type.GetType("UnityEditor.InspectorWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        public static Type Scene => typeof(SceneView);
    }
    #endregion Window Types

    #region Asset Utilities
    public static class AssetUtilities
    {
        #region Asset
        public readonly struct Asset
        {
            public readonly string name;
            public readonly string path;
            public readonly string guid;
            public readonly long localID;
            public readonly int instanceID;

            public readonly Type type;
            public readonly UnityEngine.Object asset;


            public Asset(int instanceID)
            {
                this.instanceID = instanceID;
                this.path = AssetDatabase.GetAssetPath(instanceID);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instanceID, out guid, out localID);

                asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                type = asset.GetType();
                name = asset.name;
            }

            public override string ToString() => ConsoleUtilities.Format($"{type:type} {name}\n    Path: {path:info}\n    GUID: {guid:info}  |  Local ID: {localID:info}  |  Instance ID: {instanceID:info}");
        }
        #endregion Asset


        [MenuItem("Assets/Print Info", false, 34)]
        public static void PrintInfo()
        {
            IEnumerable<Asset> assets = Selection.instanceIDs.Select(id => new Asset(id));
            assets.LogCollection($"Asset Info");
        }

        [MenuItem("Assets/Print USS URL", false, 34)]
        public static void PrintUSSURL()
        {
            IEnumerable<Asset> assets = Selection.instanceIDs.Select(id => new Asset(id));
            assets.LogCollection($"Asset USS URLs", asset => $"url('project://database/{asset.path}?fileID={asset.localID}&guid={asset.guid}&type=3#{asset.name}');");
        }

        public static string GetDirectory(this UnityEngine.Object obj) => System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));
        public static string GetPath(this UnityEngine.Object obj) => AssetDatabase.GetAssetPath(obj);
        public static string GetGuid(this UnityEngine.Object obj) => AssetDatabase.AssetPathToGUID(obj.GetPath());
        public static string GetTypeName(this object obj) => ConsoleUtilities.TitleCase((obj is Type type) ? type.Name : obj?.GetType().Name);
        public static Texture2D GetIcon<T>(this T obj) where T : UnityEngine.Object => (Texture2D)EditorGUIUtility.ObjectContent(obj, typeof(T)).image;

        public static T LoadPath<T>(string path) where T : UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(path);
        public static T LoadGuid<T>(string guid) where T : UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));

        public static string[] FindPaths(string filter, params string[] folders) => AssetDatabase.FindAssets(filter, folders).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        public static string[] FindGuids(string filter, params string[] folders) => AssetDatabase.FindAssets(filter, folders);
        public static T[] Find<T>(string filter, params string[] folders) where T : UnityEngine.Object => AssetDatabase.FindAssets(filter, folders).Select(guid => LoadGuid<T>(guid)).Where(asset => asset != null).ToArray();
    }
    #endregion Asset Utilities

    #region Monobehaviour Utilities
    public static class MonobehaviourUtilities
    {
        public static void Edit(this MonoBehaviour monobehaviour, Action action, string description)
        {
            try
            {
                Undo.RegisterCompleteObjectUndo(monobehaviour, ConsoleUtilities.Format($"{monobehaviour:type} {monobehaviour.name} : {description}"));
                action.Invoke();

                EditorUtility.SetDirty(monobehaviour);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            catch (Exception exception) { exception.Error($"Failed editing {monobehaviour:ref} : {description:info}"); }
        }

        private const BindingFlags defaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static MemberInfo GetMember(Type type, string member, BindingFlags flags = defaultFlags) => type.GetField(member, flags) as MemberInfo ?? type.GetProperty(member, flags);

        public static IValue IValue(this MonoBehaviour monobehaviour, string member, BindingFlags flags = defaultFlags) => IValue(monobehaviour, monobehaviour, GetMember(monobehaviour.GetType(), member, flags));
        public static IValue IValue(this MonoBehaviour monobehaviour, object container, string member, BindingFlags flags = defaultFlags) => IValue(monobehaviour, container, GetMember(container.GetType(), member, flags));
        public static IValue IValue(this MonoBehaviour monobehaviour, MemberInfo member) => IValue(monobehaviour, monobehaviour, member);
        public static IValue IValue(this MonoBehaviour monobehaviour, object container, MemberInfo member)
        {
            if (monobehaviour == null) throw new NullReferenceException("Null monobehaviour").Overwrite($"Failed getting IValue {member:ref} in {container:ref} of {monobehaviour:ref}");
            if (container == null) throw new NullReferenceException("Null container").Overwrite($"Failed getting IValue {member:ref} in {container:ref} of {monobehaviour:ref}");
            if (member == null) throw new NullReferenceException("Null member info").Overwrite($"Failed getting IValue {member:ref} in {container:ref} of {monobehaviour:ref}");

            Func<object> getValue = null;
            Action<object> setValue = null;

            try
            {
                if (member is FieldInfo field)
                {
                    getValue = () => field.GetValue(container);
                    setValue = value => monobehaviour.Edit(() => field.SetValue(container, value), $"{field.Name} = {value}");
                }

                else if (member is PropertyInfo property)
                {
                    getValue = () => property.GetValue(container);
                    setValue = value => monobehaviour.Edit(() => property.SetValue(container, value), $"{property.Name} = {value}");
                }

                else throw new ArgumentException("Unexpected member type");
            }
            catch (Exception exception) { exception.Error($"Failed getting IValue {member:ref} in {container:ref} of {monobehaviour:ref}"); }

            return new DelegateValue(getValue, setValue, member.Name);
        }
    }
    #endregion Monobehaviour Utilities

    #region Scriptable Object Utilities
    public static class ScriptableObjectUtilities
    {
        public static void Edit(this ScriptableObject scriptableObject, Action action, string description)
        {
            try
            {
                Undo.RegisterCompleteObjectUndo(scriptableObject, ConsoleUtilities.Format($"{scriptableObject:type} {scriptableObject.name} : {description}"));
                action.Invoke();

                EditorUtility.SetDirty(scriptableObject);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            catch (Exception exception) { exception.Error($"Failed editing {scriptableObject:ref} : {description:info}"); }
        }

        private const BindingFlags defaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static MemberInfo GetMember(Type type, string member, BindingFlags flags = defaultFlags) => type.GetField(member, flags) as MemberInfo ?? type.GetProperty(member, flags);

        public static IValue IValue(this ScriptableObject scriptableObject, string member, BindingFlags flags = defaultFlags) => IValue(scriptableObject, scriptableObject, GetMember(scriptableObject.GetType(), member, flags));
        public static IValue IValue(this ScriptableObject scriptableObject, object container, string member, BindingFlags flags = defaultFlags) => IValue(scriptableObject, container, GetMember(container.GetType(), member, flags));
        public static IValue IValue(this ScriptableObject scriptableObject, MemberInfo member) => IValue(scriptableObject, scriptableObject, member);
        public static IValue IValue(this ScriptableObject scriptableObject, object container, MemberInfo member)
        {
            if (scriptableObject == null) throw new NullReferenceException("Null scriptable object").Overwrite($"Failed getting IValue {member:ref} in {container:ref} of {scriptableObject:ref}");
            if (container == null) throw new NullReferenceException("Null container").Overwrite($"Failed getting IValue {member:ref} in {container:ref} of {scriptableObject:ref}");
            if (member == null) throw new NullReferenceException("Null member info").Overwrite($"Failed getting IValue {member:ref} in {container:ref} of {scriptableObject:ref}");

            Func<object> getValue = null;
            Action<object> setValue = null;

            try
            {
                if (member is FieldInfo field)
                {
                    getValue = () => field.GetValue(container);
                    setValue = value => scriptableObject.Edit(() => field.SetValue(container, value), $"{field.Name} = {value}");
                }

                else if (member is PropertyInfo property)
                {
                    getValue = () => property.GetValue(container);
                    setValue = value => scriptableObject.Edit(() => property.SetValue(container, value), $"{property.Name} = {value}");
                }

                else throw new ArgumentException("Unexpected member type");
            }
            catch (Exception exception) { exception.Error($"Failed getting IValue {member:ref} in {container:ref} of {scriptableObject:ref}"); }

            return new DelegateValue(getValue, setValue, member.Name);
        }
    }
    #endregion Scriptable Object Utilities

    #region Performance Utilities
    public static class PerformanceUtilities
    {
        public static void Benchmark(int count, Action action) => Benchmark($"Benchmark", count, action);
        public static void Benchmark(FormattableString header, int count, Action action)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
                action.Invoke();

            stopwatch.Stop();
            ConsoleUtilities.Log($"{ConsoleUtilities.Format(header)} - {count} Times - {stopwatch.Elapsed.TotalMilliseconds:info} milliseconds");
        }

        public static void TrackGC(object obj) => TrackGC($"{obj:ref}", obj);
        public static void TrackGC(FormattableString name, object obj)
        {
            if (obj == null) { ConsoleUtilities.Warn($"Cannot track garbage collection of {obj:ref}"); return; }

            Tick(new WeakReference(obj), ConsoleUtilities.Format(name));

            static async void Tick(WeakReference instance, string name)
            {
                if (!instance.IsAlive) ConsoleUtilities.Warn($"GC tracker for {name} is already dead");

                ConsoleUtilities.Log($"<color=lime>Live:</color> {name}");

                for (int i = 0; i < 10000; i++)
                {
                    if (!instance.IsAlive)
                    {
                        ConsoleUtilities.Log($"<color=red>Dead:</color> {name}");
                        return;
                    }

                    await GeneralUtilities.DelayMS(100);
                }

                ConsoleUtilities.Warn($"GC tracker reached limit: {name}");
            }
        }
    }
    #endregion Performance Utilities
}