using UnityEngine;
using UnityEngine.UI;

public class MultiGraphicButton : Button
{
    private GraphicColourTint[] _graphics => this.GetComponentsInChildren<GraphicColourTint>();

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        switch (this.transition)
        {
            case Transition.ColorTint:
                SetStateForAllGraphics(state);
                break;
            case Transition.None:
                break;
            default:
                throw new System.NotSupportedException(string.Format("MultiGraphicButton does not support this transition type - {0}", this.transition));
        }
    }

    private void SetStateForAllGraphics(SelectionState state)
    {
        //Set all our tints
        foreach(GraphicColourTint graphic in _graphics)
        {
            graphic.SetState((int)state);
        }

        //Override standard ColorTint behaviour with our own
        Graphic localGraphic = this.GetComponent<Graphic>();
        if (localGraphic != null)
        {
            switch (state)
            {
                case (SelectionState.Highlighted):
                    {
                        localGraphic.color = colors.highlightedColor;
                        break;
                    }
                case (SelectionState.Pressed):
                    {
                        localGraphic.color = colors.pressedColor;
                        break;
                    }
                case (SelectionState.Selected):
                    {
                        localGraphic.color = colors.selectedColor;
                        break;
                    }
                case (SelectionState.Disabled):
                    {
                        localGraphic.color = colors.disabledColor;
                        break;
                    }
                case (SelectionState.Normal):
                default:
                    {
                        localGraphic.color = colors.normalColor;
                        break;
                    }
            }
        }
    }
}
