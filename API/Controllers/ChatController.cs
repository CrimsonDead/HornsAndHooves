using API.Extensions;
using API.ViewModel;
using API.ViewModel.Chat;
using API.ViewModel.User;
using DBL.DTOs;
using DBL.Identity;
using DBL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatDTO> _logger;
        private readonly IRepository<ChatDTO, string> _repository;
        private readonly AuthorizationManager<UserDTO> _authorizationManager;

        private const int pageSize = 30;

        public ChatController(
            ILogger<ChatDTO> logger,
            IRepository<ChatDTO, string> repository,
            AuthorizationManager<UserDTO> authorizationManager)
        {
            _logger = logger;
            _repository = repository;
            _authorizationManager = authorizationManager;
        }

        [Authorize(RoleNames.USER)]
        [HttpPost("create/", Name = "CreateChat")]
        public async Task<ActionResult<ChatOnlyFields>> CreateChatAsync([FromBody] ChatCreateIn chat)
        {
            try
            {
                if (await _authorizationManager.GetUserRoleAsync(chat.AdminId) != RoleNames.ADMIN ||
                    await _authorizationManager.GetUserRoleAsync(chat.UserId) != RoleNames.USER)
                {
                    return BadRequest("Wrong user roles");
                }

                var chatDTO = await _repository.AddItemAsync(new ChatDTO { Users = new List<UserDTO> { new UserDTO { Id = chat.UserId }, new UserDTO { Id = chat.AdminId } } });

                await _repository.SaveChangesAsync();

                return Ok((await _repository.GetItemAsync(chatDTO)).ToOnlyFieldsObject());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("id/", Name = "ChatById")]
        public async Task<ActionResult<ChatGetByIdOut>> GetById([FromBody] AnyId chat)
        {
            try
            {
                var chatDTO = await _repository.GetItemAsync(new ChatDTO { Id = chat.Id });

                return Ok(chatDTO.ToChatGetByIdOut());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("usernamefilterpage/", Name = "UserNameFilterPage")]
        public async Task<ActionResult<List<ChatGetListOut>>> GetListByUserName([FromQuery] int page, string username)
        {
            try
            {
                List<ChatGetListOut> list = new List<ChatGetListOut>();

                if (_repository is ISearchable<ChatDTO, string> searchable)
                    foreach (var item in await searchable.GetEntityByFilterAsync(username, page, pageSize))
                    {
                        list.Add(item.ToChatGetListOut());
                    }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("page/", Name = "ChatPage")]
        public async Task<ActionResult<List<ChatGetListOut>>> GetList([FromQuery] int page)
        {
            try
            {
                List<ChatGetListOut> list = new List<ChatGetListOut>();

                foreach (var item in await _repository.GetItemsAsync(page, pageSize))
                {
                    list.Add(item.ToChatGetListOut());
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("pagecount/", Name = "ChatPageCount")]
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
        [HttpPatch("update/", Name = "UpdateChat")]
        public async Task<ActionResult> Update([FromBody] ChatOnlyFields chat)
        {
            try
            {
                await _repository.UpdateAsync(chat.ToDTO());
                await _repository.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.ADMIN)]
        [HttpDelete("delete/", Name = "DeleteChat")]
        public async Task<ActionResult> Delete([FromBody] AnyId chat)
        {
            try
            {
                await _repository.DeleteAsync(chat.Id);
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
