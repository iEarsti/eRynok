const API_BASE_URL = 'https://localhost:7082/api';
const DEFAULT_PLACEHOLDER = "https://via.placeholder.com/200?text=Прев'ю+фото";

// Стан застосунку
let users = [];
let currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
let products = [];
let categories = [];
let selectedProductForOrder = null;
let pendingProductIdAfterLogin = null;

// DOM Елементи сторінок
const viewBuyer = document.getElementById('viewBuyer');
const viewSeller = document.getElementById('viewSeller');
const viewAdmin = document.getElementById('viewAdmin');
const searchBarBlock = document.getElementById('searchBarBlock');
const userAuthContainer = document.getElementById('userAuthContainer');
const logoBtn = document.getElementById('logoBtn');

// DOM Елементи каталогу (Buyer)
const productsGrid = document.getElementById('productsGrid');
const categoriesList = document.getElementById('categoriesList');
const searchInput = document.getElementById('searchInput');
const searchBtn = document.getElementById('searchBtn');

// DOM Модалки деталей та покупки
const productDetailModal = document.getElementById('productDetailModal');
const detailImg = document.getElementById('detailImg');
const detailTitle = document.getElementById('detailTitle');
const detailPrice = document.getElementById('detailPrice');
const detailDesc = document.getElementById('detailDesc');
const detailBuyBtn = document.getElementById('detailBuyBtn');

const checkoutModal = document.getElementById('checkoutModal');
const orderImg = document.getElementById('orderImg');
const orderTitle = document.getElementById('orderTitle');
const orderUnitPrice = document.getElementById('orderUnitPrice');
const orderQuantity = document.getElementById('orderQuantity');
const orderTotalPrice = document.getElementById('orderTotalPrice');
const confirmOrderBtn = document.getElementById('confirmOrderBtn');

// DOM Авторизації
const loginModal = document.getElementById('loginModal');
const userSelect = document.getElementById('userSelect');
const loginEmailInput = document.getElementById('loginEmailInput');
const loginSubmitBtn = document.getElementById('loginSubmitBtn');

// DOM Панелі Продавця
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

// DOM Панелі Адміністратора
const createCategoryForm = document.getElementById('createCategoryForm');
const adminOrdersList = document.getElementById('adminOrdersList');

// ================= ІНІЦІАЛІЗАЦІЯ =================
document.addEventListener('DOMContentLoaded', async () => {
  await fetchUsers();
  await fetchCategories();
  await fetchProducts();

  setupEventListeners();
  updateAuthUI();
  routeView();
});

// Завантаження користувачів
async function fetchUsers() {
  try {
    const res = await fetch(`${API_BASE_URL}/Users`);
    if (res.ok) {
      users = await res.json();
      populateUserSelect();
    }
  } catch (err) {
    console.error('Помилка завантаження користувачів:', err);
  }
}

function populateUserSelect() {
  userSelect.innerHTML = '<option value="">-- Оберіть користувача зі списку --</option>';
  users.forEach(u => {
    const roleName = u.role === 0 ? 'Продавець' : u.role === 2 ? 'Адмін' : 'Покупець';
    const opt = document.createElement('option');
    opt.value = u.id;
    opt.textContent = `${u.name} (${roleName}) - ${u.email}`;
    userSelect.appendChild(opt);
  });
}

// Завантаження категорій
async function fetchCategories() {
  try {
    const res = await fetch(`${API_BASE_URL}/Categories`);
    if (res.ok) {
      categories = await res.json();
      renderCategories();
      populateCategorySelect();
    }
  } catch (err) {
    console.error('Помилка завантаження категорій:', err);
  }
}

function renderCategories() {
  categoriesList.innerHTML = '<li class="active" data-id="all">Усі товари</li>';
  categories.forEach(cat => {
    const li = document.createElement('li');
    li.textContent = cat.name;
    li.dataset.id = cat.id;
    li.addEventListener('click', () => filterByCategory(cat.id, cat.name, li));
    categoriesList.appendChild(li);
  });

  categoriesList.querySelector('[data-id="all"]').addEventListener('click', (e) => {
    document.getElementById('catalogTitle').textContent = 'Всі товари';
    document.querySelectorAll('.categories-list li').forEach(el => el.classList.remove('active'));
    e.target.classList.add('active');
    renderProducts(products);
  });
}

