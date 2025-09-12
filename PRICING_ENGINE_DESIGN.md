# Advanced Pricing Engine for Complex Products
## Typesetting MIS - Multi-Component Cost Calculation

### Overview
This document describes the enhanced pricing engine that can calculate accurate quotes for complex products like books, considering all components, materials, processes, labor, and overhead costs.

### Enhanced Pricing Tables

#### 1. Cost Categories Table
```sql
CREATE TABLE cost_categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    cost_type VARCHAR(50) NOT NULL, -- 'material', 'labor', 'overhead', 'equipment', 'waste'
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_cost_categories_company ON cost_categories(company_id);
CREATE INDEX idx_cost_categories_type ON cost_categories(cost_type);
```

#### 2. Material Costs Table
```sql
CREATE TABLE material_costs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    material_id UUID NOT NULL REFERENCES materials(id),
    supplier_id UUID REFERENCES suppliers(id),
    cost_per_unit DECIMAL(12,4) NOT NULL,
    unit VARCHAR(50) NOT NULL,
    effective_date DATE NOT NULL,
    expiry_date DATE,
    minimum_quantity INTEGER DEFAULT 1,
    maximum_quantity INTEGER,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_material_costs_company ON material_costs(company_id);
CREATE INDEX idx_material_costs_material ON material_costs(material_id);
CREATE INDEX idx_material_costs_supplier ON material_costs(supplier_id);
CREATE INDEX idx_material_costs_effective_date ON material_costs(effective_date);
```

#### 3. Labor Rates Table
```sql
CREATE TABLE labor_rates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    process_id UUID NOT NULL REFERENCES production_processes(id),
    skill_level VARCHAR(50) NOT NULL, -- 'junior', 'intermediate', 'senior', 'specialist'
    hourly_rate DECIMAL(8,2) NOT NULL,
    setup_rate DECIMAL(8,2), -- Different rate for setup time
    overtime_multiplier DECIMAL(3,2) DEFAULT 1.5,
    effective_date DATE NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_labor_rates_company ON labor_rates(company_id);
CREATE INDEX idx_labor_rates_process ON labor_rates(process_id);
CREATE INDEX idx_labor_rates_skill ON labor_rates(skill_level);
```

#### 4. Equipment Costs Table
```sql
CREATE TABLE equipment_costs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    equipment_id UUID NOT NULL REFERENCES equipment(id),
    cost_type VARCHAR(50) NOT NULL, -- 'hourly', 'per_setup', 'per_unit', 'depreciation'
    cost_per_unit DECIMAL(12,4) NOT NULL,
    unit VARCHAR(50) NOT NULL, -- 'hour', 'setup', 'unit', 'month'
    effective_date DATE NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_equipment_costs_company ON equipment_costs(company_id);
CREATE INDEX idx_equipment_costs_equipment ON equipment_costs(equipment_id);
CREATE INDEX idx_equipment_costs_type ON equipment_costs(cost_type);
```

#### 5. Overhead Rates Table
```sql
CREATE TABLE overhead_rates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    overhead_type VARCHAR(50) NOT NULL, -- 'general', 'facility', 'utilities', 'admin', 'marketing'
    rate_type VARCHAR(50) NOT NULL, -- 'percentage', 'per_hour', 'per_unit', 'fixed'
    rate_value DECIMAL(8,4) NOT NULL,
    base_type VARCHAR(50) NOT NULL, -- 'labor_cost', 'material_cost', 'total_cost', 'direct_cost'
    effective_date DATE NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_overhead_rates_company ON overhead_rates(company_id);
CREATE INDEX idx_overhead_rates_type ON overhead_rates(overhead_type);
```

#### 6. Waste Factors Table
```sql
CREATE TABLE waste_factors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    process_id UUID NOT NULL REFERENCES production_processes(id),
    material_id UUID REFERENCES materials(id),
    waste_percentage DECIMAL(5,2) NOT NULL, -- Percentage of material wasted
    waste_type VARCHAR(50) NOT NULL, -- 'setup', 'run', 'trim', 'defect'
    effective_date DATE NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_waste_factors_company ON waste_factors(company_id);
CREATE INDEX idx_waste_factors_process ON waste_factors(process_id);
CREATE INDEX idx_waste_factors_material ON waste_factors(material_id);
```

