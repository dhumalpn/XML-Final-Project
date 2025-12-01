// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function () {
    const KEY = 'houseBuddyScrollY';


    const saved = sessionStorage.getItem(KEY);
    if (saved !== null) {
        const y = parseInt(saved, 10);
        if (!Number.isNaN(y)) {
            window.setTimeout(function () {
                window.scrollTo(0, y);
            }, 0);
        }
        sessionStorage.removeItem(KEY);
    }


    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (form && form.classList && form.classList.contains('js-preserve-scroll')) {
            sessionStorage.setItem(KEY, String(window.scrollY || window.pageYOffset || 0));
        }
    });
})();