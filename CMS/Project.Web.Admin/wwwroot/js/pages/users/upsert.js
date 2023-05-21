"use strict";
var KTDocsAdd = function () {

    return {
        init: function () {
            const baseUrl = '/users';
            const t = document.getElementById("info_modal"),
                e = t.querySelector("#modal_form"),
                boomodal = new bootstrap.Modal(t),
                pmetter = KTPasswordMeter.getInstance(e.querySelector('[data-kt-password-meter="true"]')),
                validatePassword = function () {
                    return pmetter.getScore() >= 80;
                };
            var o = FormValidation.formValidation(e, {
                fields: {
                    userName: {
                        validators: {
                            notEmpty: {
                                message: "نام کاربری را وارد نکرده اید"
                            }
                        }
                    },
                    email: {
                        validators: {
                            notEmpty: {
                                message: "ایمیل را وارد نکرده اید"
                            }
                        }
                    },
                    phoneNumber: {
                        validators: {
                            notEmpty: {
                                message: "شماره همراه را وارد نکرده اید"
                            }
                        }
                    },
                    password: {
                        validators: {
                            callback: {
                                message: 'کلمه عبور را وارد نکرده اید',
                                callback: function (input) {
                                    let inputId = e.querySelector('[name="id"]');

                                    return inputId.value === ''
                                }
                            },
                            callback2: {
                                message: "کلمه عبور ایمن نیست",
                                callback2: function (input) {
                                    if (input.value.length > 0) {
                                        return validatePassword();
                                    }
                                }
                            }
                        }
                    },
                    confirmPassword: {
                        validators: {
                            callback: {
                                message: 'تایید کلمه عبور را وارد نکرده اید',
                                callback: function (input) {
                                    let inputId = e.querySelector('[name="password"]');

                                    if (inputId.value === '') {
                                        return true;
                                    } else {
                                        return input.value !== '';
                                    }
                                }
                            },
                            identical: {
                                compare: function () {
                                    return e.querySelector('[name="password"]').value;
                                },
                                message: "تایید کلمه عبور مطابقت ندارد"
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
                    ("Valid" == t) ?
                        (
                            i.setAttribute("data-kt-indicator", "on"),
                            i.disabled = !0,
                            fetch(`${baseUrl}/upsert`, {
                                method: 'POST',
                                body: new FormData(e),
                            })
                                .then(response => response.json())
                                .then(result => {
                                    console.log(result);
                                    i.removeAttribute("data-kt-indicator");
                                    i.disabled = !1;
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
                                .catch(error => { console.log(error) })
                        )
                        : (toastr.warning("لطفا اطلاعات فرم را تکمیل کنید"), console.log(t))
                }))
            }));
            t.querySelector('[data-kt-docs-modal-action="cancel"]').addEventListener("click", (t => {
                t.preventDefault();
                e.reset();
                boomodal.hide();
            }));
        }
    }
}();
KTUtil.onDOMContentLoaded((function () {
    KTDocsAdd.init()
}));