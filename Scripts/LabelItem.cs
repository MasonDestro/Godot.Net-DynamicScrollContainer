using Godot;
using System;

public partial class LabelItem : DynamicContainerItemBase, IDynamicContainerItem<string>
{
    [Export]
    Label textLabel;

    string data;

    public string Data => data;

    public void UpdateData(string data)
    {
        this.data = data;
        textLabel.Text = data;
    }
}
