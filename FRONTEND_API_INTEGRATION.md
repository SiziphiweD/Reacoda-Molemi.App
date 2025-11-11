# Frontend API Integration Guide

This document explains how the web frontend has been integrated with the API endpoints, allowing both the web app and mobile app to use the same backend.

## Overview

The frontend now uses the same API endpoints as the mobile app through a JavaScript API client. This ensures consistency between web and mobile applications.

## Key Components

### 1. JavaScript API Client (`wwwroot/js/api-client.js`)

A comprehensive API client class that handles:
- JWT token management (localStorage and cookies)
- All API endpoint calls
- Error handling
- Automatic token refresh

**Usage:**
```javascript
// Add to cart
apiClient.addToCart(productId, quantity)
    .then(data => {
        console.log('Added to cart:', data);
    })
    .catch(error => {
        console.error('Error:', error);
    });

// Get cart
apiClient.getCart()
    .then(cart => {
        console.log('Cart items:', cart.items);
    });
```

### 2. JWT Token Initialization

When a user is logged in via session (web app), the JWT token is automatically fetched and stored:

```javascript
// In _Layout.cshtml
fetch('/api/auth/token')
    .then(response => response.json())
    .then(data => {
        if (data.token) {
            apiClient.setToken(data.token);
        }
    });
```

### 3. API Endpoint: `/api/auth/token`

A new endpoint that generates a JWT token for logged-in web users:
- **GET** `/api/auth/token`
- **Auth**: Session-based (checks if user is logged in)
- **Response**: Returns JWT token and user details

## Updated Views

### Shop Page (`Views/Buyer/Shop.cshtml`)
- ✅ Uses `apiClient.addToCart()` instead of direct fetch to MVC controller
- ✅ Uses `apiClient.getCartCount()` for cart count

### Cart Page (`Views/Buyer/Cart.cshtml`)
- ✅ Uses `apiClient.updateCartItem()` for updating quantities
- ✅ Uses `apiClient.removeFromCart()` for removing items

## How It Works

### For Web Users (Session-Based)
1. User logs in via MVC Auth controller (session-based)
2. On page load, JavaScript fetches JWT token from `/api/auth/token`
3. Token is stored in `apiClient` and used for all API calls
4. API endpoints authenticate using JWT Bearer token

### For Mobile Users
1. User logs in via `/api/auth/login` endpoint
2. Receives JWT token directly
3. Token is stored in mobile app storage
4. Token is sent with all API requests

## Benefits

1. **Unified Backend**: Both web and mobile use the same API endpoints
2. **Consistency**: Same business logic for both platforms
3. **Maintainability**: Single source of truth for API logic
4. **Testing**: Can test APIs independently via Swagger
5. **Future-Proof**: Easy to add more platforms (iOS, etc.)

## API Client Methods

### Auth
- `register(userData)` - Register new user
- `login(email, password)` - Login and get token
- `logout()` - Clear token

### Products
- `getProducts(filters)` - Get all products with filters
- `getProduct(id)` - Get product by ID
- `getProductDetails(id)` - Get product with reviews

### Cart (Buyer)
- `addToCart(productId, quantity)` - Add to cart
- `getCart()` - Get cart items
- `getCartCount()` - Get cart count
- `updateCartItem(productId, quantity)` - Update quantity
- `removeFromCart(productId)` - Remove item

### Orders (Buyer)
- `getOrders()` - Get all orders
- `getOrder(id)` - Get order by ID
- `processCheckout(checkoutData)` - Create order
- `confirmDelivery(orderId, rating, comment)` - Confirm delivery

### Favorites (Buyer)
- `addToFavorites(productId)` - Add to favorites
- `removeFromFavorites(productId)` - Remove from favorites
- `getFavorites()` - Get all favorites

### Products (Farmer)
- `getFarmerProducts()` - Get farmer's products
- `createProduct(productData)` - Create product
- `updateProduct(id, productData)` - Update product
- `deleteProduct(id)` - Delete product
- `toggleProductAvailability(id)` - Toggle availability

### Orders (Farmer)
- `getFarmerOrders()` - Get all orders
- `acceptOrder(id, estimatedDeliveryDate)` - Accept order
- `rejectOrder(id, rejectionReason)` - Reject order
- `markOrderShipped(id, trackingNumber, estimatedDeliveryDate)` - Mark shipped
- `markOrderArrived(id)` - Mark arrived

## Error Handling

The API client automatically handles:
- 401 Unauthorized - Clears token and redirects to login
- Network errors - Shows user-friendly messages
- Validation errors - Displays field-specific errors

## Testing

### In Browser Console
```javascript
// Test API client
apiClient.getCart()
    .then(data => console.log('Cart:', data))
    .catch(error => console.error('Error:', error));
```

### Via Swagger
1. Navigate to `/swagger`
2. Use "Authorize" button to add JWT token
3. Test endpoints directly

## Migration Notes

### Old Code (MVC Controller)
```javascript
fetch('/Buyer/AddToCart', {
    method: 'POST',
    body: formData
})
```

### New Code (API Client)
```javascript
apiClient.addToCart(productId, quantity)
    .then(data => {
        // Handle success
    });
```

## Next Steps

To fully migrate the frontend:
1. ✅ Shop page - Add to cart
2. ✅ Cart page - Update/Remove items
3. ⏳ Checkout page - Process checkout
4. ⏳ Favorites page - Add/Remove favorites
5. ⏳ Product details - Submit reviews
6. ⏳ Farmer pages - Product management
7. ⏳ Order pages - Order management

## Troubleshooting

### Token Not Found
- Check browser console for errors
- Verify user is logged in
- Check `/api/auth/token` endpoint response

### CORS Errors
- Ensure CORS is configured in `Program.cs`
- Check API base URL matches frontend domain

### 401 Unauthorized
- Token may be expired
- Check token is being sent in Authorization header
- Verify JWT configuration in `appsettings.json`

## Security Notes

- JWT tokens expire after 24 hours (configurable)
- Tokens are stored in localStorage (web) or secure storage (mobile)
- Tokens are also stored as cookies for web app compatibility
- All API endpoints validate JWT tokens
- Session-based login still works for web app compatibility