function populateCategorySelect() {
  newProdCategory.innerHTML = '';
  categories.forEach(cat => {
    const opt = document.createElement('option');
    opt.value = cat.id;
    opt.textContent = cat.name;
    newProdCategory.appendChild(opt);
  });
}

// Завантаження товарів
async function fetchProducts() {
  try {
    const res = await fetch(`${API_BASE_URL}/Products`);
    if (res.ok) {
      products = await res.json();
      renderProducts(products);
      if (currentUser && currentUser.role === 0) {
        renderSellerProducts();
      }
    }
  } catch (err) {
    productsGrid.innerHTML = `<p style="color:red">Не вдалося завантажити товари з сервера.</p>`;
  }
}

// Вітрина товарів (Покупець / Гість)
function renderProducts(items) {
  productsGrid.innerHTML = '';
  if (!items || !items.length) {
    productsGrid.innerHTML = '<p>Товари відсутні.</p>';
    return;
  }

  items.forEach(prod => {
    const card = document.createElement('div');
    card.className = 'product-card';
    card.addEventListener('click', (e) => {
      if (e.target.closest('.buy-btn-card')) return;
      openProductDetails(prod.id);
    });

    card.innerHTML = `
      <img src="${prod.imageUrl || DEFAULT_PLACEHOLDER}" alt="${prod.name}" class="product-img" onerror="this.src='${DEFAULT_PLACEHOLDER}'">
      <div>
        <h4 class="product-title">${prod.name}</h4>
        <div class="product-price">${prod.price} ₴</div>
      </div>
      <div>
        <button class="primary-btn full-width buy-btn-card" onclick="handleBuyClick(${prod.id})">Купити</button>
      </div>
    `;
    productsGrid.appendChild(card);
  });
}

// Детальний перегляд товару
async function openProductDetails(productId) {
  try {
    const res = await fetch(`${API_BASE_URL}/Products/${productId}`);
    const product = res.ok ? await res.json() : products.find(p => p.id === productId);
    if (!product) return;

    detailImg.src = product.imageUrl || DEFAULT_PLACEHOLDER;
    detailImg.onerror = () => { detailImg.src = DEFAULT_PLACEHOLDER; };
    detailTitle.textContent = product.name;
    detailPrice.textContent = product.price;
    detailDesc.textContent = product.description || 'Опис відсутній';

    detailBuyBtn.onclick = () => {
      closeModal('productDetailModal');
      handleBuyClick(product.id);
    };

    openModal('productDetailModal');
  } catch (e) {
    console.error(e);
  }
}

// ================= АВТОРИЗАЦІЯ ТА РОЛІ =================
window.handleBuyClick = function(productId) {
  if (!currentUser) {
    pendingProductIdAfterLogin = productId;
    openModal('loginModal');
    return;
  }
  openCheckout(productId);
};

function loginUser(user) {
  currentUser = user;
  localStorage.setItem('currentUser', JSON.stringify(currentUser));
  updateAuthUI();
  routeView();
  closeModal('loginModal');

  if (pendingProductIdAfterLogin) {
    const prodId = pendingProductIdAfterLogin;
    pendingProductIdAfterLogin = null;
    openCheckout(prodId);
  }
}

function logoutUser() {
  currentUser = null;
  localStorage.removeItem('currentUser');
  updateAuthUI();
  routeView();
}

function updateAuthUI() {
  if (currentUser) {
    let roleTitle = 'Покупець';
    let roleClass = 'badge-role-1';
    if (currentUser.role === 0) {
      roleTitle = 'Продавець';
      roleClass = 'badge-role-0';
    } else if (currentUser.role === 2) {
      roleTitle = 'Адмін';
      roleClass = 'badge-role-2';
    }

    userAuthContainer.innerHTML = `
      <span>👤 <strong>${currentUser.name}</strong></span>
      <span class="user-badge ${roleClass}">${roleTitle}</span>
      <button class="logout-btn" id="logoutBtn">Вийти</button>
    `;
    document.getElementById('logoutBtn').addEventListener('click', logoutUser);
  } else {
    userAuthContainer.innerHTML = `
      <button id="loginOpenBtn" class="auth-btn">Увійти</button>
    `;
    document.getElementById('loginOpenBtn').addEventListener('click', () => openModal('loginModal'));
  }
}

