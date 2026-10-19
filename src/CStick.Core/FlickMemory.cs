namespace CStick
{
    public struct FlickMemory
    {
        // HeroController.ATTACK_QUEUE_STEPS. FixedUpdate increments it, so five steps is about a tenth of a second.
        public const int ShortQueueLimit = 5;

        public FlickDirection Previous;
        public FlickDirection ShortQueue;
        public int ShortQueueSteps;

        // A flick that arrived while the nail was cooling. One swing when the nail is free.
        public FlickDirection CooldownHold;

        public static void NoteFixedStep(ref FlickMemory memory)
        {
            if (memory.ShortQueue != FlickDirection.None)
            {
                memory.ShortQueueSteps++;
            }
        }

        public static FlickDirection Advance(
            ref FlickMemory memory,
            FlickDirection stick,
            bool tracking,
            bool inPlay,
            bool canBuffer,
            bool blocked,
            bool nailCooling,
            bool canSwing)
        {
            bool edge = StickFlick.IsNewFlick(memory.Previous, stick);
            FlickDirection swing = FlickDirection.None;

            if (!tracking || stick == FlickDirection.None || (memory.ShortQueue != FlickDirection.None && stick != memory.ShortQueue))
            {
                memory.ShortQueue = FlickDirection.None;
                memory.ShortQueueSteps = 0;
            }
            else if (edge && canBuffer)
            {
                memory.ShortQueue = stick;
                memory.ShortQueueSteps = 0;
                memory.CooldownHold = FlickDirection.None;
            }
            else if (edge && inPlay && canSwing)
            {
                swing = stick;
            }
            else if (edge && inPlay && nailCooling && !blocked)
            {
                memory.CooldownHold = stick;
            }

            if (!tracking || blocked || stick != memory.CooldownHold)
            {
                memory.CooldownHold = FlickDirection.None;
            }

            if (memory.ShortQueue != FlickDirection.None)
            {
                if (memory.ShortQueueSteps > ShortQueueLimit)
                {
                    memory.ShortQueue = FlickDirection.None;
                    memory.ShortQueueSteps = 0;
                }
                else if (swing == FlickDirection.None && canSwing)
                {
                    swing = memory.ShortQueue;
                    memory.ShortQueue = FlickDirection.None;
                    memory.ShortQueueSteps = 0;
                }
            }

            if (swing == FlickDirection.None && memory.CooldownHold != FlickDirection.None && !canBuffer && canSwing)
            {
                swing = memory.CooldownHold;
            }
            else if (memory.CooldownHold != FlickDirection.None && !nailCooling && !canBuffer && !canSwing)
            {
                memory.CooldownHold = FlickDirection.None;
            }

            if (swing != FlickDirection.None)
            {
                memory.CooldownHold = FlickDirection.None;
            }

            memory.Previous = stick;
            return swing;
        }
    }
}
