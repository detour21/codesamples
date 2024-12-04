using UnityEngine;
using UnityEngine.UI;

public class ColorPickerButton : MonoBehaviour
{
	[SerializeField] GameObject highlight;
	[SerializeField] Image image;
	private ColorPicker Picker;
	
	public void Initialize (Color color, ColorPicker picker)
	{
		Highlight(false);
		image.color = color;
		Picker = picker;
	}

	public void Highlight (bool state) 
	{
		highlight.SetActive(state);
	}
	
	public void ButtonPressed () 
	{
		Picker.ColorPressed(this);
	}
}
