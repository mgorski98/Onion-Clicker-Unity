using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public Button button;
    public Image image;
    public Color originalIdleColor;
    public bool selected = false;
    public bool Selected { get => selected; set {
            selected = value;
            var colors = button.colors;
            colors.normalColor = selected ? button.colors.pressedColor : originalIdleColor;
            button.colors = colors;
            image.color = colors.normalColor;
        }
    }
    void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        originalIdleColor = button.colors.normalColor;
    }
}
