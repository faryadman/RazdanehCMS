/* ============================================================
   v18 — صفحهٔ لیست اپ ها: دو نما (لیست/کارت) + عملیات مدرن
   ============================================================ */
let appsTable = $('#appsTable').DataTable();

let appsBaseUrl = "/admin/apps";
let formUrl;
let appGroups = [];
let rdAppData = [];
let rdSearchTerm = '';
let rdSearchTimer = null;
let rdViewMode = (function () { try { return localStorage.getItem('rdAppsView') || 'list'; } catch (e) { return 'list'; } })();

function rdFaNum(n) { return String(n).replace(/[0-9]/g, function (d) { return '۰۱۲۳۴۵۶۷۸۹'[d]; }); }
function rdEsc(s) { return String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;'); }

var RD_IC = {
    server: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="2" width="20" height="8" rx="2" ry="2"/><rect x="2" y="14" width="20" height="8" rx="2" ry="2"/><line x1="6" y1="6" x2="6.01" y2="6"/><line x1="6" y1="18" x2="6.01" y2="18"/></svg>',
    dots: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="1.9"/><circle cx="12" cy="12" r="1.9"/><circle cx="12" cy="19" r="1.9"/></svg>',
    edit: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>',
    trash: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>',
    layers: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="12 2 2 7 12 12 22 7 12 2"/><polyline points="2 17 12 22 22 17"/><polyline points="2 12 12 17 22 12"/></svg>',
    phone: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="5" y="2" width="14" height="20" rx="2" ry="2"/><line x1="12" y1="18" x2="12.01" y2="18"/></svg>'
};

function getapps() {
    $.ajax({
        type: "POST",
        url: appsBaseUrl + '/List',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            rdAppData = result || [];
            applyAppView();
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");
        }
    });
}

function rdAppMatch(item) {
    if (!rdSearchTerm) return true;
    return (item.appTitle || '').indexOf(rdSearchTerm) >= 0
        || (item.apiRoute || '').indexOf(rdSearchTerm) >= 0
        || String(item.id) === rdSearchTerm;
}

function rdAppServersLinks(item) {
    return '<a class="btn btn-sm btn-outline-primary rd-card-servers" href="/admin/servers?appid=' + item.id + '" title="سرورهای این اپ">' + RD_IC.server + 'سرورا</a>' +
        '<a class="btn btn-sm btn-outline-info rd-card-servers" href="/admin/servers/adindex?appid=' + item.id + '" title="سرورهای تبلیغاتی این اپ">' + RD_IC.server + 'تبلیغاتی</a>';
}

function rdAppActionsMenu(item) {
    return '<div class="rd-row-actions"><div class="dropdown rd-item-dropdown">' +
        '<button class="btn btn-sm btn-light rd-more-btn" type="button" data-rd-toggle="dropdown" aria-haspopup="true" aria-expanded="false" title="عملیات">' + RD_IC.dots + '</button>' +
        '<div class="dropdown-menu dropdown-menu-left rd-item-menu">' +
        '<button class="dropdown-item rd-apps-groups" data-item-id="' + item.id + '" title="گروه‌ها"><i class="rd-di rd-di-warn">' + RD_IC.layers + '</i>گروه‌ها</button>' +
        '<a class="dropdown-item" href="/admin/apps/edit?id=' + item.id + '" title="ویرایش"><i class="rd-di rd-di-warn">' + RD_IC.edit + '</i>ویرایش</a>' +
        '<div class="dropdown-divider"></div>' +
        '<button class="dropdown-item rd-danger-item" onclick="deleteapps(' + item.id + ')" title="حذف"><i class="rd-di rd-di-danger">' + RD_IC.trash + '</i>حذف اپ</button>' +
        '</div></div></div>';
}

/* — نمای لیست — */
function renderapps(data) {
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let titleCell = '<div class="rd-row-main"><span class="rd-row-title">' + rdEsc(item.appTitle) + '</span><span class="rd-app-route">' + rdEsc(item.apiRoute || '—') + '</span></div>';
        let actionsCell = rdAppServersLinks(item) + rdAppActionsMenu(item);
        appsTable.row.add([
            deleteChekbox,
            '<span class="mono">' + item.id + '</span>',
            titleCell,
            '<span class="mono">' + rdEsc(item.storeVersion || '—') + '</span>',
            rdEsc(item.updatedAtFormatted || '—'),
            actionsCell
        ]).node();
    }
    appsTable.draw();
}

/* — نمای کارت — */
function rdAppCard(item) {
    let row = function (k, v) { return '<div class="rd-card-row"><span class="rd-card-k">' + k + '</span><span class="rd-card-v">' + v + '</span></div>'; };
    return '<article class="rd-card" data-id="' + item.id + '">' +
        '<div class="rd-card-head">' +
        '<span class="rd-app-ic" aria-hidden="true">' + RD_IC.phone + '</span>' +
        '<div class="rd-card-main"><span class="rd-card-title" title="' + rdEsc(item.appTitle) + '">' + rdEsc(item.appTitle || '—') + '</span><span class="rd-app-route">' + rdEsc(item.apiRoute || '') + '</span></div>' +
        '<div class="rd-card-actions">' + rdAppServersLinks(item) + rdAppActionsMenu(item) + '</div>' +
        '</div>' +
        '<div class="rd-card-body">' +
        row('شناسه', '<span class="mono">' + item.id + '</span>') +
        row('نسخه', '<span class="mono">' + rdEsc(item.storeVersion || '—') + '</span>') +
        row('به‌روزرسانی', rdEsc(item.updatedAtFormatted || '—')) +
        '</div></article>';
}