#### 7. Quote Templates Table
```sql
CREATE TABLE quote_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    product_type VARCHAR(50) NOT NULL,
    template_config JSONB NOT NULL, -- Pricing rules and calculations
    is_default BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_quote_templates_company ON quote_templates(company_id);
CREATE INDEX idx_quote_templates_product_type ON quote_templates(product_type);
```

#### 8. Quote Calculations Table
```sql
CREATE TABLE quote_calculations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    quote_id UUID NOT NULL REFERENCES quotes(id),
    component_id UUID REFERENCES product_components(id),
    process_id UUID REFERENCES production_processes(id),
    cost_category_id UUID NOT NULL REFERENCES cost_categories(id),
    cost_type VARCHAR(50) NOT NULL, -- 'material', 'labor', 'equipment', 'overhead', 'waste'
    quantity DECIMAL(10,3) NOT NULL,
    unit_cost DECIMAL(12,4) NOT NULL,
    total_cost DECIMAL(12,2) NOT NULL,
    markup_percentage DECIMAL(5,2) DEFAULT 0,
    final_price DECIMAL(12,2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_quote_calculations_quote ON quote_calculations(quote_id);
CREATE INDEX idx_quote_calculations_component ON quote_calculations(component_id);
CREATE INDEX idx_quote_calculations_process ON quote_calculations(process_id);
CREATE INDEX idx_quote_calculations_category ON quote_calculations(cost_category_id);
```

### Enhanced Quote Management

#### Quote Structure for Complex Products
```typescript
interface ComplexQuoteRequest {
  customerId: string;
  complexProductId: string;
  quantity: number;
  specifications: {
    // Override product specifications
    pageCount?: number;
    trimSize?: string;
    paperWeight?: string;
    bindingType?: string;
    colorPages?: number;
    blackWhitePages?: number;
    specialFinishes?: string[];
  };
  components: {
    componentId: string;
    quantity: number;
    specifications: any;
  }[];
  pricingOptions: {
    markupPercentage?: number;
    discountPercentage?: number;
    rushOrder?: boolean;
    specialRequirements?: string[];
  };
  deliveryDate?: string;
  notes?: string;
}

interface QuoteCalculationResult {
  quoteId: string;
  totalCost: number;
  totalPrice: number;
  profitMargin: number;
  costBreakdown: {
    materials: ComponentCostBreakdown[];
    labor: ComponentCostBreakdown[];
    equipment: ComponentCostBreakdown[];
    overhead: ComponentCostBreakdown[];
    waste: ComponentCostBreakdown[];
  };
  timeEstimate: {
    totalHours: number;
    setupTime: number;
    productionTime: number;
    estimatedCompletion: string;
  };
  components: ComponentQuoteDetail[];
}

interface ComponentCostBreakdown {
  componentId: string;
  componentName: string;
  processes: ProcessCostBreakdown[];
  totalCost: number;
  totalPrice: number;
}

interface ProcessCostBreakdown {
  processId: string;
  processName: string;
  costType: string;
  quantity: number;
  unitCost: number;
  totalCost: number;
  timeRequired: number; // in minutes
}
```

### Pricing Engine Implementation

