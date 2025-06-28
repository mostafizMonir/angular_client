using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using GoogleLoginApi.Services;
using GoogleLoginApi.Models;
using System.Text.Json.Serialization;
using System.Security.Claims;

namespace GoogleLoginApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserLoginController> _logger;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly string _googleClientId;
        private readonly string _googleClientSecret;
        private readonly string _redirectUri;

        public UserLoginController(
            IConfiguration configuration, 
            ILogger<UserLoginController> logger,
            IJwtService jwtService,
            IUserService userService)
        {
            _configuration = configuration;
            _logger = logger;
            _jwtService = jwtService;
            _userService = userService;
            _googleClientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
            _googleClientSecret = _configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
            _redirectUri = _configuration["Google:RedirectUri"] ?? "http://localhost:7002/api/UserLogin/auth/google/callback";
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                // Validate user credentials
                var user = await _userService.ValidateUserAsync(request.Username, request.Password);
                if (user == null)
                {
                    return Unauthorized(new { error = "Invalid username or password" });
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                var response = new LoginResponse
                {
                    Token = token,
                    User = user
                };

                _logger.LogInformation("Login successful for user: {Email}", user.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("auth/google")]
        public IActionResult InitiateGoogleAuth()
        {
            try
            {
                // Generate state parameter for CSRF protection
                var state = GenerateState();
                
                // Store state in session for validation
                HttpContext.Session.SetString("GoogleOAuthState", state);

                // Build Google OAuth URL
                var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                    $"client_id={_googleClientId}&" +
                    $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
                    $"response_type=code&" +
                    $"scope={Uri.EscapeDataString("openid email profile")}&" +
                    $"state={state}&" +
                    $"access_type=offline&" +
                    $"prompt=consent";

                _logger.LogInformation("Redirecting to Google OAuth: {Url}", googleAuthUrl);
                
                return Redirect(googleAuthUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Google OAuth");
                return StatusCode(500, new { error = "Failed to initiate Google OAuth" });
            }
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> HandleGoogleCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string? scope = null,
            [FromQuery] string? authuser = null,
            [FromQuery] string? prompt = null
          )
        {
            try
            {

               _logger.LogInformation("Google call back: {Code}", code);

                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest(new { error = "Authorization code is required" });
                }

                // Validate state parameter
                var storedState = HttpContext.Session.GetString("GoogleOAuthState");
                if (string.IsNullOrEmpty(storedState) || storedState != state)
                {
                    return BadRequest(new { error = "Invalid state parameter" });
                }

                // Clear the state from session
                HttpContext.Session.Remove("GoogleOAuthState");

                // Exchange authorization code for access token
                var tokenResponse = await ExchangeCodeForToken(code);
                if (tokenResponse == null)
                {
                    return BadRequest(new { error = "Failed to exchange authorization code for token" });
                }

                // Get user information from Google
                var userInfo = await GetGoogleUserInfo(tokenResponse.AccessToken);
                if (userInfo == null)
                {
                    return BadRequest(new { error = "Failed to get user information from Google" });
                }

                // Create or update user in your database
                var user = await _userService.CreateOrUpdateUserAsync(userInfo);

                // Generate JWT token
                var jwtToken = _jwtService.GenerateToken(user);

                var response = new GoogleAuthResponse
                {
                    Token = jwtToken,
                    User = user
                };

                _logger.LogInformation("Google login successful for user: {Email}", user.Email);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Google callback");
                return StatusCode(500, new { error = "Failed to process Google callback" });
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new { error = "Google ID token is required" });
                }

                // Verify Google ID token
                var userInfo = await VerifyGoogleIdToken(request.IdToken);
                if (userInfo == null)
                {
                    return BadRequest(new { error = "Invalid Google ID token" });
                }

                // Create or update user in your database
                var user = await _userService.CreateOrUpdateUserAsync(userInfo);

                // Generate JWT token
                var jwtToken = _jwtService.GenerateToken(user);

                var response = new GoogleAuthResponse
                {
                    Token = jwtToken,
                    User = user
                };

                _logger.LogInformation("Google login successful for user: {Email}", user.Email);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return StatusCode(500, new { error = "Failed to process Google login" });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var user = await _userService.GetUserByEmailAsync(userEmail);
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("test-logging")]
        public IActionResult TestLogging()
        {
            _logger.LogInformation("Test log message from API - {Timestamp}", DateTime.UtcNow);
            _logger.LogWarning("Test warning message from API");
            _logger.LogError("Test error message from API");
            
            return Ok(new { message = "Test logging completed. Check Seq at http://localhost:5341" });
        }

        private string GenerateState()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        private async Task<GoogleTokenResponse?> ExchangeCodeForToken(string code)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                var tokenRequest = new
                {
                    client_id = _googleClientId,
                    client_secret = _googleClientSecret,
                    code = code,
                    grant_type = "authorization_code",
                    redirect_uri = _redirectUri
                };

                var json = JsonSerializer.Serialize(tokenRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);
                }

                _logger.LogError("Failed to exchange code for token. Status: {Status}, Content: {Content}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging code for token");
                return null;
            }
        }

        private async Task<GoogleUserInfo?> GetGoogleUserInfo(string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<GoogleUserInfo>(content);
                }

                _logger.LogError("Failed to get user info. Status: {Status}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google user info");
                return null;
            }
        }

        private async Task<GoogleUserInfo?> VerifyGoogleIdToken(string idToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                
                var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content);
                    
                    // Verify the token is for your application
                    if (tokenInfo?.Audience != _googleClientId)
                    {
                        _logger.LogWarning("Token audience mismatch. Expected: {Expected}, Got: {Got}", 
                            _googleClientId, tokenInfo?.Audience);
                        return null;
                    }

                    // Convert token info to user info
                    return new GoogleUserInfo
                    {
                        Id = tokenInfo.Subject,
                        Email = tokenInfo.Email,
                        Name = tokenInfo.Name,
                        Picture = tokenInfo.Picture
                    };
                }

                _logger.LogError("Failed to verify ID token. Status: {Status}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Google ID token");
                return null;
            }
        }
    }

    // Request/Response Models
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class GoogleAuthRequest
    {
        public string IdToken { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
    }

    public class GoogleAuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class GoogleCallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
} 
