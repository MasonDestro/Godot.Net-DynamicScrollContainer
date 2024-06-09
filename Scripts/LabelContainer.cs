using Godot;
using System;
using System.Collections.Generic;

public partial class LabelContainer : DynamicScrollContainerBase, IDynamicScrollContainer<string>
{
    List<string> datas;

    public override void _Ready()
    {
        base._Ready();
        base.OnItemShow = OnItemShowAction;
    }

    public void UpdateDatas(List<string> datas)
    {
        this.datas = datas;
        RefreshItems(datas.Count);
    }

    private void OnItemShowAction(int index, DynamicContainerItemBase itemBase)
    {
        if(index<datas.Count)
        {
            ((IDynamicContainerItem<string>)itemBase).UpdateData(datas[index]);
        }
    }
}