#### Core Pricing Service
```typescript
class PricingEngineService {
  async calculateComplexProductQuote(request: ComplexQuoteRequest): Promise<QuoteCalculationResult> {
    const complexProduct = await this.getComplexProduct(request.complexProductId);
    const components = await this.getProductComponents(request.complexProductId);
    
    const calculations: QuoteCalculationResult = {
      quoteId: generateId(),
      totalCost: 0,
      totalPrice: 0,
      profitMargin: 0,
      costBreakdown: {
        materials: [],
        labor: [],
        equipment: [],
        overhead: [],
        waste: []
      },
      timeEstimate: {
        totalHours: 0,
        setupTime: 0,
        productionTime: 0,
        estimatedCompletion: ''
      },
      components: []
    };
    
    // Calculate costs for each component
    for (const component of components) {
      const componentQuote = await this.calculateComponentCost(
        component,
        request.quantity,
        request.specifications
      );
      
      calculations.components.push(componentQuote);
      calculations.totalCost += componentQuote.totalCost;
      calculations.totalPrice += componentQuote.totalPrice;
      calculations.timeEstimate.totalHours += componentQuote.totalTimeHours;
    }
    
    // Apply markup and discounts
    calculations.totalPrice = this.applyPricingRules(
      calculations.totalCost,
      request.pricingOptions
    );
    
    calculations.profitMargin = this.calculateProfitMargin(
      calculations.totalCost,
      calculations.totalPrice
    );
    
    // Calculate estimated completion date
    calculations.timeEstimate.estimatedCompletion = this.calculateCompletionDate(
      calculations.timeEstimate.totalHours,
      request.deliveryDate
    );
    
    return calculations;
  }
  
  private async calculateComponentCost(
    component: ProductComponent,
    quantity: number,
    specifications: any
  ): Promise<ComponentQuoteDetail> {
    const processes = await this.getComponentProcesses(component.id);
    const componentCosts: ProcessCostBreakdown[] = [];
    let totalCost = 0;
    let totalTime = 0;
    
    for (const process of processes) {
      const processCost = await this.calculateProcessCost(
        process,
        component,
        quantity,
        specifications
      );
      
      componentCosts.push(processCost);
      totalCost += processCost.totalCost;
      totalTime += processCost.timeRequired;
    }
    
    return {
      componentId: component.id,
      componentName: component.componentName,
      processes: componentCosts,
      totalCost,
      totalPrice: totalCost * (1 + this.getDefaultMarkup()),
      totalTimeHours: totalTime / 60
    };
  }
  
  private async calculateProcessCost(
    process: ProductionProcess,
    component: ProductComponent,
    quantity: number,
    specifications: any
  ): Promise<ProcessCostBreakdown> {
    const costs: ProcessCostBreakdown[] = [];
    
    // Material costs
    const materialCost = await this.calculateMaterialCost(
      process,
      component,
      quantity,
      specifications
    );
    if (materialCost.totalCost > 0) {
      costs.push(materialCost);
    }
    
    // Labor costs
    const laborCost = await this.calculateLaborCost(
      process,
      quantity,
      specifications
    );
    if (laborCost.totalCost > 0) {
      costs.push(laborCost);
    }
    
    // Equipment costs
    const equipmentCost = await this.calculateEquipmentCost(
      process,
      quantity,
      specifications
    );
    if (equipmentCost.totalCost > 0) {
      costs.push(equipmentCost);
    }
    
    // Waste costs
    const wasteCost = await this.calculateWasteCost(
      process,
      component,
      quantity,
      specifications
    );
    if (wasteCost.totalCost > 0) {
      costs.push(wasteCost);
    }
    
    // Calculate totals
    const totalCost = costs.reduce((sum, cost) => sum + cost.totalCost, 0);
    const totalTime = Math.max(...costs.map(cost => cost.timeRequired));
    
    return {
      processId: process.id,
      processName: process.name,
      costType: 'total',
      quantity,
      unitCost: totalCost / quantity,
      totalCost,
      timeRequired: totalTime
    };
  }
}
```

#### Material Cost Calculation
```typescript
class MaterialCostCalculator {
  async calculateMaterialCost(
    process: ProductionProcess,
    component: ProductComponent,
    quantity: number,
    specifications: any
  ): Promise<ProcessCostBreakdown> {
    const materialRequirements = await this.getProcessMaterialRequirements(
      process.id,
      component.id
    );
    
    let totalCost = 0;
    let totalTime = 0;
    
    for (const requirement of materialRequirements) {
      const material = await this.getMaterial(requirement.materialId);
      const materialCost = await this.getCurrentMaterialCost(
        requirement.materialId,
        new Date()
      );
      
      // Calculate required quantity including waste
      const wasteFactor = await this.getWasteFactor(
        process.id,
        requirement.materialId
      );
      const requiredQuantity = requirement.quantity * quantity * (1 + wasteFactor / 100);
      
      // Calculate cost
      const materialTotalCost = requiredQuantity * materialCost.cost_per_unit;
      totalCost += materialTotalCost;
      
      // Estimate time (material handling time)
      totalTime += this.estimateMaterialHandlingTime(requirement.quantity * quantity);
    }
    
    return {
      processId: process.id,
      processName: process.name,
      costType: 'material',
      quantity,
      unitCost: totalCost / quantity,
      totalCost,
      timeRequired: totalTime
    };
  }
  
  private async getCurrentMaterialCost(
    materialId: string,
    date: Date
  ): Promise<MaterialCost> {
    const costs = await this.materialCostRepository.find({
      where: {
        materialId,
        effectiveDate: LessThanOrEqual(date),
        isActive: true
      },
      order: { effectiveDate: 'DESC' }
    });
    
    return costs[0];
  }
}
```

