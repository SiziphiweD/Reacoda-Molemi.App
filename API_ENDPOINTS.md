# API Endpoints Documentation

This document lists all available API endpoints for the mobile app integration.

## Base URL
- Development: `http://localhost:5000/api`
- Production: `https://your-render-url.onrender.com/api`

## Authentication
All endpoints (except those marked as `[AllowAnonymous]`) require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

---

## Auth Endpoints (`/api/auth`)

### Register
- **POST** `/api/auth/register`
- **Description**: Register a new user
- **Auth**: Not required
- **Request Body**:
```json
{
  "email": "user@example.com",
  "password": "password123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "1234567890",
  "role": "Buyer"
}
```
- **Response**: Returns JWT token and user details

### Login
- **POST** `/api/auth/login`
- **Description**: Login and get JWT token
- **Auth**: Not required
- **Request Body**:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```
- **Response**: Returns JWT token and user details

---

## Products Endpoints (`/api/products`)

### Get All Products (Public)
- **GET** `/api/products`
- **Description**: Get all active, approved products
- **Auth**: Not required
- **Response**: List of products

### Get Product by ID (Public)
- **GET** `/api/products/{id}`
- **Description**: Get a specific product
- **Auth**: Not required
- **Response**: Product details

### Create Product
- **POST** `/api/products`
- **Description**: Create a new product (Farmers only)
- **Auth**: Required (Farmer role)
- **Request Body**:
```json
{
  "name": "Fresh Tomatoes",
  "description": "Organic tomatoes",
  "pricePerKg": 25.50,
  "availableQuantity": 100,
  "imageUrl": "https://example.com/image.jpg",
  "location": "Cape Town",
  "harvestDate": "2024-01-15",
  "expiryDate": "2024-02-15",
  "categoryId": 1
}
```

---

## Farmer Endpoints (`/api/farmers`)

All farmer endpoints require authentication and Farmer role.

### Dashboard
- **GET** `/api/farmers/dashboard`
- **Description**: Get farmer statistics and recent orders
- **Response**: Dashboard data with totals, earnings, recent orders

### Products Management

#### Get My Products
- **GET** `/api/farmers/products`
- **Description**: Get all products created by the logged-in farmer

#### Get Product by ID
- **GET** `/api/farmers/products/{id}`
- **Description**: Get a specific product (farmer's own)

#### Create Product
- **POST** `/api/farmers/products`
- **Description**: Create a new product
- **Request Body**: Same as `/api/products` POST

#### Update Product
- **PUT** `/api/farmers/products/{id}`
- **Description**: Update an existing product
- **Request Body**: Same structure as create

#### Delete Product
- **DELETE** `/api/farmers/products/{id}`
- **Description**: Soft delete a product (sets IsActive = false)

#### Toggle Availability
- **POST** `/api/farmers/products/{id}/toggle-availability`
- **Description**: Toggle product availability

### Orders Management

#### Get All Orders
- **GET** `/api/farmers/orders`
- **Description**: Get all orders received by the farmer

#### Get Order by ID
- **GET** `/api/farmers/orders/{id}`
- **Description**: Get a specific order with details

#### Accept Order
- **POST** `/api/farmers/orders/{id}/accept`
- **Description**: Accept a pending order
- **Request Body** (optional):
```json
{
  "estimatedDeliveryDate": "2024-02-01"
}
```

#### Reject Order
- **POST** `/api/farmers/orders/{id}/reject`
- **Description**: Reject a pending order
- **Request Body**:
```json
{
  "rejectionReason": "Out of stock"
}
```

#### Mark as Shipped
- **POST** `/api/farmers/orders/{id}/mark-shipped`
- **Description**: Mark order as shipped
- **Request Body**:
```json
{
  "trackingNumber": "TRACK123",
  "estimatedDeliveryDate": "2024-02-05"
}
```

#### Mark as Arrived
- **POST** `/api/farmers/orders/{id}/mark-arrived`
- **Description**: Mark order as arrived at delivery location

### Earnings
- **GET** `/api/farmers/earnings`
- **Description**: Get earnings summary and recent transactions

### Profile

#### Get Profile
- **GET** `/api/farmers/profile`
- **Description**: Get farmer profile information

#### Update Profile
- **PUT** `/api/farmers/profile`
- **Description**: Update farmer profile
- **Request Body**:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "1234567890",
  "address": "123 Main St",
  "city": "Cape Town",
  "province": "Western Cape",
  "postalCode": "8000",
  "profileImage": "https://example.com/image.jpg",
  "bankAccountNumber": "1234567890",
  "bankName": "Standard Bank"
}
```

---

## Buyer Endpoints (`/api/buyers`)

All buyer endpoints require authentication and Buyer role.

### Dashboard
- **GET** `/api/buyers/dashboard`
- **Description**: Get buyer statistics and recent orders

### Products

#### Browse Products
- **GET** `/api/buyers/products`
- **Description**: Browse products with filters (public endpoint)
- **Query Parameters**:
  - `searchTerm` (optional): Search in name/description
  - `categoryId` (optional): Filter by category
  - `minPrice` (optional): Minimum price
  - `maxPrice` (optional): Maximum price
  - `sortBy` (optional): `price_low`, `price_high`, `newest`, `name`
