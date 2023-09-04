using Microsoft.Extensions.Logging;
using NukeSharp.ControlSystem;
using NukeSharp.Services;

namespace NukeSharp.Tests;

public class ReactorSystemTests
{
    [Fact]
    public void HandlePressureChange_PressureAboveThreshold_OpensValve()
    {
        // Arrange
        var mockValveControl = new Mock<IValveControl>();
        var mockPressureSensor = new Mock<IPressureSensor>();
        var mockLogger = new Mock<ILogger<ReactorSystem>>();
        var reactorSystem = new ReactorSystem(mockValveControl.Object, mockPressureSensor.Object, mockLogger.Object);

        // Act
        mockPressureSensor.Raise(m => m.PressureChanged += null, 0.72f);

        // Assert
        mockValveControl.Verify(m => m.Open(), Times.Once);
    }

    [Fact]
    public void HandlePressureChange_PressureBelowThreshold_ClosesValve()
    {
        // Arrange
        var mockValveControl = new Mock<IValveControl>();
        mockValveControl.Setup(x => x.IsOpen()).Returns(true);
        var mockPressureSensor = new Mock<IPressureSensor>();
        var mockLogger = new Mock<ILogger<ReactorSystem>>();
        var reactorSystem = new ReactorSystem(mockValveControl.Object, mockPressureSensor.Object, mockLogger.Object);

        // Act
        mockPressureSensor.Raise(m => m.PressureChanged += null, 0.56f);

        // Assert
        mockValveControl.Verify(m => m.Close(), Times.Once);
    }
}
