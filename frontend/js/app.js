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

document.addEventListener('DOMContentLoaded', function () {
  const signInBtn = document.getElementById('sign-in-btn');
  const signUpBtn = document.getElementById('sign-up-btn');
  const container = document.querySelector('.container');

  signInBtn.addEventListener('click', () => {
    container.classList.remove('sign-up-mode');
    container.classList.add('sign-in-mode');
  });

  signUpBtn.addEventListener('click', () => {
    container.classList.remove('sign-in-mode');
    container.classList.add('sign-up-mode');
  });
});



