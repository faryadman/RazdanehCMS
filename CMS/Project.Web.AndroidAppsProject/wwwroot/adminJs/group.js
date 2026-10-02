let groupsTable = $('#groupsTable').DataTable();

let groupsBaseUrl = "/admin/groups";
let groupsIsAd = false;
let formUrl;

/* ===== v17: نما (لیست/کارت) + جستجو + عملیات مدرن ===== */
let rdViewMode = (function () { try { return localStorage.getItem(rdGroupsViewKey()) || 'list'; } catch (e) { return 'list'; } })();
let rdGroupsData = [];
let rdSearchTerm = '';
let rdSearchTimer;

function rdGroupsViewKey() { return groupsIsAd ? 'rdAdGroupsView' : 'rdGroupsView'; }
function rdEsc(s) { return String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;'); }
function rdFaNum(n) { return String(n).replace(/\d/g, function (d) { return '۰۱۲۳۴۵۶۷۸۹'[d]; }); }
function rdGroupsMatch(item) {
    if (!rdSearchTerm) return true;
    let t = rdSearchTerm.toLowerCase();
    return ((item.title || '').toLowerCase().indexOf(t) !== -1) || (String(item.id).indexOf(t) !== -1);
}

function getgroups(isAd) {
    groupsIsAd = !!isAd;
    $.ajax({
        type: "GET",
        url: groupsBaseUrl + '/List?isAd=' + isAd,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            rdGroupsData = result || [];
            $('#rdGroupsCount').text(rdFaNum(rdGroupsData.length));
            applyGroupView();
        },
        error: function (xmlhttprequest, textstatus, errorthrown) {
            alert(" بروز اشکال در اتصال به اینترنت ");
        }
    });
}

/* — آیکون‌ها و ساختار عملیات — */
var RD_IC = {
    server: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="2" width="20" height="8" rx="2" ry="2"/><rect x="2" y="14" width="20" height="8" rx="2" ry="2"/><line x1="6" y1="6" x2="6.01" y2="6"/><line x1="6" y1="18" x2="6.01" y2="18"/></svg>',
    dots: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="1.9"/><circle cx="12" cy="12" r="1.9"/><circle cx="12" cy="19" r="1.9"/></svg>',
    edit: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>',
    trash: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>',
    users: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>'
};

function rdGroupServersLink(item) {
    let serverUrl = item.isAd ? '/servers/adIndex' : '/servers';
    return '<a class="btn btn-sm btn-outline-primary rd-card-servers" href="/admin' + serverUrl + '?groupId=' + item.id + '" title="سرورهای این گروه">' + RD_IC.server + 'سرورها</a>';
}

function rdGroupActionsMenu(item) {
    return '<div class="rd-row-actions"><div class="dropdown rd-item-dropdown">' +
        '<button class="btn btn-sm btn-light rd-more-btn" type="button" data-rd-toggle="dropdown" aria-haspopup="true" aria-expanded="false" title="عملیات">' + RD_IC.dots + '</button>' +
        '<div class="dropdown-menu dropdown-menu-left rd-item-menu">' +
        '<button class="dropdown-item rd-groups-edit" data-item-id="' + item.id + '" data-item-title="' + rdEsc(item.title) + '" title="ویرایش">' + '<i class="rd-di rd-di-warn">' + RD_IC.edit + '</i>ویرایش</button>' +
        '<div class="dropdown-divider"></div>' +
        '<button class="dropdown-item rd-danger-item" onclick="deletegroups(' + item.id + ')" title="حذف">' + '<i class="rd-di rd-di-danger">' + RD_IC.trash + '</i>حذف گروه</button>' +
        '</div></div></div>';
}

/* — نمای لیست — */
function rendergroups(data) {
    for (var i = 0; i < data.length; i++) {
        let item = data[i];
        let deleteChekbox = '<input class="deleteCheckbox" type="checkbox" data-item-id="' + item.id + '"/>';
        let titleCell = '<div class="rd-row-main"><span class="rd-row-title">' + rdEsc(item.title) + '</span></div>';
        let actionsCell = rdGroupServersLink(item) + rdGroupActionsMenu(item);
        groupsTable.row.add([
            deleteChekbox,
            '<span class="mono">' + item.id + '</span>',
            titleCell,
            rdEsc(item.updatedAtFormatted || '—'),
            actionsCell
        ]).node();
    }
    groupsTable.draw();
}

