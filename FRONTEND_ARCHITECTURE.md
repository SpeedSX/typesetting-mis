# Frontend Architecture Design
## Typesetting MIS - React-based SaaS Platform

### Technology Stack

#### Core Technologies
- **Framework**: React 18 with TypeScript
- **State Management**: Redux Toolkit with RTK Query
- **UI Library**: Material-UI (MUI) v5
- **Routing**: React Router v6
- **Forms**: React Hook Form with Yup validation
- **Charts**: Recharts for data visualization
- **Build Tool**: Vite for fast development and building
- **Testing**: Jest + React Testing Library + Cypress

#### Additional Libraries
- **Date Handling**: date-fns
- **HTTP Client**: Axios with interceptors
- **Notifications**: React Hot Toast
- **File Upload**: React Dropzone
- **Data Tables**: TanStack Table (React Table)
- **Rich Text**: React Quill
- **PDF Generation**: jsPDF
- **Excel Export**: xlsx

### Application Architecture

#### Folder Structure
```
src/
├── components/           # Reusable UI components
│   ├── common/          # Generic components
│   ├── forms/           # Form components
│   ├── tables/          # Table components
│   └── charts/          # Chart components
├── features/            # Feature-based modules
│   ├── auth/           # Authentication
│   ├── dashboard/      # Dashboard
│   ├── equipment/      # Equipment management
│   ├── quotes/         # Quote management
│   ├── orders/         # Order management
│   ├── customers/      # Customer management
│   ├── inventory/      # Inventory management
│   ├── reports/        # Reporting
│   └── settings/       # Settings
├── hooks/              # Custom React hooks
├── services/           # API services
├── store/              # Redux store
├── types/              # TypeScript type definitions
├── utils/              # Utility functions
├── constants/          # Application constants
└── assets/             # Static assets
```

### Component Architecture

#### 1. Layout Components
```typescript
// Main Layout Component
interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <Sidebar />
      <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
        <Header />
        <MainContent>
          {children}
        </MainContent>
      </Box>
    </Box>
  );
};

// Sidebar Navigation
const Sidebar: React.FC = () => {
  const { user, permissions } = useAuth();
  const navigate = useNavigate();
  
  const menuItems = useMemo(() => 
    getMenuItems(permissions), [permissions]
  );
  
  return (
    <Drawer variant="permanent" sx={{ width: 240 }}>
      <Toolbar>
        <Typography variant="h6" noWrap>
          Typesetting MIS
        </Typography>
      </Toolbar>
      <List>
        {menuItems.map((item) => (
          <ListItem key={item.path} onClick={() => navigate(item.path)}>
            <ListItemIcon>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} />
          </ListItem>
        ))}
      </List>
    </Drawer>
  );
};
```

#### 2. Feature-Based Components
```typescript
// Equipment Management Feature
// features/equipment/components/EquipmentList.tsx
interface EquipmentListProps {
  filters?: EquipmentFilters;
  onEquipmentSelect?: (equipment: Equipment) => void;
}

const EquipmentList: React.FC<EquipmentListProps> = ({ 
  filters, 
  onEquipmentSelect 
}) => {
  const { data: equipment, isLoading, error } = useGetEquipmentQuery(filters);
  const [deleteEquipment] = useDeleteEquipmentMutation();
  
  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this equipment?')) {
      await deleteEquipment(id);
    }
  };
  
  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage error={error} />;
  
  return (
    <DataTable
      data={equipment?.items || []}
      columns={equipmentColumns}
      onRowClick={onEquipmentSelect}
      onDelete={handleDelete}
      pagination
      searchable
    />
  );
};

// features/equipment/components/EquipmentForm.tsx
interface EquipmentFormProps {
  equipment?: Equipment;
  onSubmit: (data: EquipmentFormData) => void;
  onCancel: () => void;
}

const EquipmentForm: React.FC<EquipmentFormProps> = ({
  equipment,
  onSubmit,
  onCancel
}) => {
  const { control, handleSubmit, formState: { errors } } = useForm<EquipmentFormData>({
    defaultValues: equipment || {
      name: '',
      categoryId: '',
      model: '',
      serialNumber: '',
      status: 'active'
    }
  });
  
  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Controller
            name="name"
            control={control}
            rules={{ required: 'Name is required' }}
            render={({ field }) => (
              <TextField
                {...field}
                label="Equipment Name"
                error={!!errors.name}
                helperText={errors.name?.message}
                fullWidth
              />
            )}
          />
        </Grid>
        {/* More form fields */}
      </Grid>
      <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
        <Button type="submit" variant="contained">
          {equipment ? 'Update' : 'Create'}
        </Button>
        <Button onClick={onCancel} variant="outlined">
          Cancel
        </Button>
      </Box>
    </form>
  );
};
```

