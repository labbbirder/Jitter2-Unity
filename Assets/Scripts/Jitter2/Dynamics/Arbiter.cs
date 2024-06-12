/*
 * Copyright (c) Thorben Linneweber and others
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using Jitter2.Sync;
using Jitter2.UnmanagedMemory;

namespace Jitter2.Dynamics
{
    /// <summary>
    /// Holds a reference to all contacts (maximum 4) between two shapes.
    /// </summary>
    public partial class Arbiter
    {
        internal static Stack<Arbiter> Pool = new();

        [State(IsLateReference = true)]
        public RigidBody Body1 = null!;
        [State(IsLateReference = true)]
        public RigidBody Body2 = null!;

        [State(handleIndex = HandleIndex.CONTACT_DATA)]
        public JHandle<ContactData> Handle;

        public override int GetHashCode()//TODO: new first ? init first?
        {
            return Body1.GetHashCode() ^ Body2.GetHashCode();
        }
    }

    /// <summary>
    /// Look-up key for stored <see cref="Arbiter"/>.
    /// </summary>
    public struct ArbiterKey : IEquatable<ArbiterKey>
    {
        public long Key1;
        public long Key2;

        public ArbiterKey(long key1, long key2)
        {
            Key1 = key1;
            Key2 = key2;
        }

        public bool Equals(ArbiterKey other)
        {
            return Key1 == other.Key1 && Key2 == other.Key2;
        }

        public override bool Equals(object obj)
        {
            return obj is ArbiterKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Key1 + 2281 * (int)Key2;
        }
    }
}