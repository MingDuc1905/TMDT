// ponytail: service worker cơ bản — cache static assets
const CACHE_NAME = 'fastship-v2'; // Đổi tên để xóa cache cũ
const STATIC_URLS = [
  '/Source/Home/css/style.css',
  '/Source/Home/css/bootstrap.min.css',
  '/Source/Home/css/layout-sg.css',
  '/Source/Shared/css/fastship-design-tokens.css',
  'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.10.0/css/all.min.css',
  'https://code.jquery.com/jquery-3.4.1.min.js',
  'https://cdn.jsdelivr.net/npm/bootstrap@5.0.0/dist/js/bootstrap.bundle.min.js'
];

self.addEventListener('install', function(event) {
  event.waitUntil(
    caches.open(CACHE_NAME).then(function(cache) {
      return cache.addAll(STATIC_URLS);
    })
  );
  self.skipWaiting();
});

self.addEventListener('activate', function(event) {
  event.waitUntil(
    caches.keys().then(function(names) {
      return Promise.all(
        names.filter(function(n) { return n !== CACHE_NAME; })
          .map(function(n) { return caches.delete(n); })
      );
    })
  );
  self.clients.claim();
});

self.addEventListener('fetch', function(event) {
  // Network-first strategy: Luôn lấy từ mạng để tránh lỗi dính account cũ
  event.respondWith(
    fetch(event.request).catch(function() {
      return caches.match(event.request);
    })
  );
});