### State Management Architecture

#### Redux Store Structure
```typescript
// store/index.ts
export const store = configureStore({
  reducer: {
    auth: authSlice.reducer,
    company: companySlice.reducer,
    equipment: equipmentSlice.reducer,
    quotes: quotesSlice.reducer,
    orders: ordersSlice.reducer,
    customers: customersSlice.reducer,
    inventory: inventorySlice.reducer,
    reports: reportsSlice.reducer,
    ui: uiSlice.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    }).concat(api.middleware),
});

// store/api.ts - RTK Query API
export const api = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/v1',
    prepareHeaders: (headers, { getState }) => {
      const token = selectAuthToken(getState());
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['Equipment', 'Quotes', 'Orders', 'Customers', 'Inventory'],
  endpoints: (builder) => ({
    // Equipment endpoints
    getEquipment: builder.query<EquipmentListResponse, EquipmentListRequest>({
      query: (params) => ({
        url: 'equipment',
        params,
      }),
      providesTags: ['Equipment'],
    }),
    
    createEquipment: builder.mutation<Equipment, CreateEquipmentRequest>({
      query: (data) => ({
        url: 'equipment',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['Equipment'],
    }),
    
    // Quote endpoints
    getQuotes: builder.query<QuoteListResponse, QuoteListRequest>({
      query: (params) => ({
        url: 'quotes',
        params,
      }),
      providesTags: ['Quotes'],
    }),
    
    createQuote: builder.mutation<Quote, CreateQuoteRequest>({
      query: (data) => ({
        url: 'quotes',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['Quotes'],
    }),
    
    // More endpoints...
  }),
});
```

#### Custom Hooks
```typescript
// hooks/useAuth.ts
export const useAuth = () => {
  const dispatch = useAppDispatch();
  const { user, token, isAuthenticated } = useAppSelector(selectAuth);
  
  const login = useCallback(async (credentials: LoginCredentials) => {
    try {
      const result = await dispatch(loginUser(credentials)).unwrap();
      return result;
    } catch (error) {
      throw error;
    }
  }, [dispatch]);
  
  const logout = useCallback(() => {
    dispatch(logoutUser());
  }, [dispatch]);
  
  return {
    user,
    token,
    isAuthenticated,
    login,
    logout,
  };
};

// hooks/useEquipment.ts
export const useEquipment = (filters?: EquipmentFilters) => {
  const { data, isLoading, error, refetch } = useGetEquipmentQuery(filters);
  const [createEquipment] = useCreateEquipmentMutation();
  const [updateEquipment] = useUpdateEquipmentMutation();
  const [deleteEquipment] = useDeleteEquipmentMutation();
  
  return {
    equipment: data?.items || [],
    total: data?.total || 0,
    isLoading,
    error,
    refetch,
    createEquipment,
    updateEquipment,
    deleteEquipment,
  };
};
```

### UI/UX Design System

#### Theme Configuration
```typescript
// theme/index.ts
export const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
      light: '#42a5f5',
      dark: '#1565c0',
    },
    secondary: {
      main: '#dc004e',
    },
    background: {
      default: '#f5f5f5',
      paper: '#ffffff',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h1: {
      fontSize: '2.5rem',
      fontWeight: 500,
    },
    h2: {
      fontSize: '2rem',
      fontWeight: 500,
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          borderRadius: 8,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        },
      },
    },
  },
});
```

