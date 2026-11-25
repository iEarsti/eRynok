const API_BASE_URL = 'https://localhost:7082/api';

const DEFAULT_PLACEHOLDER = "data:image/svg+xml;charset=UTF-8,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80' viewBox='0 0 24 24' fill='none' stroke='%23bbb' stroke-width='1.5' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='3' width='18' height='18' rx='2' ry='2'%3E%3C/rect%3E%3Ccircle cx='8.5' cy='8.5' r='1.5'%3E%3C/circle%3E%3Cpolyline points='21 15 16 10 5 21'%3E%3C/polyline%3E%3C/svg%3E";

// ================= СТАН ЗАСТОСУНКУ =================

let token = localStorage.getItem('token');
let currentUser = null;

let products = [];
let categories = [];
let cart = null;

let selectedCategoryId = null;
let pendingProductIdAfterLogin = null;

// ================= DOM ЕЛЕМЕНТИ =================

// Сторінки
const viewBuyer = document.getElementById('viewBuyer');
const viewSeller = document.getElementById('viewSeller');
const viewAdmin = document.getElementById('viewAdmin');

const searchBarBlock = document.getElementById('searchBarBlock');
const userAuthContainer = document.getElementById('userAuthContainer');
const logoBtn = document.getElementById('logoBtn');

// Каталог
const productsGrid = document.getElementById('productsGrid');
const categoriesList = document.getElementById('categoriesList');
const searchInput = document.getElementById('searchInput');
const searchBtn = document.getElementById('searchBtn');

// Деталі товару
const detailImg = document.getElementById('detailImg');
const detailTitle = document.getElementById('detailTitle');
const detailPrice = document.getElementById('detailPrice');
const detailDesc = document.getElementById('detailDesc');
const detailBuyBtn = document.getElementById('detailBuyBtn');

// Авторизація
const loginModal = document.getElementById('loginModal');

const authModalTitle = document.getElementById('authModalTitle');
const authLoginTab = document.getElementById('authLoginTab');
const authRegisterTab = document.getElementById('authRegisterTab');

const loginForm = document.getElementById('loginForm');
const loginEmailInput = document.getElementById('loginEmailInput');
const loginPasswordInput = document.getElementById('loginPasswordInput');
const loginSubmitBtn = document.getElementById('loginSubmitBtn');

const registerForm = document.getElementById('registerForm');
const registerNameInput = document.getElementById('registerNameInput');
const registerEmailInput = document.getElementById('registerEmailInput');
const registerPasswordInput = document.getElementById('registerPasswordInput');
const registerConfirmPasswordInput = document.getElementById('registerConfirmPasswordInput');
const registerPhoneInput = document.getElementById('registerPhoneInput');
const registerAddressInput = document.getElementById('registerAddressInput');
const registerRoleSelect = document.getElementById('registerRoleSelect');
const registerSubmitBtn = document.getElementById('registerSubmitBtn');

// Кошик
const openCartBtn = document.getElementById('openCartBtn');
const cartCount = document.getElementById('cartCount');
const cartItemsList = document.getElementById('cartItemsList');
const cartTotalPrice = document.getElementById('cartTotalPrice');

// Історія покупця
const buyerOrdersSection = document.getElementById('buyerOrdersSection');
const buyerOrdersList = document.getElementById('buyerOrdersList');
const refreshOrdersBtn = document.getElementById('refreshOrdersBtn');

// Продавець
const sellerStoreName = document.getElementById('sellerStoreName');
const createProductForm = document.getElementById('createProductForm');
const productFormTitle = document.getElementById('productFormTitle');
const editProdId = document.getElementById('editProdId');
const productFormSubmitBtn = document.getElementById('productFormSubmitBtn');
const cancelEditBtn = document.getElementById('cancelEditBtn');
const newProdCategory = document.getElementById('newProdCategory');
const newProdImage = document.getElementById('newProdImage');
const formImgPreview = document.getElementById('formImgPreview');
const sellerProductsList = document.getElementById('sellerProductsList');

// Адміністратор
const createCategoryForm = document.getElementById('createCategoryForm');
const categoryFormTitle = document.getElementById('categoryFormTitle');
const editCatId = document.getElementById('editCatId');
const newCatName = document.getElementById('newCatName');
const newCatDesc = document.getElementById('newCatDesc');
const categorySubmitBtn = document.getElementById('categorySubmitBtn');
const cancelCatEditBtn = document.getElementById('cancelCatEditBtn');
const adminCategoriesList = document.getElementById('adminCategoriesList');
const adminOrdersList = document.getElementById('adminOrdersList');