function routeView() {
  viewBuyer.classList.remove('active');
  viewSeller.classList.remove('active');
  viewAdmin.classList.remove('active');

  if (currentUser && currentUser.role === 0) {
    viewSeller.classList.add('active');
    searchBarBlock.style.display = 'none';
    sellerStoreName.textContent = currentUser.name;
    renderSellerProducts();
  } else if (currentUser && currentUser.role === 2) {
    viewAdmin.classList.add('active');
    searchBarBlock.style.display = 'none';
    fetchAdminOrders();
  } else {
    viewBuyer.classList.add('active');
    searchBarBlock.style.display = 'flex';
    renderProducts(products);
  }
}

// ================= ФУНКЦІОНАЛ ПРОДАВЦЯ (ROLE: 0) =================

// Живе оновлення прев'ю картинки при вводі URL
newProdImage.addEventListener('input', () => {
  const url = newProdImage.value.trim();
  formImgPreview.src = url || DEFAULT_PLACEHOLDER;
});
formImgPreview.addEventListener('error', () => {
  formImgPreview.src = DEFAULT_PLACEHOLDER;
});

// Відображення списку товарів продавця з мініатюрами
function renderSellerProducts() {
  sellerProductsList.innerHTML = '';
  const myProducts = products.filter(p => p.userId === currentUser.id);

  if (!myProducts.length) {
    sellerProductsList.innerHTML = '<p style="color:gray;">У вас ще немає доданих товарів.</p>';
    return;
  }

  myProducts.forEach(prod => {
    const item = document.createElement('div');
    item.className = 'custom-item-row';
    item.innerHTML = `
      <div class="seller-item-left">
        <img src="${prod.imageUrl || DEFAULT_PLACEHOLDER}" class="seller-thumb-img" alt="${prod.name}" onerror="this.src='${DEFAULT_PLACEHOLDER}'">
        <div>
          <strong>${prod.name}</strong>
          <div style="font-size: 13px; color: gray;">Ціна: ${prod.price} ₴</div>
        </div>
      </div>
      <div class="item-actions-group">
        <button class="edit-btn" onclick="startEditProduct(${prod.id})">Редагувати</button>
        <button class="delete-btn" onclick="deleteProduct(${prod.id})">Видалити</button>
      </div>
    `;
    sellerProductsList.appendChild(item);
  });
}

// Заповнення форми даними для редагування
window.startEditProduct = function(id) {
  const product = products.find(p => p.id === id);
  if (!product) return;

  editProdId.value = product.id;
  document.getElementById('newProdName').value = product.name;
  document.getElementById('newProdPrice').value = product.price;
  document.getElementById('newProdCategory').value = product.categoryId;
  newProdImage.value = product.imageUrl || '';
  formImgPreview.src = product.imageUrl || DEFAULT_PLACEHOLDER;
  document.getElementById('newProdDesc').value = product.description || '';

  productFormTitle.textContent = `Редагування: ${product.name}`;
  productFormSubmitBtn.textContent = 'Зберегти зміни';
  cancelEditBtn.style.display = 'block';

  createProductForm.scrollIntoView({ behavior: 'smooth' });
};

// Скидання форми
function resetProductForm() {
  editProdId.value = '';
  createProductForm.reset();
  formImgPreview.src = DEFAULT_PLACEHOLDER;
  productFormTitle.textContent = 'Додати новий товар';
  productFormSubmitBtn.textContent = 'Опублікувати товар';
  cancelEditBtn.style.display = 'none';
}

cancelEditBtn.addEventListener('click', resetProductForm);

