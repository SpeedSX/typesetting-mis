# Database Schema Design
## Typesetting MIS - Multi-Tenant Architecture

> **Note**: This is the basic database schema. For complex book production workflows with multi-component products, work orders, and production processes, see [ENHANCED_DATABASE_SCHEMA.md](ENHANCED_DATABASE_SCHEMA.md).

### Database Strategy
- **Primary Database**: PostgreSQL 14+ per tenant
- **Shared Database**: Common reference data and system configuration
- **Cache Layer**: Redis for session management and frequently accessed data

### Core Entity Relationship Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│    Companies    │    │      Users      │    │      Roles      │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ id (PK)         │    │ id (PK)         │    │ id (PK)         │
│ name            │    │ company_id (FK) │    │ name            │
│ domain          │    │ email           │    │ permissions     │
│ settings        │    │ role_id (FK)    │    │ is_system       │
│ created_at      │    │ is_active       │    │ created_at      │
│ updated_at      │    │ created_at      │    └─────────────────┘
└─────────────────┘    │ updated_at      │             │
         │              └─────────────────┘             │
         │                       │                      │
         │                       └──────────────────────┘
         │
         │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Equipment     │    │   Equipment     │    │   Equipment     │
│   Categories    │    │                 │    │   Capabilities  │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ id (PK)         │    │ id (PK)         │    │ id (PK)         │
│ name            │    │ company_id (FK) │    │ equipment_id(FK)│
│ description     │    │ category_id(FK) │    │ capability_name │
│ created_at      │    │ name            │    │ value           │
└─────────────────┘    │ model           │    │ unit            │
         │              │ serial_number   │    │ created_at      │
         │              │ status          │    └─────────────────┘
         │              │ purchase_date   │             │
         │              │ created_at      │             │
         │              └─────────────────┘             │
         │                       │                      │
         │                       └──────────────────────┘
         │
         │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Services      │    │    Products     │    │   Pricing       │
│                 │    │                 │    │   Rules         │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ id (PK)         │    │ id (PK)         │    │ id (PK)         │
│ company_id (FK) │    │ company_id (FK) │    │ company_id (FK) │
│ name            │    │ name            │    │ name            │
│ description     │    │ description     │    │ rule_type       │
│ base_price      │    │ base_price      │    │ conditions      │
│ unit            │    │ unit            │    │ calculation     │
│ is_active       │    │ is_active       │    │ priority        │
│ created_at      │    │ created_at      │    │ is_active       │
└─────────────────┘    └─────────────────┘    │ created_at      │
         │                       │              └─────────────────┘
         │                       │                       │
         │                       │                       │
         │                       │              ┌─────────────────┐
         │                       │              │   Quotes        │
         │                       │              ├─────────────────┤
         │                       │              │ id (PK)         │
         │                       │              │ company_id (FK) │
         │                       │              │ quote_number    │
         │                       │              │ customer_id (FK)│
         │                       │              │ status          │
         │                       │              │ total_amount    │
         │                       │              │ valid_until     │
         │                       │              │ created_at      │
         │                       │              │ updated_at      │
         │                       │              └─────────────────┘
         │                       │                       │
         │                       │                       │
         │                       │              ┌─────────────────┐
         │                       │              │  Quote Items    │
         │                       │              ├─────────────────┤
         │                       │              │ id (PK)         │
         │                       │              │ quote_id (FK)   │
         │                       │              │ item_type       │
         │                       │              │ item_id (FK)    │
         │                       │              │ quantity        │
         │                       │              │ unit_price      │
         │                       │              │ total_price     │
         │                       │              │ created_at      │
         │                       │              └─────────────────┘
         │                       │
         │                       │
         │              ┌─────────────────┐    ┌─────────────────┐
         │              │     Orders      │    │   Order Items   │
         │              ├─────────────────┤    ├─────────────────┤
         │              │ id (PK)         │    │ id (PK)         │
         │              │ company_id (FK) │    │ order_id (FK)   │
         │              │ order_number    │    │ item_type       │
         │              │ customer_id (FK)│    │ item_id (FK)    │
         │              │ quote_id (FK)   │    │ quantity        │
         │              │ status          │    │ unit_price      │
         │              │ total_amount    │    │ total_price     │
         │              │ due_date        │    │ created_at      │
         │              │ created_at      │    └─────────────────┘
         │              │ updated_at      │
         │              └─────────────────┘
         │
         │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Customers     │    │   Inventory     │    │   Invoices      │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ id (PK)         │    │ id (PK)         │    │ id (PK)         │
