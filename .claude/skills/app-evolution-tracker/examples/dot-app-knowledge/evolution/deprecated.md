# Deprecated & Removed

## Recently Deprecated

### 2026-04-18: Synchronous Order Validation
**Removed:** Direct HTTP calls from OrderService to InventoryService for real-time stock checks
**Why:** Causing timeout cascades during high traffic periods
**Replaced With:** Event-driven inventory reservation with eventual consistency
**Don't Re-introduce:** Synchronous calls between services for non-critical validations

### 2026-04-12: Shared Authentication Database
**Removed:** Common identity database accessed by multiple services  
**Why:** Created tight coupling and deployment dependencies
**Replaced With:** JWT tokens with service-specific user projections
**Don't Re-introduce:** Shared data stores between services

### 2026-03-30: Manual Retry Logic  
**Removed:** Custom retry implementations scattered throughout codebase
**Why:** Inconsistent behavior and difficult to tune/monitor
**Replaced With:** Polly library with standardized policies
**Don't Re-introduce:** Hand-rolled retry logic

### 2026-03-20: Polling-Based Background Jobs
**Removed:** Timer-based polling for processing pending orders
**Why:** Inefficient resource usage and scaling issues
**Replaced With:** Event-driven processing via Service Bus
**Don't Re-introduce:** Polling patterns for real-time processing

## Historical Deprecations

### Synchronous Service Communication (deprecated 2026-02-15)
**Pattern:** Services calling each other via HTTP for business operations
**Why Removed:** Created tight coupling and cascade failure risks
**Current Approach:** Event-driven communication with async processing
**Lesson:** Synchronous calls only for truly synchronous operations (like real-time validation)

### Repository Pattern with Generic CRUD (deprecated 2026-01-20)
**Pattern:** Generic `IRepository<T>` interfaces for all data access
**Why Removed:** Led to leaky abstractions and complex query compositions
**Current Approach:** Domain-specific query interfaces with focused methods
**Lesson:** Generic patterns often hide important domain concepts

### Shared Entity Models Across Services (deprecated 2025-12-10)
**Pattern:** Common data models shared via NuGet packages
**Why Removed:** Changes required coordinated deployment across all services
**Current Approach:** Each service defines its own models, use events for integration
**Lesson:** Shared code creates shared deployment pain

## Guidelines for Avoiding Reintroduction

### Before Adding Synchronous Service Calls
- Question: Is this truly synchronous by business requirements?
- Consider: Can this be eventually consistent?
- If synchronous needed: Implement circuit breakers and timeouts

### Before Creating Shared Components
- Question: Will changes to this component require coordinated deployments?
- Consider: Can each service implement its own version?
- If sharing needed: Version carefully and maintain backward compatibility

### Before Implementing Custom Infrastructure  
- Question: Is there a proven library or service for this?
- Consider: Total cost of ownership for custom solutions
- If custom needed: Plan for monitoring, documentation, and maintenance

### Before Direct Database Access Across Services
- Question: Who owns this data?
- Consider: Events or APIs for cross-service data access
- If shared access needed: Create dedicated data service with clear ownership
