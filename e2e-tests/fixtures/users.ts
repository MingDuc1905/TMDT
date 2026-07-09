/**
 * 🧪 Test Users — Thông tin tài khoản dùng cho E2E Testing
 * 
 * Các tài khoản này được seed từ file seed.sql
 * Mật khẩu plain-text, login qua form /Home/Login
 */

export const USERS = {
  /** Khách hàng thường */
  customer1: { username: 'tranthib', password: 'abcdef', name: 'Trần Thị B' },
  customer2: { username: 'levanc',   password: 'qwerty', name: 'Lê Văn C' },
  customer3: { username: 'phamthid', password: 'xyz123', name: 'Phạm Thị D' },

  /** Quán ăn (Restaurant) */
  restaurant1: { username: 'konekopizza',    password: 'konekopizza',    name: 'Koneko Pizza' },
  restaurant2: { username: 'com1990nvs',     password: 'com1990nvs',     name: 'Cơm 1990' },
  restaurant3: { username: 'bundaugiadi',    password: 'bundaugiadi',    name: 'Bún Đậu Gia Di' },

  /** Shipper */
  shipper1: { username: 'shippery', password: 'shipy456', name: 'Shipper Y' },
  shipper2: { username: 'shipperz', password: 'shipz789', name: 'Shipper Z' },

  /** Admin */
  admin1: { username: 'admin1', password: 'admin1', name: 'Admin 1' },
  admin2: { username: 'admin2', password: 'admin2', name: 'Admin 2' },
};

/** Seed data IDs — các mã ID từ seed.sql */
export const SEED = {
  restaurantIds: {
    konekoPizza: 6,
    com1990: 7,
    bunDauGiaDi: 8,
    chayAnLacTam: 9,
    chanGaNuong: 10,
    traLong: 11,
    bunMamBaDong: 12,
    dangHoang: 13,
    sushiTotoro: 14,
    bakery43: 15,
  },
  /** Mã user IDs từ seed */
  userIds: {
    customer1: 2,   // tranthib
    customer2: 3,   // levanc
    shipper1: 5,    // shippery
    admin1: 16,     // admin1
  },
};

/** URL paths */
export const URLS = {
  home: '/',
  login: '/Home/Login',
  signup: '/Home/Signup',
  cart: '/Cart',
  checkout: '/Cart/Checkout',
  orderHistory: '/Cart/LichSuDatHang',
  orderTracking: '/Cart/OrderTracking',
  restaurant: '/Restaurant',
  restaurantOrderList: '/Restaurant/OrderList',
  shipper: '/Shipper',
  shipperIncome: '/Shipper/ThuNhap',
  shipperWallet: '/Shipper/ViTien',
  shipperHistory: '/Shipper/LichSu',
  admin: '/Admin',
  adminDashboard: '/Admin/Dashboard',
  adminUserMgmt: '/Admin/QuanLyKhachHang',
  adminOrderMgmt: '/Admin/Order',
  adminCategoryMgmt: '/Admin/Category',
  dbDebug: '/Home/DbDebug',
  health: '/health',
};

/** Shipping info mẫu cho checkout */
export const SHIPPING = {
  name: 'Nguyễn Văn A',
  phone: '0912345678',
  address: '48 Cao Thắng, Phường 2',
  district: 'Quận 3',
  note: 'Giao giờ hành chính',
};

/** Sai credentials dùng cho negative test */
export const INVALID_CREDENTIALS = {
  wrongPassword: { username: 'tranthib', password: 'sai_mat_khau_123' },
  nonExistent: { username: 'khongton_tai', password: 'abcxyz' },
};
