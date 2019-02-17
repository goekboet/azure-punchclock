using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Repository;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
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
