using UnityEngine;
using UnityEngine.UI;

public class GraphicColourTint : MonoBehaviour
{
    [SerializeField] private Graphic _graphic;

    [SerializeField] private Color normalColour = new Color32(255, 255, 255, 255);
    [SerializeField] private Color highlightedColour = new Color32(245, 245, 245, 255);
    [SerializeField] private Color pressedColour = new Color32(200, 200, 200, 255);
    [SerializeField] private Color selectedColour = new Color32(200, 200, 200, 255);
    [SerializeField] private Color disabledColour = new Color32(255, 255, 255, 128);

    //Matches Unity's SelectionState, which is inaccessible outside of the Selectable
    public enum State
    {
        Normal = 0,
        Highlighted = 1,
        Pressed = 2,
        Selected = 3,
        Disabled = 4
    }

    public void SetState(int state)
    {
        switch(state)
        {
            case ((int)State.Highlighted):
                {
                    SetColour(highlightedColour);
                    break;
                }
            case ((int)State.Pressed):
                {
                    SetColour(pressedColour);
                    break;
                }
            case ((int)State.Selected):
                {
                    SetColour(selectedColour);
                    break;
                }
            case ((int)State.Disabled):
                {
                    SetColour(disabledColour);
                    break;
                }
            case ((int)State.Normal):
            default:
                {
                    SetColour(normalColour);
                    break;
                }
        }
    }

    private void SetColour(Color color)
    {
        if (_graphic != null)
        {
            _graphic.color = color;
        }
    }
}
