﻿// Get the profile icon and dropdown elements
var profileIcon = document.getElementById("profileIcon");
var dropdownMenu = document.getElementById("dropdownMenu");
var profileMenu = document.querySelector(".profile-menu");

// Toggle dropdown when the profile icon is clicked
profileIcon.addEventListener("click", function (event) {
    event.stopPropagation();
    profileMenu.classList.toggle("show");
});

// Close the dropdown if user clicks outside
window.addEventListener("click", function (event) {
    if (!event.target.matches('.profile-icon') && profileMenu.classList.contains('show')) {
        profileMenu.classList.remove('show');
    }
});

// Hàm kiểm tra mật khẩu
function validatePassword() {
    const passwordInput = document.getElementById('password');
    if (!passwordInput) return;

    const password = passwordInput.value.trim();
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@@$#^!%*?&])[A-Za-z\d@@$#^!%*?&]{8,}$/;

    if (!passwordPattern.test(password)) {
        passwordInput.setCustomValidity("Mật khẩu phải dài ít nhất 8 kí tự, bao gồm ít nhất 1 kí tự thường, 1 kí tự hoa, 1 kí tự đặc biệt (@@$#^!%*?&) và 1 kí tự số.");
    } else {
        passwordInput.setCustomValidity(""); // Xóa thông báo lỗi nếu hợp lệ
    }
}
// Hàm kiểm tra số điện thoại
function validatePhoneNumber() {
    const phoneInput = document.getElementById('phonenumber');
    if (!phoneInput) return;

    const phoneNumber = phoneInput.value.trim();
    const phonePattern = /^(0[3,5,7,8,9])\d{8}$/;

    if (!phonePattern.test(phoneNumber)) {
        phoneInput.setCustomValidity("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 03, 05, 07, 08 hoặc 09.");
    } else {
        phoneInput.setCustomValidity(""); // Xóa thông báo lỗi nếu hợp lệ
    }
}

// Hàm kiểm tra email hợp lệ
function validateEmail() {
    const emailInput = document.getElementById('email');
    if (!emailInput) return;

    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    if (!emailPattern.test(emailInput.value.trim())) {
        emailInput.setCustomValidity("Vui lòng nhập đúng định dạng email.");
    } else {
        emailInput.setCustomValidity(""); // Xóa thông báo lỗi nếu hợp lệ
    }
}

// Hàm kiểm tra tên hợp lệ
function validateName() {
    const nameInput = document.getElementById('name');
    if (!nameInput) return;

    const namePattern = /^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠÝàáâãèéêìíòóôõùúăđĩũơưƯẠ-ỹ\s]+$/;

    if (!namePattern.test(nameInput.value.trim())) {
        nameInput.setCustomValidity("Vui lòng nhập tên không chứa chữ số và kí tự đặc biệt.");
    } else {
        nameInput.setCustomValidity(""); // Xóa thông báo lỗi nếu hợp lệ
    }
}

// Hàm kiểm tra đủ ít nhất 15 tuổi
function validateDoB() {
    const dobInput = document.getElementById('dob');
    if (!dobInput) return;

    const dobValue = dobInput.value;

    if (!dobValue) {
        dobInput.setCustomValidity("Vui lòng chọn ngày sinh.");
        return;
    }

    const dobDate = new Date(dobValue);
    const today = new Date();

    // Tính ngày tối thiểu (15 năm trước từ ngày hôm nay)
    const minDate = new Date();
    minDate.setFullYear(today.getFullYear() - 15);

    // Kiểm tra nếu ngày sinh lớn hơn ngày tối thiểu
    if (dobDate > minDate) {
        dobInput.setCustomValidity("Người dùng phải ít nhất 15 tuổi.");
    } else {
        dobInput.setCustomValidity(""); // Xóa thông báo lỗi nếu hợp lệ
    }
}

