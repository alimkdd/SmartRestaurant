SmartRestaurant – Blazor Server & .NET 8 Application

Description:
SmartRestaurant is a modern web-based restaurant management system built with Blazor Server and .NET 8, designed to streamline restaurant operations, including menu management, inventory tracking, orders, and user authentication. The system leverages Redis caching for login rate limiting, FluentValidation for robust input validation, and JWT-based authentication for secure access control.

Key Features:

Authentication & Authorization: Role-based access with Admin, Staff, and Customer roles. Supports JWT tokens and refresh tokens.

Login Rate Limiting: Prevents brute-force attacks by locking users out after configurable failed attempts. Shows a user-friendly countdown timer during lockout.

Menu & Inventory Management: CRUD operations for menu items and inventory, fully dynamic with validation.

Order Management: Place and track customer orders with detailed order history.

AI Integration: Chatbot service for customer interaction (optional).

Caching: Redis integration for login attempts, lockouts, and temporary data.

Clean Architecture: Separation of concerns with Controllers, Services, and Interfaces.

FluentValidation: Server-side and client-side validation for all user inputs.

Responsive UI: Built with modern Bootstrap-based design for a seamless user experience.

Technologies & Libraries Used:

Blazor Server

.NET 8

Entity Framework Core (SQL Server)

Redis (StackExchange.Redis)

FluentValidation

JWT Authentication

Bootstrap 5

Use Case:
SmartRestaurant is suitable for small to medium-sized restaurants aiming to digitalize their daily operations, from managing menu items and inventory to handling orders and securing user access.
