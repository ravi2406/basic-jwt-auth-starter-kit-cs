Here’s a concise documentation outline for the **JWT Authentication Starter Kit** in `README.md` format to guide developers using the starter kit API:

---

# JWT Authentication Starter Kit for .NET

**Dotnet Version**: 8

## Overview

The JWT Authentication Starter Kit is a robust and ready-to-use framework for implementing JWT-based authentication in .NET applications. 
It allows developers to integrate JWT authentication with a minimal setup, focusing on business logic while providing customizable authentication layers. 
This kit supports user registration, login, session management, and logout, making it easy to manage authenticated user sessions across different devices or clients. 
The kit can be integrated with SPA applications UI Starter app development in progress).

## Features

- **User Registration & Login**: Supports secure user registration and login with password hashing and JWT issuance.
- **Session Management**: Tracks active sessions per user, supporting multiple device sessions.
- **Customizable JWT Validation**: Custom token validation for handling session revocation and other validations.
- **REST API Endpoints**: Predefined API endpoints for common authentication actions.

---

## Endpoints

### **Auth API**

#### 1. **Login**
- **URL**: `/api/Auth/login`
- **Method**: `POST`
- **Description**: Authenticates user credentials and issues a JWT token on success.
- **Request Body**:
  - **application/json**:
    - **Schema**: `LoginRequest`
- **Response**:
  - **200**: Returns a JWT token on successful authentication.

#### 2. **Register**
- **URL**: `/api/Auth/register`
- **Method**: `POST`
- **Description**: Registers a new user and saves user details to the database.
- **Request Body**:
  - **application/json**:
    - **Schema**: `RegisterRequest`
- **Response**:
  - **200**: User registered successfully.

---

### **Session API**

#### 1. **Get Current User**
- **URL**: `/api/Session/user`
- **Method**: `GET`
- **Description**: Retrieves the details of the currently authenticated user.
- **Response**:
  - **200**: Returns user details from the current JWT token.

#### 2. **Get User Sessions**
- **URL**: `/api/Session/sessions`
- **Method**: `GET`
- **Description**: Retrieves a list of all active sessions for a given user.
- **Query Parameters**:
  - `userId`: User ID (required)
- **Response**:
  - **200**: Returns all active sessions associated with the user.

#### 3. **Logout from Session**
- **URL**: `/api/Session/logout/session`
- **Method**: `POST`
- **Description**: Logs out a specific session based on the provided session ID, revoking the JWT token.
- **Request Body**:
  - **application/json**:
    - **Schema**: `LogoutRequest`
- **Response**:
  - **200**: Session logged out successfully.

#### 4. **Logout from All Sessions**
- **URL**: `/api/Session/logout/all`
- **Method**: `POST`
- **Description**: Logs out all sessions for the user, invalidating all JWT tokens associated with the user’s active sessions.
- **Request Body**:
  - **application/json**:
    - **Schema**: `LogoutRequest`
- **Response**:
  - **200**: All sessions logged out successfully.

#### 5. **Logout**
- **URL**: `/api/Session/logout`
- **Method**: `POST`
- **Description**: Logs out the current session.
- **Response**:
  - **200**: Logged out successfully.

---

## Models

### `LoginRequest`
- **Username**: `string` (required)
- **Password**: `string` (required)

### `RegisterRequest`
- **Username**: `string` (required)
- **Password**: `string` (required)
- **Email**: `string` (optional)
- **FullName**: `string` (optional)

### `LogoutRequest`
- **SessionId**: `string` (optional for current session logout, required for session-based logout)

---

## Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/yourusername/jwt-auth-starter-kit.git
   ```
2. Install dependencies:
   ```sh
   dotnet restore
   ```
3. Configure JWT settings in `appsettings.json`.

4. Run the application:
   ```sh
   dotnet run
   ```

## Usage

1. **Register a new user** via `/api/Auth/register`.
2. **Login** with the registered credentials to receive a JWT token.
3. Use the token to authenticate requests to secure endpoints.
4. **Manage sessions** with the Session API.

---

## Customization

- Modify **JWT settings** for `Issuer`, `Audience`, and token expiration in `appsettings.json`.
- Extend **CustomJwtTokenValidator** for additional validation such as user roles or token revocation checks.

---

## Contributing

Contributions and suggestions are welcome! Please fork the repository and submit a pull request for review.

## Future

Advanced started kit with more advanced features like user group management, clients management etc is in the cards.

---

## License

This project is licensed under the MIT License.

---

This README serves as a quick start for developers to use and extend the JWT Authentication Starter Kit in .NET projects.
