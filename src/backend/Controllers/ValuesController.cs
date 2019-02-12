using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
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

    public interface IPunchClockRepository
    {
        PunchClockState[] List();
        PunchClock Get(PunchClockKey key);
        PunchClockState Punch(PunchClockKey key);
        void Delete(PunchClockKey key);
    }

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

        public IEnumerable<Punch> Punches { get; }
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

    public abstract class PunchClockResult<T> { }

    public sealed class Success<T> : PunchClockResult<T>
    {
        public Success(T result)
        {
            Result = result;
        }
        public T Result { get; }
    }

    public sealed class Err<T> : PunchClockResult<T>
    {
        public Err(string msg)
        {
            Error = msg;
        }
        public string Error { get; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PunchClockController : ControllerBase
    {
        private static IPunchClockRepository Repo { get; } = new PunchClockRepo();

        private const string MockUserId = "someUserId";

        // GET api/punchclock
        [HttpGet]
        public ActionResult<PunchClockResult<PunchClockState[]>> Get()
        {
            return new Success<PunchClockState[]>(Repo.List());
        }

        // GET api/punchclock/someId
        [HttpGet("{id}")]
        public ActionResult<PunchClockResult<PunchClock>> Get(Guid id)
        {
            var key = new PunchClockKey(MockUserId, id);

            var r = Repo.Get(key);

            if (r != null)
            {
                return new Success<PunchClock>(r);
            }
            else
            {
                return new Err<PunchClock>("Clock not found");
            }
        }

        // POST api/punchclock
        [HttpPost]
        public ActionResult Post()
        {
            var newKey = PunchClockKey.New(MockUserId);
            var clock = Repo.Punch(newKey);

            return Created("someUri", clock);
        }

        // POST api/punchclock/5
        [HttpPost("{id}")]
        public ActionResult Punch(Guid id)
        {
            var key = new PunchClockKey(MockUserId, id);
            var newState = Repo.Punch(key);

            return Created("someUrl", newState);
        }

        // DELETE api/punchclock/5
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            var key = new PunchClockKey(MockUserId, id);
            Repo.Delete(key);

            return NoContent();
        }
    }
}
