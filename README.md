# Comparing_Refit

This project demonstrates the comparison of HTTP client usage in .NET applications, specifically focusing on standard `HttpClient` and the `Refit` library. It also integrates OpenAPI and Scalar API Reference for API documentation and testing.

## Features

- **HttpClient Integration**: Demonstrates how to fetch data using the standard `HttpClient`.
- **Refit Integration**: Showcases the use of `Refit` for type-safe API calls.
- **OpenAPI Documentation**: Provides API documentation using OpenAPI.
- **Scalar API Reference**: Offers a modern API reference UI with support for themes and dark mode.
- **JWT Authentication**: Secures endpoints with JWT-based authentication.

## Endpoints

### `/hello`
- **Description**: A simple endpoint that returns a hello message.
- **Response**: `"Hello Scalar"`

### `/api/colors/httpclient`
- **Description**: Retrieves a list of CSS colors using the standard `HttpClient`. Also returns the execution time.
- **Response**:
  
- { "Time": "1523 ms", "Colors": [ { "Name": "Red", "Hex": "#FF0000" }, ... ] }


### `/api/colors/refit`
- **Description**: Retrieves a list of CSS colors using the `Refit` client. Also returns the execution time.
- **Response**:
 
- { "Time": "1230 ms", "Colors": [ { "Name": "Red", "Hex": "#FF0000" }, ... ] }

## Setup Instructions

1. **Clone the Repository**:
2. **Build the Project**:
   Ensure you have the .NET 9 SDK installed, then run: dotnet build
3. **Run the Application**:
   Start the application using: dotnet run

4. **Access the API Documentation**:
   - OpenAPI: Navigate to `https://localhost:7026/swagger`
   - Scalar API Reference: Navigate to `https://localhost:7026/scalar/v1`

## Authentication

This project uses JWT-based authentication. To access protected endpoints:
1. Obtain a valid JWT token from your authentication provider.
2. Use the "Authorize" button in the API documentation to input your token in the format: Bearer <token>


## Dependencies

- **Refit**: For type-safe API calls.
- **Newtonsoft.Json**: For JSON serialization and deserialization.
- **Scalar.AspNetCore**: For modern API reference UI.
- **Microsoft.AspNetCore.OpenApi**: For OpenAPI documentation.

## Project Structure

- **Program.cs**: Contains the main application setup, including endpoint definitions and middleware configuration.
- **IApiService.cs**: Defines the `Refit` interface for API calls.
- **CSSColorName.cs**: Represents the data model for CSS color names.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

---

Happy coding!

   