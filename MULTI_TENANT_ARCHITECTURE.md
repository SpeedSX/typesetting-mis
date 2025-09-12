# Multi-Tenant Architecture Design
## Typesetting MIS SaaS Platform

### Multi-Tenancy Strategy

#### Approach: Database per Tenant + Shared Services
- **Core Data**: Separate database per tenant for complete isolation
- **Shared Data**: Common database for reference data and system configuration
- **Services**: Shared microservices with tenant context
- **Benefits**: Complete data isolation, tenant-specific optimizations, easier compliance

### Tenant Isolation Layers

#### 1. Application Layer Isolation
```csharp
// Tenant Context Middleware
public class TenantContext
{
    public string TenantId { get; set; }
    public string CompanyId { get; set; }
    public string UserId { get; set; }
    public string Role { get; set; }
    public List<string> Permissions { get; set; }
}

// Request Context Injection
public class TenantContextMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantId = ExtractTenantId(context.Request);
        var tenantContext = await GetTenantContext(tenantId);
        context.Items["TenantContext"] = tenantContext;
        await next(context);
    }
}
```

#### 2. Database Layer Isolation
```sql
-- Tenant-specific database naming convention
-- Format: typesetting_mis_tenant_{tenant_id}

-- Example tenant databases:
-- typesetting_mis_tenant_abc123
-- typesetting_mis_tenant_def456
-- typesetting_mis_tenant_ghi789

-- Shared reference database
-- typesetting_mis_shared
```

#### 3. Service Layer Isolation
```csharp
// Service Factory Pattern
public class ServiceFactory
{
    public static IQuoteService CreateQuoteService(string tenantId)
    {
        var dbConnection = GetTenantDatabase(tenantId);
        return new QuoteService(dbConnection, tenantId);
    }
    
    public static IEquipmentService CreateEquipmentService(string tenantId)
    {
        var dbConnection = GetTenantDatabase(tenantId);
        return new EquipmentService(dbConnection, tenantId);
    }
}
```

### Tenant Management System

#### Tenant Lifecycle Management
```csharp
public interface ITenantLifecycleService
{
    // Onboarding
    Task<TenantInfo> CreateTenantAsync(CompanyData companyData);
    Task SetupTenantDatabaseAsync(string tenantId);
    Task InitializeTenantDataAsync(string tenantId);
    
    // Configuration
    Task UpdateTenantSettingsAsync(string tenantId, TenantSettings settings);
    Task ConfigureTenantFeaturesAsync(string tenantId, List<FeatureConfig> features);
    
    // Maintenance
    Task<BackupInfo> BackupTenantDataAsync(string tenantId);
    Task RestoreTenantDataAsync(string tenantId, string backupId);
    
    // Deactivation
    Task SuspendTenantAsync(string tenantId);
    Task ReactivateTenantAsync(string tenantId);
    Task DeleteTenantAsync(string tenantId);
}
```

#### Tenant Configuration Schema
```csharp
public class TenantConfiguration
{
    public string TenantId { get; set; }
    public CompanyInfo CompanyInfo { get; set; }
    public TenantFeatures Features { get; set; }
    public TenantLimits Limits { get; set; }
    public TenantCustomizations Customizations { get; set; }
    public SubscriptionInfo Subscription { get; set; }
}

public class CompanyInfo
{
    public string Name { get; set; }
    public string Domain { get; set; }
    public string Industry { get; set; }
    public CompanySize Size { get; set; }
}

public enum CompanySize { Small, Medium, Large }

public class TenantFeatures
{
    public bool EquipmentManagement { get; set; }
    public bool InventoryTracking { get; set; }
    public bool AdvancedPricing { get; set; }
    public bool Reporting { get; set; }
    public List<string> Integrations { get; set; }
}
```

### Database Connection Management

