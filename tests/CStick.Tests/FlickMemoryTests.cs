using Xunit;

namespace CStick.Tests
{
    public class FlickMemoryTests
    {
        [Fact]
        public void A_ready_flick_swings_once_and_a_held_stick_does_not_swing_again()
        {
            FlickMemory memory = default;
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.None, canSwing: true));
            Assert.Equal(FlickDirection.Right, Step(ref memory, FlickDirection.Right, canSwing: true));

            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, canSwing: true));
        }

        [Fact]
        public void A_flick_during_cooldown_swings_once_on_the_first_free_frame()
        {
            FlickMemory memory = default;
            Assert.Equal(FlickDirection.Up, Step(ref memory, FlickDirection.Up, canSwing: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.None, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Down, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Down, nailCooling: true));

            Assert.Equal(FlickDirection.Down, Step(ref memory, FlickDirection.Down, canSwing: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Down, canSwing: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Down, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Down, canSwing: true));
        }

        [Fact]
        public void Returning_below_the_deadzone_is_required_before_another_buffered_swing()
        {
            FlickMemory memory = default;
            Step(ref memory, FlickDirection.Left, nailCooling: true);
            Step(ref memory, FlickDirection.None, nailCooling: true);
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.None, canSwing: true));
            Assert.Equal(FlickDirection.Left, Step(ref memory, FlickDirection.Left, canSwing: true));
        }

        [Fact]
        public void Changing_direction_without_recentering_drops_the_cooldown_hold()
        {
            FlickMemory memory = default;
            Step(ref memory, FlickDirection.Up, nailCooling: true);
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, canSwing: true));
        }

        [Fact]
        public void A_block_drops_the_cooldown_hold()
        {
            FlickMemory memory = default;
            Step(ref memory, FlickDirection.Up, nailCooling: true);
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Up, blocked: true, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Up, canSwing: true));
        }

        [Fact]
        public void Leaving_gameplay_drops_the_cooldown_hold()
        {
            FlickMemory memory = default;
            Step(ref memory, FlickDirection.Right, nailCooling: true);
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, tracking: false, inPlay: false, nailCooling: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, canSwing: true));
        }

        [Fact]
        public void A_flick_at_the_start_of_a_dash_does_not_swing_when_the_dash_ends()
        {
            FlickMemory memory = default;
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, canBuffer: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, canBuffer: true, fixedSteps: 6));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Right, canSwing: true));
        }

        [Fact]
        public void A_flick_near_the_end_of_a_dash_swings_once_when_the_nail_is_free()
        {
            FlickMemory memory = default;
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Left, canBuffer: true));
            Assert.Equal(FlickDirection.Left, Step(ref memory, FlickDirection.Left, canBuffer: true, canSwing: true, fixedSteps: 4));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Left, canSwing: true));
        }

        [Fact]
        public void A_cooldown_hold_waits_through_a_dash_and_swings_once_afterward()
        {
            FlickMemory memory = default;
            Step(ref memory, FlickDirection.Up, nailCooling: true);
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Up, canBuffer: true, fixedSteps: 8));
            Assert.Equal(FlickDirection.Up, Step(ref memory, FlickDirection.Up, canSwing: true));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Up, canSwing: true));
        }

        [Fact]
        public void Death_and_other_closed_states_do_not_keep_a_flick()
        {
            FlickMemory memory = default;
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Down));
            Assert.Equal(FlickDirection.None, Step(ref memory, FlickDirection.Down, canSwing: true));
        }

        static FlickDirection Step(
            ref FlickMemory memory,
            FlickDirection stick,
            bool tracking = true,
            bool inPlay = true,
            bool canBuffer = false,
            bool blocked = false,
            bool nailCooling = false,
            bool canSwing = false,
            int fixedSteps = 0)
        {
            for (int i = 0; i < fixedSteps; i++)
            {
                FlickMemory.NoteFixedStep(ref memory);
            }

            return FlickMemory.Advance(ref memory, stick, tracking, inPlay, canBuffer, blocked, nailCooling, canSwing);
        }
    }
}
