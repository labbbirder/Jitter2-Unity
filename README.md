# Jitter-Unity

:book: [English](./README.md) | [中文](./README.cn.md)

This is an edition from [JitterPhysics2](https://github.com/notgiven688/jitterphysics2), with a license of [MIT](https://github.com/notgiven688/jitterphysics2/blob/main/LICENSE), based on the latest release [2.3.1](https://jitterphysics.com/docs/changelog#jitter-231-06-02-2024).

This edition make it compatible with Unity3D, ~~with a few acceptable disadvantages in performance.~~ (just forget about it)

Additional features:

- [x] state load/restore (most for local machine)
- [x] more physics-world-authoring components in Unity editor side
- [x] Compatible with the default Unity collision layer mask
- [x] 2D mode

More features are in development:

- [ ] world serialization(most for remote machine)
- [ ] deterministic math (reside in specific branch)

## Why Jitter2

* Coded in pure C#, without extra effort to migrate from.
* Frequently accessed data is compact in memory compared to Jitter1, which benefits from spatial local optimization.
* Neat and easy to understand. Faster for users to put their hands to introduce more features, and make things under their control.
* Well optimized in data structure and algorithms.
