let domainsTable = $('#domainsTable').DataTable();

let serversBaseUrl = "/admin/domains";
let formUrl;

function getdomains() {
    $.ajax({
        type: "GET",
        url: serversBaseUrl + '/list',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            domainsTable.clear().draw();
            renderdomains(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });

}

function renderdomains(data) {
    let isOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];


        let deleteButton = '<button class="btn btn-sm btn-danger    " onclick="deletedomain(' + item.id + ')">Delete</button>';

        let addedRow = domainsTable.row.add([
            item.id,
            domainName,
            domainIP,
            domainType,
            deleteButton
        ]).node();

        if (isOdd)
            $(addedRow).css('background-color', 'rgb(233 233 233)');

        isOdd = !isOdd;

        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).attr('id', item.id);
        $(addedRow).addClass('domain');
        domainsTable.rows.add(addedRow).draw();
    }
}


function newDomain() {
    loading();
    formUrl = serversBaseUrl + '/CreateDomain';
    $('#groupSelectListArea').css('display', 'block');
    $('#domainModal').modal();
    swal.close();
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