// ================= ДОПОМІЖНІ ФУНКЦІЇ API =================

async function apiRequest(endpoint, options = {}) {
  const headers = {
    ...(options.headers || {})
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  if (options.body && !(options.body instanceof FormData)) {
    headers['Content-Type'] = 'application/json';
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers
  });

  if (response.status === 401 && token) {
    logoutUser();
    throw new Error('Сесія завершилася. Увійдіть у систему повторно.');
  }

  if (response.status === 204) {
    return null;
  }

  const responseText = await response.text();

  let data = null;

  if (responseText) {
    try {
      data = JSON.parse(responseText);
    } catch {
      data = responseText;
    }
  }

  if (!response.ok) {
    const message =
      typeof data === 'string'
        ? data
        : data?.message || data?.title || 'Сталася помилка запиту';

    throw new Error(message);
  }

  return data;
}

function getRoleNumber(role) {
  if (typeof role === 'number') {
    return role;
  }

  const normalizedRole = String(role).toUpperCase();

  const roles = {
    SELLER: 0,
    BUYER: 1,
    ADMIN: 2
  };

  return roles[normalizedRole] ?? -1;
}

function formatPrice(price) {
  return Number(price || 0).toLocaleString('uk-UA');
}

function getStatusName(status) {
  const statuses = {
    0: 'Очікує',
    1: 'Підтверджено',
    2: 'В обробці',
    3: 'Відправлено',
    4: 'Доставлено',
    5: 'Скасовано'
  };

  return statuses[status] ?? status;
}

function getStatusOptions() {
  return [
    { value: 0, name: 'Очікує' },
    { value: 1, name: 'Підтверджено' },
    { value: 2, name: 'В обробці' },
    { value: 3, name: 'Відправлено' },
    { value: 4, name: 'Доставлено' },
    { value: 5, name: 'Скасовано' }
  ];
}

// ================= ІНІЦІАЛІЗАЦІЯ =================

document.addEventListener('DOMContentLoaded', async () => {
  setupEventListeners();

  await restoreSession();

  await fetchCategories();
  await fetchProducts();

  updateAuthUI();
  routeView();
});

// Відновлення авторизації після перезавантаження
async function restoreSession() {
  if (!token) {
    currentUser = null;
    return;
  }

  try {
    const user = await apiRequest('/Auth/me');

    currentUser = {
      id: Number(user.id ?? user.Id),
      name: user.name ?? user.Name,
      email: user.email ?? user.Email,
      role: getRoleNumber(user.role ?? user.Role)
    };
  } catch (error) {
    console.error('Не вдалося відновити сесію:', error);

    token = null;
    currentUser = null;

    localStorage.removeItem('token');
  }
}

// ================= АВТОРИЗАЦІЯ =================

function openAuthModal(mode = 'login') {
  switchAuthMode(mode);
  openModal('loginModal');
}

function switchAuthMode(mode) {
  const isLogin = mode === 'login';

  loginForm.style.display = isLogin ? 'block' : 'none';
  registerForm.style.display = isLogin ? 'none' : 'block';

  authLoginTab.classList.toggle('active', isLogin);
  authRegisterTab.classList.toggle('active', !isLogin);

  authModalTitle.textContent = isLogin
    ? 'Вхід у систему'
    : 'Реєстрація';
}

async function loginUser(authData) {
  token = authData.token ?? authData.Token;

  currentUser = {
    id: Number(authData.id ?? authData.Id),
    name: authData.name ?? authData.Name,
    email: authData.email ?? authData.Email,
    role: getRoleNumber(authData.role ?? authData.Role)
  };

  localStorage.setItem('token', token);

  updateAuthUI();
  routeView();

  closeModal('loginModal');

  if (pendingProductIdAfterLogin !== null) {
    const productId = pendingProductIdAfterLogin;
    pendingProductIdAfterLogin = null;

    await addProductToCart(productId);
  }
}

function logoutUser() {
  token = null;
  currentUser = null;
  cart = null;
  cart = null;

  cartCount.textContent = '0';
  cartTotalPrice.textContent = '0';
  cartItemsList.innerHTML = '<p>Кошик порожній.</p>';

  localStorage.removeItem('token');

  updateAuthUI();
  routeView();
}

