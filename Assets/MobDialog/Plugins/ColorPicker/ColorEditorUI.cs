using UnityEngine;
using UnityEngine.UI;
public class ColorEditorUI : MonoBehaviour
{
    [SerializeField] private ColorPicker colorPicker;
    [SerializeField] private Slider sliderR;
    [SerializeField] private Slider sliderG;
    [SerializeField] private Slider sliderB;
    [SerializeField] private InputField hexInput;
    private bool _isUpdatingUI;

    private void Start()
    {
        if (colorPicker != null)
        {
            colorPicker.onColorChanged += OnColorPickerChanged;
            OnColorPickerChanged(colorPicker.color);
        }
        sliderR.onValueChanged.AddListener(OnSliderChanged);
        sliderG.onValueChanged.AddListener(OnSliderChanged);
        sliderB.onValueChanged.AddListener(OnSliderChanged);
        hexInput.onValueChanged.AddListener(OnHexInputChanged);
        hexInput.onEndEdit.AddListener(OnHexInputEndEdit);
    }

    private void OnDestroy()
    {
        if (colorPicker != null)
            colorPicker.onColorChanged -= OnColorPickerChanged;
    }
    private void OnColorPickerChanged(Color color)
    {
        if (_isUpdatingUI) return;
        _isUpdatingUI = true;
        sliderR.SetValueWithoutNotify(color.r * 255f);
        sliderG.SetValueWithoutNotify(color.g * 255f);
        sliderB.SetValueWithoutNotify(color.b * 255f);

        string hex = ColorUtility.ToHtmlStringRGB(color);
        hexInput.SetTextWithoutNotify(hex);

        _isUpdatingUI = false;
    }
    private void OnSliderChanged(float _)
    {
        if (_isUpdatingUI) return;
        float r = sliderR.value / 255f;
        float g = sliderG.value / 255f;
        float b = sliderB.value / 255f;
        Color newColor = new Color(r, g, b, 1f);
        colorPicker.color = newColor;
    }

    private void OnHexInputChanged(string hex)
    {
        if (_isUpdatingUI) return;
        if (hex.Length == 6 && ColorUtility.TryParseHtmlString("#" + hex, out Color color))
        {
            colorPicker.color = color;
        }
    }
    private void OnHexInputEndEdit(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return;
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);
        while (hex.Length < 6)
            hex += "0";
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color color))
        {
            colorPicker.color = color;
        }
        else
        {
            OnColorPickerChanged(colorPicker.color);
        }
    }
}
