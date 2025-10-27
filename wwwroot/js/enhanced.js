// Reacoda Molemi - Enhanced JavaScript
// Modern interactions and animations

document.addEventListener('DOMContentLoaded', function() {
    // Initialize animations
    initializeAnimations();
    
    // Initialize cart functionality
    initializeCart();
    
    // Initialize form enhancements
    initializeForms();
    
    // Initialize tooltips and popovers
    initializeTooltips();
    
    // Initialize smooth scrolling
    initializeSmoothScrolling();
    
    // Initialize lazy loading for images
    initializeLazyLoading();
});

// Animation initialization
function initializeAnimations() {
    // Add fade-in animation to elements as they come into view
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in-up');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe all cards and sections
    document.querySelectorAll('.card, .dashboard-card, .product-card').forEach(el => {
        observer.observe(el);
    });
}

// Cart functionality
function initializeCart() {
    // Update cart count on page load
    updateCartCount();
    
    // Add to cart functionality
    document.querySelectorAll('.add-to-cart-btn').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const productId = this.dataset.productId;
            addToCart(productId);
        });
    });
    
    // Buy now functionality
    document.querySelectorAll('.buy-now-btn').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const productId = this.dataset.productId;
            buyNow(productId);
        });
    });
}

// Add to cart function
async function addToCart(productId) {
    try {
        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('quantity', 1);
        
        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            formData.append('__RequestVerificationToken', token.value);
        }
        
        const response = await fetch('/Buyer/AddToCart', {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            // Show success message
            showNotification('Product added to cart!', 'success');
            
            // Update cart count
            updateCartCount();
            
            // Add animation to button
            const btn = document.querySelector(`[data-product-id="${productId}"]`);
            if (btn) {
                btn.classList.add('btn-success');
                setTimeout(() => {
                    btn.classList.remove('btn-success');
                }, 1000);
            }
        } else {
            showNotification(result.message || 'Failed to add product to cart', 'error');
        }
    } catch (error) {
        console.error('Error adding to cart:', error);
        showNotification('An error occurred. Please try again.', 'error');
    }
}

// Buy now function
async function buyNow(productId) {
    try {
        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('quantity', 1);
        
        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            formData.append('__RequestVerificationToken', token.value);
        }
        
        const response = await fetch('/Buyer/AddToCart', {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            // Redirect to checkout
            window.location.href = '/Buyer/Checkout';
        } else {
            showNotification(result.message || 'Failed to add product to cart', 'error');
        }
    } catch (error) {
        console.error('Error with buy now:', error);
        showNotification('An error occurred. Please try again.', 'error');
    }
}

// Update cart count
async function updateCartCount() {
    try {
        const response = await fetch('/Buyer/GetCartCount');
        const result = await response.json();
        
        const cartBadge = document.querySelector('.cart-badge');
        if (cartBadge) {
            cartBadge.textContent = result.count;
            cartBadge.style.display = result.count > 0 ? 'flex' : 'none';
        }
    } catch (error) {
        console.error('Error updating cart count:', error);
    }
}

// Form enhancements
function initializeForms() {
    // Add floating labels effect
    document.querySelectorAll('.form-control').forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        input.addEventListener('blur', function() {
            if (!this.value) {
                this.parentElement.classList.remove('focused');
            }
        });
        
        // Check if input has value on load
        if (input.value) {
            input.parentElement.classList.add('focused');
        }
    });
    
    // Add form validation feedback
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
                
                // Re-enable after 3 seconds (in case of error)
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = submitBtn.dataset.originalText || 'Submit';
                }, 3000);
            }
        });
    });
}

// Tooltips and popovers
function initializeTooltips() {
    // Initialize Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize Bootstrap popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Smooth scrolling
function initializeSmoothScrolling() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// Lazy loading for images
function initializeLazyLoading() {
    const images = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });
    
    images.forEach(img => imageObserver.observe(img));
}

// Notification system
function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notification => notification.remove());
    
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        border: none;
        border-radius: 8px;
    `;
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Favorite functionality
function toggleFavorite(productId) {
    const btn = document.querySelector(`[data-product-id="${productId}"]`);
    const isFavorited = btn.classList.contains('favorited');
    
    fetch('/Buyer/AddToFavorites', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            productId: productId,
            isFavorited: isFavorited
        })
    })
    .then(response => response.json())
    .then(result => {
        if (result.success) {
            if (isFavorited) {
                btn.classList.remove('favorited');
                btn.innerHTML = '<i class="far fa-heart"></i>';
                showNotification('Removed from favorites', 'info');
            } else {
                btn.classList.add('favorited');
                btn.innerHTML = '<i class="fas fa-heart"></i>';
                showNotification('Added to favorites', 'success');
            }
        } else {
            showNotification(result.message || 'Failed to update favorites', 'error');
        }
    })
    .catch(error => {
        console.error('Error updating favorites:', error);
        showNotification('An error occurred. Please try again.', 'error');
    });
}

// Search functionality
function initializeSearch() {
    const searchInput = document.querySelector('#searchInput');
    if (searchInput) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performSearch(this.value);
            }, 300);
        });
    }
}

// Perform search
function performSearch(query) {
    if (query.length < 2) return;
    
    // Add loading state
    const searchResults = document.querySelector('#searchResults');
    if (searchResults) {
        searchResults.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Searching...</div>';
    }
    
    // Perform search request
    fetch(`/Buyer/Search?query=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(results => {
            displaySearchResults(results);
        })
        .catch(error => {
            console.error('Search error:', error);
        });
}

// Display search results
function displaySearchResults(results) {
    const searchResults = document.querySelector('#searchResults');
    if (!searchResults) return;
    
    if (results.length === 0) {
        searchResults.innerHTML = '<div class="text-center text-muted">No products found</div>';
        return;
    }
    
    const html = results.map(product => `
        <div class="col-md-6 col-lg-4 mb-3">
            <div class="card product-card">
                <img src="${product.imageUrl}" class="card-img-top" alt="${product.name}">
                <div class="card-body">
                    <h5 class="card-title">${product.name}</h5>
                    <p class="card-text">${product.description}</p>
                    <p class="product-price">R ${product.price.toFixed(2)}</p>
                    <button class="btn btn-primary add-to-cart-btn" data-product-id="${product.id}">
                        <i class="fas fa-cart-plus me-2"></i>Add to Cart
                    </button>
                </div>
            </div>
        </div>
    `).join('');
    
    searchResults.innerHTML = html;
}

// Loading states
function showLoading(element) {
    element.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Loading...';
    element.disabled = true;
}

function hideLoading(element, originalText) {
    element.innerHTML = originalText;
    element.disabled = false;
}

// Utility functions
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function throttle(func, limit) {
    let inThrottle;
    return function() {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Export functions for global use
window.ReacodaMolemi = {
    addToCart,
    buyNow,
    toggleFavorite,
    showNotification,
    updateCartCount
};
