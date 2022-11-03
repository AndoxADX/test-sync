using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Store;
using TodoApi.ViewModels;

namespace TodoApi.Controllers
{
    [Route("api/Test")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        public readonly TestStore _store;
        public ValuesController(TestStore store)
        {
            _store = store;
        }


        [HttpPost("Update")]
        public async Task<IActionResult>  TrxV3([FromBody] CreateTrxModel model)
        {
            var result = await _store.TrxV3(model);
            return Ok(result);
        }

        [HttpGet("GetIds")]
        public async Task<string> GetId()
        {
            var result = await _store.GetId();
            return result;
        }

    }
}