│ company_id (FK) │    │ company_id (FK) │    │ company_id (FK) │
│ name            │    │ item_name       │    │ invoice_number  │
│ email           │    │ item_type       │    │ customer_id (FK)│
│ phone           │    │ quantity        │    │ order_id (FK)   │
│ address         │    │ unit_cost       │    │ amount          │
│ tax_id          │    │ reorder_point   │    │ status          │
│ is_active       │    │ supplier        │    │ due_date        │
│ created_at      │    │ created_at      │    │ created_at      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Detailed Table Schemas

#### 1. Companies Table
```sql
CREATE TABLE companies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    domain VARCHAR(255) UNIQUE NOT NULL,
    settings JSONB DEFAULT '{}',
    subscription_plan VARCHAR(50) DEFAULT 'basic',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_companies_domain ON companies(domain);
CREATE INDEX idx_companies_active ON companies(is_active);
```

#### 2. Users Table
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    role_id UUID NOT NULL REFERENCES roles(id),
    is_active BOOLEAN DEFAULT true,
    last_login TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, email)
);

CREATE INDEX idx_users_company ON users(company_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_active ON users(is_active);
```

#### 3. Roles Table
```sql
CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    permissions JSONB DEFAULT '[]',
    is_system BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, name)
);

CREATE INDEX idx_roles_company ON roles(company_id);
```

#### 4. Equipment Categories Table
```sql
CREATE TABLE equipment_categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### 5. Equipment Table
```sql
CREATE TABLE equipment (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    category_id UUID NOT NULL REFERENCES equipment_categories(id),
    name VARCHAR(255) NOT NULL,
    model VARCHAR(255),
    serial_number VARCHAR(255),
    status VARCHAR(50) DEFAULT 'active',
    purchase_date DATE,
    purchase_cost DECIMAL(12,2),
    location VARCHAR(255),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_equipment_company ON equipment(company_id);
CREATE INDEX idx_equipment_category ON equipment(category_id);
CREATE INDEX idx_equipment_status ON equipment(status);
```

#### 6. Equipment Capabilities Table
```sql
CREATE TABLE equipment_capabilities (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id UUID NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    capability_name VARCHAR(100) NOT NULL,
    value VARCHAR(255) NOT NULL,
    unit VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_equipment_capabilities_equipment ON equipment_capabilities(equipment_id);
```

#### 7. Services Table
```sql
CREATE TABLE services (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    base_price DECIMAL(12,2) NOT NULL,
    unit VARCHAR(50) NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_services_company ON services(company_id);
CREATE INDEX idx_services_active ON services(is_active);
```

#### 8. Products Table
```sql
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    base_price DECIMAL(12,2) NOT NULL,
    unit VARCHAR(50) NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_products_company ON products(company_id);
CREATE INDEX idx_products_active ON products(is_active);
```

#### 9. Pricing Rules Table
```sql
CREATE TABLE pricing_rules (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    rule_type VARCHAR(50) NOT NULL, -- 'percentage', 'fixed', 'tiered'
    conditions JSONB NOT NULL,
    calculation JSONB NOT NULL,
    priority INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_pricing_rules_company ON pricing_rules(company_id);
CREATE INDEX idx_pricing_rules_active ON pricing_rules(is_active);
CREATE INDEX idx_pricing_rules_priority ON pricing_rules(priority);
```

#### 10. Customers Table
```sql
CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(50),
    address JSONB,
    tax_id VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_customers_company ON customers(company_id);
CREATE INDEX idx_customers_active ON customers(is_active);
```

#### 11. Quotes Table
```sql
CREATE TABLE quotes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    quote_number VARCHAR(50) NOT NULL,
    customer_id UUID NOT NULL REFERENCES customers(id),
    status VARCHAR(50) DEFAULT 'draft', -- 'draft', 'sent', 'accepted', 'rejected', 'expired'
    total_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    valid_until DATE,
    notes TEXT,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, quote_number)
);

CREATE INDEX idx_quotes_company ON quotes(company_id);
CREATE INDEX idx_quotes_customer ON quotes(customer_id);
CREATE INDEX idx_quotes_status ON quotes(status);
CREATE INDEX idx_quotes_created_by ON quotes(created_by);
```

