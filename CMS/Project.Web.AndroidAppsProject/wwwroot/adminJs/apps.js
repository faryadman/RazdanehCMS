let appsTable = $('#appsTable').DataTable();

let appsBaseUrl = "/admin/apps";
let formUrl;
let appGroups = [];

function getapps() {
    $.ajax({
        type: "POST",
        url: appsBaseUrl + '/List',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            appsTable.clear().draw();
            renderapps(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });

}

function renderapps(data) {
    let isOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let groupsButton = '<button class="btn btn-sm btn-dark getGroups" data-item-id="' + item.id + '">Groups</button>';
        let serversButton = '<a class="btn btn-sm btn-primary" href="/admin/servers?appid=' + item.id + '">Servers</a>';
        let adserversButton = '<a class="btn btn-sm btn-info" href="/admin/servers/adindex?appid=' + item.id + '">AdServers</a>';
        let editButton = '<a class="btn btn-sm btn-warning" href="/admin/apps/edit?id=' + item.id + '">Edit</a>';
        let deleteButton = '<button class="btn btn-sm btn-danger" onclick="deleteapps(' + item.id + ')">Delete</button>';
        let addedRow = appsTable.row.add([
            deleteChekbox,
            item.id,
            item.appTitle,
            item.apiRoute,
            item.storeVersion,
            item.updatedAtFormatted,
            groupsButton,
            serversButton,
            adserversButton,
            editButton,
            deleteButton
        ]).node();

        if (isOdd)
            $(addedRow).css('background-color', 'rgb(233 233 233)');


        isOdd = !isOdd;

        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).attr('id', item.id);
        $(addedRow).addClass('app');
        appsTable.rows.add(addedRow).draw();

    }
}

function newapp() {
    $('#parentCaegoryArea').css('display', 'block');
    formUrl = appsBaseUrl + '/Create';
    $('#appsModal').modal();
}

function submitForm() {
    loading();
    let form = document.getElementById('appForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getapps();
            document.getElementById('appForm').reset();
            $('#appsModal').modal('toggle');
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

function deleteapps(id) {
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
                url: appsBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    getapps();
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

$("#appsTable_previous").on('click', function () {
    alert();
})

$('#editForm').submit(function (e) {
    e.preventDefault();
    loading();
    let form = $(this);

    $.ajax({
        url: appsBaseUrl + "/edit",
        method: "POST",
        data: form.serialize(),
        success: function (data) {
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            let errors = xhr.responseJSON.errors;
            for (var i = 0; i < errors.length; i++) {
                toastr.error(errors[i]);
            }
            swal.close();
        }
    });
});

$('#appsTable').on('click', '.getGroups', function () {
    loading();
    appGroups = [];
    var btn = $(this);
    $('#itemId').val(btn.attr('data-item-id'));
    $.ajax({
        url: appsBaseUrl + "/Groups?id=" + btn.attr('data-item-id'),
        method: 'GET',
        success: function (data) {
            $('#appgroupsArea').empty();
            $('#appAdgroupsArea').empty();

            for (var i = 0; i < data.length; i++) {
                let item = data[i];
                let area = item.isAd ? 'appAdgroupsArea' : 'appgroupsArea';
                if (item.doTheyHaveRelation) {
                    $('#' + area).append('<div onclick="checkGroup(' + item.id + ')" class="col-3 mt-3"><input checked value=' + item.id + ' class="groupCheckbox" type ="checkbox" /><span>' + item.title + '</span></div>');
                    appGroups.push(item.id);
                } else {
                    $('#' + area).append('<div onclick="checkGroup(' + item.id + ')" class="col-3 mt-3"><input value=' + item.id + ' type ="checkbox" class="groupCheckbox" /><span>' + item.title + '</span></div>');
                }
            }

            $("#groupsbyappModal").modal();
            swal.close();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            let errors = xhr.responseJSON.errors;
            for (var i = 0; i < errors.length; i++) {
                toastr.error(errors[i]);
            }
            swal.close();
        }
    })
});

function checkGroup(id) {
    loading();
    if (appGroups.includes(id)) {
        let index = appGroups.indexOf(id);
        if (index > -1) {
            appGroups.splice(index, 1);
        }

    } else {
        appGroups.push(id);
    }
    swal.close();
}

function submitAppGroups() {
    loading();
    let model = {
        id: $('#itemId').val(),
        groupIds: appGroups.length == 0 ? "_" : appGroups.join("_")
    }
    $.ajax({
        url: appsBaseUrl + '/updateAppGroups',
        method: 'Post',
        data: model,
        success: function (data) {
            appGroups = [];
            $('#groupsbyappModal').modal('toggle');
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            let errors = xhr.responseJSON.errors;
            for (var i = 0; i < errors.length; i++) {
                toastr.error(errors[i]);
            }
            swal.close();
        }
    });
}