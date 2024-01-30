using API.Extensions;
using API.ViewModel;
using API.ViewModel.User;
using DBL.DTOs;
using DBL.Identity;
using DBL.Interfaces;
using DBL.Models;
using DBL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserDTO> _logger;
        private readonly IRepository<UserDTO, string> _userRepository;
        private readonly IRepository<ChatDTO, string> _chatRepository;
        private const int pageSize = 30;

        public UserController(
            ILogger<UserDTO> logger,
            IRepository<UserDTO, string> registrationManager,
            IRepository<ChatDTO, string> chatRepository)
        {
            _logger = logger;
            _userRepository = registrationManager;
            _chatRepository = chatRepository;
        }

        [HttpPost("registeradmin/", Name = "RegisterAdmin")]
        public async Task<ActionResult<UserOnlyFields>> CreateAdminAsync([FromBody] UserCreateAdminIn user)
        {
            try
            {
                var userDTO = await _userRepository.AddItemAsync(user.ToDTO());

                await _userRepository.SaveChangesAsync();

                return Ok(userDTO.ToOnlyFieldsObject());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("registeruser/", Name = "RegisterUser")]
        public async Task<ActionResult<UserCreateUserOut>> CreateUserAsync([FromBody] UserCreateUserIn user)
        {
            try
            {
                var userDTO = await _userRepository.GetItemAsync(new UserDTO { FirstName = user.FirstName, MiddleName = user.MiddleName, LastName = user.LastName, PhoneNumber = user.Phone}, false);

                if (userDTO == null)
                {
                    userDTO = await _userRepository.AddItemAsync(user.ToDTO());

                    UserDTO admin = null;

                    if (_userRepository is UserRepository userRepository)
                        admin = await userRepository.GetAdminAsync();

                    await _chatRepository.AddItemAsync(new ChatDTO { Users = new List<UserDTO> { new UserDTO { Id = userDTO.Id }, new UserDTO { Id = admin.Id } } });

                    await _userRepository.SaveChangesAsync();

                    userDTO = await _userRepository.GetItemAsync(new UserDTO { FirstName = userDTO.FirstName, MiddleName = userDTO.MiddleName, LastName = userDTO.LastName, PhoneNumber = userDTO.PhoneNumber }, true);
                }
                return Ok(userDTO.ToUserCreateUserOut());

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.ADMIN)]
        [HttpPost("id/", Name = "UserById")]
        public async Task<ActionResult<UserGetUserByIdOut>> GetById([FromBody] AnyId user)
        {
            try
            {
                var userDTO = await _userRepository.GetItemAsync(new UserDTO { Id = user.Id });

                return Ok(userDTO.ToUserGetByIdOut());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.ADMIN)]
        [HttpPost("admin/", Name = "Admin")]
        public async Task<ActionResult<UserGetUserByIdOut>> GetAdmin()
        {
            try
            {
                UserDTO userDTO = null;

                if (_userRepository is UserRepository userRepository)
                    userDTO = await userRepository.GetAdminAsync();

                return Ok(userDTO.ToUserGetByIdOut());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("page/", Name = "UserPage")]
        public async Task<ActionResult<List<UserOnlyFields>>> GetList([FromQuery] int page)
        {
            try
            {
                List<UserOnlyFields> list = new List<UserOnlyFields>();

                foreach (var item in await _userRepository.GetItemsAsync(page, pageSize))
                {
                    list.Add(item.ToOnlyFieldsObject());
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("pagecount/", Name = "UserPageCount")]
        public async Task<ActionResult<int>> GetListCount()
        {
            try
            {
                return Ok(_userRepository.PageCount(pageSize));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.USER)]
        [HttpPatch("update/", Name = "UpdateUser")]
        public async Task<ActionResult> Update([FromBody] UserOnlyFields user)
        {
            try
            {
                var userDTO = user.ToDTO();

                await _userRepository.UpdateAsync(userDTO);
                await _userRepository.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(RoleNames.ADMIN)]
        [HttpDelete("delete/", Name = "DeleteUser")]
        public async Task<ActionResult> Delete([FromBody] AnyId user)
        {
            try
            {
                await _userRepository.DeleteAsync(user.Id);
                await _userRepository.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("initRoles/", Name = "InitUserRoles")]
        public async Task<ActionResult> initRoles()
        {
            try
            {
                if (_userRepository is UserRepository userRepository)
                {
                    await userRepository.InitDefaultRolesAsync();
                }

                await _userRepository.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
