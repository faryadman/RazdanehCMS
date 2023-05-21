let blackListsTable = $('#blackListsTable').DataTable();

let blackListsBaseUrl = "/admin/servers/blackList";

function getblackLists() {
    let serverId = $('#selectedserverId').val();
    $.ajax({
        type: "GET",
        url: blackListsBaseUrl + '/getData?id=' + serverId,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            blackListsTable.clear().draw();
            renderblackLists(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });

}

function renderblackLists(data) {
    let counter = 1;
    let isOdd = true;

    for (var i = 0; i < data.length; i++) {
        let item = data[i];

        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let deleteButton = '<button class="btn btn-sm btn-danger" onclick="deleteblackLists(' + item.id + ')">Delete</button>';

        let addedRow = blackListsTable.row.add([
            deleteChekbox,
            counter,
            '<span class="badge badge-dark">' + item.server.serverName + '</span>',
            item.ip,
            item.updatedAtFormatted,
            deleteButton
        ]).node();

        if (isOdd)
            $(addedRow).css('background-color', 'rgb(233 233 233)');

        isOdd = !isOdd;

        counter++;
        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).attr('id', item.id);
        $(addedRow).addClass('blackList');
        blackListsTable.rows.add(addedRow).draw();

    }
}

function deleteblackLists(id) {
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
                blackListId: id
            };
            $.ajax({
                type: "POST",
                url: blackListsBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    getblackLists();
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

function newblackList() {
    loading();
    formUrl = blackListsBaseUrl + '/Create';
    $('#blackListModal').modal();
    swal.close();
}

function submitForm() {
    loading();
    let form = document.getElementById('blackListForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getblackLists();
            document.getElementById('blackListForm').reset();
            $('#blackListModal').modal('toggle');
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
