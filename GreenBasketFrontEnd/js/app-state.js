/**
 * GreenBasket Core Application State & LocalStorage Synchronization Engine
 * Handles catalog items, traceability data, reactive shopping cart, order history,
 * delivery slot tracking, and staff/admin management operations.
 */

(function () {
    'use strict';

    const STORAGE_KEYS = {
        PRODUCTS: 'gb_products_v3',
        CART: 'gb_cart_v1',
        ORDERS: 'gb_orders_v1',
        ADDRESSES: 'gb_addresses_v1',
        REPORTS: 'gb_quality_reports_v1',
        USER_ROLE: 'gb_user_role_v1',
        AUTH_USER: 'gb_auth_user_v1'
    };

    // Default Fresh Produce Seed Data (FR-2.1, FR-2.2, FR-2.4, FR-6.2)
    const DEFAULT_PRODUCTS = [
        {
            id: 'gb-001',
            name: 'Organic Dalat Spinach',
            category: 'leafy-greens',
            categoryName: 'Leafy Greens',
            price: 3.50,
            unit: 'kg',
            image: 'img/vegetable-item-2.jpg',
            farmOrigin: 'Green Valley Organic Farm, Dalat',
            harvestDate: '2026-08-02',
            stockStatus: 'In Stock',
            stockQuantity: 50,
            rating: 4.9,
            organic: true,
            description: 'Freshly harvested VietGAP organic spinach leaves packed with vitamins, iron, and rich crisp texture.'
        },
        {
            id: 'gb-002',
            name: 'Hydroponic Romaine Lettuce',
            category: 'leafy-greens',
            categoryName: 'Leafy Greens',
            price: 2.80,
            unit: '500g',
            image: 'img/vegetable-item-6.jpg',
            farmOrigin: 'Highland Hydroponic Eco-Farm',
            harvestDate: '2026-08-03',
            stockStatus: 'In Stock',
            stockQuantity: 35,
            rating: 4.8,
            organic: true,
            description: 'Pesticide-free crunchy romaine lettuce heads grown hydroponically with pure mountain water.'
        },
        {
            id: 'gb-003',
            name: 'Organic Red Bell Peppers',
            category: 'root-veggies',
            categoryName: 'Root Vegetables',
            price: 4.20,
            unit: 'kg',
            image: 'img/vegetable-item-4.jpg',
            farmOrigin: 'Sunrise Agri Farm, Moc Chau',
            harvestDate: '2026-08-01',
            stockStatus: 'In Stock',
            stockQuantity: 40,
            rating: 4.7,
            organic: true,
            description: 'Naturally sweet and juicy organic bell peppers, ideal for juicing, salads, or home cooking.'
        },
        {
            id: 'gb-004',
            name: 'Farm Sweet Potatoes',
            category: 'root-veggies',
            categoryName: 'Root Vegetables',
            price: 3.90,
            unit: 'kg',
            image: 'img/vegetable-item-5.jpg',
            farmOrigin: 'Vinh Long Eco Produce',
            harvestDate: '2026-07-31',
            stockStatus: 'Low Stock',
            stockQuantity: 8,
            rating: 4.9,
            organic: false,
            description: 'Rich in antioxidants, naturally sweet potatoes harvested directly from Mekong Delta soil.'
        },
        {
            id: 'gb-005',
            name: 'Highland Green Grapes',
            category: 'seasonal-fruit',
            categoryName: 'Seasonal Fruit',
            price: 6.99,
            unit: 'kg',
            image: 'img/fruite-item-5.jpg',
            farmOrigin: 'Ninh Thuan Vineyard Collective',
            harvestDate: '2026-08-02',
            stockStatus: 'In Stock',
            stockQuantity: 25,
            rating: 4.9,
            organic: true,
            description: 'Seedless green grapes harvested at peak sweetness with crisp bite and bursting juice.'
        },
        {
            id: 'gb-006',
            name: 'Fresh Red Raspberries',
            category: 'seasonal-fruit',
            categoryName: 'Seasonal Fruit',
            price: 8.50,
            unit: '500g',
            image: 'img/fruite-item-2.jpg',
            farmOrigin: 'Dalat Alpine Berry Farm',
            harvestDate: '2026-08-03',
            stockStatus: 'Low Stock',
            stockQuantity: 5,
            rating: 5.0,
            organic: true,
            description: 'Hand-picked fresh wild red raspberries cooled in cold-chain containers to preserve delicate flavor.'
        },
        {
            id: 'gb-007',
            name: 'Mekong Golden Oranges',
            category: 'tropical-fruit',
            categoryName: 'Tropical Fruit',
            price: 4.50,
            unit: 'kg',
            image: 'img/fruite-item-1.jpg',
            farmOrigin: 'Tien Giang Tropical Groves',
            harvestDate: '2026-08-01',
            stockStatus: 'In Stock',
            stockQuantity: 60,
            rating: 4.8,
            organic: false,
            description: 'Fragrant golden ripe oranges featuring juicy fiberless flesh and rich natural sweetness.'
        },
        {
            id: 'gb-008',
            name: 'Farm Fresh Organic Bananas',
            category: 'tropical-fruit',
            categoryName: 'Tropical Fruit',
            price: 3.80,
            unit: 'kg',
            image: 'img/vegetable-item-3.png',
            farmOrigin: 'Binh Thuan Sunrise Farms',
            harvestDate: '2026-08-02',
            stockStatus: 'In Stock',
            stockQuantity: 30,
            rating: 4.6,
            organic: true,
            description: 'Sweet organic bananas high in potassium and fiber, directly sourced from certified farms.'
        }
    ];

    // Seed Orders (FR-5.1, FR-5.3)
    const DEFAULT_ORDERS = [
        {
            id: 'ORD-9821',
            date: '2026-08-03 09:15',
            customerName: 'Alex Johnson',
            email: 'alex@example.com',
            phone: '0901234567',
            deliveryAddress: '123 High Street, District 1, HCMC',
            deliverySlot: 'Today (Aug 03): 14:00 - 16:00',
            paymentMethod: 'Credit Card / Online',
            items: [
                { id: 'gb-001', name: 'Organic Dalat Spinach', price: 3.50, qty: 2, unit: 'kg', image: 'img/fruite-item-5.jpg' },
                { id: 'gb-005', name: 'Highland Green Grapes', price: 6.99, qty: 1, unit: 'kg', image: 'img/fruite-item-5.jpg' }
            ],
            subtotal: 13.99,
            deliveryFee: 2.00,
            total: 15.99,
            status: 'Processing',
            qualityReport: null
        },
        {
            id: 'ORD-9784',
            date: '2026-08-02 15:30',
            customerName: 'Sarah Miller',
            email: 'sarah@example.com',
            phone: '0987654321',
            deliveryAddress: '45 Green Park Avenue, District 2, HCMC',
            deliverySlot: 'Aug 02: 16:00 - 18:00',
            paymentMethod: 'Cash on Delivery (COD)',
            items: [
                { id: 'gb-003', name: 'Organic Sweet Carrots', price: 4.20, qty: 1, unit: 'kg', image: 'img/fruite-item-4.jpg' },
                { id: 'gb-007', name: 'Mekong Golden Mango', price: 4.50, qty: 2, unit: 'kg', image: 'img/fruite-item-1.jpg' }
            ],
            subtotal: 13.20,
            deliveryFee: 2.00,
            total: 15.20,
            status: 'Out for Delivery',
            qualityReport: null
        },
        {
            id: 'ORD-9650',
            date: '2026-08-01 11:00',
            customerName: 'Alex Johnson',
            email: 'alex@example.com',
            phone: '0901234567',
            deliveryAddress: '123 High Street, District 1, HCMC',
            deliverySlot: 'Aug 01: 14:00 - 16:00',
            paymentMethod: 'MoMo E-Wallet',
            items: [
                { id: 'gb-006', name: 'Fresh Red Raspberries', price: 8.50, qty: 1, unit: '500g', image: 'img/fruite-item-2.jpg' }
            ],
            subtotal: 8.50,
            deliveryFee: 2.00,
            total: 10.50,
            status: 'Delivered',
            qualityReport: {
                ticketId: 'TKT-102',
                issueType: 'Damaged during transit',
                comments: 'Two raspberries were bruised upon arrival.',
                date: '2026-08-01 16:45',
                status: 'Refund Approved'
            }
        }
    ];

    const DEFAULT_ADDRESSES = [
        {
            id: 'addr-1',
            fullName: 'Alex Johnson',
            phone: '+84 901 234 567',
            street: '123 High Street, Ward Ben Nghe',
            district: 'District 1',
            city: 'Ho Chi Minh City',
            isDefault: true
        }
    ];

    // Helper functions for LocalStorage load & save
    function loadStorage(key, fallback) {
        try {
            const data = localStorage.getItem(key);
            return data ? JSON.parse(data) : fallback;
        } catch (e) {
            console.error('Error reading localStorage key: ' + key, e);
            return fallback;
        }
    }

    function saveStorage(key, value) {
        try {
            localStorage.setItem(key, JSON.stringify(value));
            // Trigger global change event for cross-component reactivity
            window.dispatchEvent(new CustomEvent('gb_state_change', { detail: { key, value } }));
        } catch (e) {
            console.error('Error saving localStorage key: ' + key, e);
        }
    }

    // Public AppState API
    window.AppState = {
        // --- Role Management ---
        getUserRole() {
            return loadStorage(STORAGE_KEYS.USER_ROLE, 'Customer');
        },
        setUserRole(role) {
            saveStorage(STORAGE_KEYS.USER_ROLE, role);
        },

        // --- Catalog & Stock (FR-2.1 to FR-2.4, FR-6.1, FR-6.2) ---
        getProducts() {
            return loadStorage(STORAGE_KEYS.PRODUCTS, DEFAULT_PRODUCTS);
        },
        getProductById(id) {
            const products = this.getProducts();
            return products.find(p => p.id === id) || null;
        },
        saveProduct(productData) {
            const products = this.getProducts();
            const existingIdx = products.findIndex(p => p.id === productData.id);
            if (existingIdx >= 0) {
                products[existingIdx] = { ...products[existingIdx], ...productData };
            } else {
                const newId = 'gb-' + String(products.length + 1).padStart(3, '0');
                products.push({ id: newId, ...productData });
            }
            saveStorage(STORAGE_KEYS.PRODUCTS, products);
            return products;
        },
        deleteProduct(id) {
            let products = this.getProducts();
            products = products.filter(p => p.id !== id);
            saveStorage(STORAGE_KEYS.PRODUCTS, products);
            return products;
        },

        // --- Shopping Cart (FR-3.1) ---
        getCart() {
            return loadStorage(STORAGE_KEYS.CART, []);
        },
        addToCart(productId, qty = 1) {
            const product = this.getProductById(productId);
            if (!product) return false;
            
            let cart = this.getCart();
            const existingItem = cart.find(item => item.productId === productId);
            if (existingItem) {
                existingItem.qty += qty;
            } else {
                cart.push({
                    productId: product.id,
                    name: product.name,
                    price: product.price,
                    unit: product.unit,
                    image: product.image,
                    farmOrigin: product.farmOrigin,
                    qty: qty
                });
            }
            saveStorage(STORAGE_KEYS.CART, cart);
            return true;
        },
        updateCartQty(productId, qty) {
            let cart = this.getCart();
            const item = cart.find(i => i.productId === productId);
            if (item) {
                item.qty = Math.max(1, qty);
                saveStorage(STORAGE_KEYS.CART, cart);
            }
        },
        removeFromCart(productId) {
            let cart = this.getCart();
            cart = cart.filter(i => i.productId !== productId);
            saveStorage(STORAGE_KEYS.CART, cart);
        },
        clearCart() {
            saveStorage(STORAGE_KEYS.CART, []);
        },
        getCartCount() {
            const cart = this.getCart();
            return cart.reduce((sum, item) => sum + item.qty, 0);
        },
        getCartSubtotal() {
            const cart = this.getCart();
            return cart.reduce((sum, item) => sum + (item.price * item.qty), 0);
        },

        // --- Delivery Addresses (FR-1.2) ---
        getAddresses() {
            return loadStorage(STORAGE_KEYS.ADDRESSES, DEFAULT_ADDRESSES);
        },
        saveAddress(addressData) {
            let addresses = this.getAddresses();
            if (addressData.isDefault) {
                addresses.forEach(a => a.isDefault = false);
            }
            if (addressData.id) {
                const idx = addresses.findIndex(a => a.id === addressData.id);
                if (idx >= 0) addresses[idx] = addressData;
            } else {
                addressData.id = 'addr-' + Date.now();
                addresses.push(addressData);
            }
            saveStorage(STORAGE_KEYS.ADDRESSES, addresses);
            return addresses;
        },

        // --- Orders & Tracking (FR-3.2, FR-3.3, FR-4.1, FR-5.1) ---
        getOrders() {
            return loadStorage(STORAGE_KEYS.ORDERS, DEFAULT_ORDERS);
        },
        getOrderById(orderId) {
            const orders = this.getOrders();
            return orders.find(o => o.id === orderId) || null;
        },
        placeOrder(checkoutDetails) {
            const cart = this.getCart();
            if (cart.length === 0) return null;

            const subtotal = this.getCartSubtotal();
            const deliveryFee = subtotal >= 30 ? 0.00 : 2.00; // Free shipping over $30
            const total = subtotal + deliveryFee;

            const newOrder = {
                id: 'ORD-' + Math.floor(1000 + Math.random() * 9000),
                date: new Date().toISOString().replace('T', ' ').substring(0, 16),
                customerName: checkoutDetails.fullName || 'Alex Johnson',
                email: checkoutDetails.email || 'alex@example.com',
                phone: checkoutDetails.phone || '0901234567',
                deliveryAddress: checkoutDetails.address || '123 High Street, District 1, HCMC',
                deliverySlot: checkoutDetails.deliverySlot || 'Today: 14:00 - 16:00',
                paymentMethod: checkoutDetails.paymentMethod || 'Cash on Delivery',
                items: [...cart],
                subtotal: subtotal,
                deliveryFee: deliveryFee,
                total: total,
                status: 'Processing',
                qualityReport: null
            };

            const orders = this.getOrders();
            orders.unshift(newOrder);
            saveStorage(STORAGE_KEYS.ORDERS, orders);
            this.clearCart();
            return newOrder;
        },
        updateOrderStatus(orderId, newStatus) {
            const orders = this.getOrders();
            const order = orders.find(o => o.id === orderId);
            if (order) {
                order.status = newStatus;
                saveStorage(STORAGE_KEYS.ORDERS, orders);
                return true;
            }
            return false;
        },
        cancelOrder(orderId) {
            const orders = this.getOrders();
            const order = orders.find(o => o.id === orderId);
            if (order && order.status === 'Processing') {
                order.status = 'Cancelled';
                saveStorage(STORAGE_KEYS.ORDERS, orders);
                return true;
            }
            return false;
        },

        // --- Quality Issue Reports (FR-5.3, FR-6.3) ---
        submitQualityReport(orderId, issueType, comments) {
            const orders = this.getOrders();
            const order = orders.find(o => o.id === orderId);
            if (!order) return false;

            const ticket = {
                ticketId: 'TKT-' + Math.floor(100 + Math.random() * 900),
                orderId: orderId,
                issueType: issueType,
                comments: comments,
                date: new Date().toISOString().replace('T', ' ').substring(0, 16),
                status: 'Pending Review'
            };

            order.qualityReport = ticket;
            saveStorage(STORAGE_KEYS.ORDERS, orders);
            return ticket;
        },
        resolveQualityReport(orderId, resolutionStatus) {
            const orders = this.getOrders();
            const order = orders.find(o => o.id === orderId);
            if (order && order.qualityReport) {
                order.qualityReport.status = resolutionStatus;
                saveStorage(STORAGE_KEYS.ORDERS, orders);
                return true;
            }
            return false;
        },

        // --- Analytics Helper (FR-6.4) ---
        getAnalytics() {
            const orders = this.getOrders();
            const products = this.getProducts();

            const completedOrders = orders.filter(o => o.status === 'Delivered');
            const totalRevenue = completedOrders.reduce((sum, o) => sum + o.total, 0);
            const totalOrders = orders.length;

            return {
                totalRevenue: totalRevenue.toFixed(2),
                totalOrders: totalOrders,
                deliveredCount: completedOrders.length,
                pendingCount: orders.filter(o => o.status === 'Processing').length,
                activeProducts: products.length,
                lowStockCount: products.filter(p => p.stockStatus === 'Low Stock' || p.stockQuantity < 10).length
            };
        },

        // --- Authentication Management ---
        getAuthUser() {
            return loadStorage(STORAGE_KEYS.AUTH_USER, null);
        },
        isLoggedIn() {
            return !!this.getAuthUser();
        },
        loginUser(email, password) {
            const isStaff = email.includes('staff') || email.includes('admin');
            const user = {
                name: email.split('@')[0].replace('.', ' '),
                email: email,
                role: isStaff ? 'Staff / Admin' : 'Customer',
                loginTime: new Date().toISOString()
            };
            this.setUserRole(user.role);
            saveStorage(STORAGE_KEYS.AUTH_USER, user);
            return user;
        },
        registerUser(name, email, password) {
            const user = {
                name: name || 'New Customer',
                email: email,
                role: 'Customer',
                loginTime: new Date().toISOString()
            };
            this.setUserRole('Customer');
            saveStorage(STORAGE_KEYS.AUTH_USER, user);
            return user;
        },
        logoutUser() {
            saveStorage(STORAGE_KEYS.AUTH_USER, null);
        }
    };

    // Ensure default initializations if storage empty
    if (!localStorage.getItem(STORAGE_KEYS.PRODUCTS)) {
        saveStorage(STORAGE_KEYS.PRODUCTS, DEFAULT_PRODUCTS);
    }
    if (!localStorage.getItem(STORAGE_KEYS.ORDERS)) {
        saveStorage(STORAGE_KEYS.ORDERS, DEFAULT_ORDERS);
    }
    if (!localStorage.getItem(STORAGE_KEYS.ADDRESSES)) {
        saveStorage(STORAGE_KEYS.ADDRESSES, DEFAULT_ADDRESSES);
    }

})();
