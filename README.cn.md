# Jitter-Unity

:book: [English](./README.md) | [中文](./README.cn.md)

这是 [JitterPhysics2](https://github.com/notgiven688/jitterphysics2) 的修改版  [2.3.1](https://jitterphysics.com/docs/changelog#jitter-231-06-02-2024).

此修改使Jitter2支持Unity

额外功能:

- [x] 物理世界状态同步
- [ ] 物理世界状态序列化
- [ ] 确定性数学计算
- [x] 丰富的Unity端组件类型
- [x] 兼容Unity默认的Collider Mask
- [x] 兼容Unity的constraints选项
- [x] Scene View下的Gizmos展示
- [x] 2D 模式
  - [x] 常用2D碰撞体
  - [x] 多边形碰撞体
  - [x] Tilemap碰撞体

## Why Jitter2

* Coded in pure C#, without extra effort to migrate from.
* Frequently accessed data is compact in memory compared to Jitter1, which benefits from spatial local optimization.
* Neat and easy to understand. Faster for users to put their hands to introduce more features, and make things under their control.
* Well optimized in data structure and algorithms.
