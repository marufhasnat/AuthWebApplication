$('#rememberMe, #btnLogin').click(function () {
    createCookie();
});

function createCookie() {
    let email = $('#email').val();
    let password = $('#password').val();
    let rememberMe = $('#rememberMe').is(':checked');

    let date = new Date();
    let ckTillDate = new Date(date.setDate(date.getDate() + 30));
    ckTillDate.setHours(ckTillDate.getHours() + 6); // Add 6 hours to GMT 

    if (rememberMe) {
        document.cookie = 'username' + '=' + btoa(email) + ';expires=' + ckTillDate;
        document.cookie = 'password' + '=' + btoa(password) + ';expires=' + ckTillDate;
    } else {
        document.cookie = 'username=; + expires=;';
        document.cookie = 'password=; + expires=;';
    }
}

function GetCookie(name) {
    let cookie = document.cookie.split(';');
    for (var i = 0; i < cookie.length; i++) {
        let ck = cookie[i].trim();
        if (ck.startsWith(name + '=')) {
            return ck.substring(name.length + 1);
        }
    }
    return null;
}

window.onload = function () {
    let email = GetCookie('username');
    let password = GetCookie('password');
    if (email && password) {
        $('#email').val(atob(email)).trigger('focus').trigger('blur');
        $('#password').val(atob(password)).trigger('focus').trigger('blur');
        $('#rememberMe').prop('checked', true);
    }
};
