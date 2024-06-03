# Jitter-Unity

This is an edition from [JitterPhysics2](https://github.com/notgiven688/jitterphysics2), with a license of [MIT](https://github.com/notgiven688/jitterphysics2/blob/main/LICENSE).

This edition make it compatible with Unity3D, ~~with a few acceptable disadvantages in performance.~~ (just forget about it)

More features are in development:

* state load/restore (most for local machine)
* world serialization(most for remote machine)
* deterministic math (reside in specific branch)
* more physics-world-authoring components in Unity editor side


## Why Jitter2

* Coded in pure C#, without extra effort to migrate from.
* Frequently accessed data is compact in memory compared to Jitter1, which benefits from spatial local optimization.
* Neat and easy to understand. Faster for users to put their hands to introduce more features, and make things under their control.
* Well optimized in data structure and algorithms.
