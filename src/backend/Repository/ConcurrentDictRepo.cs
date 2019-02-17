using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace backend.Repository
{
    public class PunchClockRepo : IPunchClockRepository
    {
        private static ConcurrentDictionary<PunchClockKey, Stack<Punch>> PunchClocks { get; } =
            new ConcurrentDictionary<PunchClockKey, Stack<Punch>>();

        public void Delete(PunchClockKey key)
        {
            PunchClocks.TryRemove(key, out var _);
        }

        public PunchClock Get(PunchClockKey key)
        {
            if (PunchClocks.TryGetValue(key, out var entries))
            {
                var last = entries.Peek();
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                return new PunchClock(
                    key.UserId,
                    key.ClockId.ToString(),
                    last.Status,
                    now - last.Time,
                    entries.ToArray()
                );
            }
            else
            {
                return null;
            }
        }

        public PunchClockState[] List()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return PunchClocks.Select(x => new PunchClockState(
                x.Key.UserId,
                x.Key.ClockId.ToString(),
                x.Value.Any() ? x.Value.Peek().Status : Status.Out,
                x.Value.Any() ? now - x.Value.Peek().Time : 0
            )).ToArray();
        }

        public PunchClockState Punch(PunchClockKey key)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var val = PunchClocks.AddOrUpdate(
                key,
                k => new Stack<Punch>(),
                (k, v) =>
                {
                    var last = v.Any() ? v.Peek() : default(Punch);
                    var next = new Punch(now, last.Status == Status.In ? Status.Out : Status.In);
                    v.Push(next);

                    return v;
                }
            );

            return new PunchClockState(
                key.UserId,
                key.ClockId.ToString(),
                val.Any() ? val.Peek().Status : Status.Out,
                0
            );
        }
    }
}