function updateAuthUI() {
  if (!currentUser) {
    userAuthContainer.innerHTML = `
      <button id="loginOpenBtn" class="auth-btn">Увійти</button>
    `;

    document.getElementById('loginOpenBtn').addEventListener('click', () => {
      openAuthModal('login');
    });

    return;
  }

  const role = currentUser.role;

  const roleData = {
    0: { title: 'Продавець', className: 'badge-role-0' },
    1: { title: 'Покупець', className: 'badge-role-1' },
    2: { title: 'Адмін', className: 'badge-role-2' }
  };

  const currentRole = roleData[role] || roleData[1];

  userAuthContainer.innerHTML = `
    <span>👤 <strong>${currentUser.name}</strong></span>
    <span class="user-badge ${currentRole.className}">
      ${currentRole.title}
    </span>
    <button class="logout-btn" id="logoutBtn">Вийти</button>
  `;

  document.getElementById('logoutBtn').addEventListener('click', logoutUser);
}

// Вхід
loginForm.addEventListener('submit', async (event) => {
  event.preventDefault();

  const loginData = {
    email: loginEmailInput.value.trim(),
    password: loginPasswordInput.value
  };

  try {
    loginSubmitBtn.disabled = true;
    loginSubmitBtn.textContent = 'Вхід...';

    const authData = await apiRequest('/Auth/login', {
      method: 'POST',
      body: JSON.stringify(loginData)
    });

    await loginUser(authData);
    loginForm.reset();
  } catch (error) {
    alert(error.message);
  } finally {
    loginSubmitBtn.disabled = false;
    loginSubmitBtn.textContent = 'Увійти';
  }
});

// Реєстрація
registerForm.addEventListener('submit', async (event) => {
  event.preventDefault();

  const password = registerPasswordInput.value;
  const confirmPassword = registerConfirmPasswordInput.value;

  if (password !== confirmPassword) {
    alert('Паролі не збігаються.');
    return;
  }

  const registerData = {
    name: registerNameInput.value.trim(),
    email: registerEmailInput.value.trim(),
    password: password,
    confirmPassword: confirmPassword,
    role: Number(registerRoleSelect.value),
    phone: registerPhoneInput.value.trim(),
    address: registerAddressInput.value.trim()
  };

  try {
    registerSubmitBtn.disabled = true;
    registerSubmitBtn.textContent = 'Реєстрація...';

    const authData = await apiRequest('/Auth/register', {
      method: 'POST',
      body: JSON.stringify(registerData)
    });

    await loginUser(authData);
    registerForm.reset();
  } catch (error) {
    alert(error.message);
  } finally {
    registerSubmitBtn.disabled = false;
    registerSubmitBtn.textContent = 'Зареєструватися';
  }
});

// ================= ПЕРЕМИКАННЯ СТОРІНОК =================

function routeView() {
  viewBuyer.classList.remove('active');
  viewSeller.classList.remove('active');
  viewAdmin.classList.remove('active');

  const role = currentUser?.role;

  if (role === 0) {
    viewSeller.classList.add('active');
    searchBarBlock.style.display = 'none';

    sellerStoreName.textContent = currentUser.name;

    renderSellerProducts();
  } else if (role === 2) {
    viewAdmin.classList.add('active');
    searchBarBlock.style.display = 'none';

    renderAdminCategories();
    fetchAdminOrders();
  } else {
    viewBuyer.classList.add('active');
    searchBarBlock.style.display = 'flex';

    renderProducts(products);

    if (role === 1) {
      buyerOrdersSection.style.display = 'block';
      fetchCart();
      fetchBuyerOrders();
    } else {
      buyerOrdersSection.style.display = 'none';
    }
  }
}

// ================= КАТЕГОРІЇ =================

async function fetchCategories() {
  try {
    categories = await apiRequest('/Categories');

    renderCategories();
    populateCategorySelect();

    if (currentUser?.role === 2) {
      renderAdminCategories();
    }
  } catch (error) {
    console.error('Помилка завантаження категорій:', error);
  }
}

