using UnityEngine;
using UnityEngine.UI;

public class MultiGraphicButton : Button
{

    private GraphicColourTint[] _graphics => this.GetComponentsInChildren<GraphicColourTint>();

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        SetStateForAllGraphics(state);
    }

    private void SetStateForAllGraphics(SelectionState state)
    {
        foreach(GraphicColourTint graphic in _graphics)
        {
            graphic.SetState((int)state);
        }
    }
}
