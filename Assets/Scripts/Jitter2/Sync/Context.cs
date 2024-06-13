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
        internal (bool needCreate, bool needSync) GetCache<T>(T dst, T src, out T res) where T : class
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

        internal void SetCache<T>(T dst, T src) where T : class
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

            SetCache(res, src);

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

            SetCache(res, src);
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
        public bool RaiseOnCreate;
        public void SyncFrom<T>(ref T dst, T src) where T : class, ISync
        {
            if (src is null)
            {
                // return to pool
                if (dst != null && !DLinked.Contains(dst))
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

                // if (RaiseOnCreate) throw new();
                // return from pool
                if (!RentFromPool<T>(DFree, type, out dst))
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
            Assert.IsTrue(item != null);
            if (DFree.TryGetValue(item.GetType(), out var set))
            {
                set.Remove(item);
            }
        }

        static bool RentFromPool<T>(Dictionary<Type, HashSet<object>> DFree,Type type, out T item) where T : class
        {
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
}