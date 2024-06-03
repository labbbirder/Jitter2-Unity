using System.Diagnostics;
using UnityEngine;

namespace Jitter2
{
    public static class Assert
    {
        [Conditional("JITTER_ASSERT")]
        [DebuggerHidden, HideInCallstack]
        public static void IsTrue(bool condition, string message = default)
        {
            if (!condition)
            {
                _ = 1;
            }
            UnityEngine.Assertions.Assert.IsTrue(condition, message);
        }
    }
}