"use strict";
var KTDocsAdd = function () {
    const baseUrl = '/faqs';
    const t = document.getElementById("info_modal"),
        e = t.querySelector("#modal_form"),
        boomodal = new bootstrap.Modal(t, {
            backdrop: 'static'
        });
    return {
        init: function () {
            (() => {
                var o = FormValidation.formValidation(e, {
                    fields: {
                        name: {
                            validators: {
                                notEmpty: {
                                    message: "نام نقش را وارد نکرده اید"
                                }
                            }
                        }
                    },
                    plugins: {
                        trigger: new FormValidation.plugins.Trigger,
                        bootstrap: new FormValidation.plugins.Bootstrap5({
                            rowSelector: ".fv-row",
                            eleInvalidClass: "",
                            eleValidClass: ""
                        })
                    }
                });
                const i = t.querySelector('[data-kt-docs-modal-action="submit"]');
                i.addEventListener("click", (t => {
                    t.preventDefault(), o && o.validate().then((function (t) {
                        console.log("validated!"), "Valid" == t ?
                            (i.setAttribute("data-kt-indicator", "on"),
                                i.disabled = !0,
                                fetch(`${baseUrl}/upsert`, {
                                    method: 'POST',
                                    body: new FormData(e),
                                })
                                    .then(response => response.json())
                                    .then(result => {
                                        i.removeAttribute("data-kt-indicator");
                                        i.disabled = !1;
                                        console.log(result.status == 1);
                                        if (result.status == 1) {
                                            toastr.success(result.message);
                                            t.isConfirmed;
                                            boomodal.hide();
                                        } else {
                                            toastr.error(result.message);
                                            result.errors.forEach((item) => {
                                                toastr.warning(item);
                                            });
                                        }
                                    })
                                    .catch(error => toastr.error(result.message))
                            )
                            : toastr.warning("لطفا اطلاعات فرم را تکمیل کنید")
                    }))
                }));
                t.querySelector('[data-kt-docs-modal-action="cancel"]').addEventListener("click", (t => {
                    t.preventDefault();
                    //e.reset();
                    boomodal.hide();
                }));
            })()
        }
    }
}();
KTUtil.onDOMContentLoaded((function () {
    KTDocsAdd.init()
}));