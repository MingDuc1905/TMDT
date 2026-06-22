// Lấy tất cả các nút radio có tên RadioAddressType
const radioButtons = document.querySelectorAll('input[name="RadioAddressType"]');
const checkoutForm = document.querySelector('.formInfor');
const selectGroup = document.querySelector('.selectGroup');
// Lặp qua mỗi nút radio và thêm sự kiện "change" cho mỗi nút
radioButtons.forEach(radioButton => {
    radioButton.addEventListener('change', function () {
        // Nếu nút radio được chọn, hiển thị giá trị của nó
        if (this.checked) {
            if (this.id == "new") {
                selectGroup.classList.add("hidden");
                checkoutForm.classList.remove("hidden");
            } else if (this.id == "current") {
                checkoutForm.classList.remove("hidden");
                selectGroup.classList.add("hidden");
            } else if (this.id == "available") {
                checkoutForm.classList.add("hidden");
                selectGroup.classList.remove("hidden");
            }
        }
    });
});