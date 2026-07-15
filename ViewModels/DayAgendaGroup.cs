using System.Globalization;

namespace Beauty_Salon.ViewModels;

// A CollectionView group: a List<AgendaEntry> IS the group's items, plus a header
// label built from Date - the standard MAUI CollectionView grouping shape.
public sealed class DayAgendaGroup : List<AgendaEntry>
{
    private static readonly CultureInfo SpanishCulture = new("es-CL");

    public DayAgendaGroup(DateOnly date, IEnumerable<AgendaEntry> entries) : base(entries)
    {
        Date = date;
    }

    public DateOnly Date { get; }

    public string DayLabel
    {
        get
        {
            var dayName = SpanishCulture.TextInfo.ToTitleCase(Date.ToString("dddd", SpanishCulture));
            return $"{dayName} {Date:dd/MM}";
        }
    }
}