function renderCategories() {
  categoriesList.innerHTML = `
    <li class="active" data-id="all">Усі товари</li>
  `;

  categories.forEach(category => {
    const li = document.createElement('li');

    li.textContent = category.name;
    li.dataset.id = category.id;

    li.addEventListener('click', () => {
      filterByCategory(category.id, category.name, li);
    });

    categoriesList.appendChild(li);
  });

  const allCategory = categoriesList.querySelector('[data-id="all"]');

  allCategory.addEventListener('click', () => {
    selectedCategoryId = null;

    document.getElementById('catalogTitle').textContent = 'Всі товари';

    document.querySelectorAll('.categories-list li')
      .forEach(item => item.classList.remove('active'));

    allCategory.classList.add('active');

    renderProducts(products);
  });
}

function populateCategorySelect() {
  newProdCategory.innerHTML = '';

  categories.forEach(category => {
    const option = document.createElement('option');

    option.value = category.id;
    option.textContent = category.name;

    newProdCategory.appendChild(option);
  });
}

// ================= ТОВАРИ =================

async function fetchProducts() {
  try {
    products = await apiRequest('/Products');

    renderProducts(products);

    if (currentUser?.role === 0) {
      renderSellerProducts();
    }
  } catch (error) {
    console.error('Помилка завантаження товарів:', error);

    productsGrid.innerHTML = `
      <p style="color:red">Не вдалося завантажити товари з сервера.</p>
    `;
  }
}

function renderProducts(items) {
  productsGrid.innerHTML = '';

  if (!items || items.length === 0) {
    productsGrid.innerHTML = '<p>Товари відсутні.</p>';
    return;
  }

  items.forEach(product => {
    const card = document.createElement('div');

    card.className = 'product-card';

    card.addEventListener('click', event => {
      if (event.target.closest('.buy-btn-card')) return;

      openProductDetails(product.id);
    });

    const image = product.imageUrl || DEFAULT_PLACEHOLDER;

    card.innerHTML = `
      <img
        src="${image}"
        alt="${product.name}"
        class="product-img"
        onerror="this.src='${DEFAULT_PLACEHOLDER}'"
      >

      <div>
        <h4 class="product-title">${product.name}</h4>
        <div class="product-price">${formatPrice(product.price)} ₴</div>
      </div>

      <div>
        <button
          class="primary-btn full-width buy-btn-card"
          onclick="handleBuyClick(${product.id})"
        >
          Додати до кошика
        </button>
      </div>
    `;

    productsGrid.appendChild(card);
  });
}

async function openProductDetails(productId) {
  try {
    const product = await apiRequest(`/Products/${productId}`);

    detailImg.src = product.imageUrl || DEFAULT_PLACEHOLDER;
    detailImg.onerror = () => {
      detailImg.src = DEFAULT_PLACEHOLDER;
    };

    detailTitle.textContent = product.name;
    detailPrice.textContent = formatPrice(product.price);
    detailDesc.textContent = product.description || 'Опис відсутній';

    detailBuyBtn.onclick = () => {
      closeModal('productDetailModal');
      handleBuyClick(product.id);
    };

    openModal('productDetailModal');
  } catch (error) {
    alert(error.message);
  }
}

// ================= ПОШУК І ФІЛЬТРАЦІЯ =================

function filterByCategory(categoryId, categoryName, clickedElement) {
  selectedCategoryId = Number(categoryId);

  document.querySelectorAll('.categories-list li')
    .forEach(item => item.classList.remove('active'));

  clickedElement.classList.add('active');

  document.getElementById('catalogTitle').textContent = categoryName;

  const filtered = products.filter(product =>
    Number(product.categoryId) === selectedCategoryId
  );

  renderProducts(filtered);
}

function handleSearch() {
  const query = searchInput.value.toLowerCase().trim();

  let filtered = products;

  if (selectedCategoryId !== null) {
    filtered = filtered.filter(product =>
      Number(product.categoryId) === selectedCategoryId
    );
  }

  if (query) {
    filtered = filtered.filter(product =>
      product.name.toLowerCase().includes(query)
    );
  }

  renderProducts(filtered);
}

// ================= КОШИК =================

window.handleBuyClick = async function(productId) {
  if (!currentUser) {
    pendingProductIdAfterLogin = productId;
    openAuthModal('login');
    return;
  }

  if (currentUser.role !== 1) {
    alert('Додавати товари до кошика може лише покупець.');
    return;
  }

  await addProductToCart(productId);
};

