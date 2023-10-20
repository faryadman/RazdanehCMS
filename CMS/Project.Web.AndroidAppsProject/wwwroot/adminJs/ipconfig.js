let table = $('#ipListTable').DataTable();

let serversBaseUrl = "/admin/ipconfig";
let formUrl;

function getList() {
    $.ajax({
        type: "GET",
        url: serversBaseUrl + '/list',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            table.clear().draw();
            renderdata(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert("بروز اشکال در اتصال به اینترنت");
        }
    });
}

function renderdata(data) {
    table.clear();
    let isOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        console.log(item)
        let deleteButton;

        if (item.isActive == 1) {
            deleteButton = '<button  class="btn btn-sm btn-danger" onclick="delete(' + item.id + ')" >Delete</button>';
        }
        else {
            deleteButton = '<button  class="btn btn-sm btn-danger" onclick="delete(' + item.id + ')" disabled>Delete</button>';
        }
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';

        table.row.add([
            deleteChekbox,
            (i + 1),
            item.id,
            item.ip,
            '<div>  ' + item.fileName + '  <hr/> ' + item.updatedAtFormatted + ' </div>',
            deleteButton
        ]).node().setAttribute('data-row-id', item.id);

        if (isOdd) {
            table.row(i).nodes().to$().css('background-color', 'rgb(233 233 233)');
        }
        isOdd = !isOdd;
    }
    table.draw();
}

function openModal() {
    loading();
    $('#ipModal').modal();
    swal.close();
}


function deleteip(id) {
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
                url: serversBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    getList();
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

function submitForm() {
    loading();
    let form = document.getElementById('ipForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getList();
            document.getElementById('ipForm').reset();
            $('#ipModal').modal('toggle');
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