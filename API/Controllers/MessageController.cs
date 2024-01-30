using API.Extensions;
using API.ViewModel;
using API.ViewModel.Message;
using DBL.DTOs;
using DBL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DBL.Identity;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageDTO> _logger;
        private readonly IRepository<MessageDTO, string> _repository;

        private const int pageSize = 100;

        public MessageController(
            ILogger<MessageDTO> logger,
            IRepository<MessageDTO, string> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [Authorize(RoleNames.USER)]
        [HttpPost("create/", Name = "CreateMessage")]
        public async Task<ActionResult> CreateMessageAsync([FromBody] MessageCreateIn message)
        {
            try
            {
                var chatDTO = await _repository.AddItemAsync(message.ToDTO());

                await _repository.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.USER)]
        [HttpPost("id/", Name = "MessageById")]
        public async Task<ActionResult<MessageGetByIdOut>> GetById([FromBody] AnyId chat)
        {
            try
            {
                var message = await _repository.GetItemAsync(new MessageDTO { Id = chat.Id });

                return Ok(message.ToMessageGetByIdOut());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.USER)]
        [HttpGet("page/", Name = "MessagePage")]
        public async Task<ActionResult<List<MessageGetListOut>>> GetList([FromQuery] int page)
        {
            try
            {
                List<MessageGetListOut> list = new List<MessageGetListOut>();

                foreach (var item in await _repository.GetItemsAsync(page, pageSize))
                {
                    list.Add(item.ToMessageGetListOut());
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.USER)]
        [HttpGet("pagecount/", Name = "MessagePageCount")]
        public async Task<ActionResult<int>> GetListCount()
        {
            try
            {
                return Ok(_repository.PageCount(pageSize));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.USER)]
        [HttpPatch("update/", Name = "UpdateMessage")]
        public async Task<ActionResult> Update([FromBody] MessageOnlyFields message)
        {
            try
            {
                await _repository.UpdateAsync(message.ToDTO());
                await _repository.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.ADMIN)]
        [HttpDelete("delete/", Name = "DeleteMessage")]
        public async Task<ActionResult> Delete([FromBody] AnyId message)
        {
            try
            {
                await _repository.DeleteAsync(message.Id);
                await _repository.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
