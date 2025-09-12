# Enhanced Database Schema for Complex Book Production
## Typesetting MIS - Multi-Component Product Support

### Overview
This enhanced schema extends the [base database design](DATABASE_SCHEMA.md) to properly handle complex book production workflows, including multi-component products, production processes, material specifications, and work order management.

> **Prerequisites**: This schema builds upon the tables defined in [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md). Ensure you have implemented the base schema first.

### New Tables for Complex Production

#### 1. Product Categories Table
```sql
CREATE TABLE product_categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    parent_id UUID REFERENCES product_categories(id),
    is_leaf BOOLEAN DEFAULT false, -- true for actual products, false for categories
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_product_categories_company ON product_categories(company_id);
CREATE INDEX idx_product_categories_parent ON product_categories(parent_id);
```

#### 2. Complex Products Table (Books, Magazines, etc.)
```sql
CREATE TABLE complex_products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    category_id UUID NOT NULL REFERENCES product_categories(id),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    product_type VARCHAR(50) NOT NULL, -- 'book', 'magazine', 'brochure', 'catalog'
    specifications JSONB NOT NULL, -- Overall product specifications
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_complex_products_company ON complex_products(company_id);
CREATE INDEX idx_complex_products_category ON complex_products(category_id);
CREATE INDEX idx_complex_products_type ON complex_products(product_type);
```

#### 3. Product Components Table (Cover, Pages, Inserts, etc.)
```sql
CREATE TABLE product_components (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complex_product_id UUID NOT NULL REFERENCES complex_products(id) ON DELETE CASCADE,
    component_name VARCHAR(255) NOT NULL, -- 'cover', 'pages', 'insert', 'dust_jacket'
    component_type VARCHAR(50) NOT NULL, -- 'cover', 'text_block', 'insert', 'endpaper'
    specifications JSONB NOT NULL, -- Component-specific specifications
    quantity INTEGER NOT NULL DEFAULT 1,
    sort_order INTEGER DEFAULT 0,
    is_required BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_product_components_product ON product_components(complex_product_id);
CREATE INDEX idx_product_components_type ON product_components(component_type);
```

#### 4. Materials Table
```sql
CREATE TABLE materials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    material_type VARCHAR(50) NOT NULL, -- 'paper', 'cardboard', 'fabric', 'plastic', 'metal'
    specifications JSONB NOT NULL, -- Weight, color, finish, etc.
    supplier_id UUID REFERENCES suppliers(id),
    unit_cost DECIMAL(12,4) NOT NULL,
    unit VARCHAR(50) NOT NULL, -- 'sheet', 'roll', 'meter', 'kg'
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_materials_company ON materials(company_id);
CREATE INDEX idx_materials_type ON materials(material_type);
CREATE INDEX idx_materials_supplier ON materials(supplier_id);
```

#### 5. Suppliers Table
```sql
CREATE TABLE suppliers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    contact_person VARCHAR(255),
    email VARCHAR(255),
    phone VARCHAR(50),
    address JSONB,
    payment_terms VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_suppliers_company ON suppliers(company_id);
```

#### 6. Production Processes Table
```sql
CREATE TABLE production_processes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    process_type VARCHAR(50) NOT NULL, -- 'printing', 'binding', 'cutting', 'folding', 'laminating'
    equipment_required JSONB, -- Array of equipment IDs
    materials_required JSONB, -- Array of material requirements
    estimated_duration INTEGER, -- in minutes
    setup_time INTEGER, -- in minutes
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_production_processes_company ON production_processes(company_id);
CREATE INDEX idx_production_processes_type ON production_processes(process_type);
```

#### 7. Component Process Requirements Table
```sql
CREATE TABLE component_process_requirements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_component_id UUID NOT NULL REFERENCES product_components(id) ON DELETE CASCADE,
    process_id UUID NOT NULL REFERENCES production_processes(id),
    process_order INTEGER NOT NULL, -- Order in which processes must be executed
    material_id UUID REFERENCES materials(id),
    material_quantity DECIMAL(10,3),
    equipment_id UUID REFERENCES equipment(id),
    specifications JSONB, -- Process-specific specifications
    estimated_duration INTEGER, -- Override default process duration
    is_required BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_component_processes_component ON component_process_requirements(product_component_id);
CREATE INDEX idx_component_processes_process ON component_process_requirements(process_id);
CREATE INDEX idx_component_processes_order ON component_process_requirements(process_order);
```

