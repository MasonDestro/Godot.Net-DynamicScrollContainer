## What is this
A simple framework based on C# abstract class and interface.
## What does this do
To reduce usage, only those in viewport are instantiated and show data dynamically. Only support vBoxContainer (for now)
## How to use it
Inherit `DynamicContainerItemBase` and `IDynamicContainerItem<Any Custom Data Type>` for any Control Node.

Inherit `DynamicScrollContainerBase` and `IDynamicScrollContainer<Data Type of Item>` for ScrollContainer.

The Base calsses work for Nodes, the Interfaces work for data.
To know more about the framework on [Bilibili](https://www.bilibili.com/video/BV1Mr421A7Va)
