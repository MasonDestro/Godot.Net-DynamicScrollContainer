using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class Test : LineEdit
{
    [Export]
    LabelContainer nlcVBox;
    [Export]
    LabelContainer nlcHBox;
    [Export]
    LabelContainer nlcGrid;

	public override void _Ready()
	{
		TextSubmitted += OnTextSubmitted;
	}

	private void OnTextSubmitted(string str)
	{
        int amount = 0;
        if (int.TryParse(str, out amount))
        {
            List<string> stringList = new List<string>();
            for (int i = 0; i < amount; i++)
            {
                stringList.Add((i + 1).ToString());
            }

            if (nlcVBox != null)
            {
                nlcVBox.UpdateDatas(stringList);
            }
            if (nlcHBox != null)
            {
                nlcHBox.UpdateDatas(stringList);
            }
            if (nlcGrid != null)
            {
                nlcGrid.UpdateDatas(stringList);
            }

            ReleaseFocus();
        }
    }
}
