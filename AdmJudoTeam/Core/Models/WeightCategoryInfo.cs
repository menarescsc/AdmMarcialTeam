using AdmJudoTeam.Core.Enums;

namespace AdmJudoTeam.Core.Models;

/// <summary>
/// Información asociada a una categoría de peso IJF: límite en kg y etiqueta de visualización.
/// </summary>
public static class WeightCategoryInfo
{
    public static double GetLimitKg(WeightCategory category) => category switch
    {
        WeightCategory.MenUnder60 => 60,
        WeightCategory.MenUnder66 => 66,
        WeightCategory.MenUnder73 => 73,
        WeightCategory.MenUnder81 => 81,
        WeightCategory.MenUnder90 => 90,
        WeightCategory.MenUnder100 => 100,
        WeightCategory.MenOver100 => double.PositiveInfinity,

        WeightCategory.WomenUnder48 => 48,
        WeightCategory.WomenUnder52 => 52,
        WeightCategory.WomenUnder57 => 57,
        WeightCategory.WomenUnder63 => 63,
        WeightCategory.WomenUnder70 => 70,
        WeightCategory.WomenUnder78 => 78,
        WeightCategory.WomenOver78 => double.PositiveInfinity,

        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Categoría de peso desconocida.")
    };

    public static string GetLabel(WeightCategory category) => category switch
    {
        WeightCategory.MenUnder60 => "-60 kg",
        WeightCategory.MenUnder66 => "-66 kg",
        WeightCategory.MenUnder73 => "-73 kg",
        WeightCategory.MenUnder81 => "-81 kg",
        WeightCategory.MenUnder90 => "-90 kg",
        WeightCategory.MenUnder100 => "-100 kg",
        WeightCategory.MenOver100 => "+100 kg",

        WeightCategory.WomenUnder48 => "-48 kg",
        WeightCategory.WomenUnder52 => "-52 kg",
        WeightCategory.WomenUnder57 => "-57 kg",
        WeightCategory.WomenUnder63 => "-63 kg",
        WeightCategory.WomenUnder70 => "-70 kg",
        WeightCategory.WomenUnder78 => "-78 kg",
        WeightCategory.WomenOver78 => "+78 kg",

        _ => category.ToString()
    };

    public static bool IsMenCategory(WeightCategory category) => category is
        WeightCategory.MenUnder60 or WeightCategory.MenUnder66 or WeightCategory.MenUnder73 or
        WeightCategory.MenUnder81 or WeightCategory.MenUnder90 or WeightCategory.MenUnder100 or
        WeightCategory.MenOver100;
}