async function fetchCart() {
  if (!currentUser || currentUser.role !== 1) {
    cart = null;
    renderCart();
    return;
  }

  try {
    cart = await apiRequest('/Orders/cart');
  } catch (error) {
    if (error.message.toLowerCase().includes('cart not found')) {
      cart = null;
    } else {
      console.error('Помилка завантаження кошика:', error);
    }
  }

  renderCart();
}

async function addProductToCart(productId) {
  const product = products.find(item => Number(item.id) === Number(productId));

  if (!product) {
    alert('Товар не знайдено.');
    return;
  }

  try {
    await fetchCart();

    const existingOrder = cart?.orders?.find(order =>
      Number(order.productId) === Number(productId)
    );

    if (existingOrder) {
      if (existingOrder.quantity >= 1000) {
        alert('Максимальна кількість товару — 1000.');
        return;
      }

      await updateCartQuantity(
        existingOrder.id,
        existingOrder.quantity + 1
      );

      return;
    }

    const orderData = {
      items: [
        {
          productId: Number(productId),
          quantity: 1
        }
      ]
    };

    cart = await apiRequest('/Orders', {
      method: 'POST',
      body: JSON.stringify(orderData)
    });

    renderCart();
    await fetchBuyerOrders();

    alert('Товар додано до кошика.');
  } catch (error) {
    alert(error.message);
  }
}

async function updateCartQuantity(orderId, newQuantity) {
  if (newQuantity < 1 || newQuantity > 1000) return;

  try {
    await apiRequest(`/Orders/${orderId}/status`, {
      method: 'PATCH',
      body: JSON.stringify({
        quantity: newQuantity
      })
    });

    await fetchCart();
    await fetchBuyerOrders();
  } catch (error) {
    alert(error.message);
  }
}

window.changeCartQuantity = async function(orderId, change) {
  const order = cart?.orders?.find(item =>
    Number(item.id) === Number(orderId)
  );

  if (!order) return;

  const newQuantity = Number(order.quantity) + change;

  if (newQuantity < 1 || newQuantity > 1000) return;

  await updateCartQuantity(orderId, newQuantity);
};

function renderCart() {
  const orders = cart?.orders || [];

  cartItemsList.innerHTML = '';

  if (orders.length === 0) {
    cartItemsList.innerHTML = '<p>Кошик порожній.</p>';
    cartTotalPrice.textContent = '0';
    cartCount.textContent = '0';
    return;
  }

  let totalQuantity = 0;

  orders.forEach(order => {
    totalQuantity += Number(order.quantity);

    const item = document.createElement('div');
    item.className = 'cart-item';

    const product = products.find(p =>
      Number(p.id) === Number(order.productId)
    );

    const image = product?.imageUrl || DEFAULT_PLACEHOLDER;

    item.innerHTML = `
      <img
        src="${image}"
        alt="${order.productName}"
        class="cart-item-img"
        onerror="this.src='${DEFAULT_PLACEHOLDER}'"
      >

      <div class="cart-item-details">
        <strong>${order.productName}</strong>

        <div class="cart-quantity-controls">
          <button
            type="button"
            class="quantity-btn"
            onclick="changeCartQuantity(${order.id}, -1)"
            ${order.quantity <= 1 ? 'disabled' : ''}
          >−</button>

          <span class="quantity-value">${order.quantity}</span>

          <button
            type="button"
            class="quantity-btn"
            onclick="changeCartQuantity(${order.id}, 1)"
            ${order.quantity >= 1000 ? 'disabled' : ''}
          >+</button>
        </div>

        <div>
          ${formatPrice(order.unitPrice)} ₴ за одиницю
        </div>

        <div class="cart-item-price">
          ${formatPrice(order.totalPrice)} ₴
        </div>
      </div>

      <button
        type="button"
        class="cart-remove-btn"
        onclick="removeCartItem(${order.id})"
      >
        Видалити
      </button>
    `;

    cartItemsList.appendChild(item);
  });

  cartTotalPrice.textContent = formatPrice(cart.totalAmount);
  cartCount.textContent = totalQuantity;
}

window.removeCartItem = async function(orderId) {
  if (!confirm('Видалити цей товар із кошика?')) return;

  try {
    await apiRequest(`/Orders/${orderId}`, {
      method: 'DELETE'
    });

    await fetchCart();
    await fetchBuyerOrders();
  } catch (error) {
    alert(error.message);
  }
};

// ================= ІСТОРІЯ ЗАМОВЛЕНЬ ПОКУПЦЯ =================