// Створення або оновлення (POST або PUT)
createProductForm.addEventListener('submit', async (e) => {
  e.preventDefault();

  const isEditing = Boolean(editProdId.value);
  const targetId = parseInt(editProdId.value, 10);

  const productData = {
    name: document.getElementById('newProdName').value.trim(),
    description: document.getElementById('newProdDesc').value.trim(),
    price: parseFloat(document.getElementById('newProdPrice').value),
    imageUrl: newProdImage.value.trim() || DEFAULT_PLACEHOLDER,
    categoryId: parseInt(newProdCategory.value, 10),
    userId: currentUser.id
  };

  if (isEditing) {
    productData.id = targetId;
  }

  const endpoint = isEditing ? `${API_BASE_URL}/Products/${targetId}` : `${API_BASE_URL}/Products`;
  const method = isEditing ? 'PUT' : 'POST';

  try {
    productFormSubmitBtn.disabled = true;

    const res = await fetch(endpoint, {
      method: method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(productData)
    });

    if (!res.ok) {
      const err = await res.text();
      throw new Error(err || 'Не вдалося зберегти товар');
    }

    alert(isEditing ? 'Товар успішно оновлено!' : 'Товар успішно додано!');
    resetProductForm();
    await fetchProducts();
  } catch (err) {
    alert(`Помилка: ${err.message}`);
  } finally {
    productFormSubmitBtn.disabled = false;
  }
});

window.deleteProduct = async function(id) {
  if (!confirm('Видалити цей товар з каталогу?')) return;
  try {
    const res = await fetch(`${API_BASE_URL}/Products/${id}`, { method: 'DELETE' });
    if (!res.ok) throw new Error('Помилка видалення');
    await fetchProducts();
  } catch (err) {
    alert(err.message);
  }
};

// ================= ФУНКЦІОНАЛ АДМІНА (ROLE: 2) =================
async function fetchAdminOrders() {
  try {
    const res = await fetch(`${API_BASE_URL}/Orders`);
    if (!res.ok) throw new Error('Не вдалося завантажити замовлення');
    const orders = await res.json();
    renderAdminOrders(orders);
  } catch (err) {
    adminOrdersList.innerHTML = `<p style="color:red">${err.message}</p>`;
  }
}

function renderAdminOrders(orders) {
  if (!orders || !orders.length) {
    adminOrdersList.innerHTML = '<p>Замовлень ще немає.</p>';
    return;
  }

  let tableHtml = `
    <table class="orders-table">
      <thead>
        <tr>
          <th>№</th>
          <th>Товар (ID)</th>
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
    const statusLabels = { 0: 'Очікує', 1: 'Виконано', 2: 'Скасовано' };
    tableHtml += `
      <tr>
        <td>#${order.id}</td>
        <td>Товар #${order.productId}</td>
        <td>Клієнт #${order.buyerId}</td>
        <td>${order.quantity} шт.</td>
        <td>${order.unitPrice ? order.unitPrice * order.quantity : '-'} ₴</td>
        <td><span class="status-badge status-${order.status}">${statusLabels[order.status] || order.status}</span></td>
        <td>
          <select onchange="updateOrderStatus(${order.id}, this.value)">
            <option value="">Змінити...</option>
            <option value="0">Очікує</option>
            <option value="1">Виконано</option>
            <option value="2">Скасовано</option>
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
    const res = await fetch(`${API_BASE_URL}/Orders/${orderId}/status`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(parseInt(newStatus, 10))
    });
    if (!res.ok) throw new Error('Помилка зміни статусу');
    fetchAdminOrders();
  } catch (err) {
    alert(err.message);
  }
};

createCategoryForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  const name = document.getElementById('newCatName').value.trim();
  if (!name) return;

  try {
    const res = await fetch(`${API_BASE_URL}/Categories`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name })
    });
    if (!res.ok) throw new Error('Помилка створення категорії');
    alert('Категорію створено!');
    document.getElementById('newCatName').value = '';
    await fetchCategories();
  } catch (err) {
    alert(err.message);
  }
});

// ================= ОФОРМЛЕННЯ ЗАМОВЛЕННЯ =================
function openCheckout(productId) {
  const product = products.find(p => p.id === productId);
  if (!product) return;

  selectedProductForOrder = product;
  orderImg.src = product.imageUrl || DEFAULT_PLACEHOLDER;
  orderImg.onerror = () => { orderImg.src = DEFAULT_PLACEHOLDER; };
  orderTitle.textContent = product.name;
  orderUnitPrice.textContent = product.price;
  orderQuantity.value = 1;

  updateCheckoutTotal();
  openModal('checkoutModal');
}