// Hàm tổng kiểm tra form
function validateForm() {
    validatePhoneNumber();
    validateEmail();
    validateName();
    validateDoB();
    validatePassword();
    return true;
}

const phoneInput = document.getElementById('phonenumber');
if (phoneInput) phoneInput.addEventListener('input', validatePhoneNumber);

const emailInput = document.getElementById('email');
if (emailInput) emailInput.addEventListener('input', validateEmail);

const nameInput = document.getElementById('name');
if (nameInput) nameInput.addEventListener('input', validateName);

const dobInput = document.getElementById('dob');
if (dobInput) dobInput.addEventListener('input', validateDoB);

const passwordInput = document.getElementById('password');
if (passwordInput) passwordInput.addEventListener('input', validatePassword);

function previewImage(event) {
    var input = event.target;
    var imagePreview = document.getElementById("imagePreview");

    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            imagePreview.src = e.target.result;
            imagePreview.style.display = "block";
        };

        reader.readAsDataURL(input.files[0]);
    }
}

const togglePasswordButton = document.getElementById('togglePassword');
if (togglePasswordButton) {
    togglePasswordButton.addEventListener('click', function () {
        const passwordInput = document.getElementById('password');
        const eyeIcon = document.getElementById('eyeIcon');

        // Toggle the type attribute
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);

        // Toggle the eye icon
        eyeIcon.classList.toggle('fa-eye');
        eyeIcon.classList.toggle('fa-eye-slash');
    });
}

const priceInput = document.getElementById("price");
if (priceInput) {
    priceInput.addEventListener("input", function () {
        // Loại bỏ ký tự không phải số
        let sanitizedValue = priceInput.value.replace(/\D/g, "");
        priceInput.value = sanitizedValue;

        // Chuyển đổi giá trị sang số
        const priceNumber = parseInt(sanitizedValue, 10);

        // Kiểm tra điều kiện hợp lệ
        if (!sanitizedValue || isNaN(priceNumber) || priceNumber < 1000 || priceNumber % 100 !== 0) {
            priceInput.setCustomValidity("Giá dịch vụ phải từ 1.000 VND trở lên và là bội số của 1.00 (ví dụ: 1.000, 1.500, 100.300).");
        } else {
            priceInput.setCustomValidity("");
        }
    });
}

const {
    ClassicEditor,
    Essentials,
    Bold,
    Italic,
    Font,
    Paragraph
} = CKEDITOR;
const { FormatPainter } = CKEDITOR_PREMIUM_FEATURES;
const descriptionElement = document.querySelector('#description');
if (descriptionElement) {
    ClassicEditor
        .create(document.querySelector('#description'), {
            licenseKey: 'eyJhbGciOiJFUzI1NiJ9.eyJleHAiOjE3NjU3NTY3OTksImp0aSI6IjhjZWEzMjBhLWY3NmItNDQ5MC04NDZiLWRkNTU0ODE5MmY1YiIsInVzYWdlRW5kcG9pbnQiOiJodHRwczovL3Byb3h5LWV2ZW50LmNrZWRpdG9yLmNvbSIsImRpc3RyaWJ1dGlvbkNoYW5uZWwiOlsiY2xvdWQiLCJkcnVwYWwiXSwiZmVhdHVyZXMiOlsiRFJVUCIsIkJPWCJdLCJ2YyI6ImM0ZDZjZGM2In0.7UVmmWAJvyMrnahcppnFCcAY5y0_CulkL7BqBnxQ-vDclJZPB7c9aX2yUPeSLS-rz-g5cCNUKGF5xl7km4sSfA',
            plugins: [Essentials, Bold, Italic, Font, Paragraph],
            toolbar: [
                'undo', 'redo', '|', 'bold', 'italic', '|',
                'fontSize', 'fontFamily', 'fontColor', 'fontBackgroundColor', '|',
                'formatPainter'
            ]
        })
        .then( /* ... */)
        .catch( /* ... */);
}
