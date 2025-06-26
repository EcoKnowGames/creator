using UnityEngine;


[CreateAssetMenu(fileName = "SO_ColourPalette", menuName = "EcoKnow/ColourPaletteScriptableObject", order = 1)]
public class ColourPaletteObject : ScriptableObject
{
    [System.Serializable]
    public class ColourSwatch
    {
        public string name;
        public Color colour;

        public ColourSwatch(string n, Color col)
        {
            name = n;
            colour = col;
        }
    }

    [SerializeField] private ColourSwatch[] _colourList;
    public ColourSwatch[] Colours => _colourList;
}
