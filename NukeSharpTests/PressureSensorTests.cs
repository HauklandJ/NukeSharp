using NukeSharp.Services;


namespace NukeSharp.Tests;

public class PressureSensorTests
{
    [Fact]
    public void GetValue_InitialValue_ReturnsInitialValue()
    {
        // Arrange
        PressureSensor sensor = new();

        // Act
        float result = sensor.GetValue();

        // Assert
        Assert.Equal(0.3f, result);
    }

    [Theory]
    [InlineData(true, 0.282f)]
    [InlineData(false, 0.309f)]
    public void Update_WithValveStatus_UpdatesPressureValue(bool isValveOpen, float expectedValue)
    {
        // Arrange
        PressureSensor sensor = new();

        // Act
        sensor.Update(isValveOpen);
        float result = sensor.GetValue();

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void Update_InvokesPressureChangedEvent()
    {
        // Arrange
        Mock<PressureSensor> mockSensor = new();
        float pressureChangedValue = 0f;
        mockSensor.Object.PressureChanged += (float pressure) => pressureChangedValue = pressure;

        // Act
        mockSensor.Object.Update(true);

        // Assert
        Assert.Equal(0.282f, pressureChangedValue);
    }

    [Fact]
    public void Pressure_SetValueAboveOne_ValueIsCappedToOne()
    {
        PressureSensor pressureSensor = new()
        {
            Pressure = 1.5f
        };

        Assert.Equal(1f, pressureSensor.Pressure);
    }
}