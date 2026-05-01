using Land_Readjustment_Tool.Services;
using Xunit;

namespace LandReadjustment.Tests;

public class AreaConverterServiceTests
{
    [Fact]
    public void SqmToRAPD_1000Sqm_ReturnsCorrectComponents()
    {
        var (ropani, aana, paisa, dam) = AreaConverterService.SqmToRAPDComponents(1000);

        Assert.Equal(1, ropani);
        Assert.Equal(15, aana);
        Assert.True(dam >= 0, "Dam should be non-negative");

        double reconstructed =
            ropani * 508.73704704 +
            aana * 31.79606544 +
            paisa * 7.94901636 +
            dam * 1.98725409;

        Assert.InRange(reconstructed, 999.0, 1001.0);
    }

    [Fact]
    public void SqmToBKD_6773Sqm_ReturnsApproximatelyOneBigha()
    {
        var (bigha, kattha, dhur) = AreaConverterService.SqmToBKDComponents(6772.631616);

        Assert.Equal(1, bigha);
        Assert.Equal(0, kattha);
        Assert.InRange(dhur, -0.01, 0.01);
    }

    [Fact]
    public void RoundTrip_SqmToRopaniAndBack_PreservesPrecision()
    {
        double original = 1234.56;
        double ropani = AreaConverterService.SqmToRopani(original);
        double backToSqm = AreaConverterService.RopaniToSqm(ropani);

        Assert.InRange(backToSqm, original - 1.0, original + 1.0);
    }
}
