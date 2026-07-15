namespace ShipFood.Helpers;

/// <summary>
/// Hardcoded constants cho FastShip (fix P9.2 — tránh magic numbers trong code)
/// </summary>
public static class FastShipConstants
{
    /// <summary>Phí ship cố định: 15,000đ / đơn</summary>
    public const decimal SHIP_FEE = 15000m;

    /// <summary>Giá trị đơn hàng tối thiểu được free ship: 200,000đ</summary>
    public const decimal FREE_SHIP_THRESHOLD = 200_000m;

    /// <summary>S? ti?n n?p t?i thi?u vào ví: 10,000d</summary>
    public const decimal MIN_DEPOSIT = 10_000m;

    /// <summary>S? ti?n n?p t?i da vào ví: 100,000,000d</summary>
    public const decimal MAX_DEPOSIT = 100_000_000m;

    /// <summary>S? ti?n rút t?i thi?u: 10,000d</summary>
    public const decimal MIN_WITHDRAW = 10_000m;

    /// <summary>Th?i gian ch? t? ??ng xác nh?n chuy?n kho?n (phút)</summary>
    public const int BANK_VERIFY_TIMEOUT_MINUTES = 15;

    /// <summary>Th?i gian lock ch?ng t?o don trùng (giây)</summary>
    public const int ORDER_IDEMPOTENCY_SECONDS = 30;

    /// <summary>S? lu?ng t?i da quick replies cho chatbot</summary>
    public const int MAX_QUICK_REPLIES = 4;

    /// <summary>Gi?i h?n tin nh?n trong l?ch s? chat bot</summary>
    public const int MAX_CHAT_HISTORY = 20;
}