async function fetchBuyerOrders() {
  if (!currentUser || currentUser.role !== 1) return;

  try {
    const orders = await apiRequest('/Orders');

    renderBuyerOrders(orders);
  } catch (error) {
    buyerOrdersList.innerHTML = `<p style="color:red">${error.message}</p>`;
  }
}

function renderBuyerOrders(orders) {
  if (!orders || orders.length === 0) {
    buyerOrdersList.innerHTML = '<p>У вас ще немає замовлень.</p>';
    return;
  }

  let tableHtml = `
    <table class="orders-table">
      <thead>
        <tr>
          <th>№</th>
          <th>Товар</th>
          <th>Кількість</th>
          <th>Ціна</th>
          <th>Сума</th>
          <th>Статус</th>
          <th>Дата</th>
        </tr>
      </thead>
      <tbody>
  `;

  orders.forEach(order => {
    tableHtml += `
      <tr>
        <td>#${order.id}</td>
        <td>${order.productName}</td>
        <td>${order.quantity} шт.</td>
        <td>${formatPrice(order.unitPrice)} ₴</td>
        <td>${formatPrice(order.totalPrice)} ₴</td>
        <td>
          <span class="status-badge status-${order.status}">
            ${getStatusName(order.status)}
          </span>
        </td>
        <td>${new Date(order.createdAt).toLocaleDateString('uk-UA')}</td>
      </tr>
    `;
  });

  tableHtml += '</tbody></table>';

  buyerOrdersList.innerHTML = tableHtml;
}

// ================= ФУНКЦІОНАЛ ПРОДАВЦЯ =================

newProdImage.addEventListener('input', () => {
  formImgPreview.src = newProdImage.value.trim() || DEFAULT_PLACEHOLDER;
});

formImgPreview.addEventListener('error', () => {
  formImgPreview.src = DEFAULT_PLACEHOLDER;
});

function renderSellerProducts() {
  sellerProductsList.innerHTML = '';

  if (!currentUser) return;

  const myProducts = products.filter(product =>
    Number(product.userId) === Number(currentUser.id)
  );

  if (myProducts.length === 0) {
    sellerProductsList.innerHTML = `
      <p style="color:gray;">У вас ще немає доданих товарів.</p>
    `;
    return;
  }

  myProducts.forEach(product => {
    const item = document.createElement('div');

    item.className = 'custom-item-row';

    item.innerHTML = `
      <div class="seller-item-left">
        <img
          src="${product.imageUrl || DEFAULT_PLACEHOLDER}"
          class="seller-thumb-img"
          alt="${product.name}"
          onerror="this.src='${DEFAULT_PLACEHOLDER}'"
        >

        <div>
          <strong>${product.name}</strong>
          <div style="font-size: 13px; color: gray;">
            Ціна: ${formatPrice(product.price)} ₴
          </div>
        </div>
      </div>

      <div class="item-actions-group">
        <button class="edit-btn" onclick="startEditProduct(${product.id})">
          Редагувати
        </button>

        <button class="delete-btn" onclick="deleteProduct(${product.id})">
          Видалити
        </button>
      </div>
    `;

    sellerProductsList.appendChild(item);
  });
}

window.startEditProduct = function(id) {
  const product = products.find(item => Number(item.id) === Number(id));

  if (!product) return;

  editProdId.value = product.id;

  document.getElementById('newProdName').value = product.name;
  document.getElementById('newProdPrice').value = product.price;
  newProdCategory.value = product.categoryId;

  newProdImage.value = product.imageUrl || '';
  formImgPreview.src = product.imageUrl || DEFAULT_PLACEHOLDER;

  document.getElementById('newProdDesc').value = product.description || '';

  productFormTitle.textContent = `Редагування: ${product.name}`;
  productFormSubmitBtn.textContent = 'Зберегти зміни';
  cancelEditBtn.style.display = 'block';

  createProductForm.scrollIntoView({ behavior: 'smooth' });
};

function resetProductForm() {
  editProdId.value = '';

  createProductForm.reset();

  formImgPreview.src = DEFAULT_PLACEHOLDER;

  productFormTitle.textContent = 'Додати новий товар';
  productFormSubmitBtn.textContent = 'Опублікувати товар';

  cancelEditBtn.style.display = 'none';
}

cancelEditBtn.addEventListener('click', resetProductForm);

