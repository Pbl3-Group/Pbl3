document.addEventListener("DOMContentLoaded", function () {
    const steps = document.querySelectorAll(".form-step");
    const progressBar = document.querySelector(".progress");
    const stepDots = document.querySelectorAll(".step-dot");
    const errorMessage = document.getElementById("error-message");
    let currentStep = 1;

    // Function to update progress bar and step dots
    function updateProgress() {
        const progressPercentage = ((currentStep - 1) / (steps.length - 1)) * 100;
        progressBar.style.width = `${progressPercentage}%`;

        stepDots.forEach((dot, index) => {
            dot.classList.toggle("active", index < currentStep);
        });
    }

    // Function to show the current step
    function showStep(step) {
        steps.forEach((s, index) => {
            s.classList.toggle("active", index + 1 === step);
        });
        updateProgress();
    }

    // Function to validate Step 1
    function validateStep1() {
        const fullName = document.querySelector('input[name="fullName"]').value;
        const phone = document.querySelector('input[name="phone"]').value;
        const dob = document.querySelector('input[name="dob"]').value;
        const email = document.querySelector('input[name="email"]').value;
        const password = document.querySelector('input[name="password"]').value;
        const confirmPassword = document.querySelector('input[name="confirmPassword"]').value;
        const terms = document.querySelector('input[name="terms"]').checked;

        if (!fullName || !phone || !dob || !email || !password || !confirmPassword || !terms) {
            errorMessage.style.display = "block";
            return false;
        }

        if (password !== confirmPassword) {
            errorMessage.textContent = "Mật khẩu không khớp!";
            errorMessage.style.display = "block";
            return false;
        }

        errorMessage.style.display = "none";
        return true;
    }

    // Handle "Đăng ký" button in Step 1
    document.querySelector(".register-step").addEventListener("click", function (e) {
        e.preventDefault();
        if (validateStep1()) {
            currentStep = 2;
            showStep(currentStep);
        }
    });

    // Handle "Next" buttons
    document.querySelectorAll(".next-step").forEach((button) => {
        button.addEventListener("click", function (e) {
            e.preventDefault();
            if (currentStep < steps.length) {
                currentStep++;
                showStep(currentStep);
            }
        });
    });

    // Handle "Previous" buttons
    document.querySelectorAll(".prev-step").forEach((button) => {
        button.addEventListener("click", function (e) {
            e.preventDefault();
            if (currentStep > 1) {
                currentStep--;
                showStep(currentStep);
            }
        });
    });

    // Handle "Skip" links
    document.querySelectorAll(".skip-link").forEach((link) => {
        link.addEventListener("click", function (e) {
            e.preventDefault();
            const skipToStep = parseInt(this.getAttribute("data-skip"));
            currentStep = skipToStep;
            showStep(currentStep);
        });
    });

    // Handle form submission
    document.querySelector(".sign-up-form").addEventListener("submit", function (e) {
        e.preventDefault();

        // Collect schedule data from checkboxes
        const scheduleCheckboxes = document.querySelectorAll('input[name="schedule"]:checked');
        const schedule = Array.from(scheduleCheckboxes).map(checkbox => checkbox.value);

        // Collect other form data
        const formData = {
            fullName: document.querySelector('input[name="fullName"]').value,
            gender: document.querySelector('input[name="gender"]:checked')?.value || "",
            phone: document.querySelector('input[name="phone"]').value,
            dob: document.querySelector('input[name="dob"]').value,
            email: document.querySelector('input[name="email"]').value,
            password: document.querySelector('input[name="password"]').value,
            description: document.querySelector('textarea[name="description"]').value,
            cv: document.querySelector('input[name="cv"]').files[0]?.name || "",
            schedule: schedule,
            city: document.querySelector('select[name="city"]').value,
        };

        console.log("Form Data:", formData);
        alert("Đăng ký thành công!");
    });

    // Toggle password visibility
    document.querySelectorAll(".toggle-password").forEach((icon) => {
        icon.addEventListener("click", function () {
            const targetId = this.getAttribute("data-target");
            const passwordInput = document.getElementById(targetId);
            if (passwordInput.type === "password") {
                passwordInput.type = "text";
                this.classList.remove("fa-eye");
                this.classList.add("fa-eye-slash");
            } else {
                passwordInput.type = "password";
                this.classList.remove("fa-eye-slash");
                this.classList.add("fa-eye");
            }
        });
    });

    // Initial setup
    showStep(currentStep);
});