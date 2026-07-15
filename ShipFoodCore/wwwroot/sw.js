// ponytail: service worker cơ b?n — cache static assets, fallback network
const CACHE_NAME = 'fastship-v1';
const STATIC_URLS = [
  '/',
  '/Home',
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
});

self.addEventListener('fetch', function(event) {
  event.respondWith(
    caches.match(event.request).then(function(response) {
      return response || fetch(event.request);
    })
  );
});
