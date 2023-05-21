"use strict";
var imageBolb;
var descriptionEditor;
var KTDocsAdd = function () {
    const baseUrl = '/pages';
    var tags;
    var keywords;
    function UploadAdapterPlugin(editor) {
        editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
            return new UploadAdapter(loader);
        };
    }

    return {
        init: function () {
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
                                message: "تیتر صفحه را وارد نکرده اید"
                            }
                        }
                    },
                    Uri: {
                        validators: {
                            notEmpty: {
                                message: "آدرس صفحه را وارد نکرده اید"
                            }
                        }
                    },
                    Content: {
                        validators: {
                            callback: {
                                message: 'شرح صفحه را وارد نکرده اید',
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
}));