/* — نمای کارت — */
function rdGroupCard(item) {
    let row = function (k, v) { return '<div class="rd-card-row"><span class="rd-card-k">' + k + '</span><span class="rd-card-v">' + v + '</span></div>'; };
    return '<article class="rd-card" data-id="' + item.id + '">' +
        '<div class="rd-card-head">' +
        '<span class="rd-group-ic" aria-hidden="true">' + RD_IC.users + '</span>' +
        '<span class="rd-card-title" title="' + rdEsc(item.title) + '">' + rdEsc(item.title || '—') + '</span>' +
        '<div class="rd-card-actions">' + rdGroupServersLink(item) + rdGroupActionsMenu(item) + '</div>' +
        '</div>' +
        '<div class="rd-card-body">' +
        row('شناسه', '<span class="mono">' + item.id + '</span>') +
        row('به‌روزرسانی', rdEsc(item.updatedAtFormatted || '—')) +
        '</div></article>';
}

function renderGroupCards() {
    let wrap = document.getElementById('rdCardsWrap');
    if (!wrap) return;
    let data = rdGroupsData.filter(rdGroupsMatch);
    let html = '';
    if (!data.length) {
        html = '<div class="rd-cards-empty"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/><line x1="8" y1="11" x2="14" y2="11"/></svg>' +
            '<div>گروهی' + (rdSearchTerm ? ' با «' + rdEsc(rdSearchTerm) + '»' : '') + ' پیدا نشد</div></div>';
    } else {
        html = '<div class="rd-card-grid">';
        for (let i = 0; i < data.length; i++) html += rdGroupCard(data[i]);
        html += '</div>';
    }
    wrap.innerHTML = html;
}

function applyGroupView() {
    let isCards = rdViewMode === 'cards';
    document.body.classList.toggle('rd-view-cards', isCards);
    let btns = document.querySelectorAll('.rd-view-btn');
    for (let i = 0; i < btns.length; i++) btns[i].classList.toggle('active', btns[i].getAttribute('data-rd-view') === rdViewMode);
    if (isCards) {
        renderGroupCards();
    } else {
        let filtered = rdGroupsData.filter(rdGroupsMatch);
        groupsTable.clear().draw();
        rendergroups(filtered);
    }
}

$(document).on('click', '.rd-view-btn', function () {
    rdViewMode = $(this).attr('data-rd-view');
    try { localStorage.setItem(rdGroupsViewKey(), rdViewMode); } catch (e) { }
    applyGroupView();
});

$(document).on('input', '#rdSearchInput', function () {
    let v = this.value;
    clearTimeout(rdSearchTimer);
    rdSearchTimer = setTimeout(function () {
        rdSearchTerm = v.trim();
        applyGroupView();
    }, 250);
});

/* — فرم / حذف — */
function newgroup() {
    formUrl = groupsBaseUrl + '/Create';
    $('#groupsModal').modal();
}

function submitForm() {
    loading();
    let form = document.getElementById('groupForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl,
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getgroups(groupsIsAd);
            document.getElementById('groupForm').reset();
            $('#groupsModal').modal('toggle');
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            let errors = xhr.responseJSON.errors;
            for (var i = 0; i < errors.length; i++) { toastr.error(errors[i]); }
            swal.close();
        }
    })
}

function submitAdForm() {
    loading();
    let form = document.getElementById('groupForm');
    let formData = new FormData(form);
    $.ajax({
        url: formUrl + "Ad",
        data: formData,
        method: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            getgroups(true);
            document.getElementById('groupForm').reset();
            $('#groupsModal').modal('toggle');
            data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
        },
        error: function (xhr, ajaxOptions, thrownError) {
            let errors = xhr.responseJSON.errors;
            for (var i = 0; i < errors.length; i++) { toastr.error(errors[i]); }
            swal.close();
        }
    })
}

function deletegroups(id) {
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
            let vm = { id: id };
            $.ajax({
                type: "POST",
                url: groupsBaseUrl + '/Delete',
                data: vm,
                success: function (data) {
                    getgroups(groupsIsAd);
                    data.status == "0" ? Swal.fire('', data.message, 'error') : Swal.fire('', data.message, 'success');
                }
            })
        }
    });
}

/* ویرایش — هم در ردیف لیست، هم در کارت */
$(document).on('click', '.rd-groups-edit', function () {
    loading();
    let item = $(this);
    $('#itemId').val(item.attr('data-item-id'));
    $('#Title').val(item.attr('data-item-title'));
    formUrl = groupsBaseUrl + '/Edit';
    $('#groupsModal').modal();
    Swal.close();
});