#### 8. Work Orders Table
```sql
CREATE TABLE work_orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    order_id UUID NOT NULL REFERENCES orders(id),
    work_order_number VARCHAR(50) NOT NULL,
    product_component_id UUID NOT NULL REFERENCES product_components(id),
    process_id UUID NOT NULL REFERENCES production_processes(id),
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'in_progress', 'completed', 'cancelled'
    priority INTEGER DEFAULT 0,
    assigned_equipment_id UUID REFERENCES equipment(id),
    assigned_user_id UUID REFERENCES users(id),
    scheduled_start TIMESTAMP,
    scheduled_end TIMESTAMP,
    actual_start TIMESTAMP,
    actual_end TIMESTAMP,
    quantity INTEGER NOT NULL,
    completed_quantity INTEGER DEFAULT 0,
    specifications JSONB,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, work_order_number)
);

CREATE INDEX idx_work_orders_company ON work_orders(company_id);
CREATE INDEX idx_work_orders_order ON work_orders(order_id);
CREATE INDEX idx_work_orders_component ON work_orders(product_component_id);
CREATE INDEX idx_work_orders_process ON work_orders(process_id);
CREATE INDEX idx_work_orders_status ON work_orders(status);
CREATE INDEX idx_work_orders_equipment ON work_orders(assigned_equipment_id);
CREATE INDEX idx_work_orders_user ON work_orders(assigned_user_id);
```

#### 9. Work Order Dependencies Table
```sql
CREATE TABLE work_order_dependencies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    work_order_id UUID NOT NULL REFERENCES work_orders(id) ON DELETE CASCADE,
    depends_on_work_order_id UUID NOT NULL REFERENCES work_orders(id) ON DELETE CASCADE,
    dependency_type VARCHAR(50) NOT NULL, -- 'finish_to_start', 'start_to_start', 'finish_to_finish'
    lag_time INTEGER DEFAULT 0, -- Delay in minutes
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(work_order_id, depends_on_work_order_id)
);

CREATE INDEX idx_work_order_deps_work_order ON work_order_dependencies(work_order_id);
CREATE INDEX idx_work_order_deps_depends_on ON work_order_dependencies(depends_on_work_order_id);
```

#### 10. Production Batches Table
```sql
CREATE TABLE production_batches (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    batch_number VARCHAR(50) NOT NULL,
    work_order_id UUID NOT NULL REFERENCES work_orders(id),
    batch_size INTEGER NOT NULL,
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'in_progress', 'completed', 'cancelled'
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    quality_notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, batch_number)
);

CREATE INDEX idx_production_batches_company ON production_batches(company_id);
CREATE INDEX idx_production_batches_work_order ON production_batches(work_order_id);
CREATE INDEX idx_production_batches_status ON production_batches(status);
```

#### 11. Quality Control Table
```sql
CREATE TABLE quality_control (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    work_order_id UUID NOT NULL REFERENCES work_orders(id),
    batch_id UUID REFERENCES production_batches(id),
    inspector_id UUID NOT NULL REFERENCES users(id),
    inspection_type VARCHAR(50) NOT NULL, -- 'incoming', 'in_process', 'final'
    status VARCHAR(50) NOT NULL, -- 'passed', 'failed', 'conditional'
    defects JSONB, -- Array of defect descriptions
    corrective_actions TEXT,
    inspection_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_quality_control_company ON quality_control(company_id);
CREATE INDEX idx_quality_control_work_order ON quality_control(work_order_id);
CREATE INDEX idx_quality_control_batch ON quality_control(batch_id);
CREATE INDEX idx_quality_control_inspector ON quality_control(inspector_id);
```

### Enhanced Order Items Table
```sql
-- Update existing order_items table to support complex products
ALTER TABLE order_items ADD COLUMN complex_product_id UUID REFERENCES complex_products(id);
ALTER TABLE order_items ADD COLUMN component_configuration JSONB; -- Component-specific settings
ALTER TABLE order_items ADD COLUMN production_notes TEXT;

CREATE INDEX idx_order_items_complex_product ON order_items(complex_product_id);
```

### Example Data for Book Production

