namespace VedioCall.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public AuthController(IAuthUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        [Authorize]
        [HttpGet]
        public IActionResult Teat() => Ok("auth");

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            TokenDto? tokenDto = await _unitOfWork.Login(request); 

            if (tokenDto != null) 
                return Ok(tokenDto);

            return BadRequest("Wrong credentials");
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsFarmer([FromForm] UserRegistrationDto request)
        {
            User user = await _unitOfWork.MapFromUserRegistrationDtoToUser(request);

            TokenDto tokenDto = await _unitOfWork.Register(user);

            return Ok(tokenDto);

        }
    }
}