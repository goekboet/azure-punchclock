using System;

namespace backend
{
    public sealed class PunchClockKey
    {
        public static PunchClockKey New(string userId)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            return new PunchClockKey(userId, Guid.NewGuid());
        }

        public PunchClockKey(
            string userId,
            Guid clockId
        )
        {
            UserId = userId;
            ClockId = clockId;
        }

        public string UserId { get; }
        public Guid ClockId { get; }

        public override bool Equals(object obj) =>
            obj is PunchClockKey k &&
            k.ClockId == ClockId &&
            k.UserId == UserId;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + UserId.GetHashCode();
                hash = hash * 23 + ClockId.GetHashCode();

                return hash;
            }
        }
    }

    public enum Status { Out = 0, In = 1 }

    public struct Punch
    {
        public Punch(
            long time,
            Status status
        )
        {
            Time = time;
            Status = status;
        }
        public long Time { get; }
        public Status Status { get; }
    }

    public sealed class PunchClock
    {
        public PunchClock(
            string userId,
            string punchClockId,
            Status status,
            long age,
            Punch[] punches)
        {
            UserId = userId;
            PunchClockId = punchClockId;
            Status = status;
            Age = age;
            Punches = punches;
        }
        public string UserId { get; }
        public string PunchClockId { get; }
        public Status Status { get; }
        public long Age { get; }

        public Punch[] Punches { get; }
    }
}