toastr.options = {
    "closeButton": false,
    "newestOnTop": true,
    "progressBar": true,
    "positionClass": "toast-top-left",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "3000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};


const SwalConfirm = Swal.mixin({
    text: "آیا مطمئن هستید؟",
    icon: "warning",
    showCancelButton: true,
    buttonsStyling: false,
    confirmButtonText: "بلی",
    cancelButtonText: "خیر",
    focusConfirm: false,
    customClass: {
        confirmButton: "btn fw-bold btn-primary",
        cancelButton: "btn fw-bold btn-active-light-primary"
    }
});

KTUtil.onDOMContentLoaded(function () {
    if (typeof $.fn.dataTable != "undefined") {
        $.extend(true, $.fn.dataTable.defaults, {
            autoWidth: false,
            processing: true,
            serverSide: true,
            ordering: false,
            language: {
                url: '/js/datatables.fa.json'
            },
            stateSave: false,
            responsive: true,
        });
    }

    $('input.alphanum').on('keyup keypress keydown change', function () {
        if (this.value.match(/[^a-zA-Z0-9 ]/g)) {
            this.value = this.value.replace(/[^a-zA-Z0-9 ]/g, '');
        }
    });
    $('input.numeric').on('keyup keypress keydown change', function () {
        if (this.value.match(/[^0-9]/g)) {
            this.value = this.value.replace(/[^0-9]/g, '');
        }
    });
    $('input.alpha').on('keyup keypress keydown change', function () {
        if (this.value.match(/[^a-zA-Zضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئوآءأإؤژيةۀپ ]/g)) {
            this.value = this.value.replace(/[^a-zA-Zضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئوآءأإؤژيةۀپ ]/g, '');
        }
    });
    $('input.alphacode').on('keyup keypress keydown change', function () {
        if (this.value.match(/[^a-zA-Z0-9ضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئوآءأإؤژيةۀپ]/g)) {
            this.value = this.value.replace(/[^a-zA-Z0-9ضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئوآءأإؤژيةۀپ]/g, '_');
        }
    });
    $('input.english').on('keyup keypress keydown change', function () {
        if (this.value.match(/[^a-zA-Z ]/g)) {
            this.value = this.value.replace(/[^a-zA-Z ]/g, '');
        }
    });
    $('input.persian').on('keyup keypress keydown change', function () {
        if (this.value.match(/[^ضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئوآءأإؤژيةۀپ ]/g)) {
            this.value = this.value.replace(/[^ضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئوآءأإؤژيةۀپ ]/g, '');
        }
    });
    $('input.english-nospace').on('keyup keypress keydown change', function () {
        if (this.value.match(/[^a-zA-Z]/g)) {
            this.value = this.value.replace(/[^a-zA-Z]/g, '');
        }
    });
    $('input.mask-custom').on('keyup keypress keydown change', function (e) {
        var regex = new RegExp(e.target.dataset.regex, 'g');
        if (this.value.match(regex)) {
            this.value = this.value.replace(regex, '');
        }
    });

    let url = window.location.href;
    url = url.split('#')[0];
    url = url.split('?')[0];
    if (url.endsWith("/")) {
        url = url.substr(0, url.length - 1);
    }
    $('#kt_aside_menu div.menu-item a.menu-link').filter(function () {
        if (this.href.endsWith("/")) {
            return url == this.href.substr(0, this.href.length - 1);
        } else {
            return url == this.href;
        }
    }).addClass('active').parent().parent().addClass('show').parent().addClass('show here');
});

Object.defineProperty(String.prototype, "ToPersianDateString", {
    value: function ToPersianDateString() {
        return moment(this).locale('fa').format('LLL');
    },
    writable: true,
    configurable: true
});

Object.defineProperty(String.prototype, "ToPersianCurrency", {
    value: function ToPersianCurrency() {
        return `${KTUtil.numberString(this)} <small>تومان</small>`
    },
    writable: true,
    configurable: true
});
Object.defineProperty(Number.prototype, "ToPersianCurrency", {
    value: function ToPersianCurrency() {
        return `${KTUtil.numberString(this)} <small>تومان</small>`
    },
    writable: true,
    configurable: true
});

const Select2Cities = function (selector) {
    function formatRepo(repo) {
        if (repo.loading) return repo.text;
        return `<span class="text-dark-75 text-hover-primary font-weight-bolder font-size-lg">` + repo.cityName + `</span> <small>-</small> <span class="text-muted font-weight-bold">` + repo.provinceName + `</span>`;
    }

    function formatRepoSelection(repo) {
        if (repo.cityName === undefined)
            return repo.text;

        document.querySelector('input[name="City"]').value = repo.cityName;
        document.querySelector('input[name="Province"]').value = repo.provinceName;
        return `${repo.cityName} - ${repo.provinceName}`
    }

    $(selector).select2({
        allowClear: false,
        language: "fa",
        dir: "rtl",
        placeholder: "لطفا شهر را انتخاب کنید",
        ajax: {
            url: "/cities",
            delay: 250,
            data: function (params) {
                return {
                    name: params.term,
                    page: params.page || 1
                }
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                var result = $.map(data, function (obj) {
                    obj.id = obj.id || obj.cityDivisionCode;
                    return obj;
                });

                return {
                    results: result,
                    pagination: {
                        more: data.length > 0
                    }
                };
            },
            cache: true
        },
        escapeMarkup: function (markup) {
            return markup;
        },
        templateResult: formatRepo,
        templateSelection: formatRepoSelection,
    });
}

function TagifyIt(selector) {
    let input = document.querySelector(selector);
    let tagify = new Tagify(input);
    return tagify;
}

function GetTagifyValues(input) {
    return input.value.flatMap(f => {
        return f.value
    }).join(',')
}

function GetBack() {
    history.back()
}