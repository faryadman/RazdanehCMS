let serversTable = $('#serversTable').DataTable();
let lastPage = getLastPage();
let serversBaseUrl = "/admin/servers";
let formUrl;

function getLastPage() {
    return localStorage.getItem('lastPage');
}
function reloadTableAndGoToPage(pageNumber) {
    serversTable.page(pageNumber - 1).draw(false);
}

function saveLastPage() {
    // حذف رویداد
    serversTable.off('draw.dt');

    serversTable.on('draw.dt').on('draw.dt', function () {
        console.log('شماره صفحه فعلی: ', serversTable.page.info().page + 1);
        localStorage.setItem('lastPage', serversTable.page.info().page + 1);
    });
}

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
            serversTable.clear().draw();
            renderservers(result);
            // تنظیم رویداد بعد از تغییر صفحه
            saveLastPage()

            // بازگشت به صفحه مورد نظر
            let currentPageNumber = lastPage !== null ? lastPage : 1;
            reloadTableAndGoToPage(currentPageNumber);
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");
        }
    });

}

function renderservers(data) {
    console.log(data);
    let isOdd = true;
    for (var i = 0; i < data.length; i++) {
        let item = data[i];

        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        // منوی عملیات ردیف (ویرایش/کپی/لاگ/بلک‌لیست/حذف) — همه کلاس‌ها و رویدادها دست‌نخورده
        let actionsMenu = '<div class="rd-row-actions"><div class="dropdown rd-item-dropdown">' +
            '<button class="btn btn-sm btn-light rd-more-btn" type="button" data-rd-toggle="dropdown" aria-haspopup="true" aria-expanded="false" title="عملیات">' +
            '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="1.9"/><circle cx="12" cy="12" r="1.9"/><circle cx="12" cy="19" r="1.9"/></svg>' +
            '</button>' +
            '<div class="dropdown-menu dropdown-menu-left rd-item-menu">' +
            '<button class="dropdown-item edit" data-item-id="' + item.id + '" data-item-serverName="' + item.serverName + '" data-item-location="' + item.location + '" data-item-ip="' + item.ip + '" title="ویرایش">' +
            '<i class="rd-di rd-di-warn"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg></i>ویرایش</button>' +
            '<button class="dropdown-item" onclick="duplicate(' + item.id + ')" title="کپی سرور">' +
            '<i class="rd-di rd-di-info"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg></i>کپی سرور</button>' +
            '<a class="dropdown-item" href="' + serversBaseUrl + '/logs?serverId=' + item.id + '" title="لاگ‌های سرور">' +
            '<i class="rd-di rd-di-primary"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg></i>لاگ‌های سرور</a>' +
            '<a class="dropdown-item" href="' + serversBaseUrl + '/blackList?id=' + item.id + '" title="بلک‌لیست سرور">' +
            '<i class="rd-di rd-di-dark"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/></svg></i>بلک‌لیست سرور</a>' +
            '<div class="dropdown-divider"></div>' +
            '<button class="dropdown-item rd-danger-item" onclick="deleteservers(' + item.id + ')" title="حذف">' +
            '<i class="rd-di rd-di-danger"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg></i>حذف سرور</button>' +
            '</div>' +
            '</div></div>';
        let logsCell = renderLogsCell(item.allLogsStatistics, item.irancellLogsStatistics, item.hamraheAvvalLogsStatistics, item.unknownLogsStatistics);
        let isAvailable = item.isAvailable ? "checked" : "";

        let isAdServer = item.isAd ? '<span class="badge badge-success">بله</span>' : '<span class="badge badge-light">خیر</span>';
        // سلول DNS: دامنه فعلی + منوی عملیات DNS (همان ۶ توابع، کال‌بک‌ها دست‌نخورده)
        let buttons = '<div class="rd-dns-cell">' +
            '<span class="mono rd-current-domain" title="دامنه فعلی">' + (item.currentDomainValue || '—') + '</span>' +
            '<div class="dropdown rd-item-dropdown">' +
            '<button class="btn btn-sm btn-outline-info rd-dns-btn" type="button" data-rd-toggle="dropdown" aria-haspopup="true" aria-expanded="false" title="عملیات DNS">' +
            '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg> DNS</button>' +
            '<div class="dropdown-menu dropdown-menu-left rd-item-menu" style="min-width:236px">' +
            '<button class="dropdown-item" onclick="domainRefresh(' + item.id + ')" title="Refresh Domain"><i class="rd-di rd-di-success"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/></svg></i>به‌روزرسانی دامنه</button>' +
            '<button class="dropdown-item" onclick="tcpRefresh(' + item.id + ')" title="Refresh Tcp"><i class="rd-di rd-di-success"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/></svg></i>به‌روزرسانی TCP</button>' +
            '<button class="dropdown-item" onclick="subdomainRefresh(' + item.id + ')" title="Refresh SubDomain"><i class="rd-di rd-di-success"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/></svg></i>به‌روزرسانی ساب‌دامین</button>' +
            '<button class="dropdown-item" onclick="createDnsRecord(' + item.id + ')" title="Create Dns Record"><i class="rd-di rd-di-warn"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 5v14M5 12h14"/></svg></i>ساخت رکورد DNS</button>' +
            '<div class="dropdown-divider"></div>' +
            '<button class="dropdown-item rd-danger-item" onclick="deleteAllDnsRecord(' + item.id + ')" title="Delete Dns Record"><i class="rd-di rd-di-danger"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg></i>حذف همه رکوردهای DNS</button>' +
            '<button class="dropdown-item rd-danger-item" onclick="deleteTcpDnsRecord(' + item.id + ')" title="Delete Tcp Dns Record"><i class="rd-di rd-di-danger"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg></i>حذف رکورد TCP DNS</button>' +
            '</div>' +
            '</div>' +
            '</div>';

        let config = item.config;
        let configObject = JSON.parse(config);
        console.log(configObject);
        var serverName = configObject.outbounds[0]?.streamSettings?.tlsSettings?.serverName;
        var hostName = configObject.outbounds[0]?.streamSettings?.wsSettings?.headers?.Host;


        let addedRow = serversTable.row.add([
            deleteChekbox,
            item.id,
            isAdServer,
            buttons,
            actionsMenu,
            '<span class="badge badge-dark">' + serverName + '<hr/>Host:' + hostName + '<hr/>' + item.updatedAtFormatted + '</span>',
            item.location,
            '<div>' + item.ip + '<br><button onclick="addToBlackList(' + item.id + ')" class="btn btn-primary btn-sm">add to blacklist</button></div>',
            '<span class="badge badge-dark">' + item.group.title + '</span>',
            '<div class="custom-control custom-switch mr-2 mb-1"><input ' + isAvailable + ' type="checkbox" class="custom-control-input isAvailableInput" data-item-id="' + item.id + '" id="customSwitch' + item.id + '"><label class="custom-control-label" for="customSwitch' + item.id + '"></label></div>',
            item.isForHamraheAvval ? '<span class="rd-check" title="پشتیبانی همراه اول">✓</span>' : '<span class="rd-no" title="پشتیبانی ندارد">—</span>',
            item.isForIrancell ? '<span class="rd-check" title="پشتیبانی ایرانسل">✓</span>' : '<span class="rd-no" title="پشتیبانی ندارد">—</span>',
            logsCell
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
function goIpList() {
    loading();
    let url = '/admin/ipconfig';
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

function subdomainRefresh(id) {
    loading();
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: serversBaseUrl + `/changeSubDomain?id=‍${id}`,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            console.log('data', data);
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            document.getElementById('serverForm').reset();
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
function domainRefresh(id) {
    saveLastPage();
    loading();
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: serversBaseUrl + `/changeDomain?id=‍${id}`,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            console.log('data', data);
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            document.getElementById('serverForm').reset();
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
function tcpRefresh(id) {
    saveLastPage();
    
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: serversBaseUrl + `/refreshTcp?id=‍${id}`,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            console.log('data', data);
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            document.getElementById('serverForm').reset();
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            const resp = xhr.responseJSON;
            const errors = (resp && resp.errors) || tryParseErrors(xhr.responseText) || ['An unexpected error occurred'];
            errors.forEach(e => toastr.error(e));
            Swal.close && Swal.close(); // prefer consistent Swal usage
        }
    })
}
function tryParseErrors(text) {
    try {
        const j = JSON.parse(text);
        return j && j.errors ? j.errors : null;
    } catch (e) { return null; }
}
function createDnsRecord(id) {
    loading();
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: serversBaseUrl + `/CreateDnsRecord?id=‍${id}`,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            console.log('data', data);
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            document.getElementById('serverForm').reset();
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
function deleteAllDnsRecord(id) {
    loading();
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: serversBaseUrl + `/DeleteDnsRecord?id=‍${id}`,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            console.log('data', data);
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            document.getElementById('serverForm').reset();
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
function deleteTcpDnsRecord(id) {
    loading();
    let form = document.getElementById('serverForm');
    let formData = new FormData(form);
    $.ajax({
        url: serversBaseUrl + `/DeleteTcpDnsRecord?id=‍${id}`,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            console.log('data', data);
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            document.getElementById('serverForm').reset();
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
            setSelectListData('IsForHamraheAvval', data.isForHamraheAvval);
            setSelectListData('IsForIrancell', data.isForIrancell);
            $('#ConfigKey').val(data.configKey);
            $('#ConfigValue').val(data.configValue);
            $('#ServerName').val(item.attr('data-item-serverName'));
            $('#Config').val(data.config);
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

    if (item == null) return '<div class="' + className + '"></div>';
    var total = item.count || 0;
    var ok = item.successCount || 0;
    var ko = item.failCount || 0;
    var okPct = total ? Math.round(ok / total * 100) : 0;
    var fa = function (n) { try { return Number(n).toLocaleString('fa-IR'); } catch (e) { return n; } };
    return '<div class="' + className + ' rd-stat">' +
        '<span class="mono rd-stat-count" title="تعداد کل">' + fa(total) + '</span>' +
        '<div class="rd-split" title="موفق: ' + fa(ok) + ' — ناموفق: ' + fa(ko) + '"><span class="ok" style="width:' + okPct + '%"></span><span class="ko" style="width:' + (100 - okPct) + '%"></span></div>' +
        '<div class="rd-split-legend"><span><i style="background:var(--rd-success)"></i>' + fa(ok) + '</span><span><i style="background:var(--rd-danger)"></i>' + fa(ko) + '</span></div>' +
        '</div>';
}

/* ستون فشردهٔ «آمار لاگ‌ها» — جایگزین ۴ ستون مجزا (کلاس‌های AllLogs/Irancell/HamraheAvval/unknown حفظ شده) */
function renderLogsCell(all, irc, hv, unk) {
    var fa = function (n) { try { return Number(n).toLocaleString('fa-IR'); } catch (e) { return n; } };
    function row(cls, name, item) {
        if (item == null) return '<div class="' + cls + ' logshow p-1 rd-log-row"><div class="rd-log-top"><span class="rd-log-name">' + name + '</span><span class="mono rd-log-count">—</span></div></div>';
        var total = item.count || 0, ok = item.successCount || 0, ko = item.failCount || 0;
        var okPct = total ? Math.round(ok / total * 100) : 0;
        return '<div class="' + cls + ' logshow p-1 rd-log-row">' +
            '<div class="rd-log-top"><span class="rd-log-name">' + name + '</span>' +
            '<span class="mono rd-log-count" title="کل: ' + fa(total) + ' — موفق: ' + fa(ok) + ' — ناموفق: ' + fa(ko) + '">' + fa(total) + '</span></div>' +
            '<div class="rd-split" title="موفق: ' + fa(ok) + ' — ناموفق: ' + fa(ko) + '"><span class="ok" style="width:' + okPct + '%"></span><span class="ko" style="width:' + (100 - okPct) + '%"></span></div>' +
            '</div>';
    }
    return '<div class="rd-logs">' +
        row('AllLogs', 'کل', all) +
        row('irancellLogs', 'ایرانسل', irc) +
        row('HamraheAvvalLogs', 'همراه اول', hv) +
        row('unknownLogs', 'نامشخص', unk) +
        '</div>';
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
function onClickChangeServerAddresses() {
    loading();
    $('#changeServerAddressModal').modal();
    swal.close();
}
function onClickDeleteDns() {
    loading();
    $('#onClickDeleteDns').modal();
    swal.close();
}

function onClickChangeServerAddressSubmit() {
    $('#changeServerAddressModal').modal('toggle');
    loading();
    let result = $('#addressInput').val();
    let model = {
        address: result
    }
    $.ajax({
        type: "POST",
        url: serversBaseUrl + '/ChangeServerAddressInput',
        data: model,
        dataType: "json",
        success: function (data) {
            if (window.location.pathname.toLowerCase() == '/admin/servers'.toLowerCase()) {
                getservers(false);
            } else {
                getservers(true);
            }
            data.status == "0" ? swal.fire('', data.message, 'error') : swal.fire('', data.message, 'success');
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            swal.close();
            alert(" بروز اشکال در اتصال به اینترنت ");

        }
    });
}

$('#selectAllCheckbox').change(function () {
    var isChecked = $(this).prop('checked');
    $('.deleteCheckbox').prop('checked', isChecked).each(function () {
        var itemId = $(this).attr('data-item-id');
        $(this).val(itemId);
    });
});


