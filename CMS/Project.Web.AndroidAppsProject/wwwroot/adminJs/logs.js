let logsTable = $('#logsTable').DataTable();

let logsBaseUrl = "/admin/servers/logs";
let cronJobInfoBaseUrl = "/admin/cronJobInfo";

function getlogs() {
    let serverId = $('#selectedserverId').val();
    $.ajax({
        type: "GET",
        url: logsBaseUrl + '/list?serverId=' + serverId,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            logsTable.clear().draw();
            renderlogs(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");
        }
    });

}

function renderlogs(data) {
    let isSuccessCounterOnOdd = true;
    let isFailedCounterOnOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        let addedRow = logsTable.row.add([
            '',
            item.id,
            item.operatorName,
            '<span class="badge badge-dark">' + item.ip + '</span>',
            item.country,
            item.isp,
            item.city,
            item.org,
            item.userId,
            item.updatedAtFormatted
        ]).node();


        if (isSuccessCounterOnOdd) {
            $(addedRow).css('background-color', '#cafdca');
        } else {
            $(addedRow).css('background-color', '#b7e6b3');
        }

        if (item.connectionStatus == 1) {
            if (isFailedCounterOnOdd) {
                $(addedRow).css('background-color', '#ff9d9d');
            } else {
                $(addedRow).css('background-color', 'rgb(253 145 145)');
            }
        }

        isSuccessCounterOnOdd = !isSuccessCounterOnOdd;
        isFailedCounterOnOdd = !isFailedCounterOnOdd;
        //$(addedRow).css('background-color', '#ff9d9d');


        //if (item.connectionStatus == 0)
        //    $(addedRow).css('background-color', '#cafdca');

        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).attr('id', item.id);
        $(addedRow).addClass('log');
        logsTable.rows.add(addedRow).draw();

    }
}


function getCronJobInfo() {
    loading();
    $.ajax({
        type: "GET",
        url: cronJobInfoBaseUrl + '/Get',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            $("#Timer").val(result.timer);
            $("#ExecutedCount").val(result.executedCount);
            $("#LastExecutionDate").val(result.lastExecutionDateToString);
            $('#cronJobInfoModal').modal();
            swal.close();
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");
            swal.close();
        }
    });
}
function submitForm() {
    loading();
    let form = document.getElementById('CronJobInfoForm');
    let formData = new FormData(form);
    $.ajax({
        url: cronJobInfoBaseUrl + '/Update',
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            $('#cronJobInfoModal').modal('toggle');
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

function massDelete() {
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
            $.ajax({
                type: "GET",
                url: cronJobInfoBaseUrl + '/MassDelete',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    getlogs();
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                },
                error: function (xmlhttprequest, textstatus, errorthrown) {
                    alert(" بروز اشکال در اتصال به اینترنت ");
                    swal.close();
                }
            });
        }
    });
    
}


function getAlllogs() {
    $.ajax({
        type: "GET",
        url: logsBaseUrl + '/GetAllLogsStatistics',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            renderlogsCard(result.result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");
        }
    });

}

