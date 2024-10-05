let domainsTable = $('#domainsTable').DataTable();

let serversBaseUrl = "/admin/domains";
let formUrl;

function getdomains(filter) {
    $.ajax({
        type: "GET",
        url: serversBaseUrl + '/list?filter=1',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            domainsTable.clear().draw();
            renderdomains(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert("بروز اشکال در اتصال به اینترنت");
        }
    });
}

function renderdomains(data) {
    domainsTable.clear();
    let isOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        console.log(item)
        let deleteButton;

        if (item.isActive == 1) {
            deleteButton = '<button  class="btn btn-sm btn-danger" onclick="deletedomain(' + item.id + ')" >Delete</button>';
        }
        else {
            deleteButton = '<button  class="btn btn-sm btn-danger" onclick="deletedomain(' + item.id + ')" disabled>Delete</button>';
        }
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';

        domainsTable.row.add([
            deleteChekbox,
            (i + 1),
            item.id,
            item.domainName,
            item.zoneId,
            '<div>  ' + item.fileName + '  <hr/> ' + item.updatedAtFormatted + ' </div>',
        ]).node().setAttribute('data-row-id', item.id);

        if (isOdd) {
            domainsTable.row(i).nodes().to$().css('background-color', 'rgb(233 233 233)');
        }
        isOdd = !isOdd;
    }
    domainsTable.draw();
}

function newDomain() {
    loading();
    formUrl = serversBaseUrl + '/CreateDomain';
    $('#groupSelectListArea').css('display', 'block');
    $('#domainModal').modal();
    swal.close();
}
function checkZoneId() {
    loading();
    $.ajax({
        type: "GET",
        url: serversBaseUrl + '/checkZoneId',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            domainsTable.clear().draw();
            renderdomains(true);
            swal.close();
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert("بروز اشکال در اتصال به اینترنت");
        }
    });
}

function deletedomain(id) {
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
                id: id
            };
            $.ajax({
                type: "POST",
                url: serversBaseUrl + '/DeleteDomain',
                data: vm,
                success: function (data) {
                    if (window.location.pathname.toLowerCase() == '/admin/domains'.toLowerCase()) {
                        getdomains(false);
                    } else {
                        getdomains(true);
                    }
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

function submitDomainForm() {
    loading();
    let form = document.getElementById('domainForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            if (window.location.pathname.toLowerCase() == '/admin/domain'.toLowerCase()) {
                getdomains(false);
            } else {
                getdomains(true);
            }
            document.getElementById('domainForm').reset();
            $('#domainModal').modal('toggle');
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

function uploadFile() {
    $(document).ready(function () {
        $('#uploadForm').submit(function (e) {
            e.preventDefault(); // اجرای عملیات آپلود از طریق JavaScript

            var formData = new FormData();
            var fileInput = $('#fileInput')[0].files[0];

            formData.append('file', fileInput);

            $.ajax({
                url: serversBaseUrl + '/Upload/Upload',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    // پاسخ دریافتی از سرور
                    alert(response);
                },
                error: function (error) {
                    // خطا در آپلود فایل
                    alert('Error: ' + error.responseText);
                }
            });
        });
    });
}

function filterservers() {
    loading();
    $('#filterdomainsModal').modal();
    swal.close();
}

function getInactiveDomains() {
    $.ajax({
        type: "GET",
        url: serversBaseUrl + '/listInactive',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            renderInactiveDomains(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert("بروز اشکال در اتصال به اینترنت");
        }
    });
}

function renderInactiveDomains(data) {
    let inactiveDomainsList = $('#inactiveDomainsList');
    inactiveDomainsList.empty();
    let domainNames = [];
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        inactiveDomainsList.append('<li>' + item.domainName + '</li>');
        domainNames.push(item.domainName);
    }
    inactiveDomainsList.wrap('<ol></ol>');

    // تبدیل آرایه نام دامنه‌ها به رشته JSON
    let jsonString = JSON.stringify(domainNames);

    // کپی کردن رشته JSON به کلیپ بورد
    copyToClipboard(jsonString);

    $('#inactiveDomainsModal').modal();
}

function copyToClipboard(text) {
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.style.position = 'fixed';
    document.body.appendChild(textarea);
    textarea.focus();
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
}

function deleteInactiveDomain() {
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

            $.ajax({
                type: "POST",
                url: serversBaseUrl + '/DeleteInactiveDomain',
                success: function (data) {
                    if (window.location.pathname.toLowerCase() == '/admin/domains'.toLowerCase()) {
                        getdomains(false);
                    } else {
                        getdomains(true);
                    }
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                    setTimeout(function () {
                        swal.close();
                        $('#filterdomainsModal').modal('toggle');
                    }, 4000)
                }
            })
        }
    });
}


function filter() {
    loading();
    let result = $('#filterInput').val();
    if (window.location.pathname.toLowerCase() == '/admin/domains'.toLowerCase()) {
        getdomains(result);
    } else {
        getdomains(result);
    }
    setTimeout(function () {
        swal.close();
        $('#filterdomainsModal').modal('toggle');
    }, 4000);
}