#### Reusable Components
```typescript
// components/common/DataTable.tsx
interface DataTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  onRowClick?: (row: T) => void;
  onDelete?: (id: string) => void;
  pagination?: boolean;
  searchable?: boolean;
  loading?: boolean;
}

const DataTable = <T,>({
  data,
  columns,
  onRowClick,
  onDelete,
  pagination = true,
  searchable = true,
  loading = false
}: DataTableProps<T>) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  
  const filteredData = useMemo(() => {
    if (!searchTerm) return data;
    return data.filter(item =>
      Object.values(item).some(value =>
        String(value).toLowerCase().includes(searchTerm.toLowerCase())
      )
    );
  }, [data, searchTerm]);
  
  return (
    <Card>
      {searchable && (
        <CardContent>
          <TextField
            placeholder="Search..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{
              startAdornment: <SearchIcon />,
            }}
          />
        </CardContent>
      )}
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell key={column.id}>
                  {column.header}
                </TableCell>
              ))}
              {onDelete && <TableCell>Actions</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={columns.length + (onDelete ? 1 : 0)}>
                  <CircularProgress />
                </TableCell>
              </TableRow>
            ) : (
              filteredData
                .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                .map((row, index) => (
                  <TableRow
                    key={index}
                    hover
                    onClick={() => onRowClick?.(row)}
                    sx={{ cursor: onRowClick ? 'pointer' : 'default' }}
                  >
                    {columns.map((column) => (
                      <TableCell key={column.id}>
                        {column.cell ? column.cell(row) : row[column.accessorKey]}
                      </TableCell>
                    ))}
                    {onDelete && (
                      <TableCell>
                        <IconButton
                          onClick={(e) => {
                            e.stopPropagation();
                            onDelete(row.id);
                          }}
                        >
                          <DeleteIcon />
                        </IconButton>
                      </TableCell>
                    )}
                  </TableRow>
                ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      {pagination && (
        <TablePagination
          component="div"
          count={filteredData.length}
          page={page}
          onPageChange={(_, newPage) => setPage(newPage)}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={(e) => setRowsPerPage(Number(e.target.value))}
        />
      )}
    </Card>
  );
};
```

### Dashboard Design

#### Main Dashboard
```typescript
// features/dashboard/components/Dashboard.tsx
const Dashboard: React.FC = () => {
  const { data: stats } = useGetDashboardStatsQuery();
  const { data: recentQuotes } = useGetRecentQuotesQuery({ limit: 5 });
  const { data: recentOrders } = useGetRecentOrdersQuery({ limit: 5 });
  
  return (
    <Grid container spacing={3}>
      {/* Key Metrics Cards */}
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Total Quotes"
          value={stats?.totalQuotes || 0}
          change={stats?.quotesChange || 0}
          icon={<QuoteIcon />}
        />
      </Grid>
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Active Orders"
          value={stats?.activeOrders || 0}
          change={stats?.ordersChange || 0}
          icon={<OrderIcon />}
        />
      </Grid>
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Revenue"
          value={formatCurrency(stats?.revenue || 0)}
          change={stats?.revenueChange || 0}
          icon={<RevenueIcon />}
        />
      </Grid>
      <Grid item xs={12} md={3}>
        <MetricCard
          title="Customers"
          value={stats?.totalCustomers || 0}
          change={stats?.customersChange || 0}
          icon={<CustomerIcon />}
        />
      </Grid>
      
      {/* Charts */}
      <Grid item xs={12} md={8}>
        <Card>
          <CardHeader title="Revenue Trend" />
          <CardContent>
            <RevenueChart data={stats?.revenueData || []} />
          </CardContent>
        </Card>
      </Grid>
      
      <Grid item xs={12} md={4}>
        <Card>
          <CardHeader title="Order Status Distribution" />
          <CardContent>
            <OrderStatusChart data={stats?.orderStatusData || []} />
          </CardContent>
        </Card>
      </Grid>
      
      {/* Recent Activity */}
      <Grid item xs={12} md={6}>
        <Card>
          <CardHeader title="Recent Quotes" />
          <CardContent>
            <RecentQuotesList quotes={recentQuotes || []} />
          </CardContent>
        </Card>
      </Grid>
      
      <Grid item xs={12} md={6}>
        <Card>
          <CardHeader title="Recent Orders" />
          <CardContent>
            <RecentOrdersList orders={recentOrders || []} />
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
};
```

### Responsive Design

