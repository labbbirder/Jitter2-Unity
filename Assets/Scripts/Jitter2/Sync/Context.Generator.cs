using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Jitter2.Collision;
using Jitter2.DataStructures;
using Jitter2.UnmanagedMemory;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Jitter2.Sync
{
    partial class SyncContext
    {

        // [InitializeOnLoadMethod]
        static void GenerateAll()
        {
            const string output = "Assets/Scripts/Jitter2/Generated";
            if (Directory.Exists(output)) return;

            var contextType = typeof(SyncContext);
            var methods = contextType.GetMethods().Where(m => false
                || m.Name == nameof(SyncFromExtra)
                || m.Name == nameof(SyncFromExtraUnmanaged)
                );
            var t2method = methods.Select(m => (m.GetParameters()[0].ParameterType, m))
                .ToDictionary(p => p.ParameterType, p => p.m)
                ;
            var t2members = contextType.Assembly.GetTypes()
                .SelectMany(t => t.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Select(m => (a: m.GetCustomAttribute<StateAttribute>(), m: m))
                .Where(p => p.a != null)
                .GroupBy(p => p.m.DeclaringType)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.a.Order).Select(p => p.m))
                ;

            foreach (var (t, members) in t2members)
            {
                var tName = t.IsGenericType ? t.Name.Replace("`1", "<T>") : t.Name;
                var tPureName = t.IsGenericType ? t.Name.Replace("`1", "") : t.Name;

                var constructor = $"{tPureName}() {{}}";
                if (t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Any(c => c.GetParameters().Length == 0))
                {
                    constructor = "";
                }

                var baseStructureInvocation = $"base.SyncFrom(another, ctx);";
                var baseReferenceInvocation = $"base.ReferencePhaseSyncFrom(another, ctx);";
                var virtualKey = "override";
                if (!IsISync(t.BaseType))
                {
                    virtualKey = "";
                    baseStructureInvocation =
                    baseReferenceInvocation =
                    "";
                }
                if (t2members.Keys.Any(sub => sub.IsSubclassOf(t)))
                {
                    virtualKey = "virtual";
                }


                var creatorMethod = $@"
        public {virtualKey} ISync CreateSimilar(SyncContext ctx) => new {tName}();
                ";
                if (t.IsAbstract)
                {
                    creatorMethod = $@"
        public {virtualKey} ISync CreateSimilar(SyncContext ctx) => default;
                ";
                }
                if (t.GetMethod(nameof(ISync.CreateSimilar)) != null)
                {
                    creatorMethod = "";
                }

                var structureRows = "";
                var referenceRows = "";
                var order = int.MinValue;
                foreach (var m in members)
                {
                    var mType = GetMemberType(m);
                    var attr = m.GetCustomAttribute<StateAttribute>();
                    ref var rows = ref (attr.IsLateReference ? ref referenceRows : ref structureRows);
                    if (attr.Order > order)
                    {
                        order = attr.Order;
                        structureRows += $@"
            ;(this as ISyncStageReceiver).OnEnterStage({order}, another, ctx);
                        ";
                    }

                    var thisAccessor = "this";
                    var otherAccessor = "other";
                    if (!string.IsNullOrEmpty(attr.Format))
                    {
                        thisAccessor = string.Format(attr.Format, thisAccessor);
                        otherAccessor = string.Format(attr.Format, otherAccessor);
                    }
                    thisAccessor += ".";
                    otherAccessor += ".";

                    var mName = m.Name.Split(".")[^1];
                    var assignor = $"{thisAccessor}{mName} = ";
                    if (IsReadOnly(m))
                    {
                        assignor = "";
                    }

                    if (IsJHandle(mType))
                    {
                        rows += $@"
            {thisAccessor}{mName} = ctx.SyncFromExtra({otherAccessor}{mName}, {attr.handleIndex});";
                    }
                    else if (mType.IsArray)
                    {
                        if (IsCompletelyValueType(mType.GetElementType()))
                        {
                            var isAllowExtraSize = attr.AllowExtraSize.ToString().ToLower();
                            rows += $@"
            if (ctx.GetOrCreate({mName}, {otherAccessor}{mName},out var _{mName}))
                {assignor}ctx.{nameof(SyncUnmanagedArray)}(_{mName}, {otherAccessor}{mName}, {isAllowExtraSize});";
                        }
                        else
                        {
                            rows += $@"
            if (ctx.GetOrCreate({mName}, {otherAccessor}{mName},out var _{mName}))
                {assignor}ctx.{nameof(SyncArray)}(_{mName}, {otherAccessor}{mName});";
                        }
                    }
                    else if (TryGetExtraMethod(mType, out var isManaged))
                    {
                        if (isManaged)
                        {
                            rows += $@"
            if (ctx.GetOrCreate({mName}, {otherAccessor}{mName},out var _{mName}))
                {assignor}ctx.{nameof(SyncFromExtra)}(_{mName}, {otherAccessor}{mName});";
                        }
                        else
                        {
                            rows += $@"
            if (ctx.GetOrCreate({mName}, {otherAccessor}{mName},out var _{mName}))
                {assignor}ctx.{nameof(SyncFromExtraUnmanaged)}(_{mName}, {otherAccessor}{mName});";
                        }
                    }
                    else if (IsCompletelyValueType(mType))
                    {
                        rows += $@"
            {thisAccessor}{mName} = {otherAccessor}{mName};";
                    }
                    else
                    {
                        rows += $@"
            {assignor}ctx.SyncFrom({mName}, {otherAccessor}{mName});";
                    }
                }

                // if (t.GetMethod(nameof(ISync.SyncFrom)) != null)
                // {
                //     creatorMethod = "";
                // }
                if (!string.IsNullOrWhiteSpace(referenceRows))
                {
                    referenceRows = $@"
        public {virtualKey} void ReferencePhaseSyncFrom(ISync another, SyncContext ctx){{
            {baseReferenceInvocation}
            var other = another as {tName};
            {referenceRows}
        }}
                    ";
                }
                var tmpl = $@"
using static Jitter2.Sync.HandleIndex;
using Jitter2.Sync;

namespace {t.Namespace}
{{
    partial class {tName} : ISync, ISyncStageReceiver
    {{
        {constructor}
        {creatorMethod}
        public {virtualKey} void SyncFrom(ISync another, SyncContext ctx){{
            {baseStructureInvocation}
            var other = another as {tName};
            {structureRows}
        }}
        {referenceRows}
    }}
}}
";
                var cspath = Path.Combine(output, $"{tName.Replace("<T>", "_T")}.cs");
                if (!Directory.Exists(output)) Directory.CreateDirectory(output);
                File.WriteAllText(cspath, tmpl);
            }
            bool IsJHandle(Type type)
            {
                return type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(JHandle<>);
            }
            bool TryGetExtraMethod(Type type, out bool isManaged)
            {
                Type[] gargs = null;
                if (type.IsGenericType)
                {
                    if (type.IsConstructedGenericType)
                    {
                        gargs = type.GenericTypeArguments;
                        type = type.GetGenericTypeDefinition();
                    }
                }

                isManaged = gargs == null || !gargs.All(IsCompletelyValueType);
                return t2method.Keys.Any(t => t.MetadataToken == type.MetadataToken);
            }
            bool IsISync(Type type)
            {
                while (type != null)
                {
                    if (t2members.ContainsKey(type))
                    {
                        return true;
                    }
                    type = type.BaseType;
                }
                return false;
            }
        }
        static bool IsCompletelyValueType(Type type)
        {
            if (!type.IsValueType) return false;
            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!IsCompletelyValueType(f.FieldType)) return false;
            }
            return true;
        }

        static Type GetMemberType(MemberInfo member)
        {
            if (member is FieldInfo fi) return fi.FieldType;
            if (member is PropertyInfo pi) return pi.PropertyType;
            return null;
        }
        static bool IsReadOnly(MemberInfo member)
        {
            if (member is FieldInfo fi) return fi.IsInitOnly;
            if (member is PropertyInfo pi) return !pi.CanWrite;
            return false;
        }
    }

    public sealed class StateAttribute : Attribute
    {
        public bool IsLateReference { get; set; } = false;
        public bool AllowExtraSize { get; set; } = true;
        public string Format { get; set; }
        public int Order { get; set; } = 0;
        public HandleIndex handleIndex { get; set; } = HandleIndex.UNSET;
    }
    public sealed class ManualStateAttribute : Attribute
    {
    }
}