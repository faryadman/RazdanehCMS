let serversTable = $('#serversTable').DataTable();

let serversBaseUrl = "/admin/servers";
let formUrl;

function getservers(isAd, filter) {
    filter = filter == undefined ? 1 : filter;
    let groupId = $('#selectedgroupId').val();
    let appId = $('#selectedappId').val();
    $.ajax({
        type: "GET",
        url: serversBaseUrl + '/List?groupId=' + groupId + '&appId=' + appId + '&isAd=' + isAd + '&filter=' + filter,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            console.log(result);
            serversTable.clear().draw();
            renderservers(result);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });

}

function renderservers(data) {
    let isOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];

        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let editButton = '<button class="btn btn-sm btn-warning edit" ' +
            'data-item-id="' + item.id + '"' +
            'data-item-serverName="' + item.serverName + '"' +
            'data-item-location="' + item.location + '"' +
            'data-item-ip="' + item.ip + '"' +
            '>edit</button>';

        let deleteButton = '<button class="btn btn-sm btn-danger    " onclick="deleteservers(' + item.id + ')">Delete</button>';
        let duplicateButton = '<button class="btn btn-sm btn-primary    " onclick="duplicate(' + item.id + ')">Duplicate</button>';
        let blackListButton = '<a class="btn btn-sm btn-dark" href="' + serversBaseUrl + '/blackList?id=' + item.id + '">BlackList</a>';
        let logsButton = '<a class="btn btn-sm btn-info" href="' + serversBaseUrl + '/logs?serverId=' + item.id + '">Logs</a>';
        let allLogsStatistics = renderStatistics(item.allLogsStatistics, 3);
        let irancellLogsStatistics = renderStatistics(item.irancellLogsStatistics, 0);
        let hamraheAvvalLogsStatistics = renderStatistics(item.hamraheAvvalLogsStatistics, 1);
        let unknownLogsStatistics = renderStatistics(item.unknownLogsStatistics, 2);
        let isAvailable = item.isAvailable ? "checked" : "";

        let isAdServer = item.isAd ? '<span class="badge badge-success">true</span>' : '<span class="badge badge-danger">false</span>';
        let isNewDomain = item.isNewDomain ? '<a class="btn btn-success"  href="' + serversBaseUrl + '/changeDomain?serverId=' + item.id + '">new(' + item.currentDomainValue + ')</button>' : '<a class="btn btn-success"  href="' + serversBaseUrl + '/changeDomain?serverId=' + item.id + '">Get New Domain!</button>';


        let addedRow = serversTable.row.add([
            deleteChekbox,
            item.id,
            isAdServer,
            isNewDomain,
            item.isNewDomain,
            '<span class="badge badge-dark">' + item.serverName + '</span>',
            item.location,
            '<div>' + item.ip + '<br><button onclick="addToBlackList(' + item.id + ')" class="btn btn-primary btn-sm">add to blacklist</button></div>',
            item.config.length > 30 ? item.config.substring(0, 30) + "..." : item.config,
            '<span class="badge badge-dark">' + item.group.title + '</span>',
            '<div class="custom-control custom-switch mr-2 mb-1"><input ' + isAvailable + ' type="checkbox" class="custom-control-input isAvailableInput" data-item-id="' + item.id + '" id="customSwitch' + item.id + '"><label class="custom-control-label" for="customSwitch' + item.id + '"></label></div>',
            item.isForHamraheAvval,
            item.isForIrancell,
            allLogsStatistics,
            irancellLogsStatistics,
            hamraheAvvalLogsStatistics,
            unknownLogsStatistics,
            item.updatedAtFormatted,
            blackListButton,
            logsButton,
            duplicateButton,
            editButton,
            deleteButton
        ]).node();

        if (isOdd)
            $(addedRow).css('background-color', 'rgb(233 233 233)');

        isOdd = !isOdd;

        $(addedRow).attr('data-row-id', item.id);
        $(addedRow).attr('id', item.id);
        $(addedRow).addClass('server');
        serversTable.rows.add(addedRow).draw();

    }
}