#### Labor Cost Calculation
```typescript
class LaborCostCalculator {
  async calculateLaborCost(
    process: ProductionProcess,
    quantity: number,
    specifications: any
  ): Promise<ProcessCostBreakdown> {
    const laborRate = await this.getLaborRate(process.id, 'intermediate');
    const processDuration = await this.estimateProcessDuration(
      process,
      quantity,
      specifications
    );
    
    // Calculate setup time
    const setupTime = process.setup_time || 0;
    const setupCost = setupTime * (laborRate.setup_rate || laborRate.hourly_rate) / 60;
    
    // Calculate production time
    const productionTime = processDuration - setupTime;
    const productionCost = productionTime * laborRate.hourly_rate / 60;
    
    const totalCost = setupCost + productionCost;
    const totalTime = processDuration;
    
    return {
      processId: process.id,
      processName: process.name,
      costType: 'labor',
      quantity,
      unitCost: totalCost / quantity,
      totalCost,
      timeRequired: totalTime
    };
  }
  
  private async estimateProcessDuration(
    process: ProductionProcess,
    quantity: number,
    specifications: any
  ): Promise<number> {
    // Base duration from process definition
    let baseDuration = process.estimated_duration || 60;
    
    // Adjust for quantity (non-linear scaling)
    const quantityMultiplier = this.calculateQuantityMultiplier(quantity);
    baseDuration *= quantityMultiplier;
    
    // Adjust for complexity
    const complexityMultiplier = this.calculateComplexityMultiplier(specifications);
    baseDuration *= complexityMultiplier;
    
    return Math.ceil(baseDuration);
  }
  
  private calculateQuantityMultiplier(quantity: number): number {
    // Efficiency improves with larger quantities
    if (quantity <= 100) return 1.0;
    if (quantity <= 500) return 0.9;
    if (quantity <= 1000) return 0.8;
    if (quantity <= 5000) return 0.7;
    return 0.6;
  }
}
```

#### Equipment Cost Calculation
```typescript
class EquipmentCostCalculator {
  async calculateEquipmentCost(
    process: ProductionProcess,
    quantity: number,
    specifications: any
  ): Promise<ProcessCostBreakdown> {
    const equipmentCosts = await this.getEquipmentCosts(process.id);
    const processDuration = await this.estimateProcessDuration(process, quantity, specifications);
    
    let totalCost = 0;
    
    for (const equipmentCost of equipmentCosts) {
      let equipmentTotalCost = 0;
      
      switch (equipmentCost.cost_type) {
        case 'hourly':
          equipmentTotalCost = (processDuration / 60) * equipmentCost.cost_per_unit;
          break;
        case 'per_setup':
          equipmentTotalCost = equipmentCost.cost_per_unit;
          break;
        case 'per_unit':
          equipmentTotalCost = quantity * equipmentCost.cost_per_unit;
          break;
        case 'depreciation':
          // Calculate depreciation based on usage
          const monthlyDepreciation = equipmentCost.cost_per_unit;
          const usageHours = processDuration / 60;
          const monthlyHours = 160; // 4 weeks * 40 hours
          equipmentTotalCost = (usageHours / monthlyHours) * monthlyDepreciation;
          break;
      }
      
      totalCost += equipmentTotalCost;
    }
    
    return {
      processId: process.id,
      processName: process.name,
      costType: 'equipment',
      quantity,
      unitCost: totalCost / quantity,
      totalCost,
      timeRequired: processDuration
    };
  }
}
```

#### Overhead Cost Calculation
```typescript
class OverheadCostCalculator {
  async calculateOverheadCost(
    directCost: number,
    laborCost: number,
    materialCost: number
  ): Promise<number> {
    const overheadRates = await this.getActiveOverheadRates();
    let totalOverhead = 0;
    
    for (const rate of overheadRates) {
      let baseAmount = 0;
      
      switch (rate.base_type) {
        case 'labor_cost':
          baseAmount = laborCost;
          break;
        case 'material_cost':
          baseAmount = materialCost;
          break;
        case 'total_cost':
          baseAmount = directCost;
          break;
        case 'direct_cost':
          baseAmount = directCost;
          break;
      }
      
      let overheadAmount = 0;
      switch (rate.rate_type) {
        case 'percentage':
          overheadAmount = baseAmount * (rate.rate_value / 100);
          break;
        case 'per_hour':
          // This would need to be calculated based on total labor hours
          overheadAmount = baseAmount * rate.rate_value;
          break;
        case 'fixed':
          overheadAmount = rate.rate_value;
          break;
      }
      
      totalOverhead += overheadAmount;
    }
    
    return totalOverhead;
  }
}
```