function renderlogsCard(result) {

    $('#logsArea').empty();

    let allLogsStatistics = renderStatistics(result.allLogsStatistics, 3);
    let irancellLogsStatistics = renderStatistics(result.irancellLogsStatistics, 0);
    let hamraheAvvalLogsStatistics = renderStatistics(result.hamraheAvvalLogsStatistics, 1);
    let unknownLogsStatistics = renderStatistics(result.unknownLogsStatistics, 2);
    $('#logsArea').append(allLogsStatistics);
    $('#logsArea').append(irancellLogsStatistics);
    $('#logsArea').append(hamraheAvvalLogsStatistics);
    $('#logsArea').append(unknownLogsStatistics);

    swal.close();
}
function renderStatistics(item, type) {
    let className = "";
    let name = "";
    switch (type) {
        case 0:
            className = "irancellLogs logshow p-1";
            name = "ایرانسل";
            break
        case 1:
            className = "HamraheAvvalLogs logshow p-1";
            name = "همراه اول";
            break
        case 2:
            className = "unknownLogs logshow p-1";
            name = "نامشخص";
            break
        default:
            className = "AllLogs logshow p-1";
            name = "کل لاگ‌ها";
    }

    var fa = function (n) { try { return Number(n).toLocaleString('fa-IR'); } catch (e) { return n; } };

    let statistics = item != null ? '<div class="col-lg-3 col-sm-6 col-12"><div class="card mb-4"><div class="card-header ' + className + '"><h5 class="mb-0" style="font-size:14px">' + name + '</h5></div><div class="card-content rd-stat-card">' +
        '<span class="mono rd-stat-count" title="تعداد کل">' + fa(item.count) + '</span>' +
        '<div class="rd-split-num big"><span class="ok' + (item.successCount ? '' : ' zero') + '" title="موفق">✓ ' + fa(item.successCount) + ' موفق</span>' +
        '<span class="ko' + (item.failCount ? '' : ' zero') + '" title="ناموفق">✗ ' + fa(item.failCount) + ' ناموفق</span></div>' +
        '</div></div></div>' : '<div class="' + className + '"></div>';

    return statistics;
}

function calculatePercentage(count, totalCount) {
    return Math.round((count / totalCount) * 100);
}

let counter = 0;
let refreshId = setInterval(function () {
    if (counter == 10) {
        clearInterval(refreshId);
    }
    $('.HamraheAvvalLogs').css('background', 'rgb(180, 243, 245)');
    $('.irancellLogs').css('background', 'rgb(250, 255, 190)');
    $('.AllLogs').css('background', 'rgb(228 200 255)');
    $('.unknownLogs').css('background', '#ffd6d9');
    $('.logshow').css('border-radius', '10px');
    counter++;
}, 1000);

//function renderlogsCard(data) {

//    $('#logsArea').empty();
//    //let isSuccessCounterOnOdd = true;
//    //let isFailedCounterOnOdd = true;
//    for (var i = 0; i < data.length; i++) {
//        let item = data[i];
//        let bgColor = '#cafdca';
//        if (item.connectionStatus == 1) {
//            bgColor = '#ff9d9d';
//        }
//        let operatorBgColor = "rgb(234 255 0)";

//        if (item.operator == 1) {
//            operatorBgColor = '#b4f3f5';
//        }

//        if (item.operator == 2) {
//            operatorBgColor = '';
//        }

//        item.isp = item.isp == "" ? "null" : item.isp;
//        $('#logsArea').append('<div class="col-lg-4 col-sm-6 col-12"><div class= "card" ><div style="background:' + bgColor + '" class="card-header d-flex align-items-start pb-0">' +
//            '<div><h4 class="text-bold-700"><span class="badge badge-dark">' + item.ip + ' <span/></h4>' +
//            '<p class= "mb-0" > Isp : ' + item.isp + '</p >' +
//            '<p class= "mb-0" > Country And City =  ' + item.country + " " + item.city + '</p >' +
//            '<p class= "mb-0" > Org =  ' + item.org + '</p >' +
//            '<p class= "mb-0" > UserId =  ' + item.userId + '</p >' +
//            '</div><div style="background:' + operatorBgColor + ';color:black;" class="avatar p-50"><div>' +
//            item.operatorName +
//            '</div></div></div>' +
//            '<div class="card-content"><div id="line-area-chart-5"></div></div>' +
//            '</div></div>');
//        //if (isSuccessCounterOnOdd) {
//        //    $(addedRow).css('background-color', '#cafdca');
//        //} else {
//        //    $(addedRow).css('background-color', '#b7e6b3');
//        //}

//        //if (item.connectionStatus == 1) {
//        //    if (isFailedCounterOnOdd) {
//        //        $(addedRow).css('background-color', '#ff9d9d');
//        //    } else {
//        //        $(addedRow).css('background-color', 'rgb(253 145 145)');
//        //    }
//        //}
//    }
//}