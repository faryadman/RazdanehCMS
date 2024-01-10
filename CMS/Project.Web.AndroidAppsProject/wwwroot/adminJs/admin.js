let confirmDeleteQuestion = "Are you sure?";
let confirmRefreshQuestion = "Are you sure for refresh?";
let idsToBeDeleted = [];
let idsToBeRefreshed = [];

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
        html: '<p>در حال بارگذاری</p><br/><p>درخواست به سمت سرورهای کلودفلر ممکن است یک تا سه دقیقه به طول بیانجامد</p><img src="/app-assets/load.gif"/>',
        allowOutsideClick: false,
        showCancelButton: false,
        showConfirmButton: false
    });
}
function loadingTasks(totalTasks) {
    Swal.fire({
        title: 'در حال بارگذاری',
        html: '<p id="progress-text">درخواست به سمت سرورهای کلودفلر ممکن است یک تا سه دقیقه به طول بیانجامد</p><img id="loading-image" src="/app-assets/load.gif"/>',
        allowOutsideClick: false,
        showCancelButton: false,
        showConfirmButton: false,
        didOpen: async () => {
            let completedTasks = 0;
            const progressText = document.getElementById('progress-text');
            const loadingImage = document.getElementById('loading-image');

            if (!progressText || !loadingImage) {
                console.error('Element not found!');
                return;
            }

            const progressInterval = setInterval(() => {
                const progress = (completedTasks / totalTasks) * 100;
                progressText.innerHTML = `%در حال بارگذاری  - ${Math.round(progress)}`;
  
                if (completedTasks === totalTasks) {
                    clearInterval(progressInterval);
                }
            }, 5000);

            const tasks = Array.from({ length: totalTasks }, (_, index) => {
                return new Promise((resolve, reject) => {
                    // Mocking an asynchronous task
                    setTimeout(() => {
                        // Assume the task is successful
                        completedTasks++;
                        resolve();
                    }, 5000 * index);
                });
            });

            try {
                await Promise.all(tasks);
            } catch (error) {
                // Handle errors if any of the tasks fail
                console.error(error);
            } finally {
                // Close loading once all tasks are complete
                clearInterval(progressInterval);
                Swal.close();
            }
        }
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
            idsToBeRefreshed.splice(index, 1);
        }

    } else {
        idsToBeDeleted.push(itemId);
        idsToBeRefreshed.push(itemId);
    }
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

function RefreshSubDomainSelectedItems(itemBaseUrl, func, args) {
    if (idsToBeRefreshed.length != 0) {
        loading();
        Swal.fire({
            title: '',
            text: confirmRefreshQuestion,
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
                    ids: idsToBeRefreshed.join("_")
                };
                $.ajax({
                    type: "POST",
                    url: itemBaseUrl + '/RefreshSubdomain',
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

function RefreshDomainSelectedItems(itemBaseUrl, func, args) {
    if (idsToBeRefreshed.length != 0) {
        // Call the loading function with the total number of tasks
        loadingTasks(idsToBeRefreshed.length);
        Swal.fire({
            title: '',
            text: confirmRefreshQuestion,
            type: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes',
            cancelButtonText: 'No',
            confirmButtonClass: 'btn btn-primary',
            cancelButtonClass: 'btn btn-danger ml-1',
            buttonsStyling: false,
        }).then(function (result) {
            if (result.value) {
                loadingTasks();
                let vm = {
                    ids: idsToBeRefreshed.join("_")
                };
                $.ajax({
                    type: "POST",
                    url: itemBaseUrl + '/RefreshDomain',
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