#### Complex Product Example: Hardcover Book
```sql
-- 1. Create product category
INSERT INTO product_categories (company_id, name, description, is_leaf) 
VALUES ('company-uuid', 'Books', 'Book products', false);

INSERT INTO product_categories (company_id, name, description, parent_id, is_leaf) 
VALUES ('company-uuid', 'Hardcover Books', 'Hardcover book products', 
        (SELECT id FROM product_categories WHERE name = 'Books'), false);

-- 2. Create complex product
INSERT INTO complex_products (company_id, category_id, name, product_type, specifications)
VALUES (
    'company-uuid',
    (SELECT id FROM product_categories WHERE name = 'Hardcover Books'),
    'Standard Hardcover Book',
    'book',
    '{
        "page_count": 300,
        "trim_size": "6x9",
        "binding_type": "hardcover",
        "cover_finish": "matte",
        "paper_weight": "80lb",
        "color_pages": 0,
        "black_white_pages": 300
    }'
);

-- 3. Create product components
INSERT INTO product_components (complex_product_id, component_name, component_type, specifications, quantity, sort_order)
VALUES 
    -- Cover component
    ((SELECT id FROM complex_products WHERE name = 'Standard Hardcover Book'),
     'Hardcover', 'cover', 
     '{"material": "cardboard", "thickness": "0.125", "finish": "matte", "spine_width": "1.5"}', 
     1, 1),
    
    -- Text block component  
    ((SELECT id FROM complex_products WHERE name = 'Standard Hardcover Book'),
     'Text Block', 'text_block',
     '{"page_count": 300, "paper_weight": "80lb", "paper_type": "white", "trim_size": "6x9"}',
     1, 2),
    
    -- Endpapers
    ((SELECT id FROM complex_products WHERE name = 'Standard Hardcover Book'),
     'Endpapers', 'endpaper',
     '{"material": "paper", "weight": "100lb", "color": "white", "quantity": 4}',
     1, 3);

-- 4. Create production processes
INSERT INTO production_processes (company_id, name, process_type, estimated_duration, setup_time)
VALUES 
    ('company-uuid', 'Digital Printing', 'printing', 60, 15),
    ('company-uuid', 'Offset Printing', 'printing', 120, 30),
    ('company-uuid', 'Perfect Binding', 'binding', 45, 20),
    ('company-uuid', 'Saddle Stitching', 'binding', 30, 10),
    ('company-uuid', 'Hardcover Binding', 'binding', 90, 30),
    ('company-uuid', 'Cutting', 'cutting', 15, 5),
    ('company-uuid', 'Folding', 'folding', 20, 5),
    ('company-uuid', 'Laminating', 'laminating', 30, 10);

-- 5. Create materials
INSERT INTO materials (company_id, name, material_type, specifications, unit_cost, unit)
VALUES 
    ('company-uuid', '80lb White Paper', 'paper', 
     '{"weight": "80lb", "color": "white", "finish": "smooth", "size": "25x38"}', 
     0.15, 'sheet'),
    ('company-uuid', 'Cardboard 0.125"', 'cardboard',
     '{"thickness": "0.125", "color": "brown", "finish": "matte"}',
     0.25, 'sheet'),
    ('company-uuid', 'Binding Glue', 'adhesive',
     '{"type": "PVA", "viscosity": "medium", "drying_time": "24h"}',
     0.05, 'ounce');

-- 6. Define component process requirements
-- Cover processes
INSERT INTO component_process_requirements (product_component_id, process_id, process_order, material_id, material_quantity, equipment_id)
VALUES 
    -- Cover printing
    ((SELECT id FROM product_components WHERE component_name = 'Hardcover'),
     (SELECT id FROM production_processes WHERE name = 'Digital Printing'),
     1, (SELECT id FROM materials WHERE name = 'Cardboard 0.125"'), 1, 
     (SELECT id FROM equipment WHERE name = 'Digital Press 1')),
    
    -- Cover cutting
    ((SELECT id FROM product_components WHERE component_name = 'Hardcover'),
     (SELECT id FROM production_processes WHERE name = 'Cutting'),
     2, NULL, NULL, (SELECT id FROM equipment WHERE name = 'Cutting Machine 1'));

-- Text block processes  
INSERT INTO component_process_requirements (product_component_id, process_id, process_order, material_id, material_quantity, equipment_id)
VALUES 
    -- Text block printing
    ((SELECT id FROM product_components WHERE component_name = 'Text Block'),
     (SELECT id FROM production_processes WHERE name = 'Offset Printing'),
     1, (SELECT id FROM materials WHERE name = '80lb White Paper'), 150, -- 300 pages / 2 = 150 sheets
     (SELECT id FROM equipment WHERE name = 'Offset Press 1')),
    
    -- Text block folding
    ((SELECT id FROM product_components WHERE component_name = 'Text Block'),
     (SELECT id FROM production_processes WHERE name = 'Folding'),
     2, NULL, NULL, (SELECT id FROM equipment WHERE name = 'Folding Machine 1')),
    
    -- Text block cutting
    ((SELECT id FROM product_components WHERE component_name = 'Text Block'),
     (SELECT id FROM production_processes WHERE name = 'Cutting'),
     3, NULL, NULL, (SELECT id FROM equipment WHERE name = 'Cutting Machine 1'));

-- 7. Create work order dependencies
-- Text block must be completed before hardcover binding
INSERT INTO work_order_dependencies (work_order_id, depends_on_work_order_id, dependency_type)
VALUES 
    -- This would be populated when work orders are created
    ('text-block-printing-work-order', 'text-block-folding-work-order', 'finish_to_start'),
    ('text-block-folding-work-order', 'text-block-cutting-work-order', 'finish_to_start'),
    ('hardcover-binding-work-order', 'text-block-cutting-work-order', 'finish_to_start');
```