#### Connection Pool Strategy
```typescript
class TenantDatabaseManager {
  private connectionPools: Map<string, Pool> = new Map();
  private sharedPool: Pool;
  
  async getTenantConnection(tenantId: string): Promise<Pool> {
    if (!this.connectionPools.has(tenantId)) {
      const config = await this.getTenantDatabaseConfig(tenantId);
      const pool = new Pool(config);
      this.connectionPools.set(tenantId, pool);
    }
    return this.connectionPools.get(tenantId)!;
  }
  
  async getSharedConnection(): Promise<Pool> {
    return this.sharedPool;
  }
  
  async closeTenantConnection(tenantId: string): Promise<void> {
    const pool = this.connectionPools.get(tenantId);
    if (pool) {
      await pool.end();
      this.connectionPools.delete(tenantId);
    }
  }
}
```

#### Database Migration Management
```typescript
class TenantMigrationManager {
  async migrateTenant(tenantId: string, version: string): Promise<void> {
    const connection = await this.getTenantConnection(tenantId);
    const migrations = await this.getMigrations(version);
    
    for (const migration of migrations) {
      await this.executeMigration(connection, migration);
    }
    
    await this.updateTenantVersion(tenantId, version);
  }
  
  async migrateAllTenants(version: string): Promise<void> {
    const tenants = await this.getActiveTenants();
    const promises = tenants.map(tenant => 
      this.migrateTenant(tenant.id, version)
    );
    await Promise.all(promises);
  }
}
```

### API Design and Microservices Structure

#### API Gateway Configuration
```yaml
# API Gateway Routes
routes:
  - path: /api/v1/auth/*
    service: auth-service
    middleware: [rate-limit, cors]
  
  - path: /api/v1/companies/*
    service: company-service
    middleware: [auth, tenant-context]
  
  - path: /api/v1/equipment/*
    service: equipment-service
    middleware: [auth, tenant-context]
  
  - path: /api/v1/quotes/*
    service: quote-service
    middleware: [auth, tenant-context]
  
  - path: /api/v1/orders/*
    service: order-service
    middleware: [auth, tenant-context]
  
  - path: /api/v1/inventory/*
    service: inventory-service
    middleware: [auth, tenant-context]
  
  - path: /api/v1/reports/*
    service: reporting-service
    middleware: [auth, tenant-context]
```

#### Microservices API Structure

##### 1. Authentication Service
```typescript
// POST /api/v1/auth/login
interface LoginRequest {
  email: string;
  password: string;
  tenantDomain: string;
}

interface LoginResponse {
  token: string;
  refreshToken: string;
  user: UserInfo;
  tenant: TenantInfo;
  permissions: string[];
}

// POST /api/v1/auth/refresh
interface RefreshRequest {
  refreshToken: string;
}

// POST /api/v1/auth/logout
interface LogoutRequest {
  token: string;
}
```

##### 2. Company Management Service
```typescript
// GET /api/v1/companies/profile
interface CompanyProfile {
  id: string;
  name: string;
  domain: string;
  settings: CompanySettings;
  subscription: SubscriptionInfo;
}

// PUT /api/v1/companies/profile
interface UpdateCompanyRequest {
  name?: string;
  settings?: CompanySettings;
}

// GET /api/v1/companies/users
interface UsersResponse {
  users: UserInfo[];
  total: number;
  page: number;
  limit: number;
}

// POST /api/v1/companies/users
interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  roleId: string;
  sendInvitation: boolean;
}
```

##### 3. Equipment Management Service
```typescript
// GET /api/v1/equipment
interface EquipmentListRequest {
  category?: string;
  status?: string;
  search?: string;
  page?: number;
  limit?: number;
}

interface EquipmentListResponse {
  equipment: Equipment[];
  total: number;
  page: number;
  limit: number;
}

// POST /api/v1/equipment
interface CreateEquipmentRequest {
  categoryId: string;
  name: string;
  model?: string;
  serialNumber?: string;
  purchaseDate?: string;
  purchaseCost?: number;
  location?: string;
  capabilities?: EquipmentCapability[];
}

// PUT /api/v1/equipment/:id
interface UpdateEquipmentRequest {
  name?: string;
  model?: string;
  status?: string;
  location?: string;
  capabilities?: EquipmentCapability[];
}
```

