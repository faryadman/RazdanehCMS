let confirmDeleteQuestion = "Are you sure?";
let idsToBeDeleted = [];

toastr.options = {
    "closeButton": true,
    "positionClass": 'toast-top-left',
}


$(function () {
    let url = window.location;
    let element = $('ul.navigation-main a').filter(function () {
        return this.href == url;
    }).parent();//.addClass('active').parent().addClass('in').parent();
    if (element.is('li')) {
        element.addClass('active');
    }
});

function loading() {
    Swal.fire({
        html: '<p>در حال بارگذاری</p><img src="/app-assets/load.gif"/>',
        allowOutsideClick: false,
        showCancelButton: false,
        showConfirmButton: false
    });
}

function isValidNumber(str) {
    if (typeof str != "string") return false // we only process strings!  
    return !isNaN(str) && // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
        !isNaN(parseFloat(str)) // ...and ensure strings of whitespace fail
}

$('table').on('click', '.deleteCheckbox', function () {
    loading();
    var btn = $(this);
    let itemId = btn.attr('data-item-id');
    if (idsToBeDeleted.includes(itemId)) {
        let index = idsToBeDeleted.indexOf(itemId);
        if (index > -1) {
            idsToBeDeleted.splice(index, 1);
        }

    } else {
        idsToBeDeleted.push(itemId);
    }
    console.log(idsToBeDeleted);
    Swal.close();
});

function DeleteSelectedItems(itemBaseUrl,func,args) {
    console.log(itemBaseUrl);
    console.log(idsToBeDeleted);
    if (idsToBeDeleted.length != 0) {
        loading();
        Swal.fire({
            title: '',
            text: confirmDeleteQuestion,
            type: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes',
            cancelButtonText: 'No',
            confirmButtonClass: 'btn btn-primary',
            cancelButtonClass: 'btn btn-danger ml-1',
            buttonsStyling: false,
        }).then(function (result) {
            if (result.value) {
                loading();
                let vm = {
                    ids: idsToBeDeleted.join("_")
                };
                $.ajax({
                    type: "POST",
                    url: itemBaseUrl + '/MassDelete',
                    data: vm,
                    success: function (data) {
                        func(args);
                        data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        let errors = xhr.responseJSON.errors;
                        for (var i = 0; i < errors.length; i++) {
                            toastr.error(errors[i]);
                        }
                        swal.close();
                    }
                })
            }
        });
    } else {
        toastr.error('Please choose an item first');
    }
}