/**
 * API Client for Molemi App
 * Handles all API calls to the backend
 */

class ApiClient {
    constructor() {
        this.baseUrl = '/api';
        this.token = this.getToken();
    }

    /**
     * Get JWT token from localStorage or cookie
     */
    getToken() {
        // Try localStorage first (for mobile app)
        let token = localStorage.getItem('jwt_token');
        
        // If not found, try to get from cookie (for web app)
        if (!token) {
            const cookies = document.cookie.split(';');
            for (let cookie of cookies) {
                const [name, value] = cookie.trim().split('=');
                if (name === 'jwt_token') {
                    token = decodeURIComponent(value);
                    break;
                }
            }
        }
        
        return token;
    }

    /**
     * Store JWT token
     */
    setToken(token) {
        this.token = token;
        localStorage.setItem('jwt_token', token);
        // Also set as cookie for web app
        document.cookie = `jwt_token=${encodeURIComponent(token)}; path=/; max-age=86400; SameSite=Lax`;
    }

    /**
     * Clear JWT token
     */
    clearToken() {
        this.token = null;
        localStorage.removeItem('jwt_token');
        document.cookie = 'jwt_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    }

    /**
     * Get headers for API requests
     */
    getHeaders(includeAuth = true) {
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };

        if (includeAuth && this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        return headers;
    }

    /**
     * Make API request
     */
    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const config = {
            ...options,
            headers: {
                ...this.getHeaders(options.requireAuth !== false),
                ...(options.headers || {})
            }
        };

        try {
            const response = await fetch(url, config);
            
            // Handle 401 Unauthorized - token might be expired
            if (response.status === 401) {
                this.clearToken();
                // Try to get token from session if on web app
                if (window.location.pathname.startsWith('/')) {
                    try {
                        const tokenResponse = await fetch('/api/auth/token');
                        if (tokenResponse.ok) {
                            const tokenData = await tokenResponse.json();
                            if (tokenData.token) {
                                this.setToken(tokenData.token);
                                // Retry the request with new token
                                config.headers = {
                                    ...this.getHeaders(true),
                                    ...(options.headers || {})
                                };
                                const retryResponse = await fetch(url, config);
                                if (!retryResponse.ok) {
                                    throw new Error('Unauthorized - Please login again');
                                }
                                const retryData = await retryResponse.json();
                                return retryData;
                            }
                        }
                    } catch (e) {
                        console.error('Token refresh failed:', e);
                    }
                    window.location.href = '/Auth/Login';
                }
                throw new Error('Unauthorized - Please login again');
            }

            // Handle 405 Method Not Allowed
            if (response.status === 405) {
                throw new Error(`Method not allowed. Check if the endpoint supports ${options.method || 'GET'}`);
            }

            // Parse JSON response
            let data;
            try {
                data = await response.json();
            } catch (e) {
                // If response is not JSON, get text
                const text = await response.text();
                throw new Error(`Server error: ${text || response.statusText}`);
            }
            
            if (!response.ok) {
                throw new Error(data.message || `HTTP error! status: ${response.status}`);
            }

            return data;
        } catch (error) {
            console.error('API Request Error:', error);
            console.error('Endpoint:', endpoint);
            console.error('Options:', options);
            throw error;
        }
    }

    // ==================== AUTH ENDPOINTS ====================

    /**
     * Register a new user
     */
    async register(userData) {
        return this.request('/auth/register', {
            method: 'POST',
            body: JSON.stringify(userData),
            requireAuth: false
        });
    }