#### Mobile-First Approach
```typescript
// hooks/useResponsive.ts
export const useResponsive = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const isTablet = useMediaQuery(theme.breakpoints.between('md', 'lg'));
  const isDesktop = useMediaQuery(theme.breakpoints.up('lg'));
  
  return {
    isMobile,
    isTablet,
    isDesktop,
    breakpoint: isMobile ? 'mobile' : isTablet ? 'tablet' : 'desktop',
  };
};

// Responsive Layout Component
const ResponsiveLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isMobile } = useResponsive();
  
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      {!isMobile && <Sidebar />}
      <Box sx={{ flexGrow: 1 }}>
        <Header />
        <Container maxWidth="xl" sx={{ py: 3 }}>
          {children}
        </Container>
      </Box>
    </Box>
  );
};
```

### Performance Optimization

#### Code Splitting and Lazy Loading
```typescript
// Lazy load feature modules
const Dashboard = lazy(() => import('../features/dashboard/Dashboard'));
const Equipment = lazy(() => import('../features/equipment/Equipment'));
const Quotes = lazy(() => import('../features/quotes/Quotes'));
const Orders = lazy(() => import('../features/orders/Orders'));

// App routing with lazy loading
const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Dashboard />} />
      <Route path="/equipment" element={<Equipment />} />
      <Route path="/quotes" element={<Quotes />} />
      <Route path="/orders" element={<Orders />} />
    </Routes>
  );
};

// Suspense wrapper
const App: React.FC = () => {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <AppRoutes />
    </Suspense>
  );
};
```

#### Memoization and Optimization
```typescript
// Memoized components
const EquipmentCard = React.memo<EquipmentCardProps>(({ equipment, onEdit, onDelete }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6">{equipment.name}</Typography>
        <Typography variant="body2" color="text.secondary">
          {equipment.model}
        </Typography>
      </CardContent>
      <CardActions>
        <Button onClick={() => onEdit(equipment)}>Edit</Button>
        <Button onClick={() => onDelete(equipment.id)}>Delete</Button>
      </CardActions>
    </Card>
  );
});

// Optimized data fetching
const useOptimizedEquipment = (filters: EquipmentFilters) => {
  return useGetEquipmentQuery(filters, {
    selectFromResult: ({ data, ...other }) => ({
      ...other,
      equipment: data?.items || [],
      total: data?.total || 0,
    }),
  });
};
```

### Testing Strategy

#### Unit Testing
```typescript
// components/__tests__/DataTable.test.tsx
describe('DataTable', () => {
  const mockData = [
    { id: '1', name: 'Equipment 1', status: 'active' },
    { id: '2', name: 'Equipment 2', status: 'inactive' },
  ];
  
  const mockColumns = [
    { id: 'name', header: 'Name', accessorKey: 'name' },
    { id: 'status', header: 'Status', accessorKey: 'status' },
  ];
  
  it('renders data correctly', () => {
    render(
      <DataTable
        data={mockData}
        columns={mockColumns}
      />
    );
    
    expect(screen.getByText('Equipment 1')).toBeInTheDocument();
    expect(screen.getByText('Equipment 2')).toBeInTheDocument();
  });
  
  it('filters data based on search term', () => {
    render(
      <DataTable
        data={mockData}
        columns={mockColumns}
        searchable
      />
    );
    
    const searchInput = screen.getByPlaceholderText('Search...');
    fireEvent.change(searchInput, { target: { value: 'Equipment 1' } });
    
    expect(screen.getByText('Equipment 1')).toBeInTheDocument();
    expect(screen.queryByText('Equipment 2')).not.toBeInTheDocument();
  });
});
```

#### Integration Testing
```typescript
// features/equipment/__tests__/EquipmentIntegration.test.tsx
describe('Equipment Integration', () => {
  it('creates new equipment successfully', async () => {
    const mockCreateEquipment = jest.fn();
    
    render(
      <Provider store={store}>
        <EquipmentForm
          onSubmit={mockCreateEquipment}
          onCancel={jest.fn()}
        />
      </Provider>
    );
    
    fireEvent.change(screen.getByLabelText('Equipment Name'), {
      target: { value: 'New Equipment' }
    });
    
    fireEvent.click(screen.getByText('Create'));
    
    await waitFor(() => {
      expect(mockCreateEquipment).toHaveBeenCalledWith({
        name: 'New Equipment',
        categoryId: '',
        model: '',
        serialNumber: '',
        status: 'active'
      });
    });
  });
});
```

This frontend architecture provides a scalable, maintainable, and user-friendly interface for the Typesetting MIS platform with modern React patterns and best practices.