- **Example**: `/api/buyers/products?categoryId=1&minPrice=10&maxPrice=50&sortBy=price_low`

#### Get Product Details
- **GET** `/api/buyers/products/{id}`
- **Description**: Get product details with reviews (public endpoint)

#### Get Categories
- **GET** `/api/buyers/categories`
- **Description**: Get all active categories (public endpoint)

### Cart Management

#### Add to Cart
- **POST** `/api/buyers/cart`
- **Description**: Add product to cart
- **Request Body**:
```json
{
  "productId": 1,
  "quantity": 5
}
```

#### Get Cart
- **GET** `/api/buyers/cart`
- **Description**: Get all cart items

#### Get Cart Count
- **GET** `/api/buyers/cart/count`
- **Description**: Get total number of items in cart

#### Update Cart Item
- **PUT** `/api/buyers/cart/{productId}`
- **Description**: Update quantity of a cart item
- **Request Body**:
```json
{
  "quantity": 10
}
```

#### Remove from Cart
- **DELETE** `/api/buyers/cart/{productId}`
- **Description**: Remove item from cart

### Checkout

#### Get Checkout Info
- **GET** `/api/buyers/checkout`
- **Description**: Get checkout information (cart items, buyer info)

#### Process Checkout
- **POST** `/api/buyers/checkout`
- **Description**: Create order from cart
- **Request Body**:
```json
{
  "deliveryAddress": "123 Main St",
  "deliveryCity": "Cape Town",
  "deliveryProvince": "Western Cape",
  "deliveryPostalCode": "8000",
  "deliveryNotes": "Leave at front door",
  "paymentMethod": "CashOnDelivery"
}
```
- **Response**: Created order details

### Orders

#### Get All Orders
- **GET** `/api/buyers/orders`
- **Description**: Get all orders placed by the buyer

#### Get Order by ID
- **GET** `/api/buyers/orders/{id}`
- **Description**: Get a specific order with details

#### Confirm Delivery
- **POST** `/api/buyers/orders/{id}/confirm-delivery`
- **Description**: Confirm delivery and rate farmer
- **Request Body**:
```json
{
  "rating": 5,
  "comment": "Great service!"
}
```

### Favorites

#### Add to Favorites
- **POST** `/api/buyers/favorites/{productId}`
- **Description**: Add product to favorites

#### Remove from Favorites
- **DELETE** `/api/buyers/favorites/{productId}`
- **Description**: Remove product from favorites

#### Get Favorites
- **GET** `/api/buyers/favorites`
- **Description**: Get all favorite products

### Reviews

#### Submit Review
- **POST** `/api/buyers/products/{productId}/reviews`
- **Description**: Submit a product review
- **Request Body**:
```json
{
  "rating": 5,
  "comment": "Excellent product!"
}
```

### Profile

#### Get Profile
- **GET** `/api/buyers/profile`
- **Description**: Get buyer profile information

#### Update Profile
- **PUT** `/api/buyers/profile`
- **Description**: Update buyer profile
- **Request Body**:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "1234567890",
  "address": "123 Main St",
  "city": "Cape Town",
  "province": "Western Cape",
  "postalCode": "8000",
  "profileImage": "https://example.com/image.jpg"
}
```

---

## Common Response Formats

### Success Response
```json
{
  "id": 1,
  "message": "Operation successful"
}
```

### Error Response
```json
{
  "message": "Error description",
  "errors": [
    {
      "field": "email",
      "errors": ["Email is required"]
    }
  ]
}
```

### Validation Error Response
```json
{
  "message": "Validation failed",
  "errors": [
    {
      "field": "password",
      "errors": ["Password must be at least 6 characters"]
    }
  ]
}
```

---

## Payment Methods
- `CashOnDelivery`
- `CreditCard`
- `BankTransfer`
- `Wallet`
- `Stripe`

---

## Order Statuses
- `Pending` - Order created, waiting for farmer response
- `Accepted` - Farmer accepted the order
- `Rejected` - Farmer rejected the order
- `Shipped` - Order has been shipped
- `Arrived` - Order arrived at delivery location
- `Delivered` - Buyer confirmed delivery

---

## Testing with Swagger
1. Start the application: `dotnet run`
2. Navigate to: `http://localhost:5000/swagger`
3. Use "Authorize" button to add JWT token
4. Test endpoints directly from Swagger UI

---

## Testing with cURL

### Register
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "farmer@example.com",
    "password": "password123",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Farmer"
  }'
```

### Login
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "farmer@example.com",
    "password": "password123"
  }'
```

### Get Products (with token)
```bash
curl -X GET "http://localhost:5000/api/buyers/products" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## Notes
- All timestamps are in UTC
- All monetary values are in ZAR (South African Rand)
- Product quantities are in kilograms (kg)
- Postal codes must be exactly 4 digits
- JWT tokens expire after 1440 minutes (24 hours) by default