    /**
     * Login and get JWT token
     */
    async login(email, password) {
        const response = await this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password }),
            requireAuth: false
        });
        
        if (response.token) {
            this.setToken(response.token);
        }
        
        return response;
    }

    /**
     * Logout (clear token)
     */
    logout() {
        this.clearToken();
    }

    // ==================== PRODUCT ENDPOINTS ====================

    /**
     * Get all products (public)
     */
    async getProducts(filters = {}) {
        const queryParams = new URLSearchParams();
        if (filters.searchTerm) queryParams.append('searchTerm', filters.searchTerm);
        if (filters.categoryId) queryParams.append('categoryId', filters.categoryId);
        if (filters.minPrice) queryParams.append('minPrice', filters.minPrice);
        if (filters.maxPrice) queryParams.append('maxPrice', filters.maxPrice);
        if (filters.sortBy) queryParams.append('sortBy', filters.sortBy);

        const query = queryParams.toString();
        return this.request(`/products${query ? '?' + query : ''}`, {
            requireAuth: false
        });
    }

    /**
     * Get product by ID (public)
     */
    async getProduct(id) {
        return this.request(`/products/${id}`, {
            requireAuth: false
        });
    }

    // ==================== BUYER ENDPOINTS ====================

    /**
     * Get buyer dashboard
     */
    async getBuyerDashboard() {
        return this.request('/buyers/dashboard');
    }

    /**
     * Get products with filters (buyer endpoint)
     */
    async getBuyerProducts(filters = {}) {
        const queryParams = new URLSearchParams();
        if (filters.searchTerm) queryParams.append('searchTerm', filters.searchTerm);
        if (filters.categoryId) queryParams.append('categoryId', filters.categoryId);
        if (filters.minPrice) queryParams.append('minPrice', filters.minPrice);
        if (filters.maxPrice) queryParams.append('maxPrice', filters.maxPrice);
        if (filters.sortBy) queryParams.append('sortBy', filters.sortBy);

        const query = queryParams.toString();
        return this.request(`/buyers/products${query ? '?' + query : ''}`, {
            requireAuth: false // Public endpoint
        });
    }

    /**
     * Get product details with reviews
     */
    async getProductDetails(id) {
        return this.request(`/buyers/products/${id}`, {
            requireAuth: false // Public endpoint
        });
    }

    /**
     * Get all categories
     */
    async getCategories() {
        return this.request('/buyers/categories', {
            requireAuth: false // Public endpoint
        });
    }

    /**
     * Add product to cart
     */
    async addToCart(productId, quantity) {
        return this.request('/buyers/cart', {
            method: 'POST',
            body: JSON.stringify({ productId, quantity })
        });
    }

    /**
     * Get cart items
     */
    async getCart() {
        return this.request('/buyers/cart');
    }

    /**
     * Get cart count
     */
    async getCartCount() {
        return this.request('/buyers/cart/count');
    }

    /**
     * Update cart item quantity
     */
    async updateCartItem(productId, quantity) {
        return this.request(`/buyers/cart/${productId}`, {
            method: 'PUT',
            body: JSON.stringify({ quantity })
        });
    }

    /**
     * Remove item from cart
     */
    async removeFromCart(productId) {
        return this.request(`/buyers/cart/${productId}`, {
            method: 'DELETE'
        });
    }

    /**
     * Get checkout information
     */
    async getCheckout() {
        return this.request('/buyers/checkout');
    }

    /**
     * Process checkout
     */
    async processCheckout(checkoutData) {
        return this.request('/buyers/checkout', {
            method: 'POST',
            body: JSON.stringify(checkoutData)
        });
    }

    /**
     * Get all orders
     */
    async getOrders() {
        return this.request('/buyers/orders');
    }

    /**
     * Get order by ID
     */
    async getOrder(id) {
        return this.request(`/buyers/orders/${id}`);
    }

    /**
     * Confirm delivery
     */
    async confirmDelivery(orderId, rating, comment) {
        return this.request(`/buyers/orders/${orderId}/confirm-delivery`, {
            method: 'POST',
            body: JSON.stringify({ rating, comment })
        });
    }

    /**
     * Add to favorites
     */
    async addToFavorites(productId) {
        return this.request(`/buyers/favorites/${productId}`, {
            method: 'POST'
        });
    }

    /**
     * Remove from favorites
     */
    async removeFromFavorites(productId) {
        return this.request(`/buyers/favorites/${productId}`, {
            method: 'DELETE'
        });
    }

    /**
     * Get favorites
     */
    async getFavorites() {
        return this.request('/buyers/favorites');
    }

    /**
     * Submit product review
     */
    async submitReview(productId, rating, comment) {
        return this.request(`/buyers/products/${productId}/reviews`, {
            method: 'POST',
            body: JSON.stringify({ rating, comment })
        });
    }

    /**
     * Get buyer profile
     */
    async getBuyerProfile() {
        return this.request('/buyers/profile');
    }

    /**
     * Update buyer profile
     */
    async updateBuyerProfile(profileData) {
        return this.request('/buyers/profile', {
            method: 'PUT',
            body: JSON.stringify(profileData)
        });
    }

    // ==================== FARMER ENDPOINTS ====================

    /**
     * Get farmer dashboard
     */
    async getFarmerDashboard() {
        return this.request('/farmers/dashboard');
    }

    /**
     * Get farmer's products
     */
    async getFarmerProducts() {
        return this.request('/farmers/products');
    }

    /**
     * Get farmer product by ID
     */
    async getFarmerProduct(id) {
        return this.request(`/farmers/products/${id}`);
    }

    /**
     * Create product
     */
    async createProduct(productData) {
        return this.request('/farmers/products', {
            method: 'POST',
            body: JSON.stringify(productData)
        });
    }

    /**
     * Update product
     */
    async updateProduct(id, productData) {
        return this.request(`/farmers/products/${id}`, {
            method: 'PUT',
            body: JSON.stringify(productData)
        });
    }

    /**
     * Delete product
     */
    async deleteProduct(id) {
        return this.request(`/farmers/products/${id}`, {
            method: 'DELETE'
        });
    }

    /**
     * Toggle product availability
     */
    async toggleProductAvailability(id) {
        return this.request(`/farmers/products/${id}/toggle-availability`, {
            method: 'POST'
        });
    }

    /**
     * Get farmer's orders
     */
    async getFarmerOrders() {
        return this.request('/farmers/orders');
    }

    /**
     * Get farmer order by ID
     */
    async getFarmerOrder(id) {
        return this.request(`/farmers/orders/${id}`);
    }

    /**
     * Accept order
     */
    async acceptOrder(id, estimatedDeliveryDate) {
        return this.request(`/farmers/orders/${id}/accept`, {
            method: 'POST',
            body: JSON.stringify({ estimatedDeliveryDate })
        });
    }

    /**
     * Reject order
     */
    async rejectOrder(id, rejectionReason) {
        return this.request(`/farmers/orders/${id}/reject`, {
            method: 'POST',
            body: JSON.stringify({ rejectionReason })
        });
    }

    /**
     * Mark order as shipped
     */
    async markOrderShipped(id, trackingNumber, estimatedDeliveryDate) {
        return this.request(`/farmers/orders/${id}/mark-shipped`, {
            method: 'POST',
            body: JSON.stringify({ trackingNumber, estimatedDeliveryDate })
        });
    }

    /**
     * Mark order as arrived
     */
    async markOrderArrived(id) {
        return this.request(`/farmers/orders/${id}/mark-arrived`, {
            method: 'POST'
        });
    }

    /**
     * Get farmer earnings
     */
    async getFarmerEarnings() {
        return this.request('/farmers/earnings');
    }

    /**
     * Get farmer profile
     */
    async getFarmerProfile() {
        return this.request('/farmers/profile');
    }

    /**
     * Update farmer profile
     */
    async updateFarmerProfile(profileData) {
        return this.request('/farmers/profile', {
            method: 'PUT',
            body: JSON.stringify(profileData)
        });
    }
}

// Create global instance
const apiClient = new ApiClient();

