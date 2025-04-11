function nextStep(currentStep) {
    // Ẩn form hiện tại
    document.getElementById('form-step' + currentStep).classList.remove('active');
    
    // Hiện form tiếp theo
    document.getElementById('form-step' + (currentStep + 1)).classList.add('active');
    
    // Cập nhật trạng thái các step
    document.getElementById('step' + currentStep).classList.remove('active');
    document.getElementById('step' + currentStep).classList.add('completed');
    document.getElementById('step' + (currentStep + 1)).classList.add('active');
}

function prevStep(currentStep) {
    // Ẩn form hiện tại
    document.getElementById('form-step' + currentStep).classList.remove('active');
    
    // Hiện form trước đó
    document.getElementById('form-step' + (currentStep - 1)).classList.add('active');
    
    // Cập nhật trạng thái các step
    document.getElementById('step' + currentStep).classList.remove('active');
    document.getElementById('step' + (currentStep - 1)).classList.remove('completed');
    document.getElementById('step' + (currentStep - 1)).classList.add('active');
}

// Xử lý chọn thành phố
function toggleCityDropdown() {
    document.getElementById("cityDropdown").style.display = document.getElementById("cityDropdown").style.display === "block" ? "none" : "block";
}

function selectCity(city) {
    document.getElementById("city").value = city;
    document.getElementById("cityDropdown").style.display = "none";
}

// Đóng dropdown khi click ra ngoài
window.onclick = function(event) {
    if (!event.target.matches('#city')) {
        document.getElementById("cityDropdown").style.display = "none";
    }
}

// Xử lý chọn time slot
const timeSlots = document.querySelectorAll('.time-slot');
timeSlots.forEach(slot => {
    slot.addEventListener('click', function() {
        this.classList.toggle('selected');
    });
});

// Xử lý upload CV
document.querySelector('.upload-cv').addEventListener('click', function() {
    document.getElementById('cv-upload').click();
});

document.getElementById('cv-upload').addEventListener('change', function() {
    if (this.files.length > 0) {
        const fileName = this.files[0].name;
        document.querySelector('.upload-cv h3').textContent = fileName;
    }
});