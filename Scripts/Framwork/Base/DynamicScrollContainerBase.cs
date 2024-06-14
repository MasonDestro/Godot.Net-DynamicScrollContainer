using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class DynamicScrollContainerBase : ScrollContainer
{
    [Export]
    Container container;
    [Export]
    PackedScene itemPrefab;

    [Export]
    bool isDynamicLoad;

    Node itemPoolNode;
    Control placeholder;


    enum ContainerType { vBox, hBox, grid, other }
    ContainerType containerType;
    int firstItemIndex = 1;


    Vector2 itemSize;
    Vector2I separation;

    Dictionary<int, DynamicContainerItemBase> itemsShowingRecord = new Dictionary<int, DynamicContainerItemBase>();
    Stack<DynamicContainerItemBase> itemObjectPool = new Stack<DynamicContainerItemBase>();

    int lastScrollVertical;
    int lastScrollHorizontal;


    int maxIndex;
    protected Action<int, DynamicContainerItemBase> OnItemShow;

    #region Godot Functions

    public override void _Ready()
    {
        if (container == null || itemPrefab == null)
        {
            GD.PrintErr($"container is null:{container == null}\nitem is null:{itemPrefab == null}");
            return;
        }

        if (container is VBoxContainer vbox)
        {
            containerType = ContainerType.vBox;
            GetVScrollBar().ValueChanged += ScrollBarValueChanged;
            separation = new Vector2I(0, vbox.GetThemeConstant("separation"));
        }
        else if (container is HBoxContainer hBox)
        {
            containerType = ContainerType.hBox;
            GetHScrollBar().ValueChanged += ScrollBarValueChanged;
            separation = new Vector2I(hBox.GetThemeConstant("separation"), 0);
        }
        else if (container is GridContainer grid)
        {
            containerType = ContainerType.grid;
            GetVScrollBar().ValueChanged += ScrollBarValueChanged;
            separation = new Vector2I(grid.GetThemeConstant("h_separation"), grid.GetThemeConstant("v_separation"));
            firstItemIndex = grid.Columns;
        }
        else
        {
            containerType = ContainerType.other;
        }


        if (itemPoolNode == null)
        {
            itemPoolNode = new Node();
            itemPoolNode.Name = "NodeObjectPool";
            AddChild(itemPoolNode);
        }

        for (int i = 0; i < firstItemIndex; i++)
        {
            placeholder = new Control();
            placeholder.Name = "Placeholder_"+ i;
            container.AddChild(placeholder);
        }
    }

    #endregion


    #region Public & Protected Functions

    public async void RefreshItems(int maxIndex)
    {
        if (container == null || itemPrefab == null)
        {
            GD.PrintErr($"container is null:{container == null}\nitem is null:{itemPrefab == null}");
            return;
        }

        ClearItems();

        this.maxIndex = maxIndex;

        for (int i = 0; i < maxIndex; i++)
        {
            DynamicContainerItemBase item = CreateItem();
            container.AddChild(item);

            OnItemShow?.Invoke(i, item);

            if (i == 0)
            {
                await ToSignal(GetTree(), "process_frame");
                itemSize = item.Size;
            }
            if (IsItemOutOfBound(i))
            {
                RemoveItem(item);

                if (isDynamicLoad)
                {
                    break;
                }

                switch (containerType)
                {
                    case ContainerType.vBox:
                        container.CustomMinimumSize = new Vector2(0, maxIndex * (itemSize.Y + separation.Y));
                        break;
                    case ContainerType.hBox:
                        container.CustomMinimumSize = new Vector2(maxIndex * (itemSize.X + separation.X), 0);
                        break;
                    case ContainerType.grid:
                        var RowCol = Math.DivRem(maxIndex, firstItemIndex);
                        int maxRow = RowCol.Quotient + (RowCol.Remainder == 0 ? 0 : 1);
                        //container.CustomMinimumSize = new Vector2(firstItemIndex * (itemSize.X + separation.X) - separation.X, row * (itemSize.Y + separation.Y));
                        container.CustomMinimumSize = new Vector2(0, maxRow * (itemSize.Y + separation.Y));
                        break;
                    default:
                        break;
                }

                break;
            }
            else
            {
                itemsShowingRecord.Add(i, item);
            }
        }

        lastScrollVertical = ScrollVertical;
        lastScrollHorizontal = ScrollHorizontal;
    }

    #endregion



    #region Private Functions

    private void ScrollBarValueChanged(double value)
    {
        if (isDynamicLoad && container.CustomMinimumSize < container.Size)
        {
            container.CustomMinimumSize = container.Size;
        }

        //  Remove Item
        foreach (var item in itemsShowingRecord)
        {
            if (IsItemOutOfBound(item.Key))
            {
                RemoveItem(item.Value);
                itemsShowingRecord.Remove(item.Key);
            }
        }

        //  Add Item
        bool scrollUp = ScrollVertical < lastScrollVertical;
        bool scrollLeft = ScrollHorizontal < lastScrollHorizontal;

        int indexHead, indexTail;
        GetShowingItemsIndexRange(out indexHead, out indexTail);

        for (int i = indexHead; i <= indexTail; i++)
        {
            if (itemsShowingRecord.ContainsKey(i)) { continue; }

            DynamicContainerItemBase item = CreateItem();
            container.AddChild(item);
            if (scrollUp || scrollLeft) { container.MoveChild(item, i - indexHead + firstItemIndex); }

            itemsShowingRecord.Add(i, item);

            OnItemShow?.Invoke(i, item);
        }

        switch (containerType)
        {
            case ContainerType.vBox:
                placeholder.CustomMinimumSize = new Vector2(0, itemsShowingRecord.Keys.Min() * (itemSize.Y + separation.Y));
                break;
            case ContainerType.hBox:
                placeholder.CustomMinimumSize = new Vector2(itemsShowingRecord.Keys.Min() * (itemSize.X + separation.X), 0);
                break;
            case ContainerType.grid:
                var RowCol = Math.DivRem(itemsShowingRecord.Keys.Min(), firstItemIndex);
                placeholder.CustomMinimumSize = new Vector2(0, RowCol.Quotient * (itemSize.Y + separation.Y));
                break;
            default:
                break;
        }

        lastScrollVertical = ScrollVertical;
        lastScrollHorizontal = ScrollHorizontal;
    }


    private DynamicContainerItemBase CreateItem()
    {
        DynamicContainerItemBase item;
        if (!itemObjectPool.TryPop(out item))
        {
            item = itemPrefab.Instantiate<DynamicContainerItemBase>();
        }

        item.GetParent()?.RemoveChild(item);
        item.Visible = true;
        item.ProcessMode = ProcessModeEnum.Inherit;

        return item;
    }

    private void RemoveItem(DynamicContainerItemBase item)
    {
        itemObjectPool.Push(item);

        item.GetParent()?.RemoveChild(item);
        itemPoolNode.AddChild(item);
        item.Visible = false;
        item.ProcessMode = ProcessModeEnum.Disabled;
    }

    private void ClearItems()
    {
        ScrollHorizontal = 0;
        ScrollVertical = 0;

        container.CustomMinimumSize = Vector2.Zero;
        placeholder.CustomMinimumSize = Vector2.Zero;
        foreach (var item in itemsShowingRecord)
        {
            RemoveItem(item.Value);
        }

        itemsShowingRecord.Clear();
        maxIndex = 0;

        GC.Collect();
    }

    private bool IsItemOutOfBound(int index)
    {
        bool result = false;
        Vector2 position = Vector2.Zero;

        if (containerType == ContainerType.grid)
        {
            var RowCol = Math.DivRem(index, firstItemIndex);
            position.Y = RowCol.Quotient * (itemSize.Y + separation.Y);
            position.X = RowCol.Remainder * (itemSize.X + separation.X) - separation.X;
        }
        else
        {
            position = index * (itemSize + separation);
        }

        switch (containerType)
        {
            case ContainerType.vBox:
            case ContainerType.grid:
                result = (position.Y + separation.Y + itemSize.Y < ScrollVertical)
                    || (position.Y > ScrollVertical + Size.Y);
                break;
            case ContainerType.hBox:
                result = (position.X + separation.X + itemSize.X < ScrollHorizontal)
                    || (position.X > ScrollHorizontal + Size.X);
                break;
            default:
                break;
        }

        return result;
    }

    private void GetShowingItemsIndexRange(out int indexHead, out int indexTail)
    {
        indexHead = 0;
        indexTail = 0;

        switch (containerType)
        {
            case ContainerType.vBox:

                indexHead = (int)((ScrollVertical) / (itemSize.Y + separation.Y));

                indexTail = (int)((ScrollVertical + Size.Y) / (itemSize.Y + separation.Y));
                indexTail = Mathf.Min(indexTail, maxIndex - 1);

                break;
            case ContainerType.hBox:
                indexHead = (int)((ScrollHorizontal) / (itemSize.X + separation.X));

                indexTail = (int)((ScrollHorizontal + Size.X) / (itemSize.X + separation.X));
                indexTail = Mathf.Min(indexTail, maxIndex - 1);

                break;
            case ContainerType.grid:

                indexHead = (int)((ScrollVertical) / (itemSize.Y + separation.Y)) * firstItemIndex;

                indexTail = (int)((ScrollVertical + Size.Y) / (itemSize.Y + separation.Y) + 1) * firstItemIndex;
                indexTail = Mathf.Min(indexTail, maxIndex - 1);

                break;
            default:
                break;
        }
    }

    #endregion
}