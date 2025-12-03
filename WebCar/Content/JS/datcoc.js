document.addEventListener('DOMContentLoaded', () => {
    // --- DATA ---
    const products = [
        {
            id: 1, name: "VF3", price: 290000000, year: 2019, seats: "4 chỗ", transmission: "tự động", category: "MiniCar", colors: [{
                name: "Đỏ", image: "/images/vf3do.jpg", code: "#d32f2f"
            }, { name: "Trắng", image: "//images/vf3trang.jpg", code: "#f5f5f5" }, { name: "Đen", image: "//images/vf3deb.jpg", code: "#212121" }]
        },
        { id: 2, name: "VF5", price: 529000000, year: 2019, seats: "5 chỗ", transmission: "Tự động", category: "A-SUV", colors: [{ name: "Trắng", image: "//images/vf5xanh.jpeg", code: "#0000FF" }, { name: "Cam", image: "/images/vf5cam.jpg", code: "#fb8c00" }, { name: "Xám", image: "/images/vf5xam.jpg", code: "#616161" }] },
        { id: 3, name: "VF6", price: 689000000, year: 2019, seats: "5 chỗ", transmission: "Tự động", category: "B-SUV", colors: [{ name: "bạc", image: "//images/vf6den.jpg", code: "#bdbdbd" }, { name: "cam", image: "/images/vf6cam.jpg", code: "#FFCC33" }] },
        { id: 4, name: "VF7", price: 799000000, year: 2020, seats: "5 chỗ", transmission: "tự động", category: "C-SUV", colors: [{ name: "Trắng", image: "//images/vf7trang.jpg", code: "#f5f5f5" }, { name: "đen", image: "/images/vf7den.jpg", code: "#212121" }] },
        { id: 5, name: "VF8", price: 1019000000, year: 2021, seats: "5 chỗ", transmission: "Tự động", category: "D-SUV", colors: [{ name: "Trắng", image: "/images/vf8trang.jpg", code: "#f5f5f5" }, { name: "đỏ", image: "/images/vf8do.jpg", code: "#d32f2f" }] },
        { id: 6, name: "VF9", price: 1499000000, year: 2023, seats: "6-7 chỗ", transmission: "Tự động", category: "E-SUV", colors: [{ name: "Bạc", image: "/images/vf9den.jpg", code: "#bdbdbd" }, { name: "Trắng", image: "/images/vf9trang.jpg", code: "#f5f5f5" }] },
    
    ];
    const districts = { "HCM": ["Quận 1", "Quận 3", "Quận 4", "Quận 5", "Quận 7", "TP. Thủ Đức"], "HN": ["Ba Đình", "Hoàn Kiếm", "Hai Bà Trưng", "Đống Đa", "Cầu Giấy"], "BD": ["TP. Thủ Dầu Một", "TP. Thuận An", "TP. Dĩ An", "Tân Uyên"] };

    // --- STATE & DOM ELEMENTS ---
    let cart = [];
    let orderHistory = [];
    const productList = document.getElementById("product-list");
    const searchInput = document.getElementById("search-input");
    const categoryFilters = document.querySelectorAll("#category-filter .filter-btn");
    const priceFilters = document.querySelectorAll('input[name="price-range"]');

    const cartModal = new bootstrap.Modal(document.getElementById('cart-modal'));
    const checkoutModal = new bootstrap.Modal(document.getElementById('checkout-modal'));
    const notificationModal = new bootstrap.Modal(document.getElementById('notification-modal'));
    const orderHistoryModal = new bootstrap.Modal(document.getElementById('order-history-modal'));


    // --- UTILITY FUNCTIONS ---
    const formatCurrency = (n) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(n);
    const showNotification = (title, message) => {
        document.getElementById('notification-title').innerText = title;
        document.getElementById('notification-message').innerText = message;
        notificationModal.show();
    };

    // --- LOCAL STORAGE ---
    const saveToLocalStorage = (key, data) => {
        try { localStorage.setItem(key, JSON.stringify(data)); }
        catch (e) { console.error(`Error saving ${key} to localStorage`, e); }
    };
    const loadFromLocalStorage = (key) => {
        try {
            const data = localStorage.getItem(key);
            return data ? JSON.parse(data) : [];
        } catch (e) {
            console.error(`Error loading ${key} from localStorage`, e);
            return [];
        }
    };

    // --- PRODUCT DISPLAY & FILTERING ---
    function displayProducts(productsToDisplay) {
        productList.innerHTML = "";
        if (productsToDisplay.length === 0) {
            productList.innerHTML = '<p class="text-center w-100 text-muted">Không tìm thấy sản phẩm phù hợp.</p>';
            return;
        }
        productsToDisplay.forEach(product => {
            const col = document.createElement("div");
            col.className = "col-lg-4 col-md-6 mb-4";

            const colorSwatchesHTML = product.colors.map((color, index) => `
                        <span class="color-swatch ${index === 0 ? 'active' : ''}"
                              style="background-color: ${color.code};"
                              onclick="changeColor(event, ${product.id}, '${color.image}')"
                              title="${color.name}">
                        </span>
                    `).join('');

            col.innerHTML = `
                        <div class="product-card" id="card-${product.id}">
                            <img src="${product.colors[0].image}" alt="${product.name}" class="product-image" onerror="this.onerror=null;this.src='https://placehold.co/400x270/f8f9fa/ccc?text=Image+Not+Found';">
                            <div class="product-content-wrapper">
                                <h3 class="product-title">${product.name}</h3>
                                <div class="product-price">${formatCurrency(product.price)}</div>
                                <div class="color-swatches">${colorSwatchesHTML}</div>
                                <div class="product-info">
                                    <div class="info-item"><i class="bi bi-calendar3"></i><p>${product.year}</p></div>
                                    <div class="info-item"><i class="bi bi-people-fill"></i><p>${product.seats}</p></div>
                                    <div class="info-item"><i class="bi bi-gear-fill"></i><p>${product.transmission}</p></div>
                                </div>
                                <button class="add-to-cart-btn" onclick="addToCart(event, ${product.id})">
                                    <i class="bi bi-cart-plus-fill"></i> Thêm vào giỏ
                                </button>
                            </div>
                        </div>`;
            productList.appendChild(col);
        });
    }

    // --- COLOR CHANGE LOGIC ---
    window.changeColor = (event, productId, newImage) => {
        event.stopPropagation();
        const card = document.getElementById(`card-${productId}`);
        const imageElement = card.querySelector('.product-image');
        imageElement.src = newImage;

        const swatchesContainer = event.target.parentElement;
        swatchesContainer.querySelectorAll('.color-swatch').forEach(sw => sw.classList.remove('active'));
        event.target.classList.add('active');
    }

    function filterAndRender() {
        let filteredProducts = [...products];
        const activeCategory = document.querySelector("#category-filter .filter-btn.active").dataset.category;
        if (activeCategory !== 'all') {
            filteredProducts = filteredProducts.filter(p => p.category === activeCategory);
        }
        const searchTerm = searchInput.value.toLowerCase().trim();
        if (searchTerm) {
            filteredProducts = filteredProducts.filter(p => p.name.toLowerCase().includes(searchTerm));
        }
        const priceRange = document.querySelector('input[name="price-range"]:checked').value;
        if (priceRange !== 'all') {
            const [min, max] = priceRange.split('-').map(val => parseFloat(val) * 1000000);
            filteredProducts = filteredProducts.filter(p => {
                if (priceRange === "700-1000") return p.price >= min; // Upper bound is open
                return p.price >= min && p.price < max;
            });
        }
        displayProducts(filteredProducts);
    }

    // --- CART LOGIC ---
    window.addToCart = (event, productId) => {
        event.stopPropagation();
        const product = products.find(p => p.id === productId);
        const card = document.getElementById(`card-${productId}`);
        const activeSwatch = card.querySelector('.color-swatch.active');

        const selectedColorName = activeSwatch.title;
        const selectedColorImage = card.querySelector('.product-image').src;
        const selectedColorCode = activeSwatch.style.backgroundColor;

        const cartItemId = `${productId}-${selectedColorName}`;
        const existingItem = cart.find(item => item.cartItemId === cartItemId);

        if (existingItem) {
            existingItem.quantity++;
        } else {
            cart.push({
                ...product,
                cartItemId,
                quantity: 1,
                selectedColor: { name: selectedColorName, image: selectedColorImage, code: selectedColorCode }
            });
        }
        updateCartUI();
        saveToLocalStorage('shoppingCart', cart);
        showNotification('Thành công', `Đã thêm "${product.name} (${selectedColorName})" vào giỏ hàng!`);
    }

    window.changeQuantity = (cartItemId, newQuantity) => {
        const cartItem = cart.find(item => item.cartItemId === cartItemId);
        if (cartItem) {
            if (newQuantity <= 0) {
                removeFromCart(cartItemId);
            } else {
                cartItem.quantity = newQuantity;
                updateCartUI();
                saveToLocalStorage('shoppingCart', cart);
            }
        }
    }

    window.removeFromCart = (cartItemId) => {
        cart = cart.filter(item => item.cartItemId !== cartItemId);
        updateCartUI();
        saveToLocalStorage('shoppingCart', cart);
    }

    function updateCartUI() {
        renderCartItems();
        updateCartCount();
        updateCartSubtotal();
    }

    function updateCartCount() {
        const totalCount = cart.reduce((sum, item) => sum + item.quantity, 0);
        document.getElementById('cart-count').innerText = totalCount;
    }

    function updateCartSubtotal() {
        const subtotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
        document.getElementById('cart-subtotal').innerText = formatCurrency(subtotal);
    }

    function renderCartItems() {
        const container = document.getElementById('cart-items-container');
        const showCheckoutBtn = document.getElementById('show-checkout-btn');
        if (cart.length === 0) {
            container.innerHTML = '<p class="text-center my-4 text-muted">Giỏ hàng của bạn đang trống.</p>';
            showCheckoutBtn.style.display = 'none';
            return;
        }
        showCheckoutBtn.style.display = 'block';
        container.innerHTML = '';
        cart.forEach(item => {
            const itemEl = document.createElement('div');
            itemEl.className = 'cart-item';
            itemEl.innerHTML = `
                        <img src="${item.selectedColor.image}" alt="${item.name}" onerror="this.onerror=null;this.src='https://placehold.co/80x80/f0f0f0/ccc?text=N/A';">
                        <div class="cart-item-info flex-grow-1">
                            <h5>${item.name}</h5>
                            <p>Màu: ${item.selectedColor.name}</p>
                            <p class="price">${formatCurrency(item.price)}</p>
                        </div>
                        <div class="quantity-selector">
                            <button onclick="changeQuantity('${item.cartItemId}', ${item.quantity - 1})">-</button>
                            <input type="text" value="${item.quantity}" readonly>
                            <button onclick="changeQuantity('${item.cartItemId}', ${item.quantity + 1})">+</button>
                        </div>
                        <button class="remove-item-btn" onclick="removeFromCart('${item.cartItemId}')"><i class="bi bi-trash-fill"></i></button>
                    `;
            container.appendChild(itemEl);
        });
    }

    // --- CHECKOUT & PAYMENT LOGIC ---
    document.getElementById('show-checkout-btn').addEventListener('click', () => {
        if (cart.length === 0) {
            showNotification('Giỏ hàng trống', 'Vui lòng thêm sản phẩm vào giỏ hàng trước khi thanh toán!');
            return;
        }
        cartModal.hide();
        renderCheckoutSummary();
        checkoutModal.show();
    });

    function renderCheckoutSummary() {
        const summaryContainer = document.getElementById('checkout-items-summary');
        summaryContainer.innerHTML = '';
        cart.forEach(item => {
            summaryContainer.innerHTML += `
                        <div class="d-flex justify-content-between align-items-center mb-2 small text-muted">
                            <span>${item.name} (${item.selectedColor.name}) x${item.quantity}</span>
                            <span class="fw-medium">${formatCurrency(item.price * item.quantity)}</span>
                        </div>`;
        });
        const total = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
        document.getElementById('checkout-total').innerText = formatCurrency(total);
    }

    window.validateAndCompleteOrder = () => {
        const name = document.getElementById('customer-name').value.trim();
        const phone = document.getElementById('customer-phone').value.trim();
        const email = document.getElementById('customer-email').value.trim();
        const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked');

        if (!name || !phone || !email) {
            showNotification('Lỗi', 'Vui lòng điền đầy đủ Họ tên, SĐT và Email.'); return;
        }
        if (!/^0\d{9}$/.test(phone)) {
            showNotification('Lỗi', 'Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.'); return;
        }
        if (document.getElementById('home-delivery').checked) {
            if (!document.getElementById('customer-city').value || !document.getElementById('customer-district').value || !document.getElementById('customer-address').value.trim()) {
                showNotification('Lỗi', 'Vui lòng điền đầy đủ thông tin giao hàng.'); return;
            }
        }
        if (!paymentMethod) {
            showNotification('Lỗi', 'Vui lòng chọn hình thức thanh toán.'); return;
        }


        // Create order object
        const newOrder = {
            id: `DH-${Date.now()}`,
            date: new Date().toISOString(),
            customer: { name, phone, email },
            delivery: {
                method: document.querySelector('input[name="deliveryMethod"]:checked').value,
                city: document.getElementById('customer-city').value,
                district: document.getElementById('customer-district').value,
                address: document.getElementById('customer-address').value.trim()
            },
            paymentMethod: paymentMethod.value,
            items: [...cart],
            total: cart.reduce((sum, item) => sum + item.price * item.quantity, 0),
            status: 'Đang xử lý'
        };

        // Save order and clear cart
        orderHistory.push(newOrder);
        saveToLocalStorage('orderHistory', orderHistory);
        cart = [];
        saveToLocalStorage('shoppingCart', cart);
        updateCartUI();

        checkoutModal.hide();
        const message = `Cảm ơn ${name} đã mua hàng!\nMã đơn hàng của bạn là ${newOrder.id}.\nHình thức thanh toán: ${newOrder.paymentMethod}.`;
        showNotification('Đặt hàng thành công!', message);
        checkoutModal.hide();
        showNotification('Đặt hàng thành công!', message);

        window.location.href = 'datcoc.html';   

    }

    // --- ORDER HISTORY LOGIC ---
    document.getElementById('order-history-modal').addEventListener('show.bs.modal', renderOrderHistory);

    function renderOrderHistory() {
        const container = document.getElementById('order-history-accordion');
        if (orderHistory.length === 0) {
            container.innerHTML = '<p class="text-center text-muted">Bạn chưa có đơn hàng nào.</p>';
            return;
        }

        container.innerHTML = '';
        [...orderHistory].reverse().forEach((order) => {
            const orderDate = new Date(order.date);
            const formattedDate = `${orderDate.toLocaleDateString('vi-VN')} ${orderDate.toLocaleTimeString('vi-VN')}`;

            const itemsHtml = order.items.map(item => `
                        <tr>
                            <td>${item.name} <br><small class="text-muted">Màu: ${item.selectedColor.name}</small></td>
                            <td>${item.quantity}</td>
                            <td>${formatCurrency(item.price)}</td>
                            <td>${formatCurrency(item.price * item.quantity)}</td>
                        </tr>
                    `).join('');

            const accordionItem = `
                    <div class="accordion-item">
                        <h2 class="accordion-header" id="heading-${order.id}">
                            <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapse-${order.id}" aria-expanded="false" aria-controls="collapse-${order.id}">
                                <div class="order-summary-item">
                                    <div>
                                        <span class="fw-bold">${order.id}</span>
                                        <span><i class="bi bi-calendar"></i> ${formattedDate}</span>
                                    </div>
                                    <div>
                                        <span class="badge bg-success">${order.status}</span>
                                        <span class="fw-bold text-danger">${formatCurrency(order.total)}</span>
                                    </div>
                                </div>
                            </button>
                        </h2>
                        <div id="collapse-${order.id}" class="accordion-collapse collapse" aria-labelledby="heading-${order.id}" data-bs-parent="#order-history-accordion">
                            <div class="accordion-body">
                               <div class="row">
                                   <div class="col-md-7">
                                       <h5>Chi tiết sản phẩm</h5>
                                        <table class="table table-sm table-bordered">
                                            <thead><tr><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead>
                                            <tbody>${itemsHtml}</tbody>
                                        </table>
                                   </div>
                                   <div class="col-md-5">
                                       <h5>Thông tin đơn hàng</h5>
                                       <table class="table table-sm order-details-table">
                                           <tbody>
                                               <tr><th>Khách hàng</th><td>${order.customer.name} - ${order.customer.phone}</td></tr>
                                               <tr><th>Nhận hàng</th><td>${order.delivery.method === 'store' ? 'Tại cửa hàng' : 'Giao tận nơi'}</td></tr>
                                               ${order.delivery.method === 'delivery' && order.delivery.address ? `<tr><th>Địa chỉ</th><td>${order.delivery.address}, ${order.delivery.district}, ${order.delivery.city}</td></tr>` : ''}
                                               <tr><th>Thanh toán</th><td>${order.paymentMethod}</td></tr>
                                           </tbody>
                                       </table>
                                   </div>
                               </div>
                            </div>
                        </div>
                    </div>`;
            container.innerHTML += accordionItem;
        });
    }


    // --- EVENT LISTENERS & INITIALIZATION ---
    categoryFilters.forEach(button => button.addEventListener("click", () => {
        categoryFilters.forEach(btn => btn.classList.remove("active"));
        button.classList.add("active");
        filterAndRender();
    }));
    priceFilters.forEach(radio => radio.addEventListener('change', filterAndRender));
    searchInput.addEventListener('input', filterAndRender);

    document.getElementById('customer-city').addEventListener('change', function () {
        const city = this.value;
        const districtSelect = document.getElementById('customer-district');
        districtSelect.innerHTML = '<option value="">Quận/Huyện (*)</option>';
        if (city && districts[city]) {
            districts[city].forEach(district => {
                districtSelect.innerHTML += `<option value="${district}">${district}</option>`;
            });
        }
    });

    document.querySelectorAll('input[name="deliveryMethod"]').forEach(elem => {
        elem.addEventListener("change", (event) => {
            document.getElementById('delivery-info').style.display = (event.target.value === "delivery") ? 'block' : 'none';
        });
    });

    // Initial load
    cart = loadFromLocalStorage('shoppingCart');
    orderHistory = loadFromLocalStorage('orderHistory');
    updateCartUI();
    filterAndRender();
});