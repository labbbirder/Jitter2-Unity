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
    public interface ISyncStageReceiver
    {
        void OnEnterStage(int stage, ISync another, SyncContext ctx) { }
    }
    public interface ISync
    {
        void SyncFrom(ISync another, SyncContext ctx);
        ISync CreateSimilar(SyncContext ctx);


        public ISync Clone(SyncContext ctx)
        {
            var inst = CreateSimilar(ctx);
            inst.SyncFrom(this, ctx);
            return inst;
        }
    }

    #region memory offset
    public enum HandleIndex : int
    {
        UNSET = -1,
        CONTACT_DATA,
        RIGID_BODY_DATA,
        CONSTRAINT_DATA,
        SMALL_CONSTRAINT_DATA,
        LENGTH,
    }

    unsafe partial class SyncContext
    {

        nint[] memoryOffsets = new nint[(int)HandleIndex.LENGTH];
        public void RegisterMemoryOffset(HandleIndex idx, nint offset)
        {
            memoryOffsets[(int)idx] = offset;
        }

        public nint GetMemoryOffset(HandleIndex idx)
        {
            return memoryOffsets[(int)idx];
        }

        public JHandle<T> SyncFromExtra<T>(JHandle<T> other, HandleIndex idx) where T : unmanaged
        {
            JHandle<T> self;
            self.Pointer = (T**)((nint)other.Pointer + GetMemoryOffset(idx));
            return self;
        }
    }
    #endregion
    partial class SyncContext
    {
        static Dictionary<int, MethodInfo> m_extraMethods;
        internal static Dictionary<int, MethodInfo> ExtraMethods
        {
            get
            {
                if (m_extraMethods == null)
                {
                    var contextType = typeof(SyncContext);
                    m_extraMethods = contextType.GetMethods()
                        .Where(m => m.Name == nameof(SyncFromExtra))
                        .Select(m => (m.GetParameters()[0].ParameterType, m))
                        .ToDictionary(p => p.ParameterType.MetadataToken, p => p.m)
                        ;
                }
                return m_extraMethods;
            }
        }


        // public T[] SyncFromExtra<T>(T[] self, T[] another) where T : class, ISync
        // {
        //     if (another.Length > self.Length)
        //     {
        //         Array.Resize(ref self, another.Length);
        //     }
        //     for (int i = 0; i < another.Length; i++)
        //     {
        //         SyncFrom(ref self[i], another[i]);
        //     }
        //     return self;
        // }

        // public T[] SyncFromExtraUnmanaged<T>(T[] self, T[] another) where T : unmanaged
        // {
        //     if (another.Length > self.Length)
        //     {
        //         Array.Resize(ref self, another.Length);
        //     }
        //     Array.ConstrainedCopy(self, 0, another, 0, another.Length);
        //     return self;
        // }

        public Dictionary<K, V> SyncFromExtra<K, V>(Dictionary<K, V> self, Dictionary<K, V> another) where V : class, ISync
        {
            // List<K> lst = new();
            foreach (var (k, v) in self)
            {
                SyncFrom(v, null);
                // if (!another.ContainsKey(k))
                // {
                //     lst.Add(k);
                // }
            }
            self.Clear();
            foreach (var (k, v) in another)
            {
                self.Add(k, SyncFrom(null, v));
            }
            Assert.IsTrue(self.Count == another.Count);
            foreach (var (k, v) in another)
            {
                Assert.IsTrue(self.ContainsKey(k));
            }
            return self;
        }

        public Stack<T> SyncFromExtraUnmanaged<T>(Stack<T> self, Stack<T> another) where T : unmanaged
        {
            self.Clear();
            foreach (var item in another.Reverse())
            {
                self.Push(item);
            }
            return self;
        }

        public List<T> SyncFromExtraUnmanaged<T>(List<T> self, List<T> another) where T : unmanaged//, ISync
        {
            self.Clear(); // For unmanaged items, the corelib only set the _size to 0, because no gc is needed
            self.AddRange(another);
            return self;
        }

        public List<T> SyncFromExtra<T>(List<T> self, List<T> another) where T : class, ISync
        {
            var selfCount = self.Count;
            var otherCount = another.Count;
            if (otherCount > selfCount) //avoid later Array.Copy
            {
                self.Capacity = another.Count;
            }

            var minCount = Math.Min(selfCount, otherCount);
            var maxCount = selfCount ^ otherCount ^ minCount;
            var i = 0;
            for (; i < minCount; i++)
            {
                self[i] = SyncFrom(self[i], another[i]);
            }
            if (otherCount > selfCount)
            {
                for (; i < maxCount; i++)
                    self.Add(SyncFrom(null, another[i]));
            }
            else
            {
                var ifrom = i;
                var icnt = 0;
                for (; i < maxCount; i++)
                {
                    SyncFrom(self[i], null);
                    icnt++;
                }
                self.RemoveRange(ifrom, icnt);
            }

            return self;
        }

        public void SyncFromExtra<T>(HashList<T> self, HashList<T> another) where T : class, ISync
        {
            self.Clear();
            foreach (var item in another)
            {
                T inst = null;
                SyncFrom(ref inst, item);
                self.Add(inst);
            }
        }

        public DynamicTree<T>.Node SyncFromExtra<T>(DynamicTree<T>.Node self, DynamicTree<T>.Node another) where T : class, IDynamicTreeProxy, IListIndex, ISync
        {
            self = another;
            self.Proxy = SyncFrom(self.Proxy, another.Proxy);
            return self;
        }

        public struct TypeHandler
        {
            public Func<object, object> creator;
            public Action<object, object, SyncContext> sync;
        }
        Dictionary<Type, TypeHandler> typeHandlers = new();


        // [InitializeOnLoadMethod]
        static void GenerateAll()
        {
            const string output = "Assets/Scripts/Jitter2/Generated";
            if (Directory.Exists(output)) return;

            var contextType = typeof(SyncContext);
            var methods = contextType.GetMethods().Where(m => m.Name == nameof(SyncFromExtra));
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

                var baseInvocation = $"base.SyncFrom(another, ctx);";
                var virtualKey = "override";
                if (!IsISync(t.BaseType))
                {
                    virtualKey = "";
                    baseInvocation = "";
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

                var rows = "";
                var order = int.MinValue;
                foreach (var m in members)
                {
                    var mType = GetMemberType(m);
                    var attr = m.GetCustomAttribute<StateAttribute>();
                    if (attr.Order > order)
                    {
                        order = attr.Order;
                        rows += $@"
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
                            rows += $@"
            if (ctx.GetOrCreate({mName}, {otherAccessor}{mName},out var _{mName}))
                {assignor}ctx.{nameof(SyncUnmanagedArray)}(_{mName}, {otherAccessor}{mName});";
                        }
                        else
                        {
                            rows += $@"
            if (ctx.GetOrCreate({mName}, {otherAccessor}{mName},out var _{mName}))
                {assignor}ctx.{nameof(SyncArray)}(_{mName}, {otherAccessor}{mName});";
                        }
                    }
                    else if (TryGetExtraMethod(mType, out var methodName))
                    {
                        rows += $@"
            if (ctx.GetOrCreate({mName}, {otherAccessor}{mName},out var _{mName}))
                {assignor}ctx.{methodName}(_{mName}, {otherAccessor}{mName});";
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
            {baseInvocation}
            var other = another as {tName};
            {rows}
        }}
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
            bool TryGetExtraMethod(Type type, out string methodName)
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

                if (gargs != null && gargs.All(IsCompletelyValueType))
                {
                    methodName = nameof(SyncFromExtraUnmanaged);
                }
                else
                {
                    methodName = nameof(SyncFromExtra);
                }
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

    public unsafe partial class SyncContext
    {

        public void Reset()
        {
        }


        public T* SyncMemory<T>(T* to, int toSize, T* from, int fromSize) where T : unmanaged
        {
            if (toSize < fromSize)
            {
                to = (T*)MemoryHelper.AllocateHeap(fromSize);
            }
            MemoryCopy(to, from, fromSize);
            return to;
            static void MemoryCopy(void* to, void* from, int size)
            {
                Buffer.MemoryCopy(from, to, size, size);
            }
        }


        Dictionary<object, object> S2D = new();
        HashSet<object> DLinked = new();
        Dictionary<Type, HashSet<object>> DFree = new();


        /// return 
        (bool needCreate, bool needSync) GetCache<T>(T dst, T src, out T res) where T : class
        {
            if (src is null)
            {
                // return to pool
                if (dst != null && !DLinked.Contains(dst))
                {
                    // TODO: remove references
                    AddToPool(DFree, dst);
                }
                res = null;
                return (false, false);
            }

            // Assert src != null

            if (S2D.TryGetValue(src, out var v))
            {
                res = v as T;
                return (false, false);
            }

            // we need a new dst
            if (dst is null || DLinked.Contains(dst))
            {
                // return from pool
                if (DFree.Count > 0)
                {
                    res = DFree.First() as T;
                    return (false, true);
                }
                // create a new one
                else
                {
                    res = null;
                    return (true, true);
                }
            }
            else
            {
                res = dst;
                return (false, true);
            }
        }

        void SetCache<T>(T dst, T src) where T : class
        {
            DLinked.Add(dst);
            RemoveFromPool(DFree, dst);
            S2D[src] = dst;
        }

        public bool GetOrCreate<T>(T dst, T src, out T res) where T : class, new()
        {
            var (needCreate, needSync) = GetCache(dst, src, out res);
            if (needCreate)
            {
                res = new();
            }
            // Assert src != null && dst !=null
            if (!needSync) return false;

            SetCache(dst, src);

            return true;
        }

        public bool GetOrCreate<T>(T[] dst, T[] src, out T[] res)
        {
            var (needCreate, needSync) = GetCache(dst, src, out res);
            if (needCreate)
            {
                res = new T[src.Length];
            }
            // Assert src != null && dst !=null
            if (!needSync) return false;

            SetCache(dst, src);
            return true;
        }

        public T[] SyncArray<T>(T[] self, T[] another, bool AllowExtraSize = true)// where T : class, ISync
        {
            if (AllowExtraSize)
            {
                if (another.Length > self.Length)
                {
                    Array.Resize(ref self, another.Length);
                }
            }
            else
            {
                if (another.Length != self.Length)
                {
                    Array.Resize(ref self, another.Length);
                }
            }
            if (self is ISync[] ss && another is ISync[] sa)
            {
                int i = 0;
                for (; i < sa.Length; i++)
                {
                    self[i] = (T)SyncFrom((ISync)self[i], sa[i]);
                }
                for (; i < ss.Length; i++)
                {
                    self[i] = (T)SyncFrom((ISync)self[i], null);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return self;
        }

        public T[] EnsureArraySize<T>(ref T[] self, int len, bool AllowExtraSize = true)
        {
            if (AllowExtraSize)
            {
                if (len > self.Length)
                {
                    Array.Resize(ref self, len);
                }
            }
            else
            {
                if (len != self.Length)
                {
                    Array.Resize(ref self, len);
                }
            }
            return self;
        }

        public T[] SyncUnmanagedArray<T>(T[] self, T[] another, bool AllowExtraSize = true)
        {
            if (AllowExtraSize)
            {
                if (another.Length > self.Length)
                {
                    Array.Resize(ref self, another.Length);
                }
            }
            else
            {
                if (another.Length != self.Length)
                {
                    Array.Resize(ref self, another.Length);
                }
            }
            Array.ConstrainedCopy(another, 0, self, 0, another.Length);
            return self;
        }

        public T SyncFrom<T>(T dst, T src) where T : class, ISync
        {
            SyncFrom(ref dst, src);
            return dst;
        }

        public void SyncFrom<T>(ref T dst, T src) where T : class, ISync
        {
            if ((dst ?? src) is null) return;

            if (src is null)
            {
                // return to pool
                if (!DLinked.Contains(dst))
                {
                    // TODO: remove references
                    AddToPool(DFree, dst);
                }
                dst = null;
                return;
            }

            // Assert src != null
            Assert.IsTrue(src != null);
            var type = src.GetType();

            if (S2D.TryGetValue(src, out var v))
            {
                dst = v as T;
                return;
            }

            // we need a new dst
            if (dst is null || DLinked.Contains(dst))
            {

                // return from pool
                if (!RentFromPool<T>(DFree, out dst))
                {
                    // create a new one
                    if (src is ISync ss1)
                    {
                        Profiler.BeginSample("new inst");
                        dst = ss1.CreateSimilar(this) as T;
                        Profiler.EndSample();
                    }
                    else if (typeHandlers.TryGetValue(type, out var handler))
                    {
                        dst = handler.creator(src) as T;
                    }
                    else
                    {
                        throw new NotImplementedException($"no handler for type {type}");
                    }
                }
            }

            // Assert src != null && dst !=null
            DLinked.Add(dst);
            RemoveFromPool(DFree, dst);
            S2D[src] = dst;

            // sync
            if (dst is ISync ds && src is ISync ss)
            {
                ds.SyncFrom(ss, this);
            }
            else if (typeHandlers.TryGetValue(type, out var handler))
            {
                handler.sync(dst, src, this);
            }
            else
            {
                throw new NotImplementedException($"no handler for type {type}");
            }
        }

        static void RemoveFromPool(Dictionary<Type, HashSet<object>> DFree, object item)
        {
            if (DFree.TryGetValue(item.GetType(), out var set))
            {
                set.Remove(item);
            }
        }

        static bool RentFromPool<T>(Dictionary<Type, HashSet<object>> DFree, out T item) where T : class
        {
            var type = typeof(T);
            if (!DFree.TryGetValue(type, out var set))
            {
                item = default;
                return false;
            }

            if (set.Count == 0)
            {
                item = default;
                return false;
            }

            item = set.First() as T;
            set.Remove(item);
            return true;
        }

        static void AddToPool(Dictionary<Type, HashSet<object>> DFree, object dst)
        {
            var type = dst.GetType();
            if (!DFree.TryGetValue(type, out var set))
            {
                DFree[type] = set = new();
            }
            set.Add(dst);
        }
    }

    public sealed class StateAttribute : Attribute
    {
        public string Format { get; set; }
        public int Order { get; set; } = 0;
        public HandleIndex handleIndex { get; set; } = HandleIndex.UNSET;
    }
    public sealed class ManualStateAttribute : Attribute
    {
    }
}