function updateCheckoutTotal() {
  if (!selectedProductForOrder) return;
  const qty = parseInt(orderQuantity.value, 10) || 1;
  orderTotalPrice.textContent = (selectedProductForOrder.price * qty).toLocaleString('uk-UA');
}

async function submitOrder() {
  if (!selectedProductForOrder || !currentUser) return;
  const qty = parseInt(orderQuantity.value, 10);
  if (isNaN(qty) || qty <= 0) {
    alert('Вкажіть коректну кількість товару!');
    return;
  }

  const orderData = {
    buyerId: currentUser.id,
    productId: selectedProductForOrder.id,
    quantity: qty
  };

  try {
    confirmOrderBtn.disabled = true;
    confirmOrderBtn.textContent = 'Оформлення...';

    const res = await fetch(`${API_BASE_URL}/Orders`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(orderData)
    });

    if (!res.ok) throw new Error('Помилка сервера при створенні замовлення');

    alert(`Дякуємо, ${currentUser.name}! Замовлення оформлено.`);
    closeModal('checkoutModal');
  } catch (err) {
    alert(err.message);
  } finally {
    confirmOrderBtn.disabled = false;
    confirmOrderBtn.textContent = 'Підтвердити замовлення';
  }
}

// ================= ДОПОМІЖНІ ПОДІЇ =================
function filterByCategory(catId, catName, clickedElement) {
  document.querySelectorAll('.categories-list li').forEach(el => el.classList.remove('active'));
  clickedElement.classList.add('active');
  document.getElementById('catalogTitle').textContent = catName;
  const filtered = products.filter(p => p.categoryId === catId);
  renderProducts(filtered);
}

function handleSearch() {
  const query = searchInput.value.toLowerCase().trim();
  const filtered = products.filter(p => p.name.toLowerCase().includes(query));
  renderProducts(filtered);
}

function openModal(id) {
  document.getElementById(id).classList.add('open');
}

window.closeModal = function(id) {
  document.getElementById(id).classList.remove('open');
};

function setupEventListeners() {
  logoBtn.addEventListener('click', () => {
    if (!currentUser || currentUser.role === 1) {
      document.getElementById('catalogTitle').textContent = 'Всі товари';
      document.querySelectorAll('.categories-list li').forEach(el => el.classList.remove('active'));
      const allLi = document.querySelector('.categories-list li[data-id="all"]');
      if (allLi) allLi.classList.add('active');
      renderProducts(products);
    }
  });

  searchBtn.addEventListener('click', handleSearch);
  searchInput.addEventListener('keyup', (e) => {
    if (e.key === 'Enter') handleSearch();
  });

  userSelect.addEventListener('change', () => {
    const selected = users.find(u => u.id === parseInt(userSelect.value, 10));
    if (selected) loginEmailInput.value = selected.email;
  });

  loginSubmitBtn.addEventListener('click', () => {
    const email = loginEmailInput.value.trim().toLowerCase();
    const user = users.find(u => u.email.toLowerCase() === email);
    if (!user) {
      alert('Користувача з такою поштою не знайдено!');
      return;
    }
    loginUser(user);
  });

  document.getElementById('qtyMinusBtn').addEventListener('click', () => {
    let val = parseInt(orderQuantity.value, 10) || 1;
    if (val > 1) {
      orderQuantity.value = val - 1;
      updateCheckoutTotal();
    }
  });

  document.getElementById('qtyPlusBtn').addEventListener('click', () => {
    let val = parseInt(orderQuantity.value, 10) || 1;
    orderQuantity.value = val + 1;
    updateCheckoutTotal();
  });

  orderQuantity.addEventListener('input', () => {
    let val = parseInt(orderQuantity.value, 10);
    if (val < 1 || isNaN(val)) orderQuantity.value = 1;
    updateCheckoutTotal();
  });

  confirmOrderBtn.addEventListener('click', submitOrder);

  window.addEventListener('click', (e) => {
    if (e.target.classList.contains('modal')) {
      e.target.classList.remove('open');
    }
  });
}