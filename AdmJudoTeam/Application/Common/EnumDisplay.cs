using AdmJudoTeam.Core.Enums;
using AdmJudoTeam.Core.Models;

namespace AdmJudoTeam.Application.Common;
/// <summary>
/// Etiquetas en español para los enums del dominio (son texto visible de UI).
/// </summary>
public static class EnumDisplay
{
    public static string GetBeltLabel(BeltGrade grade) =>
        GetBeltOptions().FirstOrDefault(option => option.Value == grade).Label ?? grade.ToString();

    public static IReadOnlyList<(Gender Value, string Label)> GetGenderOptions() => new (Gender, string)[]
    {
        (Gender.Male, "Masculino"),
        (Gender.Female, "Femenino")
    };

    public static IReadOnlyList<(BeltGrade Value, string Label)> GetBeltOptions() => new (BeltGrade, string)[]
    {
        (BeltGrade.White, "Blanco"),
        (BeltGrade.Yellow, "Amarillo"),
        (BeltGrade.Orange, "Naranja"),
        (BeltGrade.Green, "Verde"),
        (BeltGrade.Blue, "Azul"),
        (BeltGrade.Brown, "Marrón"),
        (BeltGrade.Black1Dan, "Negro 1.er Dan"),
        (BeltGrade.Black2Dan, "Negro 2.º Dan"),
        (BeltGrade.Black3Dan, "Negro 3.er Dan"),
        (BeltGrade.Black4Dan, "Negro 4.º Dan"),
        (BeltGrade.Black5Dan, "Negro 5.º Dan"),
        (BeltGrade.Black6Dan, "Negro 6.º Dan")
    };

    public static IReadOnlyList<(WeightCategory Value, string Label)> GetMenWeightOptions() =>
        new (WeightCategory, string)[]
        {
            (WeightCategory.MenUnder60, WeightCategoryInfo.GetLabel(WeightCategory.MenUnder60)),
            (WeightCategory.MenUnder66, WeightCategoryInfo.GetLabel(WeightCategory.MenUnder66)),
            (WeightCategory.MenUnder73, WeightCategoryInfo.GetLabel(WeightCategory.MenUnder73)),
            (WeightCategory.MenUnder81, WeightCategoryInfo.GetLabel(WeightCategory.MenUnder81)),
            (WeightCategory.MenUnder90, WeightCategoryInfo.GetLabel(WeightCategory.MenUnder90)),
            (WeightCategory.MenUnder100, WeightCategoryInfo.GetLabel(WeightCategory.MenUnder100)),
            (WeightCategory.MenOver100, WeightCategoryInfo.GetLabel(WeightCategory.MenOver100))
        };

    public static IReadOnlyList<(WeightCategory Value, string Label)> GetWomenWeightOptions() =>
        new (WeightCategory, string)[]
        {
            (WeightCategory.WomenUnder48, WeightCategoryInfo.GetLabel(WeightCategory.WomenUnder48)),
            (WeightCategory.WomenUnder52, WeightCategoryInfo.GetLabel(WeightCategory.WomenUnder52)),
            (WeightCategory.WomenUnder57, WeightCategoryInfo.GetLabel(WeightCategory.WomenUnder57)),
            (WeightCategory.WomenUnder63, WeightCategoryInfo.GetLabel(WeightCategory.WomenUnder63)),
            (WeightCategory.WomenUnder70, WeightCategoryInfo.GetLabel(WeightCategory.WomenUnder70)),
            (WeightCategory.WomenUnder78, WeightCategoryInfo.GetLabel(WeightCategory.WomenUnder78)),
            (WeightCategory.WomenOver78, WeightCategoryInfo.GetLabel(WeightCategory.WomenOver78))
        };
}
