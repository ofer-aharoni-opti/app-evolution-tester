# Technical Insights

## Current Patterns We Use

### Circuit Breaker Pattern
**Implementation:** Polly library with exponential backoff
**Usage:** All external API calls (payment, shipping, notifications)
**Key Learning:** Start with conservative timeouts (2s), adjust based on actual service SLAs

### Event-Driven Architecture  
**Implementation:** Azure Service Bus with competing consumers
**Usage:** All inter-service communication for non-critical operations
**Key Learning:** Always include correlation IDs for distributed tracing

### CQRS for High-Read Scenarios
**Implementation:** Separate read models using materialized views
**Usage:** Order history, product search, analytics queries
**Key Learning:** Don't over-apply - only use where read/write patterns truly differ

### Database-per-Service
**Implementation:** Each microservice owns its data schema
**Usage:** Users, Catalog, Orders have independent databases
**Key Learning:** Shared reference data (categories, countries) needs special handling

## Lessons Learned

### Performance Insights
- **Database connection pooling:** Default pool sizes too small for our traffic, increased to 50 per service
- **Redis cache warming:** Cold cache causes 2-3x slower response times, implement warming strategies
- **Service Bus batching:** Individual message processing too slow, batch processing improved throughput 5x
- **Image optimization:** Product images were 80% of bandwidth, WebP format reduced by 60%

### Resilience Discoveries
- **Cascade failure pattern:** External service timeouts propagate quickly, circuit breakers are essential
- **Resource exhaustion:** Thread pool starvation under load, async/await patterns critical
- **Event ordering:** Service Bus doesn't guarantee order, design for idempotent operations
- **Database deadlocks:** High concurrency on order updates, implemented optimistic locking

### Development Insights
- **Integration testing:** Most bugs occur at service boundaries, invest heavily in contract testing
- **Feature flags:** Essential for gradual rollout, implement from day one
- **Monitoring correlation:** Distributed tracing (Application Insights) saved 50% debugging time
- **Schema evolution:** Database migrations in microservices need careful coordination

## Recent Insights

### 2026-04-20: External API Resilience
Circuit breakers aren't just about timeouts - also implement bulkhead pattern to isolate different external services. Payment gateway issues shouldn't affect shipping calculations.

### 2026-04-15: Service Decomposition  
When extracting services, events are easier to refactor than direct calls. Start with events even within monolith to prepare for later extraction.

### 2026-04-10: Event Store Design
Event sourcing requires careful thought about event schema evolution. Include version numbers and be prepared for event migrations as business logic evolves.

### 2026-03-25: Data Consistency
Eventual consistency is acceptable for most business operations, but payment-related data needs stronger consistency guarantees. Choose your consistency model per use case.

## Anti-Patterns We Avoid

### Distributed Monolith
**Avoid:** Services that call each other synchronously for every operation
**Instead:** Embrace async communication and accept eventual consistency where possible

### Shared Database Anti-Pattern
**Avoid:** Multiple services writing to the same database tables  
**Instead:** Clear data ownership with well-defined service boundaries

### Chatty Interfaces
**Avoid:** Services making multiple round trips to get related data
**Instead:** Design APIs to return complete data sets in fewer calls

### Configuration Drift
**Avoid:** Different configuration approaches per service
**Instead:** Centralized configuration with service-specific overrides
