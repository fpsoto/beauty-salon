using BeautySalon.Application.Features.Reports;

namespace Beauty_Salon.Controls.Charts;

// Minimalist donut chart for "revenue by payment method" - no charting package added,
// just a GraphicsView + IDrawable computing segment angles from the bound DTO list.
public sealed class DonutChartView : GraphicsView
{
    private static readonly Color[] Palette =
    [
        Color.FromArgb("#2563EB"),
        Color.FromArgb("#10B981"),
        Color.FromArgb("#F59E0B"),
        Color.FromArgb("#8B5CF6"),
        Color.FromArgb("#0EA5E9"),
        Color.FromArgb("#64748B")
    ];

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IReadOnlyList<RevenueByPaymentMethodDto>), typeof(DonutChartView),
        propertyChanged: OnItemsSourceChanged);

    public IReadOnlyList<RevenueByPaymentMethodDto>? ItemsSource
    {
        get => (IReadOnlyList<RevenueByPaymentMethodDto>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DonutChartView()
    {
        Drawable = new DonutDrawable(this);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((DonutChartView)bindable).Invalidate();

    public static Color ColorFor(int index) => Palette[index % Palette.Length];

    private sealed class DonutDrawable(DonutChartView owner) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var items = owner.ItemsSource;
            if (items is null || items.Count == 0)
                return;

            var total = items.Sum(i => (double)i.Amount);
            if (total <= 0)
                return;

            var center = new PointF(dirtyRect.Width / 2, dirtyRect.Height / 2);
            var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - 4;
            const float strokeWidth = 14;

            float startAngle = -90;
            for (var i = 0; i < items.Count; i++)
            {
                var sweep = (float)((double)items[i].Amount / total * 360);
                canvas.StrokeColor = ColorFor(i);
                canvas.StrokeSize = strokeWidth;
                canvas.StrokeLineCap = LineCap.Butt;
                canvas.DrawArc(
                    center.X - radius, center.Y - radius, radius * 2, radius * 2,
                    startAngle, startAngle + sweep, false, false);
                startAngle += sweep;
            }
        }
    }
}
