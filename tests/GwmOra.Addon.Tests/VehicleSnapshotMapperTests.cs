using GwmOra.Addon.Gwm;
using libgwmapi.DTO.Vehicle;

namespace GwmOra.Addon.Tests;

public class VehicleSnapshotMapperTests
{
    [Fact]
    public void MapConvertsKnownVehicleValues()
    {
        var vehicle = new Vehicle
        {
            Vin = "VIN123",
            AppShowSeriesName = "ORA",
            BrandName = "GWM",
            Vtype = "Funky Cat"
        };
        var status = new VehicleStatus
        {
            AcquisitionTime = 1_700_000_000_000,
            UpdateTime = 1_700_000_100_000,
            DeviceId = "device",
            Latitude = 32.1,
            Longitude = 34.8,
            Items =
            [
                Item("2013021", 80, "%"),
                Item("2011501", 210, "km"),
                Item("2103010", 12345, "km"),
                Item("2201001", 234, "C"),
                Item("2202001", 1, null),
                Item("2208001", 0, null),
                Item("2210001", 1, null),
                Item("2042082", 1, null)
            ]
        };
        var basics = new VehicleBasicsInfo
        {
            Config = new VehicleConfig { AirConditionerTemperature = "23" }
        };

        var snapshot = VehicleSnapshotMapper.Map(vehicle, status, basics, true, "ok");

        Assert.Equal("VIN123", snapshot.Vin);
        Assert.Equal(80, snapshot.Values.Soc);
        Assert.Equal(210, snapshot.Values.RangeKm);
        Assert.Equal(12345, snapshot.Values.OdometerKm);
        Assert.Equal(23.4, snapshot.Values.InteriorTemperatureC);
        Assert.True(snapshot.Values.AcActive);
        Assert.True(snapshot.Values.Locked);
        Assert.False(snapshot.Values.WindowFrontLeftOpen);
        Assert.True(snapshot.Values.ChargePlugConnected);
        Assert.Equal("cool", snapshot.Climate.Mode);
        Assert.Equal(23, snapshot.Climate.TargetTemperatureC);
        Assert.NotNull(snapshot.Location);
        Assert.True(snapshot.Capabilities.RemoteCommands);
        Assert.Equal("80", snapshot.RawItems["2013021"].Value);
    }

    private static VehicleStatusItems Item(string code, object value, string? unit)
    {
        return new VehicleStatusItems
        {
            Code = code,
            Value = value,
            Unit = unit
        };
    }
}
