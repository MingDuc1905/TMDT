/**
 * FastShip Real-time Order Tracking with Leaflet.js + SignalR
 * Shared module for customer tracking view and shipper live map
 */

var FastShipTracking = window.FastShipTracking || {};

// ─── Initialize Leaflet Map ───
FastShipTracking.initMap = function(elementId, centerLat, centerLng, zoomLevel) {
    if (!document.getElementById(elementId)) return null;
    if (typeof L === 'undefined') {
        console.warn('Leaflet.js not loaded');
        return null;
    }

    var map = L.map(elementId, {
        center: [centerLat || 10.8231, centerLng || 106.6297],
        zoom: zoomLevel || 13,
        zoomControl: true
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        maxZoom: 19
    }).addTo(map);

    map._shipperMarker = null;
    map._restaurantMarker = null;
    map._customerMarker = null;

    return map;
};

// ─── Update Shipper Location on Map ───
FastShipTracking.updateShipperLocation = function(map, lat, lng, label) {
    if (!map) return;

    var icon = L.divIcon({
        html: '<div style="background:#3CB815;width:32px;height:32px;border-radius:50%;display:flex;align-items:center;justify-content:center;border:3px solid #fff;box-shadow:0 2px 8px rgba(0,0,0,.3);font-size:16px;">🚚</div>',
        className: '',
        iconSize: [32, 32],
        iconAnchor: [16, 16]
    });

    if (map._shipperMarker) {
        map._shipperMarker.setLatLng([lat, lng]);
        if (map._shipperMarker._icon) {
            map._shipperMarker._icon.style.transition = 'transform 0.5s ease';
        }
    } else {
        map._shipperMarker = L.marker([lat, lng], { icon: icon })
            .addTo(map)
            .bindPopup(label || '🚚 Shipper đang giao hàng');
    }

    // Auto-follow shipper
    map.setView([lat, lng], 15);
};

// ─── Add Restaurant Marker ───
FastShipTracking.addRestaurantMarker = function(map, lat, lng, name) {
    if (!map || !lat || !lng) return;

    var icon = L.divIcon({
        html: '<div style="background:#e74c3c;width:28px;height:28px;border-radius:50%;display:flex;align-items:center;justify-content:center;border:3px solid #fff;box-shadow:0 2px 8px rgba(0,0,0,.3);font-size:14px;">🏪</div>',
        className: '',
        iconSize: [28, 28],
        iconAnchor: [14, 14]
    });

    if (map._restaurantMarker) {
        map._restaurantMarker.setLatLng([lat, lng]);
    } else {
        map._restaurantMarker = L.marker([lat, lng], { icon: icon })
            .addTo(map)
            .bindPopup('🏪 ' + (name || 'Quán ăn'));
    }
};

// ─── Add Customer Marker ───
FastShipTracking.addCustomerMarker = function(map, lat, lng, name) {
    if (!map || !lat || !lng) return;

    var icon = L.divIcon({
        html: '<div style="background:#3498db;width:28px;height:28px;border-radius:50%;display:flex;align-items:center;justify-content:center;border:3px solid #fff;box-shadow:0 2px 8px rgba(0,0,0,.3);font-size:14px;">📍</div>',
        className: '',
        iconSize: [28, 28],
        iconAnchor: [14, 14]
    });

    if (map._customerMarker) {
        map._customerMarker.setLatLng([lat, lng]);
    } else {
        map._customerMarker = L.marker([lat, lng], { icon: icon })
            .addTo(map)
            .bindPopup('📍 ' + (name || 'Điểm giao hàng'));
    }
};