createProductForm.addEventListener('submit', async event => {
  event.preventDefault();

  const isEditing = Boolean(editProdId.value);
  const targetId = Number(editProdId.value);

  const description = document.getElementById('newProdDesc').value.trim();

  if (description.length < 5) {
    alert('Опис товару повинен містити щонайменше 5 символів.');
    return;
  }

  const productData = {
    name: document.getElementById('newProdName').value.trim(),
    description: description,
    price: Number(document.getElementById('newProdPrice').value),
    imageUrl: newProdImage.value.trim() || null,
    categoryId: Number(newProdCategory.value)
  };

  const endpoint = isEditing
    ? `/Products/${targetId}`
    : '/Products';

  const method = isEditing ? 'PUT' : 'POST';

  try {
    productFormSubmitBtn.disabled = true;

    await apiRequest(endpoint, {
      method,
      body: JSON.stringify(productData)
    });

    alert(isEditing
      ? 'Товар успішно оновлено!'
      : 'Товар успішно додано!'
    );

    resetProductForm();

    await fetchProducts();
  } catch (error) {
    alert(`Помилка: ${error.message}`);
  } finally {
    productFormSubmitBtn.disabled = false;
  }
});

window.deleteProduct = async function(id) {
  if (!confirm('Видалити цей товар з каталогу?')) return;

  try {
    await apiRequest(`/Products/${id}`, {
      method: 'DELETE'
    });

    await fetchProducts();
  } catch (error) {
    alert(error.message);
  }
};

// ================= ФУНКЦІОНАЛ АДМІНІСТРАТОРА =================

function renderAdminCategories() {
  adminCategoriesList.innerHTML = '';

  if (!categories || categories.length === 0) {
    adminCategoriesList.innerHTML = `
      <p style="color:gray;">Категорії ще не додані.</p>
    `;
    return;
  }

  categories.forEach(category => {
    const item = document.createElement('div');

    item.className = 'admin-category-item';

    item.innerHTML = `
      <div class="admin-category-info">
        <strong>${category.name}</strong>
        <small>${category.description || 'Опис відсутній'}</small>
      </div>

      <div class="item-actions-group">
        <button
          class="edit-btn"
          onclick="startEditCategory(${category.id})"
        >
          Редагувати
        </button>

        <button
          class="delete-btn"
          onclick="deleteCategory(${category.id})"
        >
          Видалити
        </button>
      </div>
    `;

    adminCategoriesList.appendChild(item);
  });
}

window.startEditCategory = function(id) {
  const category = categories.find(item => Number(item.id) === Number(id));

  if (!category) return;

  editCatId.value = category.id;

  newCatName.value = category.name;
  newCatDesc.value = category.description || '';

  categoryFormTitle.textContent = `Редагування: ${category.name}`;
  categorySubmitBtn.textContent = 'Зберегти зміни';

  cancelCatEditBtn.style.display = 'block';

  createCategoryForm.scrollIntoView({ behavior: 'smooth' });
};

function resetCategoryForm() {
  editCatId.value = '';

  createCategoryForm.reset();

  categoryFormTitle.textContent = 'Додати нову категорію';
  categorySubmitBtn.textContent = 'Створити категорію';

  cancelCatEditBtn.style.display = 'none';
}

cancelCatEditBtn.addEventListener('click', resetCategoryForm);

createCategoryForm.addEventListener('submit', async event => {
  event.preventDefault();

  const name = newCatName.value.trim();
  const description = newCatDesc.value.trim();

  if (name.length < 2) {
    alert('Назва категорії повинна містити щонайменше 2 символи.');
    return;
  }

  if (description.length < 5) {
    alert('Опис категорії повинен містити щонайменше 5 символів.');
    return;
  }

  const isEditing = Boolean(editCatId.value);
  const targetId = Number(editCatId.value);

  const endpoint = isEditing
    ? `/Categories/${targetId}`
    : '/Categories';

  const method = isEditing ? 'PUT' : 'POST';

  try {
    categorySubmitBtn.disabled = true;

    await apiRequest(endpoint, {
      method,
      body: JSON.stringify({
        name,
        description
      })
    });

    alert(isEditing
      ? 'Категорію успішно змінено!'
      : 'Категорію створено!'
    );

    resetCategoryForm();

    await fetchCategories();
  } catch (error) {
    alert(error.message);
  } finally {
    categorySubmitBtn.disabled = false;
  }
});