#### 12. Quote Items Table
```sql
CREATE TABLE quote_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    quote_id UUID NOT NULL REFERENCES quotes(id) ON DELETE CASCADE,
    item_type VARCHAR(50) NOT NULL, -- 'service', 'product'
    item_id UUID NOT NULL,
    quantity DECIMAL(10,3) NOT NULL,
    unit_price DECIMAL(12,2) NOT NULL,
    total_price DECIMAL(12,2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_quote_items_quote ON quote_items(quote_id);
CREATE INDEX idx_quote_items_type_id ON quote_items(item_type, item_id);
```

#### 13. Orders Table
```sql
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    order_number VARCHAR(50) NOT NULL,
    customer_id UUID NOT NULL REFERENCES customers(id),
    quote_id UUID REFERENCES quotes(id),
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'in_progress', 'completed', 'cancelled'
    total_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    due_date DATE,
    notes TEXT,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, order_number)
);

CREATE INDEX idx_orders_company ON orders(company_id);
CREATE INDEX idx_orders_customer ON orders(customer_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_created_by ON orders(created_by);
```

#### 14. Order Items Table
```sql
CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    item_type VARCHAR(50) NOT NULL, -- 'service', 'product'
    item_id UUID NOT NULL,
    quantity DECIMAL(10,3) NOT NULL,
    unit_price DECIMAL(12,2) NOT NULL,
    total_price DECIMAL(12,2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_order_items_type_id ON order_items(item_type, item_id);
```

#### 15. Inventory Table
```sql
CREATE TABLE inventory (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    item_name VARCHAR(255) NOT NULL,
    item_type VARCHAR(50) NOT NULL, -- 'material', 'supply', 'tool'
    quantity DECIMAL(10,3) NOT NULL DEFAULT 0,
    unit VARCHAR(50) NOT NULL,
    unit_cost DECIMAL(12,2),
    reorder_point DECIMAL(10,3) DEFAULT 0,
    supplier VARCHAR(255),
    location VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_inventory_company ON inventory(company_id);
CREATE INDEX idx_inventory_type ON inventory(item_type);
CREATE INDEX idx_inventory_reorder ON inventory(reorder_point);
```

#### 16. Invoices Table
```sql
CREATE TABLE invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    invoice_number VARCHAR(50) NOT NULL,
    customer_id UUID NOT NULL REFERENCES customers(id),
    order_id UUID REFERENCES orders(id),
    amount DECIMAL(12,2) NOT NULL,
    status VARCHAR(50) DEFAULT 'draft', -- 'draft', 'sent', 'paid', 'overdue'
    due_date DATE NOT NULL,
    paid_date DATE,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, invoice_number)
);

CREATE INDEX idx_invoices_company ON invoices(company_id);
CREATE INDEX idx_invoices_customer ON invoices(customer_id);
CREATE INDEX idx_invoices_status ON invoices(status);
CREATE INDEX idx_invoices_due_date ON invoices(due_date);
```

### Multi-Tenant Data Isolation

#### Row-Level Security (RLS) Implementation
```sql
-- Enable RLS on all tenant-specific tables
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE equipment ENABLE ROW LEVEL SECURITY;
ALTER TABLE services ENABLE ROW LEVEL SECURITY;
ALTER TABLE products ENABLE ROW LEVEL SECURITY;
-- ... (repeat for all tenant tables)

-- Create RLS policies
CREATE POLICY tenant_isolation_policy ON users
    FOR ALL TO application_role
    USING (company_id = current_setting('app.current_company_id')::uuid);

-- Set company context in application
SET app.current_company_id = 'company-uuid-here';
```

### Database Optimization

#### Indexing Strategy
- **Primary Keys**: UUID with gen_random_uuid() for distributed systems
- **Foreign Keys**: Indexed for join performance
- **Query Patterns**: Composite indexes for common query patterns
- **Text Search**: GIN indexes for JSONB columns
- **Time-based Queries**: Indexes on created_at, updated_at columns

#### Partitioning Strategy
```sql
-- Partition large tables by company_id for better performance
CREATE TABLE quotes_partitioned (
    LIKE quotes INCLUDING ALL
) PARTITION BY HASH (company_id);

-- Create partitions for each company
CREATE TABLE quotes_company_1 PARTITION OF quotes_partitioned
    FOR VALUES WITH (modulus 100, remainder 0);
```

### Data Migration Strategy

#### Tenant Onboarding
1. Create new database schema for tenant
2. Copy default configuration data
3. Set up initial user accounts and roles
4. Configure company-specific settings
5. Run data validation and integrity checks

#### Schema Evolution
1. Version-controlled migration scripts
2. Backward-compatible changes
3. Data transformation for breaking changes
4. Rollback procedures for failed migrations
