using BeautySalon.Application.Features.Reports;

namespace Beauty_Salon.Controls.Charts;

// Minimalist sparkline-style histogram for "busiest hours" - a GraphicsView + IDrawable
// scaling each hour's bar to the max count in the bound DTO list, no charting package added.
public sealed class HourHistogramView : GraphicsView
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IReadOnlyList<HourCountDto>), typeof(HourHistogramView),
        propertyChanged: OnItemsSourceChanged);

    public IReadOnlyList<HourCountDto>? ItemsSource
    {
        get => (IReadOnlyList<HourCountDto>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public HourHistogramView()
    {
        Drawable = new HistogramDrawable(this);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((HourHistogramView)bindable).Invalidate();

    private sealed class HistogramDrawable(HourHistogramView owner) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var items = owner.ItemsSource;
            if (items is null || items.Count == 0)
                return;

            var maxCount = items.Max(i => i.Count);
            if (maxCount <= 0)
                return;

            const float gap = 4;
            var barWidth = (dirtyRect.Width - gap * (items.Count - 1)) / items.Count;
            canvas.FillColor = Color.FromArgb("#2563EB");

            for (var i = 0; i < items.Count; i++)
            {
                var barHeight = (float)(items[i].Count / (double)maxCount) * dirtyRect.Height;
                var x = dirtyRect.X + i * (barWidth + gap);
                var y = dirtyRect.Height - barHeight;
                canvas.FillRoundedRectangle(x, y, barWidth, Math.Max(barHeight, 2), 2);
            }
        }
    }
}