window.deleteCategory = async function(id) {
  if (!confirm('Ви дійсно хочете видалити цю категорію?')) return;

  try {
    await apiRequest(`/Categories/${id}`, {
      method: 'DELETE'
    });

    await fetchCategories();
  } catch (error) {
    alert(error.message);
  }
};

// ================= ЗАМОВЛЕННЯ АДМІНІСТРАТОРА =================

async function fetchAdminOrders() {
  try {
    const orders = await apiRequest('/Orders');

    renderAdminOrders(orders);
  } catch (error) {
    adminOrdersList.innerHTML = `
      <p style="color:red">${error.message}</p>
    `;
  }
}

function renderAdminOrders(orders) {
  if (!orders || orders.length === 0) {
    adminOrdersList.innerHTML = '<p>Замовлень ще немає.</p>';
    return;
  }

  let tableHtml = `
    <table class="orders-table">
      <thead>
        <tr>
          <th>№</th>
          <th>Товар</th>
          <th>Покупець (ID)</th>
          <th>Кількість</th>
          <th>Сума</th>
          <th>Статус</th>
          <th>Дія</th>
        </tr>
      </thead>
      <tbody>
  `;

  orders.forEach(order => {
    const statusOptions = getStatusOptions()
      .map(status => `
        <option
          value="${status.value}"
          ${Number(order.status) === status.value ? 'selected' : ''}
        >
          ${status.name}
        </option>
      `)
      .join('');

    tableHtml += `
      <tr>
        <td>#${order.id}</td>
        <td>${order.productName}</td>
        <td>${order.buyerId}</td>
        <td>${order.quantity} шт.</td>
        <td>${formatPrice(order.totalPrice)} ₴</td>

        <td>
          <span class="status-badge status-${order.status}">
            ${getStatusName(order.status)}
          </span>
        </td>

        <td>
          <select
            onchange="updateOrderStatus(${order.id}, this.value)"
          >
            ${statusOptions}
          </select>
        </td>
      </tr>
    `;
  });

  tableHtml += '</tbody></table>';

  adminOrdersList.innerHTML = tableHtml;
}

window.updateOrderStatus = async function(orderId, newStatus) {
  if (newStatus === '') return;

  try {
    await apiRequest(`/Orders/${orderId}/status`, {
      method: 'PATCH',
      body: JSON.stringify({
        status: Number(newStatus)
      })
    });

    await fetchAdminOrders();
  } catch (error) {
    alert(error.message);
    await fetchAdminOrders();
  }
};

// ================= МОДАЛЬНІ ВІКНА =================

function openModal(id) {
  document.getElementById(id).classList.add('open');
}

window.closeModal = function(id) {
  document.getElementById(id).classList.remove('open');
};

// ================= ОБРОБНИКИ ПОДІЙ =================

function setupEventListeners() {
  // Логотип
  logoBtn.addEventListener('click', () => {
    if (currentUser && currentUser.role !== 1) return;

    selectedCategoryId = null;

    document.getElementById('catalogTitle').textContent = 'Всі товари';

    document.querySelectorAll('.categories-list li')
      .forEach(item => item.classList.remove('active'));

    const allCategory = categoriesList.querySelector('[data-id="all"]');

    if (allCategory) {
      allCategory.classList.add('active');
    }

    searchInput.value = '';

    renderProducts(products);
  });

  // Пошук
  searchBtn.addEventListener('click', handleSearch);

  searchInput.addEventListener('keyup', event => {
    if (event.key === 'Enter') {
      handleSearch();
    }
  });

  // Авторизація
  authLoginTab.addEventListener('click', () => {
    switchAuthMode('login');
  });

  authRegisterTab.addEventListener('click', () => {
    switchAuthMode('register');
  });

  // Кошик
  openCartBtn.addEventListener('click', async () => {
    if (!currentUser) {
      openAuthModal('login');
      return;
    }

    if (currentUser.role !== 1) {
      alert('Кошик доступний лише покупцям.');
      return;
    }

    await fetchCart();

    openModal('cartModal');
  });

  // Історія замовлень
  refreshOrdersBtn.addEventListener('click', fetchBuyerOrders);

  // Закриття модальних вікон кліком поза ними
  window.addEventListener('click', event => {
    if (event.target.classList.contains('modal')) {
      event.target.classList.remove('open');
    }
  });
}