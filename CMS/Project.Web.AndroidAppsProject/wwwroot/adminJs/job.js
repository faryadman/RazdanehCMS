let jobBaseUrl = "/admin/job";
let formUrl;

function jobDomain() {
    formUrl = jobBaseUrl + '/CreateDomainJob';
    var modal = $('#jobModal');
    modal.on('show.bs.modal', function (event) {
        $.ajax({
            url: '/admin/job/getJobDomainData',
            method: 'GET',
            success: function (data) {
                modal.find('#IsActiveJob').prop('checked', data.isActiveJob);
                modal.find('#ApiKey').val(data.apiKey);
                modal.find('#Email').val(data.email);
                modal.find('#JobPeriodTime').val(data.jobPeriodTime);
                modal.find('#JobExpireMinuteTime').val(data.jobExpireMinuteTime);
                modal.find('#FailConnectionCount').val(data.failConnectionCount);
                modal.find('#FailConnectionPercent').val(data.failConnectionPercent);
            },
            error: function (xhr, ajaxOptions, thrownError) {
              
                swal.close();
            }
        });
    });
    modal.modal();
}
function jobSubDomain() {
    formUrl = jobBaseUrl + '/CreateSubDomainJob';
    var modal = $('#jobSubdomainModal');
    modal.on('show.bs.modal', function (event) {
        var modal = $(this);
        $.ajax({
            url: '/admin/job/getJobSubdomainData',
            method: 'GET',
            success: function (data) {
                console.log("data", data);
                modal.find('#IsActiveJob_subdomain').prop('checked', data.isActiveJob);
                modal.find('#ApiKey_subdomain').val(data.apiKey);
                modal.find('#Email_subdomain').val(data.email);
                modal.find('#JobPeriodTime_subdomain').val(data.jobPeriodTime);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                swal.close();
            }
        });
    });
    modal.modal();
}

function jobHostDomain() {
    formUrl = jobBaseUrl + '/CreateHostDomainJob';
    var modal = $('#jobHostDomainModal');
    modal.on('show.bs.modal', function (event) {
        var modal = $(this);
        $.ajax({
            url: '/admin/job/GetJobHostDomainData',
            method: 'GET',
            success: function (data) {
                console.log("data", data);
                modal.find('#IsActiveJob_hostdomain').prop('checked', data.isActiveJob);
                modal.find('#ApiKey_hostdomain').val(data.apiKey);
                modal.find('#Email_hostdomain').val(data.email);
                modal.find('#JobPeriodTime_hostdomain').val(data.jobPeriodTime);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                swal.close();
            }
        });
    });
    modal.modal();
}

function jobIpConfigJob() {
    formUrl = jobBaseUrl + '/CreateIpConfigJob';
    var modal = $('#jobHostDomainModal');
    modal.on('show.bs.modal', function (event) {
        var modal = $(this);
        $.ajax({
            url: '/admin/job/GetJobHostDomainData',
            method: 'GET',
            success: function (data) {
                console.log("data", data);
                modal.find('#IsActiveJob_hostdomain').prop('checked', data.isActiveJob);
                modal.find('#ApiKey_hostdomain').val(data.apiKey);
                modal.find('#Email_hostdomain').val(data.email);
                modal.find('#JobPeriodTime_hostdomain').val(data.jobPeriodTime);
                modal.find('#JobExpireMinuteTime_hostdomain').val(data.JobExpireMinuteTime);
          
            },
            error: function (xhr, ajaxOptions, thrownError) {
                swal.close();
            }
        });
    });
    modal.modal();
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
            window.location.reload();
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
function submitFormSubdomain() {
    loading();
    let form = document.getElementById('jobSubdomainForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            document.getElementById('jobSubdomainForm').reset();
            $('#jobSubdomainModal').modal('toggle');
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
            window.location.reload();
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

function submitFormHostdomain() {
    loading();
    let form = document.getElementById('jobHostDomainForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            document.getElementById('jobHostDomainForm').reset();
            $('#jobHostDomainModal').modal('toggle');
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
            window.location.reload();
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
$('#IsActiveJob').on('change', function () {
    if ($(this).is(":checked")) {
        enableFields();
    } else {
        disableFields();
    }
});
$('#IsActiveJob_subdomain').on('change', function () {
    if ($(this).is(":checked")) {
        enableFields();
    } else {
        disableFields();
    }
});
$('#IsActiveJob_hostdomain').on('change', function () {
    if ($(this).is(":checked")) {
        enableFields();
    } else {
        disableFields();
    }
});
// تابع تنظیم وضعیت غیرفعال بودن فیلدها
function disableFields() {
    $("#ApiKey").prop("disabled", true);
    $("#Email").prop("disabled", true);
    $("#FailConnectionCount").prop("disabled", true);
    $("#FailConnectionPercent").prop("disabled", true);
    $("#JobPeriodTime").prop("disabled", true);
    $("#JobExpireMinuteTime").prop("disabled", true);

    $("#ApiKey_subdomain").prop("disabled", true);
    $("#Email_subdomain").prop("disabled", true);
    $("#JobPeriodTime_subdomain").prop("disabled", true);
    $("#JobExpireMinuteTime_hostdomain").prop("disabled", true);
}

// تابع تنظیم وضعیت فعال بودن فیلدها
function enableFields() {
    $("#ApiKey").prop("disabled", false);
    $("#Email").prop("disabled", false);
    $("#FailConnectionCount").prop("disabled", false);
    $("#FailConnectionPercent").prop("disabled", false);
    $("#JobPeriodTime").prop("disabled", false);
    $("#JobExpireMinuteTime").prop("disabled", false);

    $("#ApiKey_subdomain").prop("disabled", false);
    $("#Email_subdomain").prop("disabled", false);
    $("#JobPeriodTime_subdomain").prop("disabled", false);
    $("#JobExpireMinuteTime_hostdomain").prop("disabled", false);
}



