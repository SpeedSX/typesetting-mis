# TypesettingMIS Frontend

A modern React frontend for the TypesettingMIS application built with TypeScript, Material-UI, and Redux Toolkit.

## 🚀 Features

- **Modern React 18** with TypeScript
- **Material-UI** for beautiful, responsive UI components
- **Redux Toolkit** for state management
- **React Router** for navigation
- **Axios** for API communication
- **Multi-tenant support** with automatic tenant context
- **JWT Authentication** with automatic token management
- **Responsive design** that works on all devices

## 🛠️ Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Material-UI (MUI)** - Component library
- **Redux Toolkit** - State management
- **React Router** - Navigation
- **Axios** - HTTP client
- **Vite** - Build tool

## 📁 Project Structure

```
src/
├── components/          # Reusable UI components
│   ├── Layout.tsx      # Main layout with navigation
│   └── ProtectedRoute.tsx
├── pages/              # Page components
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   ├── DashboardPage.tsx
│   └── CustomersPage.tsx
├── store/              # Redux store
│   ├── index.ts
│   └── slices/         # Redux slices
│       ├── authSlice.ts
│       ├── companySlice.ts
│       └── customerSlice.ts
├── services/           # API services
│   └── api.ts
├── types/              # TypeScript type definitions
│   ├── auth.ts
│   ├── company.ts
│   ├── customer.ts
│   └── api.ts
├── hooks/              # Custom React hooks
│   └── redux.ts
└── utils/              # Utility functions
```

## 🚀 Getting Started

### Prerequisites

- Node.js 18+
- npm or yarn
- Backend API running on http://localhost:5030

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

3. Open your browser and navigate to http://localhost:5173

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## 🔐 Authentication

The frontend uses JWT tokens for authentication:

1. **Login** - Users log in with email and password
2. **Registration** - New users can register with company selection
3. **Automatic Token Management** - Tokens are stored in localStorage and automatically included in API requests
4. **Token Refresh** - Automatic logout on token expiration

## 🏢 Multi-Tenant Support

The frontend automatically handles multi-tenancy:

- **Tenant Context** - Automatically extracted from JWT claims
- **Data Isolation** - All data is automatically filtered by tenant
- **Company Selection** - Users can only see data from their company

## 📱 Pages

### Login Page
- Email and password authentication
- Form validation
- Error handling

### Registration Page
- User registration with company selection
- Form validation
- Company dropdown populated from API

### Dashboard
- Welcome message with user info
- Statistics cards (customers, companies, orders, revenue)
- Recent activity feed

### Customers Page
- List all customers (tenant-filtered)
- Add new customers
- Edit existing customers
- Delete customers
- Search and filter capabilities

## 🔌 API Integration

The frontend communicates with the backend API:

- **Base URL**: http://localhost:5030/api
- **Authentication**: JWT Bearer tokens
- **Error Handling**: Automatic token refresh and logout
- **Type Safety**: Full TypeScript integration

## 🎨 UI/UX Features

- **Material Design** - Consistent, modern interface
- **Responsive Layout** - Works on desktop, tablet, and mobile
- **Dark/Light Theme** - Configurable theme support
- **Loading States** - Proper loading indicators
- **Error Handling** - User-friendly error messages
- **Form Validation** - Real-time validation feedback

## 🔧 Development

### Adding New Pages

1. Create a new component in `src/pages/`
2. Add the route to `App.tsx`
3. Add navigation item to `Layout.tsx`

### Adding New API Endpoints

1. Add types to `src/types/`
2. Add API methods to `src/services/api.ts`
3. Create Redux slice in `src/store/slices/`
4. Use in components with `useAppDispatch` and `useAppSelector`

### State Management

The app uses Redux Toolkit for state management:

- **Auth Slice** - User authentication and profile
- **Company Slice** - Company data management
- **Customer Slice** - Customer data management

## 🚀 Deployment

### Build for Production

```bash
npm run build
```

The built files will be in the `dist/` directory.

### Environment Variables

Create a `.env` file for environment-specific configuration:

```env
VITE_API_BASE_URL=http://localhost:5030/api
```

## 🤝 Contributing

1. Follow the existing code structure
2. Use TypeScript for all new code
3. Follow Material-UI design patterns
4. Write tests for new features
5. Update documentation

## 📄 License

This project is part of the TypesettingMIS application.