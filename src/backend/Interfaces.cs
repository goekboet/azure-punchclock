namespace backend
{
    public interface IPunchClockRepository
    {
        PunchClockState[] List();
        PunchClock Get(PunchClockKey key);
        PunchClockState Punch(PunchClockKey key);
        void Delete(PunchClockKey key);
    }

    public sealed class PunchClockState
    {
        public PunchClockState(
            string userId,
            string punchClockId,
            Status status,
            long age)
        {
            UserId = userId;
            PunchClockId = punchClockId;
            Status = status;
            Age = age;
        }

        public string UserId { get; }
        public string PunchClockId { get; }
        public Status Status { get; }
        public long Age { get; }
    }
}