"use strict";
var imageBolb;
var descriptionEditor;
var KTDocsAdd = function () {
    const baseUrl = '/customers';
    function UploadAdapterPlugin(editor) {
        editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
            return new UploadAdapter(loader);
        };
    }

    return {
        init: function () {
            ClassicEditor
                .create(document.querySelector('#Description'), {
                    extraPlugins: [UploadAdapterPlugin],
                    language: 'fa',
                })
                .then(editor => {
                    descriptionEditor = editor;
                    descriptionEditor.model.document.on('change:data', (e) => {
                        form_validation.revalidateField('Description')
                    });
                })
                .catch(error => { console.error(error) });

            const form = document.getElementById("form");
            var form_validation = FormValidation.formValidation(form, {
                fields: {
                    Name: {
                        validators: {
                            notEmpty: {
                                message: "نام مشتری را وارد نکرده اید"
                            }
                        }
                    },
                    Description: {
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
                            fd.set("Description", descriptionEditor.getData()),
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