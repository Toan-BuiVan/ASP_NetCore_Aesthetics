using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ASP_NetCore_Aesthetics.Filter
{
	public class Filter_CheckToken : IAsyncAuthorizationFilter
	{
		private readonly IAccountRepository _accountRepository;
		private readonly IDistributedCache _cache;
		private readonly IConfiguration _configuration;

		public Filter_CheckToken(IAccountRepository accountRepository, IDistributedCache cache, IConfiguration configuration)
		{
			_accountRepository = accountRepository;
			_cache = cache;
			_configuration = configuration;
		}

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			var httpContext = context.HttpContext;
			var request = httpContext.Request;

			// Đọc token từ header - bắt buộc header phải truyền vào 4 tham số
			var accessToken = request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
			var refreshToken = request.Headers["RefreshToken"].FirstOrDefault();
			var userIdString = request.Headers["UserID"].FirstOrDefault();
			var deviceName = request.Headers["DeviceName"].FirstOrDefault();

			if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(userIdString))
			{
				SetUnauthorizedResponse(context, "Thiếu thông tin xác thực.");
				return;
			}

			if (!int.TryParse(userIdString, out int userId))
			{
				SetUnauthorizedResponse(context, "UserID không hợp lệ.");
				return;
			}
			var cacheKey = $"User_{userId}-{deviceName}";
			byte[] cacheData = await _cache.GetAsync(cacheKey);
			if (cacheData != null)
			{
				return; 
			}
			var userDetail = await _accountRepository.User_GetByID(userId);
			if (userDetail == null)
			{
				SetUnauthorizedResponse(context, $"Token của User {userId} không tồn tại!");
				return;
			}
			if (userDetail.TokenExprired < DateTime.Now)
			{
				SetUnauthorizedResponse(context, "Token hết hạn, vui lòng đăng nhập lại!");
				return;
			}

			// Giải mã Token
			var principal = await _accountRepository.GetPrincipalFromExpiredToken(accessToken);
			if (principal == null)
			{
				SetUnauthorizedResponse(context, "Token không hợp lệ!");
				return;
			}

			// Kiểm tra refreshToken
			var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirst("exp").Value));
			DateTime result = exp.UtcDateTime.AddHours(7);
			var userName = principal.Identity?.Name;
			var user = _accountRepository.GetUser_ByUserName(userName);

			if (user == null || user.RefeshToken != refreshToken || user.TokenExprired <= DateTime.Now)
			{
				SetUnauthorizedResponse(context, "Token không hợp lệ!");
				return;
			}

			// Tạo token mới nếu cần
			var newToken = await _accountRepository.CreateToken(principal.Claims.ToList());
			var newRefreshToken = await _accountRepository.GenerateRefreshToken();

			// Cập nhật token mới vào DB
			_ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
			await _accountRepository.UserUpdate_RefeshToken(user.UserID, newRefreshToken, DateTime.Now.AddDays(refreshTokenValidityInDays));

			// Ghi vào cache để tránh kiểm tra lại
			var tokenResponse = new CheckTokenReponsData
			{
				ResponseCode = 1,
				ResposeMessage = "Tạo mới Token thành công",
				Token = new JwtSecurityTokenHandler().WriteToken(newToken),
				RefreshToken = newRefreshToken
			};
			await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(tokenResponse.Token), new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
			});

			// Thêm token vào response header
			context.HttpContext.Response.Headers["New-AccessToken"] = tokenResponse.Token;
			context.HttpContext.Response.Headers["New-RefreshToken"] = newRefreshToken;
		}

		private void SetUnauthorizedResponse(AuthorizationFilterContext context, string message)
		{
			context.HttpContext.Response.ContentType = "application/json";
			context.HttpContext.Response.StatusCode = 401;
			context.Result = new JsonResult(new { ReturnCode = 401, ReturnMessage = message });
		}
	}
}