function newserver() {
    loading();
    initializeGroupSelectList();
    formUrl = serversBaseUrl + '/Create';
    $('#groupSelectListArea').css('display', 'block');
    $('#serversModal').modal();
    swal.close();
}
function goDomain() {
    loading();
    let url = '/admin/domains';
    window.location.href = url;
    swal.close();
}
function submitForm() {
    loading();
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            document.getElementById('serverForm').reset();
            $('#serversModal').modal('toggle');
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
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl + "Ad",
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getservers(true);
            document.getElementById('serverForm').reset();
            $('#serversModal').modal('toggle');
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

function deleteservers(id) {
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
                url: serversBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                        getservers(false);
                    } else {
                        getservers(true);
                    }
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

$('#serversTable').on('click', '.edit', function () {
    loading();
    let item = $(this);
    initializeGroupSelectList();
    $.ajax({
        url: serversBaseUrl + "/Detail?id=" + item.attr('data-item-id'),
        method: 'GET',
        success: function (data) {
            $('#itemId').val(item.attr('data-item-id'));
            $('#Config').val(data.config);
            setSelectListData('IsForHamraheAvval', data.isForHamraheAvval);
            setSelectListData('IsForIrancell', data.isForIrancell);
            $('#ConfigKey').val(data.configKey);
            $('#ConfigValue').val(data.configValue);
            $('#ServerName').val(item.attr('data-item-serverName'));
            $('#Location').val(item.attr('data-item-location'));
            $('#CurrentDomainValue').val(data.currentDomainValue);
            $('#Ip').val(item.attr('data-item-ip'));
            formUrl = serversBaseUrl + '/Edit';
            $('#groupSelectListArea').css('display', 'none');
            $('#serversModal').modal();
            Swal.close();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            let errors = xhr.responseJSON.errors;
            for (var i = 0; i < errors.length; i++) {
                toastr.error(errors[i]);
            }
            swal.close();
        }
    });
})

function setSelectListData(id, value) {
    let element = document.getElementById(id);
    element.value = value;
}

function initializeGroupSelectList() {
    loading();
    let isAd;
    if (window.location.pathname.toLowerCase() == '/admin/servers/adIndex'.toLowerCase()) {
        console.log(true);
        isAd = true;
        $.ajax({
            url: '/admin/groups/list?isAd=' + isAd,
            method: 'GET',
            success: function (data) {
                $('#GroupId').empty();
                for (var i = 0; i < data.length; i++) {
                    let item = data[i];
                    $('#GroupId').append('<option value="' + item.id + '">' + item.title + '</option>');
                }
            }
        });
    } else {
        isAd = false;
        $.ajax({
            url: '/admin/groups/list?isAd=' + isAd,
            method: 'GET',
            success: function (data) {
                $('#GroupId').empty();
                for (var i = 0; i < data.length; i++) {
                    let item = data[i];
                    $('#GroupId').append('<option value="' + item.id + '">' + item.title + '</option>');
                }
            }
        });
    }
}

function renderStatistics(item, type) {
    let className = "";
    switch (type) {
        case 0:
            className = "irancellLogs logshow p-1";
            break

        case 1:
            className = "HamraheAvvalLogs logshow p-1";
            break

        case 2:
            className = "unknownLogs logshow p-1";
            break
        default:
            className = "AllLogs logshow p-1";
    }

    let statistics = item != null ? '<div class="' + className + '">' +
        '<span>Count : ' + item.count + '</span><br/>' +
        '<span class="text-success">SCount : ' + item.successCount + '</span><br/>' +
        '<span class="text-danger">FCount : ' + item.failCount + '</span><br/>' +
        '<span class="text-success">SPercentage : ' + calculatePercentage(item.successCount, item.count) + '%</span><br/>' +
        '<span class="text-danger">FPercentage : ' + calculatePercentage(item.failCount, item.count) + '%</span><br/>' +
        '</div>' : '<div class="' + className + '"></div>';

    return statistics;
}

function calculatePercentage(count, totalCount) {
    return Math.round((count / totalCount) * 100);
}

function addToBlackList(id) {
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
                url: serversBaseUrl + '/addToBlackList',
                data: vm,
                success: function (data) {
                    //getservers();
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
    });
}

let counter = 0;
let refreshId = setInterval(function () {
    if (counter == 100000) {
        clearInterval(refreshId);
    }
    $('.HamraheAvvalLogs').css('background', 'rgb(180, 243, 245)');
    $('.irancellLogs').css('background', 'rgb(250, 255, 190)');
    $('.AllLogs').css('background', 'rgb(228 200 255)');
    $('.unknownLogs').css('background', '#ffd6d9');
    $('.logshow').css('border-radius', '10px');
    counter++;
}, 100);


$('#serversTable').on('change', '.isAvailableInput', function () {
    loading();
    let input = $(this);
    console.log(itemId);
    let model = {
        id: input.attr('data-item-id')
    }
    $.ajax({
        type: "POST",
        url: serversBaseUrl + '/toggleIsAvailableInput',
        data: model,
        dataType: "json",
        success: function (data) {
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");
            swal.close();
        }
    });
});

function filterservers() {
    loading();
    $('#filterserversModal').modal();
    swal.close();
}

function filter() {
    loading();
    let result = $('#filterInput').val();
    if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
        getservers(false, result);
    } else {
        getservers(true, result);
    }
    setTimeout(function () {
        swal.close();
        $('#filterserversModal').modal('toggle');
    }, 4000);
}

function duplicate(id) {
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
                url: serversBaseUrl + '/Duplicate',
                data: vm,
                success: function (data) {
                    if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                        getservers(false);
                    } else {
                        getservers(true);
                    }
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}