// ─── SignalR Hub Connection ───
FastShipTracking.createHubConnection = function(orderId, callbacks) {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR not loaded');
        return null;
    }

    var conn = new signalR.HubConnectionBuilder()
        .withUrl('/nhantin')
        .withAutomaticReconnect()
        .build();

    // ── Order status update ──
    if (callbacks.onStatusChanged) {
        conn.on('orderStatusChanged', function(madh, trangthai, timestamp) {
            callbacks.onStatusChanged(madh, trangthai, timestamp);
        });
    }

    // ── Shipper location update ──
    if (callbacks.onLocationUpdate) {
        conn.on('shipperLocationUpdate', function(madh, lat, lng) {
            callbacks.onLocationUpdate(madh, lat, lng);
        });
    }

    // ── Shipper assigned ──
    if (callbacks.onShipperAssigned) {
        conn.on('shipperAssigned', function(madh, shipperName, shipperPhone) {
            callbacks.onShipperAssigned(madh, shipperName, shipperPhone);
        });
    }

    // ── Payment confirmed ──
    if (callbacks.onPaymentConfirmed) {
        conn.on('paymentConfirmed', function(madh, amount) {
            callbacks.onPaymentConfirmed(madh, amount);
        });
    }

    // ── Connection state ──
    conn.onreconnecting(function() {
        console.log('SignalR reconnecting...');
        if (callbacks.onReconnecting) callbacks.onReconnecting();
    });

    conn.onreconnected(function() {
        console.log('SignalR reconnected');
        if (orderId) {
            conn.invoke('JoinOrderGroup', orderId).catch(function(){});
        }
        if (callbacks.onReconnected) callbacks.onReconnected();
    });

    // Start connection
    conn.start()
        .then(function() {
            if (orderId) {
                conn.invoke('JoinOrderGroup', orderId).catch(function(err) {
                    console.error('JoinOrderGroup error:', err);
                });
            }
            if (callbacks.onConnected) callbacks.onConnected(conn);
        })
        .catch(function(err) {
            console.error('SignalR connection failed:', err);
            if (callbacks.onError) callbacks.onError(err);
        });

    return conn;
};

// ─── Order Status Flow (5 steps) ───
FastShipTracking.STATUS_FLOW = [
    { key: 'Đã đặt', label: 'Đã đặt', icon: '📝', step: 0 },
    { key: 'Đã xác nhận', label: 'Xác nhận', icon: '👍', step: 1 },
    { key: 'Đang chuẩn bị', label: 'Chuẩn bị', icon: '👨‍🍳', step: 2 },
    { key: 'Chờ shipper lấy hàng', label: 'Chờ lấy', icon: '🛵', step: 3 },
    { key: 'Đã thanh toán', label: 'Đã TT', icon: '💳', step: 3.5 },
    { key: 'Đã lấy', label: 'Đã lấy', icon: '📦', step: 4 },
    { key: 'Đang giao', label: 'Đang giao', icon: '🚚', step: 5 },
    { key: 'Hoàn thành', label: 'Hoàn thành', icon: '✅', step: 6 }
];

FastShipTracking.getStatusStep = function(status) {
    var found = FastShipTracking.STATUS_FLOW.find(function(s) {
        return s.key === status;
    });
    return found ? found.step : 0;
};

FastShipTracking.getStatusIcon = function(status) {
    var found = FastShipTracking.STATUS_FLOW.find(function(s) {
        return s.key === status;
    });
    return found ? found.icon : '📝';
};

// ─── Render Progress Bar ───
FastShipTracking.renderProgressBar = function(containerId, currentStatus) {
    var container = document.getElementById(containerId);
    if (!container) return;

    var currentStep = FastShipTracking.getStatusStep(currentStatus);
    var html = '<div class="fs-tracking-progress">';

    FastShipTracking.STATUS_FLOW.forEach(function(s, idx) {
        var isCompleted = idx <= currentStep;
        var isCurrent = idx === currentStep;
        var isPending = idx > currentStep;

        html += '<div class="fs-tracking-step ' +
            (isCompleted ? 'completed' : '') +
            (isCurrent ? ' current' : '') +
            (isPending ? ' pending' : '') + '">';

        html += '<div class="fs-tracking-icon">' + s.icon + '</div>';
        html += '<div class="fs-tracking-label">' + s.label + '</div>';

        if (idx < FastShipTracking.STATUS_FLOW.length - 1) {
            html += '<div class="fs-tracking-line ' +
                (isCompleted && !isCurrent ? 'active' : '') + '"></div>';
        }

        html += '</div>';
    });

    html += '</div>';
    container.innerHTML = html;
};

// ─── Update Progress Bar ───
FastShipTracking.updateProgressBar = function(containerId, newStatus) {
    FastShipTracking.renderProgressBar(containerId, newStatus);
};
