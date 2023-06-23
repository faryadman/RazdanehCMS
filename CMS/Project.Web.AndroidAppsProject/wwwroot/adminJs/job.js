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
    });
    window.location.reload();

}

$('#IsActiveJob').on('change', function () {
        var isActiveJob = $("#IsActiveJob").val();
        // check IsActiveJob for value
        if (isActiveJob === "true") {
            $("#ApiKey").prop("disabled", false);
            $("#Email").prop("disabled", false);
            $("#FailConnectionCount").prop("disabled", false);
            $("#FailConnectionPercent").prop("disabled", false);
            $("#JobPeriodTime").prop("disabled", false);
            $("#JobExpireMinuteTime").prop("disabled", false);
        } else {
            $("#ApiKey").prop("disabled", true);
            $("#Email").prop("disabled", true);
            $("#FailConnectionCount").prop("disabled", true);
            $("#FailConnectionPercent").prop("disabled", true);
            $("#JobPeriodTime").prop("disabled", true);
            $("#JobExpireMinuteTime").prop("disabled", true);
    }
});