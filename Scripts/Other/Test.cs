using Godot;
using System;
using System.Collections.Generic;

public partial class Test : LineEdit
{
    [Export]
    LabelContainer nlc;

	public override void _Ready()
	{
		TextSubmitted += OnTextSubmitted;
	}

	private void OnTextSubmitted(string str)
	{
        int amount = 0;
        if (int.TryParse(str, out amount))
        {
            if (nlc != null)
            {
                List<string> stringList = new List<string>();
                for (int i = 0; i < amount; i++)
                {
                    stringList.Add((i + 1).ToString());
                }
                
                nlc.UpdateDatas(stringList);
            }
        }
    }
}
