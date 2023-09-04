using NukeSharp.Services;

namespace NukeSharpTests
{
    public class ValveControlTests
    {

        [Fact]
        public void Open_Is_Not_Instant()
        {
            // Arrange
            var valve = new ValveControl();

            // Act
            valve.Open();

            // Assert
            Assert.False(valve.IsOpen()); // Should still be closed immediately after opening is triggered
        }

        [Fact]
        public async Task OpenAndCloseTest()
        {
            var valve = new ValveControl();

            valve.Open();
            await Task.Delay(1000); // Wait for 1 second
            valve.Close(); // Should not affect the valve

            await Task.Delay(1200); // Wait for more than remaining time
            Assert.True(valve.IsOpen()); // Should be open because the close command should be ignored

            valve.Close(); // Should affect the valve as the valve is inactive
            await Task.Delay(2100); // Wait for more than 2 seconds
            Assert.False(valve.IsOpen()); // Should be closed now
        }

    }
}