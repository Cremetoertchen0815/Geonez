using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;

namespace Nez.UI;

public class NumberField : Table, IInputListener
{
    private readonly TextButton decrease;
    private readonly TextField field;
    private readonly TextButton increase;
    private float maximum;
    private float minimum;
    private float number;
    private float step = 0.1f;

    public NumberField(float initial, float min, float max, float step, bool showButtons, NumberFieldStyle style)
    {
        Defaults().Space(0);
        SetMin(min);
        SetMax(max);
        SetStep(step);

        field = new TextField(initial.ToString(CultureInfo.InvariantCulture), style);
        field.SetAlignment(UI.Align.Center);
        SetNumber(initial);

        if (showButtons)
        {
            decrease = new TextButton("", style.DecreaseButtonStyle);

            increase = new TextButton("", style.IncreaseButtonStyle);
            increase.OnClicked += _ => { IncreaseNumber(); };

            decrease.OnClicked += _ => { DecreaseNumber(); };
        }

        field.OnTextChanged += (_, s) =>
        {
            if (float.TryParse(s, out var n))
                SetNumber(n >= maximum ? maximum : n);
            else
                SetNumber(minimum);
        };

        if (showButtons)
            Add(decrease);

        Add(field).Fill().Expand();

        if (showButtons)
            Add(increase);

        //setSize(preferredWidth, preferredHeight);
    }

    public NumberField(float initial, float min, float max, float step, bool showButtons, Skin skin,
        string styleName = null) : this(initial, min, max, step, showButtons, skin.Get<NumberFieldStyle>(styleName))
    {
    }

    public void OnMouseEnter()
    {
    }

    public void OnMouseExit()
    {
    }

    public bool OnMousePressed(Vector2 mousePos)
    {
        return false;
    }

    public void OnMouseMoved(Vector2 mousePos)
    {
    }

    public void OnMouseUp(Vector2 mousePos)
    {
    }

    public bool OnMouseScrolled(int mouseWheelDelta)
    {
        if (mouseWheelDelta > 0)
            IncreaseNumber();
        else
            DecreaseNumber();

        return true;
    }

    public event Action<NumberField, float> OnNumberChanged = delegate { };

    private void IncreaseNumber()
    {
        if (number + step > maximum)
            SetNumber(maximum);
        else
            SetNumber(Mathf.RoundToNearest(number + step, step));
    }

    private void DecreaseNumber()
    {
        if (number - step < minimum)
            SetNumber(minimum);
        else
            SetNumber(Mathf.RoundToNearest(number - step, step));
    }

    public TextButton GetDecreaseButton()
    {
        return decrease;
    }

    public TextButton GetIncreaseButton()
    {
        return increase;
    }

    public TextField GetTextField()
    {
        return field;
    }

    public Cell GetDecreaseButtonCell()
    {
        return GetCell(decrease);
    }

    public Cell GetIncreaseButtonCell()
    {
        return GetCell(increase);
    }

    public Cell GetNumberFieldCell()
    {
        return GetCell(field);
    }

    public void SetNumber(float value)
    {
        field.SetTextForced(value.ToString(CultureInfo.InvariantCulture));
        number = value;

        OnNumberChanged(this, value);
    }

    public float GetNumber()
    {
        return number;
    }

    public void SetMax(float max)
    {
        maximum = max;
    }

    public void SetStep(float value)
    {
        step = value;
    }

    public void SetMin(float min)
    {
        minimum = min;
    }
}

public class NumberFieldStyle : TextFieldStyle
{
    public TextButtonStyle DecreaseButtonStyle;
    public IDrawable ImageUp, ImageDown, ImageOver, ImageChecked, ImageCheckedOver, ImageDisabled;
    public TextButtonStyle IncreaseButtonStyle;

    public NumberFieldStyle()
    {
        Font = Graphics.Instance.BitmapFont;
    }


    public NumberFieldStyle(BitmapFont font, Color fontColor, IDrawable cursor, IDrawable selection,
        IDrawable background, TextButtonStyle decreaseButtonStyle, TextButtonStyle increaseButtonStyle) : base(font,
        fontColor, cursor, selection, background)
    {
        DecreaseButtonStyle = decreaseButtonStyle;
        IncreaseButtonStyle = increaseButtonStyle;
    }


    public new TextFieldStyle Clone()
    {
        return new TextFieldStyle
        {
            Font = Font,
            FontColor = FontColor,
            FocusedFontColor = FocusedFontColor,
            DisabledFontColor = DisabledFontColor,
            Background = Background,
            FocusedBackground = FocusedBackground,
            DisabledBackground = DisabledBackground,
            Cursor = Cursor,
            Selection = Selection,
            MessageFont = MessageFont,
            MessageFontColor = MessageFontColor
        };
    }
}