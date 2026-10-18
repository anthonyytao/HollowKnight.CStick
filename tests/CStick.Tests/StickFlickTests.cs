using Xunit;

namespace CStick.Tests
{
    public class StickFlickTests
    {
        [Theory]
        [InlineData(0f, 0f, 0f, 0f, FlickDirection.None)]
        [InlineData(0.49f, 0f, 0f, 0f, FlickDirection.None)]
        [InlineData(0f, 0.49f, 0f, 0f, FlickDirection.None)]
        [InlineData(0f, 0f, 0.49f, 0f, FlickDirection.None)]
        [InlineData(0f, 0f, 0f, 0.49f, FlickDirection.None)]
        [InlineData(0.49f, 0f, 0.49f, 0f, FlickDirection.None)]
        [InlineData(0.5f, 0f, 0f, 0f, FlickDirection.Up)]
        [InlineData(0f, 0.5f, 0f, 0f, FlickDirection.Down)]
        [InlineData(0f, 0f, 0.5f, 0f, FlickDirection.Left)]
        [InlineData(0f, 0f, 0f, 0.5f, FlickDirection.Right)]
        [InlineData(1f, 0f, 0f, 0f, FlickDirection.Up)]
        public void Deadzone_blocks_until_threshold(float up, float down, float left, float right, FlickDirection expected)
        {
            Assert.Equal(expected, StickFlick.FromMagnitudes(up, down, left, right));
        }

        [Fact]
        public void Custom_threshold_changes_the_deadzone()
        {
            Assert.Equal(FlickDirection.None, StickFlick.FromMagnitudes(0.4f, 0f, 0f, 0f, 0.41f));
            Assert.Equal(FlickDirection.Up, StickFlick.FromMagnitudes(0.4f, 0f, 0f, 0f, 0.4f));
        }

        [Theory]
        [InlineData(1f, 0f, 0f, 1f, FlickDirection.Up)]
        [InlineData(0f, 1f, 1f, 0f, FlickDirection.Down)]
        [InlineData(0.8f, 0f, 0f, 0.8f, FlickDirection.Up)]
        [InlineData(0f, 0.6f, 0.6f, 0f, FlickDirection.Down)]
        public void Tie_prefers_vertical(float up, float down, float left, float right, FlickDirection expected)
        {
            Assert.Equal(expected, StickFlick.FromMagnitudes(up, down, left, right));
        }

        [Theory]
        [InlineData(1f, 1f, 0f, 0f, FlickDirection.None)]
        [InlineData(0f, 0f, 1f, 1f, FlickDirection.None)]
        [InlineData(0.8f, 0.8f, 0.7f, 0.7f, FlickDirection.None)]
        [InlineData(1f, 1f, 0f, 0.6f, FlickDirection.Right)]
        [InlineData(1f, 0.2f, 1f, 0f, FlickDirection.Left)]
        [InlineData(0.2f, 0f, 0f, 1f, FlickDirection.Right)]
        public void Opposite_axes_cancel(float up, float down, float left, float right, FlickDirection expected)
        {
            Assert.Equal(expected, StickFlick.FromMagnitudes(up, down, left, right));
        }

        [Fact]
        public void IsNewFlick_requires_a_return_to_center()
        {
            Assert.True(StickFlick.IsNewFlick(FlickDirection.None, FlickDirection.Up));
            Assert.True(StickFlick.IsNewFlick(FlickDirection.None, FlickDirection.Left));

            Assert.False(StickFlick.IsNewFlick(FlickDirection.Up, FlickDirection.Up));
            Assert.False(StickFlick.IsNewFlick(FlickDirection.Right, FlickDirection.Right));
            Assert.False(StickFlick.IsNewFlick(FlickDirection.Up, FlickDirection.Down));
            Assert.False(StickFlick.IsNewFlick(FlickDirection.Left, FlickDirection.None));
            Assert.False(StickFlick.IsNewFlick(FlickDirection.None, FlickDirection.None));
        }

        [Fact]
        public void Holding_does_not_repeat_until_the_stick_returns_to_center()
        {
            FlickDirection previous = FlickDirection.None;
            Assert.True(StickFlick.IsNewFlick(previous, FlickDirection.Right));

            previous = FlickDirection.Right;
            Assert.False(StickFlick.IsNewFlick(previous, FlickDirection.Right));
            Assert.False(StickFlick.IsNewFlick(previous, FlickDirection.Up));

            previous = FlickDirection.None;
            Assert.True(StickFlick.IsNewFlick(previous, FlickDirection.Up));
        }
    }
}
