using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace ApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly MetadataService _metadataService;

        public MetadataController(MetadataService metadataService)
        {
            _metadataService = metadataService;
        }
    }
}
