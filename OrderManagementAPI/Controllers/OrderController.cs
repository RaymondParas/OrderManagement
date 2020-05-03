using AuthenticationLibrary.Attributes;
using OrderLibrary.Models;
using OrderLibrary.Parsers;
using OrderLibrary.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace OrderManagementAPI.Controllers
{
    [JwtAuthorization()]
    [RoutePrefix("api/Order")]
    public class OrderController : ApiController
    {
        private readonly IOrderService _service;
        private readonly ICSVParser _parser;

        public OrderController(IOrderService service, ICSVParser parser)
        {
            _service = service;
            _parser = parser;
        }

        [HttpGet]
        public IHttpActionResult GetOrders()
        {
            var result = _service.GetExistingOrders();

            return Ok(result);
        }

        [HttpGet]
        [Route("{orderNumber}")]
        public IHttpActionResult GetOrder(string orderNumber)
        {
            var result = _service.GetSpecificOrder(orderNumber);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Route("summary")]
        public IHttpActionResult GetOrderSummary()
        {
            var result = _service.GetOrderSummary();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IHttpActionResult> PostOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _service.InsertManualOrder(order);

            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        [HttpPut]
        [Route("{orderNumber}")]
        public async Task<IHttpActionResult> PutOrder(string orderNumber, [FromBody] Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _service.UpdateOrderAsync(orderNumber, order);

            if (result == null)
                return NotFound();
            else if (result == false)
                return BadRequest();

            return Ok();
        }

        [HttpDelete]
        [Route("{orderNumber}")]
        public async Task<IHttpActionResult> DeleteOrder(string orderNumber)
        {
            var result = await _service.DeleteOrderAsync(orderNumber);

            if (result == false)
                return BadRequest();

            return Ok();
        }

        [HttpPut]
        [Route("status")]
        public async Task<IHttpActionResult> UpdateStatus([FromBody] StatusChangeRequest statusChange)
        {
            if (!ModelState.IsValid || statusChange.OrderNumber == null || statusChange.Status == null || statusChange.IsOrderStatus == null
                || (!statusChange.IsOrderStatus.Equals("Y") && !statusChange.IsOrderStatus.Equals("N")))
                return BadRequest();

            bool isOrderStatus = statusChange.IsOrderStatus.Equals("Y") ? true : false;
            var result = await _service.UpdateStatusAsync(statusChange.OrderNumber, statusChange.Status, isOrderStatus);

            if (result == null)
                return NotFound();
            else if (result == false)
                return BadRequest();

            return Ok();
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType));

            if (Request.Content.Headers.ContentLength == 0)
                return BadRequest("No file uploaded.");

            var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();

            if (filesReadToProvider.Contents.Any(f => f.Headers.ContentLength == 0))
                return new ResponseMessageResult(Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Empty file uploaded."));

            var type = new MediaTypeWithQualityHeaderValue("text/csv");
            if (filesReadToProvider.Contents.Any(f => !f.Headers.ContentType.Equals(type)))
                return new ResponseMessageResult(Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, "Only CSV files are permitted."));

            var csvFileContent = filesReadToProvider.Contents.Where(s => s.Headers.ContentType.Equals(type)).FirstOrDefault();
            var fileStream = new MemoryStream(await csvFileContent.ReadAsByteArrayAsync());

            var orders = _parser.MapCSVToOrderModel(fileStream);
            if (orders == null)
                return BadRequest("Invalid CSV.");

            var result = await _service.InsertOrdersAsync(orders);
            if (result == null)
                return BadRequest("Records failed to be inserted into database.");

            return Ok(result);
        }
    }
}
