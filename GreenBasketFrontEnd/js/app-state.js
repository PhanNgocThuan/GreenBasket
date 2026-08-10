/**
 * GreenBasket Core Application State & LocalStorage Synchronization Engine
 * Handles catalog items, traceability data, reactive shopping cart, order history,
 * delivery slot tracking, and staff/admin management operations.
 */

(function () {
    'use strict';

    const STORAGE_KEYS = {
        PRODUCTS: 'gb_products_v5',
        CART: 'gb_cart_v1',
        ORDERS: 'gb_orders_v1',
        ADDRESSES: 'gb_addresses_v1',
        REPORTS: 'gb_quality_reports_v1',
        USER_ROLE: 'gb_user_role_v1',
        AUTH_USER: 'gb_auth_user_v1',
        JWT_TOKEN: 'gb_jwt_token_v1',
        FARMS: 'gb_farms_v1',
        USERS_DB: 'gb_users_db_v1'
    };

    // Correct Backend API URL (port 5062 matching launchSettings.json)
    const API_BASE_URL = (window.ENV && window.ENV.API_URL) ? window.ENV.API_URL : (localStorage.getItem('gb_api_url') || 'http://localhost:5062/api');

    const DEFAULT_USERS_DB = [
        { email: 'admin@greenbasket.com', password: 'Admin@12345', name: 'System Administrator', role: 'Staff / Admin' },
        { email: 'alex@example.com', password: 'password123', name: 'Alex Johnson', role: 'Customer' },
        { email: 'customer@example.com', password: 'password123', name: 'Demo Customer', role: 'Customer' }
    ];

    // Synchronized C# Backend Fresh Produce Catalog (DbInitializer.cs)
    const DEFAULT_PRODUCTS = [
        {
            id: '1',
            name: 'Crystal Lettuce',
            category: 'leafy-greens',
            categoryName: 'Leafy Greens',
            price: 1.50,
            unit: 'kg',
            image: 'img/lettuce.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-05',
            stockStatus: 'In Stock',
            stockQuantity: 120,
            rating: 4.9,
            organic: true,
            description: 'Crisp hydroponic lettuce freshly harvested from Dalat greenhouses.'
        },
        {
            id: '2',
            name: 'Green Broccoli',
            category: 'leafy-greens',
            categoryName: 'Leafy Greens',
            price: 1.70,
            unit: 'kg',
            image: 'img/broccoli.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-06',
            stockStatus: 'In Stock',
            stockQuantity: 85,
            rating: 4.8,
            organic: true,
            description: 'Freshly cut organic broccoli crowns, harvested same-day.'
        },
        {
            id: '3',
            name: 'Celery Stalks',
            category: 'leafy-greens',
            categoryName: 'Leafy Greens',
            price: 1.20,
            unit: 'kg',
            image: 'img/celery.jpg',
            farmOrigin: 'Sunrise Organic Farm, Hanoi',
            harvestDate: '2026-08-04',
            stockStatus: 'In Stock',
            stockQuantity: 60,
            rating: 4.7,
            organic: true,
            description: 'Thick, crisp organic celery stalks, ideal for juicing and cooking.'
        },
        {
            id: '4',
            name: 'Purple Cabbage',
            category: 'leafy-greens',
            categoryName: 'Leafy Greens',
            price: 1.30,
            unit: 'kg',
            image: 'img/purple-cabbage.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-05',
            stockStatus: 'In Stock',
            stockQuantity: 90,
            rating: 4.8,
            organic: true,
            description: 'Crunchy purple cabbage heads packed with vitamins and antioxidants.'
        },
        {
            id: '5',
            name: 'Straw Mushroom',
            category: 'leafy-greens',
            categoryName: 'Leafy Greens',
            price: 3.80,
            unit: 'kg',
            image: 'img/mushroom.jpg',
            farmOrigin: 'Mekong Orchard, Can Tho',
            harvestDate: '2026-08-06',
            stockStatus: 'In Stock',
            stockQuantity: 45,
            rating: 4.9,
            organic: true,
            description: 'Freshly picked organic straw mushrooms from humid Delta farm houses.'
        },
        {
            id: '6',
            name: 'Baby Carrot',
            category: 'root-veggies',
            categoryName: 'Root Vegetables',
            price: 1.10,
            unit: 'kg',
            image: 'img/carrot.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-03',
            stockStatus: 'In Stock',
            stockQuantity: 110,
            rating: 4.7,
            organic: false,
            description: 'Naturally sweet and crunchy Dalat baby carrots.'
        },
        {
            id: '7',
            name: 'Golden Potato',
            category: 'root-veggies',
            categoryName: 'Root Vegetables',
            price: 0.90,
            unit: 'kg',
            image: 'img/potato.jpg',
            farmOrigin: 'Sunrise Organic Farm, Hanoi',
            harvestDate: '2026-08-02',
            stockStatus: 'In Stock',
            stockQuantity: 150,
            rating: 4.6,
            organic: false,
            description: 'Soft golden potatoes perfect for stews, soups, and roasting.'
        },
        {
            id: '8',
            name: 'Honey Sweet Potato',
            category: 'root-veggies',
            categoryName: 'Root Vegetables',
            price: 1.50,
            unit: 'kg',
            image: 'img/sweet-potato.jpg',
            farmOrigin: 'Mekong Orchard, Can Tho',
            harvestDate: '2026-08-04',
            stockStatus: 'In Stock',
            stockQuantity: 70,
            rating: 4.9,
            organic: true,
            description: 'Rich, honey-infused sweet potatoes grown in fertile Mekong soil.'
        },
        {
            id: '9',
            name: 'Cherry Tomato',
            category: 'root-veggies',
            categoryName: 'Root Vegetables',
            price: 2.00,
            unit: 'kg',
            image: 'img/cherry-tomato.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-05',
            stockStatus: 'In Stock',
            stockQuantity: 65,
            rating: 4.8,
            organic: true,
            description: 'Juicy, bite-sized cherry tomatoes with vibrant red color.'
        },
        {
            id: '10',
            name: 'Ri6 Durian',
            category: 'tropical-fruit',
            categoryName: 'Tropical Fruit',
            price: 6.50,
            unit: 'kg',
            image: 'img/durian.jpg',
            farmOrigin: 'Mekong Orchard, Can Tho',
            harvestDate: '2026-08-06',
            stockStatus: 'In Stock',
            stockQuantity: 40,
            rating: 5.0,
            organic: false,
            description: 'Creamy Ri6 durian with small seeds and rich golden flesh.'
        },
        {
            id: '11',
            name: '034 Avocado',
            category: 'tropical-fruit',
            categoryName: 'Tropical Fruit',
            price: 2.80,
            unit: 'kg',
            image: 'img/avocado.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-05',
            stockStatus: 'In Stock',
            stockQuantity: 55,
            rating: 4.9,
            organic: true,
            description: 'Rich and buttery 034 Dalat avocado with thin skin.'
        },
        {
            id: '12',
            name: 'Laba Banana',
            category: 'tropical-fruit',
            categoryName: 'Tropical Fruit',
            price: 1.10,
            unit: 'bunch',
            image: 'img/banana.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-04',
            stockStatus: 'In Stock',
            stockQuantity: 80,
            rating: 4.8,
            organic: true,
            description: 'Sweet, fragrant Dalat Laba bananas harvested at ideal ripeness.'
        },
        {
            id: '13',
            name: 'Royal Cantaloupe',
            category: 'tropical-fruit',
            categoryName: 'Tropical Fruit',
            price: 3.50,
            unit: 'each',
            image: 'img/cantaloupe.jpg',
            farmOrigin: 'Sunrise Organic Farm, Hanoi',
            harvestDate: '2026-08-03',
            stockStatus: 'In Stock',
            stockQuantity: 30,
            rating: 4.7,
            organic: true,
            description: 'Orange-fleshed cantaloupe melon with rich natural sweetness.'
        },
        {
            id: '14',
            name: 'New Zealand Strawberry',
            category: 'seasonal-fruit',
            categoryName: 'Seasonal Fruit',
            price: 10.50,
            unit: 'kg',
            image: 'img/strawberry.jpg',
            farmOrigin: 'Green Valley Farm, Dalat',
            harvestDate: '2026-08-06',
            stockStatus: 'In Stock',
            stockQuantity: 25,
            rating: 5.0,
            organic: true,
            description: 'Large, mildly sweet New Zealand variety strawberries grown in Dalat.'
        },
        {
            id: '15',
            name: 'Vinh Long Orange',
            category: 'seasonal-fruit',
            categoryName: 'Seasonal Fruit',
            price: 1.50,
            unit: 'kg',
            image: 'img/orange.jpg',
            farmOrigin: 'Mekong Orchard, Can Tho',
            harvestDate: '2026-08-05',
            stockStatus: 'In Stock',
            stockQuantity: 95,
            rating: 4.8,
            organic: false,
            description: 'Juicy Vinh Long king oranges, perfect for fresh morning juice.'
        },
        {
            id: '16',
            name: 'Ha Giang Rock Apple',
            category: 'seasonal-fruit',
            categoryName: 'Seasonal Fruit',
            price: 1.90,
            unit: 'kg',
            image: 'img/apple.jpg',
            farmOrigin: 'Sunrise Organic Farm, Hanoi',
            harvestDate: '2026-08-04',
            stockStatus: 'In Stock',
            stockQuantity: 75,
            rating: 4.9,
            organic: true,
            description: 'Crisp, sweet highland rock apples with deep red skin.'
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
            const products = loadStorage(STORAGE_KEYS.PRODUCTS, DEFAULT_PRODUCTS);
            let modified = false;
            const seedMap = {};
            DEFAULT_PRODUCTS.forEach(dp => { seedMap[String(dp.id)] = dp; seedMap[dp.name] = dp; });

            products.forEach(p => {
                const numPrice = parseFloat(p.price);
                if (isNaN(numPrice) || numPrice <= 0) {
                    const seed = seedMap[String(p.id)] || seedMap[p.name];
                    p.price = (seed && seed.price > 0) ? seed.price : 1.50;
                    modified = true;
                }
            });

            if (modified) {
                saveStorage(STORAGE_KEYS.PRODUCTS, products);
            }
            return products;
        },
        getProductById(id) {
            const products = this.getProducts();
            return products.find(p => String(p.id) === String(id)) || null;
        },
        saveProduct(productData) {
            const products = this.getProducts();
            const existingIdx = products.findIndex(p => String(p.id) === String(productData.id));
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
            products = products.filter(p => String(p.id) !== String(id));
            saveStorage(STORAGE_KEYS.PRODUCTS, products);
            return products;
        },
        async fetchProductsFromBackend() {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);
            try {
                const response = await fetch(`${API_BASE_URL}/Products?pageSize=50`, {
                    signal: controller.signal
                });
                clearTimeout(timeoutId);
                if (!response.ok) return this.getProducts();

                const resData = await response.json();
                const items = resData.items || resData.Items || (Array.isArray(resData) ? resData : []);

                if (items && items.length > 0) {
                    const categoryMap = {
                        'LeafyGreens': 'leafy-greens', 'RootVeggies': 'root-veggies', 
                        'TropicalFruit': 'tropical-fruit', 'SeasonalFruit': 'seasonal-fruit',
                        '0': 'leafy-greens', '1': 'root-veggies', '2': 'tropical-fruit', '3': 'seasonal-fruit'
                    };
                    
                    // 🔴 LẤY DỮ LIỆU LOCAL HIỆN TẠI ĐỂ ĐỐI CHIẾU
                    const localProducts = loadStorage(STORAGE_KEYS.PRODUCTS, []);
                    const localMap = {};
                    localProducts.forEach(p => localMap[String(p.id)] = p);

                    const syncedProducts = items.map((p, idx) => {
                        const pid = String(p.id !== undefined ? p.id : (p.Id !== undefined ? p.Id : (idx + 1)));
                        const pName = p.name || p.Name || 'Produce';
                        
                        // Đối chiếu với data Local xem Admin có đang đè trạng thái Stock không
                        const localItem = localMap[pid] || {};

                        let rawPrice = p.price !== undefined && p.price !== null ? p.price : p.Price;
                        let numPrice = parseFloat(rawPrice) || localItem.price || 1.50;

                        let rawStock = p.stockQty !== undefined && p.stockQty !== null ? p.stockQty : p.StockQty;
                        let stockVal = parseInt(rawStock, 10);
                        if (isNaN(stockVal)) stockVal = 50;

                        return {
                            id: pid,
                            name: pName,
                            category: categoryMap[p.category] || categoryMap[p.Category] || localItem.category || 'leafy-greens',
                            categoryName: localItem.categoryName || 'Produce',
                            price: numPrice,
                            unit: p.unit || p.Unit || localItem.unit || 'kg',
                            image: (function() {
                                let img = p.imageUrl || p.ImageUrl || localItem.image || 'img/vegetable-item-2.jpg';
                                return img.startsWith('/uploads') ? API_BASE_URL.replace('/api', '') + img : img.replace(/^\//, '');
                            })(),
                            farmOrigin: p.farmOrigin || p.FarmOrigin || localItem.farmOrigin || 'Green Valley Farm, Dalat',
                            harvestDate: p.harvestDate ? String(p.harvestDate).substring(0, 10) : '2026-08-05',
                            
                            // 🔴 ĐÂY LÀ CHÌA KHÓA: Ưu tiên lấy trạng thái Stock của LocalStorage nếu đã bị sửa
                            stockStatus: localItem.stockStatus || ((stockVal > 0) ? 'In Stock' : 'Out of Stock'),
                            stockQuantity: localItem.stockQuantity !== undefined ? localItem.stockQuantity : stockVal,
                            
                            rating: localItem.rating || 4.8,
                            organic: p.organic !== undefined ? !!p.organic : (p.Organic !== undefined ? !!p.Organic : true),
                            description: p.description || p.Description || localItem.description || ''
                        };
                    });

                    saveStorage(STORAGE_KEYS.PRODUCTS, syncedProducts);
                    if (window.renderAdminDashboard) window.renderAdminDashboard();
                    return syncedProducts;
                }
            } catch (err) {
                clearTimeout(timeoutId);
            }
            return this.getProducts();
        },
        async uploadProductImageAsync(file) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) return null;

            const formData = new FormData();
            formData.append('file', file);

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 5000);

            try {
                const response = await fetch(`${API_BASE_URL}/admin/uploads/image`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`
                        // KHÔNG set 'Content-Type' — để browser tự sinh multipart boundary
                    },
                    body: formData,
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) {
                    console.error('Image upload failed with status', response.status);
                    return null;
                }
                const data = await response.json();
                return data.url || null;
            } catch (err) {
                clearTimeout(timeoutId);
                console.error('Image upload failed', err);
                return null;
            }
        },
        getFarms() {
            const DEFAULT_FARMS = [
                { id: 1, name: 'Green Valley Farm, Dalat', location: 'Dalat, Lam Dong', contactInfo: '+84 901 234 567' },
                { id: 2, name: 'Sunrise Organic Farm, Hanoi', location: 'Dong Anh, Hanoi', contactInfo: '+84 912 345 678' },
                { id: 3, name: 'Mekong Organic Cooperative', location: 'Can Tho, Mekong Delta', contactInfo: '+84 903 456 789' },
                { id: 4, name: 'Dalat Organic Farm', location: 'Dalat, Lam Dong', contactInfo: '+84 908 765 432' }
            ];
            return loadStorage(STORAGE_KEYS.FARMS, DEFAULT_FARMS);
        },
        saveFarmLocal(farmData) {
            let farms = this.getFarms();
            if (farmData.id) {
                const idx = farms.findIndex(f => f.id == farmData.id);
                if (idx >= 0) farms[idx] = { ...farms[idx], ...farmData };
            } else {
                farmData.id = Date.now();
                farms.push(farmData);
            }
            saveStorage(STORAGE_KEYS.FARMS, farms);
            return farmData;
        },
        deleteFarmLocal(id) {
            let farms = this.getFarms();
            farms = farms.filter(f => f.id != id);
            saveStorage(STORAGE_KEYS.FARMS, farms);
            return true;
        },
        async loadFarmsAsync() {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) return this.getFarms();

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/admin/farms`, {
                    headers: { 'Authorization': `Bearer ${token}` },
                    signal: controller.signal
                });
                clearTimeout(timeoutId);
                if (!response.ok) return this.getFarms();
                const resData = await response.json();
                if (Array.isArray(resData)) {
                    saveStorage(STORAGE_KEYS.FARMS, resData);
                    return resData;
                }
                return this.getFarms();
            } catch (err) {
                clearTimeout(timeoutId);
                return this.getFarms();
            }
        },
        async createFarmAsync(farmData) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) return this.saveFarmLocal(farmData);

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/admin/farms`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        name: farmData.name,
                        location: farmData.location,
                        contactInfo: farmData.contactInfo || ''
                    }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) {
                    const errData = await response.json().catch(() => null);
                    const msg = (errData && errData.message) ? errData.message : 'Failed to create farm.';
                    throw new Error(msg);
                }

                const createdFarm = await response.json();
                await this.loadFarmsAsync();
                return createdFarm;
            } catch (err) {
                clearTimeout(timeoutId);
                console.warn('API create farm notice:', err.message);
                if (err.name === 'AbortError' || err.name === 'TypeError' || err.message.includes('fetch')) {
                    return this.saveFarmLocal(farmData);
                }
                throw err;
            }
        },
        async updateFarmAsync(id, farmData) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token || isNaN(parseInt(id, 10))) return this.saveFarmLocal({ ...farmData, id: id });

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/admin/farms/${id}`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        name: farmData.name,
                        location: farmData.location,
                        contactInfo: farmData.contactInfo || ''
                    }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) {
                    const errData = await response.json().catch(() => null);
                    const msg = (errData && errData.message) ? errData.message : 'Failed to update farm.';
                    throw new Error(msg);
                }

                await this.loadFarmsAsync();
                return farmData;
            } catch (err) {
                clearTimeout(timeoutId);
                console.warn('API update farm notice:', err.message);
                if (err.name === 'AbortError' || err.name === 'TypeError' || err.message.includes('fetch')) {
                    return this.saveFarmLocal({ ...farmData, id: id });
                }
                throw err;
            }
        },
        async deleteFarmAsync(id) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token || isNaN(parseInt(id, 10))) return this.deleteFarmLocal(id);

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/admin/farms/${id}`, {
                    method: 'DELETE',
                    headers: { 'Authorization': `Bearer ${token}` },
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) {
                    const errData = await response.json().catch(() => null);
                    const msg = (errData && errData.message) ? errData.message : (errData && errData.Message ? errData.Message : 'Cannot delete farm.');
                    throw new Error(msg);
                }

                await this.loadFarmsAsync();
                return true;
            } catch (err) {
                clearTimeout(timeoutId);
                if (err.message && err.message.includes('batch history')) {
                    throw err; // Re-throw Conflict error for UI toast
                }
                return this.deleteFarmLocal(id);
            }
        },
        async createProductAsync(productData) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) return this.saveProduct(productData);

            const categoryToBackendMap = {
                'leafy-greens': 0,
                'root-veggies': 1,
                'tropical-fruit': 2,
                'seasonal-fruit': 3
            };

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                // Bước 1: tạo Product (chỉ chứa thông tin catalog, không có farm/stock)
                const response = await fetch(`${API_BASE_URL}/admin/products`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        name: productData.name,
                        category: categoryToBackendMap[productData.category] ?? 0,
                        description: productData.description || '',
                        unit: productData.unit || 'kg',
                        price: parseFloat(productData.price) || 0,
                        imageUrl: productData.image || 'img/vegetable-item-2.jpg',
                        organic: !!productData.organic
                    }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) return this.saveProduct(productData);
                const newProduct = await response.json();

                // Bước 2: nhập lô hàng đầu tiên (farm + harvest date + số lượng)
                // nếu admin có chọn farm và nhập số lượng trên form
              
                // SỬA LỖI Ở ĐÂY: Lấy ID chuẩn xác dù Backend trả về chữ thường (id) hay chữ hoa (Id)
                const productId = newProduct.id !== undefined ? newProduct.id : newProduct.Id;

                // Bước 2: nhập lô hàng đầu tiên (farm + harvest date + số lượng)
                if (productData.farmId && productData.stockQuantity && productId) {
                    const batchController = new AbortController();
                    const batchTimeoutId = setTimeout(() => batchController.abort(), 2000);
                    try {
                        // Gọi đúng ID của sản phẩm để thêm lô hàng
                        await fetch(`${API_BASE_URL}/admin/products/${productId}/batches`, {
                            method: 'POST',
                            headers: {
                                'Authorization': `Bearer ${token}`,
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                                farmId: parseInt(productData.farmId, 10),
                                harvestDate: productData.harvestDate,
                                quantity: parseInt(productData.stockQuantity, 10),
                                costPrice: (productData.costPrice != null && productData.costPrice !== '')
                                    ? parseFloat(productData.costPrice)
                                    : (parseFloat(productData.price) || 0) * 0.6
                            }),
                            signal: batchController.signal
                        });
                        clearTimeout(batchTimeoutId);
                    } catch (batchErr) {
                        clearTimeout(batchTimeoutId);
                        console.error('Product created but failed to add initial batch:', batchErr);
                    }
                }

                this.fetchProductsFromBackend();
                return newProduct;
            } catch (err) {
                clearTimeout(timeoutId);
                return this.saveProduct(productData);
            }
        },

        // --- Shopping Cart (FR-3.1) ---
        getCartKey() {
            const authUser = this.getAuthUser();
            if (authUser && (authUser.email || authUser.id)) {
                const identifier = String(authUser.email || authUser.id).toLowerCase().trim().replace(/[^a-z0-9]/g, '_');
                return `gb_cart_user_${identifier}`;
            }
            return 'gb_cart_guest';
        },
        getCart() {
            const key = this.getCartKey();
            let cart = loadStorage(key, null);

            // Migration from legacy global cart for guest or first time user
            if (cart === null) {
                cart = loadStorage(STORAGE_KEYS.CART, []);
                saveStorage(key, cart);
            }

            let modified = false;
            const products = this.getProducts();
            const seedMap = {};
            DEFAULT_PRODUCTS.forEach(dp => { seedMap[String(dp.id)] = dp; seedMap[dp.name] = dp; });

            cart.forEach(item => {
                let numPrice = parseFloat(item.price);
                if (isNaN(numPrice) || numPrice <= 0) {
                    const prod = products.find(p => String(p.id) === String(item.productId));
                    const seed = seedMap[String(item.productId)] || seedMap[item.name];
                    if (prod && parseFloat(prod.price) > 0) {
                        item.price = parseFloat(prod.price);
                        modified = true;
                    } else if (seed && seed.price > 0) {
                        item.price = seed.price;
                        modified = true;
                    } else {
                        item.price = 1.50;
                        modified = true;
                    }
                }
                if (!item.name || !item.image || !item.unit) {
                    const prod = products.find(p => String(p.id) === String(item.productId));
                    const seed = seedMap[String(item.productId)];
                    const ref = prod || seed;
                    if (ref) {
                        if (!item.name) item.name = ref.name;
                        if (!item.image) item.image = ref.image;
                        if (!item.unit) item.unit = ref.unit;
                        modified = true;
                    }
                }
            });

            if (modified) {
                saveStorage(key, cart);
            }
            return cart;
        },
        addToCart(productId, qty = 1) {
            const product = this.getProductById(productId);
            if (!product) return false;
            
            let cart = this.getCart();
            const existingItem = cart.find(item => String(item.productId) === String(productId));
            const numQty = parseInt(qty, 10) || 1;
            const numPrice = parseFloat(product.price) || 0;

            if (existingItem) {
                existingItem.qty = (parseInt(existingItem.qty, 10) || 0) + numQty;
                existingItem.price = numPrice;
                existingItem.name = product.name;
                existingItem.image = product.image;
                existingItem.unit = product.unit;
            } else {
                cart.push({
                    productId: product.id,
                    name: product.name,
                    price: numPrice,
                    unit: product.unit || 'kg',
                    image: product.image || 'img/vegetable-item-2.jpg',
                    farmOrigin: product.farmOrigin || 'Certified Farm',
                    qty: numQty
                });
            }
            saveStorage(this.getCartKey(), cart);
            return true;
        },
        updateCartQty(productId, qty) {
            let cart = this.getCart();
            const item = cart.find(i => String(i.productId) === String(productId));
            if (item) {
                item.qty = Math.max(1, parseInt(qty, 10) || 1);
                saveStorage(this.getCartKey(), cart);
            }
        },
        removeFromCart(productId) {
            let cart = this.getCart();
            cart = cart.filter(i => String(i.productId) !== String(productId));
            saveStorage(this.getCartKey(), cart);
        },
        clearCart() {
            saveStorage(this.getCartKey(), []);
        },
        getCartCount() {
            const cart = this.getCart();
            return cart.reduce((sum, item) => sum + (parseInt(item.qty, 10) || 0), 0);
        },
        getCartSubtotal() {
            const cart = this.getCart();
            return cart.reduce((sum, item) => {
                const price = parseFloat(item.price) || 0;
                const qty = parseInt(item.qty, 10) || 0;
                return sum + (price * qty);
            }, 0);
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
        getOrdersKey() {
            const authUser = this.getAuthUser();
            if (authUser && (authUser.email || authUser.id)) {
                const identifier = String(authUser.email || authUser.id).toLowerCase().trim().replace(/[^a-z0-9]/g, '_');
                return `gb_orders_user_${identifier}`;
            }
            return 'gb_orders_guest';
        },
        getOrders() {
            const key = this.getOrdersKey();
            let orders = loadStorage(key, null);

            if (orders === null) {
                const allOrders = loadStorage(STORAGE_KEYS.ORDERS, DEFAULT_ORDERS);
                const authUser = this.getAuthUser();
                if (authUser && authUser.email) {
                    orders = allOrders.filter(o => o.email && o.email.toLowerCase() === authUser.email.toLowerCase());
                } else {
                    orders = allOrders;
                }
                saveStorage(key, orders);
            }
            return orders;
        },
        async getOrdersAsync() {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            const authUser = this.getAuthUser();
            if (!token || !authUser) return this.getOrders();

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 3000);

            try {
                const userId = authUser.id || authUser.email;
                const response = await fetch(`${API_BASE_URL}/Order/my-orders?userId=${encodeURIComponent(userId)}`, {
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) return this.getOrders();
                const resData = await response.json();
                if (resData && resData.isSuccess && Array.isArray(resData.data)) {
                    const products = this.getProducts();
                    const prodMap = {};
                    products.forEach(p => prodMap[String(p.id)] = p);

                    const apiOrders = resData.data.map(o => {
                        const items = (o.items || []).map(i => {
                            const prod = prodMap[String(i.productId)] || {};
                            return {
                                id: String(i.productId),
                                productId: i.productId,
                                name: prod.name || `Produce #${i.productId}`,
                                price: i.unitPrice || prod.price || 1.50,
                                qty: i.quantity || 1,
                                unit: prod.unit || 'kg',
                                image: prod.image || 'img/vegetable-item-2.jpg'
                            };
                        });
                        const subtotal = items.reduce((s, it) => s + (it.price * it.qty), 0);
                        return {
                            id: 'ORD-' + o.id,
                            date: new Date().toISOString().replace('T', ' ').substring(0, 16),
                            customerName: authUser.name || 'Customer',
                            email: authUser.email || '',
                            phone: authUser.phone || '',
                            deliveryAddress: o.deliveryAddress || 'Saved Delivery Location',
                            deliverySlot: 'Standard Window',
                            paymentMethod: 'Credit Card / Online',
                            items: items,
                            subtotal: subtotal,
                            deliveryFee: subtotal >= 30 ? 0.00 : 2.00,
                            total: subtotal + (subtotal >= 30 ? 0.00 : 2.00),
                            status: o.status || 'Processing',
                            qualityReport: null
                        };
                    });

                    if (apiOrders.length > 0) {
                        saveStorage(this.getOrdersKey(), apiOrders);
                        return apiOrders;
                    }
                }
                return this.getOrders();
            } catch (err) {
                clearTimeout(timeoutId);
                return this.getOrders();
            }
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
            saveStorage(this.getOrdersKey(), orders);
            this.clearCart();
            return newOrder;
        },
        async placeOrderAsync(checkoutDetails) {
            const localOrder = this.placeOrder(checkoutDetails);
            const authUser = this.getAuthUser();
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);

            if (token && authUser && localOrder) {
                try {
                    const controller = new AbortController();
                    const timeoutId = setTimeout(() => controller.abort(), 4000);

                    const dto = {
                        userId: authUser.id || authUser.email,
                        items: (localOrder.items || []).map(item => ({
                            productId: parseInt(item.productId, 10) || 1,
                            quantity: item.qty
                        })),
                        deliveryAddress: checkoutDetails.address || '',
                        deliverySlotId: 1
                    };

                    await fetch(`${API_BASE_URL}/Order/create`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`
                        },
                        body: JSON.stringify(dto),
                        signal: controller.signal
                    });
                    clearTimeout(timeoutId);
                } catch (err) {
                    // Fallback to local storage order
                }
            }
            return localOrder;
        },
        updateOrderStatus(orderId, newStatus) {
            const orders = this.getOrders();
            const targetId = String(orderId).replace(/^ORD-/, '');
            const order = orders.find(o => String(o.id).replace(/^ORD-/, '') === targetId);
            if (order) {
                order.status = newStatus;
                saveStorage(this.getOrdersKey(), orders);
                return true;
            }
            return false;
        },
        cancelOrder(orderId) {
            const orders = this.getOrders();
            const targetId = String(orderId).replace(/^ORD-/, '');
            const order = orders.find(o => String(o.id).replace(/^ORD-/, '') === targetId);
            if (order && (order.status === 'Processing' || order.status === 'Pending' || order.status === 'Out for Delivery')) {
                order.status = 'Cancelled';
                saveStorage(this.getOrdersKey(), orders);
                return true;
            }
            return false;
        },
        async cancelOrderAsync(orderId) {
            const result = this.cancelOrder(orderId);
            const authUser = this.getAuthUser();
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);

            if (token && authUser) {
                try {
                    const rawId = String(orderId).replace(/^ORD-/, '');
                    const userId = authUser.id || authUser.email;
                    const controller = new AbortController();
                    const timeoutId = setTimeout(() => controller.abort(), 3000);

                    await fetch(`${API_BASE_URL}/Order/cancel/${rawId}?userId=${encodeURIComponent(userId)}`, {
                        method: 'POST',
                        headers: {
                            'Authorization': `Bearer ${token}`,
                            'Content-Type': 'application/json'
                        },
                        signal: controller.signal
                    });
                    clearTimeout(timeoutId);
                } catch (err) {
                    // Local fallback already completed
                }
            }
            return result;
        },

        // --- Quality Issue Reports (FR-5.3, FR-6.3) ---
        submitQualityReport(orderId, issueType, comments) {
            const orders = this.getOrders();
            const targetId = String(orderId).replace(/^ORD-/, '');
            const order = orders.find(o => String(o.id).replace(/^ORD-/, '') === targetId);
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
            saveStorage(this.getOrdersKey(), orders);
            return ticket;
        },
        resolveQualityReport(orderId, resolutionStatus) {
            const orders = this.getOrders();
            const targetId = String(orderId).replace(/^ORD-/, '');
            const order = orders.find(o => String(o.id).replace(/^ORD-/, '') === targetId);
            if (order && order.qualityReport) {
                order.qualityReport.status = resolutionStatus;
                saveStorage(this.getOrdersKey(), orders);
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
        getApiUrl() {
            return API_BASE_URL;
        },
        getJwtToken() {
            return loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
        },
        getAuthUser() {
            return loadStorage(STORAGE_KEYS.AUTH_USER, null);
        },
        isLoggedIn() {
            return !!this.getAuthUser();
        },
        parseJwt(token) {
            try {
                if (!token) return null;
                const base64Url = token.split('.')[1];
                const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
                    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                }).join(''));
                return JSON.parse(jsonPayload);
            } catch (e) {
                console.error('Error decoding JWT token:', e);
                return null;
            }
        },
        extractRoleFromToken(token) {
            const payload = this.parseJwt(token);
            if (!payload) return 'Customer';
            const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["role"];
            if (role === 'Admin' || role === 'Staff' || (Array.isArray(role) && (role.includes('Admin') || role.includes('Staff')))) {
                return 'Staff / Admin';
            }
            return 'Customer';
        },

        // Async Login API Integration with 2s Timeout & Clean Exception Handling
        async loginUserAsync(email, password) {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/Auth/login`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, password }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                const resData = await response.json().catch(() => null);

                if (!response.ok || (resData && resData.isSuccess === false)) {
                    let errMsg = (resData && resData.message) ? resData.message : 'Incorrect password or email!';
                    if (resData && resData.errors) {
                        const errList = Object.values(resData.errors).flat().join(', ');
                        if (errList) errMsg = errList;
                    }
                    throw new Error(errMsg);
                }

                const data = resData && resData.data ? resData.data : resData;
                const token = data.token || data.Token;
                const role = this.extractRoleFromToken(token);

                const user = {
                    name: data.fullName || data.FullName || email.split('@')[0],
                    email: data.email || data.Email || email,
                    role: role,
                    loginTime: new Date().toISOString()
                };

                saveStorage(STORAGE_KEYS.JWT_TOKEN, token);
                this.setUserRole(user.role);
                saveStorage(STORAGE_KEYS.AUTH_USER, user);
                return user;
            } catch (err) {
                clearTimeout(timeoutId);
                console.warn('API Login Notice:', err.message);
                if (err.name === 'AbortError' || err.name === 'TypeError' || err.message.includes('fetch') || err.message.includes('NetworkError') || err.message.includes('Failed to fetch') || err.message.includes('aborted')) {
                    console.info('Backend API server offline/unreachable. Falling back to local authentication...');
                    return this.loginUser(email, password);
                }
                throw err;
            }
        },

        async updateProductAsync(productData) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            
            // Lưu tạm giao diện ngay lập tức
            this.saveProduct(productData);

            if (!token || isNaN(parseInt(productData.id, 10))) return productData;

            const categoryToBackendMap = {
                'leafy-greens': 0, 'root-veggies': 1, 'tropical-fruit': 2, 'seasonal-fruit': 3
            };

            try {
                // 1. Chỉ gửi ĐÚNG các trường cơ bản giống lúc Create (Tránh lỗi 400 từ C#)
                await fetch(`${API_BASE_URL}/admin/products/${productData.id}`, {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        name: productData.name,
                        category: categoryToBackendMap[productData.category] ?? 0,
                        description: productData.description || '',
                        unit: productData.unit || 'kg',
                        price: parseFloat(productData.price) || 0,
                        imageUrl: productData.image || 'img/vegetable-item-2.jpg',
                        organic: !!productData.organic
                    })
                });

                // 2. Cập nhật lô hàng nếu có nhập số lượng
                if (productData.farmId && productData.stockQuantity > 0) {
                    try {
                        await fetch(`${API_BASE_URL}/admin/products/${productData.id}/batches`, {
                            method: 'POST',
                            headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
                            body: JSON.stringify({
                                farmId: parseInt(productData.farmId, 10),
                                harvestDate: productData.harvestDate,
                                quantity: parseInt(productData.stockQuantity, 10),
                                costPrice: parseFloat(productData.costPrice) || (parseFloat(productData.price) * 0.6)
                            })
                        });
                    } catch (e) {}
                }

                // 🔴 Đã gỡ bỏ hàm this.fetchProductsFromBackend() ở đây để tránh bị đè data sau 0.5s
                return productData;
            } catch (err) {
                return productData;
            }
        },

        // Async Registration API Integration with 2s Timeout
        async registerUserAsync(fullName, email, password) {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/Auth/register`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ fullName, email, password }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                const resData = await response.json().catch(() => null);

                if (!response.ok || (resData && resData.isSuccess === false)) {
                    let errMsg = (resData && resData.message) ? resData.message : 'Registration failed.';
                    if (resData && resData.errors) {
                        const errList = Object.values(resData.errors).flat().join(', ');
                        if (errList) errMsg = errList;
                    }
                    throw new Error(errMsg);
                }

                const data = resData && resData.data ? resData.data : resData;
                const token = data.token || data.Token;
                const role = token ? this.extractRoleFromToken(token) : 'Customer';

                const user = {
                    name: data.fullName || data.FullName || fullName,
                    email: data.email || data.Email || email,
                    role: role,
                    loginTime: new Date().toISOString()
                };

                if (token) {
                    saveStorage(STORAGE_KEYS.JWT_TOKEN, token);
                }
                this.setUserRole(user.role);
                saveStorage(STORAGE_KEYS.AUTH_USER, user);
                return user;
            } catch (err) {
                clearTimeout(timeoutId);
                console.warn('API Register Notice:', err.message);
                if (err.name === 'AbortError' || err.name === 'TypeError' || err.message.includes('fetch') || err.message.includes('NetworkError') || err.message.includes('Failed to fetch') || err.message.includes('aborted')) {
                    console.info('Backend API server offline/unreachable. Falling back to local registration...');
                    return this.registerUser(fullName, email, password);
                }
                throw err;
            }
        },

        // Synchronous fallback methods with strict password verification
        registerUser(fullName, email, password) {
            const users = loadStorage(STORAGE_KEYS.USERS_DB, DEFAULT_USERS_DB);
            const existingIdx = users.findIndex(u => String(u.email).toLowerCase().trim() === String(email).toLowerCase().trim());
            const isStaff = email.includes('staff') || email.includes('admin');
            const newUser = {
                name: fullName || email.split('@')[0],
                email: email,
                password: password,
                role: isStaff ? 'Staff / Admin' : 'Customer',
                loginTime: new Date().toISOString()
            };
            if (existingIdx >= 0) {
                users[existingIdx] = newUser;
            } else {
                users.push(newUser);
            }
            saveStorage(STORAGE_KEYS.USERS_DB, users);
            this.setUserRole(newUser.role);
            saveStorage(STORAGE_KEYS.AUTH_USER, newUser);
            return newUser;
        },
        loginUser(email, password) {
            const users = loadStorage(STORAGE_KEYS.USERS_DB, DEFAULT_USERS_DB);
            const existing = users.find(u => String(u.email).toLowerCase().trim() === String(email).toLowerCase().trim());

            if (existing) {
                if (existing.password && existing.password !== password) {
                    throw new Error('Incorrect password. Please try again!');
                }
                const user = {
                    name: existing.name || email.split('@')[0],
                    email: existing.email,
                    role: existing.role || (email.includes('admin') || email.includes('staff') ? 'Staff / Admin' : 'Customer'),
                    loginTime: new Date().toISOString()
                };
                this.setUserRole(user.role);
                saveStorage(STORAGE_KEYS.AUTH_USER, user);
                return user;
            }

            // If account was created via form or default admin/demo
            if (password !== 'Admin@12345' && password !== 'password123' && password !== 'demo123') {
                throw new Error('Incorrect password. Please try again!');
            }

            const isStaff = email.includes('staff') || email.includes('admin');
            const newUser = {
                name: email.split('@')[0].replace('.', ' '),
                email: email,
                password: password,
                role: isStaff ? 'Staff / Admin' : 'Customer',
                loginTime: new Date().toISOString()
            };
            users.push(newUser);
            saveStorage(STORAGE_KEYS.USERS_DB, users);
            this.setUserRole(newUser.role);
            saveStorage(STORAGE_KEYS.AUTH_USER, newUser);
            return newUser;
        },
        updateUserProfile(profileData) {
            let user = this.getAuthUser() || {};
            user = {
                ...user,
                name: profileData.fullName || profileData.name || user.name || 'Customer',
                phone: profileData.phone || user.phone || '',
                email: profileData.email || user.email || '',
                address: profileData.address || user.address || ''
            };
            saveStorage(STORAGE_KEYS.AUTH_USER, user);
            return user;
        },

        // =========================================================
        // Multi-Address Management Methods (FR-1.2 Address Selector)
        // =========================================================
        getAddressKey() {
            const authUser = this.getAuthUser();
            if (authUser && (authUser.email || authUser.id)) {
                const identifier = String(authUser.email || authUser.id).toLowerCase().trim().replace(/[^a-z0-9]/g, '_');
                return `gb_addresses_user_${identifier}`;
            }
            return 'gb_addresses_guest';
        },
        getUserAddresses() {
            const key = this.getAddressKey();
            let addresses = JSON.parse(localStorage.getItem(key) || 'null');
            
            if (!addresses || !Array.isArray(addresses)) {
                const authUser = this.getAuthUser() || {};
                addresses = [
                    {
                        id: 1,
                        receiverName: authUser.name || 'Alex Johnson',
                        phoneNumber: authUser.phone || '+84 901 234 567',
                        email: authUser.email || '',
                        streetAddress: authUser.address ? authUser.address.split(',')[0] : '123 High Street',
                        city: 'Ho Chi Minh City',
                        district: 'District 1',
                        ward: 'Ward Ben Nghe',
                        isDefault: true
                    }
                ];
                localStorage.setItem(key, JSON.stringify(addresses));
            }
            return addresses;
        },

        getAddressById(id) {
            const addresses = this.getUserAddresses();
            return addresses.find(a => a.id == id) || null;
        },

        addAddress(addressData) {
            const addresses = this.getUserAddresses();
            if (addressData.isDefault || addresses.length === 0) {
                addresses.forEach(a => a.isDefault = false);
                addressData.isDefault = true;
            }
            const newAddress = {
                id: Date.now(),
                receiverName: addressData.receiverName || addressData.fullName || 'Recipient',
                phoneNumber: addressData.phoneNumber || addressData.phone || '',
                email: addressData.email || '',
                streetAddress: addressData.streetAddress || addressData.address || '',
                city: addressData.city || 'Ho Chi Minh City',
                district: addressData.district || 'District 1',
                ward: addressData.ward || 'Ward Ben Nghe',
                isDefault: !!addressData.isDefault
            };
            addresses.unshift(newAddress);
            
            localStorage.setItem(this.getAddressKey(), JSON.stringify(addresses));

            if (newAddress.isDefault) {
                this.updateUserProfile({
                    fullName: newAddress.receiverName,
                    phone: newAddress.phoneNumber,
                    address: `${newAddress.streetAddress}, ${newAddress.ward}, ${newAddress.district}, ${newAddress.city}`
                });
            }
            
            window.dispatchEvent(new Event('addressDataChanged'));
            return newAddress;
        },

        setDefaultAddress(addressId) {
            const addresses = this.getUserAddresses();
            let selected = null;
            addresses.forEach(a => {
                if (a.id == addressId) {
                    a.isDefault = true;
                    selected = a;
                } else {
                    a.isDefault = false;
                }
            });
            
            localStorage.setItem(this.getAddressKey(), JSON.stringify(addresses));
            
            if (selected) {
                this.updateUserProfile({
                    fullName: selected.receiverName,
                    phone: selected.phoneNumber,
                    address: `${selected.streetAddress}, ${selected.ward}, ${selected.district}, ${selected.city}`
                });
            }
            
            window.dispatchEvent(new Event('addressDataChanged'));
            return selected;
        },

        deleteAddress(addressId) {
            const userKey = this.getAddressKey();
            let userAddresses = JSON.parse(localStorage.getItem(userKey) || 'null');
            if (!userAddresses || !Array.isArray(userAddresses)) {
                userAddresses = this.getUserAddresses();
            }
            userAddresses = userAddresses.filter(a => a.id != addressId);
            if (userAddresses.length > 0 && !userAddresses.some(a => a.isDefault)) {
                userAddresses[0].isDefault = true;
            }
            localStorage.setItem(userKey, JSON.stringify(userAddresses));

            let globalAddresses = JSON.parse(localStorage.getItem(STORAGE_KEYS.ADDRESSES) || 'null');
            if (Array.isArray(globalAddresses)) {
                globalAddresses = globalAddresses.filter(a => a.id != addressId);
                if (globalAddresses.length > 0 && !globalAddresses.some(a => a.isDefault)) {
                    globalAddresses[0].isDefault = true;
                }
                localStorage.setItem(STORAGE_KEYS.ADDRESSES, JSON.stringify(globalAddresses));
            }

            // Sync backend API delete if logged in
            const token = localStorage.getItem(STORAGE_KEYS.JWT_TOKEN);
            if (token && !isNaN(parseInt(addressId, 10))) {
                fetch(`${API_BASE_URL}/Address/${addressId}`, {
                    method: 'DELETE',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                }).catch(err => console.warn('Address API delete error:', err));
            }

            window.dispatchEvent(new Event('addressDataChanged'));

            if (typeof renderSavedAddresses === 'function') {
                renderSavedAddresses();
            }

            if (window.GB && window.GB.showToast) {
                window.GB.showToast('Address deleted successfully!', 'success');
            }

            return userAddresses;
        },

        // Async C# API Address Methods with 2s Timeout & Local Fallback
        async getUserAddressesAsync() {
            const token = localStorage.getItem(STORAGE_KEYS.JWT_TOKEN);
            if (!token) return this.getUserAddresses();

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/Address`, {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) return this.getUserAddresses();
                const resData = await response.json();
                if (resData && resData.isSuccess && Array.isArray(resData.data)) {
                    localStorage.setItem(STORAGE_KEYS.ADDRESSES, JSON.stringify(resData.data));
                    return resData.data;
                }
                return this.getUserAddresses();
            } catch (err) {
                clearTimeout(timeoutId);
                return this.getUserAddresses();
            }
        },

        async createAddressAsync(addressData) {
            const token = localStorage.getItem(STORAGE_KEYS.JWT_TOKEN);
            if (!token) return this.addAddress(addressData);

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/Address`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        receiverName: addressData.receiverName || addressData.fullName,
                        phoneNumber: addressData.phoneNumber || addressData.phone,
                        email: addressData.email || '',
                        streetAddress: addressData.streetAddress || addressData.address,
                        city: addressData.city || 'Ho Chi Minh City',
                        district: addressData.district || 'District 1',
                        ward: addressData.ward || 'Ward Ben Nghe',
                        isDefault: !!addressData.isDefault
                    }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) return this.addAddress(addressData);
                const resData = await response.json();
                if (resData && resData.isSuccess && resData.data) {
                    this.getUserAddressesAsync();
                    
                    window.dispatchEvent(new Event('addressDataChanged'));
                    return resData.data;
                }
                return this.addAddress(addressData);
            } catch (err) {
                clearTimeout(timeoutId);
                return this.addAddress(addressData);
            }
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
            saveStorage(STORAGE_KEYS.JWT_TOKEN, null);
            saveStorage(STORAGE_KEYS.AUTH_USER, null);
            window.dispatchEvent(new Event('addressDataChanged'));
            window.dispatchEvent(new CustomEvent('gb_state_change'));
            if (window.updateHeaderUI) window.updateHeaderUI();
            if (window.renderCartPage) window.renderCartPage();
            if (window.renderCheckoutSummary) window.renderCheckoutSummary();
            if (window.renderOrders) window.renderOrders();
            if (window.renderSavedAddresses) window.renderSavedAddresses();
        },
        clearAllUsersKeepAdmin() {
            const adminUser = {
                name: 'System Administrator',
                email: 'admin@greenbasket.com',
                role: 'Admin',
                loginTime: new Date().toISOString()
            };
            saveStorage(STORAGE_KEYS.AUTH_USER, adminUser);
            saveStorage(STORAGE_KEYS.USER_ROLE, 'Admin');

            Object.keys(localStorage).forEach(key => {
                if (key.startsWith('gb_cart_') || key === 'gb_cart_v1') {
                    localStorage.removeItem(key);
                }
            });

            if (window.updateHeaderUI) window.updateHeaderUI();
            if (window.renderCartPage) window.renderCartPage();
            if (window.renderCheckoutSummary) window.renderCheckoutSummary();
            return adminUser;
        },
        async assignRoleAsync(email, roleName) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) throw new Error("Authentication required. Please log in as Admin.");

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/Admin/assign-role`, {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        email: email,
                        roleName: roleName
                    }),
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                const data = await response.json().catch(() => null);

                if (!response.ok) {
                    const msg = (data && data.message) ? data.message : 'Failed to assign role.';
                    throw new Error(msg);
                }

                return data;
            } catch (err) {
                clearTimeout(timeoutId);
                console.warn('API assignRole notice:', err.message);
                throw err;
            }
        },

        async getProductBatchesAsync(productId) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) throw new Error("Authentication required.");

            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 2000);

            try {
                const response = await fetch(`${API_BASE_URL}/admin/products/${productId}/batches`, {
                    headers: { 'Authorization': `Bearer ${token}` },
                    signal: controller.signal
                });
                clearTimeout(timeoutId);

                if (!response.ok) {
                    throw new Error('Failed to load batch history.');
                }
                return response.json();
            } catch (err) {
                clearTimeout(timeoutId);
                throw err;
            }
        },

        // --- Reporting API Methods ---
        async getLowStockReportAsync() {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) throw new Error("Authentication required.");
            const response = await fetch(`${API_BASE_URL}/admin/reports/low-stock`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!response.ok) throw new Error("Failed to load low stock report.");
            return response.json();
        },

        async getRevenueReportAsync(from, to, groupBy = 'day') {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) throw new Error("Authentication required.");
            const response = await fetch(`${API_BASE_URL}/admin/reports/revenue?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}&groupBy=${encodeURIComponent(groupBy)}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!response.ok) throw new Error("Failed to load revenue report.");
            return response.json();
        },

        async getInventoryTurnoverAsync(from, to) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) throw new Error("Authentication required.");
            const response = await fetch(`${API_BASE_URL}/admin/reports/inventory-turnover?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!response.ok) throw new Error("Failed to load inventory turnover report.");
            return response.json();
        },

        async getBestSellersAsync(from, to, top = 10) {
            const token = loadStorage(STORAGE_KEYS.JWT_TOKEN, null);
            if (!token) throw new Error("Authentication required.");
            const response = await fetch(`${API_BASE_URL}/admin/reports/best-sellers?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}&top=${top}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!response.ok) throw new Error("Failed to load best sellers report.");
            return response.json();
        }
    };

    // Clear legacy product caches
    ['gb_products_v1', 'gb_products_v2', 'gb_products_v3', 'gb_products_v4'].forEach(k => localStorage.removeItem(k));

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

    // Auto-sync products from Backend API if available
    setTimeout(() => {
        if (window.AppState && window.AppState.fetchProductsFromBackend) {
            window.AppState.fetchProductsFromBackend();
        }
    }, 100);

})();