### Production Workflow Example

#### Book Production Workflow
```
1. Order Creation
   ├── Complex Product: "Standard Hardcover Book"
   ├── Components: Cover, Text Block, Endpapers
   └── Quantity: 1000 copies

2. Work Order Generation
   ├── Cover Production
   │   ├── Print cover (Digital Press)
   │   └── Cut cover (Cutting Machine)
   ├── Text Block Production  
   │   ├── Print pages (Offset Press)
   │   ├── Fold pages (Folding Machine)
   │   └── Cut pages (Cutting Machine)
   └── Final Assembly
       ├── Bind text block (Perfect Binder)
       ├── Attach cover (Hardcover Binder)
       └── Final trim (Cutting Machine)

3. Dependencies
   ├── Text block must be complete before binding
   ├── Cover must be complete before final assembly
   └── All components must be complete before final trim
```

### Enhanced API Endpoints

#### Complex Product Management
```typescript
// GET /api/v1/complex-products
interface ComplexProductListRequest {
  categoryId?: string;
  productType?: string;
  search?: string;
  page?: number;
  limit?: number;
}

// POST /api/v1/complex-products
interface CreateComplexProductRequest {
  categoryId: string;
  name: string;
  productType: string;
  specifications: ComplexProductSpecs;
  components: ComponentDefinition[];
}

// GET /api/v1/complex-products/:id/components
interface ComponentListResponse {
  components: ProductComponent[];
  total: number;
}

// POST /api/v1/complex-products/:id/components
interface CreateComponentRequest {
  componentName: string;
  componentType: string;
  specifications: ComponentSpecs;
  quantity: number;
  processRequirements: ProcessRequirement[];
}
```

#### Production Management
```typescript
// GET /api/v1/work-orders
interface WorkOrderListRequest {
  orderId?: string;
  status?: string;
  processType?: string;
  assignedUserId?: string;
  scheduledDate?: string;
  page?: number;
  limit?: number;
}

// POST /api/v1/work-orders
interface CreateWorkOrderRequest {
  orderId: string;
  productComponentId: string;
  processId: string;
  quantity: number;
  priority: number;
  scheduledStart?: string;
  assignedEquipmentId?: string;
  assignedUserId?: string;
}

// PUT /api/v1/work-orders/:id/status
interface UpdateWorkOrderStatusRequest {
  status: string;
  actualStart?: string;
  actualEnd?: string;
  completedQuantity?: number;
  notes?: string;
}
```

This enhanced schema now properly supports:

1. **Multi-component products** with hierarchical structures
2. **Complex production workflows** with process dependencies
3. **Material management** with specifications and suppliers
4. **Work order management** with scheduling and dependencies
5. **Quality control** at each production stage
6. **Batch production** for efficiency
7. **Equipment and resource allocation**

The system can now handle the full complexity of book production from individual component creation through final assembly and quality control.
