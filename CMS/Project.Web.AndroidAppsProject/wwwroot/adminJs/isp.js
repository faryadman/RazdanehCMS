let ispsTable = $('#ispsTable').DataTable();

let ispsBaseUrl = "/admin/isp";
let formUrl;

function getisps() {
    $.ajax({
        type: "GET",
        url: ispsBaseUrl + '/List',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            ispsTable.clear().draw();
            renderisps(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });

}

function renderisps(data) {
    let isIrancellCounterOnOdd = true;
    let isMCICounterOnOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let deleteButton = '<button class="btn btn-sm btn-danger" onclick="deleteisps(' + item.id + ')">Delete</button>';
        let addedRow = ispsTable.row.add([
            deleteChekbox,
            item.id,
            item.text,
            item.operatorName,
            item.updatedAtFormatted,
            deleteButton
        ]).node();


        if (isIrancellCounterOnOdd) {
            $(addedRow).css('background-color', '#faffbe');
        } else {
            $(addedRow).css('background-color', 'rgb(252 255 219)');
        }

        if (item.operator == 1) {
            if (isMCICounterOnOdd) {
                $(addedRow).css('background-color', '#b4f3f5');

            } else {

                $(addedRow).css('background-color', 'rgb(228 253 254)');
            }
        }

        isIrancellCounterOnOdd = !isIrancellCounterOnOdd;
        isMCICounterOnOdd = !isMCICounterOnOdd;

        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).attr('id', item.id);
        $(addedRow).addClass('isp');
        ispsTable.rows.add(addedRow).draw();

    }
}

function newisp() {
    formUrl = ispsBaseUrl + '/Create';
    $('#ispsModal').modal();
}

function submitForm() {
    loading();
    let form = document.getElementById('ispForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            if (window.location.pathname == '/admin/isps') {
                getisps(false);
            } else {
                getisps(true);
            }
            document.getElementById('ispForm').reset();
            $('#ispsModal').modal('toggle');
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

function deleteisps(id) {
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
                url: ispsBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    if (window.location.pathname == '/admin/isps') {
                        getisps(false);
                    } else {
                        getisps(true);
                    }
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}