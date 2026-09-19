namespace Billow.Gst;

/// <summary>A GST state or union territory, identified by its two-digit GST state code.</summary>
public sealed record GstState(string Code, string Name)
{
    /// <summary>
    /// The current GST state and UT codes. Retired codes are left out: 25 (Daman and Diu, merged
    /// into 26 in 2020) and 28 (Andhra Pradesh before its division, now 37).
    /// </summary>
    public static IReadOnlyList<GstState> All { get; } =
    [
        new("01", "Jammu and Kashmir"),
        new("02", "Himachal Pradesh"),
        new("03", "Punjab"),
        new("04", "Chandigarh"),
        new("05", "Uttarakhand"),
        new("06", "Haryana"),
        new("07", "Delhi"),
        new("08", "Rajasthan"),
        new("09", "Uttar Pradesh"),
        new("10", "Bihar"),
        new("11", "Sikkim"),
        new("12", "Arunachal Pradesh"),
        new("13", "Nagaland"),
        new("14", "Manipur"),
        new("15", "Mizoram"),
        new("16", "Tripura"),
        new("17", "Meghalaya"),
        new("18", "Assam"),
        new("19", "West Bengal"),
        new("20", "Jharkhand"),
        new("21", "Odisha"),
        new("22", "Chhattisgarh"),
        new("23", "Madhya Pradesh"),
        new("24", "Gujarat"),
        new("26", "Dadra and Nagar Haveli and Daman and Diu"),
        new("27", "Maharashtra"),
        new("29", "Karnataka"),
        new("30", "Goa"),
        new("31", "Lakshadweep"),
        new("32", "Kerala"),
        new("33", "Tamil Nadu"),
        new("34", "Puducherry"),
        new("35", "Andaman and Nicobar Islands"),
        new("36", "Telangana"),
        new("37", "Andhra Pradesh"),
        new("38", "Ladakh"),
        new("97", "Other Territory"),
    ];

    /// <summary>The state with this code, or null if it is not a current GST state code.</summary>
    public static GstState? Find(string code) => All.FirstOrDefault(s => s.Code == code);

    /// <summary>How the state is shown in lists, e.g. "27 – Maharashtra".</summary>
    public override string ToString() => $"{Code} – {Name}";
}
