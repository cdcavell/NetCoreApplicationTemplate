# Middleware Pipeline

The application centralizes standard middleware ordering through `UseApplicationPipeline()` so `Program.cs` remains focused on application startup and service registration.

The pipeline order is:

1. Forwarded headers
2. Structured request logging
3. Centralized error handling
4. Problem Details handling
5. Security headers
6. HTTPS redirection
7. Static files
8. Routing
9. CORS
10. Rate limiting
11. Authentication
12. Authorization
13. Controller and Razor Page endpoint mapping

This order keeps proxy correction early, request logging close to the beginning of the request, error handling ahead of most application behavior, and endpoint-specific features such as CORS and rate limiting after routing.