### Quote Generation API

#### Enhanced Quote Endpoints
```typescript
// POST /api/v1/quotes/complex-product
interface CreateComplexQuoteRequest {
  customerId: string;
  complexProductId: string;
  quantity: number;
  specifications: ComplexProductSpecs;
  pricingOptions: PricingOptions;
  deliveryDate?: string;
  notes?: string;
}

// GET /api/v1/quotes/:id/breakdown
interface QuoteBreakdownResponse {
  quoteId: string;
  totalCost: number;
  totalPrice: number;
  profitMargin: number;
  costBreakdown: CostBreakdown;
  timeEstimate: TimeEstimate;
  components: ComponentBreakdown[];
}

// POST /api/v1/quotes/:id/optimize
interface OptimizeQuoteRequest {
  optimizationType: 'cost' | 'time' | 'quality';
  constraints: {
    maxCost?: number;
    maxTime?: number;
    minQuality?: string;
  };
}

// GET /api/v1/quotes/:id/compare
interface QuoteComparisonResponse {
  originalQuote: QuoteBreakdownResponse;
  alternatives: QuoteBreakdownResponse[];
  recommendations: string[];
}
```

### Example Quote Calculation

#### Hardcover Book Quote Example
```typescript
// Input: 1000 copies of a 300-page hardcover book
const quoteRequest = {
  customerId: "customer-123",
  complexProductId: "hardcover-book-standard",
  quantity: 1000,
  specifications: {
    pageCount: 300,
    trimSize: "6x9",
    paperWeight: "80lb",
    bindingType: "hardcover",
    colorPages: 0,
    blackWhitePages: 300
  },
  pricingOptions: {
    markupPercentage: 35,
    rushOrder: false
  }
};

// Output: Detailed cost breakdown
const quoteResult = {
  quoteId: "quote-456",
  totalCost: 2450.00,
  totalPrice: 3307.50,
  profitMargin: 26.0,
  costBreakdown: {
    materials: [
      {
        componentName: "Text Block",
        totalCost: 1200.00,
        processes: [
          { processName: "Offset Printing", cost: 800.00, time: 120 },
          { processName: "Folding", cost: 200.00, time: 30 },
          { processName: "Cutting", cost: 200.00, time: 15 }
        ]
      },
      {
        componentName: "Cover",
        totalCost: 450.00,
        processes: [
          { processName: "Digital Printing", cost: 300.00, time: 60 },
          { processName: "Cutting", cost: 150.00, time: 10 }
        ]
      }
    ],
    labor: [
      { componentName: "Text Block", totalCost: 600.00, time: 165 },
      { componentName: "Cover", totalCost: 200.00, time: 70 }
    ],
    equipment: [
      { componentName: "Text Block", totalCost: 300.00, time: 165 },
      { componentName: "Cover", totalCost: 100.00, time: 70 }
    ],
    overhead: [
      { componentName: "General", totalCost: 245.00 },
      { componentName: "Facility", totalCost: 122.50 }
    ]
  },
  timeEstimate: {
    totalHours: 4.0,
    setupTime: 1.0,
    productionTime: 3.0,
    estimatedCompletion: "2024-02-15T17:00:00Z"
  }
};
```

This enhanced pricing engine provides:

1. **Detailed Cost Breakdown** - Materials, labor, equipment, overhead, and waste
2. **Accurate Time Estimation** - Setup and production time for each process
3. **Flexible Pricing Rules** - Markup, discounts, and special pricing
4. **Multi-Component Support** - Complex products with multiple components
5. **Real-time Cost Updates** - Current material and labor rates
6. **Quote Optimization** - Cost, time, or quality optimization options
7. **Profit Analysis** - Detailed profit margin calculations

The system can now generate accurate quotes for complex book production while providing detailed cost breakdowns for both internal cost control and client transparency.