function renderAppCards() {
    let wrap = document.getElementById('rdCardsWrap');
    if (!wrap) return;
    let data = rdAppData.filter(rdAppMatch);
    let html = '';
    if (!data.length) {
        html = '<div class="rd-cards-empty"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/><line x1="8" y1="11" x2="14" y2="11"/></svg>' +
            '<div>اپی' + (rdSearchTerm ? ' با «' + rdEsc(rdSearchTerm) + '»' : '') + ' پیدا نشد</div></div>';
    } else {
        html = '<div class="rd-card-grid">';
        for (let i = 0; i < data.length; i++) html += rdAppCard(data[i]);
        html += '</div>';
    }
    wrap.innerHTML = html;
}

function applyAppView() {
    let isCards = rdViewMode === 'cards';
    document.body.classList.toggle('rd-view-cards', isCards);
    let chip = document.getElementById('rdAppsCount');
    if (chip) chip.textContent = rdFaNum(rdAppData.filter(rdAppMatch).length);
    let btns = document.querySelectorAll('.rd-view-btn');
    for (let i = 0; i < btns.length; i++) btns[i].classList.toggle('active', btns[i].getAttribute('data-rd-view') === rdViewMode);
    if (isCards) {
        renderAppCards();
    } else {
        let filtered = rdAppData.filter(rdAppMatch);
        appsTable.clear().draw();
        renderapps(filtered);
    }
}

$(document).on('click', '.rd-view-btn', function () {
    rdViewMode = $(this).attr('data-rd-view');
    try { localStorage.setItem('rdAppsView', rdViewMode); } catch (e) { }
    applyAppView();
});

$(document).on('input', '#rdSearchInput', function () {
    let v = this.value;
    clearTimeout(rdSearchTimer);
    rdSearchTimer = setTimeout(function () {
        rdSearchTerm = v.trim();
        applyAppView();
    }, 250);
});

/* — فرم / حذف — */
function newapp() {
    $('#parentCaegoryArea').css('display', 'block');
    formUrl = appsBaseUrl + '/Create';
    $('#appsModal').modal();
}

function submitForm() {
    loading();
    let form = document.getElementById('appForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getapps();
            document.getElementById('appForm').reset();
            $('#appsModal').modal('toggle');
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

function deleteapps(id) {
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
                url: appsBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    getapps();
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

$('#editForm').submit(function (e) {
    e.preventDefault();
    loading();
    let form = $(this);

    $.ajax({
        url: appsBaseUrl + "/edit",
        method: "POST",
        data: form.serialize(),
        success: function (data) {
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
});

/* — گروه‌های اپ (از لیست و کارت) — */
$(document).on('click', '.rd-apps-groups', function () {
    loading();
    appGroups = [];
    var btn = $(this);
    $('#itemId').val(btn.attr('data-item-id'));
    $.ajax({
        url: appsBaseUrl + "/Groups?id=" + btn.attr('data-item-id'),
        method: 'GET',
        success: function (data) {
            $('#appgroupsArea').empty();
            $('#appAdgroupsArea').empty();

            for (var i = 0; i < data.length; i++) {
                let item = data[i];
                let area = item.isAd ? 'appAdgroupsArea' : 'appgroupsArea';
                if (item.doTheyHaveRelation) {
                    $('#' + area).append('<div onclick="checkGroup(' + item.id + ')" class="col-3 mt-3"><input checked value=' + item.id + ' class="groupCheckbox" type ="checkbox" /><span>' + item.title + '</span></div>');
                    appGroups.push(item.id);
                } else {
                    $('#' + area).append('<div onclick="checkGroup(' + item.id + ')" class="col-3 mt-3"><input value=' + item.id + ' type ="checkbox" class="groupCheckbox" /><span>' + item.title + '</span></div>');
                }
            }

            $("#groupsbyappModal").modal();
            swal.close();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            let errors = xhr.responseJSON.errors;
            for (var i = 0; i < errors.length; i++) {
                toastr.error(errors[i]);
            }
            swal.close();
        }
    })
});

function checkGroup(id) {
    loading();
    if (appGroups.includes(id)) {
        let index = appGroups.indexOf(id);
        if (index > -1) {
            appGroups.splice(index, 1);
        }

    } else {
        appGroups.push(id);
    }
    swal.close();
}

function submitAppGroups() {
    loading();
    let model = {
        id: $('#itemId').val(),
        groupIds: appGroups.length == 0 ? "_" : appGroups.join("_")
    }
    $.ajax({
        url: appsBaseUrl + '/updateAppGroups',
        method: 'Post',
        data: model,
        success: function (data) {
            appGroups = [];
            $('#groupsbyappModal').modal('toggle');
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
}
