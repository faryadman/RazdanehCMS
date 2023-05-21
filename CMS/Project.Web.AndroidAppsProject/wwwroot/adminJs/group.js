let groupsTable = $('#groupsTable').DataTable();

let groupsBaseUrl = "/admin/groups";
let formUrl;

function getgroups(isAd) {
    $.ajax({
        type: "GET",
        url: groupsBaseUrl + '/List?isAd=' + isAd,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            groupsTable.clear().draw();
            rendergroups(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });

}

function rendergroups(data) {
    let counter = 1;
    let isOdd = true;

    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        let serverUrl = !item.isAd ? '/servers' : '/servers/adIndex';
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let serversButton = '<a class="btn btn-sm btn-primary" href="/admin' + serverUrl + '?groupId=' + item.id + '">Servers</a>';
        let editButton = '<button class="btn btn-sm btn-warning edit" data-item-id="' + item.id + '" data-item-title="' + item.title + '">edit</button>';
        let deleteButton = '<button class="btn btn-sm btn-danger" onclick="deletegroups(' + item.id + ')">Delete</button>';
        let addedRow = groupsTable.row.add([
            deleteChekbox,
            counter,
            item.title,
            item.updatedAtFormatted,
            serversButton,
            editButton,
            deleteButton
        ]).node();
        counter++;

        if (isOdd)
            $(addedRow).css('background-color', 'white');

        isOdd = !isOdd;

        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).attr('id', item.id);
        $(addedRow).addClass('group');
        groupsTable.rows.add(addedRow).draw();

    }
}

function newgroup() {
    formUrl = groupsBaseUrl + '/Create';
    $('#groupsModal').modal();
}

function submitForm() {
    loading();
    let form = document.getElementById('groupForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            if (window.location.pathname == '/admin/groups') {
                getgroups(false);
            } else {
                getgroups(true);
            }
            document.getElementById('groupForm').reset();
            $('#groupsModal').modal('toggle');
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

function submitAdForm() {
    loading();
    let form = document.getElementById('groupForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl + "Ad",
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getgroups(true);
            document.getElementById('groupForm').reset();
            $('#groupsModal').modal('toggle');
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

function deletegroups(id) {
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
                url: groupsBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    if (window.location.pathname == '/admin/groups') {
                        getgroups(false);
                    } else {
                        getgroups(true);
                    }
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

$('#groupsTable').on('click', '.edit', function () {
    loading();
    let item = $(this);
    $('#itemId').val(item.attr('data-item-id'));
    $('#Title').val(item.attr('data-item-title'));
    formUrl = groupsBaseUrl + '/Edit';
    $('#groupsModal').modal();
    Swal.close();
})