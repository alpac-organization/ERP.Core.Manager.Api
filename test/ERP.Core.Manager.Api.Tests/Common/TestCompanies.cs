namespace ERP.Core.Manager.Api.Tests.Common;

/// <summary>
/// Las 5 empresas sembradas de forma determinista por ERP.Core.Testing.
/// El usuario por defecto (área TI de ALPAC) tiene perfil en todas ellas.
/// </summary>
public static class TestCompanies
{
    public static readonly Guid Alpac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Aminsa = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Avasa = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Vigemsa = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Tmn = Guid.Parse("55555555-5555-5555-5555-555555555555");

    /// <summary>Fuente de casos de prueba: una entrada por cada empresa.</summary>
    public static IEnumerable<TestCaseData> All()
    {
        yield return new TestCaseData(Alpac).SetArgDisplayNames("ALPAC");
        yield return new TestCaseData(Aminsa).SetArgDisplayNames("AMINSA");
        yield return new TestCaseData(Avasa).SetArgDisplayNames("AVASA");
        yield return new TestCaseData(Vigemsa).SetArgDisplayNames("VIGEMSA");
        yield return new TestCaseData(Tmn).SetArgDisplayNames("TMN");
    }
}
