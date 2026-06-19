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
        Mock<IValveControl> mockValveControl = new();
        Mock<IPressureSensor> mockPressureSensor = new();
        Mock<ILogger<ReactorSystem>> mockLogger = new();
        ReactorSystem reactorSystem = new(
            mockValveControl.Object,
            mockPressureSensor.Object,
            mockLogger.Object
        );

        // Act
        mockPressureSensor.Raise(m => m.PressureChanged += null, 0.72f);

        // Assert
        mockValveControl.Verify(m => m.Open(), Times.Once);
    }

    [Fact]
    public void HandlePressureChange_PressureBelowThreshold_ClosesValve()
    {
        // Arrange
        Mock<IValveControl> mockValveControl = new();
        mockValveControl.Setup(x => x.IsOpen()).Returns(true);
        Mock<IPressureSensor> mockPressureSensor = new();
        Mock<ILogger<ReactorSystem>> mockLogger = new();
        ReactorSystem reactorSystem = new(
            mockValveControl.Object,
            mockPressureSensor.Object,
            mockLogger.Object
        );

        // Act
        mockPressureSensor.Raise(m => m.PressureChanged += null, 0.56f);

        // Assert
        mockValveControl.Verify(m => m.Close(), Times.Once);
    }

    [Fact]
    public void TriggerEmergencyOpen_OpensValveAndDisablesAutomation()
    {
        // Arrange
        Mock<IValveControl> mockValveControl = new();
        Mock<IPressureSensor> mockPressureSensor = new();
        Mock<ILogger<ReactorSystem>> mockLogger = new();
        ReactorSystem reactorSystem = new(
            mockValveControl.Object,
            mockPressureSensor.Object,
            mockLogger.Object
        );

        // Act
        reactorSystem.TriggerEmergencyOpen();

        // Assert
        mockValveControl.Verify(m => m.Open(), Times.Once);
        Assert.False(reactorSystem.AutomationEnabled);
    }

    [Fact]
    public void HandlePressureChange_AutomationDisabled_DoesNotCloseValve()
    {
        // Arrange
        Mock<IValveControl> mockValveControl = new();
        mockValveControl.Setup(x => x.IsOpen()).Returns(true);
        Mock<IPressureSensor> mockPressureSensor = new();
        Mock<ILogger<ReactorSystem>> mockLogger = new();
        ReactorSystem reactorSystem = new(
            mockValveControl.Object,
            mockPressureSensor.Object,
            mockLogger.Object
        );
        reactorSystem.TriggerEmergencyOpen();

        // Act
        mockPressureSensor.Raise(m => m.PressureChanged += null, 0.10f);

        // Assert
        mockValveControl.Verify(m => m.Close(), Times.Never);
    }

    [Fact]
    public void RestartAutomation_ReEnablesAutomation()
    {
        // Arrange
        Mock<IValveControl> mockValveControl = new();
        mockValveControl.Setup(x => x.IsOpen()).Returns(true);
        Mock<IPressureSensor> mockPressureSensor = new();
        Mock<ILogger<ReactorSystem>> mockLogger = new();
        ReactorSystem reactorSystem = new(
            mockValveControl.Object,
            mockPressureSensor.Object,
            mockLogger.Object
        );
        reactorSystem.TriggerEmergencyOpen();

        // Act
        reactorSystem.RestartAutomation();
        mockPressureSensor.Raise(m => m.PressureChanged += null, 0.10f);

        // Assert
        Assert.True(reactorSystem.AutomationEnabled);
        mockValveControl.Verify(m => m.Close(), Times.Once);
    }
}
