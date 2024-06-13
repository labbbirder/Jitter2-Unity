# Jitter-Unity

:book: [English](./README.md) | [中文](./README.cn.md)

这是 [JitterPhysics2](https://github.com/notgiven688/jitterphysics2) 的修改版  [2.3.1](https://jitterphysics.com/docs/changelog#jitter-231-06-02-2024).

此修改使Jitter2支持Unity

额外功能:

- [x] 物理世界状态同步
- [ ] 物理世界状态序列化
- [ ] 确定性数学计算
- [x] 完善的Unity端组件类型
- [x] 兼容Unity默认的Collider Mask
- [x] 兼容Unity的constraints选项
- [x] Scene View下的Gizmos展示
- [x] 2D 模式
  - [x] 常用2D碰撞体
  - [x] 多边形碰撞体
  - [x] Tilemap碰撞体

## Why Jitter2

* 纯C#编写，接入简单
* 常访问字段在内存中紧凑存放
* Jitter代码可读性极高，能快速上手，并让物理系统处于掌控中
* 数据结构和算法高效

## Start With Sample Scene

运行两个World，两两之间可以同步状态

* 按小键盘加号，生成实体
* 按小键盘减号，随机删除实体
* 按S，记录当前世界状态
* 按R，重置世界状态