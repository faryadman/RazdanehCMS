"use strict";
var KTSigninGeneral = function () {
    var e, t, i;
    return {
        init: function () {
            e = document.querySelector("#kt_sign_in_form"),
                t = document.querySelector("#kt_sign_in_submit"),
                i = FormValidation.formValidation(e, {
                    fields: {
                        Username: {
                            validators: {
                                notEmpty: {
                                    message: "نام کاربری را وارد کنید"
                                },
                            }
                        },
                        Password: {
                            validators: {
                                notEmpty: {
                                    message: "کلمه عبور را وارد کنید"
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
                }),
                t.addEventListener("click", (function (n) {
                    n.preventDefault(), i.validate().then((function (i) {
                        "Valid" == i ?
                            (t.setAttribute("data-kt-indicator", "on"), t.disabled = !0,
                                fetch("/account/signin", {
                                    method: 'POST',
                                    body: new FormData(e),
                                })
                                    .then(response => response.json())
                                    .then(result => {
                                        e.reset();
                                        console.log(result.data);
                                        if (result.status == 1) {
                                            toastr.success(result.message);
                                            window.location.href = result.data;
                                        } else {
                                            t.removeAttribute("data-kt-indicator");
                                            t.disabled = !1;
                                            toastr.error(result.message);
                                        }
                                    })
                                    .catch(error => toastr.error(result.message))
                            )
                            : toastr.warning("لطفا اطلاعات فرم را تکمیل کنید")
                    }))
                }))
        }
    }
}();
KTUtil.onDOMContentLoaded((function () {
    KTSigninGeneral.init()
}));