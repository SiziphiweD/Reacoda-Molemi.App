# Mobile Views Documentation
## Android App Implementation Guide for Molemi Marketplace

This document provides comprehensive documentation for implementing mobile views that mirror the web application features for **Buyer** and **Farmer** roles.

---

## Table of Contents
1. [Buyer Role Views](#buyer-role-views)
2. [Farmer Role Views](#farmer-role-views)
3. [Common Components](#common-components)
4. [Navigation Flow](#navigation-flow)
5. [Error Handling Guidelines](#error-handling-guidelines)

---

## Buyer Role Views

### 1. Login/Registration Screen

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `LoginFragment` / `RegisterFragment` |
| **Layout Type** | Form with `ScrollView`, `TextInputLayout`, `Button` |
| **Web App Mapping** | `/Auth/Login`, `/Auth/Register` |

#### Login Form
- **Fields:**
  - Email (TextInputLayout with email validation)
  - Password (TextInputLayout with password visibility toggle)
- **Buttons:**
  - "Login" → Calls API, navigates to Dashboard/Shop
  - "Register" → Navigates to RegisterFragment
  - "Forgot Password?" → Future feature
- **Validation:**
  - Email format validation
  - Password minimum 6 characters
  - Show error messages below fields

#### Registration Form
- **Fields:**
  - First Name (required)
  - Last Name (required)
  - Email (required, email format)
  - Phone Number (optional)
  - Password (required, min 6 chars)
  - Confirm Password (required, must match)
  - Role Selection (RadioGroup: Buyer/Farmer)
- **Buttons:**
  - "Register" → Creates account, auto-login, navigates to appropriate dashboard
  - "Already have account?" → Navigates to LoginFragment
- **Validation:**
  - All required fields must be filled
  - Email uniqueness check (handled by API)
  - Password match validation

#### API Integration

**Login:**
- **Endpoint:** `POST /api/auth/login`
- **Request Body:**
```json
{
  "email": "buyer@example.com",
  "password": "password123"
}
```
- **Response:**
```json
{
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "buyer@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Buyer"
  }
}
```
- **Authentication:** Store JWT token in SharedPreferences or SecureStorage
- **Error Handling:** Show toast for invalid credentials, network errors

**Register:**
- **Endpoint:** `POST /api/auth/register`
- **Request Body:**
```json
{
  "email": "buyer@example.com",
  "password": "password123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "1234567890",
  "role": "Buyer"
}
```
- **Response:** Same as login (returns token and user)
- **Error Handling:** Show validation errors from API response

---

### 2. Buyer Dashboard

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `BuyerDashboardFragment` |
| **Layout Type** | `CoordinatorLayout` with `RecyclerView` (statistics cards), `RecyclerView` (recent orders) |
| **Web App Mapping** | `/Buyer/Dashboard` |

#### UI Components
- **Statistics Cards (GridLayout/RecyclerView):**
  - Total Orders (CardView with icon, number, label)
  - Pending Orders (CardView)
  - Delivered Orders (CardView)
  - Total Spent (CardView with currency formatting)
- **Recent Orders Section:**
  - `RecyclerView` with `LinearLayoutManager`
  - Each item: Order number, date, status badge, total amount
  - Click → Navigate to OrderDetailsFragment
- **Quick Actions:**
  - "Browse Products" Button → Navigate to ProductsListFragment
  - "View Cart" Button → Navigate to CartFragment
  - "Order History" Button → Navigate to OrdersHistoryFragment

#### API Integration

**Get Dashboard:**
- **Endpoint:** `GET /api/buyers/dashboard`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
{
  "totalOrders": 15,
  "pendingOrders": 2,
  "deliveredOrders": 10,
  "totalSpent": 2500.50,
  "recentOrders": [
    {
      "id": 123,
      "orderNumber": "ORD-20240115-ABC123",
      "totalAmount": 150.00,
      "status": "Pending",
      "farmerName": "John Farmer",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```
- **Error Handling:** Show empty state if no orders, handle 401 (token expired)

---

### 3. Products List / Browse Products

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `ProductsListFragment` |
| **Layout Type** | `RecyclerView` (GridLayoutManager, 2 columns) with `SearchView`, Filter chips |
| **Web App Mapping** | `/Shop/Index`, `/Buyer/Shop` |

#### UI Components
- **Search Bar:**
  - `SearchView` in Toolbar
  - Real-time search (debounce 500ms)
- **Filter Chips (Horizontal RecyclerView):**
  - Categories (from API)
  - Price Range (Min/Max sliders in bottom sheet)
  - Sort Options (Price Low/High, Newest, Name)
- **Product Grid:**
  - `RecyclerView` with `GridLayoutManager` (2 columns)
  - Each item: Product image, name, price per kg, farmer name, favorite icon
  - Click → Navigate to ProductDetailsFragment
  - Swipe actions: Add to favorites
- **Floating Action Button:**
  - Cart icon with badge (cart count)
  - Click → Navigate to CartFragment

#### API Integration

**Get Products:**
- **Endpoint:** `GET /api/buyers/products`
- **Query Parameters:**
  - `searchTerm` (optional): String
  - `categoryId` (optional): Integer
  - `minPrice` (optional): Decimal
  - `maxPrice` (optional): Decimal
  - `sortBy` (optional): "price_low", "price_high", "newest", "name"
- **Example:** `/api/buyers/products?categoryId=1&minPrice=10&maxPrice=50&sortBy=price_low`
- **Headers:** Not required (public endpoint)
- **Response:**
```json
[
  {
    "id": 1,
    "name": "Fresh Tomatoes",
    "description": "Organic tomatoes",
    "pricePerKg": 25.50,
    "availableQuantity": 100,
    "imageUrl": "https://example.com/image.jpg",
    "location": "Cape Town",
    "categoryId": 1,
    "categoryName": "Vegetables",
    "farmerId": 5,
    "farmerName": "John Farmer",
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```
- **Error Handling:** Show empty state, retry button for network errors

**Get Categories:**
- **Endpoint:** `GET /api/buyers/categories`
- **Response:**
```json
[
  {
    "id": 1,
    "name": "Vegetables",
    "description": "Fresh vegetables",
    "iconUrl": "https://example.com/icon.jpg"
  }
]
```

---

### 4. Product Details

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `ProductDetailsFragment` |
| **Layout Type** | `NestedScrollView` with `ViewPager2` (images), `RecyclerView` (reviews) |
| **Web App Mapping** | `/Shop/Details/{id}`, `/Buyer/ProductDetails/{id}` |

#### UI Components
- **Image Gallery:**
  - `ViewPager2` with `TabLayout` indicators
  - Support multiple images (if available)
- **Product Info:**
  - Product name (TextView, large)
  - Price per kg (TextView, bold, currency formatted)
  - Available quantity (TextView with stock indicator)
  - Category badge (Chip)
  - Farmer name (TextView, clickable → FarmerProfileFragment)
  - Description (ExpandableTextView)
  - Location (TextView with location icon)
  - Harvest/Expiry dates (if available)
- **Quantity Selector:**
  - `NumberPicker` or `-` `+` buttons with quantity display
  - Max quantity = availableQuantity
  - Validation: Show error if exceeds available
- **Action Buttons:**
  - "Add to Cart" (Primary button) → Calls API, shows snackbar
  - "Add to Favorites" (Icon button, toggle state)
  - "Share" (Icon button)
- **Reviews Section:**
  - Average rating (RatingBar, TextView with count)
  - `RecyclerView` with reviews (rating, comment, buyer name, date)
  - "Write Review" button (only if user purchased product)

#### API Integration

**Get Product Details:**
- **Endpoint:** `GET /api/buyers/products/{id}`
- **Headers:** Not required (public endpoint)
- **Response:**
```json
{
  "id": 1,
  "name": "Fresh Tomatoes",
  "description": "Organic tomatoes",
  "pricePerKg": 25.50,
  "availableQuantity": 100,
  "imageUrl": "https://example.com/image.jpg",
  "location": "Cape Town",
  "categoryId": 1,
  "categoryName": "Vegetables",
  "farmerId": 5,
  "farmerName": "John Farmer",
  "averageRating": 4.5,
  "reviewCount": 12,
  "reviews": [
    {
      "id": 1,
      "rating": 5,
      "comment": "Great product!",
      "buyerName": "Jane Buyer",
      "createdAt": "2024-01-10T10:00:00Z"
    }
  ],
  "createdAt": "2024-01-15T10:00:00Z"
}
```

**Add to Cart:**
- **Endpoint:** `POST /api/buyers/cart`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
```json
{
  "productId": 1,
  "quantity": 5
}
```
- **Response:**
```json
{
  "success": true,
  "message": "Added to cart",
  "cartCount": 8,
  "totalAmount": 127.50
}
```
- **Error Handling:** Show error if product unavailable, quantity exceeds stock

**Add to Favorites:**
- **Endpoint:** `POST /api/buyers/favorites/{productId}`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** `{ "message": "Added to favorites" }`

**Submit Review:**
- **Endpoint:** `POST /api/buyers/products/{productId}/reviews`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
```json
{
  "rating": 5,
  "comment": "Excellent product!"
}
```
- **Validation:** Only buyers who purchased can review

---

### 5. Shopping Cart

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `CartFragment` |
| **Layout Type** | `RecyclerView` (cart items) with `CardView` footer (total, checkout button) |
| **Web App Mapping** | `/Buyer/Cart` |

#### UI Components
- **Cart Items List:**
  - `RecyclerView` with `LinearLayoutManager`
  - Each item: Product image, name, price per kg, quantity selector, total price, remove button
  - Swipe to delete (optional)
- **Quantity Controls:**
  - `-` button (decrement, min 1)
  - Quantity display (TextView)
  - `+` button (increment, max = availableQuantity)
  - Update via API on change
- **Footer (Sticky):**
  - Subtotal (TextView)
  - Delivery fee (if applicable)
  - Total amount (TextView, large, bold)
  - "Proceed to Checkout" button (Primary, disabled if cart empty)
- **Empty State:**
  - Illustration/image
  - "Your cart is empty" message
  - "Browse Products" button

#### API Integration

**Get Cart:**
- **Endpoint:** `GET /api/buyers/cart`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
{
  "items": [
    {
      "productId": 1,
      "productName": "Fresh Tomatoes",
      "price": 25.50,
      "quantity": 5,
      "imageUrl": "https://example.com/image.jpg",
      "categoryName": "Vegetables",
      "farmerName": "John Farmer",
      "totalPrice": 127.50
    }
  ],
  "totalAmount": 127.50,
  "itemCount": 5
}
```

**Update Cart Item:**
- **Endpoint:** `PUT /api/buyers/cart/{productId}`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
```json
{
  "quantity": 10
}
```
- **Note:** Setting quantity to 0 removes item

**Remove from Cart:**
- **Endpoint:** `DELETE /api/buyers/cart/{productId}`
- **Headers:** `Authorization: Bearer {token}`

**Get Cart Count (for badge):**
- **Endpoint:** `GET /api/buyers/cart/count`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** `{ "success": true, "count": 5 }`

---

### 6. Checkout Screen

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `CheckoutFragment` |
| **Layout Type** | `ScrollView` with form fields, `RecyclerView` (order summary) |
| **Web App Mapping** | `/Buyer/Checkout` |

#### UI Components
- **Order Summary Section:**
  - `RecyclerView` with cart items (read-only)
  - Subtotal, delivery fee, total
- **Delivery Address Form:**
  - Address line (TextInputLayout)
  - City (TextInputLayout)
  - Province (Spinner/Dropdown)
  - Postal Code (TextInputLayout, 4 digits validation)
  - Delivery Notes (TextInputLayout, multiline, optional)
- **Payment Method:**
  - RadioGroup with options:
    - Cash on Delivery
    - Credit Card (future)
    - Bank Transfer (future)
- **Action Buttons:**
  - "Place Order" (Primary button)
  - "Back to Cart" (Secondary button)

#### API Integration

**Get Checkout Info:**
- **Endpoint:** `GET /api/buyers/checkout`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
{
  "cartItems": [...],
  "totalAmount": 127.50,
  "buyer": {
    "id": 1,
    "email": "buyer@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "1234567890",
    "address": "123 Main St",
    "city": "Cape Town",
    "province": "Western Cape",
    "postalCode": "8000"
  }
}
```
- **Pre-fill form** with buyer info from response

**Process Checkout:**
- **Endpoint:** `POST /api/buyers/checkout`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
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
- **Response:** Order object (same as Get Order)
- **Navigation:** On success → Navigate to OrderConfirmationFragment with order ID
- **Error Handling:** Show validation errors, handle empty cart

---

### 7. Order History

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `OrdersHistoryFragment` |
| **Layout Type** | `RecyclerView` with `SwipeRefreshLayout` |
| **Web App Mapping** | `/Buyer/OrdersHistory` |

#### UI Components
- **Filter Tabs:**
  - `TabLayout` with: All, Pending, Accepted, Shipped, Delivered
- **Orders List:**
  - `RecyclerView` with `LinearLayoutManager`
  - Each item: Order number, date, status badge (color-coded), total amount, farmer name
  - Click → Navigate to OrderDetailsFragment
- **Pull to Refresh:**
  - `SwipeRefreshLayout` wrapper
- **Empty State:**
  - "No orders yet" message
  - "Browse Products" button

#### API Integration

**Get Orders:**
- **Endpoint:** `GET /api/buyers/orders`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
[
  {
    "id": 123,
    "orderNumber": "ORD-20240115-ABC123",
    "totalAmount": 150.00,
    "status": "Pending",
    "deliveryAddress": "123 Main St",
    "deliveryCity": "Cape Town",
    "deliveryProvince": "Western Cape",
    "deliveryPostalCode": "8000",
    "paymentMethod": "CashOnDelivery",
    "farmerId": 5,
    "farmerName": "John Farmer",
    "orderItems": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Fresh Tomatoes",
        "quantity": 5,
        "unitPrice": 25.50,
        "totalPrice": 127.50
      }
    ],
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

---

### 8. Order Details / Tracking

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `OrderDetailsFragment` |
| **Layout Type** | `NestedScrollView` with `RecyclerView` (order items), status timeline |
| **Web App Mapping** | `/Buyer/OrderTracking/{id}` |

#### UI Components
- **Order Header:**
  - Order number (TextView, large)
  - Status badge (Chip, color-coded)
  - Order date (TextView)
- **Status Timeline (if Shipped/Arrived):**
  - Vertical timeline with steps:
    - Order Placed (always completed)
    - Accepted (if status >= Accepted)
    - Shipped (if status >= Shipped, show tracking number)
    - Arrived (if status >= Arrived)
    - Delivered (if status = Delivered)
- **Order Items:**
  - `RecyclerView` with items (product name, quantity, price, total)
- **Delivery Info:**
  - Address, city, province, postal code
  - Delivery notes (if any)
- **Payment Info:**
  - Payment method
  - Total amount (large, bold)
- **Action Buttons (conditional):**
  - "Confirm Delivery" (if status = Arrived) → Shows rating dialog
  - "Contact Farmer" (always visible) → Opens messaging/phone
- **Tracking Info (if shipped):**
  - Tracking number (copyable)
  - Estimated delivery date

#### API Integration

**Get Order:**
- **Endpoint:** `GET /api/buyers/orders/{id}`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** Same structure as Orders list, single object

**Confirm Delivery:**
- **Endpoint:** `POST /api/buyers/orders/{id}/confirm-delivery`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
```json
{
  "rating": 5,
  "comment": "Great service!"
}
```
- **Response:** `{ "message": "Delivery confirmed successfully", "orderId": 123 }`
- **Validation:** Only available if status = "Arrived"

---

### 9. Favorites

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `FavoritesFragment` |
| **Layout Type** | `RecyclerView` (GridLayoutManager, 2 columns) |
| **Web App Mapping** | `/Buyer/Favorites` |

#### UI Components
- **Products Grid:**
  - Same layout as ProductsListFragment
  - Favorite icon (filled, red) on each item
  - Click icon → Remove from favorites
  - Click item → Navigate to ProductDetailsFragment
- **Empty State:**
  - "No favorites yet" message
  - "Browse Products" button

#### API Integration

**Get Favorites:**
- **Endpoint:** `GET /api/buyers/favorites`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** Array of ProductDto objects

**Remove from Favorites:**
- **Endpoint:** `DELETE /api/buyers/favorites/{productId}`
- **Headers:** `Authorization: Bearer {token}`

---

### 10. Buyer Profile

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `BuyerProfileFragment` |
| **Layout Type** | `ScrollView` with form fields, profile image |
| **Web App Mapping** | `/Home/Profile` |

#### UI Components
- **Profile Header:**
  - Profile image (CircleImageView, clickable to change)
  - Name, email (read-only)
- **Editable Fields:**
  - First Name (TextInputLayout)
  - Last Name (TextInputLayout)
  - Phone Number (TextInputLayout)
  - Address (TextInputLayout)
  - City (TextInputLayout)
  - Province (Spinner)
  - Postal Code (TextInputLayout)
- **Action Buttons:**
  - "Save Changes" (Primary button)
  - "Change Password" (Secondary button, future)
  - "Logout" (Text button, red)

#### API Integration

**Get Profile:**
- **Endpoint:** `GET /api/buyers/profile`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
{
  "id": 1,
  "email": "buyer@example.com",
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

**Update Profile:**
- **Endpoint:** `PUT /api/buyers/profile`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:** Same structure as response (all fields optional except firstName, lastName)
- **Response:** Updated profile object

---

## Farmer Role Views

### 1. Farmer Dashboard

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `FarmerDashboardFragment` |
| **Layout Type** | `CoordinatorLayout` with `RecyclerView` (statistics cards), `RecyclerView` (recent orders) |
| **Web App Mapping** | `/Farmer/Dashboard` |

#### UI Components
- **Statistics Cards (GridLayout/RecyclerView):**
  - Total Products (CardView)
  - Active Products (CardView)
  - Total Orders (CardView)
  - Pending Orders (CardView, highlight if > 0)
  - Total Earnings (CardView, currency formatted)
- **Recent Orders Section:**
  - `RecyclerView` with orders
  - Each item: Order number, buyer name, amount, status, time remaining (if pending)
  - Click → Navigate to OrderDetailsFragment
- **Quick Actions:**
  - "Add Product" Button → Navigate to AddProductFragment
  - "My Products" Button → Navigate to MyProductsFragment
  - "View Orders" Button → Navigate to OrdersReceivedFragment
  - "Earnings" Button → Navigate to EarningsFragment

#### API Integration

**Get Dashboard:**
- **Endpoint:** `GET /api/farmers/dashboard`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
{
  "totalProducts": 25,
  "activeProducts": 20,
  "totalOrders": 50,
  "pendingOrders": 3,
  "totalEarnings": 15000.00,
  "recentOrders": [
    {
      "id": 123,
      "orderNumber": "ORD-20240115-ABC123",
      "totalAmount": 150.00,
      "status": "Pending",
      "buyerName": "Jane Buyer",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

### 2. My Products List

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `MyProductsFragment` |
| **Layout Type** | `RecyclerView` (GridLayoutManager, 2 columns) with `FloatingActionButton` |
| **Web App Mapping** | `/Farmer/MyProducts` |

#### UI Components
- **Products Grid:**
  - `RecyclerView` with `GridLayoutManager` (2 columns)
  - Each item: Product image, name, price, quantity, status badge, availability toggle
  - Click → Navigate to EditProductFragment
  - Long press → Show context menu (Edit, Delete, Toggle Availability)
- **Floating Action Button:**
  - "+" icon
  - Click → Navigate to AddProductFragment
- **Filter/Sort:**
  - Status filter (All, Pending, Approved, Rejected)
  - Availability filter (All, Available, Unavailable)
  - Sort by: Name, Price, Date

#### API Integration

**Get My Products:**
- **Endpoint:** `GET /api/farmers/products`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
[
  {
    "id": 1,
    "name": "Fresh Tomatoes",
    "description": "Organic tomatoes",
    "pricePerKg": 25.50,
    "availableQuantity": 100,
    "imageUrl": "https://example.com/image.jpg",
    "location": "Cape Town",
    "categoryId": 1,
    "categoryName": "Vegetables",
    "farmerId": 5,
    "status": "Approved",
    "isAvailable": true,
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

**Toggle Availability:**
- **Endpoint:** `POST /api/farmers/products/{id}/toggle-availability`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** Updated ProductDto

**Delete Product:**
- **Endpoint:** `DELETE /api/farmers/products/{id}`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** `{ "message": "Product deleted successfully" }`
- **Note:** Soft delete (sets IsActive = false)

---

### 3. Add/Edit Product

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `AddProductFragment` / `EditProductFragment` |
| **Layout Type** | `ScrollView` with form fields, image picker |
| **Web App Mapping** | `/Farmer/AddProduct`, `/Farmer/EditProduct/{id}` |

#### UI Components
- **Image Upload:**
  - Image preview (ImageView)
  - "Choose Image" button → Open image picker/camera
  - Support single image (can extend to multiple)
- **Form Fields:**
  - Product Name (TextInputLayout, required)
  - Description (TextInputLayout, multiline, required)
  - Price per Kg (TextInputLayout, number input, required)
  - Available Quantity (TextInputLayout, number input, required)
  - Location (TextInputLayout, optional)
  - Category (Spinner, required, populated from API)
  - Harvest Date (DatePicker, optional)
  - Expiry Date (DatePicker, optional)
- **Action Buttons:**
  - "Save Product" (Primary button)
  - "Cancel" (Secondary button)

#### API Integration

**Get Categories (for dropdown):**
- **Endpoint:** `GET /api/buyers/categories` (public endpoint)
- **Response:** Array of CategoryDto

**Create Product:**
- **Endpoint:** `POST /api/farmers/products`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
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
- **Response:** Created ProductDto
- **Note:** Image upload should be handled separately (upload to storage, get URL, then use in request)

**Update Product:**
- **Endpoint:** `PUT /api/farmers/products/{id}`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:** Same as create
- **Response:** Updated ProductDto

**Get Product (for edit):**
- **Endpoint:** `GET /api/farmers/products/{id}`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** ProductDto

---

### 4. Orders Received

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `OrdersReceivedFragment` |
| **Layout Type** | `RecyclerView` with `SwipeRefreshLayout`, filter tabs |
| **Web App Mapping** | `/Farmer/Orders` |

#### UI Components
- **Filter Tabs:**
  - `TabLayout`: All, Pending, Accepted, Shipped, Delivered
- **Orders List:**
  - `RecyclerView` with `LinearLayoutManager`
  - Each item: Order number, buyer name, total amount, status badge, time remaining (if pending)
  - Click → Navigate to OrderDetailsFragment
- **Pull to Refresh:**
  - `SwipeRefreshLayout` wrapper

#### API Integration

**Get Orders:**
- **Endpoint:** `GET /api/farmers/orders`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
[
  {
    "id": 123,
    "orderNumber": "ORD-20240115-ABC123",
    "totalAmount": 150.00,
    "status": "Pending",
    "buyerId": 1,
    "buyerName": "Jane Buyer",
    "buyerEmail": "buyer@example.com",
    "buyerPhone": "1234567890",
    "orderItems": [...],
    "responseDeadline": "2024-01-15T11:00:00Z",
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

---

### 5. Order Details (Farmer View)

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `FarmerOrderDetailsFragment` |
| **Layout Type** | `NestedScrollView` with order info, action buttons |
| **Web App Mapping** | `/Farmer/OrderDetails/{id}` |

#### UI Components
- **Order Header:**
  - Order number, status badge, order date
  - Time remaining (if pending, countdown timer)
- **Buyer Info:**
  - Buyer name, email, phone (clickable to call/email)
- **Order Items:**
  - `RecyclerView` with items
- **Delivery Info:**
  - Address, city, province, postal code
  - Delivery notes
- **Action Buttons (conditional based on status):**
  - "Accept Order" (if Pending) → Shows dialog with estimated delivery date picker
  - "Reject Order" (if Pending) → Shows dialog with rejection reason input
  - "Mark as Shipped" (if Accepted) → Shows dialog with tracking number input
  - "Mark as Arrived" (if Shipped) → Confirmation dialog
- **Payment Info:**
  - Payment method, total amount

#### API Integration

**Get Order:**
- **Endpoint:** `GET /api/farmers/orders/{id}`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** OrderDto (same as list, single object)

**Accept Order:**
- **Endpoint:** `POST /api/farmers/orders/{id}/accept`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body (optional):**
```json
{
  "estimatedDeliveryDate": "2024-02-01"
}
```
- **Response:** `{ "message": "Order accepted successfully", "orderId": 123 }`
- **Validation:** Only if status = "Pending", within 30-minute deadline

**Reject Order:**
- **Endpoint:** `POST /api/farmers/orders/{id}/reject`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
```json
{
  "rejectionReason": "Out of stock"
}
```
- **Response:** `{ "message": "Order rejected successfully", "orderId": 123 }`

**Mark as Shipped:**
- **Endpoint:** `POST /api/farmers/orders/{id}/mark-shipped`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:**
```json
{
  "trackingNumber": "TRACK123",
  "estimatedDeliveryDate": "2024-02-05"
}
```
- **Response:** `{ "message": "Order marked as shipped successfully", "orderId": 123 }`

**Mark as Arrived:**
- **Endpoint:** `POST /api/farmers/orders/{id}/mark-arrived`
- **Headers:** `Authorization: Bearer {token}`
- **Response:** `{ "message": "Order marked as arrived successfully", "orderId": 123 }`

---

### 6. Earnings

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `EarningsFragment` |
| **Layout Type** | `ScrollView` with summary cards, `RecyclerView` (transactions) |
| **Web App Mapping** | `/Farmer/Earnings` |

#### UI Components
- **Summary Cards:**
  - Total Earnings (CardView, large, currency formatted)
  - Pending Earnings (CardView)
- **Recent Transactions:**
  - `RecyclerView` with transactions
  - Each item: Order number, buyer name, amount, date
  - Click → Navigate to OrderDetailsFragment
- **Filter Options:**
  - Date range picker (optional)
  - Status filter (All, Delivered, Pending)

#### API Integration

**Get Earnings:**
- **Endpoint:** `GET /api/farmers/earnings`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
{
  "totalEarnings": 15000.00,
  "pendingEarnings": 500.00,
  "recentTransactions": [
    {
      "orderId": 123,
      "orderNumber": "ORD-20240115-ABC123",
      "amount": 150.00,
      "date": "2024-01-15T10:30:00Z",
      "buyerName": "Jane Buyer"
    }
  ]
}
```

---

### 7. Farmer Profile

| **Property** | **Details** |
|--------------|-------------|
| **Fragment/Activity** | `FarmerProfileFragment` |
| **Layout Type** | `ScrollView` with form fields, profile image |
| **Web App Mapping** | `/Farmer/ProfileSettings` |

#### UI Components
- **Profile Header:**
  - Profile image (CircleImageView)
  - Name, email (read-only)
  - Verification badge (if verified)
- **Editable Fields:**
  - First Name, Last Name
  - Phone Number
  - Address, City, Province, Postal Code
  - Bank Account Number (sensitive, masked display)
  - Bank Name
- **Action Buttons:**
  - "Save Changes" (Primary button)
  - "Logout" (Text button, red)

#### API Integration

**Get Profile:**
- **Endpoint:** `GET /api/farmers/profile`
- **Headers:** `Authorization: Bearer {token}`
- **Response:**
```json
{
  "id": 5,
  "email": "farmer@example.com",
  "firstName": "John",
  "lastName": "Farmer",
  "phoneNumber": "1234567890",
  "address": "123 Farm St",
  "city": "Cape Town",
  "province": "Western Cape",
  "postalCode": "8000",
  "profileImage": "https://example.com/image.jpg",
  "bankAccountNumber": "1234567890",
  "bankName": "Standard Bank",
  "isVerified": true
}
```

**Update Profile:**
- **Endpoint:** `PUT /api/farmers/profile`
- **Headers:** `Authorization: Bearer {token}`
- **Request Body:** Same structure as response
- **Response:** Updated profile object

---

## Common Components

### Navigation Structure

```
MainActivity (Bottom Navigation)
├── Home Tab
│   ├── Buyer: DashboardFragment
│   └── Farmer: DashboardFragment
├── Products Tab (Buyer) / My Products Tab (Farmer)
│   ├── Buyer: ProductsListFragment
│   └── Farmer: MyProductsFragment
├── Orders Tab
│   ├── Buyer: OrdersHistoryFragment
│   └── Farmer: OrdersReceivedFragment
└── Profile Tab
    ├── Buyer: BuyerProfileFragment
    └── Farmer: FarmerProfileFragment

Additional Screens (Full Screen):
- LoginActivity / RegisterActivity
- ProductDetailsFragment
- CartFragment
- CheckoutFragment
- OrderDetailsFragment
- AddProductFragment / EditProductFragment
- EarningsFragment (Farmer)
- FavoritesFragment (Buyer)
```

### Reusable Components

1. **ProductCardView** (RecyclerView item)
   - Image, name, price, farmer name, favorite icon
   - Used in: ProductsListFragment, FavoritesFragment

2. **OrderCardView** (RecyclerView item)
   - Order number, status badge, amount, date
   - Used in: OrdersHistoryFragment, OrdersReceivedFragment

3. **StatusBadge** (Custom View/Chip)
   - Color-coded status indicators
   - Colors: Pending (yellow), Accepted (blue), Shipped (purple), Delivered (green), Rejected (red)

4. **QuantitySelector** (Custom View)
   - `-` button, quantity display, `+` button
   - Used in: CartFragment, ProductDetailsFragment

5. **LoadingIndicator**
   - ProgressBar or Shimmer effect
   - Show during API calls

6. **EmptyStateView**
   - Illustration, message, action button
   - Used in: CartFragment, OrdersHistoryFragment, FavoritesFragment

---

## Navigation Flow

### Buyer Flow
```
Login → Dashboard → Products List → Product Details → Add to Cart → Cart → Checkout → Order Confirmation → Order Details
                                                                                    ↓
                                                                              Order History
```

### Farmer Flow
```
Login → Dashboard → My Products → Add/Edit Product → Products List
                ↓
         Orders Received → Order Details → Accept/Reject/Update Status
                ↓
            Earnings
```

---

## Error Handling Guidelines

### Network Errors
- **No Internet:** Show snackbar with "No internet connection" and retry button
- **Timeout:** Show "Request timed out" with retry option
- **Server Error (500):** Show "Server error. Please try again later"
- **Not Found (404):** Show "Resource not found" message

### Authentication Errors
- **401 Unauthorized:** Clear token, navigate to LoginActivity
- **403 Forbidden:** Show "You don't have permission" message
- **Token Expired:** Auto-refresh token if refresh endpoint available, else redirect to login

### Validation Errors
- **400 Bad Request:** Parse error response, show field-specific errors below inputs
- **Example Response:**
```json
{
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "errors": ["Email is required"]
    }
  ]
}
```

### Business Logic Errors
- **Product Unavailable:** Show "Product is no longer available" when adding to cart
- **Quantity Exceeds Stock:** Show "Only X kg available" message
- **Order Deadline Passed:** Show "Order deadline has passed" when trying to accept/reject
- **Empty Cart:** Disable checkout button, show message

### User Feedback
- **Success Actions:** Show snackbar with success message (e.g., "Added to cart")
- **Loading States:** Show progress indicator during API calls
- **Optimistic Updates:** Update UI immediately, revert on error

### Retry Logic
- Implement exponential backoff for failed requests
- Max 3 retry attempts
- Show retry button in error states

---

## UI/UX Enhancements

### Performance
- **Image Loading:** Use Glide or Coil for image loading with caching
- **Pagination:** Implement infinite scroll for product lists (if API supports)
- **Caching:** Cache product categories, user profile locally
- **Offline Support:** Show cached data when offline, sync when online

### Accessibility
- Add content descriptions for icons and images
- Ensure proper touch target sizes (min 48dp)
- Support screen readers
- High contrast mode support

### Animations
- Smooth transitions between fragments
- Loading shimmer effects
- Success checkmark animations
- Pull-to-refresh animations

### Notifications
- Push notifications for:
  - New orders (Farmer)
  - Order status updates (Buyer)
  - Product approval/rejection (Farmer)
- Local notifications for reminders

---

## Implementation Notes

### Retrofit Setup
```kotlin
// Base URL
const val BASE_URL = "https://your-api-url.com/api/"

// Interceptor for JWT token
class AuthInterceptor(private val tokenManager: TokenManager) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request().newBuilder()
            .addHeader("Authorization", "Bearer ${tokenManager.getToken()}")
            .build()
        return chain.proceed(request)
    }
}
```

### Data Models
- Create Kotlin data classes matching API DTOs
- Use `@SerializedName` for field mapping
- Implement Parcelable for fragment arguments

### State Management
- Use ViewModel with LiveData/StateFlow
- Handle loading, success, error states
- Use Repository pattern for API calls

### Testing
- Unit tests for ViewModels
- UI tests for critical flows (login, checkout)
- API mocking for offline development

---

## Additional Resources

- **API Base URL:** Configure in `BuildConfig` or `strings.xml`
- **JWT Token Storage:** Use `EncryptedSharedPreferences` or `SecureStorage`
- **Image Upload:** Use multipart/form-data for product images
- **Date Formatting:** Use `SimpleDateFormat` or `java.time` for date display
- **Currency Formatting:** Use `NumberFormat.getCurrencyInstance(Locale("en", "ZA"))` for ZAR

---

**Last Updated:** 2024-01-15
**Version:** 1.0

