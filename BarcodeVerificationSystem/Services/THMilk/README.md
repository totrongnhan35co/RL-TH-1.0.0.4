# TH Milk API Service

This folder contains the complete server service implementation for handling TH True Milk APIs.

## Structure

### Models
- **ReceiveModel/** - Request models for incoming API calls
  - `ReceiveLogin.cs` - Login request model (username, password, secret_key)
  - `ReceiveRefreshToken.cs` - Refresh token request model
  - `ReceiveData.cs` - QR Code data model

- **ResponseModel/** - Response models for API responses
  - `ResponseLogin.cs` - Login/refresh token response (access_token, refresh_token)
  - `ResponseData.cs` - Generic response model (is_success, message)

### Controllers
- **LoginController.cs** - Handles `/api/login` endpoint
  - Validates credentials and secret key
  - Generates JWT access and refresh tokens
  - Token expiry: 60 seconds

- **RefreshTokenController.cs** - Handles `/api/refresh-token` endpoint
  - Validates refresh token
  - Generates new access and refresh tokens

- **CodeController.cs** - Handles `/api/code` endpoint
  - Validates authorization token
  - Processes QR code data
  - Raises events for application integration

### Services
- **ServerService.cs** - HTTP listener service
  - Listens on port 5001
  - Handles incoming HTTP requests
  - Provides events for request processing

- **THMilkAPIHandler.cs** - Main API router
  - Routes requests to appropriate controllers
  - Coordinates between ServerService and Controllers
  - Provides events for QR code data and API requests

## API Endpoints

### 1. Login
```
POST /api/login
Content-Type: application/json

{
    "username": "thtruemilk",
    "password": "thtruemilk@#062026",
    "secret_key": "NUzfz%sqL%j&0sTMfi2oYPWnTlV8wYW6TJcIssOclIVTQpG"
}

Response:
{
    "access_token": "eyJhbGc...",
    "refresh_token": "eyJhbGc..."
}
```

### 2. Refresh Token
```
POST /api/refresh-token
Content-Type: application/json

{
    "username": "thtruemilk",
    "refresh_token": "eyJhbGc..."
}

Response:
{
    "access_token": "eyJhbGc...",
    "refresh_token": "eyJhbGc..."
}
```

### 3. QR Code Data
```
POST /api/code
Content-Type: application/json
Authorization: Bearer <access_token>

{
    "code": "https://ndatrace.vn/01/8935217402816/21/hp9v3mzoio",
    "line_IP": "192.168.67.08",
    "line_name": "Line-08"
}

Response:
{
    "is_success": true,
    "message": "Thành công"
}
```

## Usage Example

```csharp
// Initialize the API handler
var apiHandler = new THMilkAPIHandler("http://*:5001/");

// Subscribe to QR code received events
apiHandler.QRCodeReceived += (sender, e) =>
{
    Console.WriteLine($"QR Code: {e.Code}");
    Console.WriteLine($"Line: {e.LineName}");
    Console.WriteLine($"User: {e.AuthenticatedUser}");
    // Process the QR code in your application
};

// Subscribe to API request events
apiHandler.APIRequestReceived += (sender, e) =>
{
    Console.WriteLine($"{e.Method} {e.Path} from {e.ClientIP}");
};

// Start the server
apiHandler.Start();

// Later, stop the server
apiHandler.Stop();
```

## Security Notes

1. **JWT Tokens**: Tokens expire after 60 seconds as specified in the API documentation
2. **Secret Key**: Hard-coded for TH True Milk integration
3. **Credentials**: Stored in-memory (production should use database)
4. **HTTPS**: Consider using HTTPS in production environment

## Dependencies

Required NuGet packages:
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`
- `Newtonsoft.Json`

## Integration

The service integrates with the existing R-Link system through events. When QR code data is received and validated, the `QRCodeReceived` event is raised, allowing the main application to process the data according to production line requirements.
