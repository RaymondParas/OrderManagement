const userField = document.getElementById("userField");
const passField = document.getElementById("passField");
const button = document.querySelector("#loginButton button");
const url = button.dataset.url;

const navigateToDashboard = () => window.location.href = url;

const clearAndGenerateCookie = (data) => {
    let cookies = document.cookie;
    if (cookies)
        cookies.split(";").forEach(function (c) { document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/"); });
    document.cookie = 'access_token=' + data;
}

const shakeAndClearInputs = () => {
    userField.classList.add("errorInput");
    passField.classList.add("errorInput");

    setTimeout(() => {
        userField.classList.remove("errorInput");
        passField.classList.remove("errorInput");
    }, 300);

    userField.value = "";
    passField.value = "";
}

const login = () => {
    let credentials = { username: userField.value, password: passField.value };
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(credentials),
        url: "/api/Token",
        success: (data) => {
            clearAndGenerateCookie(data);
            navigateToDashboard();
        },
        error: (result) => {
            console.log(result);
            shakeAndClearInputs();
        }
    })
}

button.addEventListener('click', () => {
    console.log("Page Login Called");
    login();
}, false);