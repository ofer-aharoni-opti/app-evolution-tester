# Current Architecture - Updated 2026-04-23

## System Overview
E-commerce platform with microservices architecture. Three main domains: User Management, Product Catalog, and Order Processing. Event-driven communication via Azure Service Bus.

## Core Services
- **UserService** - Authentication, profiles, preferences
- **CatalogService** - Product data, search, recommendations  
- **OrderService** - Cart, checkout, payment processing
- **NotificationService** - Email, SMS, push notifications
- **AnalyticsService** - User behavior, business metrics

## Data Flow
```
Web/Mobile Client → API Gateway → Domain Services → Azure SQL Database
                                              ↓
                                         Service Bus → Background Workers
```

## Recent Evolution
- **2026-04-20:** Added circuit breaker pattern to all external API calls after payment gateway incidents
- **2026-04-15:** Migrated from monolithic checkout to dedicated OrderService for better scaling
- **2026-04-10:** Implemented event sourcing for order state changes to improve audit trails

## Key Decisions
- **Event-driven:** All domain services communicate via events, not direct calls
- **Database-per-service:** Each service owns its data, no shared databases
- **Circuit breakers:** All external dependencies protected by circuit breaker pattern
- **CQRS:** Read/write separation for order processing to handle high query volume

## Integration Patterns
- **API Gateway:** Single entry point, handles auth, rate limiting, routing
- **Service Bus:** Azure Service Bus for reliable async messaging
- **Background Jobs:** Hangfire for scheduled tasks and retry logic
- **Caching:** Redis for session data and frequently accessed catalog info

## Current Technology Stack
- **Backend:** .NET 8, Entity Framework Core
- **Database:** Azure SQL Database with read replicas
- **Messaging:** Azure Service Bus
- **Cache:** Redis Cache
- **Frontend:** React SPA with TypeScript
- **Deployment:** Docker containers on Azure Container Apps
