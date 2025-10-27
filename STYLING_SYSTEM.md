# Reacoda Molemi - Styling System

## Overview
Reacoda Molemi features a modern, responsive design system built with Bootstrap 5 and custom CSS. The design emphasizes clean aesthetics, smooth animations, and excellent user experience across all devices.

## Design System

### Color Palette
- **Primary Green**: `#1FAA59` - Main brand color for buttons, links, and accents
- **Primary Red**: `#E74C3C` - Secondary color for alerts, warnings, and CTAs
- **Accent Cream**: `#FFF8F0` - Light background color for sections
- **Accent White**: `#FFFFFF` - Card backgrounds and main content areas
- **Text Dark**: `#2C3E50` - Primary text color
- **Text Muted**: `#6C757D` - Secondary text color
- **Text Light**: `#ADB5BD` - Tertiary text color

### Typography
- **Primary Font**: Inter - Clean, modern sans-serif for body text
- **Heading Font**: Poppins - Rounded, friendly sans-serif for headings
- **Font Weights**: 300, 400, 500, 600, 700

### Spacing & Layout
- **Border Radius**: 12px (standard), 8px (small), 16px (large)
- **Shadows**: Light (`0 2px 4px rgba(0,0,0,0.1)`), Medium (`0 4px 8px rgba(0,0,0,0.15)`), Large (`0 8px 16px rgba(0,0,0,0.2)`)
- **Transitions**: `all 0.3s cubic-bezier(0.4, 0, 0.2, 1)` for smooth animations

## Component Styles

### Cards
- **Product Cards**: Hover effects with scale and shadow changes
- **Dashboard Cards**: Color-coded left borders (green, red, blue, orange)
- **Feature Cards**: Centered content with large icons

### Buttons
- **Primary**: Green gradient with hover animations
- **Secondary**: Outline style with color transitions
- **Danger**: Red gradient for destructive actions
- **Success**: Green gradient for positive actions
- **Warning**: Orange gradient for caution actions
- **Info**: Blue gradient for informational actions

### Forms
- **Input Fields**: Custom focus states with green borders
- **Labels**: Bold, dark text with proper spacing
- **Validation**: Color-coded feedback with smooth transitions

### Navigation
- **Header**: Clean white background with subtle shadow
- **Brand**: Green logo with hover effects
- **Links**: Smooth hover transitions with background color changes
- **Cart Badge**: Animated pulse effect for new items

### Tables
- **Headers**: Green gradient backgrounds
- **Rows**: Hover effects with cream background
- **Borders**: Light gray separators

## Responsive Design

### Breakpoints
- **Mobile**: < 576px
- **Tablet**: 576px - 768px
- **Desktop**: 768px - 992px
- **Large Desktop**: 992px - 1200px
- **Extra Large**: > 1200px

### Mobile Optimizations
- Reduced font sizes for better readability
- Simplified card layouts
- Touch-friendly button sizes
- Optimized image heights

## Animations

### Entrance Animations
- **Fade In Up**: Elements slide up and fade in
- **Slide In Right**: Elements slide in from the right
- **Staggered**: Multiple elements animate with delays

### Interactive Animations
- **Button Hover**: Lift effect with shadow increase
- **Card Hover**: Scale and shadow changes
- **Cart Badge**: Pulse animation for new items
- **Form Focus**: Smooth border color transitions

## JavaScript Enhancements

### Features
- **Cart Management**: Real-time cart updates
- **Form Validation**: Enhanced user feedback
- **Smooth Scrolling**: Animated page navigation
- **Lazy Loading**: Optimized image loading
- **Notifications**: Toast-style alerts
- **Search**: Debounced search functionality

### Performance
- **Debounced Functions**: Optimized search and input handling
- **Throttled Events**: Smooth scroll and resize handling
- **Intersection Observer**: Efficient animation triggers
- **Lazy Loading**: Reduced initial page load time

## Usage Guidelines

### CSS Classes
```css
/* Utility Classes */
.text-primary-green    /* Green text color */
.text-primary-red      /* Red text color */
.bg-primary-green      /* Green background */
.bg-primary-red        /* Red background */
.bg-accent-cream       /* Cream background */
.shadow-custom         /* Custom shadow */
.rounded-custom        /* Custom border radius */

/* Component Classes */
.dashboard-card        /* Dashboard statistics card */
.product-card          /* Product display card */
.hero-section          /* Hero banner section */
.footer                /* Footer styling */
```

### JavaScript Functions
```javascript
// Global functions available
ReacodaMolemi.addToCart(productId)      // Add product to cart
ReacodaMolemi.buyNow(productId)         // Buy now functionality
ReacodaMolemi.toggleFavorite(productId) // Toggle favorite status
ReacodaMolemi.showNotification(message, type) // Show notification
ReacodaMolemi.updateCartCount()         // Update cart badge
```

## Browser Support
- **Chrome**: 90+
- **Firefox**: 88+
- **Safari**: 14+
- **Edge**: 90+

## Performance Considerations
- **CSS**: Minified and optimized
- **JavaScript**: Debounced and throttled functions
- **Images**: Lazy loaded with optimized sizes
- **Fonts**: Preloaded for faster rendering
- **Animations**: Hardware accelerated with CSS transforms

## Accessibility
- **Color Contrast**: WCAG AA compliant
- **Keyboard Navigation**: Full keyboard support
- **Screen Readers**: Proper ARIA labels
- **Focus States**: Visible focus indicators
- **Alt Text**: Descriptive image alternatives

## Customization
The styling system is built with CSS custom properties (variables) for easy customization:

```css
:root {
    --primary-green: #1FAA59;
    --primary-red: #E74C3C;
    --accent-cream: #FFF8F0;
    /* ... other variables */
}
```

To customize colors, simply update the CSS custom properties in `wwwroot/css/custom.css`.

## File Structure
```
wwwroot/
├── css/
│   ├── custom.css          # Main custom styles
│   └── site.css            # Additional site styles
├── js/
│   ├── enhanced.js         # Enhanced JavaScript functionality
│   └── site.js             # Basic site JavaScript
└── lib/                    # Third-party libraries
```

## Maintenance
- **Regular Updates**: Keep Bootstrap and dependencies updated
- **Performance Monitoring**: Monitor Core Web Vitals
- **Browser Testing**: Test across different browsers and devices
- **Accessibility Audits**: Regular accessibility checks
- **Code Reviews**: Maintain code quality and consistency
