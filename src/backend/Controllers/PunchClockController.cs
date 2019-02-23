using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PunchClockController : ControllerBase
    {
        private static IPunchClockRepository Repo { get; } = new PunchClockRepo();

        private string UserId => User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

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
            var key = new PunchClockKey(UserId, id);

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
            var newKey = PunchClockKey.New(UserId);
            var clock = Repo.Punch(newKey);

            return Created($"api/punchclock/{clock.PunchClockId}", clock);
        }

        // POST api/punchclock/5
        [HttpPost("{id}")]
        public ActionResult Punch(Guid id)
        {
            var key = new PunchClockKey(UserId, id);
            var newState = Repo.Punch(key);

            return Created($"api/punchclock/{newState.PunchClockId}", newState);
        }

        // DELETE api/punchclock/5
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            var key = new PunchClockKey(UserId, id);
            Repo.Delete(key);

            return NoContent();
        }
    }
}