##### 4. Quote Management Service
```typescript
// GET /api/v1/quotes
interface QuoteListRequest {
  status?: string;
  customerId?: string;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  limit?: number;
}

interface QuoteListResponse {
  quotes: Quote[];
  total: number;
  page: number;
  limit: number;
}

// POST /api/v1/quotes
interface CreateQuoteRequest {
  customerId: string;
  items: QuoteItem[];
  validUntil?: string;
  notes?: string;
}

// POST /api/v1/quotes/:id/convert
interface ConvertQuoteRequest {
  orderNumber?: string;
  dueDate?: string;
  notes?: string;
}
```

##### 5. Order Management Service
```typescript
// GET /api/v1/orders
interface OrderListRequest {
  status?: string;
  customerId?: string;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  limit?: number;
}

interface OrderListResponse {
  orders: Order[];
  total: number;
  page: number;
  limit: number;
}

// POST /api/v1/orders
interface CreateOrderRequest {
  customerId: string;
  quoteId?: string;
  items: OrderItem[];
  dueDate?: string;
  notes?: string;
}

// PUT /api/v1/orders/:id/status
interface UpdateOrderStatusRequest {
  status: string;
  notes?: string;
}
```

##### 6. Pricing Engine Service
```typescript
// POST /api/v1/pricing/calculate
interface PricingCalculationRequest {
  items: PricingItem[];
  customerId?: string;
  orderType?: string;
  applyRules?: boolean;
}

interface PricingCalculationResponse {
  items: CalculatedItem[];
  subtotal: number;
  discounts: Discount[];
  taxes: Tax[];
  total: number;
}

// GET /api/v1/pricing/rules
interface PricingRulesResponse {
  rules: PricingRule[];
  total: number;
}

// POST /api/v1/pricing/rules
interface CreatePricingRuleRequest {
  name: string;
  ruleType: string;
  conditions: PricingCondition[];
  calculation: PricingCalculation;
  priority: number;
}
```

### Tenant Data Isolation Implementation

#### Row-Level Security (RLS) Policies
```sql
-- Enable RLS on all tenant tables
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE equipment ENABLE ROW LEVEL SECURITY;
ALTER TABLE quotes ENABLE ROW LEVEL SECURITY;
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;

-- Create tenant isolation policies
CREATE POLICY tenant_isolation_users ON users
    FOR ALL TO application_role
    USING (company_id = current_setting('app.current_company_id')::uuid);

CREATE POLICY tenant_isolation_equipment ON equipment
    FOR ALL TO application_role
    USING (company_id = current_setting('app.current_company_id')::uuid);

-- Set tenant context in application
SET app.current_company_id = 'company-uuid-here';
```

#### Application-Level Tenant Context
```typescript
class TenantContextManager {
  private static context: Map<string, TenantContext> = new Map();
  
  static setContext(requestId: string, context: TenantContext): void {
    this.context.set(requestId, context);
  }
  
  static getContext(requestId: string): TenantContext | undefined {
    return this.context.get(requestId);
  }
  
  static clearContext(requestId: string): void {
    this.context.delete(requestId);
  }
}

// Middleware to inject tenant context
app.use((req, res, next) => {
  const requestId = req.headers['x-request-id'] as string;
  const tenantId = extractTenantId(req);
  
  getTenantContext(tenantId).then(context => {
    TenantContextManager.setContext(requestId, context);
    next();
  });
});
```

### Tenant Onboarding Process

