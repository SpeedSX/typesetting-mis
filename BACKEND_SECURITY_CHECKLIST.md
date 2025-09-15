# Backend Security Checklist: HttpOnly Cookie Implementation

## Overview
This checklist outlines the required backend changes to implement secure refresh token handling using httpOnly cookies instead of localStorage.

## ✅ Frontend Changes Completed
- [x] Updated `ApiService` to use `withCredentials: true`
- [x] Modified `refreshToken()` to call `/auth/refresh` without body
- [x] Updated `logout()` to call `/auth/logout` without body
- [x] Removed all `localStorage` handling of `refreshToken` in auth slice
- [x] Updated response interceptor to not clear `refreshToken` from localStorage

## 🔧 Backend Changes Required

### 1. Set Refresh Token as HttpOnly Cookie
**Location**: `AuthController.cs` - `Login` and `RefreshToken` endpoints

```csharp
// In Login endpoint
var cookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true, // Use HTTPS in production
    SameSite = SameSiteMode.Strict,
    Expires = DateTime.UtcNow.AddDays(7) // Adjust as needed
};
Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

// In RefreshToken endpoint
var newCookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTime.UtcNow.AddDays(7)
};
Response.Cookies.Append("refreshToken", newRefreshToken, newCookieOptions);
```

### 2. Accept and Validate Refresh Token from Cookie Only
**Location**: `AuthController.cs` - `RefreshToken` endpoint

```csharp
[HttpPost("refresh")]
public async Task<ActionResult<AuthResponseDto>> RefreshToken()
{
    // Read refresh token from cookie instead of request body
    if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || 
        string.IsNullOrEmpty(refreshToken))
    {
        return Unauthorized("No refresh token found in cookie");
    }

    var result = await authService.RefreshTokenAsync(refreshToken);
    // ... rest of implementation
}
```

### 3. Clear Cookie on Logout/Invalid Token
**Location**: `AuthController.cs` - `Logout` endpoint

```csharp
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
    // Read refresh token from cookie
    if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
    {
        await authService.LogoutAsync(refreshToken);
    }

    // Clear the refresh token cookie
    Response.Cookies.Delete("refreshToken", new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict
    });

    return Ok(new { message = "Logged out successfully" });
}
```

### 4. Update AuthService Methods
**Location**: `AuthService.cs`

```csharp
// Update RefreshTokenAsync to not expect token parameter
public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
{
    // Implementation remains the same, just remove the parameter
    // and read from cookie in the controller
}

// Update LogoutAsync to not expect token parameter
public async Task LogoutAsync(string refreshToken)
{
    // Implementation remains the same
}
```

### 5. Implement CSRF Protection
**Option A: Double-Submit Cookie Pattern**
```csharp
// In Program.cs or Startup.cs
services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false; // Allow JavaScript to read
});

// Add CSRF token to responses
[HttpPost("refresh")]
public async Task<ActionResult<AuthResponseDto>> RefreshToken()
{
    // Validate CSRF token
    await HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
        .ValidateRequestAsync(HttpContext);
    
    // ... rest of implementation
}
```

**Option B: SameSite=Strict (Simpler)**
```csharp
// Already implemented in cookie options above
SameSite = SameSiteMode.Strict
```

### 6. Update CORS Configuration
**Location**: `Program.cs` or `Startup.cs`

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Frontend URL
               .AllowCredentials() // Important for cookies
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Apply CORS policy
app.UseCors("AllowFrontend");
```

## 🚀 Migration Plan

### Phase 1: Immediate Backend Changes (Required)
1. **Update RefreshToken endpoint** to read from cookie
2. **Update Logout endpoint** to clear cookie
3. **Set httpOnly cookies** in Login/Refresh responses
4. **Update CORS** to allow credentials

### Phase 2: Security Enhancements (Recommended)
1. **Implement CSRF protection** (double-submit cookie or SameSite=Strict)
2. **Add token rotation** (generate new refresh token on each refresh)
3. **Implement token blacklisting** for logout
4. **Add rate limiting** to refresh endpoint

### Phase 3: Production Hardening (Future)
1. **Use HTTPS only** (Secure=true)
2. **Implement token fingerprinting** (bind to device/browser)
3. **Add refresh token expiration** policies
4. **Implement refresh token family** (detect token reuse)

## 🔍 Testing Checklist

### Frontend Testing
- [ ] Login sets httpOnly cookie
- [ ] Refresh token works without localStorage
- [ ] Logout clears cookie
- [ ] 401 errors redirect to login
- [ ] CORS allows credentials

### Backend Testing
- [ ] Refresh endpoint reads from cookie
- [ ] Logout endpoint clears cookie
- [ ] CSRF protection works (if implemented)
- [ ] CORS allows frontend with credentials

## ⚠️ Security Considerations

1. **HttpOnly cookies** prevent XSS attacks from stealing refresh tokens
2. **Secure flag** ensures cookies only sent over HTTPS in production
3. **SameSite=Strict** prevents CSRF attacks
4. **Token rotation** limits damage if refresh token is compromised
5. **CORS with credentials** must be properly configured

## 📝 Notes

- The frontend changes are **backward compatible** - they will work with the current backend
- Backend changes are **required** for the security improvements to take effect
- Consider implementing **token rotation** for additional security
- Monitor for **CORS issues** during development and testing
