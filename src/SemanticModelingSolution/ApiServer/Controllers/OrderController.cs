using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SimpleDomain1;

namespace ApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly RepositoryService _repositoryService;

        public OrderController(RepositoryService repositoryService)
        {
            _repositoryService = repositoryService;
        }

        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return _repositoryService.GetOrders();
        }

    }
}
