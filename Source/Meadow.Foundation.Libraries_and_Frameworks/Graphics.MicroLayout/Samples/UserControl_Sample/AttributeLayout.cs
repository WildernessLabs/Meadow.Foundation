using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HMI_Sample;

internal class AttributeLayout : AbsoluteLayout
{
    private readonly Label _nameLabel;
    private readonly Label _modifierLabel;
    private readonly Label _valueLabel;
    private readonly Box _valueBox;

    public AttributeLayout(string name, int left, int top, int value, int modifier)
        : base(left, top, 210, 60)
    {
        var smallFont = new Font12x16();
        var largeFont = new Font16x24();

        _nameLabel = new Label(0, 0, 150, this.Height / 2)
        {
            BackgroundColor = Color.Black,
            TextColor = Color.White,
            Font = smallFont,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Text = name
        };
        _modifierLabel = new Label(0, _nameLabel.Bottom, _nameLabel.Width, this.Height - _nameLabel.Height)
        {
            BackgroundColor = Color.White,
            TextColor = Color.Black,
            Font = smallFont,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _valueBox = new Box(_nameLabel.Right, 0, this.Width - _nameLabel.Width, this.Height)
        {
            ForegroundColor = Color.Black,
            IsFilled = false
        };
        _valueLabel = new Label(_valueBox.Left + 2, _valueBox.Top + 2, _valueBox.Width - 4, _valueBox.Height - 4)
        {
            BackgroundColor = Color.White,
            TextColor = Color.Black,
            Font = largeFont,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        this.Controls.Add(_nameLabel, _modifierLabel, _valueBox, _valueLabel);

        SetValue(value, modifier);
    }

    public void SetValue(int value, int modifier)
    {
        _valueLabel.Text = value.ToString();
        _modifierLabel.Text = $"modifier: {modifier:+#;-#;+0}";
    }
}
