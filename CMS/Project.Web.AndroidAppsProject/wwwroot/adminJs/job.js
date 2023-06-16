let groupsBaseUrl = "/admin/job";
let formUrl;

function jobDomain() {
    formUrl = groupsBaseUrl + '/CreateDomainJob';
    $('#jobModal').modal();
}


function submitForm() {
    loading();
    let form = document.getElementById('jobForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            document.getElementById('jobForm').reset();
            $('#jobModal').modal('toggle');
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

$('#IsActiveJob').on('change', function () {
        var isActiveJob = $("#IsActiveJob").val();
        // بررسی مقدار IsActiveJob و اعمال تغییرات
        if (isActiveJob === "true") {
            $("#RunDuringTimeJob").prop("disabled", false);
            $("#FailConnectionCount").prop("disabled", false);
            $("#FailConnectionPercent").prop("disabled", false);
            $("#MinuteCheckTime").prop("disabled", false);
        } else {
            $("#RunDuringTimeJob").prop("disabled", true);
            $("#FailConnectionCount").prop("disabled", true);
            $("#FailConnectionPercent").prop("disabled", true);
            $("#MinuteCheckTime").prop("disabled", true);
        }
});