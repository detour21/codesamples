using System.Collections.Generic;
using UnityEngine;

public class ColorPicker : MonoBehaviour
{
	//TODO this will be a simple line of images that get turned on/off depending on how many color options there are. Currently limited to 8. Everything hard-coded but eventually this data would come from a Style class
	[SerializeField] List <ColorPickerButton> colorPickerButtons;
	
	public void Initialize (List<Color> colors) 
	{
		for (int i = 0; i < colorPickerButtons.Count; i++) 
		{
			if (i < colors.Count) 
			{
				colorPickerButtons[i].gameObject.SetActive(true);
				colorPickerButtons[i].Initialize(colors[i], this);
			}
			else 
			{
				colorPickerButtons[i].gameObject.SetActive(false);
			}
		}
	}
	
	public void ColorPressed (ColorPickerButton button) 
	{
		foreach (ColorPickerButton pickerButton in colorPickerButtons) 
		{
			pickerButton.Highlight(pickerButton == button);
		}
	}

}
