using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace aqua_api.Modules.Identity.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserDetailController : ControllerBase
    {
        private readonly IUserDetailService _service;
        private readonly ILocalizationService _localizationService;

        public UserDetailController(IUserDetailService service, ILocalizationService localizationService)
        {
            _service = service;
            _localizationService = localizationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _service.GetPagedAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user/{userId:long}")]
        public async Task<IActionResult> GetByUserId([FromRoute] long userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            var result = await _service.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDetailDto dto)
        {

            var result = await _service.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDetailDto dto)
        {

            var result = await _service.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("users/{userId}/profile-picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(
            [FromRoute] long userId,
            IFormFile file) // ❗ [FromForm] YOK - IFormFile otomatik olarak form'dan bind edilir
        {
            var result = await _service.UploadProfilePictureAsync(userId, file);
            return StatusCode(result.StatusCode, result);
        }
    }
}