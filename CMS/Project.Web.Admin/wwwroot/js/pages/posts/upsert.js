"use strict";
var imageBolb;
var descriptionEditor;
var KTDocsAdd = function () {
    const baseUrl = '/posts';
    var tags;
    var keywords;
    function UploadAdapterPlugin(editor) {
        editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
            return new UploadAdapter(loader);
        };
    }

    const categoriesSelect2 = function (selector) {
        function formatRepo(repo) {
            if (repo.loading) return repo.text;
            return `<span class="text-dark-75 text-hover-primary font-weight-bolder font-size-lg">` + repo.title + `</span>`;
        }

        function formatRepoSelection(repo) {
            if (repo.title === undefined)
                return repo.text;

            return `${repo.title}`
        }

        $(selector).select2({
            allowClear: false,
            language: "fa",
            dir: "rtl",
            placeholder: "لطفا دسته بندی را انتخاب کنید",
            ajax: {
                url: `/postcategories/getallpaginate`,
                delay: 250,
                data: function (params) {
                    return {
                        takeMain: false,
                        title: params.term,
                        page: params.page || 1
                    }
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data,
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


    return {
        init: function () {
            categoriesSelect2("#PostCategoryId");
            tags = TagifyIt("#Tags");
            keywords = TagifyIt("#Keywords");
            ClassicEditor
                .create(document.querySelector('#Content'), {
                    extraPlugins: [UploadAdapterPlugin],
                    language: 'fa',
                })
                .then(editor => {
                    descriptionEditor = editor;
                    descriptionEditor.model.document.on('change:data', (e) => {
                        form_validation.revalidateField('Content')
                    });
                })
                .catch(error => { console.error(error) });

            const form = document.getElementById("form");
            var form_validation = FormValidation.formValidation(form, {
                fields: {
                    Title: {
                        validators: {
                            notEmpty: {
                                message: "تیتر خبر را وارد نکرده اید"
                            }
                        }
                    },
                    PostCategoryId: {
                        validators: {
                            notEmpty: {
                                message: "دسته بندی را انتخاب نکرده اید"
                            }
                        }
                    },
                    Content: {
                        validators: {
                            callback: {
                                message: 'شرح خبر را وارد نکرده اید',
                                callback: function (input) {
                                    return descriptionEditor.getData() != ''
                                }
                            },
                        }
                    },
                },
                plugins: {
                    trigger: new FormValidation.plugins.Trigger,
                    bootstrap: new FormValidation.plugins.Bootstrap5({
                        rowSelector: ".form-group",
                        eleInvalidClass: "",
                        eleValidClass: ""
                    })
                }
            });
            const submitButton = document.querySelector('[data-kt-docs-action="submit"]');
            submitButton.addEventListener("click", e => {
                e.preventDefault();
                var fd;
                form_validation.validate().then((function (t) {
                    ("Valid" == t) ?
                        (
                            fd = new FormData(form),
                            fd.set("Content", descriptionEditor.getData()),
                            fd.set("Tags", GetTagifyValues(tags)),
                            fd.set("Keywords", GetTagifyValues(keywords)),
                            (imageBolb !== undefined) ? fd.append('Image', imageBolb, imageBolb.type.replace('/', '.')) : '',
                            submitButton.setAttribute("data-kt-indicator", "on"),
                            submitButton.disabled = !0,
                            fetch(`${baseUrl}/upsert`, {
                                method: 'POST',
                                body: fd,
                            })
                                .then(response => {
                                    submitButton.removeAttribute("data-kt-indicator");
                                    submitButton.disabled = !1;
                                    return response.json()
                                })
                                .then(result => {
                                    if (result.status == 1) {
                                        toastr.success(result.message);

                                    } else {
                                        toastr.error(result.message);
                                        result.errors.forEach((item) => {
                                            toastr.warning(item);
                                        });
                                    }
                                })
                                .catch(error => { console.log(error) })
                        )
                        : (toastr.warning("لطفا اطلاعات فرم را تکمیل کنید"), console.log(t))
                }))

            });
        }
    }
}();
KTUtil.onDOMContentLoaded((function () {
    KTDocsAdd.init();

    CropperInstance.init('cropper-image', 600, 400, (blob) => {
        imageBolb = blob;
    });
}));