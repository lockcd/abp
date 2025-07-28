document.addEventListener("DOMContentLoaded", function (event) {
    var timer = setTimeout(function () {
        var redirectButton = document.getElementById("redirectButton");
        if (!redirectButton) {
            clearTimeout(timer);
            return;
        }
        var clientName = redirectButton.getAttribute("cname");
        if (!clientName) {
            window.clientName = clientName;
        }
        var href = redirectButton.getAttribute("href");
        if (!href) {
            clearTimeout(timer);
            return;
        }
        window.location = href;
        clearTimeout(timer);
    }, 3000);
});