#### Automated Tenant Provisioning
```typescript
class TenantProvisioningService {
  async provisionTenant(companyData: CompanyData): Promise<TenantInfo> {
    // 1. Create tenant record
    const tenant = await this.createTenantRecord(companyData);
    
    // 2. Create tenant database
    await this.createTenantDatabase(tenant.id);
    
    // 3. Run database migrations
    await this.migrateTenantDatabase(tenant.id);
    
    // 4. Initialize default data
    await this.initializeTenantData(tenant.id);
    
    // 5. Create admin user
    await this.createAdminUser(tenant.id, companyData.adminUser);
    
    // 6. Configure tenant settings
    await this.configureTenantSettings(tenant.id, companyData.settings);
    
    // 7. Send welcome email
    await this.sendWelcomeEmail(tenant.id);
    
    return tenant;
  }
  
  private async createTenantDatabase(tenantId: string): Promise<void> {
    const dbName = `typesetting_mis_tenant_${tenantId}`;
    await this.databaseManager.createDatabase(dbName);
  }
  
  private async initializeTenantData(tenantId: string): Promise<void> {
    const connection = await this.getTenantConnection(tenantId);
    
    // Insert default equipment categories
    await this.insertDefaultEquipmentCategories(connection);
    
    // Insert default roles
    await this.insertDefaultRoles(connection);
    
    // Insert default pricing rules
    await this.insertDefaultPricingRules(connection);
  }
}
```

### Monitoring and Observability

#### Tenant-Specific Metrics
```typescript
interface TenantMetrics {
  tenantId: string;
  activeUsers: number;
  quotesGenerated: number;
  ordersProcessed: number;
  apiCalls: number;
  storageUsed: number;
  lastActivity: Date;
  performance: {
    avgResponseTime: number;
    errorRate: number;
    uptime: number;
  };
}

class TenantMonitoringService {
  async collectTenantMetrics(tenantId: string): Promise<TenantMetrics> {
    const connection = await this.getTenantConnection(tenantId);
    
    const metrics = await Promise.all([
      this.getActiveUsersCount(connection),
      this.getQuotesCount(connection),
      this.getOrdersCount(connection),
      this.getApiCallsCount(tenantId),
      this.getStorageUsed(tenantId),
      this.getLastActivity(connection),
      this.getPerformanceMetrics(tenantId)
    ]);
    
    return {
      tenantId,
      activeUsers: metrics[0],
      quotesGenerated: metrics[1],
      ordersProcessed: metrics[2],
      apiCalls: metrics[3],
      storageUsed: metrics[4],
      lastActivity: metrics[5],
      performance: metrics[6]
    };
  }
}
```

### Security Considerations

#### Tenant Data Encryption
```typescript
class TenantEncryptionService {
  async encryptTenantData(tenantId: string, data: any): Promise<string> {
    const key = await this.getTenantEncryptionKey(tenantId);
    return this.encrypt(JSON.stringify(data), key);
  }
  
  async decryptTenantData(tenantId: string, encryptedData: string): Promise<any> {
    const key = await this.getTenantEncryptionKey(tenantId);
    return JSON.parse(this.decrypt(encryptedData, key));
  }
  
  private async getTenantEncryptionKey(tenantId: string): Promise<string> {
    // Retrieve tenant-specific encryption key
    // Keys are stored in AWS KMS or similar key management service
    return await this.keyManagementService.getKey(tenantId);
  }
}
```

#### Audit Logging
```typescript
interface AuditLogEntry {
  id: string;
  tenantId: string;
  userId: string;
  action: string;
  resource: string;
  resourceId: string;
  changes: any;
  timestamp: Date;
  ipAddress: string;
  userAgent: string;
}

class AuditLoggingService {
  async logAction(tenantId: string, userId: string, action: string, resource: string, changes: any): Promise<void> {
    const logEntry: AuditLogEntry = {
      id: generateId(),
      tenantId,
      userId,
      action,
      resource,
      resourceId: changes.id,
      changes,
      timestamp: new Date(),
      ipAddress: this.getClientIP(),
      userAgent: this.getUserAgent()
    };
    
    await this.auditLogRepository.create(logEntry);
  }
}
```

This multi-tenant architecture provides complete data isolation, scalable tenant management, and robust security measures while maintaining high performance and ease of management.
