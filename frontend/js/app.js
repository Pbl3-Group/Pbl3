const sign_in_btn = document.querySelector("#sign-in-btn");
const sign_up_btn = document.querySelector("#sign-up-btn");
const container = document.querySelector(".container");

sign_up_btn.addEventListener("click", () => {
  container.classList.add("sign-up-mode");
});

sign_in_btn.addEventListener("click", () => {
  container.classList.remove("sign-up-mode");
});

document.getElementById("loginBtn").addEventListener("click", function () {
    window.location.href = "../pages/login.html";
});

document.addEventListener("DOMContentLoaded", function () {
    const container = document.querySelector(".container");
    const signUpBtn = document.querySelector("#sign-up-btn");
    const signInBtn = document.querySelector("#sign-in-btn");

    signUpBtn.addEventListener("click", () => {
        container.classList.add("sign-up-mode");
    });

    signInBtn.addEventListener("click", () => {
        container.classList.remove("sign-up-mode");
    });
});



