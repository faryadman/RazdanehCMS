let ipTable = $('#ipTable').DataTable();

let ipBaseUrl = "/api";
let formUrl;

function getip() {
    $.ajax({
        type: "GET",
        url: ipBaseUrl + '/ListIp',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            ipTable.clear().draw();
            renderisps(result.data);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });

}
function renderisps(data) {

    for (var i = 0; i < data.length; i++) {
        let item = data[i];
      
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let deleteButton = '<button class="btn btn-sm btn-danger" onclick="deleteip(' + item.id + ')">Delete</button>';
        let addedRow = ipTable.row.add([
            deleteChekbox,
            item.ip,
            item.updatedAtFormatted,
            item.tcp,
            deleteButton
        ]).node();


        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).addClass('id');
        $(addedRow).addClass('ip');
        $(addedRow).addClass('tcp');
        ipTable.rows.add(addedRow).draw();

    }
}
function deleteip(id) {
    console.log(id);
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
                url: '/admin/ip/DeleteIp',
                data: vm,
                success: function (data) {
                    if (window.location.pathname == '/admin/ip') {
                        getip(false);
                    } else {
                        getip(true);
                    }
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}